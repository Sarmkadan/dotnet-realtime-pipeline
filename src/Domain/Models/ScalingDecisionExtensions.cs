#nullable enable
// =============================================================================
// Author: [Your Name]
// =====================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

public static class ScalingDecisionExtensions
{
    /// <summary>Indicates whether a scaling decision increased the consumer count.</summary>
    /// <param name="decision">The scaling decision to evaluate.</param>
    /// <returns><c>true</c> if the scaling direction is <see cref="ScalingDirection.Up"/>; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="decision"/> is <c>null</c>.</exception>
    public static bool IsScaleUp(this ScalingDecision decision) => decision.Direction == ScalingDirection.Up;

    /// <summary>Indicates whether a scaling decision decreased the consumer count.</summary>
    /// <param name="decision">The scaling decision to evaluate.</param>
    /// <returns><c>true</c> if the scaling direction is <see cref="ScalingDirection.Down"/>; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="decision"/> is <c>null</c>.</exception>
    public static bool IsScaleDown(this ScalingDecision decision) => decision.Direction == ScalingDirection.Down;

    /// <summary>Gets a human-readable summary of the scaling decision.</summary>
    /// <param name="decision">The scaling decision to summarize.</param>
    /// <returns>A formatted string containing the scaling decision details.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="decision"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="decision.StageName"/> or <paramref name="decision.Reason"/> is null or whitespace.</exception>
    public static string GetSummary(this ScalingDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        if (string.IsNullOrWhiteSpace(decision.StageName))
        {
            throw new ArgumentException("StageName cannot be null or whitespace.", nameof(decision));
        }

        if (string.IsNullOrWhiteSpace(decision.Reason))
        {
            throw new ArgumentException("Reason cannot be null or whitespace.", nameof(decision));
        }

        return $"Stage '{decision.StageName}' scaled {(decision.Direction == ScalingDirection.Up ? "up" : "down")} from {decision.FromConsumers} to {decision.ToConsumers} consumers. Reason: {decision.Reason}";
    }

    /// <summary>Formats the scaling decision as a CSV row.</summary>
    /// <param name="decision">The scaling decision to format.</param>
    /// <returns>A CSV-formatted string containing the scaling decision data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="decision"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="decision.StageName"/> is null or whitespace.
    /// </exception>
    public static string ToCsvRow(this ScalingDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        if (string.IsNullOrWhiteSpace(decision.StageName))
        {
            throw new ArgumentException("StageName cannot be null or whitespace.", nameof(decision));
        }

        return $"{decision.StageName},{decision.DecidedAt.ToString("o", CultureInfo.InvariantCulture)},{decision.Direction},{decision.FromConsumers},{decision.ToConsumers},{decision.BufferFillPercent.ToString(CultureInfo.InvariantCulture)},{decision.BackpressureFrequency.ToString(CultureInfo.InvariantCulture)}";
    }
}