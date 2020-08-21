# IMetricsExporter

The `IMetricsExporter` type represents the contract for exporting metrics data from the pipeline to external monitoring systems. Concrete implementations such as `PrometheusMetricsExporter`, `HttpMetricsExporter`, and `CompositeMetricsExporter` provide specific export mechanisms, while static factory methods simplify their creation. The interface enables uniform invocation of asynchronous export operations across different exporter types.

## API

### PrometheusMetricsExporter()
Initializes a new instance of the `PrometheusMetricsExporter` class.  
**Parameters:** None.  
**Return value:** A new `PrometheusMetricsExporter` ready to export metrics in Prometheus format.  
**Exceptions:** None under normal conditions; may throw `ObjectDisposedException` if the underlying resources have been disposed prior to construction (unlikely).

### ExportAsync()
Asynchronously exports the current set of metrics to the configured Prometheus endpoint.  
**Parameters:** None.  
**Return value:** A `Task` that completes when the export operation finishes.  
**Exceptions:**  
- `IOException` if a network or I/O error occurs while contacting the endpoint.  
- `OperationCanceledException` if the operation is canceled via an external cancellation token (if applicable).  
- `InvalidOperationException` if the exporter has not been properly initialized.

### ExportBatchAsync()
Asynchronously exports a batch of metrics data to the Prometheus endpoint.  
**Parameters:** None.  
**Return value:** A `Task` that completes when the batch export operation finishes.  
**Exceptions:** Same as `ExportAsync()`.

### HttpMetricsExporter()
Initializes a new instance of the `HttpMetricsExporter` class.  
**Parameters:** None.  
**Return value:** A new `HttpMetricsExporter` ready to export metrics via HTTP.  
**Exceptions:** None under normal conditions.

### ExportAsync()
Asynchronously exports metrics to the configured HTTP endpoint.  
**Parameters:** None.  
**Return value:** A `Task` that completes when the export operation finishes.  
**Exceptions:**  
- `IOException` for network‑related failures.  
- `HttpRequestException` if the HTTP request fails (e.g., non‑success status code).  
- `OperationCanceledException` if the operation is canceled.  
- `InvalidOperationException` if the exporter is not ready.

### ExportBatchAsync()
Asynchronously exports a batch of metrics to the HTTP endpoint.  
**Parameters:** None.  
**Return value:** A `Task` that completes when the batch export finishes.  
**Exceptions:** Same as `ExportAsync()`.

### CompositeMetricsExporter()
Initializes a new instance of the `CompositeMetricsExporter` class, which aggregates multiple exporters.  
**Parameters:** None.  
**Return value:** A new `CompositeMetricsExporter` with an empty collection of exporters.  
**Exceptions:** None.

### AddExporter(IMetricsExporter exporter)
Adds an exporter to the composite’s internal collection.  
**Parameters:**  
- `exporter`: The `IMetricsExporter` instance to add; must not be `null`.  
**Return value:** `void`.  
**Exceptions:**  
- `ArgumentNullException` if `exporter` is `null`.  
- `InvalidOperationException` if the composite has been disposed or is otherwise unable to accept new exporters.

### ExportAsync()
Asynchronously exports metrics by delegating the call to each added exporter in sequence.  
**Parameters:** None.  
**Return value:** A `Task` that completes when all exporters have finished their export operations.  
**Exceptions:**  
- Propagates any exception thrown by an individual exporter.  
- `ObjectDisposedException` if the composite has been disposed.  
- `InvalidOperationException` if no exporters have been added.

### ExportBatchAsync()
Asynchronously exports a batch of metrics by delegating the call to each added exporter.  
**Parameters:** None.  
**Return value:** A `Task` that completes when all exporters have finished the batch export.  
**Exceptions:** Same as `ExportAsync()` for the composite.

### CreatePrometheus()
Static factory method that creates a `PrometheusMetricsExporter` instance and returns it as an `IMetricsExporter`.  
**Parameters:** None.  
**Return value:** An `IMetricsExporter` wrapping a new `PrometheusMetricsExporter`.  
**Exceptions:** None.

### CreateHttp()
Static factory method that creates an `HttpMetricsExporter` instance and returns it as an `IMetricsExporter`.  
**Parameters:** None.  
**Return value:** An `IMetricsExporter` wrapping a new `HttpMetricsExporter`.  
**Exceptions:** None.

### CreateComposite()
Static factory method that creates a `CompositeMetricsExporter` instance.  
**Parameters:** None.  
**Return value:** A new `CompositeMetricsExporter`.  
**Exceptions:** None.

## Usage

### Exporting metrics via Prometheus
```csharp
using DotnetRealtimePipeline.Metrics;

// Create a Prometheus exporter using the factory helper
IMetricsExporter exporter = MetricsExporterFactory.CreatePrometheus();

// Export the current metrics snapshot
await exporter.ExportAsync();

// Export a batch of metrics (e.g., after a processing window)
await exporter.ExportBatchAsync();
```

### Using a composite exporter to send metrics to multiple endpoints
```csharp
using DotnetRealtimePipeline.Metrics;

// Build a composite that forwards to both Prometheus and HTTP endpoints
var composite = MetricsExporterFactory.CreateComposite();
composite.AddExporter(MetricsExporterFactory.CreatePrometheus());
composite.AddExporter(MetricsExporterFactory.CreateHttp());

// Export to all registered endpoints
await composite.ExportAsync();
await composite.ExportBatchAsync();
```

## Notes
- All exporter implementations are designed to be thread‑safe for concurrent calls to `ExportAsync` and `ExportBatchAsync`. However, mutating the exporter collection (e.g., calling `AddExporter` on a `CompositeMetricsExporter`) should be performed before any export operations begin or with appropriate external synchronization.
- The static factory methods return interfaces or concrete types that are ready to use immediately; no additional configuration is required in the current version of the library.
- If an exporter encounters a fatal error (e.g., persistent network failure), subsequent export calls may continue to throw exceptions until the underlying issue is resolved or the exporter is disposed.
- Consumers should dispose of exporters that implement `IDisposable` when they are no longer needed to release any held resources such as HTTP clients or timers. The factory methods do not return disposables directly; the concrete types expose `Dispose` if needed.
