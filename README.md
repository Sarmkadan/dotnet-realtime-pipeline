## PipelineOrchestrator

`PipelineOrchestrator` coordinates ingestion, processing, windowing, backpressure, queries, and metrics for a configured pipeline.

- `StartAsync()` starts the processing loop and creates backpressure contexts for the configured stages. Calling it while the pipeline is already running has no effect.
- `StopAsync()` gracefully stops the pipeline by calling `DrainAsync` with a five-second timeout.
- `DrainAsync(TimeSpan timeout)` immediately stops new ingestion, waits for queued items to be processed up to the positive timeout, discards any items still queued, flushes active windows, captures a final health report, and returns a `DrainResult` with processed, failed, dropped, and flushed counts plus timeout status.
- `DisposeAsync()` performs the same five-second drain when the pipeline has not already been stopped or drained, then suppresses finalization. Repeated disposal has no effect.
- `IngestDataPointAsync(DataPoint dataPoint)` requires a running pipeline and enqueues one point when the ingestion backpressure buffer accepts it. It returns `false` after applying backpressure when capacity is unavailable.
- `ProcessBatchDataPointsAsync(List<DataPoint> dataPoints)` submits each point through `IngestDataPointAsync` and returns a `BatchProcessingResult` containing successful and failed acceptance counts.
- `GetStatus()` returns a `PipelineStatus` snapshot with running state, lifetime processed and failed totals, queued item count, configuration identity, backpressure status, and a UTC timestamp. Call `GetSummary()` on that returned `PipelineStatus` for its compact status string.
- `GetHealthReportAsync()` generates the current metrics health report.
- `GetThroughput()` returns pipeline-wide events per second; `GetThroughput(string stageName)` returns events per second for one stage.
- `GetPerformanceTrendAsync()` analyzes the recorded metrics and returns a `PerformanceTrend`.

The orchestrator is registered by the library's service collection setup, so an application can resolve and use it through dependency injection:

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;

await using PipelineOrchestrator pipeline =
    serviceProvider.GetRequiredService<PipelineOrchestrator>();

await pipeline.StartAsync();

await pipeline.IngestDataPointAsync(new DataPoint(
    id: 1,
    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    value: 21.5,
    source: "sensor-a"));

BatchProcessingResult batch = await pipeline.ProcessBatchDataPointsAsync(
    new List<DataPoint>
    {
        new(2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 22.0, "sensor-a"),
        new(3, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 19.8, "sensor-b")
    });

PipelineStatus status = pipeline.GetStatus();
Console.WriteLine(status.GetSummary());
Console.WriteLine($"Throughput: {pipeline.GetThroughput():F2} events/sec");

HealthReport health = await pipeline.GetHealthReportAsync();
PerformanceTrend trend = await pipeline.GetPerformanceTrendAsync();
DrainResult drain = await pipeline.DrainAsync(TimeSpan.FromSeconds(10));
```

## PipelineConfigurationBuilder

`PipelineConfigurationBuilder` creates a `PipelineConfig` with configuration ID `1` from the required, non-empty pipeline name and version passed to its constructor. Its fluent methods are:

- `WithBufferConfiguration(long maxBufferSize, long flushIntervalMs, int maxConcurrentConsumers)` sets buffer capacity, flush interval, and consumer concurrency.
- `WithWindowingConfiguration(long windowSizeMs, long windowSlideMs, string windowType)` sets the window duration, slide, and non-empty window type.
- `WithPerformanceConfiguration(int maxRetries, long retryDelayMs, long processingTimeoutMs, double backpressureTriggerThreshold)` sets retry, timeout, and backpressure values.
- `WithQualityConfiguration(int minDataQualityThreshold, bool validateOnIngestion, bool enableMetricsCollection)` sets quality validation and metrics options.
- `WithStage(string stageName, string stageType)` adds a stage with a non-empty name and type.
- `WithCustomSetting(string key, object value)` adds or replaces a non-null custom setting under a non-empty key.
- `WithHighPerformanceDefaults()` sets buffer size `100000`, flush interval `500` ms, `16` consumers, window size `1000` ms, window slide `500` ms, `2` retries, and retry delay `50` ms.
- `WithLowLatencyDefaults()` sets buffer size `5000`, flush interval `100` ms, `2` consumers, window size `1000` ms, window slide `100` ms, and processing timeout `5000` ms.
- `WithHighReliabilityDefaults()` sets buffer size `50000`, flush interval `2000` ms, `4` consumers, `5` retries, retry delay `500` ms, minimum data quality `85`, and ingestion validation enabled.
- `Build()` adds default stages when no stages were supplied, validates the configuration, and returns the `PipelineConfig`; invalid values cause an `InvalidOperationException`.

The builder also provides `Activate()` and `Deactivate()` to set `IsActive`. Before fluent overrides, the underlying `PipelineConfig` defaults are a buffer size of `10000`, a `1000` ms flush interval, `4` consumers, a `5000` ms tumbling window sliding every `1000` ms, `3` retries with a `100` ms delay, a `30000` ms processing timeout, an `80.0` backpressure threshold, a minimum data quality of `70`, ingestion validation and metrics collection enabled, and an active configuration. Any property not changed by a preset retains this underlying default (or a value set earlier in the chain).

If no `WithStage` call is made, `Build()` adds `Ingestion` (`SOURCE`), `Validation` (`FILTER`), `Transformation` (`TRANSFORM`), `Windowing` (`WINDOW`), `Aggregation` (`AGGREGATE`), and `Output` (`SINK`) in that order.

```csharp
using DotNetRealtimePipeline.Configuration;
using DotNetRealtimePipeline.Domain.Models;

PipelineConfig config = new PipelineConfigurationBuilder("telemetry", "1.0.0")
    .WithBufferConfiguration(
        maxBufferSize: 25_000,
        flushIntervalMs: 500,
        maxConcurrentConsumers: 8)
    .WithWindowingConfiguration(
        windowSizeMs: 10_000,
        windowSlideMs: 2_000,
        windowType: "SLIDING")
    .WithPerformanceConfiguration(
        maxRetries: 4,
        retryDelayMs: 250,
        processingTimeoutMs: 20_000,
        backpressureTriggerThreshold: 75.0)
    .WithQualityConfiguration(
        minDataQualityThreshold: 80,
        validateOnIngestion: true,
        enableMetricsCollection: true)
    .WithCustomSetting("region", "us-east")
    .Activate()
    .Build();
```

## SlidingWindowAggregator

`SlidingWindowAggregator` produces overlapping aggregates from timestamped `DataPoint` values. Its constructor accepts `windowSizeMs`, the duration of every window in milliseconds, and `stepIntervalMs`, the slide between successive window boundaries. Both values must be positive, and the slide cannot exceed the window size; setting them equal produces tumbling windows. The implementation has no configurable timestamp-extractor constructor parameter: it reads each point's Unix-millisecond timestamp directly from `DataPoint.Timestamp`.

- `Add(DataPoint)` adds one non-null point. Points may arrive out of timestamp order; the buffer is kept sorted.
- `AddRange(IEnumerable<DataPoint>)` adds a non-null sequence using the same behavior and validation as `Add`.
- `FlushDueWindows(long currentTimeMs)` emits every not-yet-emitted window whose step boundary is at or before the supplied Unix-millisecond time. A window contains points whose timestamps are greater than or equal to its start and less than its end. Old points are pruned after flushing.
- `FlushDueWindows()` performs the same operation using the current UTC Unix-millisecond time.
- `GetPercentile(double percentile)` calculates a percentile from values in the current UTC-time window. The percentile must be from `0` through `100`; the method returns `0` for an empty window and linearly interpolates between adjacent sorted values when necessary.

Each `SlidingWindowResult` exposes:

- `WindowId`: a sequential result identifier.
- `WindowStartMs` and `WindowEndMs`: the start-inclusive, end-exclusive Unix-millisecond bounds.
- `WindowSizeMs` and `StepIntervalMs`: the configured window size and slide.
- `DataPointCount`: the number of points in the window.
- `Average`, `Sum`, `Min`, and `Max`: value aggregates, each `0` when the window is empty.
- `Trend`: the second-half average minus the first-half average, or `0` when fewer than two points are present.
- `EmittedAt`: the UTC time when the result was created.
- `AggregatedData`: a dictionary containing the window type and aggregate metadata (`WindowType`, `Average`, `Sum`, `Min`, `Max`, `Count`, `Trend`, `WindowSizeMs`, and `StepIntervalMs`).

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;

long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
var aggregator = new SlidingWindowAggregator(
    windowSizeMs: 10_000,
    stepIntervalMs: 2_000);

aggregator.Add(new DataPoint(1, now - 3_000, 12.5, "sensor-a"));
aggregator.AddRange(new[]
{
    new DataPoint(2, now - 2_000, 15.0, "sensor-a"),
    new DataPoint(3, now - 1_000, 17.5, "sensor-a")
});

double p95 = aggregator.GetPercentile(95);

foreach (SlidingWindowResult window in aggregator.FlushDueWindows(now))
{
    Console.WriteLine(
        $"Window {window.WindowId}: {window.DataPointCount} points, " +
        $"average {window.Average}, p95 at flush time {p95}");
}

// Uses DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() internally.
IReadOnlyList<SlidingWindowResult> laterWindows = aggregator.FlushDueWindows();
```

## BackpressureService

`BackpressureService` keeps an in-memory `BackpressureContext` for each pipeline stage. Register a stage with `CreateContext(stageName, maxBufferCapacity)`; stage names must be non-empty, capacities must be positive, and duplicate names are rejected. Each new context receives a sequential ID and allows four concurrent consumers by default. `GetContext` returns the registered context or `null`, while `Clear` removes every registration.

Buffer accounting is item-based. `TryAddToBuffer` increases `BufferSize` when the requested count fits. If it would exceed `MaxBufferCapacity`, the buffer is left unchanged, the full requested count is added to `DroppedItemCount`, backpressure is activated, and the method returns `false`. `RemoveFromBuffer` subtracts items without allowing the size to fall below zero. `GetBufferStatus`, `GetDroppedItemCount`, and `GetSystemStatus` expose per-stage sizes, loss, and aggregate status. Consumer concurrency is tracked separately through `TryRegisterConsumer` and `UnregisterConsumer`.

The threshold constants are `PipelineConstants.BackpressureHighWaterMark` (80%), `PipelineConstants.BackpressureLowWaterMark` (60%), and `PipelineConstants.BackpressureCriticalMark` (95%). `ApplyBackpressureAsync` activates at or above the 80% high-water value through the default threshold of `BackpressureContext.ShouldApplyBackpressure()`; a rejected over-capacity add also activates backpressure. Removing items deactivates it only when fill falls below the 60% low-water value. Those context methods currently express 80 and 60 as their own numeric defaults, while `GetSystemStatus` directly references `BackpressureHighWaterMark` and marks the system backpressured when average fill is strictly above it. The critical mark is a shared reference value but is not used by `BackpressureService` to change state or select a strategy.

Activation records the start time and appends it to `BackpressureEventTimestamps` (retaining the latest 100 timestamps); deactivation adds the elapsed duration to `TotalBackpressureTimeMs`. The service itself declares and raises no CLR events. Callers can observe state through `IsBackpressured`, `GetContext`, and `GetSystemStatus`. `ResetBackpressure` deactivates a known stage and clears its buffer, but preserves its dropped-item count and accumulated duration.

```csharp
using DotNetRealtimePipeline.Constants;
using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Services;

var backpressure = new BackpressureService();
backpressure.CreateContext("enrichment", maxBufferCapacity: 1_000);

backpressure.TryAddToBuffer("enrichment", 800);
var response = await backpressure.ApplyBackpressureAsync(
    "enrichment",
    BackpressureStrategy.Throttle,
    timeoutMs: 250);

Console.WriteLine(
    $"High={PipelineConstants.BackpressureHighWaterMark}%, " +
    $"Low={PipelineConstants.BackpressureLowWaterMark}%, " +
    $"Critical={PipelineConstants.BackpressureCriticalMark}%");
Console.WriteLine($"Applied={response.Applied}, Fill={response.BufferFillPercent:F1}%");

backpressure.RemoveFromBuffer("enrichment", 201); // 59.9%, below the low-water mark
Console.WriteLine($"Active={backpressure.IsBackpressured("enrichment")}");
```

## Checkpointing and recovery

The `CheckpointManager` service allows a pipeline to record processing offsets so that after a restart it can resume from the last committed position rather than reprocessing from the beginning.

### Configuration

Create a `CheckpointManager` instance, optionally specifying a persistence path for durable storage (e.g., a file). If no path is provided, checkpoints are kept in-memory only and will be lost on process exit.

```csharp
using DotNetRealtimePipeline.Services;

// Persist checkpoints to a file in the current directory.
var checkpointManager = new CheckpointManager("checkpoints.json");

// On startup, load any previously persisted checkpoints.
await checkpointManager.LoadAsync();
```

### Usage in a pipeline stage

Within each pipeline stage, after successfully processing a batch or a single data point, commit the offset (e.g., the sequence number or timestamp) to the checkpoint manager.

```csharp
// Example: committing after processing a data point.
long lastProcessedOffset = dataPoint.Timestamp; // or any monotonic offset
await checkpointManager.CommitAsync("enrichment", lastProcessedOffset, cancellationToken);
```

### Resuming after restart

When the pipeline starts again, load the persisted checkpoints and use `GetLastOffset` (or `GetCheckpoint`) to determine where to resume for each stage.

```csharp
// After LoadAsync as shown above:
long enrichmentOffset = checkpointManager.GetLastOffset("enrichment");
// If the offset is -1, no checkpoint exists; start from the beginning.
// Otherwise, begin processing from the next offset after enrichmentOffset.
```

### Delivery guarantees

The checkpoint manager itself provides an **at-least-once** delivery guarantee when used in the typical pattern of committing after successful processing. If the process crashes after a data point is processed but before its offset is checkpointed, that data point will be reprocessed after restart. Conversely, because offsets are only advanced after successful processing, no data point is skipped, guaranteeing at-least-once processing.

Exactly-once processing is not automatically provided; it requires that the processing side effects be idempotent or that transactions span both the processing and checkpoint commitment. The `CheckpointManager` does not coordinate with external systems to achieve exactly-once semantics.

## DeadLetterQueue

`DeadLetterQueue` is a thread-safe, in-memory queue for data points that failed pipeline processing. Its constructor accepts `maxCapacity` (default `1000`) and `defaultMaxRetries` (default `3`); capacity must be positive and the retry limit cannot be negative. The default retry limit is copied to each new entry.

- `EnqueueAsync(dataPoint, stageName, failureReason, exception, attemptsMade)` adds a pending entry and records its failure context. The exception is optional and `attemptsMade` defaults to `1`.
- `PeekAsync(maxCount)` returns the oldest entries first without changing them; `maxCount` defaults to `100`.
- `DequeueForRetryAsync(maxCount)` selects up to `maxCount` retryable entries, oldest first. It does not remove them: each returned entry has its retry count incremented, its last-retry time updated, and its status changed to `InRetry`. The default batch size is `10`.
- `ReplayAsync(filter)` resets every matching entry to `Pending`, clears its retry count and resolution details, and returns the number reset. The filter can select entries by stage, exception type, status, or any other entry property.
- `AcknowledgeSuccessAsync(entryId)` removes a successfully reprocessed entry and increments the lifetime resolved count. `AcknowledgeFailureAsync(entryId, finalReason)` retains the entry as `PermanentFailure` with the reason and resolution time. Both operations are no-ops for an unknown ID.
- `GetStatsAsync()` reports current total, pending, in-retry, and permanent-failure counts, plus the number successfully resolved since the queue was created and the UTC generation time. The `Count` property returns the current number of stored entries.

