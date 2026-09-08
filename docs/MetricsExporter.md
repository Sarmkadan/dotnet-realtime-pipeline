# Metrics Exporter

This document describes the metrics exporter types defined in `src/Integration/MetricsExporter.cs`. Exporters accept `MetricAggregation` snapshots and either format them for logging, send them over HTTP, or fan them out to multiple exporters.

All types are in the `DotNetRealtimePipeline.Integration` namespace.

## IMetricsExporter

`IMetricsExporter` defines the asynchronous contract implemented by every metrics exporter.

### `Task ExportAsync(MetricAggregation metrics)`

Exports one metric aggregation.

- **Parameters**:
  - `metrics`: The metric snapshot to export.
- **Returns**: A `Task` that completes after the exporter has processed the snapshot.

### `Task ExportBatchAsync(List<MetricAggregation> metrics)`

Exports a list of metric aggregations.

- **Parameters**:
  - `metrics`: The metric snapshots to export.
- **Returns**: A `Task` that completes after the exporter has processed the batch.

The interface does not define cancellation-token overloads or perform null validation. Validation and failure behavior depend on the selected implementation.

## PrometheusMetricsExporter

`PrometheusMetricsExporter` converts a `MetricAggregation` into six Prometheus text-format sample lines:

- `pipeline_average_processing_time_ms`
- `pipeline_total_items_processed_total`
- `pipeline_failed_items_total`
- `pipeline_skipped_items_total`
- `pipeline_backpressure_events_total`
- `pipeline_total_backpressure_ms`

Each line uses `ComputedAt`, converted to Unix epoch milliseconds, as its timestamp. The current implementation constructs these lines in memory and logs their count at debug level; it does not expose or push the generated text.

### Constructor

```csharp
PrometheusMetricsExporter(ILogger<PrometheusMetricsExporter> logger)
```

The logger is required. Passing `null` throws `ArgumentNullException`.

### Export behavior

- `ExportAsync` formats one aggregation and logs the number of generated metric lines.
- `ExportBatchAsync` calls `ExportAsync` for every item and awaits all calls with `Task.WhenAll`.

## HttpMetricsExporter

`HttpMetricsExporter` serializes metrics with `System.Text.Json` and sends them to a configured endpoint in an HTTP `POST` request. The request body uses the `application/json` media type and UTF-8 encoding.

### Constructor

```csharp
HttpMetricsExporter(
    string endpoint,
    HttpClient httpClient,
    ILogger<HttpMetricsExporter> logger)
```

All three arguments are required. A `null` argument throws `ArgumentNullException`. The exporter uses the supplied `HttpClient`; it does not create or dispose the client.

### Export behavior

- `ExportAsync` serializes one `MetricAggregation` and sends one request.
- `ExportBatchAsync` serializes the complete `List<MetricAggregation>` and sends it in one request.
- Successful responses are logged. Non-success HTTP status codes generate a warning.
- Serialization and request exceptions are caught and logged, so these failures do not propagate to the caller.

## CompositeMetricsExporter

`CompositeMetricsExporter` forwards each export call to every registered `IMetricsExporter` concurrently.

### Constructor

```csharp
CompositeMetricsExporter(ILogger<CompositeMetricsExporter> logger)
```

The logger is required. Passing `null` throws `ArgumentNullException`.

### `void AddExporter(IMetricsExporter exporter)`

Adds an exporter to the composite.

- **Parameters**:
  - `exporter`: The exporter that should receive subsequent single and batch exports.
- **Returns**: Nothing.
- **Exceptions**: Throws `ArgumentNullException` when `exporter` is `null`.

`ExportAsync` and `ExportBatchAsync` invoke the corresponding method on all registered exporters and wait for every invocation to finish. A fault from an individual exporter is logged as `Exporter failed`; the composite observes that fault rather than rethrowing it. If no exporters have been added, both methods complete without doing any work.

Add exporters during setup. The internal exporter list does not provide synchronization for concurrent calls to `AddExporter` and export methods.

## MetricsExporterFactory

`MetricsExporterFactory` provides static creation helpers. Each method passes its arguments directly to the corresponding constructor.

### `IMetricsExporter CreatePrometheus(ILogger<PrometheusMetricsExporter> logger)`

Creates a `PrometheusMetricsExporter` and returns it through the `IMetricsExporter` interface.

### `IMetricsExporter CreateHttp(string endpoint, HttpClient client, ILogger<HttpMetricsExporter> logger)`

Creates an `HttpMetricsExporter` for the supplied endpoint and client, returned through the `IMetricsExporter` interface.

### `CompositeMetricsExporter CreateComposite(ILogger<CompositeMetricsExporter> logger)`

Creates an empty `CompositeMetricsExporter`. Its concrete return type allows callers to register children with `AddExporter`.

## Usage

The following example creates both concrete exporters through the factory, composes them, and exports one snapshot and one batch:

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Integration;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

using var httpClient = new HttpClient();

IMetricsExporter prometheus = MetricsExporterFactory.CreatePrometheus(
    loggerFactory.CreateLogger<PrometheusMetricsExporter>());

IMetricsExporter http = MetricsExporterFactory.CreateHttp(
    "https://metrics.example.com/v1/aggregations",
    httpClient,
    loggerFactory.CreateLogger<HttpMetricsExporter>());

CompositeMetricsExporter composite = MetricsExporterFactory.CreateComposite(
    loggerFactory.CreateLogger<CompositeMetricsExporter>());

composite.AddExporter(prometheus);
composite.AddExporter(http);

var snapshot = new MetricAggregation
{
    MetricId = 1,
    MetricType = "minute",
    TimeWindowStartMs = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeMilliseconds(),
    TimeWindowEndMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    TotalItemsProcessed = 1_000,
    TotalItemsFailed = 2,
    TotalItemsSkipped = 3,
    AverageProcessingTimeMs = 4.5,
    BackpressureEvents = 1,
    TotalBackpressureMs = 25,
    ComputedAt = DateTime.UtcNow
};

await composite.ExportAsync(snapshot);
await composite.ExportBatchAsync(new List<MetricAggregation> { snapshot });
```

In an application that uses dependency injection, reuse a managed `HttpClient` rather than creating a new client for every export.
