#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Data.Repositories;

using DotNetRealtimePipeline.Constants;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// In-memory implementation of the metrics repository.
/// Maintains a rolling history of metrics for analysis.
/// </summary>
public sealed class InMemoryMetricsRepository : IMetricsRepository
{
    private readonly List<MetricAggregation> _metrics = new();
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryMetricsRepository"/> class.
    /// </summary>
    public InMemoryMetricsRepository()
    {
    }

    /// <inheritdoc/>
    public Task<MetricAggregation?> GetByIdAsync(long metricId)
    {
        lock (_lockObject)
        {
            var metric = _metrics.FirstOrDefault(m => m.MetricId == metricId);
            return Task.FromResult(metric);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="startMs"/> is greater than <paramref name="endMs"/>.
    /// </exception>
    public Task<List<MetricAggregation>> GetByTimeRangeAsync(long startMs, long endMs)
    {
        if (startMs > endMs)
            throw new ArgumentException("Start time must be <= end time");

        lock (_lockObject)
        {
            var results = _metrics
                .Where(m => m.TimeWindowStartMs >= startMs && m.TimeWindowEndMs <= endMs)
                .OrderBy(m => m.TimeWindowStartMs)
                .ToList();

            return Task.FromResult(results);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="metricType"/> is <see langword="null"/>, empty, or consists only of white-space characters.
    /// </exception>
    public Task<List<MetricAggregation>> GetByTypeAsync(string metricType)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type cannot be null", nameof(metricType));

        lock (_lockObject)
        {
            var results = _metrics
                .Where(m => m.MetricType == metricType)
                .OrderByDescending(m => m.TimeWindowStartMs)
                .ToList();

            return Task.FromResult(results);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="metric"/> is <see langword="null"/>.
    /// </exception>
    public Task<MetricAggregation> SaveAsync(MetricAggregation metric)
    {
        if (metric is null) throw new ArgumentNullException(nameof(metric));

        lock (_lockObject)
        {
            var existing = _metrics.FirstOrDefault(m => m.MetricId == metric.MetricId);
            if (existing is not null)
            {
                _metrics.Remove(existing);
            }

            _metrics.Add(metric);

            // Maintain rolling history size
            if (_metrics.Count > PipelineConstants.MaxMetricHistorySize)
            {
                _metrics.RemoveAt(0);
            }

            return Task.FromResult(metric);
        }
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(long metricId)
    {
        lock (_lockObject)
        {
            var metric = _metrics.FirstOrDefault(m => m.MetricId == metricId);
            if (metric is null) return Task.FromResult(false);

            _metrics.Remove(metric);
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">No metrics are available.</exception>
    public Task<MetricAggregation> GetLatestAsync()
    {
        lock (_lockObject)
        {
            var latest = _metrics.LastOrDefault();
            if (latest is null)
                throw new InvalidOperationException("No metrics available");

            return Task.FromResult(latest);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="count"/> is less than one.
    /// </exception>
    public Task<List<MetricAggregation>> GetHistoryAsync(int count)
    {
        if (count < 1) throw new ArgumentException("Count must be >= 1", nameof(count));

        lock (_lockObject)
        {
            var results = _metrics
                .OrderByDescending(m => m.TimeWindowStartMs)
                .Take(count)
                .ToList();

            return Task.FromResult(results);
        }
    }

    /// <summary>
    /// Clears all metrics (useful for testing).
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            _metrics.Clear();
        }
    }

    /// <summary>
    /// Gets the internal metrics list for serialization purposes.
    /// This method is intended for use by InMemoryMetricsRepositoryJsonExtensions only.
    /// </summary>
    /// <returns>The internal metrics list.</returns>
    internal List<MetricAggregation> GetInternalMetrics()
    {
        lock (_lockObject)
        {
            return new List<MetricAggregation>(_metrics);
        }
    }
}
