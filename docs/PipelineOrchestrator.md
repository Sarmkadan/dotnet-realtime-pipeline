# PipelineOrchestrator

The `PipelineOrchestrator` coordinates the lifecycle and data flow of a real‑time processing pipeline. It provides asynchronous methods to start and stop the pipeline, ingest data points individually or in batches, query pipeline state, and retrieve performance and health metrics.

## API

### PipelineOrchestrator()
Constructs a new orchestrator instance. The orchestrator is initially stopped and holds no configuration until explicitly set elsewhere.

### StartAsync()
**Purpose:** Begins processing of incoming data points.  
**Parameters:** None.  
**Return Value:** A `Task` that completes when the pipeline has started successfully.  
**Exceptions:**  
- `InvalidOperationException` – the pipeline is already running.  
- `ObjectDisposedException` – the orchestrator has been disposed.  
- `OperationCanceledException` – the start operation was cancelled via an external token.

### StopAsync()
**Purpose:** Halts the pipeline and waits for any in‑flight work to finish.  
**Parameters:** None.  
**Return Value:** A `Task` that completes when the pipeline has stopped.  
**Exceptions:**  
- `InvalidOperationException` – the pipeline is not running.  
- `ObjectDisposedException` – the orchestrator has been disposed.  
- `OperationCanceledException` – the stop operation was cancelled.

### IngestDataPointAsync(T dataPoint)
**Purpose:** Submits a single data point for processing.  
**Parameters:**  
- `dataPoint` – the data point to ingest; type `T` is the generic payload type used by the pipeline.  
**Return Value:** A `Task<bool>` where `true` indicates the point was accepted for processing and `false` indicates it was rejected (e.g., due to queue limits).  
**Exceptions:**  
- `ArgumentNullException` – `dataPoint` is `null`.  
- `InvalidOperationException` – the pipeline is not running.  
- `OperationCanceledException` – the pipeline is stopping.

### ProcessBatchDataPointsAsync(IEnumerable<T> batch)
**Purpose:** Submits a collection of data points for batch processing.  
**Parameters:**  
- `batch` – sequence of data points to process; must not be `null`.  
**Return Value:** A `Task<BatchProcessingResult>` containing counts of succeeded and failed items and any aggregated errors.  
**Exceptions:**  
- `ArgumentNullException` – `batch` is `null`.  
- `InvalidOperationException` – the pipeline is not running.  
- `OperationCanceledException` – the pipeline is stopping during batch submission.

### GetQueryService()
**Purpose:** Provides access to a read‑only query interface for inspecting pipeline state.  
**Parameters:** None.  
**Return Value:** An instance of `QueryService` that can be used to query historical data, metrics, and configuration.  
**Exceptions:** None.

### GetStatus()
**Purpose:** Retrieves the current operational status of the pipeline.  
**Parameters:** None.  
**Return Value:** A `PipelineStatus` enum value (`Stopped`, `Starting`, `Running`, `Stopping`, `Faulted`).  
**Exceptions:** None.

### GetHealthReportAsync()
**Purpose:** Obtains a detailed health report including latency, error rates, and resource usage.  
**Parameters:** None.  
**Return Value:** A `Task<HealthReport>` representing the pipeline’s health at the moment of the call.  
**Exceptions:**  
- `ObjectDisposedException` – the orchestrator has been disposed.  
- `OperationCanceledException` – the health check was cancelled.

### GetThroughput()
**Purpose:** Gets the current processing throughput measured in data points per second.  
**Parameters:** None.  
**Return Value:** A `double` representing the instantaneous throughput.  
**Exceptions:** None.

### GetPerformanceTrendAsync()
**Purpose:** Retrieves a historical trend of performance metrics over a configurable window.  
**Parameters:** None.  
**Return Value:** A `Task<PerformanceTrend>` containing time‑series data for throughput, latency, and error rates.  
**Exceptions:**  
- `ObjectDisposedException` – the orchestrator has been disposed.  
- `OperationCanceledException` – the trend query was cancelled.

### SuccessfulCount
**Purpose:** Number of data points successfully processed since the pipeline started.  
**Type:** `int`  
**Exceptions:** None.

### FailedCount
**Purpose:** Number of data points that failed processing since the pipeline started.  
**Type:** `int`  
**Exceptions:** None.

### IsRunning
**Purpose:** Indicates whether the pipeline is currently processing data.  
**Type:** `bool`  
**Exceptions:** None.

