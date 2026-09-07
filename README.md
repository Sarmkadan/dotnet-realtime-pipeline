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