When an enqueue occurs at capacity, the queue first removes all entries already marked `Resolved` or `PermanentFailure`. If it is still full, it removes the oldest remaining entry before adding the new one. Eviction is therefore automatic and can discard an unresolved entry when no resolved or permanently failed entries are available.

```csharp
using DotNetRealtimePipeline.DeadLetter;
using DotNetRealtimePipeline.Domain.Models;

var queue = new DeadLetterQueue(maxCapacity: 1_000, defaultMaxRetries: 3);
var point = new DataPoint(
    id: 42,
    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    value: 18.7,
    source: "sensor-a");

await queue.EnqueueAsync(
    point,
    stageName: "validation",
    failureReason: "Reading was outside the accepted range",
    exception: new InvalidOperationException("Invalid reading"),
    attemptsMade: 2);

IReadOnlyList<DeadLetterEntry> waiting = await queue.PeekAsync(maxCount: 20);
IReadOnlyList<DeadLetterEntry> retryBatch = await queue.DequeueForRetryAsync(maxCount: 10);

foreach (DeadLetterEntry entry in retryBatch)
{
    bool reprocessed = entry.DataPoint.Value >= 0;

    if (reprocessed)
        await queue.AcknowledgeSuccessAsync(entry.EntryId);
    else
        await queue.AcknowledgeFailureAsync(entry.EntryId, "Manual review required");
}

int replayed = await queue.ReplayAsync(entry =>
    entry.FailureStageName == "validation" &&
    entry.Status == DeadLetterStatus.PermanentFailure);

DeadLetterQueueStats stats = await queue.GetStatsAsync();
Console.WriteLine(
    $"Stored={stats.TotalEntries}, Pending={stats.PendingEntries}, " +
    $"InRetry={stats.InRetryEntries}, Resolved={stats.TotalResolved}, Replayed={replayed}");
```

## Dead-Letter Retry Policy

The retry policy decides whether a failed operation is attempted again before its item is routed to the dead-letter queue. It is defined by `IRetryPolicy` (`src/DeadLetter/IRetryPolicy.cs`), configured by `RetryPolicyOptions` (`src/DeadLetter/RetryPolicyOptions.cs`), and implemented by `ExponentialBackoffRetryPolicy` (`src/DeadLetter/ExponentialBackoffRetryPolicy.cs`).

- `MaxAttempts` is the total number of attempts, including the first one. `RetryPolicyOptions.MaxAttempts` defaults to `3` and must be at least `1`.
- `IsTransient(exception)` classifies an exception as retryable. `ExponentialBackoffRetryPolicy` delegates to `RetryPolicyOptions.TransientExceptionPredicate`, which defaults to `RetryPolicyOptions.DefaultTransientPredicate`.
- `GetBackoffDelay(attemptNumber)` returns the delay after the 1-based attempt that just failed. The base delay is `BaseDelay * 2^(attemptNumber - 1)`, capped at `MaxDelay`. Jitter is then applied as a random offset of up to `±JitterFactor` of the capped delay, and the result is never negative. Defaults are `BaseDelay` `200 ms`, `MaxDelay` `30 s`, and `JitterFactor` `0.25`. The exponent is capped at 20 to avoid overflow. Because jitter is applied after the cap, a delay can exceed `MaxDelay` by up to `JitterFactor`.
- `ExecuteAsync(operation, cancellationToken)` runs `operation(attempt)` with the 1-based attempt number and returns a `RetryResult<T>`. It stops on the first success, on a non-transient exception, or when `MaxAttempts` is reached. It waits `GetBackoffDelay` between attempts only, so there is no delay after the final failure. A cancellation requested through `cancellationToken` propagates as `OperationCanceledException`.
- `RetryResult<T>` exposes `Succeeded`, `Value`, `Attempts`, and `LastException`.
- `DefaultTransientPredicate(exception)` treats `TimeoutException`, `IOException`, `SocketException`, and `HttpRequestException` as transient. It unwraps `AggregateException` and `PipelineProcessingException` to their inner exception and classifies that instead. `OperationCanceledException` and every other type are permanent.

`RetryPolicyOptions.ToString()` and `ExponentialBackoffRetryPolicy.ToString()` return a one-line summary of the configuration, which is useful in logs.

```csharp
using DotNetRealtimePipeline.DeadLetter;

var options = new RetryPolicyOptions
{
    MaxAttempts = 5,
    BaseDelay = TimeSpan.FromMilliseconds(100),
    MaxDelay = TimeSpan.FromSeconds(10),
    JitterFactor = 0.2,
    // Treat InvalidOperationException as transient in addition to the defaults.
    TransientExceptionPredicate = ex =>
        RetryPolicyOptions.DefaultTransientPredicate(ex) || ex is InvalidOperationException
};

IRetryPolicy policy = new ExponentialBackoffRetryPolicy(options);

RetryResult<string> result = await policy.ExecuteAsync(async attempt =>
{
    // Called once per attempt; throw to request another attempt if the error is transient.
    if (attempt < 3)
        throw new TimeoutException($"Upstream timed out on attempt {attempt}");

    await Task.Delay(10);
    return "payload";
});

if (result.Succeeded)
    Console.WriteLine($"Succeeded after {result.Attempts} attempts: {result.Value}");
else
    Console.WriteLine($"Gave up after {result.Attempts} attempts: {result.LastException?.Message}");
```

## ValidationHelperValidation

The ValidationHelperValidation class provides a suite of static methods for validating data integrity, including time range checks and boundary validations. It supports multiple validation strategies, returning boolean flags, detailed result objects, or lists of error messages to accommodate different error handling requirements.

Example usage:
```csharp
// Perform specific checks
bool timeValid = ValidationHelperValidation.IsInTimeRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(-1));
bool boundsValid = ValidationHelperValidation.IsWithinBounds(50, 0, 100);

// Validate and retrieve results
var result = ValidationHelperValidation.Validate(inputData);
var errors = ValidationHelperValidation.Validate(inputData);

// Check validity or enforce it
if (ValidationHelperValidation.IsValid(inputData))
{
    ValidationHelperValidation.EnsureValid(inputData);
}
```

## ThroughputCounter

`ThroughputCounter` (`src/Metrics/ThroughputCounter.cs`) is a sealed class that implements `IPipelineMetrics` and reports events-per-second over a sliding time window. It keeps one bucket per second in a circular array of `windowSeconds` buckets; each event is added to the bucket for the current Unix second (`second % windowSeconds`), so recording and querying avoid per-event allocation or scanning of history.

- The constructor takes `windowSeconds`, the window length in seconds. It defaults to `ThroughputCounter.DefaultWindowSeconds` (`60`) and must be positive; otherwise it throws `ArgumentOutOfRangeException`.
- `RecordEvents(long count)` adds events to the pipeline-wide (global) counter. Counts of zero or less are ignored.
- `RecordEvents(string stageName, long count)` adds events to a named stage. Counts of zero or less are ignored. A null, empty, or whitespace-only `stageName` throws `ArgumentException`. Stages are created on first use.
- `GetThroughput()` returns the global events per second: the sum of the window's buckets divided by `windowSeconds`.
- `GetThroughput(string stageName)` returns the same calculation for one stage. It returns `0` for an unknown or blank stage name.
- `ToString()` returns a summary with the window length, the global rate, and the number of tracked stages.

Recording and reading use `Interlocked` operations and `Volatile` reads, so one instance can be shared by several threads (for example, one per pipeline stage) without external locking. Stage buckets are stored in a `ConcurrentDictionary<string, long[]>`.

```csharp
using DotNetRealtimePipeline.Metrics;

// 30-second window; the default constructor uses 60 seconds.
var throughput = new ThroughputCounter(windowSeconds: 30);

// Called by the pipeline for every batch that leaves the source.
throughput.RecordEvents(128);

// Per-stage counts, recorded from each stage's worker.
throughput.RecordEvents("parse", 128);
throughput.RecordEvents("enrich", 120);

double overall = throughput.GetThroughput();          // events per second, global
double parse = throughput.GetThroughput("parse");     // events per second, one stage
double unknown = throughput.GetThroughput("missing"); // 0

Console.WriteLine(throughput); // ThroughputCounter(window=30s, global=..., stages=2)
```

Because the counter is typed as `IPipelineMetrics`, it can be used anywhere the interface is accepted:

```csharp
IPipelineMetrics metrics = new ThroughputCounter();
metrics.RecordEvents("sink", 1);
```

## SubscriberChannelManager

`SubscriberChannelManager` (`src/Events/SubscriberChannelManager.cs`) is an internal sealed class that owns the `SubscriberChannel` instances for each event name. `PipelineEventPublisher` creates one and delegates subscriber registration, posting, start/stop, and counters to it. Each subscriber gets its own channel, so a slow or failing handler does not block or affect the others.

- `RegisterSubscriber<T>(eventName, handler, options)` adds a `Func<T, Task>` handler for an event name. `T` must derive from `PipelineEventArgs`. A null or whitespace `eventName` throws `ArgumentException`; a null `handler` throws `ArgumentNullException`. When `options` is `null`, the manager's default `SubscriberOptions` are used. The subscriber name is `options.Name`, or the handler's declaring type name, or `AnonymousSubscriber`.
- `PostEventAsync(eventName, args)` offers `args` to every channel registered for `eventName` and waits until all posts complete. It does not wait for the handlers to process the event. If nothing is registered for the name, it does nothing. After disposal it logs a warning and returns.
- `StartAll()` starts every registered channel. A failure to start one channel is logged and the others still start. After disposal it logs a warning and does nothing.
- `StopAllAsync()` stops all channels concurrently and waits for them. It does nothing after disposal.
- `GetTotalErrorCount(eventName)` sums the error counts of all channels for an event. It returns `0` for an unknown event.
- `GetSubscriberCount(eventName)` returns the number of channels registered for an event. It returns `0` for an unknown event.
- `DisposeAsync()` stops all channels, disposes them, and clears the registry. `Dispose()` calls `DisposeAsync()` and blocks on it. The class implements `IDisposable` and exposes `DisposeAsync()` as a `ValueTask`, but it does not declare `IAsyncDisposable`, so `await using` does not compile against it.

Because the class is `internal`, the example below only compiles inside the `DotNetRealtimePipeline` assembly (for example, from another internal class in the library).

```csharp
using DotNetRealtimePipeline.Events;
using Microsoft.Extensions.Logging;

public sealed class ItemProcessedArgs : PipelineEventArgs
{
    public int ItemCount { get; init; }
}

using var manager = new SubscriberChannelManager(logger);

// Each subscriber gets its own channel; the Name is used in logs.
manager.RegisterSubscriber<ItemProcessedArgs>(
    "ItemProcessed",
    args => AuditAsync(args.ItemCount),
    new SubscriberOptions { Name = "audit", MaxQueueSize = 500 });

manager.StartAll();

// Handlers run on their channels; this returns once every channel has accepted the event.
await manager.PostEventAsync("ItemProcessed", new ItemProcessedArgs { ItemCount = 128 });

int subscribers = manager.GetSubscriberCount("ItemProcessed"); // 1
int failures = manager.GetTotalErrorCount("ItemProcessed");    // 0 unless a handler threw

await manager.StopAllAsync();
```

## Output Formatters

`src/Formatters/OutputFormatter.cs` contains the formatters that turn pipeline data into text for files, logs, and reports. Every formatter implements `IOutputFormatter`, and `OutputFormatterFactory.Create(OutputFormat)` returns the one that matches a format.

- `IOutputFormatter` declares `string Format<T>(T data) where T : class` and `Task<string> FormatAsync<T>(T data) where T : class`. `FormatAsync` wraps the synchronous `Format` call in a completed task, so it does no real asynchronous work.
- `OutputFormat` is the enum of supported formats: `Json`, `Csv`, `Table`, and `Html`.
- `OutputFormatterFactory.Create(format)` maps each enum value to a new formatter instance. An unknown value falls back to `JsonOutputFormatter` and does not throw.
- `JsonOutputFormatter` serializes any object with `System.Text.Json`, indented, with case-insensitive property names. Null data throws `ArgumentNullException`.
- `CsvOutputFormatter` writes a header row and one line per record. It supports `List<DataPoint>` (columns `Id,Timestamp,Value,Source,Quality,Tags`) and `List<ProcessingResult>` (columns `ResultId,Success,ErrorMessage,ProcessingTimeMs,StageName`). Values are formatted with `InvariantCulture`, and text containing commas, quotes, or newlines is quoted. Any other type returns an empty string.
- `TableOutputFormatter` writes a fixed-width ASCII table. A `List<DataPoint>` shows the first 10 rows (with a `... and N more rows` line for the rest), and an empty list returns `No data points`. A `Dictionary<string, object>` becomes a `Key`/`Value` table. Any other type is serialized to a single line of JSON.
- `HtmlOutputFormatter` produces a complete HTML page titled `Pipeline Data Report` with inline CSS. A `List<DataPoint>` becomes a table of the first 50 rows; other types produce a page with only the heading. Values are written into the markup without HTML encoding, so do not pass untrusted text through this formatter without escaping it first.

The factory is the usual entry point, so the caller can pick the format at runtime from configuration or a command-line option:

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Formatters;

List<DataPoint> points = GetRecentPoints();

// Choose the format at runtime, for example from a --format option.
OutputFormat format = OutputFormat.Csv;
IOutputFormatter formatter = OutputFormatterFactory.Create(format);

string text = formatter.Format(points);
string json = await OutputFormatterFactory.Create(OutputFormat.Json).FormatAsync(points);

