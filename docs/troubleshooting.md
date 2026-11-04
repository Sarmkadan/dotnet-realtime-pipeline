# Troubleshooting Guide

This document covers concrete symptoms and fixes drawn from the codebase for the `dotnet-realtime-pipeline` project.

---

## 1. Backpressure Activation

### Symptom
Pipeline slows down or stops processing, events appear stuck, or `IsBackpressured` returns `true` for a stage.

### Root Cause
Backpressure is triggered when a pipeline stage's buffer exceeds the configured threshold.

### Configuration
- **`PipelineConfig.BackpressureTriggerThreshold`** (default: `80.0`%) — the percentage of buffer capacity at which backpressure activates.
- **`PipelineConfig.MaxBufferSize`** (default: `10000`) — maximum number of items the buffer can hold.
- **`PipelineConfig.WindowSizeMs`** and **`PipelineConfig.WindowSlideMs`** — windowing parameters that affect buffer fill rate.

### How It Works
1. Items are added to a stage buffer via `BackpressureService.TryAddToBuffer()`.
2. When `newBufferSize > MaxBufferCapacity`, the oldest items are dropped and `DroppedItemCount` is incremented.
3. Backpressure is activated when the buffer fill percentage reaches the trigger threshold.
4. `BackpressureContext.ShouldApplyBackpressure(triggerThresholdPercent)` compares the current fill percentage against the threshold.
5. The system-wide status can be checked via `BackpressureService.GetSystemStatus().IsSystemBackpressured`, which returns `true` when the average buffer fill exceeds `PipelineConstants.BackpressureHighWaterMark` (also `80.0`%).

### Fixes
- **Increase `MaxBufferSize`** in `PipelineConfig` if the buffer is consistently overflowing for your workload.
- **Raise `BackpressureTriggerThreshold`** (e.g., from `80.0` to `90.0`) if you want backpressure to activate later, giving more room before throttling.
- **Reduce `MaxConcurrentConsumers`** if backpressure is activating too aggressively — fewer consumers means slower buffer clearance.
- **Check `DroppedItemCount`** per stage via `BackpressureService.GetDroppedItemCount(stageName)` to measure data loss.

### Health Status
- `BackpressureContext.GetHealthStatus()` returns `"BACKPRESSURED"`, `"HIGH_LOAD"`, `"MODERATE_LOAD"`, or `"HEALTHY"`.
- `BackpressureSystemStatus.GetHealthStatus()` returns `"CRITICAL"`, `"WARNING"`, `"CAUTION"`, or `"HEALTHY"` based on system-wide average buffer fill.

---

## 2. Dead-Letter Queue Growth

### Symptom
Dead-letter queue size grows unbounded, consuming memory, or entries are being evicted unexpectedly.

### Root Cause
The dead-letter queue has a configurable capacity limit. When full, it evicts entries to make room.

### Configuration
- **`DeadLetterQueue(maxCapacity, defaultMaxRetries)`** — `maxCapacity` defaults to `1000` entries.
- **`DeadLetterQueue.DefaultMaxRetries`** defaults to `3` retry attempts per entry.

### How It Works
1. When a processing stage fails, the failed `DataPoint` is enqueued via `DeadLetterQueue.EnqueueAsync()`.
2. On each `EnqueueAsync()` call, `EvictIfNeeded()` runs:
   - **First pass**: Removes all `Resolved` or `PermanentFailure` entries.
   - **Second pass**: If still at capacity, removes the oldest `Pending` entry by enqueue time.
3. Entries can be replayed via `ReplayAsync()`, acknowledged as success via `AcknowledgeSuccessAsync()`, or acknowledged as permanent failure via `AcknowledgeFailureAsync()`.

### Fixes
- **Increase `maxCapacity`** when constructing the `DeadLetterQueue` if you expect more failures:
  ```csharp
  var dlq = new DeadLetterQueue(maxCapacity: 5000); // instead of default 1000
  ```
- **Increase `defaultMaxRetries`** to give failed items more retry attempts before dead-lettering:
  ```csharp
  var dlq = new DeadLetterQueue(defaultMaxRetries: 5);
  ```
- **Monitor `DeadLetterQueueStats`** via `GetStatsAsync()` to track `PendingEntries`, `InRetryEntries`, and `PermanentFailureEntries`.
- **Regular cleanup**: Call `AcknowledgeSuccessAsync()` for successfully reprocessed entries, or `AcknowledgeFailureAsync()` for permanent failures to keep the queue lean.

