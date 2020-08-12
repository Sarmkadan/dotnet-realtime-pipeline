#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Implements sliding-window aggregation over a stream of <see cref="DataPoint"/> values.
/// A new aggregate is emitted every <see cref="StepIntervalMs"/> covering the last
/// <see cref="WindowSizeMs"/> of data, producing overlapping windows — suitable for
/// rolling averages, anomaly detection, and other continuous analysis patterns.
/// </summary>
/// <remarks>
/// Usage pattern (consistent with the existing <see cref="WindowingService"/> API):
/// <code>
/// var aggregator = new SlidingWindowAggregator(windowSizeMs: 10_000, stepIntervalMs: 2_000);
/// aggregator.Add(dataPoint);
/// foreach (var window in aggregator.FlushDueWindows())
///     Console.WriteLine(window.AggregatedData["Average"]);
/// </code>
/// </remarks>
public sealed class SlidingWindowAggregator
{
    private readonly long _windowSizeMs;
    private readonly long _stepIntervalMs;

    // Sorted list of all data points currently held in memory.
    private readonly List<DataPoint> _buffer = new();

    // Tracks the last step boundary at which windows were emitted.
    private long _lastEmitBoundaryMs = -1;

    private long _nextWindowId = 1;

    /// <summary>
    /// Size of each window in milliseconds.
    /// </summary>
    public long WindowSizeMs => _windowSizeMs;

    /// <summary>
    /// Interval between successive window emissions in milliseconds.
    /// </summary>
    public long StepIntervalMs => _stepIntervalMs;

    /// <param name="windowSizeMs">Duration of each sliding window (must be &gt; 0).</param>
    /// <param name="stepIntervalMs">
    /// How often a new window is emitted. Must be &gt; 0 and &lt;= <paramref name="windowSizeMs"/>.
    /// When equal to <paramref name="windowSizeMs"/> the behaviour matches a tumbling window.
    /// </param>
    public SlidingWindowAggregator(long windowSizeMs, long stepIntervalMs)
    {
        if (windowSizeMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(windowSizeMs), "Window size must be greater than zero.");
        if (stepIntervalMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(stepIntervalMs), "Step interval must be greater than zero.");
        if (stepIntervalMs > windowSizeMs)
            throw new ArgumentOutOfRangeException(nameof(stepIntervalMs),
                "Step interval cannot exceed window size; that would create gaps in coverage.");

        _windowSizeMs = windowSizeMs;
        _stepIntervalMs = stepIntervalMs;
    }

    /// <summary>
    /// Adds a data point to the internal buffer.
    /// </summary>
    public void Add(DataPoint dataPoint)
    {
        if (dataPoint is null) throw new ArgumentNullException(nameof(dataPoint));

        _buffer.Add(dataPoint);

        // Keep buffer sorted by timestamp to make range queries cheap.
        if (_buffer.Count > 1 && _buffer[^1].Timestamp < _buffer[^2].Timestamp)
        {
            _buffer.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        }
    }

    /// <summary>
    /// Adds a batch of data points to the internal buffer.
    /// </summary>
    public void AddRange(IEnumerable<DataPoint> dataPoints)
    {
        if (dataPoints is null) throw new ArgumentNullException(nameof(dataPoints));
        foreach (var dp in dataPoints) Add(dp);
    }

