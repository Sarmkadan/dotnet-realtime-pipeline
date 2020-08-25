# BackpressureContext

Represents the runtime state and metrics of a backpressure-aware pipeline stage. It tracks buffer occupancy, consumer activity, dropped items, and backpressure duration, enabling stages to make informed decisions about whether to apply flow control.

## API

### Constructors

- **`BackpressureContext()`**  
  Initializes a new instance with default values. The buffer metrics dictionary and backpressure event timestamp queue are empty, counters are zeroed, and timestamps are set to the current UTC time.

- **`BackpressureContext(long contextId, string pipelineStageName, long maxBufferCapacity, int maxConcurrentConsumers)`**  
  Initializes a new instance with the specified identity and capacity limits.  
  *Parameters*:  
  `contextId` — Unique identifier for this context instance.  
  `pipelineStageName` — Name of the pipeline stage this context belongs to.  
  `maxBufferCapacity` — Maximum number of items the buffer can hold before backpressure is triggered.  
  `maxConcurrentConsumers` — Maximum number of consumers allowed to process items concurrently.

### Properties

- **`long ContextId`**  
  Gets the unique identifier for this backpressure context.

- **`string PipelineStageName`**  
  Gets the name of the pipeline stage associated with this context.

- **`long BufferSize`**  
  Gets the current number of items in the buffer.

- **`long MaxBufferCapacity`**  
  Gets the maximum buffer capacity. When `BufferSize` approaches this value, backpressure may be applied.

- **`bool IsBackpressured`**  
  Gets whether the stage is currently applying backpressure (i.e., rejecting or delaying new items).

- **`long BackpressureStartTimeMs`**  
  Gets the epoch timestamp in milliseconds when the current backpressure period began. Returns `0` if not currently backpressured.

- **`long TotalBackpressureTimeMs`**  
  Gets the cumulative time in milliseconds the stage has spent in backpressure state across all periods.

- **`long DroppedItemCount`**  
  Gets the total number of items dropped due to buffer overflow or backpressure policies.

- **`int ActiveConsumers`**  
  Gets the current number of consumers actively processing items from this stage.

- **`int MaxConcurrentConsumers`**  
  Gets the maximum number of consumers allowed to process items concurrently.

- **`DateTime CreatedAt`**  
  Gets the UTC timestamp when this context was created.

- **`DateTime LastUpdatedAt`**  
  Gets the UTC timestamp when any mutable field was last modified.

- **`Queue<long> BackpressureEventTimestamps`**  
  Gets a queue of epoch millisecond timestamps recording when backpressure was activated. Each entry corresponds to the start of a backpressure period.

- **`Dictionary<string, long> BufferMetrics`**  
  Gets a dictionary of named metrics related to buffer behavior (e.g., peak occupancy, average flush time). Keys and values are implementation-defined.

### Methods

- **`double GetBufferFillPercentage()`**  
  Returns the current buffer fill level as a percentage (`0.0` to `100.0`).  
  *Returns*: `(BufferSize / MaxBufferCapacity) * 100.0`. Returns `0.0` if `MaxBufferCapacity` is `0`.

- **`bool ShouldApplyBackpressure()`**  
  Determines whether backpressure should be applied based on current buffer fill and consumer availability.  
  *Returns*: `true` if the buffer fill percentage exceeds a threshold or `ActiveConsumers` equals `MaxConcurrentConsumers`; otherwise `false`.  
  *Remarks*: The threshold is typically 80% but may vary by implementation. This method does not mutate state.

- **`bool TryAddToBuffer(long itemCount)`**  
  Attempts to reserve space for the specified number of items in the buffer.  
  *Parameters*: `itemCount` — Number of items to add. Must be non-negative.  
  *Returns*: `true` if the items were accepted and `BufferSize` was incremented; `false` if adding would exceed `MaxBufferCapacity`.  
  *Throws*: `ArgumentOutOfRangeException` when `itemCount` is negative.

- **`void RemoveFromBuffer(long itemCount)`**  
  Decrements `BufferSize` by the specified number of items, representing consumption or flushing.  
  *Parameters*: `itemCount` — Number of items removed. Must be non-negative.  
  *Throws*: `ArgumentOutOfRangeException` when `itemCount` is negative.  
  *Remarks*: If `itemCount` exceeds `BufferSize`, `BufferSize` is clamped to `0`. No exception is thrown for over-removal.

## Usage

### Example 1: Basic Producer-Consumer Flow Control

```csharp
var ctx = new BackpressureContext(
    contextId: 1,
    pipelineStageName: "DataEnrichment",
    maxBufferCapacity: 1000,
    maxConcurrentConsumers: 4
);

// Producer checks before enqueuing
if (ctx.ShouldApplyBackpressure())
{
    // Slow down or redirect
    await Task.Delay(100);
}

if (ctx.TryAddToBuffer(itemCount: 10))
{
    // Items accepted; proceed with enqueue
    Console.WriteLine($"Buffer at {ctx.GetBufferFillPercentage():F1}%");
}
else
{
    // Buffer full; drop or persist to dead-letter store
    ctx.DroppedItemCount += 10; // manual tracking if not handled internally
}
```

### Example 2: Monitoring and Consumer Tracking

```csharp
var ctx = new BackpressureContext(2, "ValidationStage", 500, 2);

// Consumer starts work
ctx.ActiveConsumers++;

// Process batch
ctx.RemoveFromBuffer(itemCount: 25);

// Check backpressure state for metrics dashboard
if (ctx.IsBackpressured)
{
    var currentDuration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - ctx.BackpressureStartTimeMs;
    Console.WriteLine($"Backpressure active for {currentDuration}ms");
}

// Record custom buffer metric
ctx.BufferMetrics["PeakObserved"] = Math.Max(
    ctx.BufferMetrics.GetValueOrDefault("PeakObserved", 0),
    ctx.BufferSize
);

// Consumer finishes
ctx.ActiveConsumers--;
```

## Notes

- **Thread Safety**: This type is not inherently thread-safe. Concurrent calls to `TryAddToBuffer`, `RemoveFromBuffer`, and property mutations (e.g., `ActiveConsumers`, `IsBackpressured`) must be synchronized externally if accessed from multiple threads.
- **Buffer Over-Removal**: `RemoveFromBuffer` clamps `BufferSize` to `0` rather than throwing when `itemCount` exceeds the current size. Callers tracking exact counts should validate before calling.
- **Backpressure State Transitions**: `IsBackpressured` and `BackpressureStartTimeMs` are not automatically toggled by `TryAddToBuffer` or `RemoveFromBuffer`. The owning pipeline stage is responsible for setting these fields when thresholds are crossed.
- **Timestamp Queue**: `BackpressureEventTimestamps` accumulates entries over the context lifetime. Long-running stages should periodically drain or cap this queue to avoid unbounded memory growth.
- **Percentage Calculation**: `GetBufferFillPercentage` guards against division by zero when `MaxBufferCapacity` is `0`, returning `0.0` in that degenerate case.
- **Dropped Items**: `DroppedItemCount` is a mutable field exposed for manual incrementing. It is not automatically updated by `TryAddToBuffer` returning `false`.