### Eviction Behavior
- The queue prioritizes evicting resolved/permanent-failure entries first.
- Only when those are exhausted does it evict the oldest pending entry.
- This means `PermanentFailure` entries can accumulate if never acknowledged — ensure you call `AcknowledgeFailureAsync()` for known permanent failures.

---

## 3. Rate Limiting 429 Responses

### Symptom
HTTP clients receive `429 Too Many Requests` responses, or internal operations are being throttled unexpectedly.

### Root Cause
The `RateLimitingMiddleware` uses a token bucket algorithm to control request rates.

### Configuration
- **`RateLimitingMiddleware(tokensPerSecond, maxBurstSize)`** — defaults: `1000 tokens/sec`, `5000 max burst`.
- **`StageRateLimitingMiddleware.RegisterStageLimit(stageName, itemsPerSecond, burstSize)`** — per-stage rate limits.

### How It Works
1. Each identifier gets a `RateLimitBucket` with a token bucket.
2. Tokens are replenished at `_tokensPerSecond` per second, up to `_maxBurstSize` capacity.
3. `TryAcquire(tokensRequired)` consumes tokens. If insufficient tokens are available, it returns `false`.
4. A `false` return typically results in a `429` response to the caller.

### Fixes
- **Increase `tokensPerSecond`** if legitimate traffic is being throttled:
  ```csharp
  var middleware = new RateLimitingMiddleware(tokensPerSecond: 2000); // double the default
  ```
- **Increase `maxBurstSize`** to allow larger short-term bursts without immediate throttling.
- **Check `GetStatus(identifier)`** to see current `AvailableTokens` and `Capacity` for a specific identifier.
- **Use `Reset(identifier)`** to clear a specific bucket (e.g., after a maintenance window).
- **Configure per-stage limits** via `StageRateLimitingMiddleware` if different pipeline stages have different rate requirements.

### Status
- `RateLimitStatus.IsLimited` returns `true` when `AvailableTokens < Capacity / 2`.
- `GetAllStatuses()` provides a snapshot of all buckets' current state.

---

## 4. Retry Exhaustion

### Symptom
Failed operations are not being retried, or items are sent to the dead-letter queue prematurely. Alternatively, operations retry indefinitely without backoff.

### Root Cause
The `ExponentialBackoffRetryPolicy` has a maximum number of attempts and configurable delay parameters.

### Configuration
- **`ExponentialBackoffRetryPolicy(options)`** or **`RetryPolicyOptions`** with:
  - **`MaxAttempts`** (default: `3`) — maximum number of retry attempts (including the first attempt).
  - **`BaseDelay`** (default: `200ms`) — initial delay before the second attempt.
  - **`MaxDelay`** (default: `30 seconds`) — upper bound on computed delay.
  - **`JitterFactor`** (default: `0.25`) — random jitter ±25% of computed delay.

### How It Works
1. `ExecuteAsync(operation)` runs the operation in a loop up to `MaxAttempts` times.
2. After each failure, `GetBackoffDelay(attemptNumber)` computes: `min(baseDelay * 2^(attempt-1), maxDelay) ± jitter`.
3. The delay increases exponentially: attempt 1 → 200ms, attempt 2 → 400ms, attempt 3 → 800ms, etc., capped at `MaxDelay`.
4. Jitter adds randomness: with `JitterFactor=0.25`, delay could be 600-1000ms on attempt 3 (instead of exactly 800ms).
5. If the operation still fails after `MaxAttempts`, the result is a `Failure` and the item is handed to the dead-letter queue.

### Fixes
- **Increase `MaxAttempts`** if transient failures should have more retries:
  ```csharp
  var options = new RetryPolicyOptions { MaxAttempts = 5 };
  var policy = new ExponentialBackoffRetryPolicy(options);
  ```
- **Adjust `BaseDelay`** for shorter or longer initial delays.
- **Adjust `MaxDelay`** to cap or extend the maximum backoff time.
- **Adjust `JitterFactor`** — set to `0.0` for deterministic delays, or keep at `0.25` (default) for randomized timing that avoids thundering herd problems.
- **Configure transient exception predicate** via `TransientExceptionPredicate` to control which exceptions trigger retries (defaults to `TimeoutException`, `IOException`, `SocketException`, `HttpRequestException`).

### Example: Aggressive Retries
```csharp
var options = new RetryPolicyOptions
{
    MaxAttempts = 7,
    BaseDelay = TimeSpan.FromMilliseconds(100),
    MaxDelay = TimeSpan.FromSeconds(60),
    JitterFactor = 0.1
};
var policy = new ExponentialBackoffRetryPolicy(options);
```

