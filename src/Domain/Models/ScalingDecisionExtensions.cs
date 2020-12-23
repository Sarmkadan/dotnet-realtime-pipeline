#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Globalization;

public static class ScalingDecisionExtensions
{
    /// <summary>Indicates whether a scaling decision increased the consumer count.</summary>
    /// <param name="decision">The scaling decision to evaluate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="decision"/> is <c>null</c>.</exception>
    public static bool IsScaleUp(this ScalingDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        return decision.Direction == ScalingDirection.Up;
    }

    /// <summary>Indicates whether a scaling decision decreased the consumer count.</summary>
    /// <param name="decision">The scaling decision to evaluate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="decision"/> is <c>null</c>.</exception>
    public static bool IsScaleDown(this ScalingDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        return decision.Direction == ScalingDirection.Down;
    }

    /// <summary>Gets a human-readable summary of the scaling decision.</summary>
    /// <param name="decision">The scaling decision to summarize.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="decision"/> is <c>null</c>.</exception>
    public static string GetSummary(this ScalingDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        return $"Stage '{decision.StageName}' scaled {(decision.IsScaleUp() ? "up" : "down")} from {decision.FromConsumers} to {decision.ToConsumers} consumers. Reason: {decision.Reason}";
    }

    /// <summary>Formats the scaling decision as a CSV row.</summary>
    /// <param name="decision">The scaling decision to format.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="decision"/> is <c>null</c>.</exception>
    public static string ToCsvRow(this ScalingDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        return $"{decision.StageName},{decision.DecidedAt.ToString("o", CultureInfo.InvariantCulture)},{decision.Direction},{decision.FromConsumers},{decision.ToConsumers},{decision.BufferFillPercent.ToString(CultureInfo.InvariantCulture)},{decision.BackpressureFrequency.ToString(CultureInfo.InvariantCulture)}";
    }
}
