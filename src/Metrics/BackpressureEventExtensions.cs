#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Metrics;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Extension methods for <see cref="BackpressureEvent"/> that provide convenient
/// queries and formatting operations.
/// </summary>
public static class BackpressureEventExtensions
{
    /// <summary>
    /// Determines whether this backpressure event represents a critical situation
    /// where buffer fill percentage exceeds a threshold (80% by default).
    /// </summary>
    /// <param name="event">The backpressure event to evaluate.</param>
    /// <param name="thresholdPercent">The buffer fill percentage threshold (0-100).</param>
    /// <returns>
    /// <c>true</c> if <c>BufferFillPercent</c> is greater than or equal to the threshold;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="thresholdPercent"/> is less than 0 or greater than 100.
    /// </exception>
    public static bool IsCritical(this BackpressureEvent @event, double thresholdPercent = 80.0)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentOutOfRangeException.ThrowIfNegative(thresholdPercent);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(thresholdPercent, 100.0);

        return @event.BufferFillPercent >= thresholdPercent;
    }

    /// <summary>
    /// Calculates the severity level of this backpressure event based on buffer fill percentage.
    /// </summary>
    /// <param name="event">The backpressure event to evaluate.</param>
    /// <returns>
    /// A <see cref="BackpressureSeverity"/> enum value indicating the severity level:
    /// <list type="bullet">
    ///   <item><see cref="BackpressureSeverity.None"/></item> for 0-30%
    ///   <item><see cref="BackpressureSeverity.Low"/></item> for 31-60%
    ///   <item><see cref="BackpressureSeverity.Medium"/></item> for 61-80%
    ///   <item><see cref="BackpressureSeverity.High"/></item> for 81-95%
    ///   <item><see cref="BackpressureSeverity.Critical"/></item> for 96-100%
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <c>null</c>.</exception>
    public static BackpressureSeverity GetSeverityLevel(this BackpressureEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event.BufferFillPercent switch
        {
            <= 30.0 => BackpressureSeverity.None,
            <= 60.0 => BackpressureSeverity.Low,
            <= 80.0 => BackpressureSeverity.Medium,
            <= 95.0 => BackpressureSeverity.High,
            _ => BackpressureSeverity.Critical
        };
    }

    /// <summary>
    /// Formats the backpressure event as a human-readable string with invariant culture formatting.
    /// </summary>
    /// <param name="event">The backpressure event to format.</param>
    /// <returns>A formatted string representation of the event.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <c>null</c>.</exception>
    public static string ToFormattedString(this BackpressureEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return $"BackpressureEvent [Timestamp=" + @event.Timestamp.ToString("O") + $", Stage=" + @event.StageName +
               $", BufferFill=" + @event.BufferFillPercent.ToString("F2", CultureInfo.InvariantCulture) + "%, IsActivation=" +
               @event.IsActivation + $", DroppedItems=" + @event.DroppedItems + "]";
    }

    /// <summary>
    /// Determines whether this backpressure event represents a new activation.
    /// </summary>
    /// <param name="event">The backpressure event to evaluate.</param>
    /// <returns>
    /// <c>true</c> if this event represents a new activation (<c>IsActivation == true</c>);
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <c>null</c>.</exception>
    public static bool IsNewActivation(this BackpressureEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return @event.IsActivation;
    }

    /// <summary>
    /// Determines whether this backpressure event represents a release (backpressure ended).
    /// </summary>
    /// <param name="event">The backpressure event to evaluate.</param>
    /// <returns>
    /// <c>true</c> if this event represents a release (<c>IsActivation == false</c>);
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <c>null</c>.</exception>
    public static bool IsRelease(this BackpressureEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return !@event.IsActivation;
    }
}

/// <summary>
/// Represents the severity level of a backpressure event based on buffer fill percentage.
/// </summary>
public enum BackpressureSeverity
{
    /// <summary>No significant backpressure detected (0-30% buffer fill).</summary>
    None,

    /// <summary>Low backpressure (31-60% buffer fill).</summary>
    Low,

    /// <summary>Medium backpressure (61-80% buffer fill).</summary>
    Medium,

    /// <summary>High backpressure (81-95% buffer fill).</summary>
    High,

    /// <summary>Critical backpressure (96-100% buffer fill).</summary>
    Critical
}