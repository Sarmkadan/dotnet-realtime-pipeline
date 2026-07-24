#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Constants;
using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Exceptions;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Service for windowing operations on data streams.
/// Manages time-based window creation, aggregation, and emission.
/// </summary>
public sealed class WindowingService
{
    private readonly PipelineConfig _config;
    private long _nextWindowId = 1;
    private readonly Dictionary<long, WindowEvent> _activeWindows = new();

    public WindowingService(PipelineConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Creates a new window based on configuration parameters.
    /// </summary>
    /// <param name="windowStartMs">The start time of the window in milliseconds.</param>
    /// <returns>A new <see cref="WindowEvent"/> object.</returns>
    public WindowEvent CreateWindow(long windowStartMs)
    {
        long windowEndMs = windowStartMs + _config.WindowSizeMs;

        return new WindowEvent(
            _nextWindowId++,
            windowStartMs,
            windowEndMs,
            _config.WindowType
        );
    }

    /// <summary>
    /// Attempts to add a data point to the appropriate window.
    /// </summary>
    /// <param name="dataPoint">The <see cref="DataPoint"/> to add.</param>
    /// <param name="window">The <see cref="WindowEvent"/> to add the data point to.</param>
    /// <returns>True if the data point was added successfully, false otherwise.</returns>
    public bool TryAddDataPointToWindow(DataPoint dataPoint, WindowEvent window)
    {
        if (dataPoint is null) throw new ArgumentNullException(nameof(dataPoint));
        if (window is null) throw new ArgumentNullException(nameof(window));

        return window.TryAddDataPoint(dataPoint);
    }

    /// <summary>
    /// Processes a list of data points, assigning them to appropriate windows
    /// and emitting any windows that become complete.
    /// </summary>
    /// <param name="dataPoints">The list of <see cref="DataPoint"/> to process.</param>
    /// <returns>A list of <see cref="WindowEmissionResult"/> for windows that became complete.</returns>
    public List<WindowEmissionResult> ProcessDataPoints(List<DataPoint> dataPoints)
    {
        if (dataPoints is null || dataPoints.Count == 0)
            return new();

        var emittedWindows = new List<WindowEmissionResult>();

        foreach (var dataPoint in dataPoints)
        {
            ProcessDataPointForWindows(dataPoint);
        }

        // Check for and emit any windows that have completed
        var completedWindowIds = new List<long>();
        foreach (var window in _activeWindows.Values.ToList()) // ToList() to avoid modifying collection during iteration
        {
            if (IsWindowComplete(window))
            {
                emittedWindows.Add(EmitWindow(window));
                completedWindowIds.Add(window.WindowId);
            }
        }

        foreach (var windowId in completedWindowIds)
        {
            _activeWindows.Remove(windowId);
        }

        return emittedWindows;
    }

    /// <summary>
    /// Processes a single data point, assigning it to relevant active windows.
    /// </summary>
    private void ProcessDataPointForWindows(DataPoint dataPoint)
    {
        // Unknown window types fall back to tumbling semantics.
        IEnumerable<long> windowStarts = string.Equals(_config.WindowType, "SLIDING", StringComparison.OrdinalIgnoreCase)
            ? GetApplicableSlidingWindowStarts(dataPoint.Timestamp)
            : [GetTumblingWindowStart(dataPoint.Timestamp)];

        foreach (var windowStartMs in windowStarts)
        {
            if (!_activeWindows.TryGetValue(windowStartMs, out var window))
            {
                window = CreateWindow(windowStartMs);
                _activeWindows[windowStartMs] = window;
            }

            window.TryAddDataPoint(dataPoint);
        }
    }

    /// <summary>
    /// Returns the tumbling window start that contains the supplied timestamp.
    /// </summary>
    private long GetTumblingWindowStart(long timestamp) =>
        (timestamp / _config.WindowSizeMs) * _config.WindowSizeMs;

    /// <summary>
    /// Calculates all relevant window start times for a given timestamp in a sliding window context.
    /// </summary>
    private IEnumerable<long> GetApplicableSlidingWindowStarts(long timestamp)
    {
        long windowSize = _config.WindowSizeMs;
        long slide = _config.WindowSlideMs;

        // Calculate the earliest possible window start that could contain this timestamp
        // A data point 'ts' can belong to windows that start between (ts - windowSize + 1) and ts (inclusive).
        // And the start time must be a multiple of 'slide'.
        long earliestPossibleStart = timestamp - windowSize + 1;

        // Round down to the nearest slide multiple
        long firstWindowStart = (earliestPossibleStart / slide) * slide;
        if (firstWindowStart < 0) firstWindowStart = 0; // Window starts cannot be negative

        for (long currentStart = firstWindowStart; currentStart <= timestamp; currentStart += slide)
        {
            // Ensure the window [currentStart, currentStart + windowSize) actually covers the timestamp
            if (currentStart + windowSize > timestamp)
            {
                yield return currentStart;
            }
        }
    }

    /// <summary>
    /// Performs aggregation on a window using the configured aggregation type.
    /// </summary>
    /// <param name="window">The <see cref="WindowEvent"/> to aggregate.</param>
    /// <returns>A dictionary containing aggregated window data.</returns>
    public Dictionary<string, object> AggregateWindow(WindowEvent window)
    {
        if (window is null) throw new ArgumentNullException(nameof(window));
        if (window.DataPoints.Count == 0)
            throw new WindowingException("Cannot aggregate window with no data points", window.WindowId);

        string aggregationType = _config.WindowType;

        return aggregationType.ToUpperInvariant() switch
        {
            "TUMBLING" => AggregateTumblingWindow(window),
            "SLIDING" => AggregateSlidingWindow(window),
            "SESSION" => AggregateSessionWindow(window),
            _ => AggregateDefaultWindow(window)
        };
    }

    /// <summary>
    /// Calculates statistics for all values in a window.
    /// </summary>
    /// <param name="window">The <see cref="WindowEvent"/> to calculate statistics for.</param>
    /// <returns>A <see cref="WindowStatistics"/> object.</returns>
    public WindowStatistics CalculateWindowStatistics(WindowEvent window)
    {
        if (window is null) throw new ArgumentNullException(nameof(window));
        if (window.DataPoints.Count == 0)
            return new WindowStatistics { WindowId = window.WindowId, DataPointCount = 0 };

        var stats = new WindowStatistics
        {
            WindowId = window.WindowId,
            DataPointCount = window.DataPoints.Count,
            Sum = window.CalculateSum(),
            Average = window.CalculateAverage(),
            Min = window.CalculateMin(),
            Max = window.CalculateMax(),
            StdDev = window.CalculateStandardDeviation(),
            WindowDurationMs = window.GetDurationMs(),
            Throughput = window.DataPoints.Count / (window.GetDurationMs() / 1000d)
        };

        return stats;
    }

    /// <summary>
    /// Determines if a window is complete and ready for output.
    /// Uses a monotonic clock source (<see cref="Stopwatch.GetTimestamp"/>) to guard against
    /// NTP clock corrections that can cause <see cref="DateTimeOffset.UtcNow"/> to step backward,
    /// which would otherwise emit duplicate events at window boundaries in containerised deployments.
    /// </summary>
    /// <param name="window">The <see cref="WindowEvent"/> to check.</param>
    /// <returns>True if the window is complete, false otherwise.</returns>
    public bool IsWindowComplete(WindowEvent window)
    {
        if (window is null) throw new ArgumentNullException(nameof(window));

        long elapsedTicks = Stopwatch.GetTimestamp() - window.CreatedAtTicks;
        long elapsedMs = (long)(elapsedTicks * 1000.0 / Stopwatch.Frequency);
        return elapsedMs >= window.GetDurationMs();
    }

    /// <summary>
    /// Emits (finalizes) a window and returns its aggregated results.
    /// </summary>
    /// <param name="window">The <see cref="WindowEvent"/> to emit.</param>
    /// <returns>A <see cref="WindowEmissionResult"/> object.</returns>
    public WindowEmissionResult EmitWindow(WindowEvent window)
    {
        if (window is null) throw new ArgumentNullException(nameof(window));

        window.MarkComplete();

        var result = new WindowEmissionResult
        {
            WindowId = window.WindowId,
            StartMs = window.WindowStartMs,
            EndMs = window.WindowEndMs,
            DataPointCount = window.GetDataPointCount(),
            Statistics = CalculateWindowStatistics(window),
            AggregatedData = AggregateWindow(window),
            EmittedAt = DateTime.UtcNow
        };

        return result;
    }

    /// <summary>
    /// Merges multiple windows (for session windows).
    /// </summary>
    /// <param name="windows">The list of <see cref="WindowEvent"/> to merge.</param>
    /// <returns>A new merged <see cref="WindowEvent"/>.</returns>
    public WindowEvent MergeWindows(List<WindowEvent> windows)
    {
        if (windows is null || windows.Count == 0)
            throw new ArgumentException("Must provide at least one window to merge");

        var merged = new WindowEvent(
            _nextWindowId++,
            windows.Min(w => w.WindowStartMs),
            windows.Max(w => w.WindowEndMs),
            "SESSION"
        );

        foreach (var window in windows)
        {
            if (window.DataPoints is not null)
            {
                foreach (var dataPoint in window.DataPoints)
                {
                    merged.TryAddDataPoint(dataPoint);
                }
            }
        }

        return merged;
    }

    /// <summary>
    /// Forcibly emits every currently active window regardless of whether it has
    /// reached its natural completion time. Used during a graceful shutdown / drain
    /// so that partial (in-flight) tumbling and sliding window state is not silently
    /// lost when the host stops.
    /// </summary>
    /// <returns>A list of <see cref="WindowEmissionResult"/> for every active window that was flushed.</returns>
    public List<WindowEmissionResult> FlushAllWindows()
    {
        var flushed = new List<WindowEmissionResult>();

        foreach (var window in _activeWindows.Values.ToList())
        {
            flushed.Add(EmitWindow(window));
        }

        _activeWindows.Clear();

        return flushed;
    }

    /// <summary>
    /// Gets a summary of active windows.
    /// </summary>
    /// <returns>A <see cref="WindowingSummary"/> object.</returns>
    public WindowingSummary GetWindowingSummary()
    {
        var summary = new WindowingSummary
        {
            TotalWindowsCreated = _nextWindowId - 1,
            ActiveWindowCount = _activeWindows.Count,
            ConfiguredWindowSizeMs = _config.WindowSizeMs,
            ConfiguredWindowSlideMs = _config.WindowSlideMs,
            ConfiguredWindowType = _config.WindowType
        };

        return summary;
    }

    // Private helper methods

    private Dictionary<string, object> AggregateTumblingWindow(WindowEvent window)
    {
        return new()
        {
            { "WindowType", "TUMBLING" },
            { "Sum", window.CalculateSum() },
            { "Average", window.CalculateAverage() },
            { "Min", window.CalculateMin() },
            { "Max", window.CalculateMax() },
            { "Count", window.GetDataPointCount() }
        };
    }

    private Dictionary<string, object> AggregateSlidingWindow(WindowEvent window)
    {
        return new()
        {
            { "WindowType", "SLIDING" },
            { "Average", window.CalculateAverage() },
            { "Trend", CalculateTrend(window.DataPoints) },
            { "Count", window.GetDataPointCount() }
        };
    }

    private Dictionary<string, object> AggregateSessionWindow(WindowEvent window)
    {
        var result = new Dictionary<string, object>
        {
            { "WindowType", "SESSION" },
            { "SessionStart", window.WindowStartMs },
            { "SessionEnd", window.WindowEndMs },
            { "SessionDurationMs", window.GetDurationMs() },
            { "EventsInSession", window.GetDataPointCount() }
        };
        return result;
    }

    private Dictionary<string, object> AggregateDefaultWindow(WindowEvent window)
    {
        return window.GetMetadata();
    }

    private double CalculateTrend(List<DataPoint> dataPoints)
    {
        if (dataPoints.Count < 2) return 0d;

        double firstAvg = dataPoints.Take(dataPoints.Count / 2).Average(p => p.Value);
        double lastAvg = dataPoints.Skip(dataPoints.Count / 2).Average(p => p.Value);

        return lastAvg - firstAvg;
    }
}

/// <summary>
/// Statistics calculated for a window.
/// </summary>
public sealed class WindowStatistics
{
    public long WindowId { get; set; }
    public int DataPointCount { get; set; }
    public double Sum { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double StdDev { get; set; }
    public long WindowDurationMs { get; set; }
    public double Throughput { get; set; }
}

/// <summary>
/// Result of emitting a window.
/// </summary>
public sealed class WindowEmissionResult
{
    public long WindowId { get; set; }
    public long StartMs { get; set; }
    public long EndMs { get; set; }
    public int DataPointCount { get; set; }
    public WindowStatistics Statistics { get; set; }
    public Dictionary<string, object> AggregatedData { get; set; }
    public DateTime EmittedAt { get; set; }
}

/// <summary>
/// Summary of windowing state.
/// </summary>
public sealed class WindowingSummary
{
    public long TotalWindowsCreated { get; set; }
    public int ActiveWindowCount { get; set; }
    public long ConfiguredWindowSizeMs { get; set; }
    public long ConfiguredWindowSlideMs { get; set; }
    public string ConfiguredWindowType { get; set; }
}