    /// <summary>
    /// Emits all windows whose step boundary has been reached but not yet emitted,
    /// given that <paramref name="currentTimeMs"/> is the current wall-clock time.
    /// Expired data points (older than the oldest possible window start) are pruned
    /// from the buffer automatically.
    /// </summary>
    /// <param name="currentTimeMs">
    /// Current time as a Unix millisecond timestamp (e.g.
    /// <c>DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()</c>).
    /// </param>
    /// <returns>One <see cref="SlidingWindowResult"/> per step boundary crossed.</returns>
    public IReadOnlyList<SlidingWindowResult> FlushDueWindows(long currentTimeMs)
    {
        if (_buffer.Count == 0)
            return Array.Empty<SlidingWindowResult>();

        long firstDataMs = _buffer[0].Timestamp;

        // Align boundaries to the step interval grid.
        long gridStart = (firstDataMs / _stepIntervalMs) * _stepIntervalMs;
        if (_lastEmitBoundaryMs < 0)
            _lastEmitBoundaryMs = gridStart - _stepIntervalMs;

        var results = new List<SlidingWindowResult>();

        for (long boundary = _lastEmitBoundaryMs + _stepIntervalMs;
             boundary <= currentTimeMs;
             boundary += _stepIntervalMs)
        {
            long windowStart = boundary - _windowSizeMs;
            long windowEnd   = boundary;

            var points = _buffer
                .Where(dp => dp.Timestamp >= windowStart && dp.Timestamp < windowEnd)
                .ToList();

            results.Add(BuildResult(windowStart, windowEnd, points));
            _lastEmitBoundaryMs = boundary;
        }

        // Prune data older than the earliest window that could still be emitted.
        long cutoff = currentTimeMs - _windowSizeMs;
        _buffer.RemoveAll(dp => dp.Timestamp < cutoff);

        return results;
    }

    /// <summary>
    /// Overload that uses the current UTC clock as <c>currentTimeMs</c>.
    /// </summary>
    public IReadOnlyList<SlidingWindowResult> FlushDueWindows()
        => FlushDueWindows(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

    // Builds an aggregated result for a single window.
    private SlidingWindowResult BuildResult(long startMs, long endMs, List<DataPoint> points)
    {
        double avg   = points.Count > 0 ? points.Average(p => p.Value) : 0d;
        double sum   = points.Count > 0 ? points.Sum(p => p.Value)     : 0d;
        double min   = points.Count > 0 ? points.Min(p => p.Value)     : 0d;
        double max   = points.Count > 0 ? points.Max(p => p.Value)     : 0d;
        double trend = CalculateTrend(points);

        return new SlidingWindowResult
        {
            WindowId        = _nextWindowId++,
            WindowStartMs   = startMs,
            WindowEndMs     = endMs,
            WindowSizeMs    = _windowSizeMs,
            StepIntervalMs  = _stepIntervalMs,
            DataPointCount  = points.Count,
            Average         = avg,
            Sum             = sum,
            Min             = min,
            Max             = max,
            Trend           = trend,
            EmittedAt       = DateTime.UtcNow,
            AggregatedData  = new Dictionary<string, object>
            {
                { "WindowType",    "SLIDING"      },
                { "Average",       avg            },
                { "Sum",           sum            },
                { "Min",           min            },
                { "Max",           max            },
                { "Count",         points.Count   },
                { "Trend",         trend          },
                { "WindowSizeMs",  _windowSizeMs  },
                { "StepIntervalMs",_stepIntervalMs}
            }
        };
    }

    private static double CalculateTrend(List<DataPoint> points)
    {
        if (points.Count < 2) return 0d;
        double firstHalf = points.Take(points.Count / 2).Average(p => p.Value);
        double lastHalf  = points.Skip(points.Count / 2).Average(p => p.Value);
        return lastHalf - firstHalf;
    }
}

/// <summary>
/// The aggregated output produced for a single sliding window step.
/// </summary>
public sealed class SlidingWindowResult
{
    public long WindowId { get; set; }
    public long WindowStartMs { get; set; }
    public long WindowEndMs { get; set; }
    public long WindowSizeMs { get; set; }
    public long StepIntervalMs { get; set; }
    public int DataPointCount { get; set; }
    public double Average { get; set; }
    public double Sum { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }

    /// <summary>
    /// Difference between the average of the second half and the first half of data
    /// in the window — a positive value indicates an upward trend.
    /// </summary>
    public double Trend { get; set; }

    public DateTime EmittedAt { get; set; }
    public Dictionary<string, object> AggregatedData { get; set; } = new();
}