// Falls back to JSON for a value outside the enum.
IOutputFormatter fallback = OutputFormatterFactory.Create((OutputFormat)99);
```

## Metrics Exporters

`src/Integration/MetricsExporter.cs` contains the exporters that send `MetricAggregation` results to other systems. Every exporter implements `IMetricsExporter`, which has two methods: `ExportAsync(MetricAggregation)` for one aggregation and `ExportBatchAsync(List<MetricAggregation>)` for several.

- `PrometheusMetricsExporter` formats each aggregation as Prometheus text lines (`pipeline_average_processing_time_ms`, `pipeline_total_items_processed_total`, `pipeline_failed_items_total`, `pipeline_skipped_items_total`, `pipeline_backpressure_events_total`, `pipeline_total_backpressure_ms`), each with the aggregation's `ComputedAt` as a Unix millisecond timestamp. In the current code, it builds those lines, logs how many there are at `Debug` level, and returns. It does not write them to a file or an endpoint, so use it only if you add that step yourself.
- `HttpMetricsExporter` POSTs JSON to a fixed endpoint with the `HttpClient` you pass in. `ExportAsync` sends one JSON object, and `ExportBatchAsync` sends a JSON array. A success logs at `Debug` or `Information`, and a non-success status logs a `Warning`. Exceptions (such as network failures) are logged as errors and are not rethrown, so a failed export does not stop the caller. The exporter does not dispose the `HttpClient`.
- `CompositeMetricsExporter` fans each call out to every exporter added with `AddExporter(IMetricsExporter)`, and runs them concurrently. If one exporter's task faults, the failure is logged as `Exporter failed` and the other exporters still run. `AddExporter` null-checks its argument and adds to a plain `List`, so register all exporters during setup, before the composite is used from several threads. A custom exporter that throws synchronously, before it returns a task, is not caught by the composite and propagates to the caller.
- `MetricsExporterFactory` creates the exporters with their dependencies:
  - `CreatePrometheus(logger)` returns a `PrometheusMetricsExporter` as `IMetricsExporter`.
  - `CreateHttp(endpoint, client, logger)` returns an `HttpMetricsExporter` as `IMetricsExporter`. A `null` endpoint throws `ArgumentNullException`, and a whitespace endpoint throws `ArgumentException`. A `null` client also throws `ArgumentNullException`.
  - `CreateComposite(logger)` returns a `CompositeMetricsExporter` (not the interface), so you can call `AddExporter` without a cast.

Use the composite when one set of metrics should go to several destinations. Each exporter is added once, and the caller then makes one call:

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Integration;

// Shared, long-lived HttpClient; the exporter does not dispose it.
HttpClient httpClient = httpClientFactory.CreateClient("metrics");

CompositeMetricsExporter composite = MetricsExporterFactory.CreateComposite(
    loggerFactory.CreateLogger<CompositeMetricsExporter>());

composite.AddExporter(MetricsExporterFactory.CreatePrometheus(
    loggerFactory.CreateLogger<PrometheusMetricsExporter>()));

composite.AddExporter(MetricsExporterFactory.CreateHttp(
    "https://metrics.example.com/ingest",
    httpClient,
    loggerFactory.CreateLogger<HttpMetricsExporter>()));

var aggregation = new MetricAggregation(metricId: 1, startMs: 0, endMs: 60_000, metricType: "throughput")
{
    TotalItemsProcessed = 1200,
    TotalItemsFailed = 3,
    AverageProcessingTimeMs = 4.7,
    ComputedAt = DateTime.UtcNow
};

// Sends the aggregation to both exporters; each one logs its own failures.
await composite.ExportAsync(aggregation);

// Batch form: the HTTP exporter posts the whole list as one JSON array.
await composite.ExportBatchAsync(new List<MetricAggregation> { aggregation });
```

## External Data Sources

`src/Integration/ExternalDataSource.cs` contains the connectors that pull `DataPoint` records from systems outside the pipeline. Every connector implements `IExternalDataSource`, which has two methods: `FetchDataAsync(DateTime startTime, DateTime endTime)` returns a `List<DataPoint>` for the time range, and `IsAvailableAsync()` reports whether the source can be reached.

- `HttpDataSource` is the HTTP connector. The constructor takes a base URL, an `HttpClient` you own, and an `ILogger<HttpDataSource>`; null arguments throw `ArgumentNullException`. `FetchDataAsync` converts both times to Unix milliseconds and calls `GET {baseUrl}/data?start={startMs}&end={endMs}`, then deserializes the body as a JSON array of `DataPoint`. `IsAvailableAsync` calls `GET {baseUrl}/health` and returns `true` only for a success status.
- `HttpDataSource` does not throw on failure. A non-success status, a network error, or invalid JSON is logged and turns into an empty list. Because of this, `DataSourceManager` cannot fail over on a failed fetch from an `HttpDataSource`. It only fails over when `IsAvailableAsync()` returns `false` or the source throws.
- `DataSourceManager` holds several named sources, ordered by priority. It has no lock, so register all sources during setup, before the manager is used from several threads.
  - `Register(string name, IExternalDataSource source, int priority = 0)` adds a source and re-sorts the list so that the highest priority comes first. A null or whitespace `name` throws `ArgumentException`, and a null `source` throws `ArgumentNullException`. Duplicate names are not rejected, but see `GetSourceHealth` below.
  - `FetchDataAsync(DateTime startTime, DateTime endTime)` walks the sources that are still marked healthy, in priority order. For each one it calls `IsAvailableAsync()`. If that returns `false`, or the source throws, the source is marked unhealthy and skipped. The first source that is available returns its data, even if the list is empty. If no source is left, it logs a warning and returns an empty list.
  - `GetSourceHealth()` returns a `Dictionary<string, bool>` that maps each name to its health flag. A source is marked unhealthy and is never set back to healthy, so a source that fails stays excluded until the manager is recreated. Because the result is built with `ToDictionary`, registering two sources with the same name makes `GetSourceHealth()` throw `ArgumentException`.
- `CachedDataSource` wraps another `IExternalDataSource` and keeps the results of `FetchDataAsync` in a `CacheService` with a capacity of 1000 entries and a time-to-live of one hour. The cache key is the start and end time in ISO 8601 round-trip format. `IsAvailableAsync` is not cached and always calls the inner source.
  - A cache hit returns the stored list without calling the inner source. A miss calls the inner source and stores whatever it returns, including an empty list from a failed `HttpDataSource` fetch. Empty results are therefore cached for an hour as well.
  - `ClearCache()` removes all cached entries and logs at `Information` level. It does not change the entries' time-to-live, and the next fetch calls the inner source again.

A typical setup places a cached HTTP connector first and a second connector behind it, then asks the manager for data:

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Integration;

// Shared, long-lived HttpClient; the connectors do not dispose it.
HttpClient httpClient = httpClientFactory.CreateClient("sensors");

var primary = new HttpDataSource(
    "https://sensors-eu.example.com",
    httpClient,
    loggerFactory.CreateLogger<HttpDataSource>());

var secondary = new HttpDataSource(
    "https://sensors-us.example.com",
    httpClient,
    loggerFactory.CreateLogger<HttpDataSource>());

// Results of the primary source are cached for one hour per time range.
var cachedPrimary = new CachedDataSource(
    primary,
    loggerFactory.CreateLogger<CachedDataSource>());

var manager = new DataSourceManager(loggerFactory.CreateLogger<DataSourceManager>());

// Higher priority is tried first; the secondary is used when the primary is unavailable.
manager.Register("primary", cachedPrimary, priority: 10);
manager.Register("secondary", secondary, priority: 5);

DateTime end = DateTime.UtcNow;
DateTime start = end.AddMinutes(-5);

List<DataPoint> points = await manager.FetchDataAsync(start, end);

// Example: { "primary": False, "secondary": True } after the primary failed its availability check.
Dictionary<string, bool> health = manager.GetSourceHealth();

// Drop cached results, for example after the upstream system reprocesses its data.
cachedPrimary.ClearCache();
```

Note that `GetSourceHealth()` reports the state the manager has seen so far, not a fresh check. A source only shows `False` after a `FetchDataAsync` call has found it unavailable.

## Plugin System

`src/Plugins/ExtensionSystem.cs` provides the plugin contracts, a manager that owns plugin lifecycle, a base class, a configuration record, a registry of configurations, a sample plugin, and a named hook system. The library does not call these types on its own: the host application creates `PluginManager` or `HookManager`, registers plugins or hooks, and calls the methods below. Nothing in the pipeline currently fetches plugins from the manager or fires hooks.

### Plugin contracts

- `IPipelinePlugin` is the base contract. It exposes `Name` and `Version` and two lifecycle methods, `InitializeAsync()` and `ShutdownAsync()`.
- `IDataProcessingPlugin` adds `Task<ProcessingResult> ProcessAsync(DataPoint dataPoint)` for plugins that handle one data point at a time.
- `IDataTransformPlugin` adds `DataPoint Transform(DataPoint dataPoint)`, a synchronous mapping from one point to another.
- `IOutputPlugin` adds `Task OutputAsync(ProcessingResult result)` for sending results to a destination.

A class can implement several of these interfaces. `PluginManager` filters by type, so a class that implements both `IDataProcessingPlugin` and `IOutputPlugin` appears in both lists.

### PluginManager

`PluginManager` keeps plugins in registration order. Its constructor takes an `ILogger<PluginManager>`, and a null logger throws `ArgumentNullException`. It is not registered in `ServiceCollectionExtensions`, so create it yourself.

- `RegisterPlugin(IPipelinePlugin plugin)` appends the plugin and logs its name and version. A null plugin throws `ArgumentNullException`. Registering does not call the plugin's lifecycle methods.
- `InitializeAsync()` calls `InitializeAsync()` on each plugin in registration order. If a plugin throws, the error is logged and the loop moves on to the next plugin. The method does not rethrow, so check the logs to find out whether a plugin failed. `IsInitialized` becomes `true` when the loop ends, even if some plugins failed. A plugin registered after this call is not initialized by it.
- `ShutdownAsync()` calls `ShutdownAsync()` on each plugin in reverse registration order, with the same error handling. It sets `IsInitialized` back to `false`.
- `GetProcessingPlugins()`, `GetTransformPlugins()` and `GetOutputPlugins()` return a new `List<T>` filtered by interface. `GetAllPlugins()` returns a copy of the full list.

The manager has no lock. Register plugins during setup, before the manager is used from several threads.

### PipelinePluginBase

An abstract base class for plugins. Subclasses override `Name` and `Version`. The constructor takes a non-generic `ILogger` and throws `ArgumentNullException` when it is null. The virtual `InitializeAsync()` and `ShutdownAsync()` write one information log line and complete. Override them to acquire and release resources.

### PluginConfiguration and PluginRegistry

`PluginConfiguration` is a plain settings object with these properties:

- `Name` and `Version`.
- `Enabled`, which defaults to `true`.
- `Settings`, a `Dictionary<string, object>`.
- `Dependencies`, a `List<string>` of plugin names. The values are stored but nothing checks them.

`PluginRegistry` stores configurations by `Name`. Despite its summary comment, it does not create or load plugin instances. It only holds the settings, and the host decides which plugins to build from them.

- `RegisterConfiguration(PluginConfiguration config)` adds the configuration, or replaces the existing one with the same name. A null config throws `ArgumentNullException`.
- `GetConfiguration(string pluginName)` returns `null` for an unknown name. A null or whitespace name throws `ArgumentException`.
- `GetEnabledConfigurations()` returns only the configurations whose `Enabled` is `true`.
- `GetAllConfigurations()` returns a copy of the internal dictionary.

### LoggingPlugin

`LoggingPlugin` is the bundled example. It derives from `PipelinePluginBase` and implements `IDataProcessingPlugin`. `ProcessAsync` logs the data point's `Id` and `Source` at Debug level and returns a successful `ProcessingResult` with `StageName` set to `"Logging Plugin"`. Use it as a template for new processing plugins.

### HookManager

`HookManager` lets you attach several handlers to a named hook and run them together.

- `RegisterHook(string hookName, Delegate handler)` appends the handler. A null or whitespace name throws `ArgumentException`, and a null handler throws `ArgumentNullException`.
- `ExecuteHookAsync(string hookName, params object[] args)` runs the handlers in registration order using `DynamicInvoke(args)`. When a handler returns a `Task`, the call waits for it before it runs the next handler. Other return values are ignored. A hook name with no handlers does nothing.
- A handler that throws is logged at Error level, and the remaining handlers still run. `ExecuteHookAsync` does not throw because of a handler failure. This also covers arguments that do not match the delegate's signature, because the resulting exception is caught the same way.

Use delegates that return `Task`. An `async void` handler is not awaited, so the hook can finish before it completes, and its exceptions are not caught by `ExecuteHookAsync`. `HookManager` has no lock either, so register hooks during setup.

### Example: registering a custom plugin

The example below adds a plugin that flags values above a threshold. It runs the bundled `LoggingPlugin` next to it, then shuts both down.

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Plugins;
using Microsoft.Extensions.Logging;

public sealed class ThresholdPlugin : PipelinePluginBase, IDataProcessingPlugin
{
    private readonly double _maxValue;

    public ThresholdPlugin(ILogger<ThresholdPlugin> logger, double maxValue) : base(logger)
    {
        _maxValue = maxValue;
    }

    public override string Name => "Threshold Plugin";
    public override string Version => "1.0.0";

    public Task<ProcessingResult> ProcessAsync(DataPoint dataPoint)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        bool withinLimit = dataPoint.Value <= _maxValue;
        if (!withinLimit)
        {
            _logger.LogWarning("Value {Value} from {Source} exceeds {Max}", dataPoint.Value, dataPoint.Source, _maxValue);
        }

        var result = new ProcessingResult
        {
            Success = withinLimit,
            StageName = Name,
            ProcessedAt = DateTime.UtcNow
        };

        if (!withinLimit)
        {
            result.MarkFailure($"Value {dataPoint.Value} is above {_maxValue}");
        }

        return Task.FromResult(result);
    }
}
```

```csharp
ILoggerFactory loggerFactory = /* from your host */;

var manager = new PluginManager(loggerFactory.CreateLogger<PluginManager>());

// Register during setup, before InitializeAsync().
manager.RegisterPlugin(new ThresholdPlugin(loggerFactory.CreateLogger<ThresholdPlugin>(), maxValue: 100));
manager.RegisterPlugin(new LoggingPlugin(loggerFactory.CreateLogger<LoggingPlugin>()));

await manager.InitializeAsync();

// The pipeline does not call plugins for you; the host asks for them and runs them.
foreach (IDataProcessingPlugin plugin in manager.GetProcessingPlugins())
{
    ProcessingResult result = await plugin.ProcessAsync(dataPoint);
}

// Shutdown runs in reverse registration order.
await manager.ShutdownAsync();
```

A hook is registered and fired the same way. The handler signature has to match the arguments you pass to `ExecuteHookAsync`:

```csharp
var hooks = new HookManager(loggerFactory.CreateLogger<HookManager>());

hooks.RegisterHook("AfterProcessing", new Func<DataPoint, Task>(point =>
{
    Console.WriteLine($"Processed {point.Id}");
    return Task.CompletedTask;
}));

await hooks.ExecuteHookAsync("AfterProcessing", dataPoint);
```

## HTTP Client Factory

`src/Integration/HttpClientFactory.cs` contains two types. `PipelineHttpClientFactory` creates and caches `HttpClient` instances. `HttpClientBuilder` builds one `HttpClient` from a fluent configuration, starting from the defaults in `HttpClientConfiguration`. `PipelineHttpClientFactory` is registered as a singleton in `ServiceCollectionExtensions`; `HttpClientBuilder` is not registered, so create it yourself.

### PipelineHttpClientFactory

The constructor takes an `ILogger<PipelineHttpClientFactory>`; null throws `ArgumentNullException`. The factory's properties (`Timeout`, `MaxRetries`, `RetryDelay`, `MaxConnectionsPerHost`, `UseCompression`, `UserAgent`, `DefaultHeaders`) document the defaults. None of the methods below read them, so changing `DefaultHeaders` has no effect on the clients the factory creates.

- `CreateDefaultClient()` returns a new client that is not cached. It has a 30-second timeout, GZip and Deflate decompression, up to 3 redirects, `User-Agent: DotNetRealtimePipeline/1.0`, and `Accept: application/json`. The caller owns the instance and must dispose it.
- `CreateServiceClient(string serviceName, TimeSpan timeout, bool useCompression = true)` returns a cached client keyed by `serviceName`. A null or whitespace name throws `ArgumentException`. On the first call it creates a client with default credentials and sets `User-Agent: DotNetRealtimePipeline/{serviceName}`, `Accept: application/json`, and `Accept-Encoding: gzip, deflate`. The `Accept-Encoding` header is set even when `useCompression` is `false`, and the handler then does not decompress. On later calls with the same name, the cached client is returned and `timeout` and `useCompression` are ignored.
- `GetOrCreateClient(string key, Action<HttpClient> configure)` returns a cached client for `key`. If there is none, it creates one with `CreateDefaultClient()`, calls `configure` on it once (`configure` may be null), and caches it. A cache hit does not call `configure`. This cache is shared with `CreateServiceClient`, so the same key in both methods returns the same instance.
- `RemoveClient(string serviceName)` disposes the cached client for that name and removes it. A null or whitespace name throws `ArgumentException`. An unknown name does nothing.
- `ClearClients()` disposes every cached client and empties the cache.

