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

## DataPointTests
The `DataPointTests` class provides unit tests for the `DataPoint` model, covering validation logic, quality threshold checks, cloning behavior, and metadata management. It validates proper error handling for invalid inputs and ensures correct behavior across various data point scenarios.

Example usage:
```csharp
// Create a valid data point with all required properties
var dataPoint = new DataPoint(1, 1_000_000L, 42.5, "sensor-01")
{
    Quality = 85,
    Tags = "env:production"
};

// Validate the data point (should return true for valid data)
bool isValid = dataPoint.Validate();
Console.WriteLine($"Validation result: {isValid}");

// Test validation with zero ID (should return false)
var invalidIdPoint = new DataPoint(0, 1_000_000L, 42.5, "sensor-01");
bool isValidWithZeroId = invalidIdPoint.Validate();
Console.WriteLine($"Validation with zero ID: {isValidWithZeroId}");

// Check if quality meets threshold
bool meetsThreshold = dataPoint.MeetsQualityThreshold(75);
Console.WriteLine($"Meets quality threshold 75: {meetsThreshold}");

// Clone with a new ID
var clonedPoint = dataPoint.Clone(newId: 999);
Console.WriteLine($"Original ID: {dataPoint.Id}, Cloned ID: {clonedPoint.Id}");

// Add metadata
clonedPoint.AddMetadata("region", "us-east-1");
clonedPoint.AddMetadata("environment", "production");
Console.WriteLine($"Metadata count: {clonedPoint.Metadata.Count}");
```
This example demonstrates validation, quality threshold checking, cloning with new identifiers, and metadata management using the `DataPointTests` class.

## MetricAggregationTests

The `MetricAggregationTests` class provides unit tests for the `MetricAggregation` class, validating various metric calculation methods including throughput, error rate, success rate, backpressure ratio, and average processing time calculations.

Example usage:
```csharp
// Create a metric aggregation for testing
var metric = new MetricAggregation(1, 0, 5_000, "STANDARD")
{
    TotalItemsProcessed = 500,
    TotalItemsFailed = 50,
    TotalItemsSkipped = 10,
    TotalBackpressureMs = 1_000
};

// Calculate throughput: 500 items over 5 seconds = 100 items/second
double throughput = metric.CalculateThroughput();
Console.WriteLine($"Throughput: {throughput} items/second");

// Calculate error rate: 50 failed out of 500 total = 10%
double errorRate = metric.CalculateErrorRate();
Console.WriteLine($"Error rate: {errorRate}%");

// Calculate success rate: 450 successful out of 500 total = 90%
double successRate = metric.CalculateSuccessRate();
Console.WriteLine($"Success rate: {successRate}%");

// Check if unhealthy: error rate (10%) exceeds 5% threshold
bool isUnhealthy = metric.IsUnhealthy();
Console.WriteLine($"Is unhealthy: {isUnhealthy}");

// Calculate backpressure ratio: 1_000ms backpressure out of 5_000ms window = 20%
double backpressureRatio = metric.CalculateBackpressureRatio();
Console.WriteLine($"Backpressure ratio: {backpressureRatio}%");

// Compute average processing time from samples
var samples = new List<double> { 15.5, 22.3, 18.7, 20.1 };
metric.ComputeAverageProcessingTime(samples);
Console.WriteLine($"Average processing time: {metric.AverageProcessingTimeMs}ms");
```
This example demonstrates creating a metric aggregation, calculating throughput, error rate, success rate, checking health status, calculating backpressure ratio, and computing average processing time using the `MetricAggregationTests` class.

## DeadLetterQueueTests

The `DeadLetterQueueTests` class provides unit tests for the `DeadLetterQueue` class, validating dead letter queue operations including enqueueing, peeking, retrying, acknowledging, and capacity management. Tests verify proper error handling for invalid inputs and ensure correct behavior across various dead letter queue scenarios.

Example usage:
```csharp
// Create a dead letter queue with capacity of 100 and default max retries of 3
var deadLetterQueue = new DeadLetterQueue(maxCapacity: 100, defaultMaxRetries: 3);

// Enqueue a data point that failed processing
var dataPoint = new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 42.5, "sensor-01");
await deadLetterQueue.EnqueueAsync(dataPoint, "Ingestion", "Validation failed");

// Peek at pending entries without removing them
var pendingEntries = await deadLetterQueue.PeekAsync(10);
Console.WriteLine($"Pending entries: {pendingEntries.Count}");

// Dequeue entries for retry (marks them as InRetry status)
var retryBatch = await deadLetterQueue.DequeueForRetryAsync(5);
foreach (var entry in retryBatch)
{
    Console.WriteLine($"Retrying entry {entry.EntryId} from stage {entry.Stage}");
    
    // Simulate retry attempt
    try
    {
        // Attempt to reprocess...
        entry.RetryFailed("Still failing after retry"); // Mark as permanent failure if exhausted
    }
    catch
    {
        entry.RetryFailed("Retry attempt failed");
    }
}

// Acknowledge successful processing (removes entry from queue)
var entries = await deadLetterQueue.PeekAsync(1);
await deadLetterQueue.AcknowledgeSuccessAsync(entries[0].EntryId);

// Acknowledge permanent failure (marks entry as PermanentFailure)
await deadLetterQueue.AcknowledgeFailureAsync(entries[0].EntryId, "Non-retryable error");

// Get queue statistics
var stats = await deadLetterQueue.GetStatsAsync();
Console.WriteLine($"Total: {stats.TotalEntries}, Pending: {stats.PendingEntries}, Resolved: {stats.TotalResolved}");

// Check if an entry can be retried
var entry = new DeadLetterEntry { MaxRetries = 3, RetryCount = 0, Status = DeadLetterStatus.Pending };
bool canRetry = entry.CanRetry; // true when under budget
```
This example demonstrates creating a dead letter queue, enqueueing failed data points, peeking at entries, dequeuing for retry, acknowledging success/failure, retrieving statistics, and checking retry eligibility.



