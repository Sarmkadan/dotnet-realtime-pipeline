#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="BackpressureService"/> instances.
/// Validates all public members for null, empty, out-of-range, and default values.
/// </summary>
public static class BackpressureServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="BackpressureService"/> instance.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this BackpressureService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate system status metrics
        var status = value.GetSystemStatus();
        if (status.TotalStages < 0)
        {
            errors.Add($"TotalStages must be non-negative, but was {status.TotalStages}");
        }

        if (status.BackpressuredStages < 0)
        {
            errors.Add($"BackpressuredStages must be non-negative, but was {status.BackpressuredStages}");
        }

        if (status.BackpressuredStages > status.TotalStages)
        {
            errors.Add($"BackpressuredStages ({status.BackpressuredStages}) cannot exceed TotalStages ({status.TotalStages})");
        }

        if (status.AverageBufferFillPercent is < 0 or > 100)
        {
            errors.Add($"AverageBufferFillPercent must be between 0 and 100, but was {status.AverageBufferFillPercent}");
        }

        if (status.TotalBackpressureTimeMs < 0)
        {
            errors.Add($"TotalBackpressureTimeMs must be non-negative, but was {status.TotalBackpressureTimeMs}");
        }

        if (status.Timestamp == default)
        {
            errors.Add("Timestamp must be set to a non-default DateTime value");
        }

        if (status.TotalDroppedItems < 0)
        {
            errors.Add($"TotalDroppedItems must be non-negative, but was {status.TotalDroppedItems}");
        }

        // Validate buffer status
        var bufferStatus = value.GetBufferStatus();
        ArgumentNullException.ThrowIfNull(bufferStatus);

        foreach (var kvp in bufferStatus)
        {
            if (kvp.Value < 0)
            {
                errors.Add($"Buffer size for stage '{kvp.Key}' must be non-negative, but was {kvp.Value}");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="BackpressureService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this BackpressureService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="BackpressureService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this BackpressureService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"BackpressureService validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}