#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.State;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages pipeline state and lifecycle transitions.
/// Tracks state history and provides state queries.
/// </summary>
public class PipelineStateManager
{
    private volatile PipelineState _currentState = PipelineState.Stopped;
    private readonly List<StateTransition> _stateHistory = new();
    private readonly object _lockObject = new();
    private readonly ILogger<PipelineStateManager> _logger;
    private readonly List<StateChangeListener> _listeners = new();

    public PipelineStateManager(ILogger<PipelineStateManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current pipeline state.
    /// </summary>
    public PipelineState CurrentState => _currentState;

    /// <summary>
    /// Transitions to a new state.
    /// </summary>
    public bool TransitionTo(PipelineState newState, string reason = "")
    {
        lock (_lockObject)
        {
            if (!IsValidTransition(_currentState, newState))
            {
                _logger.LogWarning("Invalid state transition: {From} -> {To}", _currentState, newState);
                return false;
            }

            var oldState = _currentState;
            _currentState = newState;

            var transition = new StateTransition
            {
                FromState = oldState,
                ToState = newState,
                Timestamp = DateTime.UtcNow,
                Reason = reason
            };

            _stateHistory.Add(transition);
            _logger.LogInformation("Pipeline state transitioned: {From} -> {To}", oldState, newState);

            // Notify listeners
            NotifyStateChange(oldState, newState, reason);

            return true;
        }
    }

    /// <summary>
    /// Registers a state change listener.
    /// </summary>
    public void RegisterStateChangeListener(Action<PipelineState, PipelineState> listener)
    {
        _listeners.Add(new StateChangeListener { Callback = listener });
    }

    /// <summary>
    /// Gets the state history.
    /// </summary>
    public List<StateTransition> GetStateHistory()
    {
        lock (_lockObject)
        {
            return new List<StateTransition>(_stateHistory);
        }
    }

    /// <summary>
    /// Gets the time spent in the current state.
    /// </summary>
    public TimeSpan GetCurrentStateDuration()
    {
        lock (_lockObject)
        {
            var lastTransition = _stateHistory.LastOrDefault();
            if (lastTransition is null)
                return TimeSpan.Zero;

            return DateTime.UtcNow - lastTransition.Timestamp;
        }
    }

    /// <summary>
    /// Checks if the pipeline is in a valid state for operations.
    /// </summary>
    public bool IsOperational => _currentState == PipelineState.Running;

    /// <summary>
    /// Validates state transitions.
    /// </summary>
    private static bool IsValidTransition(PipelineState from, PipelineState to)
    {
        return (from, to) switch
        {
            (PipelineState.Stopped, PipelineState.Running) => true,
            (PipelineState.Running, PipelineState.Paused) => true,
            (PipelineState.Paused, PipelineState.Running) => true,
            (PipelineState.Running, PipelineState.Stopped) => true,
            (PipelineState.Paused, PipelineState.Stopped) => true,
            (_, PipelineState.Failed) => true,
            (PipelineState.Failed, PipelineState.Stopped) => true,
            _ => false
        };
    }

    /// <summary>
    /// Notifies listeners of state changes.
    /// </summary>
    private void NotifyStateChange(PipelineState oldState, PipelineState newState, string reason)
    {
        foreach (var listener in _listeners)
        {
            try
            {
                listener.Callback?.Invoke(oldState, newState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying state change listener");
            }
        }
    }
}

/// <summary>
/// Pipeline lifecycle states.
/// </summary>
public enum PipelineState
{
    Stopped,
    Running,
    Paused,
    Failed,
    Initializing
}

/// <summary>
/// Represents a state transition.
/// </summary>
public class StateTransition
{
    public PipelineState FromState { get; set; }
    public PipelineState ToState { get; set; }
    public DateTime Timestamp { get; set; }
    public string Reason { get; set; }
}

internal class StateChangeListener
{
    public Action<PipelineState, PipelineState> Callback { get; set; }
}

/// <summary>
/// Manages configuration state and overrides.
/// </summary>
public class ConfigurationStateManager
{
    private readonly ConcurrentDictionary<string, object> _overrides = new();
    private readonly ILogger<ConfigurationStateManager> _logger;

    public ConfigurationStateManager(ILogger<ConfigurationStateManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sets a configuration override.
    /// </summary>
    public void SetOverride(string key, object value)
    {
        _overrides[key] = value;
        _logger.LogInformation("Configuration override set: {Key} = {Value}", key, value);
    }

    /// <summary>
    /// Gets a configuration override.
    /// </summary>
    public T GetOverride<T>(string key, T defaultValue = default)
    {
        if (_overrides.TryGetValue(key, out var value) && value is T typed)
        {
            return typed;
        }

        return defaultValue;
    }

    /// <summary>
    /// Removes a configuration override.
    /// </summary>
    public bool RemoveOverride(string key)
    {
        return _overrides.TryRemove(key, out _);
    }

    /// <summary>
    /// Gets all active overrides.
    /// </summary>
    public Dictionary<string, object> GetAllOverrides()
    {
        return new Dictionary<string, object>(_overrides);
    }

    /// <summary>
    /// Clears all overrides.
    /// </summary>
    public void ClearAllOverrides()
    {
        _overrides.Clear();
        _logger.LogInformation("All configuration overrides cleared");
    }
}

/// <summary>
/// Tracks operation metrics and statistics.
/// </summary>
public class OperationMetricsTracker
{
    private readonly ConcurrentDictionary<string, OperationMetrics> _metrics = new();
    private readonly ILogger<OperationMetricsTracker> _logger;

    public OperationMetricsTracker(ILogger<OperationMetricsTracker> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Records an operation execution.
    /// </summary>
    public void RecordOperation(string operationName, long durationMs, bool success)
    {
        var metrics = _metrics.GetOrAdd(operationName, _ => new OperationMetrics());

        metrics.TotalExecutions++;
        metrics.TotalDurationMs += durationMs;

        if (success)
            metrics.SuccessfulExecutions++;
        else
            metrics.FailedExecutions++;

        if (durationMs < metrics.MinDurationMs || metrics.MinDurationMs == 0)
            metrics.MinDurationMs = durationMs;

        if (durationMs > metrics.MaxDurationMs)
            metrics.MaxDurationMs = durationMs;
    }

    /// <summary>
    /// Gets metrics for a specific operation.
    /// </summary>
    public OperationMetrics GetOperationMetrics(string operationName)
    {
        return _metrics.TryGetValue(operationName, out var metrics) ? metrics : null;
    }

    /// <summary>
    /// Gets all operation metrics.
    /// </summary>
    public Dictionary<string, OperationMetrics> GetAllMetrics()
    {
        return new Dictionary<string, OperationMetrics>(_metrics);
    }
}

/// <summary>
/// Metrics for a specific operation.
/// </summary>
public class OperationMetrics
{
    public long TotalExecutions { get; set; }
    public long SuccessfulExecutions { get; set; }
    public long FailedExecutions { get; set; }
    public long TotalDurationMs { get; set; }
    public long MinDurationMs { get; set; } = long.MaxValue;
    public long MaxDurationMs { get; set; }

    public double AverageDurationMs => TotalExecutions > 0 ? (double)TotalDurationMs / TotalExecutions : 0;
    public double SuccessRate => TotalExecutions > 0 ? (SuccessfulExecutions * 100.0) / TotalExecutions : 0;
}
