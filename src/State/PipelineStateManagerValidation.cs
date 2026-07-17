#nullable enable

namespace DotNetRealtimePipeline.State;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="PipelineStateManager"/> instances.
/// </summary>
public static class PipelineStateManagerValidation
{
    /// <summary>
    /// Validates the specified <see cref="PipelineStateManager"/> instance.
    /// </summary>
    /// <param name="value">The pipeline state manager to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PipelineStateManager? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate CurrentState
        if (value.CurrentState == default)
        {
            errors.Add("CurrentState is not set (default value).");
        }

        // Validate state history
        var stateHistory = value.GetStateHistory();
        ArgumentNullException.ThrowIfNull(stateHistory);

        ValidateStateHistory(stateHistory, errors);

        // Validate current state duration
        var currentDuration = value.GetCurrentStateDuration();
        if (currentDuration < TimeSpan.Zero)
        {
            errors.Add("GetCurrentStateDuration() returned a negative time span.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PipelineStateManager"/> instance is valid.
    /// </summary>
    /// <param name="value">The pipeline state manager to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PipelineStateManager value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="PipelineStateManager"/> instance is valid.
    /// </summary>
    /// <param name="value">The pipeline state manager to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this PipelineStateManager value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "PipelineStateManager validation failed. Errors:\n" + string.Join("\n", errors),
                nameof(value));
        }
    }

    private static void ValidateStateHistory(IReadOnlyList<StateTransition> stateHistory, List<string> errors)
    {
        if (stateHistory.Count == 0)
        {
            return; // Empty history is valid for a newly created manager
        }

        PipelineState? previousState = null;
        DateTime? previousTimestamp = null;

        for (int i = 0; i < stateHistory.Count; i++)
        {
            var transition = stateHistory[i];

            if (transition is null)
            {
                errors.Add($"State history at index {i} is null.");
                continue;
            }

            // Validate FromState
            if (transition.FromState == default)
            {
                errors.Add($"StateTransition at index {i} has FromState set to default value.");
            }

            // Validate ToState
            if (transition.ToState == default)
            {
                errors.Add($"StateTransition at index {i} has ToState set to default value.");
            }

            // Validate Timestamp
            if (transition.Timestamp == default)
            {
                errors.Add($"StateTransition at index {i} has Timestamp set to default (DateTime.MinValue).");
            }
            else if (transition.Timestamp.Kind != DateTimeKind.Utc)
            {
                errors.Add($"StateTransition at index {i} has Timestamp that is not in UTC format.");
            }

            // Validate Reason
            if (string.IsNullOrWhiteSpace(transition.Reason))
            {
                // Reason can be empty, but if provided it should not be whitespace only
                // No error for empty reason
            }

            // Validate state transition sequence
            if (previousState.HasValue && previousTimestamp.HasValue)
            {
                if (transition.Timestamp < previousTimestamp.Value)
                {
                    errors.Add(
                        $"State history at index {i} has timestamp {transition.Timestamp:O} " +
                        $"that is earlier than previous timestamp {previousTimestamp.Value:O}.");
                }

                // Check if transition is valid according to state machine rules
                if (!IsValidTransition(previousState.Value, transition.FromState))
                {
                    errors.Add(
                        $"State history at index {i} shows invalid transition: " +
                        $"{previousState.Value} -> {transition.FromState}.");
                }
            }

            previousState = transition.ToState;
            previousTimestamp = transition.Timestamp;
        }
    }

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
}