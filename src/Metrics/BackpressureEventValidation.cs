#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

namespace DotNetRealtimePipeline.Metrics;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="BackpressureEvent"/> instances.
/// </summary>
public static class BackpressureEventValidation
{
    /// <summary>
    /// Validates the specified <see cref="BackpressureEvent"/> instance.
    /// </summary>
    /// <param name="value">The event to validate.</param>
    /// <returns>An empty list if the instance is valid; otherwise, a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this BackpressureEvent value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.StageName))
        {
            errors.Add("StageName cannot be null or whitespace.");
        }

        if (value.BufferFillPercent < 0 || value.BufferFillPercent > 100)
        {
            errors.Add(string.Format(
                CultureInfo.InvariantCulture,
                "BufferFillPercent must be between 0 and 100, but was {0:F2}.",
                value.BufferFillPercent));
        }

        if (value.Timestamp == default)
        {
            errors.Add("Timestamp cannot be default DateTime.");
        }

        if (value.DroppedItems < 0)
        {
            errors.Add(string.Format(
                CultureInfo.InvariantCulture,
                "DroppedItems must be non-negative, but was {0}.",
                value.DroppedItems));
        }

        if (value.IsActivation is false)
        {
            errors.Add("IsActivation must be true for activation events.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="BackpressureEvent"/> instance is valid.
    /// </summary>
    /// <param name="value">The event to check.</param>
    /// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this BackpressureEvent value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="BackpressureEvent"/> instance is valid.
    /// </summary>
    /// <param name="value">The event to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this BackpressureEvent value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "BackpressureEvent validation failed:\n" + string.Join("\n", errors),
                nameof(value));
        }
    }
}