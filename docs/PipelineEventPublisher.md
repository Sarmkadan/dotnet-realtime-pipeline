# PipelineEventPublisher

The `PipelineEventPublisher` serves as the central coordination hub for broadcasting lifecycle events within the `dotnet-realtime-pipeline` architecture. It facilitates decoupled communication between pipeline stages by allowing components to publish specific state changes—such as data ingestion, processing completion, backpressure detection, metric aggregation, and error conditions—while providing a subscription mechanism for observers to react to these events in real time. The class maintains contextual metadata including correlation identifiers and timestamps to ensure traceability across distributed operations.

## API

### Constructors

#### `public PipelineEventPublisher()`
Initializes a new instance of the `PipelineEventPublisher` class. This constructor sets up the internal subscription registry and prepares the instance to handle event publication and context tracking.

### Publication Methods

#### `public async Task PublishDataIngestedAsync(DataIngestedEvent eventArgs)`
Asynchronously broadcasts a `DataIngestedEvent` to all subscribed observers, signaling that new data has entered the pipeline.
*   **Parameters**: `eventArgs` - An instance of `DataIngestedEvent` containing details about the ingested data.
*   **Returns**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an exception if the internal subscriber dispatch mechanism fails or if the event argument is null.

#### `public async Task PublishProcessingCompletedAsync(ProcessingCompletedEvent eventArgs)`
Asynchronously notifies subscribers that a specific processing stage has finished executing.
*   **Parameters**: `eventArgs` - An instance of `ProcessingCompletedEvent` containing the outcome of the processing step.
*   **Returns**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an exception if the dispatch fails or if the event argument is null.

#### `public async Task PublishBackpressureDetectedAsync(BackpressureContext context)`
Asynchronously signals that the pipeline is experiencing backpressure, providing context regarding the bottleneck.
*   **Parameters**: `context` - A `BackpressureContext` object describing the current load and constraints.
*   **Returns**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an exception if the context is invalid or dispatch fails.

#### `public async Task PublishMetricsCollectedAsync(MetricAggregation metrics)`
Asynchronously publishes aggregated performance metrics to interested observers.
*   **Parameters**: `metrics` - A `MetricAggregation` instance containing the collected statistical data.
*   **Returns**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an exception if the metrics object is null or dispatch fails.

#### `public async Task PublishPipelineErrorAsync(string operationName, Exception exception)`
Asynchronously reports a critical error occurring within a specific pipeline operation.
*   **Parameters**: 
    *   `operationName` - The name of the operation where the error occurred.
    *   `exception` - The `Exception` object detailing the failure.
*   **Returns**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an exception if the exception argument is null or dispatch fails.

### Subscription Management

#### `public void Subscribe<T>(Action<T> handler)`
Registers a typed event handler to receive notifications for events of type `T`.
*   **Parameters**: `handler` - The delegate to invoke when an event of type `T` is published.
*   **Returns**: None.
*   **Throws**: Throws an exception if the handler is null or if the type `T` is not a supported event type.

#### `public void Unsubscribe<T>(Action<T> handler)`
Removes a previously registered typed event handler.
*   **Parameters**: `handler` - The delegate to remove from the subscription list.
*   **Returns**: None.
*   **Throws**: Throws an exception if the handler was not previously subscribed.

#### `public int GetSubscriberCount()`
Returns the total number of active subscribers currently registered across all event types.
*   **Parameters**: None.
*   **Returns**: An `int` representing the count of subscribers.
*   **Throws**: None.

### Contextual Properties

#### `public DateTime Timestamp`
Gets or sets the `DateTime` representing the moment the current event context was created or last updated.

#### `public string CorrelationId`
Gets or sets the unique identifier used to trace a specific request or data unit across multiple pipeline stages.

#### `public DataPoint DataPoint`
Gets or sets the `DataPoint` associated with the current event context, representing the payload being processed.

#### `public ProcessingResult Result`
Gets or sets the `ProcessingResult` indicating the status and output of the most recent processing action.

#### `public string StageName`
Gets or sets the name of the pipeline stage currently associated with the publisher's context.

#### `public BackpressureContext Context`
Gets or sets the `BackpressureContext` detailing flow control status when backpressure events are relevant.

