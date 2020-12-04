#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Metrics;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for backpressure metrics types.
/// </summary>
public static class BackpressureMetricsCollectorValidation
{
    /// <summary>
    /// Validates the specified <see cref="StageBackpressureMetrics"/> instance.
    /// </summary>
    /// <param name="value">The stage metrics to validate.</param>
    /// <returns>An empty list if the instance is valid; otherwise, a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this StageBackpressureMetrics value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.StageName))
        {
            errors.Add("StageName cannot be null or whitespace.");
        }

        if (value.ActivationCount < 0)
        {
            errors.Add($"ActivationCount must be non-negative, but was {value.ActivationCount}.");
        }

        if (value.TotalActiveDurationMs < 0)
        {
            errors.Add($"TotalActiveDurationMs must be non-negative, but was {value.TotalActiveDurationMs}.");
        }

        if (value.PeakBufferFillPercent < 0 || value.PeakBufferFillPercent > 100)
        {
            errors.Add(string.Format(
                CultureInfo.InvariantCulture,
                "PeakBufferFillPercent must be between 0 and 100, but was {0:F2}.",
                value.PeakBufferFillPercent));
        }

        if (value.CurrentBufferFillPercent < 0 || value.CurrentBufferFillPercent > 100)
        {
            errors.Add(string.Format(
                CultureInfo.InvariantCulture,
                "CurrentBufferFillPercent must be between 0 and 100, but was {0:F2}.",
                value.CurrentBufferFillPercent));
        }

        if (value.TotalDroppedItems < 0)
        {
            errors.Add($"TotalDroppedItems must be non-negative, but was {value.TotalDroppedItems}.");
        }

        if (value.LastActivationAt.HasValue && value.LastActivationAt.Value == default)
        {
            errors.Add("LastActivationAt has default DateTime value, which is invalid.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the specified <see cref="BackpressureMetricsSnapshot"/> instance.
    /// </summary>
    /// <param name="value">The snapshot to validate.</param>
    /// <returns>An empty list if the instance is valid; otherwise, a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this BackpressureMetricsSnapshot value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.StageMetrics is null)
        {
            errors.Add("StageMetrics cannot be null.");
        }
        else
        {
            if (value.StageMetrics.Count == 0)
            {
                errors.Add("StageMetrics list cannot be empty.");
            }

            for (int i = 0; i < value.StageMetrics.Count; i++)
            {
                var stageMetrics = value.StageMetrics[i];
                if (stageMetrics is null)
                {
                    errors.Add($"StageMetrics[{i}] cannot be null.");
                }
                else
                {
                    var stageErrors = stageMetrics.Validate();
                    if (stageErrors.Count > 0)
                    {
                        errors.AddRange(stageErrors.Select(e => $"StageMetrics[{i}]: {e}"));
                    }
                }
            }
        }

        if (value.TotalActivations < 0)
        {
            errors.Add($"TotalActivations must be non-negative, but was {value.TotalActivations}.");
        }

        if (value.TotalDroppedItems < 0)
        {
            errors.Add($"TotalDroppedItems must be non-negative, but was {value.TotalDroppedItems}.");
        }

        if (value.ActiveBackpressureStages < 0)
        {
            errors.Add($"ActiveBackpressureStages must be non-negative, but was {value.ActiveBackpressureStages}.");
        }

        if (value.SnapshotAt == default)
        {
            errors.Add("SnapshotAt cannot be default DateTime.");
        }

        return errors.AsReadOnly();
    }


    /// <summary>
    /// Determines whether the specified <see cref="StageBackpressureMetrics"/> instance is valid.
    /// </summary>
    /// <param name="value">The stage metrics to check.</param>
    /// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this StageBackpressureMetrics value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="BackpressureMetricsSnapshot"/> instance is valid.
    /// </summary>
    /// <param name="value">The snapshot to check.</param>
    /// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this BackpressureMetricsSnapshot value)
    {
        return value.Validate().Count == 0;
    }


    /// <summary>
    /// Ensures that the specified <see cref="StageBackpressureMetrics"/> instance is valid.
    /// </summary>
    /// <param name="value">The stage metrics to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this StageBackpressureMetrics value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "StageBackpressureMetrics validation failed:\n" + string.Join("\n", errors),
                nameof(value));
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="BackpressureMetricsSnapshot"/> instance is valid.
    /// </summary>
    /// <param name="value">The snapshot to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this BackpressureMetricsSnapshot value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "BackpressureMetricsSnapshot validation failed:\n" + string.Join("\n", errors),
                nameof(value));
        }
    }
}