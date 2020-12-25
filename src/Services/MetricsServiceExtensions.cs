#nullable enable

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="MetricsService"/> that provide additional metric analysis and reporting capabilities.
/// </summary>
public static class MetricsServiceExtensions
{
    /// <summary>
    /// Creates a metric aggregation with automatic window calculation based on current time.
    /// </summary>
    /// <param name="service">The metrics service instance.</param>
    /// <param name="itemsProcessed">Total items processed in the window.</param>
    /// <param name="itemsFailed">Total items that failed.</param>
    /// <param name="itemsSkipped">Total items that were skipped.</param>
    /// <param name="windowSeconds">Window size in seconds (default: 60).</param>
    /// <returns>A metric aggregation for the specified window.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static async Task<MetricAggregation> CreateMetricAggregationAsync(
        this MetricsService service,
        long itemsProcessed,
        long itemsFailed,
        long itemsSkipped,
        int windowSeconds = 60)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (windowSeconds <= 0)
        {
            throw new ArgumentException("Window must be positive", nameof(windowSeconds));
        }

        var windowStartMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - (windowSeconds * 1000);
        var windowEndMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return await service.CreateMetricAggregationAsync(
            windowStartMs,
            windowEndMs,
            itemsProcessed,
            itemsFailed,
            itemsSkipped);
    }

    /// <summary>
    /// Generates a formatted health report string suitable for logging or dashboard display.
    /// </summary>
    /// <param name="service">The metrics service instance.</param>
    /// <param name="includeDetails">Whether to include detailed metrics in the report.</param>
    /// <returns>A formatted health report string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static async Task<string> GenerateHealthReportStringAsync(
        this MetricsService service,
        bool includeDetails = true)
    {
        ArgumentNullException.ThrowIfNull(service);

        var report = await service.GenerateHealthReportAsync();

        var culture = CultureInfo.InvariantCulture;

        var reportLines = new List<string>
        {
            $"=== Pipeline Health Report ===",
            $"Status: {report.Status}",
            $"Message: {report.Message}",
            $"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}",
            $"Throughput: {report.ThroughputItemsPerSecond.ToString("F2", culture)} items/sec",
            $"Success Rate: {report.SuccessRatePercent.ToString("F2", culture)}%"
        };

        if (includeDetails)
        {
            reportLines.AddRange(new[]
            {
                $"Error Rate: {report.ErrorRatePercent.ToString("F2", culture)}%",
                $"Avg Processing: {report.AverageProcessingTimeMs.ToString("F2", culture)} ms",
                $"P95 Latency: {report.P95ProcessingTimeMs.ToString("F2", culture)} ms",
                $"P99 Latency: {report.P99ProcessingTimeMs.ToString("F2", culture)} ms",
                $"Backpressure: {report.BackpressurePercentage.ToString("F2", culture)}%",
                $"Total Processed: {report.TotalProcessed:N0}",
                $"Total Failed: {report.TotalFailed:N0}"
            });
        }

        return string.Join(Environment.NewLine, reportLines);
    }

    /// <summary>
    /// Gets the current processing time statistics as a dictionary for easy access.
    /// </summary>
    /// <param name="service">The metrics service instance.</param>
    /// <returns>A dictionary containing processing time statistics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static IReadOnlyDictionary<string, double> GetProcessingTimeStatistics(
        this MetricsService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        lock (service.GetProcessingTimesLock())
        {
            var stats = new Dictionary<string, double>
            {
                ["Count"] = service.GetProcessingTimesCount(),
                ["AverageMs"] = service.GetAverageProcessingTime(),
                ["MinimumMs"] = service.GetMinimumProcessingTime(),
                ["MaximumMs"] = service.GetMaximumProcessingTime(),
                ["P95Ms"] = service.GetP95ProcessingTime(),
                ["P99Ms"] = service.GetP99ProcessingTime()
            };

            return stats.AsReadOnly();
        }
    }

    /// <summary>
    /// Creates a performance trend analysis with customizable history depth.
    /// </summary>
    /// <param name="service">The metrics service instance.</param>
    /// <param name="historyCount">Number of historical metrics to analyze (default: 10).</param>
    /// <returns>A performance trend analysis.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static async Task<PerformanceTrend> AnalyzePerformanceTrendAsync(
        this MetricsService service,
        int historyCount = 10)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (historyCount < 2)
        {
            throw new ArgumentException("History count must be at least 2", nameof(historyCount));
        }

        return await service.AnalyzePerformanceTrendAsync(historyCount);
    }

    // Helper methods to access private members via reflection patterns
    private static object GetProcessingTimesLock(this MetricsService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var field = typeof(MetricsService).GetField(
            "_processingTimes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(service) is object lockObj)
        {
            return lockObj;
        }

        throw new InvalidOperationException("Could not access processing times collection");
    }

    private static int GetProcessingTimesCount(this MetricsService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var field = typeof(MetricsService).GetField(
            "_processingTimes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(service) is List<double> list)
        {
            lock (list)
            {
                return list.Count;
            }
        }

        return 0;
    }

    private static double GetAverageProcessingTime(this MetricsService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var field = typeof(MetricsService).GetField(
            "_processingTimes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(service) is List<double> list)
        {
            lock (list)
            {
                return list.Count > 0 ? list.Average() : 0d;
            }
        }

        return 0d;
    }

    private static double GetMinimumProcessingTime(this MetricsService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var field = typeof(MetricsService).GetField(
            "_processingTimes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(service) is List<double> list)
        {
            lock (list)
            {
                return list.Count > 0 ? list.Min() : 0d;
            }
        }

        return 0d;
    }

    private static double GetMaximumProcessingTime(this MetricsService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var field = typeof(MetricsService).GetField(
            "_processingTimes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(service) is List<double> list)
        {
            lock (list)
            {
                return list.Count > 0 ? list.Max() : 0d;
            }
        }

        return 0d;
    }

    private static double GetP95ProcessingTime(this MetricsService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var field = typeof(MetricsService).GetField(
            "_processingTimes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(service) is List<double> list)
        {
            lock (list)
            {
                if (list.Count == 0) return 0d;
                var sorted = list.OrderBy(x => x).ToList();
                int index = (int)Math.Ceiling(0.95 * sorted.Count) - 1;
                return sorted[Math.Max(0, Math.Min(index, sorted.Count - 1))];
            }
        }

        return 0d;
    }

    private static double GetP99ProcessingTime(this MetricsService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var field = typeof(MetricsService).GetField(
            "_processingTimes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(service) is List<double> list)
        {
            lock (list)
            {
                if (list.Count == 0) return 0d;
                var sorted = list.OrderBy(x => x).ToList();
                int index = (int)Math.Ceiling(0.99 * sorted.Count) - 1;
                return sorted[Math.Max(0, Math.Min(index, sorted.Count - 1))];
            }
        }

        return 0d;
    }

}