# RestApiHandler

This document describes the specific API handlers defined in `src/API/RestApiHandler.cs`.

## Base Class

The `ApiEndpointHandler` base class and its `ApiResponse<T>` are documented in [ApiEndpointHandler.md](./ApiEndpointHandler.md).

## Specific Handlers

### DataIngestionHandler

Handles data ingestion endpoints.

#### Methods

##### IngestAsync(DataPoint dataPoint)
Ingests a single data point.

- **Parameters**: 
  - `dataPoint`: The data point to ingest.
- **Returns**: `Task<ApiResponse<bool>>`
  - `Success`: True if the data point was ingested (accepted by the orchestrator), false otherwise.
  - `Data`: Same as Success (bool).
  - `Message`: A message indicating the result.
  - `StatusCode`: 200 on success, 429 if rejected due to backpressure, 400 if dataPoint is null, 500 on exception.

##### IngestBatchAsync(List<DataPoint> dataPoints)
Ingests a batch of data points.

- **Parameters**:
  - `dataPoints`: The list of data points to ingest.
- **Returns**: `Task<ApiResponse<BatchIngestResult>>`
  - `Success`: True if the batch was processed (even if some items failed, unless an exception occurred).
  - `Data`: A `BatchIngestResult` object with counts.
  - `Message`: A message summarizing the batch processing.
  - `StatusCode`: 200 on success (even if some failed), 400 if the list is null or empty, 500 on exception.

### StatusHandler

Handles pipeline status endpoints.

#### Methods

##### GetStatusAsync()
Retrieves the current pipeline status and health.

- **Parameters**: None.
- **Returns**: `Task<ApiResponse<PipelineStatusInfo>>`
  - `Success`: True if the status was retrieved successfully.
  - `Data`: A `PipelineStatusInfo` object containing:
    - `PipelineName`: The name of the pipeline.
    - `Version`: The version of the pipeline.
    - `IsRunning`: Whether the pipeline is currently running.
    - `TotalProcessed`: Total number of data points processed.
    - `TotalFailed`: Total number of data points that failed.
    - `Pending`: Number of items pending in the queue.
    - `HealthStatus`: A string representing the health status (e.g., "Healthy").
    - `Throughput`: The throughput in items per second.
    - `SuccessRate`: The success rate as a percentage.
    - `AverageLatency`: The average processing time in milliseconds.
  - `Message`: A message indicating the result.
  - `StatusCode`: 200 on success, 500 on exception.

### QueryHandler

Handles data query endpoints.

#### Methods

##### QueryAsync(long startMs, long endMs, string source = "", int minQuality = 0)
Queries for data points within a time range and optional filters.

- **Parameters**:
  - `startMs`: The start time in milliseconds since epoch.
  - `endMs`: The end time in milliseconds since epoch.
  - `source`: Optional source filter (default: empty string, meaning no filter).
  - `minQuality`: Optional minimum quality filter (default: 0, meaning no minimum quality).
- **Returns**: `Task<ApiResponse<List<DataPoint>>>`
  - `Success`: True if the query was executed successfully.
  - `Data`: A list of `DataPoint` objects matching the query.
  - `Message`: A message indicating the number of results returned.
  - `StatusCode`: 200 on success, 500 on exception.

### ApiErrorResponse

Represents an error response from the API.

#### Properties

- `StatusCode`: The HTTP status code.
- `Message`: A human-readable error message.
- `ErrorCode`: A string code representing the type of error.
- `Timestamp`: The UTC timestamp when the error occurred.

#### Static Factory Methods

- `BadRequest(string message)`: Creates an error response with status code 400.
- `NotFound(string message)`: Creates an error response with status code 404.
- `InternalError(string message)`: Creates an error response with status code 500.
- `TooManyRequests(string message)`: Creates an error response with status code 429.

## Usage Examples

We can refer to the examples in [ApiEndpointHandler.md](./ApiEndpointHandler.md) for the base usage patterns.

However, note that the specific handlers are used as follows:

### Example: Ingesting a Single Data Point

```csharp
var handler = new DataIngestionHandler(orchestrator, logger);
var dataPoint = new DataPoint { /* ... */ };
var response = await handler.IngestAsync(dataPoint);

if (response.Success)
{
    Console.WriteLine($"Data point ingested: {response.Message}");
}
else
{
    Console.WriteLine($"Failed to ingest data point: {response.Message} (Status: {response.StatusCode})");
}
```

### Example: Checking Pipeline Status

```csharp
var handler = new StatusHandler(orchestrator, logger);
var response = await handler.GetStatusAsync();

if (response.Success)
{
    var status = response.Data;
    Console.WriteLine($"Pipeline {status.PipelineName} is {(status.IsRunning ? "running" : "stopped")}");
    Console.WriteLine($"Health: {status.HealthStatus}, Throughput: {status.Throughput} eps");
}
else
{
    Console.WriteLine($"Failed to get status: {response.Message}");
}
```

## Notes

- The `ApiResponse<T>` class is defined in the base class `ApiEndpointHandler` and is documented in [ApiEndpointHandler.md](./ApiEndpointHandler.md).
- All handlers depend on an `ILogger` and a `PipelineOrchestrator` (or its methods) for their operation.
- Error handling is consistent: exceptions are caught and logged, and an `ApiResponse` with `Success=false` and appropriate status code is returned.