---

## 5. Subscriber Queue Overflow

### Symptom
Events are being dropped when the subscriber queue is full, or subscribers are blocking indefinitely.

### Root Cause
Each subscriber channel uses a bounded channel with a configurable overflow behavior.

### Configuration
- **`SubscriberOptions`** with:
  - **`MaxQueueSize`** (default: `1000`) — maximum number of events the queue can hold.
  - **`MaxQueueSizeBehavior`** (default: `DropNew`) — what happens when the queue is full.
  - **`DispatchMode`** (default: `Sequential`) — sequential vs. parallel processing.
  - **`MaxDegreeOfParallelism`** (default: `1`) — number of parallel workers.

### How It Works
1. When `PipelineEventPublisher.Subscribe()` is called with `SubscriberOptions`, a `SubscriberChannel` is created with a `Channel<SubscriberWorkItem>` bounded by `MaxQueueSize`.
2. The `FullMode` of the bounded channel is set based on `MaxQueueSizeBehavior`:
   - **`DropNew`** (default): Newest events are dropped when the queue is full (`BoundedChannelFullMode.DropNewest`).
   - **`Block`**: Consumer waits (blocks) when the queue is full (`BoundedChannelFullMode.Wait`).
   - **`Reject`**: Oldest events are dropped when the queue is full (`BoundedChannelFullMode.DropOldest`).
3. `BackpressureAlertSubscriber` overrides `GetSubscriberOptions()` to use `MaxQueueSize = int.MaxValue` and `MaxQueueSizeBehavior = Block`, giving alerts an effectively unbounded, non-dropping queue.
4. `ErrorAlertSubscriber` similarly uses `MaxQueueSize = int.MaxValue` and `Block` behavior.

### Fixes
- **Change `MaxQueueSizeBehavior`** to control overflow:
  - Use `Block` if you want consumers to wait for space (may cause latency under sustained load).
  - Use `Reject` if you want to drop oldest events and keep newest.
  - Use `DropNew` (default) if you want to drop newest events and keep oldest.
- **Increase `MaxQueueSize`** if you're losing events via `DropNew` or `Reject`:
  ```csharp
  var options = new SubscriberOptions
  {
      MaxQueueSize = 5000, // instead of default 1000
      MaxQueueSizeBehavior = MaxQueueSizeBehavior.Block
  };
  ```
- **Use `DispatchMode = Parallel`** and set `MaxDegreeOfParallelism` > 1 for higher throughput.
- **For critical events** (backpressure alerts, error alerts), override `GetSubscriberOptions()` like `BackpressureAlertSubscriber` does, using `int.MaxValue` capacity and `Block` behavior.
- **Monitor `GetErrorCount()`** per subscriber to detect processing failures that might fill the queue.

### Health Check
- `RateLimitStatus.IsLimited` can indicate when a subscriber is approaching capacity limits.
- Check subscriber error counts via `PipelineEventPublisher.GetSubscriberErrorCount(eventName)`.

---

## Summary of Key Thresholds and Defaults

| Component | Setting | Default | Purpose |
|-----------|---------|---------|---------|
| **Backpressure** | `PipelineConfig.BackpressureTriggerThreshold` | `80.0%` | Buffer fill % that triggers backpressure |
| **Backpressure** | `PipelineConstants.BackpressureHighWaterMark` | `80.0%` | System-wide backpressure status threshold |
| **Dead Letter** | `DeadLetterQueue.DefaultMaxCapacity` | `1000` entries | Max entries before eviction |
| **Rate Limit** | `RateLimitingMiddleware.DefaultTokensPerSecond` | `1000` | Token refill rate |
| **Rate Limit** | `RateLimitingMiddleware.DefaultMaxBurstSize` | `5000` | Max tokens in bucket |
| **Retry** | `RetryPolicyOptions.DefaultMaxAttempts` | `3` | Max retry attempts |
| **Retry** | `RetryPolicyOptions.DefaultBaseDelay` | `200ms` | Initial delay between retries |
| **Retry** | `RetryPolicyOptions.DefaultJitterFactor` | `0.25` | ±25% random jitter |
| **Subscriber** | `SubscriberOptions.DefaultMaxQueueSize` | `1000` | Max events per subscriber queue |
| **Subscriber** | `SubscriberOptions.DefaultMaxDegreeOfParallelism` | `1` | Parallel workers |
| **Subscriber** | `MaxQueueSizeBehavior.Default` | `DropNew` | Queue overflow action |

---