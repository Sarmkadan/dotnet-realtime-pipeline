#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="IMetricsExporter"/>.
/// </summary>
public static class MetricsExporterExtensions
{
    /// <summary>
    /// Exports a batch of metrics by materializing the enumerable to a list and delegating to <see cref="IMetricsExporter.ExportBatchAsync(List{MetricAggregation})"/>.
    /// </summary>
    /// <param name="exporter">The metrics exporter.</param>
    /// <param name="metrics">The metrics to export.</param>
    /// <returns>A task representing the export operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exporter"/> or <paramref name="metrics"/> is null.</exception>
    public static Task ExportBatchAsync(this IMetricsExporter exporter, IEnumerable<MetricAggregation> metrics)
    {
        ArgumentNullException.ThrowIfNull(exporter);
        ArgumentNullException.ThrowIfNull(metrics);
        return exporter.ExportBatchAsync(metrics.ToList());
    }

    /// <summary>
    /// Attempts to export a single metric aggregation, swallowing any exceptions and logging them if a logger is provided.
    /// </summary>
    /// <param name="exporter">The metrics exporter.</param>
    /// <param name="metric">The metric aggregation to export.</param>
    /// <param name="logger">Optional logger to use for exception logging.</param>
    /// <returns>True if the export succeeded; false if an exception occurred.</returns>
    public static async Task<bool> TryExportAsync(this IMetricsExporter exporter, MetricAggregation metric, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(exporter);
        ArgumentNullException.ThrowIfNull(metric);

        try
        {
            await exporter.ExportAsync(metric);
            return true;
        }
        catch (Exception ex)
        {
            if (logger != null)
            {
                logger.LogError(ex, "Failed to export metric");
            }
            return false;
        }
    }

    /// <summary>
    /// Creates a metrics exporter that tries the primary exporter first, falling back to the fallback exporter if the primary fails.
    /// </summary>
    /// <param name="primary">The primary metrics exporter.</param>
    /// <param name="fallback">The fallback metrics exporter.</param>
    /// <returns>A metrics exporter that attempts primary then fallback.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="primary"/> or <paramref name="fallback"/> is null.</exception>
    public static IMetricsExporter WithFallback(this IMetricsExporter primary, IMetricsExporter fallback)
    {
        ArgumentNullException.ThrowIfNull(primary);
        ArgumentNullException.ThrowIfNull(fallback);
        return new FallbackMetricsExporter(primary, fallback);
    }

    private sealed class FallbackMetricsExporter : IMetricsExporter
    {
        private readonly IMetricsExporter _primary;
        private readonly IMetricsExporter _fallback;

        public FallbackMetricsExporter(IMetricsExporter primary, IMetricsExporter fallback)
        {
            _primary = primary;
            _fallback = fallback;
        }

        public Task ExportAsync(MetricAggregation metrics)
        {
            ArgumentNullException.ThrowIfNull(metrics);
            return _primary.ExportAsync(metrics).CatchAsync(_fallback.ExportAsync(metrics));
        }

        public Task ExportBatchAsync(List<MetricAggregation> metrics)
        {
            ArgumentNullException.ThrowIfNull(metrics);
            return _primary.ExportBatchAsync(metrics).CatchAsync(_fallback.ExportBatchAsync(metrics));
        }
    }
}

/// <summary>
/// Provides extension methods for <see cref="Task"/> to support fallback behavior.
/// </summary>
internal static class TaskExtensions
{
    /// <summary>
    /// Returns a task that falls back to the fallback task if the source task fails.
    /// </summary>
    /// <param name="source">The source task.</param>
    /// <param name="fallback">The fallback task to execute if the source task fails.</param>
    /// <returns>A task that completes when either the source succeeds or the fallback completes.</returns>
    public static Task CatchAsync(this Task source, Task fallback)
    {
        return source.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                return fallback;
            }
            return Task.CompletedTask;
        }).Unwrap();
    }

    /// <summary>
    /// Returns a task that falls back to the fallback task if the source task fails.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    /// <param name="source">The source task.</param>
    /// <param name="fallback">The fallback task to execute if the source task fails.</param>
    /// <returns>A task that completes with the result of the source if it succeeds, or the result of the fallback if the source fails.</returns>
    public static Task<T> CatchAsync<T>(this Task<T> source, Task<T> fallback)
    {
        return source.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                return fallback.Result;
            }
            return t.Result;
        });
    }
}