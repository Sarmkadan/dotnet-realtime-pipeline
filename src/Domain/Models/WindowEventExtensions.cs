#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DotNetRealtimePipeline.Domain.Models;

/// <summary>
/// Extension methods for <see cref="WindowEvent"/> that provide additional
/// analysis and formatting capabilities.
/// </summary>
public static class WindowEventExtensions
{
    /// <summary>
    /// Gets the duration of the window as a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="windowEvent">The window to evaluate.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the window length.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="windowEvent"/> is <c>null</c>.</exception>
    public static TimeSpan GetDuration(this WindowEvent windowEvent)
    {
        ArgumentNullException.ThrowIfNull(windowEvent);
        return TimeSpan.FromMilliseconds(windowEvent.GetDurationMs());
    }

    /// <summary>
    /// Returns the data points sorted by their timestamp.
    /// </summary>
    /// <param name="windowEvent">The window whose data points are to be sorted.</param>
    /// <returns>An <see cref="IReadOnlyList{DataPoint}"/> ordered by <c>Timestamp</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="windowEvent"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="windowEvent"/>.DataPoints is <c>null</c>.</exception>
    public static IReadOnlyList<DataPoint> GetDataPointsSortedByTimestamp(this WindowEvent windowEvent)
    {
        ArgumentNullException.ThrowIfNull(windowEvent);
        ArgumentNullException.ThrowIfNull(windowEvent.DataPoints);
        return windowEvent.DataPoints
            .OrderBy(dp => dp.Timestamp)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Calculates the value at the specified percentile of the window's data point values.
    /// </summary>
    /// <param name="windowEvent">The window to analyse.</param>
    /// <param name="percentile">
    /// The desired percentile (0‑100). Values outside this range cause an <see cref="ArgumentOutOfRangeException"/>.
    /// </param>
    /// <returns>The percentile value, or <c>0</c> if the window contains no data points.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="windowEvent"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="percentile"/> is not between 0 and 100.</exception>
    public static double GetPercentile(this WindowEvent windowEvent, double percentile)
    {
        ArgumentNullException.ThrowIfNull(windowEvent);
        ArgumentOutOfRangeException.ThrowIfLessThan(percentile, 0.0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percentile, 100.0);

        if (windowEvent.DataPoints.Count == 0)
            return 0.0;

        // Extract the values and sort them.
        double[] ordered = windowEvent.DataPoints
            .Select(dp => dp.Value)
            .OrderBy(v => v)
            .ToArray();

        int n = ordered.Length;
        double rank = percentile / 100.0 * (n - 1);
        int lower = (int)Math.Floor(rank);
        int upper = (int)Math.Ceiling(rank);

        // Exact match.
        if (lower == upper)
            return ordered[lower];

        // Linear interpolation between the two surrounding values.
        double weight = rank - lower;
        return ordered[lower] * (1 - weight) + ordered[upper] * weight;
    }

    /// <summary>
    /// Produces a concise, culture‑invariant summary string for the window.
    /// </summary>
    /// <param name="windowEvent">The window to summarise.</param>
    /// <returns>A formatted string containing key metrics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="windowEvent"/> is <c>null</c>.</exception>
    public static string ToSummaryString(this WindowEvent windowEvent)
    {
        ArgumentNullException.ThrowIfNull(windowEvent);
        return string.Format(
            CultureInfo.InvariantCulture,
            "Window {0} [{1}-{2}] ({3} ms): Count={4}, Avg={5:F2}, Min={6:F2}, Max={7:F2}, StdDev={8:F2}",
            windowEvent.WindowId,
            windowEvent.WindowStartMs,
            windowEvent.WindowEndMs,
            windowEvent.GetDurationMs(),
            windowEvent.GetDataPointCount(),
            windowEvent.CalculateAverage(),
            windowEvent.CalculateMin(),
            windowEvent.CalculateMax(),
            windowEvent.CalculateStandardDeviation());
    }
}
