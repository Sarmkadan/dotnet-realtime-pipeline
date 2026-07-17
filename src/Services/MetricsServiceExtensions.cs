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
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="windowSeconds"/> is not positive.</exception>
    public static async Task<MetricAggregation> CreateMetricAggregationAsync(
        this MetricsService service,
        long itemsProcessed,
        long itemsFailed,
        long itemsSkipped,
        int windowSeconds = 60)
    {
        ArgumentNullException.ThrowIfNull(service);

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(windowSeconds, 0);

        var windowStartMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - (windowSeconds * 1000L);
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

        var processingTimes = GetProcessingTimesList(service);
        lock (processingTimes)
        {
            var stats = new Dictionary<string, double>
            {
                ["Count"] = processingTimes.Count,
                ["AverageMs"] = processingTimes.Count > 0 ? processingTimes.Average() : 0d,
                ["MinimumMs"] = processingTimes.Count > 0 ? processingTimes.Min() : 0d,
                ["MaximumMs"] = processingTimes.Count > 0 ? processingTimes.Max() : 0d,
                ["P95Ms"] = CalculatePercentile(processingTimes, 95),
                ["P99Ms"] = CalculatePercentile(processingTimes, 99)
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
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="historyCount"/> is less than 2.</exception>
    public static async Task<PerformanceTrend> AnalyzePerformanceTrendAsync(
        this MetricsService service,
        int historyCount = 10)
    {
        ArgumentNullException.ThrowIfNull(service);

        ArgumentOutOfRangeException.ThrowIfLessThan(historyCount, 2);

        return await service.AnalyzePerformanceTrendAsync(historyCount);
    }

    /// <summary>
    /// Gets the processing times list from the metrics service using reflection.
    /// </summary>
    /// <param name="service">The metrics service instance.</param>
    /// <returns>The processing times list.</returns>
    private static List<double> GetProcessingTimesList(MetricsService service)
    {
        var field = typeof(MetricsService).GetField(
            "_processingTimes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field?.GetValue(service) is List<double> list)
        {
            return list;
        }

        throw new InvalidOperationException("Could not access processing times collection");
    }

    /// <summary>
    /// Calculates a percentile value from a list of values.
    /// </summary>
    /// <param name="values">The list of values.</param>
    /// <param name="percentile">The percentile to calculate (0-100).</param>
    /// <returns>The percentile value.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="percentile"/> is not between 0 and 100.</exception>
    private static double CalculatePercentile(IReadOnlyList<double> values, int percentile)
    {
        if (values.Count == 0)
            return 0d;

        if (percentile is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percentile), "Percentile must be between 0 and 100");

        var sorted = values.OrderBy(x => x).ToList();
        int index = (int)Math.Ceiling((percentile / 100.0) * sorted.Count) - 1;
        index = Math.Max(0, Math.Min(index, sorted.Count - 1));

        return sorted[index];
    }
}