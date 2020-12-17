#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Validation helpers for <see cref="BackpressureMetricsCollectorTests"/> unit tests.
/// </summary>
public static class BackpressureMetricsCollectorTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="BackpressureMetricsCollectorTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this BackpressureMetricsCollectorTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate public methods are not null (they're delegates in the test class)
        if (value.GetStageMetrics_UnknownStage_ReturnsNull == null)
        {
            errors.Add("GetStageMetrics_UnknownStage_ReturnsNull delegate cannot be null.");
        }

        if (value.RecordManualEvent_Activation_IncrementsActivationCount == null)
        {
            errors.Add("RecordManualEvent_Activation_IncrementsActivationCount delegate cannot be null.");
        }

        if (value.RecordManualEvent_TwoActivations_CountIsTwo == null)
        {
            errors.Add("RecordManualEvent_TwoActivations_CountIsTwo delegate cannot be null.");
        }

        if (value.GetSnapshot_WithNoEvents_ReturnsEmptySnapshot == null)
        {
            errors.Add("GetSnapshot_WithNoEvents_ReturnsEmptySnapshot delegate cannot be null.");
        }

        if (value.GetSnapshot_AggregatesAcrossStages == null)
        {
            errors.Add("GetSnapshot_AggregatesAcrossStages delegate cannot be null.");
        }

        if (value.GetRecentEvents_ReturnsUpToRequestedCount == null)
        {
            errors.Add("GetRecentEvents_ReturnsUpToRequestedCount delegate cannot be null.");
        }

        if (value.GetStageEvents_ReturnsOnlyEventsForThatStage == null)
        {
            errors.Add("GetStageEvents_ReturnsOnlyEventsForThatStage delegate cannot be null.");
        }

        if (value.Reset_ClearsAllMetricsAndEvents == null)
        {
            errors.Add("Reset_ClearsAllMetricsAndEvents delegate cannot be null.");
        }

        if (value.Poll_AfterBackpressureActivated_RecordsActivationEvent == null)
        {
            errors.Add("Poll_AfterBackpressureActivated_RecordsActivationEvent delegate cannot be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="BackpressureMetricsCollectorTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static bool IsValid(this BackpressureMetricsCollectorTests value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="BackpressureMetricsCollectorTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing a list of problems.</exception>
    public static void EnsureValid(this BackpressureMetricsCollectorTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"BackpressureMetricsCollectorTests instance is invalid. Errors:{Environment.NewLine}" +
                string.Join(Environment.NewLine, errors));
        }
    }
}