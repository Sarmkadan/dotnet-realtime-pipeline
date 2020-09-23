#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetRealtimePipeline.Metrics;

/// <summary>
/// Extension methods that provide convenient aggregate queries over <see cref="BackpressureMetricsCollector"/>.
/// </summary>
public static class BackpressureMetricsCollectorExtensions
{
    /// <summary>
    /// Returns the total number of back‑pressure activations that have been recorded across all stages.
    /// </summary>
    /// <param name="collector">The collector instance.</param>
    /// <returns>The sum of <c>ActivationCount</c> for every stage.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collector"/> is <c>null</c>.</exception>
    public static long GetTotalActivations(this BackpressureMetricsCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);
        return collector.GetSnapshot().TotalActivations;
    }

    /// <summary>
    /// Returns the total number of items that have been dropped across the whole pipeline.
    /// </summary>
    /// <param name="collector">The collector instance.</param>
    /// <returns>The sum of <c>TotalDroppedItems</c> for every stage.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collector"/> is <c>null</c>.</exception>
    public static long GetTotalDroppedItems(this BackpressureMetricsCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);
        return collector.GetSnapshot().TotalDroppedItems;
    }

    /// <summary>
    /// Returns the highest buffer‑fill percentage ever observed among all stages.
    /// </summary>
    /// <param name="collector">The collector instance.</param>
    /// <returns>The maximum <c>PeakBufferFillPercent</c> value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collector"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">No stage metrics are available.</exception>
    public static double GetOverallPeakBufferFillPercent(this BackpressureMetricsCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);
        var snapshot = collector.GetSnapshot();

        // If there are no stages, Max() would throw; surface a clear exception.
        if (snapshot.StageMetrics.Count == 0)
            throw new InvalidOperationException("No stage metrics are available to calculate a peak buffer fill percent.");

        return snapshot.StageMetrics.Max(m => m.PeakBufferFillPercent);
    }

    /// <summary>
    /// Retrieves the most recent back‑pressure events that represent activations.
    /// </summary>
    /// <param name="collector">The collector instance.</param>
    /// <param name="count">
    /// The maximum number of activation events to return. Must be greater than zero.
    /// </param>
    /// <returns>
    /// A read‑only list of <see cref="BackpressureEvent"/> where <c>IsActivation</c> is <c>true</c>,
    /// ordered from oldest to newest.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="collector"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than or equal to zero.</exception>
    public static IReadOnlyList<BackpressureEvent> GetRecentActivationEvents(
        this BackpressureMetricsCollector collector,
        int count = 50)
    {
        ArgumentNullException.ThrowIfNull(collector);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        // Get the most recent events (including both activations and releases) and filter.
        var recent = collector.GetRecentEvents(count);
        var activations = recent.Where(e => e.IsActivation).ToList();

        return activations.AsReadOnly();
    }
}
