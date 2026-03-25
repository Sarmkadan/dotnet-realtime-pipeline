#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

/// <summary>
/// Represents a windowed aggregation of data points over a time interval.
/// Used for time-series aggregations and windowing operations.
/// </summary>
public sealed class WindowEvent
{
    public long WindowId { get; set; }
    public long WindowStartMs { get; set; }
    public long WindowEndMs { get; set; }
    public string AggregationType { get; set; } = "";
    public List<DataPoint> DataPoints { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
    public bool IsComplete { get; set; }

    /// <summary>
    /// Monotonic timestamp (from <see cref="Stopwatch.GetTimestamp()"/>) recorded when
    /// this window was created. Used for clock-skew-safe completion detection.
    /// </summary>
    public long CreatedAtTicks { get; set; }

    public WindowEvent()
    {
        DataPoints = new();
        CreatedAtTicks = Stopwatch.GetTimestamp();
    }

    public WindowEvent(long windowId, long startMs, long endMs, string aggregationType)
    {
        WindowId = windowId;
        WindowStartMs = startMs;
        WindowEndMs = endMs;
        AggregationType = aggregationType ?? throw new ArgumentNullException(nameof(aggregationType));
        DataPoints = new();
        CreatedAt = DateTime.UtcNow;
        CreatedAtTicks = Stopwatch.GetTimestamp();
        IsComplete = false;
    }

    /// <summary>
    /// Gets the window duration in milliseconds.
    /// </summary>
    public long GetDurationMs() => WindowEndMs - WindowStartMs;

    /// <summary>
    /// Gets the count of data points in this window.
    /// </summary>
    public int GetDataPointCount() => DataPoints.Count;

    /// <summary>
    /// Adds a data point if it falls within the window boundaries.
    /// </summary>
    public bool TryAddDataPoint(DataPoint dataPoint)
    {
        if (dataPoint is null) throw new ArgumentNullException(nameof(dataPoint));
        if (dataPoint.Timestamp < WindowStartMs || dataPoint.Timestamp > WindowEndMs)
            return false;

        DataPoints.Add(dataPoint);
        return true;
    }

    /// <summary>
    /// Calculates the average value across all data points in the window.
    /// </summary>
    public double CalculateAverage()
    {
        return DataPoints.Count == 0 ? 0d : DataPoints.Average(dp => dp.Value);
    }

    /// <summary>
    /// Calculates the sum of all values in the window.
    /// </summary>
    public double CalculateSum()
    {
        return DataPoints.Sum(dp => dp.Value);
    }

    /// <summary>
    /// Calculates the minimum value in the window.
    /// </summary>
    public double CalculateMin()
    {
        return DataPoints.Count == 0 ? 0d : DataPoints.Min(dp => dp.Value);
    }

    /// <summary>
    /// Calculates the maximum value in the window.
    /// </summary>
    public double CalculateMax()
    {
        return DataPoints.Count == 0 ? 0d : DataPoints.Max(dp => dp.Value);
    }

    /// <summary>
    /// Calculates standard deviation of values in the window.
    /// </summary>
    public double CalculateStandardDeviation()
    {
        if (DataPoints.Count <= 1) return 0d;

        double avg = CalculateAverage();
        double sumOfSquares = DataPoints.Sum(dp => Math.Pow(dp.Value - avg, 2));
        return Math.Sqrt(sumOfSquares / DataPoints.Count);
    }

    /// <summary>
    /// Marks the window as complete and ready for output.
    /// </summary>
    public void MarkComplete()
    {
        IsComplete = true;
    }

    /// <summary>
    /// Gets window metadata for reporting.
    /// </summary>
    public Dictionary<string, object> GetMetadata()
    {
        return new()
        {
            { "WindowId", WindowId },
            { "StartMs", WindowStartMs },
            { "EndMs", WindowEndMs },
            { "DurationMs", GetDurationMs() },
            { "DataPointCount", GetDataPointCount() },
            { "AggregationType", AggregationType },
            { "IsComplete", IsComplete },
            { "Average", CalculateAverage() },
            { "Sum", CalculateSum() },
            { "Min", CalculateMin() },
            { "Max", CalculateMax() }
        };
    }
}