`RemoveClient` and `ClearClients` dispose clients that callers may still hold, so their next request fails with `ObjectDisposedException`. The cache is a plain `Dictionary` with no lock, so call the factory from one thread at a time or add your own synchronization.

```csharp
var factory = new PipelineHttpClientFactory(loggerFactory.CreateLogger<PipelineHttpClientFactory>());

HttpClient sensors = factory.CreateServiceClient("sensors", TimeSpan.FromSeconds(10));

// Same instance back; the 60-second timeout is ignored because "sensors" is already cached.
HttpClient sameSensors = factory.CreateServiceClient("sensors", TimeSpan.FromSeconds(60));

HttpClient reports = factory.GetOrCreateClient("reports", client =>
{
    client.BaseAddress = new Uri("https://reports.example.com");
});

factory.RemoveClient("sensors");   // disposes the "sensors" client
factory.ClearClients();            // disposes everything else
```

### HttpClientConfiguration

`HttpClientConfiguration` is a plain class with settable properties. It is the starting point for `HttpClientBuilder`, which creates its own instance internally. The defaults are:

| Property | Default | Constant |
|----------|---------|----------|
| `Timeout` | 30 seconds | `DefaultTimeout` |
| `MaxRetries` | 3 | `DefaultMaxRetries` |
| `RetryDelay` | 1 second | `DefaultRetryDelay` |
| `MaxConnectionsPerHost` | 10 | `DefaultMaxConnectionsPerHost` |
| `UseCompression` | `true` | none |
| `UserAgent` | `DotNetRealtimePipeline/1.0` | `DefaultUserAgent` |
| `DefaultHeaders` | empty | none |

### HttpClientBuilder

The constructor takes an `ILogger<HttpClientBuilder>`; null throws `ArgumentNullException`. Each configuration method returns the builder, so calls chain. The builder keeps one configuration. Each `Build()` call creates a new `HttpClient` from the configuration as it is at that moment, and the builder does not dispose the clients it creates.

- `WithTimeout(TimeSpan timeout)` sets the request timeout. The value is not validated.
- `WithRetry(int maxRetries, TimeSpan delay)` stores the retry count and delay. A negative `maxRetries` or a negative `delay` throws `ArgumentOutOfRangeException`.
- `WithCompression(bool enabled)` turns GZip and Deflate decompression on or off.
- `WithHeader(string name, string value)` adds a default header. A null or whitespace `name` throws `ArgumentException`, and a null `value` throws `ArgumentNullException`. Headers are stored by name, so a second call with the same name replaces the stored value.
- `WithMaxConnectionsPerHost(int connections)` stores the connection limit. A negative value throws `ArgumentOutOfRangeException`.
- `Build()` returns a new `HttpClient` with the stored timeout, decompression setting, user agent and headers. It logs the timeout, retry count and compression setting at `Information` level.

`Build()` applies the timeout, the compression setting, the user agent, and the headers. It does not apply `MaxRetries`, `RetryDelay`, or `MaxConnectionsPerHost`. `WithRetry` and `WithMaxConnectionsPerHost` store values that nothing reads, so they have no effect on the client yet; add retry handling in your own code if you need it. Also, `Build()` adds the user agent and the stored headers with `DefaultRequestHeaders.Add`, so `WithHeader("User-Agent", ...)` sends a second `User-Agent` value instead of replacing the default.

```csharp
var builder = new HttpClientBuilder(loggerFactory.CreateLogger<HttpClientBuilder>());

HttpClient client = builder
    .WithTimeout(TimeSpan.FromSeconds(10))
    .WithCompression(true)
    .WithHeader("X-Api-Version", "2")
    .WithHeader("Authorization", $"Bearer {apiToken}")
    .Build();

// Later builds reuse the same builder state; this client also gets a 10-second timeout.
HttpClient second = builder
    .WithHeader("X-Api-Version", "3")   // replaces the "2" stored above
    .Build();

// Retry and connection-limit values are stored but not applied by Build().
HttpClient tuned = builder
    .WithRetry(maxRetries: 5, delay: TimeSpan.FromMilliseconds(500))
    .WithMaxConnectionsPerHost(20)
    .Build();
```

## WorkerOptions

`WorkerOptions` (`src/Configuration/EventServiceConfiguration.cs`) is a sealed class with settings for the background workers. It is declared in the global namespace. The validation helpers are in `src/Configuration/EventServiceConfigurationValidation.cs` (`EventServiceConfigurationValidation`, namespace `DotNetRealtimePipeline.Configuration`).

| Property | Default | Constant | Meaning |
|----------|---------|----------|---------|
| `MetricsAggregationIntervalMs` | `5000` | `DefaultMetricsAggregationIntervalMs` | Milliseconds between metrics aggregation passes. |
| `HealthCheckIntervalMs` | `10000` | `DefaultHealthCheckIntervalMs` | Milliseconds between health check passes. |
| `EnableProcessingWorker` | `true` | none | Intended switch for `BackgroundProcessingWorker`. |
| `EnableMetricsWorker` | `true` | none | Intended switch for `MetricsAggregationWorker`. |
| `EnableHealthCheckWorker` | `true` | none | Intended switch for `HealthCheckWorker`. |

`ToString()` returns a one-line summary of all five values.

### How the workers use these values

Nothing in `src/Workers/` reads `WorkerOptions`, and `WorkerOptions` is not registered in `ServiceCollectionExtensions`. The workers take their settings through their own constructors, so the caller has to copy the values across:

- `MetricsAggregationWorker(MetricsService, ILogger<...>, int intervalMs = 5000)` uses `intervalMs` as its delay between passes. Pass `MetricsAggregationIntervalMs` here. The constructor does not clamp the value.
- `HealthCheckWorker(PipelineOrchestrator, ILogger<...>, int intervalMs = 10000)` uses `intervalMs` the same way. Pass `HealthCheckIntervalMs` here.
- `DynamicScalingWorker(DynamicScalingService, ILogger<...>, int intervalMs = 5000)` has its own interval, which is clamped to a minimum of 500 ms. It is not controlled by `WorkerOptions`.
- `BackgroundProcessingWorker` has no interval parameter. It polls the orchestrator with a fixed 100 ms delay.
- The three `Enable*Worker` flags are not read anywhere. `WorkerCoordinator.StartAll()` starts the processing, metrics, and health check workers unconditionally. To honor the flags, call `Start()` on each enabled worker yourself instead of using `StartAll()`.

`EventServiceConfigurationValidation` checks only the two intervals, which must be positive. It does not check the `Enable*Worker` flags. Use `Validate()` to get a list of problems, `IsValid()` for a boolean, or `EnsureValid()` to throw an `ArgumentException` that lists them. Call the validators before you pass the intervals to the workers.

### Configuration example

```csharp
using DotNetRealtimePipeline.Configuration;
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Workers;

var options = new WorkerOptions
{
    MetricsAggregationIntervalMs = 2000,
    HealthCheckIntervalMs = 15000,
    EnableHealthCheckWorker = false   // not read by any worker; see the note above
};

options.EnsureValid();

var processing = new BackgroundProcessingWorker(
    orchestrator,
    loggerFactory.CreateLogger<BackgroundProcessingWorker>());

var metrics = new MetricsAggregationWorker(
    metricsService,
    loggerFactory.CreateLogger<MetricsAggregationWorker>(),
    options.MetricsAggregationIntervalMs);

var health = new HealthCheckWorker(
    orchestrator,
    loggerFactory.CreateLogger<HealthCheckWorker>(),
    options.HealthCheckIntervalMs);

// Start only the workers that are enabled. The flag is applied here, in caller code.
if (options.EnableProcessingWorker) processing.Start();
if (options.EnableMetricsWorker) metrics.Start();
if (options.EnableHealthCheckWorker) health.Start();
```

Because `EnableHealthCheckWorker = false` is applied by the `if` above, the health worker is skipped in this example. Without that `if`, the flag would have no effect. Also, `WorkerCoordinator` requires all three workers in its constructor, so if you use it, you cannot leave one out. Start the workers individually in that case.

## Middleware

The middleware types are in `src/Middleware/` (namespace `DotNetRealtimePipeline.Middleware`). None of them is registered in `ServiceCollectionExtensions`, so create the ones you need yourself and pass them an `ILogger<T>` where one is required. Each source file has a companion `*Extensions.cs` file with helper overloads and a `*Validation.cs` file.

### LoggingMiddleware

`LoggingMiddleware` (`src/Middleware/LoggingMiddleware.cs`) is a sealed class that takes `ILogger<LoggingMiddleware>`. It writes structured log lines and does not wrap or change any operation:

- `LogDataIngestion(DataPoint dataPoint, string stage)` logs at Information. Each call generates its own 8-character ID for the line.
- `LogProcessingCompletion(ProcessingResult result, long elapsedMs)` logs at Information with `SUCCESS` or `FAILED`.
- `LogBackpressureEvent(string stageName, BackpressureContext context)` logs at Warning with the buffer utilization as a percentage.
- `LogMetricsCollection(string metricName, long value, string unit)` logs at Debug.
- `LogError(string operationName, Exception ex, string context)` logs at Error with the exception.
- `LogPerformanceWarning(string operationName, long elapsedMs, long thresholdMs)` logs at Warning only when `elapsedMs` is greater than `thresholdMs`.

The extension methods add `LogDataIngestionBatch(IEnumerable<DataPoint>, string stage)`, a `LogProcessingCompletion` overload that takes a `Stopwatch`, and a `LogPerformanceWarning` overload that takes a `Stopwatch`.

`PerformanceLoggingMiddleware` (same file, constructor takes `ILogger<PerformanceLoggingMiddleware>`) times an operation:

- `MeasureAsync<T>(string operationName, Func<Task<T>> operation)` and `Measure<T>(string operationName, Func<T> operation)` log the start at Debug and the elapsed time at Information when the operation succeeds.
- On an exception they log at Error with the elapsed time and rethrow the original exception. Nothing is swallowed.

`CorrelationMiddleware` (same file) carries a correlation ID through async code using a static `AsyncLocal<string>`:

- `WithCorrelationAsync<T>(Func<string, Task<T>> operation)` is an instance method. It creates a 16-character ID, passes it to the delegate, and restores the previous value in a `finally` block.
- `SetCorrelationId(string)` and `ClearCorrelationId()` set and clear the ambient ID directly.
- `GetCorrelationId()` returns the ambient ID. If none is set, it returns a new 8-character ID and does not store it, so each call without an ambient ID returns a different value.

`LoggingMiddleware` does not read the ambient ID. Its log lines use their own IDs, so the IDs from `CorrelationMiddleware` do not appear in them.

### ErrorHandlingMiddleware

`ErrorHandlingMiddleware` (`src/Middleware/ErrorHandlingMiddleware.cs`) converts exceptions into an `ErrorResponse<T>` instead of letting them propagate. Its constructor takes `ILogger<ErrorHandlingMiddleware>`.

- `ExecuteWithErrorHandlingAsync<T>(string operationName, Func<Task<T>> operation)` and `ExecuteWithErrorHandling<T>(string operationName, Func<T> operation)` return `ErrorResponse<T>`. On success, `Success` is `true` and `Data` holds the result. Any exception is caught, and the returned response has `Success = false`.
- An extension overload takes the operation name from `[CallerMemberName]`.

Exception mapping is by exact type. A derived type, such as `ArgumentNullException` (a subclass of `ArgumentException`), is not matched by its base type and falls through to `INTERNAL_ERROR`.

| Exception type | `ErrorCode` | `Message` | `IsRecoverable` |
|----------------|-------------|-----------|-----------------|
| `PipelineException` | `PIPELINE_ERROR` | The exception message | `true` |
| `TimeoutException` | `TIMEOUT` | `Operation timed out` | `true` |
| `InvalidOperationException` | `INVALID_OPERATION` | `Invalid operation` | `false` |
| `ArgumentException` | `INVALID_ARGUMENT` | `Invalid argument provided` | `false` |
| Anything else | `INTERNAL_ERROR` | `An internal error occurred` | `false` |

Recoverable errors are logged at Warning and the rest at Error, both with the exception attached. The failure response copies `ErrorCode`, `Message`, and `IsRecoverable`. It does not copy `Details`, which stays empty, so read the log to see the exception detail.

`ErrorResponse` has `ErrorCode` (default `"ERROR"`), `Message`, `IsRecoverable`, `Details`, and `Timestamp` (UTC, set at creation). `ErrorResponse<T>` adds `Success` and `Data`.

`RetryMiddleware` and `CircuitBreakerMiddleware` are in the same file.

### RetryMiddleware

`RetryMiddleware` retries an async operation with exponential backoff.

- Constructor: `RetryMiddleware(ILogger<RetryMiddleware> logger, int maxRetries = 3, int initialDelayMs = 100)`. The constants `DefaultMaxRetries` (3) and `DefaultInitialDelayMs` (100) hold the defaults.
- `ExecuteWithRetryAsync<T>(string operationName, Func<Task<T>> operation, Func<Exception, bool> shouldRetry)` calls the operation. If it throws and `shouldRetry` returns `true` while fewer than `maxRetries` retries have been made, it waits and calls again. The delay doubles after each retry.
- `maxRetries` counts retries after the first call, so the operation runs at most `maxRetries + 1` times. With the defaults, the waits are 100 ms, 200 ms, and 400 ms.
- When retries run out or `shouldRetry` returns `false`, the original exception propagates unchanged.
- There is no maximum delay and no jitter. There is no `CancellationToken` parameter.

### CircuitBreakerMiddleware

`CircuitBreakerMiddleware` has `ExecuteAsync<T>(string operationName, Func<Task<T>> operation)`. The constructor takes `ILogger<CircuitBreakerMiddleware>` and `int failureThreshold = 5`.

- The breaker opens after `failureThreshold` failures. In the closed state, a success does not reset the failure count.
- While open, calls throw `InvalidOperationException` without running the operation. After a fixed 30 seconds, the next call runs in the half-open state. A success there closes the breaker.
- It uses plain fields with no locking, so use one instance from one caller at a time.

### RateLimitingMiddleware

`RateLimitingMiddleware` (`src/Middleware/RateLimitingMiddleware.cs`) is a token bucket with one bucket per identifier. The constructor is `RateLimitingMiddleware(int tokensPerSecond = 1000, int maxBurstSize = 5000)`. The constants `DefaultTokensPerSecond` and `DefaultMaxBurstSize` hold these defaults.