### TotalDataPointsProcessed
**Purpose:** Cumulative count of all data points that have entered the pipeline (successful + failed).  
**Type:** `long`  
**Exceptions:** None.

### TotalDataPointsFailed
**Purpose:** Cumulative count of data points that have failed processing.  
**Type:** `long`  
**Exceptions:** None.

### PendingItemsInQueue
**Purpose:** Approximate number of data points waiting to be processed in the internal queue.  
**Type:** `int`  
**Exceptions:** None.

### ConfigurationName
**Purpose:** Human‑readable name of the active pipeline configuration.  
**Type:** `string`  
**Exceptions:** None.

### ConfigurationVersion
**Purpose:** Version identifier of the active pipeline configuration (e.g., semantic version or hash).  
**Type:** `string`  
**Exceptions:** None.

### Timestamp
**Purpose:** UTC timestamp indicating when the orchestrator’s state was last updated (useful for staleness checks).  
**Type:** `DateTime`  
**Exceptions:** None.

## Usage

### Example 1: Starting the pipeline, ingesting data, and stopping

```csharp
using System.Threading.Tasks;
using DotnetRealtimePipeline;

public class Demo
{
    private readonly PipelineOrchestrator<MyPayload> _orchestrator;

    public Demo()
    {
        _orchestrator = new PipelineOrchestrator<MyPayload>();
    }

    public async Task RunAsync()
    {
        // Start processing
        await _orchestrator.StartAsync();

        // Ingest a few data points
        var point1 = new MyPayload { Value = 42 };
        var point2 = new MyPayload { Value = 7 };
        await _orchestrator.IngestDataPointAsync(point1);
        await _orchestrator.IngestDataPointAsync(point2);

        // Stop gracefully
        await _orchestrator.StopAsync();
    }
}
```

### Example 2: Batch processing and querying metrics

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetRealtimePipeline;

public class MetricsDemo
{
    private readonly PipelineOrchestrator<SensorReading> _orchestrator;

    public MetricsDemo()
    {
        _orchestrator = new PipelineOrchestrator<SensorReading>();
    }

    public async Task ProcessAndReportAsync()
    {
        await _orchestrator.StartAsync();

        var batch = new List<SensorReading>
        {
            new SensorReading { Timestamp = System.DateTime.UtcNow, Temperature = 22.5 },
            new SensorReading { Timestamp = System.DateTime.UtcNow.AddSeconds(-1), Temperature = 23.0 }
        };

        var result = await _orchestrator.ProcessBatchDataPointsAsync(batch);
        // result.SuccessfulCount, result.FailedCount, result.Errors

        var throughput = _orchestrator.GetThroughput();
        var health   = await _orchestrator.GetHealthReportAsync();
        var status   = _orchestrator.GetStatus();

        await _orchestrator.StopAsync();
    }
}
```

## Notes

- **Thread‑safety:** All public members of `PipelineOrchestrator` are safe to call concurrently from multiple threads. Internal state is protected by locks or concurrent collections; however, rapid successive calls to `StartAsync` or `StopAsync` from different threads may result in `InvalidOperationException` if the requested transition conflicts with the current state.
- **State transitions:** The orchestrator enforces a linear lifecycle (`Stopped → Starting → Running → Stopping → Stopped`). Calling `StartAsync` while `IsRunning` is `true` or `StopAsync` while `IsRunning` is `false` throws `InvalidOperationException`.
- **Back‑pressure:** `IngestDataPointAsync` may return `false` when the internal queue is bounded and full. Callers should handle this by retrying, dropping, or applying their own back‑pressure logic.
- **Error handling:** Batch processing returns a `BatchProcessingResult` that aggregates failures; individual point failures do not throw exceptions unless the pipeline itself faults, in which case subsequent calls may throw `ObjectDisposedException` or related errors.
- **Performance metrics:** `GetThroughput` provides an instantaneous snapshot; for trending analysis use `GetPerformanceTrendAsync`. The values are updated periodically and may lag behind real‑time activity by a few milliseconds.
- **Configuration:** `ConfigurationName` and `ConfigurationVersion` reflect the configuration loaded at orchestrator construction or via external configuration mechanisms; they do not change after the orchestrator is started.
- **Timestamp:** The `Timestamp` property is updated on each state change (start, stop, fault) and can be used to detect stale orchestrator instances in long‑running services.
