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
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Interface for metrics exporters.
/// </summary>
public interface IMetricsExporter
{
    Task ExportAsync(MetricAggregation metrics);
    Task ExportBatchAsync(List<MetricAggregation> metrics);
}

/// <summary>
/// Prometheus format metrics exporter.
/// </summary>
public class PrometheusMetricsExporter : IMetricsExporter
{
    private readonly ILogger<PrometheusMetricsExporter> _logger;

    public PrometheusMetricsExporter(ILogger<PrometheusMetricsExporter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Exports a single metric aggregation in Prometheus format.
    /// </summary>
    public async Task ExportAsync(MetricAggregation metrics)
    {
        var lines = new List<string>();

        var timestamp = new DateTimeOffset(metrics.ComputedAt).ToUnixTimeMilliseconds();

        lines.Add($"pipeline_average_processing_time_ms {metrics.AverageProcessingTimeMs} {timestamp}");
        lines.Add($"pipeline_total_items_processed_total {metrics.TotalItemsProcessed} {timestamp}");
        lines.Add($"pipeline_failed_items_total {metrics.TotalItemsFailed} {timestamp}");
        lines.Add($"pipeline_skipped_items_total {metrics.TotalItemsSkipped} {timestamp}");
        lines.Add($"pipeline_backpressure_events_total {metrics.BackpressureEvents} {timestamp}");
        lines.Add($"pipeline_total_backpressure_ms {metrics.TotalBackpressureMs} {timestamp}");

        _logger.LogDebug("Prometheus metrics exported: {LineCount} metrics", lines.Count);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Exports a batch of metrics.
    /// </summary>
    public async Task ExportBatchAsync(List<MetricAggregation> metrics)
    {
        var tasks = metrics.Select(m => ExportAsync(m));
        await Task.WhenAll(tasks);
    }
}

/// <summary>
/// HTTP-based metrics exporter for pushing metrics to remote endpoints.
/// </summary>
public class HttpMetricsExporter : IMetricsExporter
{
    private readonly string _endpoint;
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpMetricsExporter> _logger;

    public HttpMetricsExporter(string endpoint, HttpClient httpClient, ILogger<HttpMetricsExporter> logger)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Exports metrics to an HTTP endpoint.
    /// </summary>
    public async Task ExportAsync(MetricAggregation metrics)
    {
        try
        {
            var json = JsonSerializer.Serialize(metrics);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Metrics exported successfully to {Endpoint}", _endpoint);
            }
            else
            {
                _logger.LogWarning("Failed to export metrics: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting metrics to {Endpoint}", _endpoint);
        }
    }

    /// <summary>
    /// Exports a batch of metrics.
    /// </summary>
    public async Task ExportBatchAsync(List<MetricAggregation> metrics)
    {
        try
        {
            var json = JsonSerializer.Serialize(metrics);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Batch of {Count} metrics exported successfully", metrics.Count);
            }
            else
            {
                _logger.LogWarning("Failed to export metrics batch: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting metrics batch");
        }
    }
}

/// <summary>
/// Composite metrics exporter supporting multiple export targets.
/// </summary>
public class CompositeMetricsExporter : IMetricsExporter
{
    private readonly List<IMetricsExporter> _exporters = new();
    private readonly ILogger<CompositeMetricsExporter> _logger;

    public CompositeMetricsExporter(ILogger<CompositeMetricsExporter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Adds an exporter to the composite exporter.
    /// </summary>
    public void AddExporter(IMetricsExporter exporter)
    {
        _exporters.Add(exporter ?? throw new ArgumentNullException(nameof(exporter)));
    }

    /// <summary>
    /// Exports metrics to all registered exporters.
    /// </summary>
    public async Task ExportAsync(MetricAggregation metrics)
    {
        var tasks = _exporters.Select(e =>
        {
            return e.ExportAsync(metrics).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "Exporter failed");
                }
            });
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Exports a batch of metrics to all exporters.
    /// </summary>
    public async Task ExportBatchAsync(List<MetricAggregation> metrics)
    {
        var tasks = _exporters.Select(e =>
        {
            return e.ExportBatchAsync(metrics).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "Exporter failed");
                }
            });
        });

        await Task.WhenAll(tasks);
    }
}

/// <summary>
/// Factory for creating metrics exporters.
/// </summary>
public static class MetricsExporterFactory
{
    public static IMetricsExporter CreatePrometheus(ILogger<PrometheusMetricsExporter> logger)
    {
        return new PrometheusMetricsExporter(logger);
    }

    public static IMetricsExporter CreateHttp(string endpoint, HttpClient client, ILogger<HttpMetricsExporter> logger)
    {
        return new HttpMetricsExporter(endpoint, client, logger);
    }

    public static CompositeMetricsExporter CreateComposite(ILogger<CompositeMetricsExporter> logger)
    {
        return new CompositeMetricsExporter(logger);
    }
}
