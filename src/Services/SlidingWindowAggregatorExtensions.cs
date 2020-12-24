#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Extension methods for <see cref="SlidingWindowAggregator"/>.
/// </summary>
public static class SlidingWindowAggregatorExtensions
{
    /// <summary>
    /// Adds a data point to the aggregator with a generated timestamp.
    /// </summary>
    /// <param name="aggregator">The aggregator to add to.</param>
    /// <param name="value">The value of the data point.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregator"/> is null.</exception>
    public static void AddWithCurrentTimestamp(this SlidingWindowAggregator aggregator, double value)
    {
        ArgumentNullException.ThrowIfNull(aggregator);
        aggregator.Add(new DataPoint { Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Value = value });
    }

    /// <summary>
    /// Adds a batch of data points to the aggregator with generated timestamps.
    /// </summary>
    /// <param name="aggregator">The aggregator to add to.</param>
    /// <param name="values">The values of the data points.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregator"/> or <paramref name="values"/> is null.</exception>
    public static void AddRangeWithCurrentTimestamps(this SlidingWindowAggregator aggregator, IEnumerable<double> values)
    {
        ArgumentNullException.ThrowIfNull(aggregator);
        ArgumentNullException.ThrowIfNull(values);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var dataPoints = values.Select((value, index) => new DataPoint { Timestamp = now, Value = value });
        aggregator.AddRange(dataPoints);
    }

    /// <summary>
    /// Formats the aggregated data as a CSV string.
    /// </summary>
    /// <param name="result">The result to format.</param>
    /// <returns>A CSV string representation of the aggregated data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static string ToCsv(this SlidingWindowResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var csv = new List<string> { "WindowId,WindowStartMs,WindowEndMs,WindowSizeMs,StepIntervalMs,DataPointCount,Average,Sum,Min,Max,Trend" };
        csv.Add($"{result.WindowId},{result.WindowStartMs},{result.WindowEndMs},{result.WindowSizeMs},{result.StepIntervalMs},{result.DataPointCount},{result.Average.ToString(CultureInfo.InvariantCulture)},{result.Sum.ToString(CultureInfo.InvariantCulture)},{result.Min.ToString(CultureInfo.InvariantCulture)},{result.Max.ToString(CultureInfo.InvariantCulture)},{result.Trend.ToString(CultureInfo.InvariantCulture)}");
        return string.Join(Environment.NewLine, csv);
    }
}
