# PipelineEventPublisherExtensions

The `PipelineEventPublisherExtensions` class provides a set of static extension methods and utility functions designed to facilitate event broadcasting within the real-time data processing pipeline. It serves as the primary interface for emitting lifecycle events—such as data ingestion, processing completion, backpressure detection, and error reporting—to registered subscribers without requiring direct coupling between the pipeline core and event consumers. This abstraction ensures consistent event propagation and enables centralized monitoring and diagnostics.

## API

### PublishDataIngestedAsync
Asynchronously publishes an event indicating that a specific data item has been successfully ingested into the pipeline.
*   **Parameters**: Accepts the data payload and relevant metadata context required by the event subscribers.
*   **Return Value**: Returns a `Task` that completes when the event has been dispatched to all current subscribers.
*   **Exceptions**: Throws an exception if the underlying event dispatcher is in a faulted state or if serialization of the event payload fails.

### PublishProcessingCompletedAsync
Asynchronously publishes an event signaling that a specific data item has finished all processing stages within the pipeline.
*   **Parameters**: Accepts the processed result object and execution context, including timing information.
*   **Return Value**: Returns a `Task` representing the asynchronous operation.
*   **Exceptions**: Throws if the pipeline context is null or if the event distribution mechanism encounters an internal error.

### PublishBackpressureDetectedAsync
Asynchronously publishes an alert event when the pipeline detects backpressure, indicating that the ingestion rate is exceeding the processing capacity.
*   **Parameters**: Accepts severity level, current queue depth, and throttle recommendations.
*   **Return Value**: Returns a `Task` that completes upon notification delivery.
*   **Exceptions**: Throws if the backpressure metrics provided are invalid (e.g., negative queue depth).

### PublishMetricsCollectedAsync
Asynchronously publishes a snapshot of aggregated pipeline metrics to subscribed monitoring tools.
*   **Parameters**: Accepts a metrics collection object containing counters, gauges, and histograms.
*   **Return Value**: Returns a `Task` completing when the metrics are broadcast.
*   **Exceptions**: Throws if the metrics collection is empty or malformed.

### PublishPipelineErrorAsync
Asynchronously publishes a critical event detailing a fatal or non-fatal error occurring within the pipeline execution flow.
*   **Parameters**: Accepts the exception object, error code, and the component identifier where the failure occurred.
*   **Return Value**: Returns a `Task` representing the dispatch operation.
*   **Exceptions**: Throws if the provided exception object is null.

### GetAllSubscriberCounts
Retrieves a read-only dictionary mapping event type names to the number of active subscribers currently listening to those events.
*   **Parameters**: None.
*   **Return Value**: Returns an `IReadOnlyDictionary<string, int>` where the key is the event name and the value is the subscriber count.
*   **Exceptions**: Does not throw under normal operation; returns an empty dictionary if no subscribers are registered.

### HasSubscribers
Determines whether there are any active subscribers registered for any event type within the pipeline.
*   **Parameters**: None.
*   **Return Value**: Returns a `bool` value; `true` if at least one subscriber exists, otherwise `false`.
*   **Exceptions**: Does not throw.

### PublishDataIngestedBatchAsync
Asynchronously publishes a single event representing the ingestion of a batch of data items, optimizing throughput for high-volume scenarios.
*   **Parameters**: Accepts an enumerable collection of data items and batch metadata (e.g., batch ID, total count).
*   **Return Value**: Returns a `Task` that completes when the batch event is dispatched.
*   **Exceptions**: Throws if the batch collection is null or empty.

### PublishProcessingCompletedBatchAsync
Asynchronously publishes an event indicating that a batch of data items has completed processing.
*   **Parameters**: Accepts the collection of processed results and aggregate batch statistics.
*   **Return Value**: Returns a `Task` representing the asynchronous notification.
*   **Exceptions**: Throws if the batch result collection is invalid or if aggregate statistics are inconsistent.

## Usage

### Example 1: Single Item Lifecycle Tracking
This example demonstrates how to emit events for a single data item as it moves through the ingestion and processing phases, including error handling.

```csharp
public async Task ProcessSingleItemAsync(DataItem item)
{
    try
    {
        // Announce ingestion
        await PipelineEventPublisherExtensions.PublishDataIngestedAsync(item, new IngestionContext { Timestamp = DateTime.UtcNow });

        // Perform business logic
        var result = await TransformService.ExecuteAsync(item);

        // Announce completion
        await PipelineEventPublisherExtensions.PublishProcessingCompletedAsync(result, new ProcessingContext { DurationMs = 45 });
    }
    catch (Exception ex)
    {
        // Report the failure immediately
        await PipelineEventPublisherExtensions.PublishPipelineErrorAsync(ex, "TransformService", item.Id);
        throw;
    }
}
```

### Example 2: Batch Processing and Monitoring
This example illustrates publishing batch events and checking for subscriber presence before performing expensive metric aggregation.

```csharp
public async Task ProcessBatchAsync(IEnumerable<DataItem> batch)
{
    if (!PipelineEventPublisherExtensions.HasSubscribers)
    {
        // Skip expensive event preparation if no one is listening
        await ExecuteBatchLogicAsync(batch);
        return;
    }

    var batchId = Guid.NewGuid().ToString();
    
    // Publish batch ingestion
    await PipelineEventPublisherExtensions.PublishDataIngestedBatchAsync(batch, new BatchContext { BatchId = batchId, Count = batch.Count() });

    var results = await ExecuteBatchLogicAsync(batch);

    // Publish batch completion
    await PipelineEventPublisherExtensions.PublishProcessingCompletedBatchAsync(results, new BatchStatistics { SuccessCount = results.Count() });

    // Optional: Check specific subscriber counts for logging
    var counts = PipelineEventPublisherExtensions.GetAllSubscriberCounts;
    if (counts.TryGetValue("MetricsCollector", out int metricSubs) && metricSubs > 0)
    {
        await PipelineEventPublisherExtensions.PublishMetricsCollectedAsync(CollectCurrentMetrics());
    }
}
```

## Notes

*   **Thread Safety**: All members of `PipelineEventPublisherExtensions` are thread-safe. The internal subscriber registry utilizes concurrent data structures to allow safe registration and invocation from multiple pipeline threads simultaneously.
*   **Fire-and-Forget Behavior**: While the methods return `Task` objects, callers should be aware that depending on the configured event dispatcher strategy, these tasks may complete as soon as the event is queued rather than when every subscriber has finished processing the event. Await these tasks to ensure backpressure is respected if synchronous flow control is required.
*   **Empty Subscriber Handling**: If `HasSubscribers` returns `false`, the `Publish*` methods still execute but return immediately with minimal overhead. However, explicitly checking `HasSubscribers` before constructing complex event payloads can optimize performance in high-throughput scenarios.
*   **Batch vs. Single Events**: Use `PublishDataIngestedBatchAsync` and `PublishProcessingCompletedBatchAsync` when processing collections to reduce event noise and overhead. Do not mix single-item and batch events for the same logical transaction, as subscribers may expect consistent event granularity.
*   **Exception Propagation**: Exceptions thrown by individual subscribers during event handling are typically aggregated or logged by the internal dispatcher to prevent one faulty subscriber from breaking the pipeline flow; however, critical serialization errors or invalid argument exceptions in the publisher methods themselves will propagate to the caller.
