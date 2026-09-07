## CommandLineParserTests

The CommandLineParserTests class contains unit tests for the CommandLineParser class. These tests verify that the parser correctly handles various command-line scenarios, including parsing empty arguments, unknown commands, and commands with required and optional options.

Example usage:
```csharp
CommandLineParser parser = new CommandLineParser();
parser.Parse("--help");
```

## MetricsServiceTestsExtensions

The MetricsServiceTestsExtensions class provides extension methods for testing the MetricsService class. It includes methods for getting the service instance, setting up a mock repository, generating test data points, and verifying the results.

Example usage:
```csharp
public static MetricsService GetService
public static Mock<IMetricsRepository> GetMockRepository
public static void VerifyMetricsService
public static IReadOnlyList<DataPoint> GenerateTestDataPoints
public static void ConfigureMockRepository
public static void VerifyTestResult
public static void VerifyTestResult
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

## DataProcessingServiceTestsExtensions

The DataProcessingServiceTestsExtensions class provides static methods for creating test data and pipeline configurations for unit testing the DataProcessingService class.

Example usage:
```csharp
public static DataPoint CreateValidDataPoint
public static DataPoint CreateLowQualityDataPoint
public static DataPoint CreateInvalidDataPoint
public static PipelineConfig CreateTestPipelineConfig
public static ProcessingResult CreateSuccessfulResult
public static ProcessingResult CreateFailedResult
```

## PipelineEventPublisherJsonExtensionsTests

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

## PipelineEventPublisherValidationTests

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

## ApiEndpointHandlerExtensionsTests

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

## RateLimitingMiddlewareConcurrencyTests

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

## ApiEndpointHandlerValidationTests

The ApiEndpointHandlerValidationTests class is an xUnit test fixture that verifies validation behavior for API responses, batch ingestion results, and pipeline status information. It covers valid and invalid models as well as the exceptions raised for invalid or null values; xUnit normally discovers these public test methods automatically, but they can also be invoked directly when debugging a specific scenario.

Example usage:
```csharp
using DotNetRealtimePipeline.Tests;

var tests = new ApiEndpointHandlerValidationTests();

tests.Validate_ApiResponse_Valid_ReturnsEmptyList();
tests.Validate_BatchIngestResult_Invalid_ReturnsErrors();
tests.EnsureValid_PipelineStatusInfo_ThrowsWhenInvalid();
```

## BatchProcessorTests

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

## SlidingWindowAggregatorTests

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

## ApiEndpointHandlerTests

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
