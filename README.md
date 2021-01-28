## PathHelper
The `PathHelper` class provides cross-platform file path validation, normalization, and directory operations. It includes utilities for path combination, disk space checks, file size formatting, and directory monitoring via `FileSystemMonitor`.

Example usage:
```csharp
// Validate and normalize paths
bool isValid = PathHelper.IsValidPath("/invalid/path/");
string normalized = PathHelper.Normalize("C:\\\\Windows\\\\..\\\\Users");
string combined = PathHelper.CombinePaths("data", "logs", "2023-10");

// Check file relationships
bool isInDir = PathHelper.IsPathInDirectory("/home/user/data/file.txt", "/home/user/data");

// Sanitize and generate filenames
string safeName = PathHelper.SanitizeFilename("report<>.txt");
string uniquePath = PathHelper.GenerateUniqueFilename("report.txt", "/output");

// Disk space and file operations
long freeSpace = PathHelper.GetAvailableDiskSpace("/");
long totalSpace = PathHelper.GetTotalDiskSpace("/");
PathHelper.EnsureDirectory("/temp/backups");

// Temporary files and formatting
string tempFile = PathHelper.GetTemporaryFilePath(".log");
long dirSize = PathHelper.GetDirectorySize("/var/logs");
string formattedSize = PathHelper.FormatFileSize(dirSize);

// File system monitoring
using var monitor = new PathHelper.FileSystemMonitor("/watched-folder");
monitor.Changed += (sender, e) => Console.WriteLine($"File changed: {e.FullPath}");
monitor.Created += (sender, e) => Console.WriteLine($"File created: {e.FullPath}");
monitor.Start();
// ... monitor for changes ...
monitor.Stop();
```
This example demonstrates path validation, directory checks, filename sanitization, disk space queries, and file system monitoring using `PathHelper` and `FileSystemMonitor`.

## RetryHelper
The `RetryHelper` class implements retry logic with exponential backoff, jitter, and customizable retry conditions. It supports both synchronous and asynchronous operations, allowing developers to define retry policies with specific delays, maximum attempts, and exception types to retry on.

Example usage:
```csharp
// Configure retry policy with exponential backoff and jitter
var policy = new RetryPolicyBuilder()
    .WithMaxAttempts(5)
    .WithInitialDelay(500)
    .WithMaxDelay(30000)
    .WithJitter(true)
    .RetryOn<HttpRequestException>()
    .RetryOn<TimeoutException>()
    .Build();

// Execute async operation with retry policy
try
{
    var result = await policy.ExecuteAsync<string>(async () =>
    {
        // Simulate a flaky API call
        if (new Random().Next(0, 3) == 0)
            throw new HttpRequestException("Simulated network failure");
        
        return await FetchDataFromServiceAsync();
    });
    
    Console.WriteLine($"Success: {result}");
}
catch (Exception ex) when (ex is OperationCanceledException)
{
    Console.WriteLine($"Operation failed after retries: {ex.Message}");
}

// Track retry statistics
var stats = new RetryStatistics();
stats.RecordAttempt(true, 500);  // Record successful attempt
stats.RecordAttempt(false, 1000); // Record failed attempt
Console.WriteLine($"Success rate: {stats.SuccessRate:F2}%");
```
This example demonstrates configuring a retry policy with exponential backoff, jitter, and specific retryable exceptions, then using it to execute an asynchronous operation while tracking retry statistics.

## PerformanceHelper
The `PerformanceHelper` class provides utilities for measuring execution time of synchronous and asynchronous operations, running benchmarks, and retrieving memory usage statistics. It exposes the `BenchmarkResult` and `MemoryStats` types for inspecting collected data.

Example usage:
```csharp
using DotNetRealtimePipeline.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

// Measure a synchronous operation
var syncResult = PerformanceHelper.MeasureExecution(() =>
{
    // Simulate work
    Thread.Sleep(150);
    return 42;
});
Console.WriteLine($"Sync result: {syncResult.Result}, elapsed: {syncResult.ElapsedMs}ms");

// Measure an asynchronous operation
var asyncResult = await PerformanceHelper.MeasureExecutionAsync(async () =>
{
    await Task.Delay(200);
    return "done";
});
Console.WriteLine($"Async result: {asyncResult.Result}, elapsed: {asyncResult.ElapsedMs}ms");

// Run a benchmark
var benchmark = PerformanceHelper.Benchmark(() =>
{
    // Operation to benchmark
    Math.Sqrt(12345);
}, iterations: 500);
Console.WriteLine(benchmark);

// Get memory statistics
var memStats = PerformanceHelper.GetMemoryStats();
Console.WriteLine(memStats);
```
The example demonstrates how to use `MeasureExecution`, `MeasureExecutionAsync`, `Benchmark`, and `GetMemoryStats`, and prints the resulting `BenchmarkResult` and `MemoryStats` objects.

## MetricsServiceTests
The `MetricsServiceTests` class provides unit tests for the `MetricsService` functionality, covering processing time recording, failure tracking, health reporting, and performance trend analysis. It validates proper error handling for invalid inputs and ensures correct behavior across various metric aggregation scenarios.

Example usage:
```csharp
// Create a mock repository for testing
var mockRepository = new Mock<IMetricsRepository>();

// Test recording processing time with negative value (should throw)
Assert.Throws<ArgumentException>(() => 
    new MetricsServiceTests(mockRepository.Object).RecordProcessingTime("-pipeline", -100));

// Test recording processing time with zero value (should not throw)
new MetricsServiceTests(mockRepository.Object).RecordProcessingTime("-pipeline", 0);

// Test health report generation when repository throws
mockRepository.Setup(r => r.GetHealthMetricsAsync())
    .ThrowsAsync(new InvalidOperationException("Database unavailable"));
var service = new MetricsServiceTests(mockRepository.Object);
var healthStatus = await service.GenerateHealthReportAsync();
Assert.Equal(HealthStatus.Unknown, healthStatus);

// Test health report generation with healthy metrics
mockRepository.Setup(r => r.GetHealthMetricsAsync())
    .ReturnsAsync(new HealthMetrics
    {
        CpuUsage = 45.2,
        MemoryUsage = 68.7,
        DiskUsage = 72.3
    });
var healthyStatus = await service.GenerateHealthReportAsync();
Assert.Equal(HealthStatus.Healthy, healthyStatus);

// Test performance trend analysis with insufficient data
var trendResult = await service.AnalyzePerformanceTrendAsync("response-time", 
    new List<MetricSample> { new MetricSample(DateTime.UtcNow, 150) });
Assert.Equal(TrendAnalysisResult.InsufficientData, trendResult);

// Test failure recording with null stage name (should throw)
Assert.Throws<ArgumentException>(() => 
    new MetricsServiceTests(mockRepository.Object).RecordFailure(null, "timeout"));
```
This example demonstrates testing various scenarios including error handling for invalid inputs, health status generation, and performance trend analysis with the `MetricsServiceTests` class.