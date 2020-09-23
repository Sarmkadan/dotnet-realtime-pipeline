#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Workers;

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Background worker that periodically invokes
/// <see cref="DynamicScalingService.EvaluateScalingAsync"/> to adapt per-stage consumer
/// concurrency in response to observed backpressure.
/// </summary>
public sealed class DynamicScalingWorker : IDisposable
{
    private readonly DynamicScalingService _scalingService;
    private readonly ILogger<DynamicScalingWorker> _logger;
    private readonly int _intervalMs;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task _workerTask;
    private volatile bool _isRunning;

    /// <summary>
    /// Creates the worker with a scaling service, logger, and polling interval.
    /// </summary>
    /// <param name="scalingService">The scaling service to drive on each evaluation tick.</param>
    /// <param name="logger">Logger for lifecycle and error events.</param>
    /// <param name="intervalMs">Milliseconds between evaluation passes (default 5000).</param>
    public DynamicScalingWorker(
        DynamicScalingService scalingService,
        ILogger<DynamicScalingWorker> logger,
        int intervalMs = 5000)
    {
        _scalingService = scalingService ?? throw new ArgumentNullException(nameof(scalingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _intervalMs = Math.Max(500, intervalMs);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>Gets a value indicating whether the evaluation loop is active.</summary>
    public bool IsRunning => _isRunning;

    /// <summary>Starts the background scaling evaluation loop.</summary>
    public void Start()
    {
        if (_isRunning)
        {
            _logger.LogWarning("Dynamic scaling worker is already running");
            return;
        }

        _isRunning = true;
        _logger.LogInformation("Starting dynamic scaling worker (interval: {Interval}ms)", _intervalMs);
        _workerTask = EvaluateAsync(_cancellationTokenSource.Token);
    }

    /// <summary>Signals the evaluation loop to stop and waits for it to drain.</summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
        {
            _logger.LogWarning("Dynamic scaling worker is not running");
            return;
        }

        _logger.LogInformation("Stopping dynamic scaling worker");
        _cancellationTokenSource.Cancel();

        if (_workerTask is not null)
        {
            try
            {
                await _workerTask;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Dynamic scaling worker stopped");
            }
        }

        _isRunning = false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isRunning)
        {
            _cancellationTokenSource.Cancel();

            try
            {
                _workerTask?.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown.
            }

            _isRunning = false;
        }

        _cancellationTokenSource.Dispose();
    }

    private async Task EvaluateAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _scalingService.EvaluateScalingAsync(cancellationToken);
                    await Task.Delay(_intervalMs, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in dynamic scaling worker evaluation loop");
                    await Task.Delay(_intervalMs, cancellationToken);
                }
            }
        }
        finally
        {
            _isRunning = false;
        }
    }
}

/// <summary>
/// Dependency-injection extension methods for registering dynamic pipeline scaling.
/// </summary>
public static class DynamicScalingServiceExtensions
{
    /// <summary>
    /// Adds <see cref="DynamicScalingService"/> and <see cref="DynamicScalingWorker"/> to
    /// the service container.  Must be called after <c>AddPipelineServices</c> so that
    /// <see cref="BackpressureService"/> and <see cref="PipelineConfig"/> are already registered.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="minConsumers">Lower bound on consumers per stage (default 1).</param>
    /// <param name="maxConsumers">Upper bound on consumers per stage (default 16).</param>
    /// <param name="scaleUpThresholdPercent">Buffer fill % that triggers scale-up (default 75).</param>
    /// <param name="scaleDownThresholdPercent">Buffer fill % that triggers scale-down (default 30).</param>
    /// <param name="cooldownSeconds">Minimum seconds between scaling actions per stage (default 15).</param>
    /// <param name="evaluationIntervalMs">Worker polling interval in milliseconds (default 5000).</param>
    public static IServiceCollection AddDynamicScaling(
        this IServiceCollection services,
        int minConsumers = 1,
        int maxConsumers = 16,
        double scaleUpThresholdPercent = 75.0,
        double scaleDownThresholdPercent = 30.0,
        int cooldownSeconds = 15,
        int evaluationIntervalMs = 5000)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        services.AddSingleton(sp => new DynamicScalingService(
            sp.GetRequiredService<BackpressureService>(),
            sp.GetRequiredService<PipelineConfig>(),
            sp.GetRequiredService<ILogger<DynamicScalingService>>(),
            minConsumers, maxConsumers,
            scaleUpThresholdPercent, scaleDownThresholdPercent,
            cooldownSeconds));

        services.AddSingleton(sp => new DynamicScalingWorker(
            sp.GetRequiredService<DynamicScalingService>(),
            sp.GetRequiredService<ILogger<DynamicScalingWorker>>(),
            evaluationIntervalMs));

        return services;
    }
}
