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
    public bool TryAddDataPointToWindow(DataPoint dataPoint, WindowEvent window)
    {
        if (dataPoint is null) throw new ArgumentNullException(nameof(dataPoint));
        if (window is null) throw new ArgumentNullException(nameof(window));

        return window.TryAddDataPoint(dataPoint);
    }

    /// <summary>
    /// Gets or creates windows for a set of data points.
    /// </summary>
    public List<WindowEvent> AssignDataPointsToWindows(List<DataPoint> dataPoints)
    {
        if (dataPoints is null || dataPoints.Count == 0)
            return new();

        var windows = new Dictionary<long, WindowEvent>();

        foreach (var dataPoint in dataPoints)
        {
            long windowStartMs = (dataPoint.Timestamp / _config.WindowSizeMs) * _config.WindowSizeMs;

            if (!windows.TryGetValue(windowStartMs, out var window))
            {
                window = CreateWindow(windowStartMs);
                windows[windowStartMs] = window;
            }

            window.TryAddDataPoint(dataPoint);
        }

        return windows.Values.ToList();
    }

    /// <summary>
    /// Performs aggregation on a window using the configured aggregation type.
    /// </summary>
    public Dictionary<string, object> AggregateWindow(WindowEvent window)
    {
        if (window is null) throw new ArgumentNullException(nameof(window));
        if (window.DataPoints.Count == 0)
            throw new WindowingException("Cannot aggregate window with no data points", window.WindowId);

        string aggregationType = _config.WindowType;

        return aggregationType.ToUpper() switch
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
    /// </summary>
    public bool IsWindowComplete(WindowEvent window)
    {
        if (window is null) throw new ArgumentNullException(nameof(window));

        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return now > window.WindowEndMs;
    }

    /// <summary>
    /// Emits (finalizes) a window and returns its aggregated results.
    /// </summary>
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
    /// Gets a summary of active windows.
    /// </summary>
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
