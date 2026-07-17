# DeadLetterQueueExtensions

Provides extension methods for querying, reprocessing, and reporting on messages that have been routed to the dead-letter queue within the `dotnet-realtime-pipeline`. This type enables inspection of failed messages by stage, batch retry operations, and summary reporting, exposing processing counts and the entries that were handled during a retry cycle.

## API

### ProcessForRetryAsync

```csharp
public static async Task<DeadLetterProcessingResult> ProcessForRetryAsync(
    this IPipelineContext context,
    Func<DeadLetterEntry, Task<bool>> retryHandler,
    CancellationToken cancellationToken = default)
```

Attempts to reprocess dead-lettered entries by invoking a caller-supplied handler for each entry. The handler returns `true` if the entry was successfully reprocessed, `false` otherwise. The method returns a `DeadLetterProcessingResult` that aggregates the outcome counts and the list of entries that were processed.

**Parameters**
- `context` (`IPipelineContext`): The pipeline context providing access to the dead letter store.
- `retryHandler` (`Func<DeadLetterEntry, Task<bool>>`): An asynchronous function that receives a dead letter entry and returns `true` when reprocessing succeeds.
- `cancellationToken` (`CancellationToken`): A token to cancel the retry loop.

**Returns**
`DeadLetterProcessingResult` containing `TotalProcessed`, `SuccessfullyProcessed`, `FailedProcessing`, and the `EntriesProcessed` collection.

**Exceptions**
- `ArgumentNullException` if `context` or `retryHandler` is `null`.
- `OperationCanceledException` if the cancellation token is signaled before the operation completes.

---

### FindAsync

```csharp
public static async Task<IReadOnlyList<DeadLetterEntry>> FindAsync(
    this IPipelineContext context,
    DateTimeOffset from,
    DateTimeOffset to,
    CancellationToken cancellationToken = default)
```

Retrieves all dead letter entries within a given time window.

**Parameters:**
- `context` (`IPipelineContext`): The pipeline context.
- `from` (`DateTimeOffset`): Inclusive start of the time window.
- `to` (`DateTimeOffset`): Exclusive end of the time window.
- `cancellationToken` (`CancellationToken`): Cancellation token.

**Returns**
An `IReadOnlyList<DeadLetterEntry>` containing the matching entries.

**Exceptions**
- `ArgumentNullException` if `context` is `null`.
- `ArgumentOutOfRangeException` if `from` is later than `to`.

---

### FindByStageAsync

```csharp
public static async Task<IReadOnlyList<DeadLetterEntry>> FindByStageAsync(
    this IPipelineContext context,
    string stageName,
    DateTimeOffset from,
    DateTimeOffset to,
    CancellationToken cancellationToken = default)
```

Retrieves dead letter entries for a specific pipeline stage within a time window.

**Parameters:**
- `context` (`IPipelineContext`): The pipeline context.
- `stageName` (`string`): The name of the pipeline stage to filter by.
- `from` (`DateTimeOffset`): Inclusive start of the time window.
- `to` (`DateTimeOffset`): Exclusive end of the time window.
- `cancellationToken` (`CancellationToken`): Cancellation token.

**Returns**
An `IReadOnlyList<DeadLetterEntry>` filtered by stage and time range.

**Exceptions**
- `ArgumentNullException` if `context` or `stageName` is `null`.
- `ArgumentOutOfRangeException` if `from` is later than `to`.

---

### GetReportAsync

```csharp
public static async Task<string> GetReportAsync(
    this IPipelineContext context,
    CancellationToken cancellationToken = default)
```

Generates a human-readable report summarizing the current state of the dead letter queue, including counts by stage and the age distribution of entries.

**Parameters:**
- `context` (`IPipelineContext`): The pipeline context.
- `cancellationToken` (`CancellationToken`): Cancellation token.

**Returns**
A `string` containing the formatted report.

**Exceptions**
- `ArgumentNullException` if `context` is `null`.

---

### TotalProcessed

```csharp
public int TotalProcessed { get; }
```

The total number of dead letter entries that were attempted during the most recent `ProcessForRetryAsync` call.

---

### SuccessfullyProcessed

```csharp
public int SuccessfullyProcessed { get; }
```

The number of entries that were successfully reprocessed during the most recent retry operation.

---

### FailedProcessing

```csharp
public int FailedProcessing { get; }
```

The number of entries that failed reprocessing during the most recent retry operation.

---

### EntriesProcessed

```csharp
public IReadOnlyList<DeadLetterEntry> EntriesProcessed { get; }
```

The collection of dead letter entries that were handled in the most recent retry cycle. This list includes both successful and failed entries.

## Usage

### 1. Querying and retrying dead letter entries for a specific stage

```csharp
var from = DateTimeOffset.UtcNow.AddHours(-1);
var to = DateTimeOffset.UtcNow;

IReadOnlyList<DeadLetterEntry> entries = await pipelineContext.FindByStageAsync(
    "order-processing",
    from,
    to);

if (entries.Count > 0)
{
    var result = await DeadLetterQueueExtensions.ProcessForRetryAsync(
        pipelineContext,
        async entry =>
        {
            // Replay the message to the target handler
            return await messageHandler.TryReprocessAsync(entry.Payload);
        });

    Console.WriteLine(
        $"Retried {result.TotalProcessed} entries: " +
        $"{result.SuccessfullyProcessed} succeeded, " +
        $"{result.FailedProcessing} failed.");
}
```

### 2. Generate a report and inspect the last retry outcome

```csharp
string report = await pipelineContext.GetReportAsync();
Console.WriteLine(report);

// After a retry cycle, inspect the properties directly
Console.WriteLine($"Last retry totals - Processed: {DeadLetterQueueExtensions.TotalProcessed}, " +
                  $"Succeeded: {DeadLetterQueueExtensions.SuccessfullyProcessed}, " +
                  $"Failed: {DeadLetterQueueExtensions.FailedProcessing}");

foreach (var entry in DeadLetterQueueExtensions.EntriesProcessed)
{
    Console.WriteLine($"Entry {entry.Id} - Stage: {entry.StageName}, Error: {entry.ErrorMessage}");
}
```

## Notes

- The `TotalProcessed`, `SuccessfullyProcessed`, `FailedProcessing`, and `EntriesProcessed` properties reflect the outcome of the **most recent** call to `ProcessForRetryAsync`. They are overwritten on each invocation and are not cumulative across multiple retry cycles.
- These properties are static and therefore shared across all callers in the same process. In concurrent scenarios, reading them immediately after a retry call is safe only if no other thread is simultaneously executing `ProcessForRetryAsync`. For accurate per-operation tracking, use the `DeadLetterProcessingResult` returned by the method rather than relying on the static properties.
- `FindAsync` and `FindByStageAsync` return read-only lists. Modifying the returned collection will throw at runtime; treat them as snapshots.
- `GetReportAsync` may be expensive on queues with a large number of entries. Avoid calling it in hot paths.
- All methods that accept an `IPipelineContext` will throw `ArgumentNullException` if the context is `null`. Time-window methods additionally validate that `from` is not later than `to`.