- `tokensPerSecond` is the refill rate. Tokens are added in whole units: each refill adds `(long)(tokensPerSecond * elapsedSeconds)`. Each refill also moves the timestamp forward even when no whole token was added. If calls for one identifier arrive more often than once every `1 / tokensPerSecond` seconds, the bucket never refills, because the fractional tokens are discarded.
- `maxBurstSize` is the bucket capacity. A new bucket starts full at `maxBurstSize`, and the available count never exceeds it.
- `TryAcquire(string identifier, int tokensRequired = 1)` returns `true` and consumes the tokens if enough are available. Otherwise it returns `false` and consumes nothing. It never waits. A request for more than `maxBurstSize` tokens always returns `false`. A null or empty identifier throws `ArgumentException`.
- `GetStatus(string identifier)` returns a `RateLimitStatus` with `AvailableTokens`, `Capacity`, and `ResetTime`. `IsLimited` is `true` when `AvailableTokens` is below half of `Capacity`. For an identifier that has never been used, `AvailableTokens` is `tokensPerSecond` rather than `maxBurstSize`, and `ResetTime` keeps its default value.
- `Reset(string identifier)` removes the bucket, so the next call starts with a full bucket.
- `GetAllStatuses()` returns the status of every known identifier.

The extension methods are `IsRateLimitExceeded(string identifier)`, which returns `true` when `AvailableTokens` is 0 or less, and `GetStatusesNearLimit(int threshold)`, which returns the statuses at or below a token count.

`StageRateLimitingMiddleware` (same file) applies a separate limit to each pipeline stage. `RegisterStageLimit(string stageName, int itemsPerSecond, int burstSize)` adds a limit, and `CanProcessInStage(string stageName, int itemCount = 1)` checks it. A stage with no registered limit is always allowed. The registry is a plain `Dictionary`, so register all stages before you start processing.

### Composition example

The middleware types are independent of each other and of the pipeline. Compose them in the caller. The example below rate-limits by source, retries transient timeouts, times the call, and returns an `ErrorResponse<bool>` in every case:

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Middleware;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.Logging;

var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

var limiter = new RateLimitingMiddleware(tokensPerSecond: 100, maxBurstSize: 200);
var retry = new RetryMiddleware(loggerFactory.CreateLogger<RetryMiddleware>(), maxRetries: 2, initialDelayMs: 50);
var perf = new PerformanceLoggingMiddleware(loggerFactory.CreateLogger<PerformanceLoggingMiddleware>());
var errors = new ErrorHandlingMiddleware(loggerFactory.CreateLogger<ErrorHandlingMiddleware>());
var logging = new LoggingMiddleware(loggerFactory.CreateLogger<LoggingMiddleware>());
var correlation = new CorrelationMiddleware();

async Task<ErrorResponse<bool>> IngestAsync(PipelineOrchestrator pipeline, DataPoint point)
{
    return await correlation.WithCorrelationAsync(async _ =>
    {
        // 1. Rate limit per source. A denied request never reaches the pipeline.
        if (!limiter.TryAcquire(point.Source))
        {
            return new ErrorResponse<bool>
            {
                Success = false,
                ErrorCode = "RATE_LIMITED",
                Message = $"Source {point.Source} is over its limit",
                IsRecoverable = true
            };
        }

        logging.LogDataIngestion(point, "ingest");

        // 2. Error handling is outermost, so anything that escapes the retry becomes an ErrorResponse.
        // 3. Timing wraps the retried call, so the duration includes retry delays.
        return await errors.ExecuteWithErrorHandlingAsync("IngestDataPoint", () =>
            perf.MeasureAsync("IngestDataPoint", () =>
                retry.ExecuteWithRetryAsync(
                    "IngestDataPoint",
                    () => pipeline.IngestDataPointAsync(point),
                    ex => ex is TimeoutException)));
    });
}

ErrorResponse<bool> response = await IngestAsync(pipeline, new DataPoint(
    id: 42,
    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    value: 21.5,
    source: "sensor-a"));

if (!response.Success)
{
    Console.WriteLine($"{response.ErrorCode}: {response.Message} (recoverable: {response.IsRecoverable})");
}
```

The order in the example matters. The rate limit runs first, so denied requests are never retried. Retry sits inside the error handler, so a timeout that is still failing after the last retry is converted into a `TIMEOUT` response. `IngestDataPointAsync` returns `false` under backpressure without throwing, so the retry does not run for that case.

## Health Checks

The health check types are in `src/Monitoring/HealthCheckService.cs` (namespace `DotNetRealtimePipeline.Monitoring`). `HealthCheckService` and `ResourceMonitor` are not registered in `ServiceCollectionExtensions`. `PipelineInitializer` calls `GetRequiredService<HealthCheckService>()` and `GetRequiredService<ResourceMonitor>()`, and logs a warning if either fails. So either register both in your container or construct them yourself, as the example below does. `HealthCheckServiceExtensions.cs` adds helpers such as `GetOverallHealthAsync`, `GetHealthSummaryAsync`, and `RegisterComponents`, and `HealthCheckServiceValidation.cs` has the validators.

### HealthCheckService

`HealthCheckService` is a sealed class. Its constructor takes a `PipelineOrchestrator` and an `ILogger<HealthCheckService>`, and throws `ArgumentNullException` for either null argument.

- `RegisterComponent(string name, Func<Task<ComponentHealth>> healthCheck)` adds a named check. It throws `ArgumentException` for a null or whitespace name and `ArgumentNullException` for a null delegate. It does not reject duplicate names. Components run in registration order. If two components share a name, the later result overwrites the earlier one in `SystemHealthReport.Components`, but both still run.
- `PerformCompleteHealthCheckAsync()` runs every registered check one after another, with no timeout. It then reads `PipelineOrchestrator.GetHealthReportAsync()` and returns a `SystemHealthReport`. The method does not throw when a component fails:
  - If a check returns `IsHealthy = false`, the component is recorded as its result and its `ComponentStatus` is set to `Degraded`.
  - If a check throws, the component is recorded with `IsHealthy = false` and a `Message` of `Error: <exception message>`. Its `ComponentStatus` is set to `Unhealthy`. The exception is logged at Error.
  - If reading the pipeline health throws, the error is logged and `PipelineStatus`, `Throughput`, and `SuccessRate` keep their defaults (`null`, `0`, `0`).
- `GetQuickStatusAsync()` does not run any registered component. It reads `PipelineOrchestrator.GetStatus()` and `GetHealthReportAsync()` and returns a `QuickHealthStatus`. If either call throws, it logs the error and returns a status with `HealthStatus = "Error"` and every other field at its default.
- `GetComponentStatus(string componentName)` returns the stored `ComponentStatus` for a name. The lookup ignores case. It returns `Unknown` for a blank name, an unregistered name, or a component that has not been checked yet. The stored status changes only when `PerformCompleteHealthCheckAsync()` runs.

`OverallStatus` is computed from the component results only. The pipeline status does not affect it. The rule is:

| Components | Results with `IsHealthy = false` | `OverallStatus` |
|------------|----------------------------------|-----------------|
| 0 | any | `Unknown` |
| 3 | 0 | `Healthy` |
| 1 | 1 | `Unhealthy` |
| 2 | 1 | `Degraded` |
| 3 | 1 | `Degraded` |
| 3 | 2 | `Unhealthy` |
| 4 | 2 | `Degraded` |

The code uses `unhealthyCount > totalCount / 2` with integer division. The system is `Unhealthy` only when more than half of the components fail. Any other failure makes it `Degraded`.

### ComponentHealth

`ComponentHealth` is the result a check returns. It has these properties:

- `IsHealthy` (`bool`, default `false`).
- `Message` (`string`). There is no default, so it can be `null`.
- `CheckedAt` (`DateTime`, UTC). It is set when the object is created, not when the check finishes.
- `Details` (`Dictionary<string, object>`), initialized to an empty dictionary.

### ComponentStatus and SystemHealth

`ComponentStatus` has the values `Unknown`, `Healthy`, `Degraded`, and `Unhealthy`. `SystemHealth` has the same four values. They are separate enums: `SystemHealthReport.OverallStatus` is a `SystemHealth`, and `GetComponentStatus` returns a `ComponentStatus`.

### SystemHealthReport

`SystemHealthReport` is the result of `PerformCompleteHealthCheckAsync()`:

- `CheckedAt` (UTC) is set at the start of the check.
- `OverallStatus` (`SystemHealth`) is described above.
- `Components` (`Dictionary<string, ComponentHealth>`) holds one entry for each component that ran, keyed by name.
- `PipelineStatus` (`string`) is the `HealthReport.Status` text, such as `HEALTHY` or `UNHEALTHY`. It is `"Unknown"` when the pipeline returns no report, and `null` when reading the report throws.
- `Throughput` is items per second. `SuccessRate` is a percentage.

`ToString()` shows the counts, not the component details.

### QuickHealthStatus

`QuickHealthStatus` is a flat summary with no component checks:

- `IsRunning` comes from `PipelineStatus.IsRunning`.
- `HealthStatus` is the `HealthReport.Status` text, or `"Unknown"` when there is no report.
- `PendingItems` comes from `PipelineStatus.PendingItemsInQueue`.
- `ThroughputOk` is `true` when a report exists and its throughput is above 0.
- `ErrorRateAcceptable` is `true` when `SuccessRatePercent` is at least 80. A missing report counts as 100.

### ResourceMonitor and ResourceUsage

`ResourceMonitor` (constructor takes `ILogger<ResourceMonitor>`) reads the current process:

- `GetResourceUsage()` returns a `ResourceUsage` snapshot. It reads `Process.GetCurrentProcess()`. If that throws, the error is logged and a zeroed snapshot is returned with `CheckedAt` set.
- `IsResourceUsageAcceptable(double maxMemoryMB = 1024, int maxThreads = 100)` returns `true` when memory is below `maxMemoryMB` and the thread count is below `maxThreads`. Both comparisons are strict.

`ResourceUsage` has these properties:

- `MemoryMB` is the working set in megabytes.
- `ThreadCount` is the process thread count.
- `CpuUsagePercent` is always `0`. The CPU calculation is not implemented, and the private `CalculateCpuUsage` helper is never called.
- `PrivateMemoryMB` is also the working set. It is not the private memory, so do not compare it with the Windows "Private Bytes" counter.
- `CheckedAt` (UTC) is the time of the snapshot.

### Example

The example registers a component that checks the queue depth, runs a full health check, and reads the report. It does not start the pipeline. `HealthCheckService` is constructed directly because it is not registered in the DI container.

```csharp
using DotNetRealtimePipeline.Monitoring;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.Logging;

var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();

var health = new HealthCheckService(
    orchestrator,
    loggerFactory.CreateLogger<HealthCheckService>());

health.RegisterComponent("queue", () =>
{
    int pending = orchestrator.GetStatus().PendingItemsInQueue;

    return Task.FromResult(new ComponentHealth
    {
        IsHealthy = pending < 1000,
        Message = $"{pending} items pending",
        Details = new Dictionary<string, object> { ["pending"] = pending }
    });
});

// Runs every component, then reads the pipeline health. It does not throw on component failures.
SystemHealthReport report = await health.PerformCompleteHealthCheckAsync();

Console.WriteLine($"Overall: {report.OverallStatus} (checked at {report.CheckedAt:O})");

string pipelineStatus = report.PipelineStatus ?? "unavailable";
Console.WriteLine($"Pipeline: {pipelineStatus}, throughput {report.Throughput:F2}/s, success {report.SuccessRate:F1}%");

foreach (var (name, component) in report.Components)
{
    Console.WriteLine($"{name}: {(component.IsHealthy ? "OK" : "FAIL")} - {component.Message}");
}

// The stored status is updated by the same call, so it matches the report.
ComponentStatus queueStatus = health.GetComponentStatus("queue");
if (queueStatus != ComponentStatus.Healthy)
{
    Console.WriteLine($"Queue needs attention: {queueStatus}");
}

// Cheap check without running the components.
QuickHealthStatus quick = await health.GetQuickStatusAsync();
Console.WriteLine($"Running: {quick.IsRunning}, pending: {quick.PendingItems}, health: {quick.HealthStatus}");

// Process memory and thread count, for example as a guard before starting more work.
var resources = new ResourceMonitor(loggerFactory.CreateLogger<ResourceMonitor>());
ResourceUsage usage = resources.GetResourceUsage();
Console.WriteLine($"Memory: {usage.MemoryMB:F1} MB, threads: {usage.ThreadCount}");
```

With only the `queue` component registered, a backlog of 1000 or more items makes `queue` `Degraded` and `OverallStatus` `Unhealthy`, because the single component is unhealthy and `1 > 1 / 2` (that is, `1 > 0`) holds. A check that throws also gives the component `ComponentStatus.Unhealthy`, and the overall status is `Unhealthy` in that case too.

The example was not compiled or run against the repository. Check the exact names and namespaces before relying on it.

## State Management

The state types are in `src/State/PipelineStateManager.cs` (namespace `DotNetRealtimePipeline.State`). `PipelineStateManagerExtensions.cs` adds query helpers, and `PipelineStateManagerValidation.cs` has `Validate()`, `IsValid()`, and `EnsureValid()` for `PipelineStateManager`. None of these types are registered in `ServiceCollectionExtensions`. `PipelineInitializer` resolves `PipelineStateManager` from its service provider, so either register it yourself or construct the types directly, as the example below does.

### PipelineStateManager

`PipelineStateManager` is a sealed class that holds the pipeline lifecycle state. It starts in `PipelineState.Stopped`. Its constructor takes an `ILogger<PipelineStateManager>`.

- `CurrentState` returns the current `PipelineState`. `IsOperational` is `true` only when the state is `Running`.
- `TransitionTo(PipelineState newState, string reason = "")` returns `true` if the transition is allowed and `false` otherwise. An invalid transition does not throw. It logs a warning and leaves the state unchanged. A successful transition is appended to the history and listeners are notified.
- `GetStateHistory()` returns a copy of the transition list, in the order the transitions happened.
- `GetCurrentStateDuration()` returns the time since the last transition, or `TimeSpan.Zero` if there has been none.

The allowed transitions are:

| From | To |
|------|----|
| `Stopped` | `Running` |
| `Running` | `Paused`, `Stopped` |
| `Paused` | `Running`, `Stopped` |
| `Failed` | `Stopped` |
| any state | `Failed` |

Any other pair is rejected. `Stopped` to `Stopped` and `Running` to `Running` are rejected too. `Initializing` is defined in the enum, but no rule allows a transition into it, so `TransitionTo(PipelineState.Initializing)` always returns `false`. Treat `Initializing` as unused by this manager.

### RegisterStateChangeListener

`RegisterStateChangeListener(Action<PipelineState, PipelineState> listener)` adds a callback that receives `(oldState, newState)`. It throws `ArgumentNullException` for a null listener.

- Listeners run synchronously inside `TransitionTo`, while the manager's lock is held. Keep them short, and do not call `TransitionTo` from inside a listener.
- An exception thrown by a listener is logged at Error and does not stop the other listeners or the transition.
- There is no way to unregister a listener. Listeners are kept in a plain `List`, which is not thread-safe, so register them before the pipeline starts changing state.

### StateTransition

`StateTransition` is a class that records one transition. It has the properties `FromState`, `ToState`, `Timestamp` (UTC, set by `TransitionTo`), and `Reason`. `Reason` is `""` when `TransitionTo` is called without a reason.

### PipelineStateManagerExtensions

These extension methods work on `PipelineStateManager`:

- `GetTransitionsTo(PipelineState)` returns the transitions whose `ToState` matches.
- `GetLastTransition()` returns the most recent transition, or `null`.
- `GetTotalTimeInState(PipelineState)` returns the cumulative time spent in a state, including the open interval if the manager is in that state now.
- `ToHistoryString()` returns one line per transition, in the form `Timestamp | From → To | Reason`.
- `GetTransitionCount()` returns the number of recorded transitions.

### ConfigurationStateManager

`ConfigurationStateManager` is a sealed class that stores named runtime overrides in a `ConcurrentDictionary<string, object>`. Its constructor takes an `ILogger<ConfigurationStateManager>`.

- `SetOverride(string key, object value)` stores the value and logs it at Information. It throws `ArgumentException` for a null or whitespace key and `ArgumentNullException` for a null value. An existing key is replaced.
- `GetOverride<T>(string key, T defaultValue = default)` returns the stored value if it exists and is of type `T`. Otherwise it returns `defaultValue`, including when the stored value has a different type.
- `RemoveOverride(string key)` returns `true` if the key was removed.
- `GetAllOverrides()` returns a copy of all overrides.
- `ClearAllOverrides()` removes all overrides and logs the action.

### OperationMetricsTracker and OperationMetrics

`OperationMetricsTracker` is a sealed class that keeps one `OperationMetrics` object per operation name. Its constructor takes an `ILogger<OperationMetricsTracker>`.

- `RecordOperation(string operationName, long durationMs, bool success)` adds one execution. It throws `ArgumentException` for a null or whitespace name. The first call for a name creates its `OperationMetrics`.
- `GetOperationMetrics(string operationName)` returns the live `OperationMetrics` object for that name, or `null` if the name has not been recorded. It is not a snapshot. Its values change as more executions are recorded.
- `GetAllMetrics()` returns a new dictionary that contains the same live `OperationMetrics` objects.

`OperationMetrics` has these members:

- `TotalExecutions`, `SuccessfulExecutions`, and `FailedExecutions` (`long`).
- `TotalDurationMs` (`long`), `MinDurationMs` (`long`, starts at `long.MaxValue`), and `MaxDurationMs` (`long`).
- `AverageDurationMs` (`double`) is `TotalDurationMs / TotalExecutions`, or `0` when there are no executions.
- `SuccessRate` (`double`) is a percentage from `0` to `100`, or `0` when there are no executions.

The counters are updated with plain increments and assignments, not with `Interlocked` or a lock. Calls to `RecordOperation` for the same name from several threads can lose updates. If you record from multiple threads, serialize the calls or accept approximate numbers.

### Example

```csharp
using DotNetRealtimePipeline.State;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

