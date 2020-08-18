# ApiEndpointHandler

The `ApiEndpointHandler` provides a structured interface for interacting with a real-time data processing pipeline, exposing endpoints to ingest data, check pipeline status, and retrieve operational metrics. It encapsulates response handling, error tracking, and batch processing capabilities for clients consuming the pipeline's API.

## API

### Properties

#### `public bool Success`
Indicates whether the last operation completed successfully. Returns `true` if the operation succeeded; otherwise, `false`.

#### `public T Data`
Gets the strongly-typed payload returned by the last operation. The type `T` varies depending on the endpoint called (e.g., `bool`, `BatchIngestResult`, `PipelineStatusInfo`). Returns `default(T)` if the operation failed or no data was returned.

#### `public string Message`
Provides a human-readable status or error message associated with the last operation. Useful for logging and client-facing feedback.

#### `public int StatusCode`
Contains the HTTP status code returned by the last operation (e.g., 200, 400, 500). Reflects the server's response status.

#### `public DateTime Timestamp`
The UTC timestamp when the last operation completed. Useful for tracking response freshness and debugging timing issues.

#### `public DataIngestionHandler`
Provides access to a handler specialized for data ingestion operations. Used internally to delegate ingestion-related calls.

#### `public int SuccessfulCount`
The number of items successfully processed in the most recent batch ingestion. Only valid after calling `IngestBatchAsync`.

#### `public int FailedCount`
The number of items that failed processing in the most recent batch ingestion. Only valid after calling `IngestBatchAsync`.

#### `public int TotalCount`
The total number of items processed in the most recent batch ingestion. Only valid after calling `IngestBatchAsync`.

#### `public StatusHandler`
Provides access to a handler specialized for pipeline status operations. Used internally to delegate status-related calls.

#### `public string PipelineName`
The name of the pipeline being monitored or controlled. Set during initialization and immutable for the lifetime of the handler.

#### `public string Version`
The version identifier of the pipeline service. Useful for compatibility checks and diagnostics.

#### `public bool IsRunning`
Indicates whether the pipeline is currently running. Returns `true` if the pipeline is active; otherwise, `false`.

#### `public long TotalProcessed`
The cumulative count of items successfully processed by the pipeline since startup.

#### `public long TotalFailed`
The cumulative count of items that failed processing by the pipeline since startup.

#### `public int Pending`
The number of items currently queued or awaiting processing in the pipeline.

#### `public string HealthStatus`
A string summarizing the pipeline's operational health (e.g., "Healthy", "Degraded", "Unhealthy"). Reflects internal health checks.

### Methods

#### `public async Task<ApiResponse<bool>> IngestAsync(...)`
Initiates an asynchronous ingestion of a single data item.

- **Parameters**: Accepts the data payload and optional metadata required for ingestion.
- **Return value**: Returns an `ApiResponse<bool>` where `Success` indicates ingestion acceptance, `Data` is `true` if accepted, `StatusCode` reflects HTTP status, and `Message` provides details.
- **Exceptions**: May throw if the pipeline is unreachable or the payload is malformed.

#### `public async Task<ApiResponse<BatchIngestResult>> IngestBatchAsync(...)`
Initiates an asynchronous batch ingestion of multiple data items.

- **Parameters**: Accepts a collection of items and optional batch-level metadata.
- **Return value**: Returns an `ApiResponse<BatchIngestResult>` containing `SuccessfulCount`, `FailedCount`, `TotalCount`, and per-item results in `Data`.
- **Exceptions**: May throw if the batch is empty, too large, or the pipeline rejects the payload format.

#### `public async Task<ApiResponse<PipelineStatusInfo>> GetStatusAsync()`
Retrieves the current operational status of the pipeline.

- **Parameters**: None.
- **Return value**: Returns an `ApiResponse<PipelineStatusInfo>` with `IsRunning`, `TotalProcessed`, `TotalFailed`, `Pending`, `HealthStatus`, and other metadata in `Data`.
- **Exceptions**: May throw if the status service is unavailable.

## Usage

### Example 1: Single Item Ingestion
```csharp
var handler = new ApiEndpointHandler("sample-pipeline", "1.0.0");
var response = await handler.IngestAsync(new DataItem { Id = "123", Payload = "test" });

if (response.Success)
{
    Console.WriteLine($"Ingestion accepted: {response.Message}");
}
else
{
    Console.WriteLine($"Ingestion failed: {response.Message} (Status: {response.StatusCode})");
}
```

### Example 2: Batch Ingestion with Status Check
```csharp
var handler = new ApiEndpointHandler("analytics-pipeline", "2.1.0");
var batch = new[] { new DataItem { Id = "a" }, new DataItem { Id = "b" } };

var batchResponse = await handler.IngestBatchAsync(batch);
Console.WriteLine($"Batch processed: {batchResponse.Data.SuccessfulCount} succeeded, {batchResponse.Data.FailedCount} failed");

var statusResponse = await handler.GetStatusAsync();
Console.WriteLine($"Pipeline health: {statusResponse.Data.HealthStatus}");
```

## Notes

- **Thread Safety**: All public members are safe for concurrent read access. Write operations (e.g., ingestion) are thread-safe at the handler level, but clients should avoid overlapping calls that mutate shared state (e.g., calling `IngestAsync` and `IngestBatchAsync` simultaneously without coordination).
- **State Validity**: Properties like `SuccessfulCount`, `FailedCount`, and `TotalCount` reflect the most recent batch operation. If no batch has been processed, these values are undefined and should not be used.
- **Error Handling**: Non-success responses do not throw; instead, they populate `Success`, `Message`, and `StatusCode`. Clients should check `Success` before accessing `Data`.
- **Initialization**: `PipelineName` and `Version` are set at construction and cannot be changed. Ensure these values match the target pipeline to avoid misrouting requests.
- **Health Fluctuations**: `HealthStatus` may change rapidly under load. Clients should poll or subscribe to updates rather than caching the value for critical decisions.
