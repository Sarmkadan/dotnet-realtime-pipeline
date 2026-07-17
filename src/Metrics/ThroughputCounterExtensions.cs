// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Metrics;

using System;
using System.Globalization;

/// <summary>
/// Provides extension methods for <see cref="ThroughputCounter"/> to simplify throughput recording and retrieval.
/// </summary>
public static class ThroughputCounterExtensions
{
    /// <summary>
    /// Records a single event in the global throughput counter.
    /// </summary>
    /// <param name="counter">The <see cref="ThroughputCounter"/> instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="counter"/> is null.</exception>
    public static void RecordSingleEvent(this ThroughputCounter counter)
    {
        ArgumentNullException.ThrowIfNull(counter);
        counter.RecordEvents(1);
    }

    /// <summary>
    /// Records a single event in the specified stage of the throughput counter.
    /// </summary>
    /// <param name="counter">The <see cref="ThroughputCounter"/> instance.</param>
    /// <param name="stageName">The name of the stage.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="counter"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="stageName"/> is null or whitespace.</exception>
    public static void RecordSingleEvent(this ThroughputCounter counter, string stageName)
    {
        ArgumentNullException.ThrowIfNull(counter);
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        counter.RecordEvents(stageName, 1);
    }

    /// <summary>
    /// Gets the global throughput as a formatted string.
    /// </summary>
    /// <param name="counter">The <see cref="ThroughputCounter"/> instance.</param>
    /// <returns>A string representation of the throughput with 2 decimal places.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="counter"/> is null.</exception>
    public static string GetFormattedThroughput(this ThroughputCounter counter)
    {
        ArgumentNullException.ThrowIfNull(counter);
        return counter.GetThroughput().ToString("F2", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets the throughput for a specific stage as a formatted string.
    /// </summary>
    /// <param name="counter">The <see cref="ThroughputCounter"/> instance.</param>
    /// <param name="stageName">The name of the stage.</param>
    /// <returns>A string representation of the throughput with 2 decimal places.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="counter"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="stageName"/> is null or whitespace.</exception>
    public static string GetFormattedThroughput(this ThroughputCounter counter, string stageName)
    {
        ArgumentNullException.ThrowIfNull(counter);
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        return counter.GetThroughput(stageName).ToString("F2", CultureInfo.InvariantCulture);
    }
}