var stateManager = new PipelineStateManager(loggerFactory.CreateLogger<PipelineStateManager>());
var config = new ConfigurationStateManager(loggerFactory.CreateLogger<ConfigurationStateManager>());
var metrics = new OperationMetricsTracker(loggerFactory.CreateLogger<OperationMetricsTracker>());

stateManager.RegisterStateChangeListener((from, to) =>
    Console.WriteLine($"State: {from} -> {to}"));

stateManager.TransitionTo(PipelineState.Running, "Startup complete");  // true
stateManager.TransitionTo(PipelineState.Paused, "Manual pause");       // true
stateManager.TransitionTo(PipelineState.Initializing);                 // false: no rule allows it

Console.WriteLine($"Transitions: {stateManager.GetTransitionCount()}"); // 2

config.SetOverride("Pipeline:BatchSize", 500);
int batchSize = config.GetOverride("Pipeline:BatchSize", 100);          // 500
string label = config.GetOverride<string>("Pipeline:BatchSize", "n/a"); // "n/a": stored value is not a string

metrics.RecordOperation("ProcessBatch", 42, true);
metrics.RecordOperation("ProcessBatch", 18, false);

OperationMetrics? batch = metrics.GetOperationMetrics("ProcessBatch");
if (batch is not null)
{
    Console.WriteLine($"{batch.TotalExecutions} runs, avg {batch.AverageDurationMs:F1} ms, success {batch.SuccessRate:F0}%");
}
// Output: 2 runs, avg 30.0 ms, success 50%
```

The example was not compiled or run against the repository. Check the exact names and namespaces before relying on it.

## REST API Handlers

The handler types are in `src/API/RestApiHandler.cs` (namespace `DotNetRealtimePipeline.API`). `ApiEndpointHandlerExtensions.cs` in the same folder adds helper methods for `ApiEndpointHandler`; this section does not cover them. The handlers are not registered in `ServiceCollectionExtensions`, so create them yourself with a `PipelineOrchestrator` and an `ILogger<T>`.

The file contains these types:

- `ApiEndpointHandler` is an abstract base class. Its constructor takes an `ILogger` and throws `ArgumentNullException` when it is `null`. It also holds the status code constants (`StatusOk` 200, `StatusBadRequest` 400, `StatusTooManyRequests` 429, `StatusInternalServerError` 500) and the nested generic `ApiEndpointHandler.ApiResponse<T>`.
- `DataIngestionHandler` (sealed) has `IngestAsync(DataPoint)` and `IngestBatchAsync(List<DataPoint>)`. It returns `ApiResponse<bool>` and `ApiResponse<BatchIngestResult>`.
- `BatchIngestResult` (sealed) has `SuccessfulCount`, `FailedCount`, and `TotalCount`.
- `StatusHandler` (sealed) has `GetStatusAsync()`. It returns `ApiResponse<PipelineStatusInfo>`.
- `PipelineStatusInfo` (sealed) is the payload for the status endpoint.
- `QueryHandler` (sealed) has `QueryAsync(long startMs, long endMs, string source = "", int minQuality = 0)`. It returns `ApiResponse<List<DataPoint>>`.
- `ApiErrorResponse` (sealed) is a separate error type with static factory methods. The handlers above do not return it.

### ApiEndpointHandler.ApiResponse<T>

Every handler method returns an `ApiResponse<T>` with these properties:

- `Success` is `true` for a successful request.
- `Data` holds the payload. It is `default` when the request fails.
- `Message` is a human-readable description.
- `StatusCode` is the HTTP status code the caller should use.
- `Timestamp` is set to `DateTime.UtcNow` when the object is created.

Handler methods do not throw. Each one catches exceptions, logs them, and returns a 500 response whose `Message` is the exception message.

### DataIngestionHandler

`IngestAsync(DataPoint dataPoint)`:

- A `null` data point returns 400 with `Message` "Data point is null".
- Otherwise it calls `PipelineOrchestrator.IngestDataPointAsync`. A `true` result returns 200 with `Data` set to `true`. A `false` result returns 429 with `Data` set to `false`, because the pipeline applied backpressure.

`IngestBatchAsync(List<DataPoint> dataPoints)`:

- A `null` or empty list returns 400 with `Message` "Data points list is empty".
- Otherwise it calls `PipelineOrchestrator.ProcessBatchDataPointsAsync` and returns 200 with a `BatchIngestResult`. `Success` is `true` even when `FailedCount` is greater than 0. Check `Data.FailedCount` to see whether any items failed.
- `TotalCount` is `dataPoints.Count`, the size of the input list.

`BatchIngestResult.ToString()` returns `BatchIngestResult { Total=..., Successful=..., Failed=... }`.

### StatusHandler

`GetStatusAsync()` reads `PipelineOrchestrator.GetStatus()` and then `GetHealthReportAsync()`. It returns 200 with a `PipelineStatusInfo`. The health report may be `null`. In that case `HealthStatus` is `null`, and `Throughput`, `SuccessRate`, and `AverageLatency` are `0`.

`PipelineStatusInfo` has these properties:

| Property | Type | Source |
|----------|------|--------|
| `PipelineName` | `string` | configuration name |
| `Version` | `string` | configuration version |
| `IsRunning` | `bool` | pipeline status |
| `TotalProcessed` | `long` | processed data point count |
| `TotalFailed` | `long` | failed data point count |
| `Pending` | `int` | items waiting in the queue |
| `HealthStatus` | `string` | health status name |
| `Throughput` | `double` | items per second |
| `SuccessRate` | `double` | percentage |
| `AverageLatency` | `double` | milliseconds |

### QueryHandler

`QueryAsync` gets the query service from `PipelineOrchestrator.GetQueryService()` and calls `SearchDataPointsAsync(startMs, endMs, source, minQuality)`. It returns 200 with the matching `DataPoint` list, and `Message` reports the number of results. The handler does not check the time range or the other arguments. It passes them through unchanged.

### ApiErrorResponse

`ApiErrorResponse` has `StatusCode`, `Message`, `ErrorCode`, and `Timestamp`. Each static factory method sets a fixed status code and error code:

| Factory method | StatusCode | ErrorCode |
|----------------|------------|-----------|
| `BadRequest(string message)` | 400 | `BAD_REQUEST` |
| `NotFound(string message)` | 404 | `NOT_FOUND` |
| `InternalError(string message)` | 500 | `INTERNAL_ERROR` |
| `TooManyRequests(string message)` | 429 | `RATE_LIMIT` |

The rate-limit factory is named `TooManyRequests`, not `RateLimited`. Its error code is `RATE_LIMIT`.

`ToString()` returns `ApiErrorResponse { StatusCode=..., ErrorCode=..., Message=... }`.

### Example

```csharp
using DotNetRealtimePipeline.API;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.Logging;

// orchestrator is a PipelineOrchestrator resolved from your service provider.
var ingestion = new DataIngestionHandler(orchestrator, loggerFactory.CreateLogger<DataIngestionHandler>());
var status = new StatusHandler(orchestrator, loggerFactory.CreateLogger<StatusHandler>());

var batch = await ingestion.IngestBatchAsync(new List<DataPoint> { point1, point2 });
Console.WriteLine($"{batch.StatusCode}: {batch.Message}");  // 200: Batch processed: 2 successful, 0 failed
if (batch.Data is not null)
{
    Console.WriteLine(batch.Data);                            // BatchIngestResult { Total=2, Successful=2, Failed=0 }
}

var empty = await ingestion.IngestBatchAsync(new List<DataPoint>());
Console.WriteLine($"{empty.StatusCode}: {empty.Message}");   // 400: Data points list is empty

var snapshot = await status.GetStatusAsync();
Console.WriteLine($"{snapshot.StatusCode}: {snapshot.Data?.PipelineName}");

ApiErrorResponse error = ApiErrorResponse.NotFound("Unknown pipeline");
Console.WriteLine(error);
// Output: ApiErrorResponse { StatusCode=404, ErrorCode=NOT_FOUND, Message=Unknown pipeline }
```

The example was not compiled or run against the repository. The sample output for the batch call assumes both data points are ingested successfully. Check the exact names and namespaces before relying on it.

## Webhooks

`src/Integration/WebhookHandler.cs` (namespace `DotNetRealtimePipeline.Integration`) has the outbound and inbound webhook types. The file contains these types:

- `WebhookHandler` (sealed) sends pipeline events as HTTP POST requests to subscribed URLs. It has `Subscribe`, `Unsubscribe`, `SendWebhookEventAsync`, and `GetSubscriptions`. The constructor takes an `HttpClient` and an `ILogger<WebhookHandler>`. A `null` argument throws `ArgumentNullException`.
- `WebhookSubscription` (sealed) is one registered endpoint.
- `WebhookEventType` is a `[Flags]` enum of the events a subscription can receive.
- `WebhookPayload` (sealed) is the JSON body sent to subscribers.
- `InboundWebhookHandler` (sealed) takes payloads from external senders and passes them to registered callbacks.

Like the REST handlers, these types are not registered in `ServiceCollectionExtensions`. Create them yourself. Use a long-lived `HttpClient`, for example from `IHttpClientFactory`, and set a timeout on it.

### WebhookHandler

`Subscribe(string url, WebhookEventType eventTypes, string secret = null)`:

- A `null`, empty, or whitespace `url` throws `ArgumentException`.
- It adds a subscription with a new GUID `Id`, `IsActive` set to `true`, and `CreatedAt` set to the current UTC time. It returns nothing. Read the `Id` from `GetSubscriptions()`.
- Subscribing the same URL twice creates two subscriptions, and each one receives its own delivery.
- `secret` is stored on the subscription, but it is not used. Payloads are not signed, and no signature header is sent. Do not rely on the secret to verify senders.

`Unsubscribe(string url)`:

- A `null`, empty, or whitespace `url` throws `ArgumentException`.
- It removes every subscription whose `Url` matches exactly, and returns `true`. It returns `false` when nothing matched.

`GetSubscriptions()` returns a new list that holds the same `WebhookSubscription` objects. Changing a property on one of them changes the handler's state.

`SendWebhookEventAsync(WebhookEventType eventType, object eventData)`:

- A `null` `eventData` throws `ArgumentNullException`.
- It selects the subscriptions that are active and whose `EventTypes` include every flag in `eventType`. Passing a combined value such as `PipelineError | DataIngested` only matches subscriptions that have both flags.
- It sends to all selected subscriptions in parallel and waits for all of them to finish (`Task.WhenAll`). If nothing matches, it returns immediately.
- No `CancellationToken` parameter exists, so the call cannot be cancelled.

Each request is an HTTP `POST` with a JSON body and the `application/json` content type. The body is a `WebhookPayload`:

```json
{
  "EventType": "PipelineError",
  "Data": { "Code": "E42", "Message": "Sink unavailable" },
  "Timestamp": "2026-11-04T10:15:30.1234567Z",
  "SubscriptionId": "5b7e2c1a-0f4d-4e8b-9a3c-2d6f1b9e7a10"
}
```

Delivery rules:

- A `2xx` response sets `LastDeliveredAt` to the current UTC time and resets `FailureCount` to `0`.
- A `400` or `409` response is not retried. The subscription is set to `IsActive = false`. It stays in the list, but later sends skip it. Nothing in the class sets it back to `true`, although the property has a public setter.
- An `HttpRequestException` counts as one attempt. A delivery makes up to 3 attempts. The handler waits 1 second after the first failure and 2 seconds after the second. It does not wait after the third.
- When all 3 attempts fail, `FailureCount` goes up by one. When it reaches 5, the subscription is set to `IsActive = false`. The count is per delivery, so five consecutive failed events disable a subscription.
- Other non-success statuses, such as `500` or `503`, are not counted, and the loop does not wait before it sends again. The loop only ends on a `2xx`, a `400`, a `409`, or an exception. A subscriber that keeps returning `500` will receive requests without a limit until its response changes. Do not point a subscription at an endpoint that can return a `5xx` status until this is fixed.
- Exceptions other than `HttpRequestException` are not caught. An `HttpClient` timeout throws `TaskCanceledException`, which is an example. It is raised from `SendWebhookEventAsync` after the other deliveries in the same call have finished.

`WebhookEventType` values:

| Flag | Value |
|---|---|
| `DataIngested` | 1 |
| `ProcessingCompleted` | 2 |
| `BackpressureDetected` | 4 |
| `MetricsCollected` | 8 |
| `PipelineError` | 16 |
| `All` | all five flags |

### WebhookSubscription

Public properties: `Id`, `Url`, `EventTypes`, `Secret`, `CreatedAt`, `LastDeliveredAt` (`DateTime?`, `null` until the first success), `IsActive`, and `FailureCount`. `ToString()` shows whether a secret is set, but never its value.

### WebhookPayload

Public properties: `EventType` (the enum name as a string), `Data` (the object passed to `SendWebhookEventAsync`, serialized with `System.Text.Json`), `Timestamp` (UTC), and `SubscriptionId`. `ToString()` prints all four.

### InboundWebhookHandler

The constructor takes an `ILogger<InboundWebhookHandler>`. A `null` logger throws `ArgumentNullException`.

`RegisterHandler(Func<WebhookPayload, Task> handler)` adds a callback. A `null` handler throws `ArgumentNullException`. The list is not locked, so register all handlers during startup, before you call `ProcessWebhookAsync` from several threads.

`ProcessWebhookAsync(WebhookPayload payload)`:

- A `null` payload logs a warning and returns.
- Otherwise it logs the `EventType` at `Information` level and awaits each handler in registration order.
- If a handler throws, the exception is logged at `Error` level and the next handler still runs. The method does not throw because of a handler failure.
- It does not check a signature or any authentication header, and it does not parse HTTP requests. Verify the request in your endpoint first, then build the `WebhookPayload` from the body.

### Example

```csharp
using DotNetRealtimePipeline.Integration;
using Microsoft.Extensions.Logging.Abstractions;