#### `public MetricAggregation Metrics`
Gets or sets the `MetricAggregation` object holding current performance statistics.

#### `public string OperationName`
Gets or sets the descriptive name of the operation currently being executed or monitored.

#### `public Exception Exception`
Gets or sets the `Exception` object associated with the current error state, if any.

### Event Types

#### `public sealed class DataIngestedEvent`
A sealed class representing the event arguments passed when data is successfully ingested into the pipeline.

#### `public sealed class ProcessingCompletedEvent`
A sealed class representing the event arguments passed when a processing stage completes successfully.

## Usage

### Example 1: Subscribing to Lifecycle Events
This example demonstrates how to attach handlers to specific event types and react to data ingestion and processing completion.

```csharp
var publisher = new PipelineEventPublisher();

// Subscribe to data ingestion events
publisher.Subscribe<DataIngestedEvent>(evt =>
{
    Console.WriteLine($"Data ingested at {publisher.Timestamp}: {evt.ToString()}");
});

// Subscribe to processing completion events
publisher.Subscribe<ProcessingCompletedEvent>(evt =>
{
    if (publisher.Result.IsSuccess)
    {
        Console.WriteLine($"Stage '{publisher.StageName}' completed successfully.");
    }
});

// Simulate setting context and publishing
publisher.CorrelationId = Guid.NewGuid().ToString();
publisher.StageName = "Validation";
publisher.Timestamp = DateTime.UtcNow;

await publisher.PublishDataIngestedAsync(new DataIngestedEvent());
await publisher.PublishProcessingCompletedAsync(new ProcessingCompletedEvent());
```

### Example 2: Error Handling and Backpressure Monitoring
This example illustrates publishing error events and monitoring backpressure conditions while tracking active subscribers.

```csharp
var publisher = new PipelineEventPublisher();

// Monitor backpressure
publisher.Subscribe<BackpressureContext>(ctx =>
{
    Console.WriteLine($"Backpressure detected: {ctx.ThresholdExceeded}");
});

// Monitor errors
publisher.Subscribe<Exception>(ex =>
{
    Console.Error.WriteLine($"Pipeline Error in {publisher.OperationName}: {ex.Message}");
});

publisher.OperationName = "DatabaseWrite";
publisher.Context = new BackpressureContext { ThresholdExceeded = true, CurrentLoad = 95 };

// Publish backpressure alert
await publisher.PublishBackpressureDetectedAsync(publisher.Context);

// Publish an error
try
{
    throw new InvalidOperationException("Connection timeout");
}
catch (Exception ex)
{
    publisher.Exception = ex;
    await publisher.PublishPipelineErrorAsync(publisher.OperationName, ex);
}

Console.WriteLine($"Active subscribers: {publisher.GetSubscriberCount()}");
```

## Notes

*   **Thread Safety**: The subscription methods (`Subscribe`, `Unsubscribe`) and the publication methods are designed to be thread-safe. Internal locking mechanisms ensure that subscribers can be added or removed while events are being dispatched without causing race conditions. However, the contextual properties (e.g., `CorrelationId`, `DataPoint`) are mutable and not thread-safe; they should be set by a single coordinating thread before invoking publication methods.
*   **Asynchronous Dispatch**: All `Publish` methods are asynchronous and return a `Task`. Callers must await these methods to ensure events are fully delivered before proceeding, especially in scenarios where event ordering is critical.
*   **Exception Propagation**: If a subscriber handler throws an exception during event dispatch, the `PipelineEventPublisher` captures the error to prevent it from disrupting other subscribers, but the primary publication task may complete successfully depending on the internal error handling strategy. Explicit errors should be reported via `PublishPipelineErrorAsync`.
*   **Type Safety**: The `Subscribe<T>` method relies on generic type matching. Attempting to subscribe to a type that is not explicitly supported by the publisher's internal routing logic will result in an exception. Only defined event types like `DataIngestedEvent`, `ProcessingCompletedEvent`, `BackpressureContext`, `MetricAggregation`, and `Exception` are guaranteed to be routable.
*   **Resource Management**: While the class manages internal subscriber lists, it does not implement `IDisposable`. Long-lived applications should ensure that handlers are explicitly removed via `Unsubscribe` when no longer needed to prevent memory leaks caused by lingering references.
