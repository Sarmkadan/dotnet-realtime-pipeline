# IDeadLetterQueue Interface

Abstracts the dead-letter queue so implementations can be swapped (in-memory, database-backed, external broker, etc.) without changing the calling code.

## Methods

### EnqueueAsync
```csharp
Task EnqueueAsync(DataPoint dataPoint, string stageName, string failureReason, Exception? exception = null, int attemptsMade = 1);
```
Enqueues a failed data point with contextual failure information.

**Parameters:**
- `dataPoint`: The data point that failed processing.
- `stageName`: The name of the pipeline stage where the failure occurred.
- `failureReason`: The reason for the failure.
- `exception`: The exception that caused the failure, if any.
- `attemptsMade`: The number of in-line processing attempts made before dead-lettering.

### ReplayAsync
```csharp
Task<int> ReplayAsync(Func<DeadLetterEntry, bool> filter);
```
Requeues entries matching `<paramref name="filter"/>` for another round of retries: their status is reset to `<see cref="DeadLetterStatus.Pending"/>` and the retry counter is cleared, so operators can replay transient failures.

**Parameters:**
- `filter`: Selects which entries to replay (e.g. by stage or exception type).

**Returns:**
The number of entries that were reset for replay.

**Exceptions:**
- `ArgumentNullException`: Thrown when `<paramref name="filter"/>` is `<see langword="null"/>`.

### PeekAsync
```csharp
Task<IReadOnlyList<DeadLetterEntry>> PeekAsync(int maxCount = 100);
```
Returns up to `<paramref name="maxCount"/>` entries from the queue without removing them.

**Parameters:**
- `maxCount`: The maximum number of entries to peek.

**Returns:**
List of dead letter entries.

### DequeueForRetryAsync
```csharp
Task<IReadOnlyList<DeadLetterEntry>> DequeueForRetryAsync(int maxCount = 10);
```
Returns and removes up to `<paramref name="maxCount"/>` entries ready for re-processing.

**Parameters:**
- `maxCount`: The maximum number of entries to dequeue for retry.

**Returns:**
List of dead letter entries ready for retry.

### AcknowledgeFailureAsync
```csharp
Task AcknowledgeFailureAsync(Guid entryId, string finalReason);
```
Marks an entry as permanently failed (exhausted retries or non-retryable error).

**Parameters:**
- `entryId`: The identifier of the entry to acknowledge.
- `finalReason`: The final reason for the permanent failure.

### AcknowledgeSuccessAsync
```csharp
Task AcknowledgeSuccessAsync(Guid entryId);
```
Marks an entry as successfully reprocessed and removes it from the queue.

**Parameters:**
- `entryId`: The identifier of the entry to acknowledge.

### GetStatsAsync
```csharp
Task<DeadLetterQueueStats> GetStatsAsync();
```
Returns queue statistics.

**Returns:**
Queue statistics including counts and totals.

### Count
```csharp
int Count { get; }
```
Gets the total number of entries currently waiting in the queue (pending + failed-permanently).

## Entry Lifecycle

The dead letter queue entry follows this lifecycle:

1. **Enqueued**: A failed data point is added to the queue via `EnqueueAsync`.
2. **Dequeued for Retry**: An operator retrieves entries ready for reprocessing using `DequeueForRetryAsync`. At this point, the entry is removed from the queue.
3. **Acknowledged**:
   - **Success**: If reprocessing succeeds, the operator calls `AcknowledgeSuccessAsync` to confirm the entry was processed correctly (no further action needed).
   - **Failure**: If reprocessing fails permanently, the operator calls `AcknowledgeFailureAsync` with a final reason, marking the entry as permanently failed.

Entries can also be replayed via `ReplayAsync`, which resets their status to `Pending` and clears the retry counter, allowing them to be dequeued again for another retry attempt.

## Implementation Reference

For the in-memory implementation of this interface, see [DeadLetterQueue.md](DeadLetterQueue.md).