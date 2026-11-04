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
