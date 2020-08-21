# DeadLetterQueue
The `DeadLetterQueue` provides a durable holding area for messages that have failed processing in a real‑time pipeline, allowing them to be inspected, retried, or permanently discarded.

## API
### DeadLetterQueue()
Creates a new instance of the dead letter queue. The queue starts empty and is ready for immediate use.

### EnqueueAsync()
Adds a dead letter entry to the queue for later processing.  
**Return value:** A `Task` that completes when the entry has been stored.  
**Throws:**  
- `ObjectDisposedException` if the queue has been disposed.  
- Any exception thrown by the underlying storage mechanism (e.g., `IOException`).

### PeekAsync()
Retrieves a snapshot of the entries currently in the queue without removing them.  
**Return value:** A `Task<IReadOnlyList<DeadLetterEntry>>` containing the queued entries. The list may be empty.  
**Throws:**  
- `ObjectDisposedException` if the queue has been disposed.  
- Any exception from the storage layer.

### DequeueForRetryAsync()
Removes and returns a batch of entries that are eligible for retry.  
**Return value:** A `Task<IReadOnlyList<DeadLetterEntry>>` with the entries taken out of the queue. The list may be empty if no entries are ready for retry.  
**Throws:**  
- `ObjectDisposedException` if the queue has been disposed.  
- Any exception from the storage layer.

### AcknowledgeFailureAsync()
Marks the most recently dequeued entry as still failed, causing it to be retained or re‑queued according to the queue’s policy.  
**Return value:** A `Task` that completes when the acknowledgment is recorded.  
**Throws:**  
- `ObjectDisposedException` if the queue has been disposed.  
- `InvalidOperationException` if no entry has been dequeued for acknowledgment.  
- Any exception from the storage layer.

### AcknowledgeSuccessAsync()
Marks the most recently dequeued entry as successfully processed, resulting in its permanent removal from the queue.  
**Return value:** A `Task` that completes when the acknowledgment is recorded.  
**Throws:**  
- `ObjectDisposedException` if the queue has been disposed.  
- `InvalidOperationException` if no entry has been dequeued for acknowledgment.  
- Any exception from the storage layer.

### GetStatsAsync()
Retrieves statistical information about the queue, such as current depth and failure rates.  
**Return value:** A `Task<DeadLetterQueueStats>` containing the metrics.  
**Throws:**  
- `ObjectDisposedException` if the queue has been disposed.  
- Any exception from the storage layer.

## Usage
```csharp
var dlq = new DeadLetterQueue();

// Enqueue a failed message
await dlq.EnqueueAsync();

// Later, inspect what is waiting
IReadOnlyList<DeadLetterEntry> pending = await dlq.PeekAsync();
foreach (var entry in pending)
{
    Console.WriteLine($"Entry {entry.Id} awaits retry");
}

// Attempt to retry a batch of entries
IReadOnlyList<DeadLetterEntry> toRetry = await dlq.DequeueForRetryAsync();
foreach (var entry in toRetry)
{
    try
    {
        await ProcessMessage(entry);
        await dlq.AcknowledgeSuccessAsync(); // remove on success
    }
    catch
    {
        await dlq.AcknowledgeFailureAsync(); // keep for later
    }
}

// Monitor queue health
DeadLetterQueueStats stats = await dlq.GetStatsAsync();
Console.WriteLine($"Depth: {stats.CurrentDepth}, Failed: {stats.FailedCount}");
```

```csharp
using (var dlq = new DeadLetterQueue())
{
    // Simulate a pipeline that continuously feeds the dead letter queue
    while (pipeline.IsRunning)
    {
        var failed = await pipeline.TryProcessNextAsync();
        if (failed != null)
        {
            await dlq.EnqueueAsync(failed);
        }

        // Periodically attempt retries
        var batch = await dlq.DequeueForRetryAsync();
        foreach (var entry in batch)
        {
            if (await TryRecover(entry))
            {
                await dlq.AcknowledgeSuccessAsync();
            }
            else
            {
                await dlq.AcknowledgeFailureAsync();
            }
        }

        await Task.Delay(TimeSpan.FromSeconds(30));
    }
}
```

## Notes
- All public methods are safe to invoke concurrently from multiple threads; internal synchronization ensures consistent state.
- If the queue is disposed, any subsequent call to an instance method will throw `ObjectDisposedException`.
- `PeekAsync` returns a snapshot that reflects the queue state at the moment the task starts; concurrent enqueues or dequeues may change the actual contents after the snapshot is taken.
- `DequeueForRetryAsync` removes entries from the queue; if the operation fails after removal, those entries are lost unless the implementation retries internally (consult the specific provider’s documentation).
- The acknowledgment methods (`AcknowledgeFailureAsync` and `AcknowledgeSuccessAsync`) assume they are called after a successful `DequeueForRetryAsync` for the same entry; calling them without a prior dequeue results in `InvalidOperationException`.
- The `DeadLetterQueueStats` structure returned by `GetStatsAsync` includes at least `CurrentDepth` (number of entries stored) and `FailedCount` (cumulative number of entries that have been acknowledged as failed). Additional metrics may be present depending on the underlying storage.
