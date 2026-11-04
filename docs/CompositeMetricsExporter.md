# CompositeMetricsExporter

> **Namespace:** `DotNetRealtimePipeline.Integration`  
> **Source:** `src/Integration/MetricsExporter.cs`

---

## Overview

The metrics exporter subsystem provides a pluggable way to emit pipeline metrics to different backends. It consists of:

- **`IMetricsExporter`** — the common interface.
- **`PrometheusMetricsExporter`** — emits Prometheus text-format lines.
- **`HttpMetricsExporter`** — POSTs JSON to an HTTP endpoint.
- **`CompositeMetricsExporter`** — fans out to multiple exporters, tolerating individual failures.
- **`MetricsExporterFactory`** — convenience factories for the above.

All exporters are registered as singletons via `AddPipelineServices` (see `ServiceCollectionExtensions.cs`).

---

## IMetricsExporter

```csharp
public interface IMetricsExporter
{
    Task ExportAsync(MetricAggregation metrics);
    Task ExportBatchAsync(List<MetricAggregation> metrics);
}
```

- `ExportAsync` — called periodically by `MetricsService` with the latest aggregation.
- `ExportBatchAsync` — called on shutdown or manual flush; default implementation in each exporter forwards to `ExportAsync` per item.

---

## PrometheusMetricsExporter

### Output format

Produces **Prometheus exposition format (text-based, one metric per line)** with a Unix-millisecond timestamp:

```
pipeline_average_processing_time_ms <value> <timestamp>
pipeline_total_items_processed_total <value> <timestamp>
pipeline_failed_items_total <value> <timestamp>
pipeline_skipped_items_total <value> <timestamp>
pipeline_backpressure_events_total <value> <timestamp>
pipeline_total_backpressure_ms <value> <timestamp>
```

- Metric names follow the `pipeline_<snake_case>` convention with `_total` suffix for counters.
- Timestamp = `metrics.ComputedAt` converted to milliseconds since epoch.
- No histogram / summary buckets — only the current gauge / counter values.

### Error handling

- Validates `metrics` argument (`ArgumentNullException`).
- Never throws on export; logs at `Debug` level (`Prometheus metrics exported: {LineCount} metrics`).
- `ExportBatchAsync` runs each item in parallel via `Task.WhenAll`; a failure in one item does not affect others (exceptions bubble up via `Task.WhenAll`).

### Usage

```csharp
var exporter = MetricsExporterFactory.CreatePrometheus(logger);
await exporter.ExportAsync(aggregation);
```

---

## HttpMetricsExporter

### Output format

Serializes `MetricAggregation` (or `List<MetricAggregation>`) to **JSON** using `System.Text.Json` defaults (camelCase property names) and POSTs as `application/json`.

Single export payload example:

```json
{
  "averageProcessingTimeMs": 12.34,
  "totalItemsProcessed": 10000,
  "totalItemsFailed": 5,
  "totalItemsSkipped": 2,
  "backpressureEvents": 3,
  "totalBackpressureMs": 450,
  "computedAt": "2026-11-04T12:34:56.789Z"
}
```

Batch export payload is a JSON array of the above objects.

### Error handling

- Validates `metrics` / `endpoint` / `HttpClient` arguments.
- **Swallows all exceptions** — logs at `Error` level and returns without throwing.
- Non-success HTTP status codes are logged at `Warning` level (`Failed to export metrics: {StatusCode}`).
- Success logged at `Debug` (single) or `Information` (batch).

### Configuration

Constructor parameters:

| Parameter | Description |
|-----------|-------------|
| `endpoint` | Full URL (e.g. `https://metrics.example.com/ingest`) |
| `httpClient` | Shared `HttpClient` (configure timeouts, headers, etc. externally) |
| `logger` | `ILogger<HttpMetricsExporter>` |

### Usage

```csharp
var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
var exporter = MetricsExporterFactory.CreateHttp("https://metrics.example.com/ingest", httpClient, logger);
await exporter.ExportAsync(aggregation);
```

---

## CompositeMetricsExporter

### Purpose

Aggregates multiple `IMetricsExporter` instances and fans out each export call to **all** of them. Designed for scenarios where you want to simultaneously:

