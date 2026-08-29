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
