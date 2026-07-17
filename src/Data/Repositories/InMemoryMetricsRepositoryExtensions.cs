#nullable enable

namespace DotNetRealtimePipeline.Data.Repositories;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="InMemoryMetricsRepository"/> providing additional query capabilities
/// and convenience methods for working with metric data.
/// </summary>
public static class InMemoryMetricsRepositoryExtensions
{
    /// <summary>
    /// Gets the most recent metric of a specific type.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="metricType">The metric type to filter by.</param>
    /// <returns>The most recent metric of the specified type, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when metricType is null or whitespace.</exception>
    public static async Task<MetricAggregation?> GetLatestByTypeAsync(this InMemoryMetricsRepository repository, string metricType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metricType);

        var metrics = await repository.GetByTypeAsync(metricType).ConfigureAwait(false);
        return metrics.FirstOrDefault();
    }

    /// <summary>
    /// Gets metrics filtered by type and time range.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="metricType">The metric type to filter by.</param>
    /// <param name="startMs">Start time in milliseconds.</param>
    /// <param name="endMs">End time in milliseconds.</param>
    /// <returns>Filtered metrics within the time range, ordered by time.</returns>
    /// <exception cref="ArgumentException">Thrown when metricType is null or whitespace, or startMs > endMs.</exception>
    public static async Task<IReadOnlyList<MetricAggregation>> GetByTypeAndTimeRangeAsync(
        this InMemoryMetricsRepository repository,
        string metricType,
        long startMs,
        long endMs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metricType);

        if (startMs > endMs)
            throw new ArgumentException("Start time must be <= end time", nameof(startMs));

        var allMetrics = await repository.GetByTypeAsync(metricType).ConfigureAwait(false);
        var filtered = allMetrics
            .Where(m => m.TimeWindowStartMs >= startMs && m.TimeWindowEndMs <= endMs)
            .OrderBy(m => m.TimeWindowStartMs)
            .ToList();

        return filtered.AsReadOnly();
    }

    /// <summary>
    /// Gets the average processing time for a specific metric type over the specified time range.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="metricType">The metric type to calculate average for.</param>
    /// <param name="startMs">Start time in milliseconds.</param>
    /// <param name="endMs">End time in milliseconds.</param>
    /// <returns>The average processing time in milliseconds, or null if no metrics found.</returns>
    /// <exception cref="ArgumentException">Thrown when metricType is null or whitespace, or startMs > endMs.</exception>
    public static async Task<double?> GetAverageProcessingTimeAsync(
        this InMemoryMetricsRepository repository,
        string metricType,
        long startMs,
        long endMs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metricType);

        if (startMs > endMs)
            throw new ArgumentException("Start time must be <= end time", nameof(startMs));

        var metrics = await repository.GetByTypeAndTimeRangeAsync(metricType, startMs, endMs).ConfigureAwait(false);

        return metrics.Count == 0
            ? null
            : metrics.Average(m => m.AverageProcessingTimeMs);
    }

    /// <summary>
    /// Gets the maximum processing time for a specific metric type over the specified time range.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="metricType">The metric type to find maximum for.</param>
    /// <param name="startMs">Start time in milliseconds.</param>
    /// <param name="endMs">End time in milliseconds.</param>
    /// <returns>The maximum processing time in milliseconds, or null if no metrics found.</returns>
    /// <exception cref="ArgumentException">Thrown when metricType is null or whitespace, or startMs > endMs.</exception>
    public static async Task<double?> GetMaxProcessingTimeAsync(
        this InMemoryMetricsRepository repository,
        string metricType,
        long startMs,
        long endMs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metricType);

        if (startMs > endMs)
            throw new ArgumentException("Start time must be <= end time", nameof(startMs));

        var metrics = await repository.GetByTypeAndTimeRangeAsync(metricType, startMs, endMs).ConfigureAwait(false);

        return metrics.Count == 0
            ? null
            : metrics.Max(m => m.MaxProcessingTimeMs);
    }

    /// <summary>
    /// Gets the minimum processing time for a specific metric type over the specified time range.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="metricType">The metric type to find minimum for.</param>
    /// <param name="startMs">Start time in milliseconds.</param>
    /// <param name="endMs">End time in milliseconds.</param>
    /// <returns>The minimum processing time in milliseconds, or null if no metrics found.</returns>
    /// <exception cref="ArgumentException">Thrown when metricType is null or whitespace, or startMs > endMs.</exception>
    public static async Task<double?> GetMinProcessingTimeAsync(
        this InMemoryMetricsRepository repository,
        string metricType,
        long startMs,
        long endMs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metricType);

        if (startMs > endMs)
            throw new ArgumentException("Start time must be <= end time", nameof(startMs));

        var metrics = await repository.GetByTypeAndTimeRangeAsync(metricType, startMs, endMs).ConfigureAwait(false);

        return metrics.Count == 0
            ? null
            : metrics.Min(m => m.MinProcessingTimeMs);
    }

    /// <summary>
    /// Gets metrics filtered by multiple types.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="metricTypes">The metric types to filter by.</param>
    /// <returns>Metrics of the specified types, ordered by time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when metricTypes is null.</exception>
    public static async Task<IReadOnlyList<MetricAggregation>> GetByTypesAsync(
        this InMemoryMetricsRepository repository,
        IEnumerable<string> metricTypes)
    {
        ArgumentNullException.ThrowIfNull(metricTypes);

        var types = metricTypes.ToList();
        if (types.Count == 0)
            return Array.Empty<MetricAggregation>();

        var allMetrics = new List<MetricAggregation>();
        foreach (var type in types)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(type);
            var metrics = await repository.GetByTypeAsync(type).ConfigureAwait(false);
            allMetrics.AddRange(metrics);
        }

        return allMetrics
            .OrderBy(m => m.TimeWindowStartMs)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets metrics filtered by type and time range, with processing time threshold filtering.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="metricType">The metric type to filter by.</param>
    /// <param name="startMs">Start time in milliseconds.</param>
    /// <param name="endMs">End time in milliseconds.</param>
    /// <param name="minProcessingTimeMs">Minimum processing time threshold in milliseconds (inclusive).</param>
    /// <param name="maxProcessingTimeMs">Maximum processing time threshold in milliseconds (inclusive).</param>
    /// <returns>Filtered metrics within processing time range, ordered by time.</returns>
    /// <exception cref="ArgumentException">Thrown when metricType is null or whitespace, startMs > endMs, or minProcessingTimeMs > maxProcessingTimeMs.</exception>
    public static async Task<IReadOnlyList<MetricAggregation>> GetByTypeAndTimeRangeWithProcessingTimeFilterAsync(
        this InMemoryMetricsRepository repository,
        string metricType,
        long startMs,
        long endMs,
        double minProcessingTimeMs,
        double maxProcessingTimeMs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metricType);

        if (startMs > endMs)
            throw new ArgumentException("Start time must be <= end time", nameof(startMs));

        if (minProcessingTimeMs > maxProcessingTimeMs)
            throw new ArgumentException("Minimum processing time must be <= maximum processing time", nameof(minProcessingTimeMs));

        var metrics = await repository.GetByTypeAndTimeRangeAsync(metricType, startMs, endMs).ConfigureAwait(false);

        var filtered = metrics
            .Where(m => m.AverageProcessingTimeMs >= minProcessingTimeMs && m.AverageProcessingTimeMs <= maxProcessingTimeMs)
            .OrderBy(m => m.TimeWindowStartMs)
            .ToList();

        return filtered.AsReadOnly();
    }

    /// <summary>
    /// Gets the last N metrics across all types, ordered by time.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="count">Number of metrics to retrieve.</param>
    /// <returns>Last N metrics across all types.</returns>
    /// <exception cref="ArgumentException">Thrown when count < 1.</exception>
    public static async Task<IReadOnlyList<MetricAggregation>> GetLastNMetricsAsync(
        this InMemoryMetricsRepository repository,
        int count)
    {
        if (count < 1)
            throw new ArgumentException("Count must be >= 1", nameof(count));

        var allMetrics = await repository.GetByTimeRangeAsync(
            long.MinValue,
            long.MaxValue).ConfigureAwait(false);

        return allMetrics
            .OrderByDescending(m => m.TimeWindowStartMs)
            .Take(count)
            .OrderBy(m => m.TimeWindowStartMs)
            .ToList()
            .AsReadOnly();
    }
}