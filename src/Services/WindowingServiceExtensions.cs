#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
/// Extension methods for <see cref="WindowingService"/> that provide additional windowing operations
/// and convenience methods for working with windows and data points.
/// </summary>
public static class WindowingServiceExtensions
{
    /// <summary>
    /// Creates a new window with a specified custom duration that differs from the configured window size.
    /// Useful for creating ad-hoc windows for special processing or debugging.
    /// </summary>
    /// <param name="service">The <see cref="WindowingService"/> instance.</param>
    /// <param name="windowStartMs">The start time of the window in milliseconds.</param>
    /// <param name="customDurationMs">The custom duration for this window in milliseconds.</param>
    /// <returns>A new <see cref="WindowEvent"/> with the specified custom duration.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static WindowEvent CreateCustomDurationWindow(this WindowingService service, long windowStartMs, long customDurationMs)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (customDurationMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(customDurationMs), "Custom duration must be positive");
        }

        return new WindowEvent(
            service.GetNextWindowId(),
            windowStartMs,
            windowStartMs + customDurationMs,
            "CUSTOM"
        );
    }

    /// <summary>
    /// Processes data points and immediately emits any windows that are complete,
    /// returning both the emitted results and the remaining active windows.
    /// </summary>
    /// <param name="service">The <see cref="WindowingService"/> instance.</param>
    /// <param name="dataPoints">The list of <see cref="DataPoint"/> to process.</param>
    /// <returns>A tuple containing the emitted window results and the remaining active windows.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="dataPoints"/> is null.</exception>
    public static (IReadOnlyList<WindowEmissionResult> Emitted, IReadOnlyList<WindowEvent> Active) ProcessDataPointsWithState(this WindowingService service, List<DataPoint> dataPoints)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(dataPoints);

        var emitted = service.ProcessDataPoints(dataPoints);

        // Get active windows (excluding the ones we just emitted)
        var activeWindows = service.GetActiveWindows().ToList();

        return (emitted.AsReadOnly(), activeWindows.AsReadOnly());
    }

    /// <summary>
    /// Calculates statistics for multiple windows and returns a combined summary.
    /// Useful for getting an overview of window statistics across multiple windows.
    /// </summary>
    /// <param name="service">The <see cref="WindowingService"/> instance.</param>
    /// <param name="windows">The collection of <see cref="WindowEvent"/> to calculate statistics for.</param>
    /// <returns>A <see cref="WindowStatistics"/> object containing aggregated statistics across all windows.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="windows"/> is null.</exception>
    public static WindowStatistics CalculateCombinedWindowStatistics(this WindowingService service, IEnumerable<WindowEvent> windows)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(windows);

        var windowList = windows.ToList();

        if (windowList.Count == 0)
        {
            return new WindowStatistics { WindowId = -1, DataPointCount = 0 };
        }

        return new WindowStatistics
        {
            WindowId = -1, // Combined statistics don't have a single window ID
            DataPointCount = windowList.Sum(w => w.DataPoints.Count),
            Sum = windowList.Sum(w => w.CalculateSum()),
            Average = windowList.Average(w => w.CalculateAverage()),
            Min = windowList.Min(w => w.CalculateMin()),
            Max = windowList.Max(w => w.CalculateMax()),
            StdDev = CalculateCombinedStandardDeviation(windowList),
            WindowDurationMs = windowList.Max(w => w.GetDurationMs()),
            Throughput = windowList.Sum(w => w.DataPoints.Count) / (windowList.Max(w => w.GetDurationMs()) / 1000d)
        };
    }

    /// <summary>
    /// Filters and returns only the complete windows from the active windows collection.
    /// </summary>
    /// <param name="service">The <see cref="WindowingService"/> instance.</param>
    /// <returns>An enumerable of complete <see cref="WindowEvent"/> objects.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static IEnumerable<WindowEvent> GetCompleteWindows(this WindowingService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetActiveWindows().Where(service.IsWindowComplete);
    }

    /// <summary>
    /// Gets all currently active windows managed by the service.
    /// </summary>
    /// <param name="service">The <see cref="WindowingService"/> instance.</param>
    /// <returns>An enumerable of active <see cref="WindowEvent"/> objects.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static IReadOnlyList<WindowEvent> GetActiveWindows(this WindowingService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        // Use reflection to access the private _activeWindows field
        // This is necessary since the field is private in the original class
        var field = typeof(WindowingService).GetField("_activeWindows",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(service) is Dictionary<long, WindowEvent> activeWindows)
        {
            return activeWindows.Values.ToList().AsReadOnly();
        }

        return Array.Empty<WindowEvent>();
    }

    /// <summary>
    /// Gets the next window ID that would be assigned by the service.
    /// </summary>
    /// <param name="service">The <see cref="WindowingService"/> instance.</param>
    /// <returns>The next window ID that would be assigned.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static long GetNextWindowId(this WindowingService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var field = typeof(WindowingService).GetField("_nextWindowId",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return field != null ? Convert.ToInt64(field.GetValue(service)) : 1;
    }

    /// <summary>
    /// Calculates the combined standard deviation across multiple windows.
    /// </summary>
    private static double CalculateCombinedStandardDeviation(List<WindowEvent> windows)
    {
        if (windows.Count == 0)
        {
            return 0d;
        }

        // Calculate combined mean
        double totalSum = windows.Sum(w => w.CalculateSum());
        int totalCount = windows.Sum(w => w.DataPoints.Count);
        double combinedMean = totalSum / totalCount;

        // Calculate sum of squared differences from mean
        double sumOfSquaredDifferences = 0d;

        foreach (var window in windows)
        {
            foreach (var dataPoint in window.DataPoints)
            {
                double difference = dataPoint.Value - combinedMean;
                sumOfSquaredDifferences += difference * difference;
            }
        }

        // Calculate variance and standard deviation
        double variance = sumOfSquaredDifferences / totalCount;
        return Math.Sqrt(variance);
    }
}