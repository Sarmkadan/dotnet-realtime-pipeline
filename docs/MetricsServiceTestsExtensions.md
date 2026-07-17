# MetricsServiceTestsExtensions

Static class that provides a collection of helper methods for unit‑testing the `MetricsService` type. These methods encapsulate common arrange‑act‑assert patterns, mock configuration, and data generation to keep test code concise and readable.

## API

### GetService
- **Purpose:** Creates and returns a fully configured `MetricsService` instance suitable for testing.
- **Parameters:** None.
- **Return:** `MetricsService`
- **Throws:** `InvalidOperationException` if the required dependencies cannot be resolved.

### GetMockRepository
- **Purpose:** Returns a new `Mock<IMetricsRepository>` that can be further configured or inspected.
- **Parameters:** None.
- **Return:** `Mock<IMetricsRepository>`
- **Throws:** None.

### VerifyMetricsService
- **Purpose:** Executes a user‑supplied verification action against a `MetricsService` instance.
- **Parameters:** 
  - `MetricsService service` – the service to verify.
  - `Action<MetricsService> verification` – delegate containing the assertions to run.
- **Return:** `void`
- **Throws:** 
  - `ArgumentNullException` if `service` or `verification` is `null`.
  - Any exception thrown by the `verification` delegate is propagated.

### GenerateTestDataPoints
- **Purpose:** Produces a list of `DataPoint` objects that can be used as test input or expected output.
- **Parameters:** 
  - `int count` – number of data points to generate (must be non‑negative).
- **Return:** `IReadOnlyList<DataPoint>`
- **Throws:** `ArgumentOutOfRangeException` if `count` is less than zero.

### ConfigureMockRepository
- **Purpose:** Applies a standard setup to a mock repository, typically arranging it to return a supplied sequence of data points.
- **Parameters:** 
  - `Mock<IMetricsRepository> mock` – the mock to configure.
  - `IEnumerable<DataPoint> data` – the data the mock should return.
- **Return:** `void`
- **Throws:** `ArgumentNullException` if `mock` or `data` is `null`.

### VerifyTestResult
- **Purpose:** Asserts that an actual Boolean result matches an expected value, optionally providing a custom message.
- **Parameters:** 
  - `bool actual` – the result produced by the code under test.
  - `bool expected` – the anticipated result.
  - `string? message` – optional message to include in the failure.
- **Return:** `void`
- **Throws:** `AssertFailedException` when `actual` does not equal `expected`.

### VerifyRepositoryCall
- **Purpose:** Verifies that a specific method on the mocked repository was invoked the expected number of times.
- **Parameters:** 
  - `Mock<IMetricsRepository> mock` – the mock to inspect.
  - `string methodName` – the name of the repository method to verify.
  - `Times times` – the expected call count (e.g., `Times.Once()`).
- **Return:** `void`
- **Throws:** `MockException` if the actual invocation count does not match `times`.

### SetupRepositoryToReturn
- **Purpose:** Configures a mock repository method to return a value produced by a factory delegate when invoked.
- **Parameters:** 
  - `Mock<IMetricsRepository> mock` – the mock to set up.
  - `string methodName` – the name of the method to configure.
  - `Func<object> factory` – delegate that returns the value to be yielded by the mock.
  - `Times times` – how many times the setup should apply (e.g., `Times.Any()`).
- **Return:** `void`
- **Throws:** `ArgumentNullException` if `mock`, `methodName`, or `factory` is `null`.

## Usage

### Example 1: Basic service test with mock verification
```csharp
using Moq;
using Xunit;

public class MetricsServiceTests
{
    [Fact]
    public void ProcessData_CallsRepository_ReturnsExpectedResult()
    {
        // Arrange
        var mockRepo = MetricsServiceTestsExtensions.GetMockRepository();
        var service  = MetricsServiceTestsExtensions.GetService(); // uses mockRepo internally

        var testPoints = MetricsServiceTestsExtensions.GenerateTestDataPoints(3);
        MetricsServiceTestsExtensions.ConfigureMockRepository(mockRepo, testPoints);

        // Act
        var result = service.ProcessData();

        // Assert
        Assert.True(result);
        MetricsServiceTestsExtensions.VerifyRepositoryCall(
            mockRepo,
            nameof(IMetricsRepository.SaveAsync),
            Times.Once);
    }
}
```

### Example 2: Using generated data and custom verification
```csharp
using Moq;
using Xunit;

public class MetricsServiceTestsExtensionsDemo
{
    [Fact]
    public void CalculateMetrics_WithKnownData_ReturnsCorrectSum()
    {
        // Arrange
        var mock = MetricsServiceTestsExtensions.GetMockRepository();
        var service = MetricsServiceTestsExtensions.GetService(); // assumes constructor injects mock

        var dataPoints = MetricsServiceTestsExtensions.GenerateTestDataPoints(5);
        MetricsServiceTestsExtensions.SetupRepositoryToReturn(
            mock,
            nameof(IMetricsRepository.GetLatestAsync),
            () => dataPoints,
            Times.Once);

        // Act
        var sum = service.CalculateMetrics();

        // Assert
        var expectedSum = dataPoints.Sum(p => p.Value);
        MetricsServiceTestsExtensions.VerifyTestResult(
            sum == expectedSum,
            true,
            $"Expected sum {expectedSum} but got {sum}");
    }
}
```

## Notes
- All extension methods are **static** and contain no mutable state; they are safe to call concurrently from multiple threads.
- The mock objects returned by `GetMockRepository` are **not** thread‑safe; sharing a single `Mock<IMetricsRepository>` instance across parallel test threads without proper synchronization may lead to undefined behavior.
- Passing `null` for any reference‑type parameter will result in an `ArgumentNullException` (where applicable) as documented.
- `GenerateTestDataPoints` creates deterministic `DataPoint` instances; if unique values are required per call, adjust the factory supplied to `SetupRepositoryToReturn` accordingly.
- These helpers are intended solely for test scenarios; they should not be referenced in production code.
