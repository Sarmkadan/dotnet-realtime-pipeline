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
public sealed class PipelineStateManager
{
    private volatile PipelineState _currentState = PipelineState.Stopped;
    private readonly List<StateTransition> _stateHistory = new();
    private readonly object _lockObject = new();
    private readonly ILogger<PipelineStateManager> _logger;
    private readonly List<StateChangeListener> _listeners = new();

    /// <summary>
    /// Initializes a new instance of the PipelineStateManager class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
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
    /// <param name="newState">The new state to transition to.</param>
    /// <param name="reason">The reason for the state transition.</param>
    /// <returns>True if the transition was successful, false otherwise.</returns>
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
    /// <param name="listener">The listener to register.</param>
    public void RegisterStateChangeListener(Action<PipelineState, PipelineState> listener)
    {
        _listeners.Add(new StateChangeListener { Callback = listener });
    }

    /// <summary>
    /// Gets the state history.
    /// </summary>
    /// <returns>A list of state transitions.</returns>
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
    /// <returns>The time spent in the current state.</returns>
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
    /// <returns>True if the pipeline is operational, false otherwise.</returns>
    public bool IsOperational => _currentState == PipelineState.Running;

    /// <summary>
    /// Validates state transitions.
    /// </summary>
    /// <param name="from">The current state.</param>
    /// <param name="to">The new state.</param>
    /// <returns>True if the transition is valid, false otherwise.</returns>
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
    /// <param name="oldState">The previous state.</param>
    /// <param name="newState">The new state.</param>
    /// <param name="reason">The reason for the state transition.</param>
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
    /// <summary>
    /// The pipeline is stopped.
    /// </summary>
    Stopped,
    /// <summary>
    /// The pipeline is running.
    /// </summary>
    Running,
    /// <summary>
    /// The pipeline is paused.
    /// </summary>
    Paused,
    /// <summary>
    /// The pipeline has failed.
    /// </summary>
    Failed,
    /// <summary>
    /// The pipeline is initializing.
    /// </summary>
    Initializing
}

/// <summary>
/// Represents a state transition.
/// </summary>
public sealed class StateTransition
{
    /// <summary>
    /// Gets or sets the previous state.
    /// </summary>
    public PipelineState FromState { get; set; }
    /// <summary>
    /// Gets or sets the new state.
    /// </summary>
    public PipelineState ToState { get; set; }
    /// <summary>
    /// Gets or sets the timestamp of the transition.
    /// </summary>
    public DateTime Timestamp { get; set; }
    /// <summary>
    /// Gets or sets the reason for the transition.
    /// </summary>
    public string Reason { get; set; }
}

internal sealed class StateChangeListener
{
    /// <summary>
    /// Gets or sets the callback for the state change listener.
    /// </summary>
    public Action<PipelineState, PipelineState> Callback { get; set; }
}

/// <summary>
/// Manages configuration state and overrides.
/// </summary>
public sealed class ConfigurationStateManager
{
    private readonly ConcurrentDictionary<string, object> _overrides = new();
    private readonly ILogger<ConfigurationStateManager> _logger;

    /// <summary>
    /// Initializes a new instance of the ConfigurationStateManager class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ConfigurationStateManager(ILogger<ConfigurationStateManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sets a configuration override.
    /// </summary>
    /// <param name="key">The key of the override.</param>
    /// <param name="value">The value of the override.</param>
    public void SetOverride(string key, object value)
    {
        _overrides[key] = value;
        _logger.LogInformation("Configuration override set: {Key} = {Value}", key, value);
    }

    /// <summary>
    /// Gets a configuration override.
    /// </summary>
    /// <typeparam name="T">The type of the override value.</typeparam>
    /// <param name="key">The key of the override.</param>
    /// <param name="defaultValue">The default value to return if the override is not set.</param>
    /// <returns>The value of the override, or the default value if not set.</returns>
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
    /// <param name="key">The key of the override to remove.</param>
    /// <returns>True if the override was removed, false otherwise.</returns>
    public bool RemoveOverride(string key)
    {
        return _overrides.TryRemove(key, out _);
    }

    /// <summary>
    /// Gets all active overrides.
    /// </summary>
    /// <returns>A dictionary of all active overrides.</returns>
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
public sealed class OperationMetricsTracker
{
    private readonly ConcurrentDictionary<string, OperationMetrics> _metrics = new();
    private readonly ILogger<OperationMetricsTracker> _logger;

    /// <summary>
    /// Initializes a new instance of the OperationMetricsTracker class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public OperationMetricsTracker(ILogger<OperationMetricsTracker> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Records an operation execution.
    /// </summary>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="durationMs">The duration of the operation in milliseconds.</param>
    /// <param name="success">Whether the operation was successful.</param>
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
    /// <param name="operationName">The name of the operation.</param>
    /// <returns>The metrics for the operation, or null if not found.</returns>
    public OperationMetrics GetOperationMetrics(string operationName)
    {
        return _metrics.TryGetValue(operationName, out var metrics) ? metrics : null;
    }

    /// <summary>
    /// Gets all operation metrics.
    /// </summary>
    /// <returns>A dictionary of all operation metrics.</returns>
    public Dictionary<string, OperationMetrics> GetAllMetrics()
    {
        return new Dictionary<string, OperationMetrics>(_metrics);
    }
}

/// <summary>
/// Metrics for a specific operation.
/// </summary>
public sealed class OperationMetrics
{
    /// <summary>
    /// Gets or sets the total number of executions.
    /// </summary>
    public long TotalExecutions { get; set; }
    /// <summary>
    /// Gets or sets the number of successful executions.
    /// </summary>
    public long SuccessfulExecutions { get; set; }
    /// <summary>
    /// Gets or sets the number of failed executions.
    /// </summary>
    public long FailedExecutions { get; set; }
    /// <summary>
    /// Gets or sets the total duration of all executions in milliseconds.
    /// </summary>
    public long TotalDurationMs { get; set; }
    /// <summary>
    /// Gets or sets the minimum duration of all executions in milliseconds.
    /// </summary>
    public long MinDurationMs { get; set; } = long.MaxValue;
    /// <summary>
    /// Gets or sets the maximum duration of all executions in milliseconds.
    /// </summary>
    public long MaxDurationMs { get; set; }

    /// <summary>
    /// Gets the average duration of all executions in milliseconds.
    /// </summary>
    public double AverageDurationMs => TotalExecutions > 0 ? (double)TotalDurationMs / TotalExecutions : 0;
    /// <summary>
    /// Gets the success rate of the operation as a percentage.
    /// </summary>
    public double SuccessRate => TotalExecutions > 0 ? (SuccessfulExecutions * 100.0) / TotalExecutions : 0;
}