var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
var outbound = new WebhookHandler(httpClient, NullLogger<WebhookHandler>.Instance);

// Receives PipelineError and ProcessingCompleted events. The secret is stored but not sent.
outbound.Subscribe(
    "https://hooks.example.com/pipeline",
    WebhookEventType.PipelineError | WebhookEventType.ProcessingCompleted,
    "shared-secret");

// Delivered to every active subscription whose flags include PipelineError.
await outbound.SendWebhookEventAsync(
    WebhookEventType.PipelineError,
    new { Code = "E42", Message = "Sink unavailable" });

var subscription = outbound.GetSubscriptions()[0];
Console.WriteLine(subscription);  // WebhookSubscription { ..., Secret = set, ... }

outbound.Unsubscribe("https://hooks.example.com/pipeline");  // true

var inbound = new InboundWebhookHandler(NullLogger<InboundWebhookHandler>.Instance);
inbound.RegisterHandler(payload =>
{
    Console.WriteLine($"Received {payload.EventType} for {payload.SubscriptionId}");
    return Task.CompletedTask;
});

// Call this from your endpoint after the request has been verified.
await inbound.ProcessWebhookAsync(new WebhookPayload
{
    EventType = nameof(WebhookEventType.DataIngested),
    Data = new { Source = "sensor-1" },
    Timestamp = DateTime.UtcNow,
    SubscriptionId = "external"
});
```

## Event Subscribers

The subscriber classes in `src/Events/EventSubscriber.cs` attach handlers to a `PipelineEventPublisher` and add a small amount of domain logic on top. `SubscriberOptions.cs` defines the settings that control how each handler's channel queues, dispatches, and handles errors.

| Class | Event (`nameof`) | Purpose |
|-------|------------------|---------|
| `EventSubscriberBase` | - | Abstract base. Holds the publisher and logger, tracks `_isSubscribed`, and provides `Subscribe()`, `Unsubscribe()`, `Start()`, `StopAsync()`, and the `GetSubscriberOptions()` default. |
| `DataIngestSubscriber` | `DataIngestedEvent` | Validates each ingested `DataPoint` (ID, source, quality, timestamp window, finite value within ±10000) and then calls the `protected virtual ProcessIngestedDataAsync` hook. Exposes the constant `MaxBatchSize` (10000), which the handler does not enforce. |
| `ProcessingCompletionSubscriber` | `ProcessingCompletedEvent` | Counts successes and failures and logs failures as warnings. `GetSuccessRatePercent()` returns 100 when nothing has completed yet. Extend through `protected virtual OnProcessingCompletionAsync`. |
| `BackpressureAlertSubscriber` | `BackpressureDetectedEvent` | Counts backpressure events and logs them with the stage name and buffer utilization. When utilization is above 95%, it calls `protected virtual OnCriticalBackpressureAsync`. Exposes `GetBackpressureEventCount()`. |
| `MetricsAggregationSubscriber` | `MetricsCollectedEvent` | Accumulates processing-time totals in 16 striped `Interlocked` counters, so the hot path does not take a lock. `GetAverageProcessingTime()`, `GetMetricsCount()`, and `Reset()` read or clear the totals. It is `sealed` and has no extension hook. |
| `ErrorAlertSubscriber` | `PipelineErrorEvent` | Counts pipeline errors and logs each one as an error. Calls `protected virtual OnErrorDetectedAsync` for custom handling. Exposes `GetErrorCount()`. |

Usage rules that apply to these subscribers:

- Subscriber handlers are `private`. To customize behavior, override the `protected virtual` hook named in the table, not the handler.
- `Subscribe()` calls `PipelineEventPublisher.Subscribe<T>` with the options from `GetSubscriberOptions()`. Call it once per instance, and call `publisher.Start()` after all subscriptions are in place. Channels consume events only after `Start()`.
- The default options are `MaxQueueSize = 1000`, `MaxQueueSizeBehavior = DropNew`, `DispatchMode = Sequential`, `ErrorPolicy = SwallowAndCount`, and `MaxDegreeOfParallelism = 1`.
- `BackpressureAlertSubscriber` and `ErrorAlertSubscriber` override `GetSubscriberOptions()` to use `MaxQueueSize = int.MaxValue` with `MaxQueueSizeBehavior.Block`, so alerts are not dropped when the queue fills. Both use sequential dispatch.
- `ProcessingCompletionSubscriber`, `BackpressureAlertSubscriber`, `MetricsAggregationSubscriber`, and `ErrorAlertSubscriber` catch exceptions thrown by their handlers and log them, so the channel's `ErrorPolicy` does not see those exceptions. `DataIngestSubscriber` does not catch them, so its validation exceptions reach the channel's `ErrorPolicy`.
- `EventSubscriberBase.Start()` and `StopAsync()` are no-ops and exist as virtual extension points. The publisher's `Start()` and `StopAsync()` control the channels.

### SubscriberOptions

`SubscriberOptions` (`src/Events/SubscriberOptions.cs`) is a mutable `sealed` class passed to `PipelineEventPublisher.Subscribe<T>(eventName, handler, options)`. Its `ToString()` returns a summary of all settings, which is useful in logs.

| Property | Default | Meaning |
|----------|---------|---------|
| `MaxQueueSize` | `1000` (`DefaultMaxQueueSize`) | Capacity of the subscriber's bounded channel. |
| `MaxQueueSizeBehavior` | `DropNew` | What happens when the queue is full. See below. |
| `DispatchMode` | `Sequential` | `Sequential` uses one consumer task. `Parallel` uses `Parallel.ForEachAsync` bounded by `MaxDegreeOfParallelism`. |
| `MaxDegreeOfParallelism` | `1` (`DefaultMaxDegreeOfParallelism`) | Concurrency limit, used only in `Parallel` mode. |
| `ErrorPolicy` | `SwallowAndCount` | What happens when a handler throws. See below. |
| `Name` | `null` | Label for logs and diagnostics. When it is `null`, the publisher falls back to the handler's declaring type name, then `AnonymousSubscriber`. |

**`SubscriberErrorPolicy`** (the `ErrorPolicy` values). Every failure increments the channel's error count, which `publisher.GetSubscriberErrorCount(eventName)` reports, whatever the policy.

- `SwallowAndCount`: logs the error and continues with the next event.
- `DeadLetter`: logs the error and continues. The channel does not write to a dead-letter queue yet; the code has a comment saying a real system would do so.
- `FailFast`: logs the error and rethrows, which stops the consumer task.

**`MaxQueueSizeBehavior`** (the queue-full behavior). The channel maps these values as follows:

- `DropNew` maps to `BoundedChannelFullMode.DropNewest`. The incoming event is discarded.
- `Block` maps to `BoundedChannelFullMode.Wait`. The publisher waits until there is room.
- `Reject` maps to `BoundedChannelFullMode.DropOldest`. The oldest queued event is discarded and the new one is kept. Nothing throws, even though the enum's XML comment says it rejects with an exception.

**`SubscriberDispatchMode`**:

- `Sequential`: one background consumer processes events in order.
- `Parallel`: up to `MaxDegreeOfParallelism` events are processed at the same time. Ordering is not guaranteed.

### Example: subscribing to `PipelineEventPublisher`

You can subscribe a handler directly, or use one of the subscriber classes. Both work with the same publisher.

```csharp
using DotNetRealtimePipeline.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var publisher = new PipelineEventPublisher(loggerFactory.CreateLogger<PipelineEventPublisher>());

// 1. Direct subscription with default options (queue 1000, DropNew, sequential).
publisher.Subscribe<ProcessingCompletedEventArgs>(
    nameof(ProcessingCompletedEvent),
    args =>
    {
        Console.WriteLine($"Completed {args.Result.ResultId}: success={args.Result.Success}");
        return Task.CompletedTask;
    });

// 2. Direct subscription with explicit options: a parallel audit lane that waits instead of dropping.
publisher.Subscribe<DataIngestedEventArgs>(
    nameof(DataIngestedEvent),
    args =>
    {
        Console.WriteLine($"Audit: data point {args.DataPoint.Id}");
        return Task.CompletedTask;
    },
    new SubscriberOptions
    {
        Name = "ingest-audit",
        MaxQueueSize = 500,
        MaxQueueSizeBehavior = MaxQueueSizeBehavior.Block,
        DispatchMode = SubscriberDispatchMode.Parallel,
        MaxDegreeOfParallelism = 4,
        ErrorPolicy = SubscriberErrorPolicy.SwallowAndCount
    });

// 3. Built-in subscriber class: logs pipeline errors and keeps a count.
var errorAlerts = new ErrorAlertSubscriber(
    publisher,
    loggerFactory.CreateLogger<ErrorAlertSubscriber>());
errorAlerts.Subscribe();

// Start the channels after all subscriptions are registered.
publisher.Start();

await publisher.PublishPipelineErrorAsync("ingest", new InvalidOperationException("sensor offline"));

// Handlers run on their own channels. This delay only keeps the demo output deterministic;
// production code should wait on its own signal instead.
await Task.Delay(TimeSpan.FromMilliseconds(100));
Console.WriteLine($"Errors seen: {errorAlerts.GetErrorCount()}"); // 1

await publisher.StopAsync();
publisher.Dispose();
```

`PublishPipelineErrorAsync` returns once the event is queued on every channel, not once the handlers have run, so do not read `GetErrorCount()` straight after a publish call. `StopAsync()` cancels the consumer tasks instead of draining their queues, so events still queued at that point may never be handled. Wait for the work you need before stopping the publisher.

## Utilities

The helpers in `src/Utilities/` are shared support code for batching, compression, paths, retries, serialization, statistics, and validation. Most are `public sealed` classes with companion `*Extensions.cs` and `*Validation.cs` files. `PipelineJsonUtilities` is `internal`.

| Class | Purpose |
|-------|---------|
| `BatchProcessor<TInput, TOutput>` | Splits a collection into batches and runs them through `ProcessAsync` with bounded parallelism. Also provides `BatchProcessingException`, `BatchProcessingProgress`, and the pipeline-specific `DataPointBatchProcessor`. |
| `CompressionHelper` | GZIP and Deflate compression for strings and byte arrays, async GZIP file compress and decompress, and compression ratio and efficiency analysis (`CompressionStats`, `CompressionComparison`). |
| `DateTimeExtensions` | Extension methods that convert to and from Unix milliseconds, compute time-window boundaries and age, and check whether a timestamp falls inside a window. |
| `PathHelper` (incl. `FileSystemMonitor`) | Validates and normalizes paths, combines and sanitizes path segments, generates unique filenames, reports disk and directory sizes, and ensures directories exist. `FileSystemMonitor` raises create, change, and delete events for a watched path. |
| `PerformanceHelper` | Times sync and async operations, benchmarks them over repeated runs, and reads memory stats. `PerformanceTracker` records metric samples and computes statistics over a time window. |
| `PipelineJsonUtilities` (internal) | Shared `JsonSerializerOptions` (web defaults, string enums, camelCase) and size and length guards that reject oversized JSON before deserialization. |
| `RetryHelper` (`RetryPolicyBuilder` / `RetryPolicy` / `RetryStatistics`) | Retries async and sync operations with exponential backoff and optional jitter. `RetryPolicyBuilder` fluently builds a `RetryPolicy`, and `RetryStatistics` tracks attempts, successes, failures, and events. |
| `SerializationHelper` (`BatchSerializationHelper` / `DateTimeSerializationHelper` / `DictionaryConversionHelper`) | JSON and CSV serialization of data points, processing results, and metrics. Also covers file-based batch read and write, ISO 8601 timestamp conversion, and object-to-dictionary conversion. |
| `StatisticsHelper` | Computes mean, median, standard deviation, percentile, coefficient of variation, moving average, rate of change, IQR outliers, and Pearson correlation. Results are returned as a record. |
| `ValidationHelper` | Validates data points, pipeline configurations, processing results, and window events, returning `ValidationResult` objects. Also checks time ranges and value bounds. |

## Dependency Injection

Two files set up the container and start the pipeline:

- `src/Configuration/ServiceCollectionExtensions.cs` (namespace `DotNetRealtimePipeline.Configuration`) is the composition root. `AddPipelineServices` registers the repositories, the core services, the dead-letter queue, the retry policy, and the HTTP client factory.
- `src/Initialization/PipelineInitializer.cs` (namespace `DotNetRealtimePipeline.Initialization`) runs the startup sequence. It resolves many types that `AddPipelineServices` does not register, so those have to be added to the container separately.

### What `AddPipelineServices` registers

Every registration is a singleton. Nothing is scoped or transient, so one instance of each type lives for the lifetime of the root `IServiceProvider`.

| Service | Registration |
|---------|--------------|
| `IDataPointRepository` | `InMemoryDataPointRepository` |
| `IMetricsRepository` | `InMemoryMetricsRepository` |
| `PipelineConfig` | The instance passed in, or the default configuration |
| `DataProcessingService`, `WindowingService`, `MetricsService`, `BackpressureService`, `QueryService` | Concrete type |
| `PipelineOrchestrator` | Concrete type. Its constructor takes the five services above and the `PipelineConfig`. |
| `PipelineVisualizer` | Concrete type |
| `BackpressureMetricsCollector` | Concrete type |
| `IDeadLetterQueue` | `DeadLetterQueue` |
| `RetryPolicyOptions` | One instance, created during registration |
| `IRetryPolicy` | Factory that returns `ExponentialBackoffRetryPolicy` built on the same `RetryPolicyOptions` instance |
| `PipelineHttpClientFactory` | Concrete type |

There are three overloads, and they differ only in where the `PipelineConfig` comes from:

- `AddPipelineServices(PipelineConfig, Action<RetryPolicyOptions>? = null)` uses the instance you pass in.
- `AddPipelineServices(Action<RetryPolicyOptions>? = null)` uses the default configuration.
- `AddPipelineServices(Action<PipelineConfig>, Action<RetryPolicyOptions>? = null)` starts from the default configuration and lets you change it.

The default configuration has ID `1`, the name `DefaultPipeline`, version `1.0.0`, and six stages: `Ingestion` (SOURCE), `Validation` (FILTER), `Transformation` (TRANSFORM), `Windowing` (WINDOW), `Aggregation` (AGGREGATE), and `Output` (SINK).

The `configureRetryPolicy` callback runs once, during the `AddPipelineServices` call. It does not run when the retry policy is resolved.

### What `PipelineInitializer` needs

`PipelineInitializer` is not registered by `AddPipelineServices`, so create it yourself. Its constructor takes `IServiceProvider` and `ILogger<PipelineInitializer>`. It calls `GetRequiredService<PipelineStateManager>()` in the constructor, so construction throws if `PipelineStateManager` is not registered.

These types are resolved during initialization, and `AddPipelineServices` does not register any of them:

| Type | Resolved in |
|------|-------------|
| `PipelineStateManager` | Constructor, and every state transition |
| `PipelineEventPublisher` | Event subscribers step |
| `DataIngestSubscriber`, `ProcessingCompletionSubscriber`, `BackpressureAlertSubscriber`, `MetricsAggregationSubscriber`, `ErrorAlertSubscriber` | Event subscribers step |
| `LoggingMiddleware`, `ErrorHandlingMiddleware`, `RateLimitingMiddleware` | Middleware step |
| `BackgroundProcessingWorker`, `MetricsAggregationWorker`, `HealthCheckWorker` | Workers step |
| `WorkerCoordinator` | `StartAsync` and `StopAsync` |
| `HealthCheckService`, `ResourceMonitor` | Monitoring step |

All of these are registered as singletons in the example below. The constructor parameters that have default values, such as `RateLimitingMiddleware`'s token settings and the worker intervals, use their defaults.

### Initialization sequence

`InitializeAsync` runs these steps in order:

1. Logs the start and moves the state to `Initializing`.
2. **Core services**: resolves `PipelineOrchestrator`, `DataProcessingService`, and `MetricsService`.
3. **Event subscribers**: resolves `PipelineEventPublisher` and the five subscribers, then calls `Subscribe()` on each subscriber.
4. **Middleware**: resolves the three middleware types.
5. **Workers**: resolves the three background workers. It does not start them.
6. **Integrations**: resolves `PipelineHttpClientFactory`. A failure here is logged as a warning, and initialization continues.
7. **Monitoring**: resolves `HealthCheckService` and `ResourceMonitor`. A failure here is logged as a warning, and initialization continues.
8. Sets `IsInitialized` to `true` and returns `InitializationResult` with `Success = true`. `ComponentsInitialized` is a fixed `8`, not a count of the steps above.

A failure in steps 2 to 5 stops the sequence. The state moves to `Failed`, and the method returns `Success = false` with `ErrorMessage` set. The exception is not rethrown.

`InitializeAsync` has no guard against repeated calls. Calling it again runs every step again, including `Subscribe()` on each subscriber.

`StartAsync` returns `false` if the pipeline is not initialized. Otherwise it starts the orchestrator, moves the state to `Running`, and calls `WorkerCoordinator.StartAll()`. `StopAsync` calls `WorkerCoordinator.StopAllAsync()`, then `PipelineOrchestrator.StopAsync()`, then moves the state to `Stopped`.

Neither `InitializeAsync` nor `StartAsync` calls `PipelineEventPublisher.Start()`. Nothing else in `src/` calls it either, so the subscribers' channels do not run until your code calls it. Call it after `InitializeAsync` and before you publish events.

`PipelineInitializerExtensions.cs` adds `InitializeAndStartAsync`, `InitializeWithRetryAsync`, and `SafeStopAsync`. Those wrap the methods above.

### Example

This follows the layout of `Program.cs`. It registers the core services, then the types that `PipelineInitializer` resolves, then runs the initialization sequence.

```csharp
using DotNetRealtimePipeline.Configuration;
using DotNetRealtimePipeline.Events;
using DotNetRealtimePipeline.Initialization;
using DotNetRealtimePipeline.Middleware;
using DotNetRealtimePipeline.Monitoring;
using DotNetRealtimePipeline.State;
using DotNetRealtimePipeline.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Core registrations. The first callback customizes the default PipelineConfig,
// and the second customizes the retry policy.
services.AddPipelineServices(
    config =>
    {
        config.PipelineName = "RealTime-DataPipeline";
        config.Version = "1.0.0";
        config.WindowSizeMs = 5000;
        config.WindowSlideMs = 1000;
    },
    retry =>
    {
        retry.MaxAttempts = 3;
        retry.BaseDelay = TimeSpan.FromMilliseconds(200);
    });

