# Backpressure Strategies

This page explains what backpressure is, which strategy to choose, and the trade-offs involved.  All runnable examples reference the public API exposed by `BackpressureService` and `BackpressureContext`.

---

## What is backpressure?

Backpressure is the mechanism by which a **slow consumer** signals to a **fast producer** that it should slow down or stop producing.  Without it, a pipeline's internal buffers fill up and either the process runs out of memory or data is silently discarded.

```
Producer → [Buffer (capacity N)] → Consumer
              ↑ backpressure signal
```

The pipeline tracks buffer fill percentage via `BackpressureContext.GetBufferFillPercentage()` and activates backpressure automatically when the fill level crosses `BackpressureTriggerThreshold` (default: 80 %).

---

## Available strategies

| Strategy | `BackpressureStrategy` value | What happens when the buffer is full |
|---|---|---|
| **Block** | `Block` | The write call waits until space is available. |
| **Throttle** | `Throttle` | The producer is slowed with a short delay; partial writes are allowed. |
| **Drop newest** | `DropNewest` | Incoming items are discarded; existing buffer content is preserved. |
| **Drop oldest** | `DropOldest` | The oldest buffered items are evicted to make room for new ones. |
| **Queue** | `Queue` | Overflow is queued in a secondary (unbounded) structure — memory grows. |

---

## Decision flowchart

```
Can you afford to lose data?
├─ No → Can the producer be slowed without cascading failures?
│        ├─ Yes → Block  (latency grows; throughput is preserved)
│        └─ No  → Queue  (memory grows; add an alerting threshold)
└─ Yes → Is the most recent data more valuable than older data?
          ├─ Yes → DropOldest  (sliding-window / last-value semantics)
          └─ No  → DropNewest  (preserve ordered history; discard bursts)
```

If you want to soften the impact before a hard block, set a lower `BackpressureTriggerThreshold` (e.g. 60 %) and pair it with `Throttle` to slow producers gradually.

---

## Latency vs. throughput trade-offs

| Strategy | Latency | Throughput | Data loss risk | Memory risk |
|---|---|---|---|---|
| Block | High under load | Preserved | None | Low |
| Throttle | Moderate | Slightly reduced | Very low | Low |
| DropNewest | Low | High | Medium–high | Low |
| DropOldest | Low | High | Medium–high | Low |
| Queue | Low initially | High initially | None | **High** |

> **Key insight — `Block` with a synchronous consumer will deadlock.**  
> If the consumer and producer share the same thread or a single-threaded `SynchronizationContext`, `Block` will suspend the producer indefinitely.  Always ensure the consumer runs on a separate task/thread before choosing `Block`.

---

## Monitoring data loss

When a buffer overflows, `BackpressureContext.DroppedItemCount` is incremented instead of silently discarding data.  Poll this counter to detect loss:

```csharp
var status = backpressureService.GetSystemStatus();
if (status.TotalDroppedItems > 0)
{
    logger.LogWarning("Data loss detected: {Count} items dropped across all stages",
        status.TotalDroppedItems);
}

// Or per stage:
long dropped = backpressureService.GetDroppedItemCount("Ingestion");
```

---

## Runnable examples

### Block — wait for space (zero data loss, higher latency)

```csharp
// Configure
var config = new PipelineConfig { MaxBufferSize = 1_024 };
var service = new BackpressureService();
service.CreateContext("Ingestion", config.MaxBufferSize);

// Write loop
while (true)
{
    bool accepted = service.TryAddToBuffer("Ingestion", 1);
    if (!accepted)
    {
        // Buffer full — wait before retrying (honours BoundedChannelFullMode.Wait semantics)
        var response = await service.ApplyBackpressureAsync(
            "Ingestion", BackpressureStrategy.Block, timeoutMs: 5_000);
        // Retry after the delay
        continue;
    }
    // ... process item
}
```

### DropNewest — discard bursts, keep history

```csharp
bool accepted = service.TryAddToBuffer("Ingestion", 1);
if (!accepted)
{
    // Item is dropped; the existing buffer is untouched.
    // DroppedItemCount is already incremented by TryAddToBuffer.
    metrics.IncrementCounter("pipeline.dropped");
    return;   // skip this item
}
```

### DropOldest — always keep the most recent data

```csharp
// The caller is responsible for evicting old items before adding new ones.
bool accepted = service.TryAddToBuffer("Ingestion", 1);
if (!accepted)
{
    // Evict the oldest item first, then retry.
    service.RemoveFromBuffer("Ingestion", 1);
    service.TryAddToBuffer("Ingestion", 1);
}
```

### Throttle — soften load spikes

```csharp
// Apply throttle when the buffer exceeds the trigger threshold.
if (service.IsBackpressured("Ingestion"))
{
    await service.ApplyBackpressureAsync(
        "Ingestion", BackpressureStrategy.Throttle, timeoutMs: 500);
    // Processing continues after the short delay; no items are dropped.
}
```

---

## When NOT to use `BoundedChannelFullMode.Wait`

The `Block` strategy relies on the consumer reading from the channel concurrently.  Common pitfalls:

* **Single-threaded consumers** — the await in `ApplyBackpressureAsync` suspends the producer but the consumer never gets scheduled.  Use `Task.Run(() => ConsumeAsync())` to guarantee a separate thread.
* **High-throughput, latency-sensitive paths** — `Block` adds queuing delay that compounds under sustained load.  Prefer `DropNewest` with an alerting threshold instead.
* **Containerised deployments with frequent NTP corrections** — window-boundary timing is already handled with a monotonic clock (`Stopwatch.GetTimestamp()`), but producer logic that sleeps based on wall-clock time can still be affected.

---

## Further reading

* [Architecture overview](architecture.md)
* [Performance tuning guide](PERFORMANCE.md)
* [API reference — BackpressureService](api-reference.md)
