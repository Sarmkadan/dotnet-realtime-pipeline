# BackpressureContextExtensions

The `BackpressureContextExtensions` class provides a set of static utility methods designed to monitor, manage, and react to buffer capacity constraints within the real-time pipeline. These extensions facilitate safe data ingestion by offering mechanisms to estimate processing delays, evaluate critical fullness states, format diagnostic metrics, and perform thread-safe buffer modifications. By leveraging these methods, developers can implement robust flow control strategies that prevent system overload and ensure stable throughput under varying load conditions.

## API

### `EstimateTimeToCapacity`
Calculates the estimated time required for the buffer to reach a specific capacity level based on current consumption rates.
*   **Parameters**: Accepts the target `BackpressureContext` instance and the desired capacity threshold.
*   **Return Value**: Returns a `long` representing the estimated time in milliseconds.
*   **Exceptions**: Throws an exception if the context is null or if the consumption rate data required for calculation is unavailable or invalid.

### `IsCriticallyFull`
Determines whether the buffer associated with the provided context has exceeded a critical fullness threshold, indicating an immediate need to halt ingestion.
*   **Parameters**: Accepts the target `BackpressureContext` instance.
*   **Return Value**: Returns a `bool` value; `true` if the buffer is critically full, otherwise `false`.
*   **Exceptions**: Throws an exception if the context argument is null.

### `GetBackpressureDurationFormatted`
Retrieves the duration for which backpressure has been active, formatted as a human-readable string.
*   **Parameters**: Accepts the target `BackpressureContext` instance.
*   **Return Value**: Returns a `string` representation of the duration (e.g., "00:01:30").
*   **Exceptions**: Throws an exception if the context is null or if the internal timestamp tracking is corrupted.

### `RecordBackpressureEvent`
Logs a specific backpressure event into the context's history for auditing and trend analysis.
*   **Parameters**: Accepts the target `BackpressureContext` instance and details regarding the event (such as severity or trigger source).
*   **Return Value**: Returns `void`.
*   **Exceptions**: Throws an exception if the context is null or if the internal event log has reached its maximum capacity and cannot accept new entries.

### `GetBufferMetricsSummary`
Generates a comprehensive summary string containing current buffer metrics, including count, capacity percentage, and recent throughput.
*   **Parameters**: Accepts the target `BackpressureContext` instance.
*   **Return Value**: Returns a `string` containing the formatted metrics summary.
*   **Exceptions**: Throws an exception if the context is null.

### `SafeRemoveFromBuffer`
Attempts to remove a specified number of items from the buffer in a thread-safe manner, ensuring data integrity during concurrent access.
*   **Parameters**: Accepts the target `BackpressureContext` instance and the count of items to remove.
*   **Return Value**: Returns a `long` indicating the actual number of items successfully removed (which may be less than requested if the buffer contained fewer items).
*   **Exceptions**: Throws an exception if the context is null or if the requested count is negative.

### `HasSufficientCapacityForBatch`
Validates whether the buffer currently has enough available space to accommodate a new batch of items of a given size.
*   **Parameters**: Accepts the target `BackpressureContext` instance and the size of the incoming batch.
*   **Return Value**: Returns a `bool` value; `true` if sufficient capacity exists, otherwise `false`.
*   **Exceptions**: Throws an exception if the context is null or if the batch size is negative.

## Usage

### Example 1: Pre-ingestion Capacity Check
This example demonstrates how to verify buffer availability before attempting to push a large batch of events, preventing potential overflow errors.

```csharp
public async Task ProcessEventBatchAsync(BackpressureContext context, List<Event> events)
{
    if (!BackpressureContextExtensions.HasSufficientCapacityForBatch(context, events.Count))
    {
        var summary = BackpressureContextExtensions.GetBufferMetricsSummary(context);
        logger.LogWarning("Insufficient buffer capacity. Current metrics: {Metrics}", summary);
        return;
    }

    if (BackpressureContextExtensions.IsCriticallyFull(context))
    {
        var duration = BackpressureContextExtensions.GetBackpressureDurationFormatted(context);
        logger.LogError("Buffer critically full for {Duration}. Dropping batch.", duration);
        return;
    }

    await buffer.WriteAsync(events);
}
```

### Example 2: Dynamic Flow Control and Cleanup
This example illustrates estimating recovery time and safely draining the buffer when a downstream service becomes available again.

```csharp
public void ManageBufferRecovery(BackpressureContext context)
{
    if (downstreamService.IsHealthy)
    {
        var timeToRecover = BackpressureContextExtensions.EstimateTimeToCapacity(context, targetCapacity: 0);
        logger.LogInformation("Estimated time to clear buffer: {Ms}ms", timeToRecover);

        // Attempt to drain 100 items safely
        long removedCount = BackpressureContextExtensions.SafeRemoveFromBuffer(context, 100);
        
        if (removedCount > 0)
        {
            BackpressureContextExtensions.RecordBackpressureEvent(context, new BackpressureEvent 
            { 
                Type = EventType.Drain, 
                Count = removedCount 
            });
        }
    }
}
```

## Notes

*   **Thread Safety**: Methods modifying the buffer state, specifically `SafeRemoveFromBuffer` and `RecordBackpressureEvent`, are designed to be thread-safe and utilize internal synchronization primitives. Read-only operations like `IsCriticallyFull` and `GetBufferMetricsSummary` provide a snapshot of the state at the time of invocation but do not lock the context for the duration of external processing.
*   **Null Handling**: All extension methods strictly validate the `BackpressureContext` instance. Passing a `null` context will result in an immediate exception rather than a silent failure or default return value.
*   **Capacity Logic**: The `HasSufficientCapacityForBatch` method accounts for both the current item count and any reserved overhead defined in the context configuration. It does not guarantee that capacity will remain available by the time the write operation occurs in a highly concurrent environment.
*   **Time Estimation**: `EstimateTimeToCapacity` relies on historical consumption rates. If the pipeline has just started or if consumption patterns are erratic, the returned time estimate may have a higher margin of error.
