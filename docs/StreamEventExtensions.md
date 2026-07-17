# StreamEventExtensions

Provides a set of static extension methods for inspecting, transforming, and extracting data from `StreamEvent` instances within the real-time pipeline. These utilities cover payload filtering, typed payload retrieval, stage-processing diagnostics, deep copying, staleness checks, and serialization helpers, enabling consistent event handling across pipeline stages without coupling to the internal structure of `StreamEvent`.

## API

### FilterPayload
```csharp
public static Dictionary<string, object> FilterPayload(
    this StreamEvent streamEvent,
    params string[] allowedKeys)
```
Returns a new dictionary containing only the entries from the event's payload whose keys match the specified `allowedKeys`. When `allowedKeys` is null or empty, an empty dictionary is returned. Throws `ArgumentNullException` if `streamEvent` is null.

### GetPayload\<T\>
```csharp
public static T? GetPayload<T>(
    this StreamEvent streamEvent)
    where T : class
```
Deserializes the event's payload into an instance of `T`. Returns `null` if the payload is missing, empty, or cannot be deserialized to the requested type. Throws `ArgumentNullException` if `streamEvent` is null. Throws `JsonException` only when the payload is present but structurally incompatible with `T` and strict deserialization is configured.

### HasBeenProcessedByAnyStage
```csharp
public static bool HasBeenProcessedByAnyStage(
    this StreamEvent streamEvent)
```
Returns `true` if the event has passed through at least one processing stage (i.e., its stage-processing history is non-empty). Returns `false` for events that have not yet entered the pipeline or whose history has been cleared. Throws `ArgumentNullException` if `streamEvent` is null.

### GetRemainingStagesCount
```csharp
public static int GetRemainingStagesCount(
    this StreamEvent streamEvent)
```
Calculates the number of pipeline stages still pending for this event based on the total configured stages minus those already recorded as processed. Returns zero if the event has completed all stages or if no stage configuration is available. Throws `ArgumentNullException` if `streamEvent` is null.

### DeepCopy
```csharp
public static StreamEvent DeepCopy(
    this StreamEvent streamEvent)
```
Creates a fully independent clone of the `StreamEvent`, including its payload, metadata, headers, and stage-processing history. The returned copy shares no mutable references with the original. Throws `ArgumentNullException` if `streamEvent` is null. Throws `InvalidOperationException` if the event's payload stream is non-seekable and cannot be re-read for copying.

### IsStale
```csharp
public static bool IsStale(
    this StreamEvent streamEvent,
    TimeSpan maxAge)
```
Returns `true` if the event's ingestion timestamp plus `maxAge` is earlier than the current UTC time, indicating the event has exceeded its useful processing window. Returns `false` if the event has no ingestion timestamp. Throws `ArgumentNullException` if `streamEvent` is null.

### GetPriorityString
```csharp
public static string GetPriorityString(
    this StreamEvent streamEvent)
```
Returns a human-readable string representation of the event's priority level (e.g., "High", "Normal", "Low"). Returns "Unknown" if no priority is assigned. Throws `ArgumentNullException` if `streamEvent` is null.

### GetPayloadAsJson
```csharp
public static string? GetPayloadAsJson(
    this StreamEvent streamEvent)
```
Serializes the event's payload to a compact JSON string. Returns `null` if the payload is absent or empty. Throws `ArgumentNullException` if `streamEvent` is null. Throws `JsonException` if the payload contains non-serializable constructs.

### HasFailed
```csharp
public static bool HasFailed(
    this StreamEvent streamEvent)
```
Returns `true` if the event has recorded at least one processing failure in its stage history. Returns `false` for events with no failures or no processing history at all. Throws `ArgumentNullException` if `streamEvent` is null.

### GetProcessingCompletionPercentage
```csharp
public static int GetProcessingCompletionPercentage(
    this StreamEvent streamEvent)
```
Returns an integer from 0 to 100 representing the percentage of pipeline stages completed for this event. Returns 0 if no stages are configured or the event has not started processing. Returns 100 when all stages are completed. Throws `ArgumentNullException` if `streamEvent` is null.

## Usage

### Example 1: Filtering and Forwarding Sensitive Data
```csharp
StreamEvent incoming = pipeline.Receive();

// Retain only non-sensitive fields before forwarding to an external sink
var safePayload = incoming.FilterPayload("userId", "timestamp", "eventType");
var sanitized = incoming.DeepCopy();
sanitized.ReplacePayload(safePayload);

if (!sanitized.IsStale(TimeSpan.FromMinutes(5)))
{
    externalSink.Send(sanitized);
}
```

### Example 2: Monitoring Pipeline Progress and Failures
```csharp
foreach (var batch in pipeline.ReceiveBatch(100))
{
    foreach (var evt in batch)
    {
        if (evt.HasFailed())
        {
            logger.Warn($"Event {evt.Id} failed at {evt.GetProcessingCompletionPercentage()}% completion");
            deadLetterQueue.Enqueue(evt);
            continue;
        }

        var remaining = evt.GetRemainingStagesCount();
        if (remaining > 0)
        {
            metrics.Record($"Remaining stages: {remaining}, Priority: {evt.GetPriorityString()}");
        }
    }
}
```

## Notes

- All methods throw `ArgumentNullException` when the `streamEvent` argument is null; callers should guard at ingestion boundaries.
- `DeepCopy` requires the underlying payload stream to be seekable. Events backed by forward-only network streams will cause `InvalidOperationException`; buffer such events before copying.
- `IsStale` relies on the event's ingestion timestamp. Events without a timestamp always return `false`, which may hide staleness in manually constructed events.
- `GetPayload<T>` and `GetPayloadAsJson` internally access the payload stream. Repeated calls without resetting the stream position may yield empty results; the caller is responsible for stream position management if the event was constructed with a non-buffered stream.
- `GetRemainingStagesCount` and `GetProcessingCompletionPercentage` depend on a globally or contextually configured stage count. In dynamic pipelines where stages are added or removed at runtime, these values may not reflect the actual remaining work.
- These methods are not thread-safe. Concurrent access to the same `StreamEvent` instance, particularly during payload reads or deep copying, requires external synchronization.
