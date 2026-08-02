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
public static void VerifyRepositoryCall
public static void VerifyTestResult
```