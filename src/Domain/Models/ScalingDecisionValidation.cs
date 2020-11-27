#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="ScalingDecision"/> instances.
/// </summary>
public static class ScalingDecisionValidation
{
    /// <summary>
    /// Validates a <see cref="ScalingDecision"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The scaling decision to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ScalingDecision? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate StageName
        if (string.IsNullOrWhiteSpace(value.StageName))
        {
            errors.Add("StageName cannot be null or whitespace.");
        }

        // Validate Reason
        if (string.IsNullOrWhiteSpace(value.Reason))
        {
            errors.Add("Reason cannot be null or whitespace.");
        }

        // Validate FromConsumers (must be non-negative)
        if (value.FromConsumers < 0)
        {
            errors.Add("FromConsumers must be a non-negative integer.");
        }

        // Validate ToConsumers (must be non-negative)
        if (value.ToConsumers < 0)
        {
            errors.Add("ToConsumers must be a non-negative integer.");
        }

        // Validate consumer transition (ToConsumers should be >= FromConsumers for Up/None, can be < for Down)
        if (value.Direction != ScalingDirection.Down && value.ToConsumers < value.FromConsumers)
        {
            errors.Add("ToConsumers must be greater than or equal to FromConsumers for Up or None scaling directions.");
        }

        // Validate BufferFillPercent (0-100 range)
        if (value.BufferFillPercent < 0 || value.BufferFillPercent > 100)
        {
            errors.Add("BufferFillPercent must be between 0 and 100 inclusive.");
        }

        // Validate BackpressureFrequency (must be non-negative)
        if (value.BackpressureFrequency < 0)
        {
            errors.Add("BackpressureFrequency must be a non-negative value.");
        }

        // Validate DecidedAt (should not be default/MinValue)
        if (value.DecidedAt == default)
        {
            errors.Add("DecidedAt cannot be the default DateTime value.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ScalingDecision"/> instance is valid.
    /// </summary>
    /// <param name="value">The scaling decision to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ScalingDecision? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a <see cref="ScalingDecision"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with detailed error messages if it is not.
    /// </summary>
    /// <param name="value">The scaling decision to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the scaling decision is invalid, with error details.</exception>
    public static void EnsureValid(this ScalingDecision? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ScalingDecision is invalid. Errors: {string.Join(" ", errors)}");
        }
    }
}