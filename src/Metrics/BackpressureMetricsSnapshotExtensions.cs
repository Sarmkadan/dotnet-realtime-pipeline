#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Metrics;

using System;
using System.Globalization;

/// <summary>
/// Provides extension methods for <see cref="BackpressureMetricsSnapshot"/>.
/// </summary>
public static class BackpressureMetricsSnapshotExtensions
{
    /// <summary>
    /// Determines whether the backpressure metrics indicate a critical state based on the given threshold.
    /// </summary>
    /// <param name="snapshot">The backpressure metrics snapshot.</param>
    /// <param name="threshold">The threshold percentage (0-100) above which the state is considered critical.</param>
    /// <returns><c>true</c> if any stage's current buffer fill percent exceeds the threshold; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="snapshot"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="threshold"/> is less than 0 or greater than 100.</exception>
    public static bool IsCritical(this BackpressureMetricsSnapshot snapshot, double threshold)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (threshold < 0 || threshold > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be between 0 and 100.");
        }

        foreach (var stage in snapshot.StageMetrics)
        {
            if (stage.CurrentBufferFillPercent > threshold)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Calculates the overall utilization percentage across all stages.
    /// </summary>
    /// <param name="snapshot">The backpressure metrics snapshot.</param>
    /// <returns>The average current buffer fill percentage across all stages, or 0 if there are no stages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="snapshot"/> is <c>null</c>.</exception>
    public static double UtilizationPercent(this BackpressureMetricsSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot.StageMetrics.Count == 0)
        {
            return 0.0;
        }

        double sum = 0.0;
        foreach (var stage in snapshot.StageMetrics)
        {
            sum += stage.CurrentBufferFillPercent;
        }

        return sum / snapshot.StageMetrics.Count;
    }

    /// <summary>
    /// Returns a human-readable display string for the backpressure metrics snapshot.
    /// </summary>
    /// <param name="snapshot">The backpressure metrics snapshot.</param>
    /// <returns>A formatted string containing key metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="snapshot"/> is <c>null</c>.</exception>
    public static string ToDisplayString(this BackpressureMetricsSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var utilization = snapshot.UtilizationPercent();
        var isCritical = snapshot.IsCritical(80.0);

        return string.Format(CultureInfo.InvariantCulture,
            "BackpressureMetricsSnapshot {{ SnapshotAt = {0}, TotalActivations = {1}, TotalDroppedItems = {2}, ActiveStages = {3}, Utilization = {4:F1}%, Critical = {5} }}",
            snapshot.SnapshotAt.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
            snapshot.TotalActivations,
            snapshot.TotalDroppedItems,
            snapshot.ActiveBackpressureStages,
            utilization,
            isCritical);
    }
}