- Scrape locally via Prometheus (`PrometheusMetricsExporter`).
- Push to a remote collector (`HttpMetricsExporter`).
- Add custom exporters (e.g., file, CloudWatch, Datadog).

### Error handling (key behavior)

- **Fail-soft**: each exporter runs independently; an exception in one **does not cancel** the others.
- Implementation: `ContinueWith` on each task, logging faults at `Error` level (`Exporter failed`).
- `Task.WhenAll` waits for all tasks to complete (success or fault).
- No retry logic — callers should schedule re-export via `MetricsService` interval.

### API

```csharp
public void AddExporter(IMetricsExporter exporter)   // thread-safe for single-threaded registration
public Task ExportAsync(MetricAggregation metrics)
public Task ExportBatchAsync(List<MetricAggregation> metrics)
```

### Usage

```csharp
var composite = MetricsExporterFactory.CreateComposite(logger);
composite.AddExporter(MetricsExporterFactory.CreatePrometheus(promLogger));
composite.AddExporter(MetricsExporterFactory.CreateHttp(endpoint, httpClient, httpLogger));

// Later, called by MetricsService on a timer
await composite.ExportAsync(currentAggregation);
```

---

## MetricsExporterFactory

Static factory methods — no internal state, pure convenience.

| Method | Returns | Notes |
|--------|---------|-------|
| `CreatePrometheus(ILogger<PrometheusMetricsExporter>)` | `IMetricsExporter` | |
| `CreateHttp(string endpoint, HttpClient, ILogger<HttpMetricsExporter>)` | `IMetricsExporter` | Validates non-empty endpoint and non-null client |
| `CreateComposite(ILogger<CompositeMetricsExporter>)` | `CompositeMetricsExporter` | Returns concrete type to allow `AddExporter` chaining |

### Typical DI registration (in `ServiceCollectionExtensions.cs`)

```csharp
services.AddSingleton<IMetricsExporter>(sp =>
{
    var composite = MetricsExporterFactory.CreateComposite(sp.GetRequiredService<ILogger<CompositeMetricsExporter>>());
    composite.AddExporter(MetricsExporterFactory.CreatePrometheus(sp.GetRequiredService<ILogger<PrometheusMetricsExporter>>()));
    composite.AddExporter(MetricsExporterFactory.CreateHttp(
        configuration["Metrics:HttpEndpoint"],
        sp.GetRequiredService<HttpClient>(),
        sp.GetRequiredService<ILogger<HttpMetricsExporter>>()));
    return composite;
});
```

---

## MetricAggregation (input model)

Defined in `Domain/Models/MetricAggregation.cs`. Fields consumed by exporters:

| Property | Type | Description |
|----------|------|-------------|
| `AverageProcessingTimeMs` | `double` | Mean processing latency in ms |
| `TotalItemsProcessed` | `long` | Successful items counter |
| `TotalItemsFailed` | `long` | Failed items counter |
| `TotalItemsSkipped` | `long` | Skipped items counter |
| `BackpressureEvents` | `long` | Number of backpressure triggers |
| `TotalBackpressureMs` | `long` | Cumulative backpressure wait time |
| `ComputedAt` | `DateTime` | Aggregation window end (UTC) |

---

## Best practices

1. **Reuse `HttpClient`** — inject a single instance (or `IHttpClientFactory`) to avoid socket exhaustion.
2. **Set timeouts** on the `HttpClient` (e.g., 5–10 s) so a hung collector doesn't block the pipeline.
3. **Register as singleton** — exporters are stateless and thread-safe.
4. **Monitor exporter health** — check logs for `Exporter failed` or HTTP warnings; consider adding a health-check endpoint that verifies the last successful export timestamp.
5. **Prometheus scraping** — `PrometheusMetricsExporter` does not self-host `/metrics`; pair with a sidecar (e.g., `prometheus-net.AspNetCore`) or push-gateway if you need pull-based scraping.

---

## Related documentation

- `docs/architecture.md` — overall pipeline architecture.
- `docs/monitoring/` — Prometheus + Grafana stack (docker-compose).
- `src/Metrics/MetricsService.cs` — producer that calls `ExportAsync` on a timer.