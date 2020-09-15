# DeadLetterQueueTests

Unit tests for the Dead Letter Queue (DLQ) component that handles failed or unprocessable messages in the dotnet-realtime-pipeline system. These tests validate the core behaviors of enqueueing, peeking, dequeuing for retry, acknowledging success/failure, and statistical reporting under various conditions including capacity limits and retry exhaustion.

## API

### `EnqueueAsync_ValidEntry_IncreasesCount`
Validates that a correctly formed entry increases the DLQ's item count. The test constructs a valid `DeadLetterEntry` and asserts that the queue's internal counter increments after enqueueing.

### `EnqueueAsync_NullDataPoint_Throws`
Ensures that attempting to enqueue a `DeadLetterEntry` with a null `DataPoint` throws an `ArgumentNullException`. This enforces data integrity by rejecting entries without payloads.

### `EnqueueAsync_EmptyReason_Throws`
Confirms that enqueueing a `DeadLetterEntry` with an empty or whitespace `Reason` throws an `ArgumentException`. A descriptive reason is required for operational observability.

### `EnqueueAsync_WithException_StoresExceptionInfo`
Verifies that when a `DeadLetterEntry` is enqueued with an associated `Exception`, the exception details are preserved in the entry's `Exception` property and can be retrieved later.

### `PeekAsync_DoesNotRemoveEntries`
Checks that calling `PeekAsync` returns the next available entry without removing it from the queue. The queue's item count should remain unchanged after peeking.

### `DequeueForRetryAsync_ReturnsPendingEntries`
Validates that `DequeueForRetryAsync` returns entries that are eligible for retry (i.e., have remaining retries and are not marked as permanent failures). The returned entries are removed from the queue.

### `DequeueForRetryAsync_EntryExhaustedRetries_NotReturned`
Ensures that entries with no remaining retries are not returned by `DequeueForRetryAsync`, even if they have not been acknowledged as permanent failures. This prevents wasted processing cycles on exhausted entries.

### `AcknowledgeSuccessAsync_RemovesEntry`
Confirms that calling `AcknowledgeSuccessAsync` on a dequeued entry removes it from the DLQ. The entry should no longer be visible via `PeekAsync` or `DequeueForRetryAsync`.

### `AcknowledgeFailureAsync_MarksAsPermanentFailure`
Validates that `AcknowledgeFailureAsync` marks a dequeued entry as a permanent failure, preventing further retries. The entry remains in the queue but is no longer eligible for retry.

### `GetStatsAsync_ReflectsCurrentState`
Ensures that `GetStatsAsync` returns accurate statistics reflecting the current state of the DLQ, including total entries, pending retries, and permanent failures.

### `Enqueue_WhenAtCapacity_EvictsOldest`
Tests that when the DLQ reaches its configured capacity, the oldest entry is evicted to make room for the new entry. This behavior enforces a bounded queue size.

### `DeadLetterEntry_CanRetry_TrueWhenUnderBudget`
Validates that `DeadLetterEntry.CanRetry` returns `true` when the entry has remaining retries within its retry budget. This method is used by the queue to determine eligibility.

### `DeadLetterEntry_CanRetry_FalseWhenExhausted`
Confirms that `DeadLetterEntry.CanRetry` returns `false` when the entry's retry count has been exhausted. Such entries should not be dequeued for retry.

## Usage

### Example 1: Basic DLQ Workflow
