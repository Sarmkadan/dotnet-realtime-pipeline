# DeadLetterEntry

Represents a single item that has failed during processing in the real‑time pipeline and has been moved to the dead‑letter queue for inspection, possible retries, or permanent failure handling. The type captures all contextual information about the failure, tracks retry state, and provides operations to initiate or record the outcome of a retry attempt.

## API

- **Guid EntryId**  
  Unique identifier for the dead‑letter entry. Assigned when the entry is created and never changes.

- **DataPoint DataPoint**  
  The original data payload that caused the failure. Provides access to the full message or record that was being processed.

- **string FailureStageName**  
  Name of the pipeline stage (e.g., “Validation”, “Enrichment”) where the failure occurred.

- **string FailureReason**  
  Human‑readable description of why the processing failed (e.g., “Schema mismatch”, “Timeout”).

- **string? ExceptionType**  
  Fully qualified name of the exception type thrown during processing, if any; otherwise `null`.

- **string? ExceptionMessage**  
  Message associated with the captured exception, if any; otherwise `null`.

- **int RetryCount**  
  Number of retry attempts that have already been made for this entry. Starts at `0` and is incremented by `BeginRetry`.

- **int MaxRetries**  
  Maximum number of retry attempts allowed before the entry is considered a permanent failure. Set at creation time and immutable.

- **DateTime EnqueuedAt**  
  Timestamp (UTC) when the entry was first placed into the dead‑letter queue.

- **DateTime? LastRetryAt**  
  Timestamp (UTC) of the most recent retry attempt; `null` if no retries have been performed.

- **DateTime? ResolvedAt**  
  Timestamp (UTC) when the entry was finally resolved (either succeeded after retry or marked as permanent failure); `null` while still pending.

- **DeadLetterStatus Status**  
  Current lifecycle state of the entry. Possible values are `Pending`, `InRetry`, `ResolvedSuccess`, `PermanentFailure`.

- **string? ResolutionNote**  
  Optional free‑form note added when the entry is resolved, describing the outcome or any manual intervention applied.

- **void BeginRetry()**  
  Prepares the entry for another retry attempt. Increments `RetryCount`, sets `LastRetryAt` to the current UTC time, and transitions `Status` to `InRetry`.  
  **Throws:**  
  - `InvalidOperationException` if `RetryCount >= MaxRetries` (no retries left) or if `Status` is not `Pending`.

- **void RetryFailed()**  
  Records that a retry attempt has failed. Leaves `RetryCount` unchanged, updates `LastRetryAt` to the current UTC time, and sets `Status` back to `Pending` if further retries remain, or to `PermanentFailure` if the retry limit has been exceeded.  
  **Throws:**  
  - `InvalidOperationException` if called when `Status` is not `InRetry`.

- **string GetSummary()**  
  Returns a concise, human‑readable string summarizing the entry, including `EntryId`, `FailureStageName`, `RetryCount`, and current `Status`.  
  **Return value:** A non‑null string suitable for logging or display.  
  **Throws:** None.

- **static int TotalEntries**  
  Total number of `DeadLetterEntry` instances that have been created since the application started. Updated atomically on construction.

- **static int PendingEntries**  
  Number of entries currently in the `Pending` state (available for retry). Updated when `Status` changes to or from `Pending`.

- **static int InRetryEntries**  
  Number of entries currently in the `InRetry` state (a retry is in progress). Updated when `Status` changes to or from `InRetry`.

- **static int PermanentFailureEntries**  
  Number of entries that have reached the `PermanentFailure` state. Updated when `Status` becomes `PermanentFailure`.

## Usage

```csharp
// Example 1: Creating a dead‑letter entry and attempting a retry
var deadLetter = new DeadLetterEntry
{
    EntryId = Guid.NewGuid(),
    DataPoint = failedDataPoint,
    FailureStageName = "Enrichment",
    FailureReason = "Missing required field",
    ExceptionType = typeof(ArgumentNullException).FullName,
    ExceptionMessage = "Value cannot be null.",
    MaxRetries = 3,
    EnqueuedAt = DateTime.UtcNow,
    Status = DeadLetterStatus.Pending
};

if (deadLetter.RetryCount < deadLetter.MaxRetries)
{
    deadLetter.BeginRetry(); // moves status to InRetry, updates timestamps
    // ... perform retry logic ...
    if (retrySucceeded)
    {
        deadLetter.Status = DeadLetterStatus.ResolvedSuccess;
        deadLetter.ResolvedAt = DateTime.UtcNow;
        deadLetter.ResolutionNote = "Retry succeeded after correcting input.";
    }
    else
    {
        deadLetter.RetryFailed(); // updates status based on remaining retries
    }
}

// Example 2: Inspecting aggregated dead‑letter statistics
Console.WriteLine($"Total dead‑letter entries: {DeadLetterEntry.TotalEntries}");
Console.WriteLine($"Pending: {DeadLetterEntry.PendingEntries}");
Console.WriteLine($"In retry: {DeadLetterEntry.InRetryEntries}");
Console.WriteLine($"Permanent failures: {DeadLetterEntry.PermanentFailureEntries}");

// Example 3: Getting a summary for logging
var summary = deadLetter.GetSummary();
logger.Info($"Dead‑letter summary: {summary}");
```

## Notes

- All mutable fields (`RetryCount`, `LastRetryAt`, `ResolvedAt`, `Status`, `ResolutionNote`) are not thread‑safe. Concurrent access from multiple threads should be synchronized externally (e.g., using locks or `Interlocked` operations where appropriate).  
- The static counters (`TotalEntries`, `PendingEntries`, `InRetryEntries`, `PermanentFailureEntries`) are updated atomically in the constructor and status‑change methods; they can be read safely without additional synchronization.  
- `BeginRetry` will throw if the entry has already exhausted its retry allowance or is not in a `Pending` state; callers should check `RetryCount < MaxRetries` and `Status == Pending` before invoking.  
- `RetryFailed` assumes the most recent retry attempt has just finished unsuccessfully; calling it when the entry is not currently `InRetry` results in an `InvalidOperationException`.  
- Nullable string members (`ExceptionType`, `ExceptionMessage`, `ResolutionNote`) may be `null` when no exception was captured or no note has been supplied; consumers should handle null values accordingly.  
- `GetSummary` is designed to never throw; it formats available data safely, substituting empty strings for null optional fields.  
- The `DataPoint` property is assumed to be a reference type supplied by the pipeline; the dead‑letter entry does not take ownership of the object and does not modify it.  
- When an entry transitions to `PermanentFailure`, `ResolvedAt` is set to the time of that transition and `ResolutionNote` may be used to record the reason for giving up on retries.  
- The `DeadLetterStatus` enum is expected to be defined elsewhere in the project; the documentation reflects the values used by the current implementation.