// Types that PipelineInitializer resolves but AddPipelineServices does not register.
services.AddSingleton<PipelineStateManager>();
services.AddSingleton<PipelineEventPublisher>();
services.AddSingleton<DataIngestSubscriber>();
services.AddSingleton<ProcessingCompletionSubscriber>();
services.AddSingleton<BackpressureAlertSubscriber>();
services.AddSingleton<MetricsAggregationSubscriber>();
services.AddSingleton<ErrorAlertSubscriber>();
services.AddSingleton<LoggingMiddleware>();
services.AddSingleton<ErrorHandlingMiddleware>();
services.AddSingleton<RateLimitingMiddleware>();
services.AddSingleton<BackgroundProcessingWorker>();
services.AddSingleton<MetricsAggregationWorker>();
services.AddSingleton<HealthCheckWorker>();
services.AddSingleton<WorkerCoordinator>();
services.AddSingleton<HealthCheckService>();
services.AddSingleton<ResourceMonitor>();

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

var initializer = new PipelineInitializer(
    serviceProvider,
    serviceProvider.GetRequiredService<ILogger<PipelineInitializer>>());

var initResult = await initializer.InitializeAsync();
if (!initResult.Success)
{
    logger.LogError("Initialization failed: {Error}", initResult.ErrorMessage);
    return;
}

logger.LogInformation("{Result}", initResult.ToString());

// Subscribers are attached during InitializeAsync, but their channels only run after Start().
serviceProvider.GetRequiredService<PipelineEventPublisher>().Start();

if (await initializer.StartAsync())
{
    logger.LogInformation("Pipeline is running");
    await Task.Delay(TimeSpan.FromSeconds(10));
}

await initializer.StopAsync();
await serviceProvider.DisposeAsync();
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Test Suite

Unit test fixtures and test helpers for the pipeline library.

### CommandLineParserTests

The CommandLineParserTests class contains unit tests for the CommandLineParser class. These tests verify that the parser correctly handles various command-line scenarios, including parsing empty arguments, unknown commands, and commands with required and optional options.

Example usage:
```csharp
CommandLineParser parser = new CommandLineParser();
parser.Parse("--help");
```

### MetricsServiceTestsExtensions

The MetricsServiceTestsExtensions class provides extension methods for testing the MetricsService class. It includes methods for getting the service instance, setting up a mock repository, generating test data points, and verifying the results.

Example usage:
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Tests.Unit;
using FluentAssertions;
using Moq;
using Xunit;

[Fact]
public async Task CreateMetricAggregation_SavesOneAggregation()
{
    // Arrange: a fresh test instance creates a MetricsService backed by a mock repository
    var tests = new MetricsServiceTests();
    tests.VerifyMetricsService();
    MetricsService service = tests.GetService();

    // Configure the mock so SaveAsync echoes back whatever it receives
    tests.ConfigureMockRepository(repo =>
        repo.Setup(r => r.SaveAsync(It.IsAny<MetricAggregation>()))
            .ReturnsAsync((MetricAggregation m) => m));

    // Act
    MetricAggregation result = await service.CreateMetricAggregationAsync(0, 1000, 10, 0, 0);

    // Assert
    tests.VerifyTestResult(result);
    tests.VerifyRepositoryCall(m => m.MetricType == "STANDARD", Times.Once());
}

[Fact]
public void GenerateTestDataPoints_ReturnsRequestedCount()
{
    var tests = new MetricsServiceTests();

    // Generate sample data points on the test instance
    IReadOnlyList<DataPoint> points = tests.GenerateTestDataPoints(5);
    points.Should().HaveCount(5);

    // Make the mock repository return a fixed aggregation for SaveAsync and GetLatestAsync
    tests.SetupRepositoryToReturn(new MetricAggregation(1, 0, 1000, "STANDARD"));
}
```

### DataProcessingServiceTestsExtensions

The DataProcessingServiceTestsExtensions class provides static methods for creating test data and pipeline configurations for unit testing the DataProcessingService class.

Example usage:
```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Tests.Unit;

var tests = new DataProcessingServiceTests();

// Data points: valid (quality 90), low quality (quality 30), and an empty/zero-valued point
DataPoint valid = tests.CreateValidDataPoint(id: 42, value: 25.5, source: "Sensor2", quality: 90);
DataPoint lowQuality = tests.CreateLowQualityDataPoint(id: 43);
DataPoint invalid = tests.CreateInvalidDataPoint();

// Pipeline configuration with a custom quality threshold and retry count
PipelineConfig config = tests.CreateTestPipelineConfig(minDataQualityThreshold: 60, maxRetries: 3);

// Processing results: one marked successful, one marked failed with a custom message
ProcessingResult ok = tests.CreateSuccessfulResult(resultId: 1, stageName: "Output");
ProcessingResult failed = tests.CreateFailedResult(resultId: 2, errorMessage: "Timeout");

bool passes = valid.MeetsQualityThreshold(config.MinDataQualityThreshold); // true (90 >= 60)
```

### PipelineEventPublisherJsonExtensionsTests

The PipelineEventPublisherJsonExtensionsTests class validates the JSON serialization and deserialization functionality for PipelineEventPublisher, ensuring proper formatting, null handling, and robust parsing through both direct and Try-based methods.

Example usage:
```csharp
var publisher = new PipelineEventPublisher();

// Serialize to JSON (supports indentation)
string json = publisher.ToJson(indented: true);

// Deserialize from JSON
var deserialized = PipelineEventPublisherJsonExtensions.FromJson(json);

// Try to parse from JSON safely
if (PipelineEventPublisherJsonExtensions.TryFromJson(json, out var result))
{
    // Result is now an instance of PipelineEventPublisher
}
```

### PipelineEventPublisherValidationTests

The `PipelineEventPublisherValidationTests` class is an xUnit test fixture that verifies the publisher validation extension methods, including successful validation, null handling, exception behavior, and read-only validation results. xUnit discovers its public test methods automatically, while direct invocation can be useful when debugging a particular validation scenario.

Example usage:
```csharp
using DotNetRealtimePipeline.Tests;

var tests = new PipelineEventPublisherValidationTests();

tests.Validate_WithValidPublisher_ReturnsEmptyList();
tests.IsValid_WithNullPublisher_ThrowsArgumentNullException();
tests.EnsureValid_WithInvalidPublisher_ThrowsArgumentException();
tests.Validate_ReturnsReadOnlyList();
tests.Methods_AreExtensionMethodsForPipelineEventPublisher();
```

### ApiEndpointHandlerExtensionsTests

The ApiEndpointHandlerExtensionsTests class verifies that the ApiEndpointHandlerExtensions class correctly manages API response construction, ensuring that successful results, error responses, and paginated outputs are generated as expected. It includes comprehensive test coverage for edge cases, including null handling and invalid input parameters, guaranteeing reliability across all response scenarios.

Example usage:
```csharp
// Verify standard successful response creation
[Fact]
public void Ok_CreatesSuccessfulResponseWithData_Example()
{
    var handler = new ApiEndpointHandler();
    var data = new { Message = "Test" };
    var result = handler.Ok(data);
    Assert.Equal(200, result.StatusCode);
}
```

### RateLimitingMiddlewareConcurrencyTests

The RateLimitingMiddlewareConcurrencyTests class validates the thread-safety and correctness of the RateLimitingMiddleware under concurrent access.
It ensures that the rate limiter does not exceed its limit, that properties are thread-safe, and that concurrent operations on different identifiers do not interfere.

Example usage:
```csharp
// Create a rate limiter that allows 10 tokens per second with a max burst of 10.
var limiter = new RateLimitingMiddleware(tokensPerSecond: 10, maxBurstSize: 10);
string identifier = "my-api";

// Try to acquire a token (returns true if allowed, false if rate limited).
bool allowed = limiter.TryAcquire(identifier);

// Get current status (available tokens, capacity, reset time, etc.)
var status = limiter.GetStatus(identifier);
int availableTokens = status.AvailableTokens;

// Reset the limiter for a specific identifier (e.g., after a configuration change).
limiter.Reset(identifier);
```

### ApiEndpointHandlerValidationTests

The ApiEndpointHandlerValidationTests class is an xUnit test fixture that verifies validation behavior for API responses, batch ingestion results, and pipeline status information. It covers valid and invalid models as well as the exceptions raised for invalid or null values; xUnit normally discovers these public test methods automatically, but they can also be invoked directly when debugging a specific scenario.

Example usage:
```csharp
using DotNetRealtimePipeline.Tests;

var tests = new ApiEndpointHandlerValidationTests();

tests.Validate_ApiResponse_Valid_ReturnsEmptyList();
tests.Validate_BatchIngestResult_Invalid_ReturnsErrors();
tests.EnsureValid_PipelineStatusInfo_ThrowsWhenInvalid();
```

### BatchProcessorTests

The `BatchProcessorTests` class is an xUnit test fixture that verifies generic and data-point batch processing, including batching boundaries, parallel execution, progress callbacks, empty input, and failure handling. It also checks the available `BatchProcessingException` constructors; xUnit discovers these public test methods automatically, though they can be called directly when debugging.

Example usage:
```csharp
using DotNetRealtimePipeline.Tests;

var tests = new BatchProcessorTests();

await tests.ProcessAsync_AllItemsSucceed_ReturnsAllResults();
tests.CreateBatches_ItemsNotDivisibleByBatchSize_ReturnsPartialFinalBatch();
await tests.DataPointBatchProcessor_ProcessBatchAsync_AllItemsSucceed_ReturnsAllResults();
tests.BatchProcessingException_ConstructsWithMessageAndInnerException();
```

### SlidingWindowAggregatorTests

The `SlidingWindowAggregatorTests` class is an xUnit test fixture that verifies sliding-window aggregation, eviction, empty and single-value windows, out-of-order timestamps, multiple-window emission, trend calculations, and window metadata. xUnit discovers its public test methods automatically, though they can also be invoked directly when debugging the aggregator's behavior.

Example usage:
```csharp
using DotNetRealtimePipeline.Tests;

var tests = new SlidingWindowAggregatorTests();

tests.ValuesWithinWindowAggregateCorrectly();
tests.ValuesOlderThanWindowAreEvictedFromAggregate();
tests.EmptyWindowResult();
tests.SingleValueInWindow();
tests.OutOfOrderTimestampHandling();
tests.MultipleWindowsEmitted();
tests.AggregationCalculationsAreCorrect();
tests.TrendCalculationIsCorrect();
tests.WindowMetadataIsCorrect();
```

### ApiEndpointHandlerTests

The `ApiEndpointHandlerTests` class is an xUnit test fixture that verifies single-point and batch ingestion responses for successful, rejected, invalid, and exceptional requests. It also checks the public response and batch-result properties; xUnit discovers these methods automatically, while direct invocation is useful when debugging a specific case.

Example usage:
```csharp
using DotNetRealtimePipeline.Tests;

var tests = new ApiEndpointHandlerTests();

await tests.IngestAsync_WithValidDataPoint_ReturnsSuccessResponse();
await tests.IngestBatchAsync_WithValidBatch_ReturnsSuccessResponseWithBatchResults();
tests.ApiResponse_StatusCodeProperty_ReturnsCorrectValue();
tests.BatchIngestResult_TotalCountProperty_ReturnsCorrectValue();
```