## PipelineIntegrationTests
The `PipelineIntegrationTests` class provides integration tests for the real-time data pipeline, validating end-to-end scenarios including pipeline lifecycle management, data ingestion from multiple sources, health monitoring, and metrics collection. It tests concurrent data ingestion, proper cleanup during pipeline shutdown, and query capabilities for filtered data retrieval.

Example usage:
```csharp
// Initialize pipeline with configuration
var pipelineConfig = new PipelineConfiguration
{
SourceType = DataSourceType.Kafka,
BatchSize = 1000,
MaxConcurrentStreams = 4
};

// Start and stop pipeline lifecycle
var pipeline = new PipelineIntegrationTests(pipelineConfig);
await pipeline.StartStop_ShouldInitializeAndCleanup();

// Ingest single data point
var dataPoint = new DataPoint
{
Timestamp = DateTime.UtcNow,
Value = 42.5,
Source = "sensor-001"
};
await pipeline.IngestDataPoint(dataPoint);

// Ingest batch of data points
var batch = new List<DataPoint>
{
new DataPoint { Timestamp = DateTime.UtcNow, Value = 10.1, Source = "sensor-001" },
new DataPoint { Timestamp = DateTime.UtcNow, Value = 20.2, Source = "sensor-002" },
new DataPoint { Timestamp = DateTime.UtcNow, Value = 30.3, Source = "sensor-003" }
};
await pipeline.IngestBatch(batch);

// Get health metrics
var healthReport = await pipeline.GetHealthReport();
Console.WriteLine($"CPU Usage: {healthReport.CpuUsage}%");

// Query filtered data points
var queryResults = await pipeline.QueryDataPoints(
startTime: DateTime.UtcNow.AddMinutes(-5),
endTime: DateTime.UtcNow,
sourceFilter: "sensor-001"
);

// Multiple source ingestion handling
var multiSourcePipeline = new PipelineIntegrationTests(new PipelineConfiguration
{
SourceType = DataSourceType.Multiple,
MaxConcurrentStreams = 8
});
await multiSourcePipeline.MultipleSourceIngestion_ShouldHandleConcurrentData();

// Get metrics history
var metricsHistory = await pipeline.GetMetricsHistory("ingestion-throughput", 100);
foreach (var metric in metricsHistory)
{
Console.WriteLine($"{metric.Timestamp}: {metric.Value}");
}
```
This example demonstrates pipeline lifecycle management, data ingestion from various sources, health monitoring, metrics collection, and concurrent data handling using the `PipelineIntegrationTests` class.

## IOutputFormatter

The `IOutputFormatter` interface defines a contract for formatting data into various output formats such as JSON, CSV, table, and HTML. It provides both synchronous and asynchronous methods for flexibility in different scenarios. Implementations include `JsonOutputFormatter`, `CsvOutputFormatter`, `TableOutputFormatter`, and `HtmlOutputFormatter`.

Example usage:
```csharp
// Create a data point to format
var dataPoint = new DataPoint(1, 1_000_000L, 42.5, "sensor-01")
{
    Quality = 85,
    Tags = "env:production"
};

// Use the JSON formatter
var jsonFormatter = new JsonOutputFormatter();
string jsonOutput = jsonFormatter.Format(dataPoint);
Console.WriteLine(jsonOutput);

// Use the CSV formatter for a list of data points
var dataPoints = new List<DataPoint> { dataPoint };
var csvFormatter = new CsvOutputFormatter();
string csvOutput = csvFormatter.Format(dataPoints);
Console.WriteLine(csvOutput);

// Use the Table formatter
var tableFormatter = new TableOutputFormatter();
string tableOutput = tableFormatter.Format(dataPoints);
Console.WriteLine(tableOutput);

// Use the HTML formatter
var htmlFormatter = new HtmlOutputFormatter();
string htmlOutput = htmlFormatter.Format(dataPoints);
Console.WriteLine(htmlOutput);

// Use the factory to create formatters based on format type
var jsonFormatter2 = OutputFormatterFactory.Create(OutputFormat.Json);
string formatted = await jsonFormatter2.FormatAsync(dataPoint);
```
This example demonstrates creating and using different output formatters to format data in various formats.