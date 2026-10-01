#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Aggregates <see cref="DataPoint"/> values over configurable time windows
/// (tumbling, sliding, or session) and produces <see cref="MetricAggregation"/> results.
/// </summary>
public sealed class WindowAggregator
{
    private readonly long _windowSizeMs;
    private readonly long _slideSizeMs;
    private readonly WindowType _windowType;
    private readonly ConcurrentDictionary<long, List<DataPoint>> _windows = new();
    private long _lastEmittedWindowEnd;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowAggregator"/> class.
    /// </summary>
    /// <param name="windowSizeMs">Size of each aggregation window in milliseconds.</param>
    /// <param name="slideSizeMs">
    /// Slide interval in milliseconds. For tumbling windows set equal to <paramref name="windowSizeMs"/>.
    /// </param>
    /// <param name="windowType">The type of windowing strategy.</param>
    public WindowAggregator(long windowSizeMs, long slideSizeMs, WindowType windowType = WindowType.Tumbling)
    {
        if (windowSizeMs <= 0) throw new ArgumentException("Window size must be > 0", nameof(windowSizeMs));
        if (slideSizeMs <= 0) throw new ArgumentException("Slide size must be > 0", nameof(slideSizeMs));

        _windowSizeMs = windowSizeMs;
        _slideSizeMs = slideSizeMs;
        _windowType = windowType;
    }

    /// <summary>Gets the number of active (not yet emitted) windows.</summary>
    public int ActiveWindowCount => _windows.Count;

    /// <summary>
    /// Adds a data point to all windows it belongs to.
    /// For tumbling windows each point belongs to exactly one window;
    /// for sliding windows a point may land in multiple overlapping windows.
    /// </summary>
    /// <param name="dataPoint">The data point to ingest.</param>
    /// <exception cref="ArgumentNullException"><paramref name="dataPoint"/> is <see langword="null"/>.</exception>
    public void Add(DataPoint dataPoint)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        if (_windowType == WindowType.Tumbling)
        {
            long windowStart = (dataPoint.Timestamp / _windowSizeMs) * _windowSizeMs;
            AddToWindow(windowStart, dataPoint);
        }
        else
        {
            // Sliding: data point belongs to every window whose [start, start+size) contains it
            long earliest = ((dataPoint.Timestamp - _windowSizeMs) / _slideSizeMs + 1) * _slideSizeMs;
            earliest = Math.Max(0, earliest);

            for (long ws = earliest; ws <= dataPoint.Timestamp; ws += _slideSizeMs)
            {
                if (dataPoint.Timestamp >= ws && dataPoint.Timestamp < ws + _windowSizeMs)
                    AddToWindow(ws, dataPoint);
            }
        }
    }

    /// <summary>
    /// Emits completed aggregation results for all windows that end before or at
    /// <paramref name="currentTimeMs"/>. Emitted windows are removed from the active set.
    /// </summary>
    /// <param name="currentTimeMs">The current wall-clock time in Unix milliseconds.</param>
    /// <param name="aggregationType">The aggregation function to apply.</param>
    /// <returns>A list of completed aggregations, ordered by window start time.</returns>
    public List<MetricAggregation> Emit(long currentTimeMs, AggregationType aggregationType = AggregationType.Average)
    {
        var results = new List<MetricAggregation>();
        var toRemove = new List<long>();

        foreach (var (windowStart, points) in _windows)
        {
            long windowEnd = windowStart + _windowSizeMs;
            if (windowEnd > currentTimeMs)
                continue;

            if (windowEnd <= _lastEmittedWindowEnd)
            {
                toRemove.Add(windowStart);
                continue;
            }

            var agg = Aggregate(windowStart, windowEnd, points, aggregationType);
            results.Add(agg);
            toRemove.Add(windowStart);
        }

        foreach (var key in toRemove)
            _windows.TryRemove(key, out _);

        if (results.Count > 0)
            _lastEmittedWindowEnd = results.Max(r => r.TimeWindowEndMs);

        results.Sort((a, b) => a.TimeWindowStartMs.CompareTo(b.TimeWindowStartMs));
        return results;
    }

    /// <summary>
    /// Computes the aggregate value for a set of data points without emitting or
    /// modifying window state. Useful for peeking at partial results.
    /// </summary>
    /// <param name="points">The data points to aggregate.</param>
    /// <param name="aggregationType">The aggregation function.</param>
    /// <returns>The computed aggregate value.</returns>
    public static double ComputeValue(IReadOnlyList<DataPoint> points, AggregationType aggregationType)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (points.Count == 0) return 0d;

        return aggregationType switch
        {
            AggregationType.Sum => points.Sum(p => p.Value),
            AggregationType.Average => points.Average(p => p.Value),
            AggregationType.Min => points.Min(p => p.Value),
            AggregationType.Max => points.Max(p => p.Value),
            AggregationType.Count => points.Count,
            AggregationType.StdDev => ComputeStdDev(points),
            _ => points.Sum(p => p.Value)
        };
    }

    /// <summary>
    /// Removes all active windows and resets the emitted watermark.
    /// </summary>
    public void Clear()
    {
        _windows.Clear();
        _lastEmittedWindowEnd = 0;
    }

    private void AddToWindow(long windowStart, DataPoint dataPoint)
    {
        _windows.AddOrUpdate(
            windowStart,
            _ => new List<DataPoint> { dataPoint },
            (_, list) => { lock (list) { list.Add(dataPoint); } return list; });
    }

    private static MetricAggregation Aggregate(
        long windowStart, long windowEnd,
        List<DataPoint> points,
        AggregationType aggregationType)
    {
        double value;
        lock (points)
        {
            value = ComputeValue(points, aggregationType);
        }

        return new MetricAggregation
        {
            TimeWindowStartMs = windowStart,
            TimeWindowEndMs = windowEnd,
            MetricType = aggregationType.ToString(),
            TotalItemsProcessed = points.Count,
        };
    }

    private static double ComputeStdDev(IReadOnlyList<DataPoint> points)
    {
        if (points.Count < 2) return 0d;
        double mean = points.Average(p => p.Value);
        double sumSq = points.Sum(p => (p.Value - mean) * (p.Value - mean));
        return Math.Sqrt(sumSq / (points.Count - 1));
    }
}
