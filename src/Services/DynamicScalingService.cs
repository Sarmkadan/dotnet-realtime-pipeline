// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Dynamically adjusts the maximum concurrent consumers of each active pipeline stage
/// by observing per-stage backpressure metrics.  Scale-up is triggered when buffer fill
/// exceeds a configurable high-water mark; scale-down when fill stays below a low-water
/// mark with negligible backpressure frequency.  A per-stage cooldown prevents oscillation.
/// </summary>
public sealed class DynamicScalingService
{
    private readonly BackpressureService _backpressureService;
    private readonly PipelineConfig _config;
    private readonly ILogger<DynamicScalingService> _logger;
    private readonly int _minConsumers;
    private readonly int _maxConsumers;
    private readonly double _scaleUpThresholdPercent;
    private readonly double _scaleDownThresholdPercent;
    private readonly TimeSpan _cooldown;

    private readonly Dictionary<string, StageScalingState> _states = new();
    private readonly object _lock = new();

    /// <summary>
    /// Initialises the service with backpressure source, pipeline configuration, and tuning parameters.
    /// </summary>
    /// <param name="backpressureService">Shared service for reading and adjusting stage contexts.</param>
    /// <param name="config">Pipeline configuration providing the list of stages to monitor.</param>
    /// <param name="logger">Logger for scaling decisions and diagnostic output.</param>
    /// <param name="minConsumers">Lower bound on concurrent consumers; never scales below this value.</param>
    /// <param name="maxConsumers">Upper bound on concurrent consumers; never scales above this value.</param>
    /// <param name="scaleUpThresholdPercent">Buffer fill % that triggers a scale-up (default 75).</param>
    /// <param name="scaleDownThresholdPercent">Buffer fill % below which a scale-down is considered (default 30).</param>
    /// <param name="cooldownSeconds">Minimum seconds between consecutive scaling actions per stage (default 15).</param>
    public DynamicScalingService(
        BackpressureService backpressureService,
        PipelineConfig config,
        ILogger<DynamicScalingService> logger,
        int minConsumers = 1,
        int maxConsumers = 16,
        double scaleUpThresholdPercent = 75.0,
        double scaleDownThresholdPercent = 30.0,
        int cooldownSeconds = 15)
    {
        _backpressureService = backpressureService ?? throw new ArgumentNullException(nameof(backpressureService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _minConsumers = Math.Max(1, minConsumers);
        _maxConsumers = Math.Max(_minConsumers + 1, maxConsumers);
        _scaleUpThresholdPercent = scaleUpThresholdPercent;
        _scaleDownThresholdPercent = scaleDownThresholdPercent;
        _cooldown = TimeSpan.FromSeconds(Math.Max(1, cooldownSeconds));
    }

    /// <summary>
    /// Evaluates all enabled pipeline stages and applies any warranted scaling decisions.
    /// Safe to call repeatedly from a polling loop; stages still in cooldown are skipped.
    /// </summary>
    /// <param name="cancellationToken">Token used to cooperatively abort the evaluation pass.</param>
    public Task EvaluateScalingAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            foreach (var stage in _config.Stages.Where(s => s.Enabled))
            {
                try
                {
                    EvaluateStage(stage.StageName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error evaluating scaling for stage '{StageName}'", stage.StageName);
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns a snapshot of the current scaling state for the specified stage,
    /// or <c>null</c> if the stage has not yet been evaluated.
    /// </summary>
    /// <param name="stageName">The pipeline stage to query.</param>
    public StageScalingState? GetScalingState(string stageName)
    {
        lock (_lock)
        {
            return _states.TryGetValue(stageName, out var state) ? state : null;
        }
    }

    /// <summary>Returns a read-only snapshot of the scaling state for all evaluated stages.</summary>
    public IReadOnlyDictionary<string, StageScalingState> GetAllScalingStates()
    {
        lock (_lock)
        {
            return _states.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }

    private void EvaluateStage(string stageName)
    {
        var context = _backpressureService.GetContext(stageName);
        if (context == null)
            return;

        if (!_states.TryGetValue(stageName, out var state))
        {
            state = new StageScalingState
            {
                StageName = stageName,
                CurrentConsumers = context.MaxConcurrentConsumers
            };
            _states[stageName] = state;
        }

        if (DateTime.UtcNow - state.LastScalingActionAt < _cooldown)
            return;

        double fill = context.GetBufferFillPercentage();
        double freq = context.GetBackpressureFrequency();
        var decision = ComputeDecision(stageName, fill, freq, context.MaxConcurrentConsumers);

        if (decision.Direction == ScalingDirection.None)
            return;

        // Propagate the new limit directly into the backpressure context so that
        // TryRegisterConsumer() immediately respects the updated concurrency cap.
        context.MaxConcurrentConsumers = decision.ToConsumers;
        state.CurrentConsumers = decision.ToConsumers;
        state.LastDecision = decision;
        state.LastScalingActionAt = DateTime.UtcNow;

        if (decision.Direction == ScalingDirection.Up)
            state.ScaleUpCount++;
        else
            state.ScaleDownCount++;

        _logger.LogInformation(
            "Scaled {Direction} stage '{Stage}': {From} → {To} consumers (buffer {Fill:F1}%, freq {Freq:F2}/min)",
            decision.Direction, stageName,
            decision.FromConsumers, decision.ToConsumers,
            fill, freq);
    }

    private ScalingDecision ComputeDecision(string stageName, double fill, double freq, int current)
    {
        if (fill >= _scaleUpThresholdPercent && current < _maxConsumers)
        {
            return new ScalingDecision
            {
                StageName = stageName,
                Direction = ScalingDirection.Up,
                Reason = $"Buffer at {fill:F1}% exceeds scale-up threshold ({_scaleUpThresholdPercent}%)",
                FromConsumers = current,
                ToConsumers = current + 1,
                BufferFillPercent = fill,
                BackpressureFrequency = freq
            };
        }

        if (fill <= _scaleDownThresholdPercent && freq < 0.5 && current > _minConsumers)
        {
            return new ScalingDecision
            {
                StageName = stageName,
                Direction = ScalingDirection.Down,
                Reason = $"Buffer at {fill:F1}% is below scale-down threshold ({_scaleDownThresholdPercent}%)",
                FromConsumers = current,
                ToConsumers = current - 1,
                BufferFillPercent = fill,
                BackpressureFrequency = freq
            };
        }

        return new ScalingDecision { StageName = stageName, Direction = ScalingDirection.None };
    }
}
