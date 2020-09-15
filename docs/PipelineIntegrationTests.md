# PipelineIntegrationTests

The `PipelineIntegrationTests` class serves as the primary integration test suite for the `dotnet-realtime-pipeline` project, validating the end-to-end functionality of the data ingestion, processing, and retrieval subsystems. It verifies that the pipeline correctly initializes and cleans up resources, handles single and batched data points, manages concurrent ingestion from multiple sources, and accurately reports health metrics and historical data. These tests are designed to run against a fully instantiated environment to ensure system stability and data integrity under realistic operational conditions.

## API

### `StartStop_ShouldInitializeAndCleanup`
Validates the lifecycle management of the pipeline by ensuring that all components initialize correctly upon startup and release all resources without leakage during shutdown.
- **Parameters**: None.
- **Return Value**: Returns a `Task` that completes when the verification is finished.
- **Throws**: Throws an assertion exception if initialization fails, if resources remain allocated after cleanup, or if the shutdown sequence times out.

### `IngestDataPoint_ShouldAcceptAndProcess`
Verifies that the pipeline accepts a single data point, processes it through the configured stages, and persists or forwards it as expected.
- **Parameters**: None (test data is generated internally).
- **Return Value**: Returns a `Task` that completes when the ingestion and subsequent processing verification are done.
- **Throws**: Throws an assertion exception if the data point is rejected, processing fails, or the final state does not match the expected outcome.

### `IngestBatch_ShouldProcessMultiplePoints`
Ensures the pipeline correctly handles a batch of data points, maintaining order (if required) and processing efficiency without dropping items.
- **Parameters**: None (batch data is generated internally).
- **Return Value**: Returns a `Task` that completes when the batch processing verification is finished.
- **Throws**: Throws an assertion exception if any item in the batch is lost, processed incorrectly, or if the batch operation exceeds allowed latency thresholds.

### `GetHealthReport_ShouldReturnMetrics`
Confirms that the health reporting endpoint returns a valid structure containing current system metrics, such as throughput, error rates, and active connection counts.
- **Parameters**: None.
- **Return Value**: Returns a `Task` that completes when the health report validation is finished.
- **Throws**: Throws an assertion exception if the health report is null, missing critical metric fields, or contains invalid data types.

### `QueryDataPoints_ShouldReturnFilteredResults`
Tests the data retrieval mechanism by issuing queries with specific filters and verifying that only matching data points are returned.
- **Parameters**: None (filter criteria and test data are generated internally).
- **Return Value**: Returns a `Task` that completes when the query result validation is finished.
- **Throws**: Throws an assertion exception if the returned dataset includes non-matching items, excludes matching items, or if the query operation fails.

### `MultipleSourceIngestion_ShouldHandleConcurrentData`
Validates the pipeline's concurrency model by simulating simultaneous data ingestion from multiple distinct sources to ensure thread safety and data consistency.
- **Parameters**: None (concurrent sources are simulated internally).
- **Return Value**: Returns a `Task` that completes when the concurrent ingestion verification is finished.
- **Throws**: Throws an assertion exception if data corruption occurs, race conditions cause deadlocks, or the total count of ingested items does not match the sum of inputs.

### `GetMetricsHistory_ShouldReturnCollectedMetrics`
Verifies the historical metrics storage and retrieval logic, ensuring that metrics collected over a time window are accurately stored and can be queried.
- **Parameters**: None.
- **Return Value**: Returns a `Task` that completes when the historical metrics validation is finished.
- **Throws**: Throws an assertion exception if the history is empty when data should exist, if timestamps are incorrect, or if metric values do not align with recorded events.

## Usage

### Example 1: Executing the Test Suite via Test Runner
The following example demonstrates how to invoke the integration tests using the .NET test CLI, targeting the specific class to validate the pipeline deployment.

```csharp
// Command line execution context
// dotnet test --filter "FullyQualifiedName~PipelineIntegrationTests" --logger "console;verbosity=detailed"

// Programmatic invocation context within a test runner harness
var testAssembly = Assembly.LoadFrom("dotnet-realtime-pipeline.Tests.dll");
var testType = testAssembly.GetType("dotnet-realtime-pipeline.Tests.PipelineIntegrationTests");
var testInstance = Activator.CreateInstance(testType);

// Invoke a specific test method asynchronously
var methodInfo = testType.GetMethod(nameof(PipelineIntegrationTests.IngestBatch_ShouldProcessMultiplePoints));
await (Task)methodInfo.Invoke(testInstance, null);
```

### Example 2: Validating Pipeline Health in a CI/CD Script
This example illustrates a scenario where the health check test is executed as part of a post-deployment verification step to ensure the pipeline is operational before routing live traffic.

```csharp
public async Task<bool> VerifyPipelineHealthAsync()
{
    var tests = new PipelineIntegrationTests();
    
    try 
    {
        // Execute the health report validation
        await tests.GetHealthReport_ShouldReturnMetrics();
        
        // Execute the startup/shutdown lifecycle check
        await tests.StartStop_ShouldInitializeAndCleanup();
        
        return true; // All integration checks passed
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Pipeline integration verification failed: {ex.Message}");
        return false; // Indicate deployment failure
    }
}
```

## Notes

- **Execution Environment**: These tests require a fully configured runtime environment, including access to the underlying message broker, database, and metric storage systems. They will fail if executed in an isolated unit test environment without these dependencies mocked or provisioned.
- **Concurrency and Thread Safety**: The `MultipleSourceIngestion_ShouldHandleConcurrentData` test explicitly validates thread safety. If this test fails intermittently, it indicates a race condition within the pipeline's ingestion layer rather than a test flaw. The test suite itself is designed to be thread-safe for parallel execution of different test methods, provided the underlying pipeline instance supports concurrent test interactions or is reset between calls.
- **Resource Cleanup**: The `StartStop_ShouldInitializeAndCleanup` test is critical for preventing resource exhaustion in continuous integration loops. Failure in this test often indicates unmanaged handles or lingering network connections that may cause subsequent tests to timeout.
- **Data Isolation**: Tests involving data ingestion and querying (`IngestDataPoint`, `QueryDataPoints`, etc.) assume logical data isolation per test run. If running in a shared environment, ensure unique identifiers are used to prevent cross-test contamination, although the implementation typically handles scoped data generation internally.
- **Timing Sensitivity**: As integration tests, methods like `GetMetricsHistory_ShouldReturnCollectedMetrics` may have implicit timing dependencies. If metrics are not immediately visible due to asynchronous aggregation windows, the test implementation includes retry logic; however, extreme system latency may still cause false negatives.
