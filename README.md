# dotnet-realtime-pipeline

An in-process, in-memory real-time data pipeline library for .NET with backpressure
management, tumbling/sliding windowing, metrics collection, and a console demo entry
point (`Program.cs`).

## Quick Start

```bash
dotnet build dotnet-realtime-pipeline.csproj
dotnet run --project dotnet-realtime-pipeline.csproj
```

The demo configures the pipeline via `AddPipelineServices(...)`, ingests 500 sample
sensor points, and prints processing stats and a health report.

## Architecture

See [docs/architecture.md](docs/architecture.md) for the component breakdown, data flow,
concurrency model, extension points, and known limitations.

## Class Reference

The sections below are generated per-class API notes; more per-class docs live in
[docs/](docs/).

## BatchProcessorExtensions

The `BatchProcessorExtensions` class provides extension methods for `BatchProcessor<TInput, TOutput>` and `DataPointBatchProcessor` that simplify batch processing operations. It includes methods for processing collections in batches, creating batches from collections, batch transformations, parallel processing with aggregation, estimating processing time, and analyzing batch statistics.

Example usage:

```csharp
using DotNetRealtimePipeline.Utilities;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Create a batch processor with default settings (batch size: 1000, parallelism: 4)
var processor = new BatchProcessor<string, string>(batchSize: 1000, maxDegreeOfParallelism: 4);

// Example 1: Process a collection in batches
var items = Enumerable.Range(1, 5000).Select(i => $"Item {i}").ToList();

var results = await processor.ProcessAsync(
    items,
    async batch => {
        // Process each batch (simulating async work)
        await Task.Delay(10);
        return batch.Select(item => $"Processed: {item}").ToList();
    },
    progress => Console.WriteLine($"Progress: {progress.Current}/{progress.Total}")
);

Console.WriteLine($"Processed {results.Count} batches");

// Example 2: Batch transformation with selector
var transformed = processor.BatchSelect(
    items,
    batch => batch.Count
);

Console.WriteLine($"Batch sizes: {string.Join(", ", transformed)}");

// Example 3: Parallel processing with aggregation
var totalLength = await processor.ProcessAsync(
    items,
    async batch => {
        await Task.Delay(5);
        return batch.Select(item => item.Length).ToList();
    },
    seed: 0,
    (aggregate, result) => aggregate + result,
    progress => Console.WriteLine($"Aggregating: {progress.Current}/{progress.Total}")
);

Console.WriteLine($"Total length of all processed items: {totalLength}");

// Example 4: Estimate processing time
var estimatedTime = processor.GetEstimatedProcessingTime<string, string>(
    itemCount: 10000,
    estimatedBatchProcessingTimeMs: 50.0
);

Console.WriteLine($"Estimated processing time: {estimatedTime.TotalSeconds:F2}s");

// Example 5: Get batch statistics
var stats = processor.GetBatchStatistics<string, string>(items);
Console.WriteLine(stats);
Console.WriteLine($"Total items: {stats.TotalItems}, Batches: {stats.TotalBatches}, Batch size: {stats.BatchSize}");

// Example 6: Process DataPoints in batches
var dataPointProcessor = new DataPointBatchProcessor(batchSize: 500);
var dataPoints = new List<DataPoint>();
for (int i = 0; i < 2500; i++)
{
    dataPoints.Add(new DataPoint {
        Id = i,
        Source = $"Sensor{i % 10}",
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        Value = i * 0.5,
        Quality = 95
    });
}

var processingResults = await dataPointProcessor.ProcessBatchAsync(
    dataPoints,
    async batch => {
        await Task.Delay(20);
        return batch.Select(dp => new ProcessingResult(
            stageName: "DataProcessing",
            success: true,
            outputData: new Dictionary<string, object> { { "count", batch.Count } },
            processingTimeMs: 25
        )).ToList();
    }
);

Console.WriteLine($"Processed {processingResults.Count} batches of DataPoints");
```

## CommandExecutorExtensions

The `CommandExecutorExtensions` class provides convenient extension methods for `CommandExecutor`, simplifying common data operations and pipeline management scenarios. It includes methods for executing commands with success checking, ingesting data from files, querying data points, getting pipeline status, counting data points, exporting data, and generating status summaries.

## HealthCheckServiceValidation

The `HealthCheckServiceValidation` static class provides validation helpers for `HealthCheckService` and related health check types (`ComponentHealth`, `SystemHealthReport`, `QuickHealthStatus`). It includes extension methods for validating health check instances, checking validity status, and throwing exceptions when invalid states are detected. This ensures health check configurations are properly validated before use in pipeline monitoring.

Example usage:

```csharp
using DotNetRealtimePipeline.Monitoring;
using System;
using System.Collections.Generic;

// Create a HealthCheckService instance (typically registered via DI)
var healthCheckService = new HealthCheckService();

// Validate the health check service instance
var validationErrors = healthCheckService.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("HealthCheckService validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if health check service is valid using IsValid extension method
bool isValid = healthCheckService.IsValid();
Console.WriteLine($"HealthCheckService is valid: {isValid}");

// Ensure validity (throws ArgumentException if invalid)
healthCheckService.EnsureValid();

// Create a ComponentHealth instance for validation
var componentHealth = new ComponentHealth
{
    Message = "All systems operational",
    CheckedAt = DateTime.UtcNow,
    Details = new Dictionary<string, object>
    {
        ["Status"] = "Healthy",
        ["Components"] = 15,
        ["BackpressureActive"] = false
    }
};

// Validate component health
var componentErrors = componentHealth.Validate();
if (componentErrors.Count > 0)
{
    Console.WriteLine("ComponentHealth validation failed:");
    foreach (var error in componentErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check component health validity
bool componentIsValid = componentHealth.IsValid();
Console.WriteLine($"ComponentHealth is valid: {componentIsValid}");

// Ensure component health is valid
componentHealth.EnsureValid();

// Create a SystemHealthReport instance for validation
var healthReport = new SystemHealthReport
{
    CheckedAt = DateTime.UtcNow,
    OverallStatus = SystemHealth.Healthy,
    Components = new List<ComponentHealth>
    {
        new ComponentHealth
        {
            Message = "Pipeline stage operational",
            CheckedAt = DateTime.UtcNow,
            Details = new Dictionary<string, object> { ["Stage"] = "DataProcessing" }
        }
    },
    PipelineStatus = "Running",
    Throughput = 12500,
    SuccessRate = 99.8
};

// Validate system health report
var reportErrors = healthReport.Validate();
if (reportErrors.Count > 0)
{
    Console.WriteLine("SystemHealthReport validation failed:");
    foreach (var error in reportErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check system health report validity
bool reportIsValid = healthReport.IsValid();
Console.WriteLine($"SystemHealthReport is valid: {reportIsValid}");

// Ensure system health report is valid
healthReport.EnsureValid();
```

## SerializationHelperValidation

The `SerializationHelperValidation` static class provides validation helpers for `SerializationHelper` operations and related types (`DataPoint`, `ProcessingResult`, `MetricAggregation`). It includes extension methods for validating serialization-related objects before serialization, checking validity status, and throwing exceptions when invalid states are detected. This ensures data integrity when working with pipeline serialization operations.

Example usage:

```csharp
using DotNetRealtimePipeline.Utilities;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Create a valid DataPoint instance for serialization
var dataPoint = new DataPoint
{
    Id = 1,
    Source = "TemperatureSensor",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    Value = 23.5,
    Quality = 95,
    CreatedAt = DateTime.UtcNow
};

// Validate the DataPoint before serialization
var validationErrors = dataPoint.ValidateDataPoint();
if (validationErrors.Count > 0)
{
    Console.WriteLine("DataPoint validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if DataPoint is valid using IsValid extension method
bool isValid = dataPoint.IsValid();
Console.WriteLine($"DataPoint is valid: {isValid}");

// Ensure validity (throws ArgumentException if invalid)
dataPoint.EnsureValid();

// Create a ProcessingResult instance for validation
var processingResult = new ProcessingResult(
    stageName: "DataProcessing",
    success: true,
    outputData: new Dictionary<string, object> { { "processedItems", 100 } },
    processingTimeMs: 125
);

// Validate ProcessingResult before serialization
var resultErrors = processingResult.ValidateProcessingResult();
if (resultErrors.Count > 0)
{
    Console.WriteLine("ProcessingResult validation failed:");
    foreach (var error in resultErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if ProcessingResult is valid
bool resultIsValid = processingResult.IsValid();
Console.WriteLine($"ProcessingResult is valid: {resultIsValid}");

// Ensure ProcessingResult validity
processingResult.EnsureValid();

// Create a MetricAggregation instance for validation
var metrics = new MetricAggregation
{
    MetricId = 1,
    MetricType = "PipelinePerformance",
    TimeWindowStartMs = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds(),
    TimeWindowEndMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    TotalItemsProcessed = 10000,
    TotalItemsFailed = 250,
    TotalItemsSkipped = 120,
    AverageProcessingTimeMs = 45.2,
    MinProcessingTimeMs = 5,
    MaxProcessingTimeMs = 250,
    P95ProcessingTimeMs = 120,
    P99ProcessingTimeMs = 180,
    BackpressureEvents = 8,
    TotalBackpressureMs = 1500,
    ComputedAt = DateTime.UtcNow
};

// Validate MetricAggregation before serialization
var metricsErrors = metrics.ValidateMetricAggregation();
if (metricsErrors.Count > 0)
{
    Console.WriteLine("MetricAggregation validation failed:");
    foreach (var error in metricsErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if MetricAggregation is valid
bool metricsIsValid = metrics.IsValid();
Console.WriteLine($"MetricAggregation is valid: {metricsIsValid}");

// Ensure MetricAggregation validity
metrics.EnsureValid();

// Validate a list of DataPoints
var dataPoints = new List<DataPoint>
{
    new DataPoint { Id = 1, Source = "Sensor1", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Value = 23.5, Quality = 95 },
    new DataPoint { Id = 2, Source = "Sensor2", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Value = 24.1, Quality = 92 }
};

var listErrors = dataPoints.ValidateDataPoints();
if (listErrors.Count > 0)
{
    Console.WriteLine("DataPoint list validation failed:");
    foreach (var error in listErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

## RetryHelperValidation

The `RetryHelperValidation` static class provides validation helpers for retry-related types (`RetryHelper`, `RetryPolicyBuilder`, `RetryPolicy`, `RetryStatistics`, `RetryEvent`). It includes extension methods for validating retry configurations and statistics, checking validity status, and throwing exceptions when invalid states are detected. This ensures retry policies are properly configured before use in pipeline operations.

Example usage:

```csharp
using DotNetRealtimePipeline.Utilities;
using Polly;
using System;
using System.Threading.Tasks;

// Create a retry policy builder with valid configuration
var retryPolicyBuilder = Policy
  .Handle<Exception>()
  .WaitAndRetryAsync(
    retryCount: 3,
    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
    onRetry: (exception, delay, retryCount, context) =>
    {
      Console.WriteLine($"Retry {retryCount} of 3. Waiting {delay.TotalSeconds}s. Error: {exception.Message}");
    }
  );

// Validate the retry policy builder
var builderValidationErrors = retryPolicyBuilder.Validate();
if (builderValidationErrors.Count > 0)
{
  Console.WriteLine("RetryPolicyBuilder validation failed:");
  foreach (var error in builderValidationErrors)
  {
    Console.WriteLine($"- {error}");
  }
}

// Check if retry policy builder is valid using IsValid extension method
bool builderIsValid = retryPolicyBuilder.IsValid();
Console.WriteLine($"RetryPolicyBuilder is valid: {builderIsValid}");

// Ensure builder validity (throws ArgumentException if invalid)
retryPolicyBuilder.EnsureValid();

// Create a RetryPolicy instance for validation
var retryPolicy = new RetryPolicy
{
  MaxAttempts = 5,
  InitialDelayMs = 100,
  MaxDelayMs = 10000,
  RetryableExceptions = new List<Type> { typeof(Exception), typeof(TimeoutException) },
  CreatedAt = DateTime.UtcNow
};

// Validate the retry policy
var policyValidationErrors = retryPolicy.Validate();
if (policyValidationErrors.Count > 0)
{
  Console.WriteLine("RetryPolicy validation failed:");
  foreach (var error in policyValidationErrors)
  {
    Console.WriteLine($"- {error}");
  }
}

// Check if retry policy is valid
bool policyIsValid = retryPolicy.IsValid();
Console.WriteLine($"RetryPolicy is valid: {policyIsValid}");

// Ensure policy validity
retryPolicy.EnsureValid();

// Create retry statistics for validation
var retryStatistics = new RetryStatistics
{
  TotalAttempts = 15,
  SuccessfulAttempts = 12,
  FailedAttempts = 3,
  AverageRetryDelayMs = 1500,
  LastRetryAt = DateTime.UtcNow.AddMinutes(-2)
};

// Validate retry statistics
var statsValidationErrors = retryStatistics.Validate();
if (statsValidationErrors.Count > 0)
{
  Console.WriteLine("RetryStatistics validation failed:");
  foreach (var error in statsValidationErrors)
  {
    Console.WriteLine($"- {error}");
  }
}

// Check if statistics are valid
bool statsIsValid = retryStatistics.IsValid();
Console.WriteLine($"RetryStatistics is valid: {statsIsValid}");

// Ensure statistics validity
retryStatistics.EnsureValid();

// Create a retry event for validation
var retryEvent = new RetryEvent
{
  Timestamp = DateTime.UtcNow,
  DelayMs = 2000,
  AttemptNumber = 2,
  ExceptionType = typeof(TimeoutException)
};

// Validate retry event
var eventValidationErrors = retryEvent.Validate();
if (eventValidationErrors.Count > 0)
{
  Console.WriteLine("RetryEvent validation failed:");
  foreach (var error in eventValidationErrors)
  {
    Console.WriteLine($"- {error}");
  }
}

// Check if event is valid
bool eventIsValid = retryEvent.IsValid();
Console.WriteLine($"RetryEvent is valid: {eventIsValid}");

// Ensure event validity
retryEvent.EnsureValid();
```

## PipelineInitializerExtensions

The `PipelineInitializerExtensions` class provides extension methods for `PipelineInitializer` that enhance pipeline lifecycle management with additional functionality. It includes methods for initializing and starting pipelines in a single operation, retrying initialization on transient failures, safely stopping pipelines, and checking pipeline initialization state.

Example usage:

```csharp
using DotNetRealtimePipeline.Initialization;
using System;
using System.Threading.Tasks;

// Assume initializer is an initialized instance of PipelineInitializer
var initializer = new PipelineInitializer();

// Initialize and start pipeline in one operation
var initAndStartResult = await initializer.InitializeAndStartAsync();
Console.WriteLine($"Initialized and started: {initAndStartResult.Success}");
if (!initAndStartResult.Success)
{
    Console.WriteLine($"Error: {initAndStartResult.ErrorMessage}");
}

// Initialize with automatic retry for transient failures
var retryResult = await initializer.InitializeWithRetryAsync(maxAttempts: 5, delayBetweenAttempts: 2000);
Console.WriteLine($"Retry initialization successful: {retryResult.Success}");

// Check if pipeline is initialized
bool isInitialized = initializer.IsInitialized();
Console.WriteLine($"Pipeline initialized: {isInitialized}");

// Get current pipeline state
string pipelineState = initializer.GetPipelineState();
Console.WriteLine($"Pipeline state: {pipelineState}");

// Safely stop the pipeline (swallows exceptions)
bool stoppedSuccessfully = await initializer.SafeStopAsync();
Console.WriteLine($"Pipeline stopped successfully: {stoppedSuccessfully}");
```

## PerformanceHelperExtensions

The `PerformanceHelperExtensions` class provides extension methods for performance benchmarking, memory analysis, and performance comparison operations. It includes methods for benchmarking synchronous and asynchronous operations, calculating statistical measures like standard deviation and percentiles, analyzing memory usage and GC pressure, and comparing performance between different runs.

Example usage:

```csharp
using DotNetRealtimePipeline.Utilities;
using System;
using System.Threading.Tasks;

// Create a performance helper instance
var performanceHelper = new PerformanceHelper();

// Benchmark a synchronous operation (1000 iterations by default)
var benchmarkResult = performanceHelper.Benchmark(() => {
    // Your operation to benchmark here
    for (int i = 0; i < 1000; i++) {
        _ = i * i;
    }
});

Console.WriteLine(benchmarkResult.ToDetailedString());
Console.WriteLine($"Standard deviation: {benchmarkResult.GetStandardDeviation():F2}ms");

// Benchmark an asynchronous operation
var asyncBenchmarkResult = await performanceHelper.BenchmarkAsync(async () => {
    // Your async operation to benchmark
    await Task.Delay(10);
});

Console.WriteLine(asyncBenchmarkResult.ToCompactString());

// Analyze memory usage
var memoryStats = performanceHelper.GetMemoryStats();
Console.WriteLine(memoryStats.ToDetailedString());
Console.WriteLine($"GC Pressure Score: {memoryStats.GetGcPressureScore():F2}");
Console.WriteLine($"Has memory pressure: {memoryStats.HasMemoryPressure()}");

// Compare performance between two benchmark runs
var baselineResult = performanceHelper.Benchmark(() => {
    for (int i = 0; i < 100; i++) {
        _ = i * i;
    }
});

var improvedResult = performanceHelper.Benchmark(() => {
    for (int i = 0; i < 100; i++) {
        _ = Math.Sqrt(i);
    }
});

double improvement = improvedResult.ComparePerformance(baselineResult);
Console.WriteLine($"Performance improvement: {improvement:+0.00;-0.00;0.00}%");

// Get raw measurements for custom analysis
IReadOnlyList<long> measurements = benchmarkResult.GetMeasurements();
Console.WriteLine($"Raw measurements count: {measurements.Count}");
```

## PerformanceHelperValidation

The `PerformanceHelperValidation` static class provides validation helpers for `BenchmarkResult` and `MemoryStats` instances, as well as execution results from `PerformanceHelper.MeasureExecution` and `PerformanceHelper.MeasureExecutionAsync` methods. It includes extension methods for comprehensive validation, checking validity status, and throwing exceptions when invalid states are detected. This ensures performance measurements meet business rules and data integrity constraints before use in pipeline operations.

Example usage:

```csharp
using DotNetRealtimePipeline.Utilities;
using System;
using System.Threading.Tasks;

// Create benchmark results for validation
var benchmarkResult = new BenchmarkResult
{
    Iterations = 1000,
    Measurements = new long[] { 15, 18, 16, 17, 19 },
    AverageMs = 17.0,
    MinMs = 15,
    MaxMs = 19,
    MedianMs = 17.0,
    P95Ms = 19.0,
    P99Ms = 19.0
};

// Validate benchmark result
var validationErrors = benchmarkResult.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Benchmark validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if benchmark is valid
bool isValid = benchmarkResult.IsValid();
Console.WriteLine($"Benchmark is valid: {isValid}");

// Ensure validity (throws ArgumentException if invalid)
benchmarkResult.EnsureValid();

// Create memory statistics for validation
var memoryStats = new MemoryStats
{
    WorkingSetMb = 125.5,
    PrivateMemoryMb = 85.2,
    PeakWorkingSetMb = 150.0,
    GC0Collections = 2,
    GC1Collections = 0,
    GC2Collections = 0,
    TotalMemoryMb = 210.7
};

// Validate memory statistics
var statsErrors = memoryStats.Validate();
if (statsErrors.Count > 0)
{
    Console.WriteLine("Memory stats validation failed:");
    foreach (var error in statsErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if memory stats are valid
bool statsValid = memoryStats.IsValid();
Console.WriteLine($"Memory stats are valid: {statsValid}");

// Ensure memory stats validity
memoryStats.EnsureValid();

// Validate execution results from PerformanceHelper.MeasureExecution
var executionResult = new PerformanceHelper().MeasureExecution(() => {
    // Operation to measure
    return 42;
});

// Validate execution result
var executionErrors = executionResult.Validate();
if (executionErrors.Count > 0)
{
    Console.WriteLine("Execution result validation failed:");
    foreach (var error in executionErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check execution result validity
bool executionValid = executionResult.IsValid();
Console.WriteLine($"Execution result is valid: {executionValid}");

// Ensure execution result validity
(executionResult.Result, executionResult.ElapsedMs).EnsureValid();

// Validate async execution results from PerformanceHelper.MeasureExecutionAsync
var asyncExecutionResult = await new PerformanceHelper().MeasureExecutionAsync(async () => {
    await Task.Delay(10);
    return "success";
});

// Validate async execution result
var asyncErrors = asyncExecutionResult.Validate("expected-result");
if (asyncErrors.Count > 0)
{
    Console.WriteLine("Async execution result validation failed:");
    foreach (var error in asyncErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check async execution result validity
bool asyncValid = asyncExecutionResult.IsValid("expected-result");
Console.WriteLine($"Async execution result is valid: {asyncValid}");

// Ensure async execution result validity
asyncExecutionResult.EnsureValid("expected-result");
```

## CommandLineParserExtensions

The `CommandLineParserExtensions` class provides extension methods for `CommandLineParser` to simplify command registration and parsing scenarios. It includes methods for registering individual commands, bulk registering commands from dictionaries or sequences, parsing command line arguments into structured commands, and executing parsed commands with proper error handling.

Example usage:

```csharp
using DotNetRealtimePipeline.CLI;
using System;
using System.Collections.Generic;

// Assume parser is an initialized instance of CommandLineParser
var parser = new CommandLineParser();

// Register a single command with a factory method
parser.RegisterCommand("ingest", () => new ParsedCommand("ingest", new Dictionary<string, string> 
{
    {"file", "data.json"}
}));

// Register multiple commands from a dictionary
var commands = new Dictionary<string, Func<ParsedCommand>>
{
    ["ingest"] = () => new ParsedCommand("ingest", new Dictionary<string, string> { { "file", "data.json" } }),
    ["query"] = () => new ParsedCommand("query", new Dictionary<string, string> { { "source", "sensors" } }),
    ["status"] = () => new ParsedCommand("status")
};
parser.RegisterCommands(commands);

// Register multiple commands from a sequence of tuples
var commandRegistrations = new List<(string verb, Func<ParsedCommand> factory)>
{
    ("ingest", () => new ParsedCommand("ingest", new Dictionary<string, string> { { "file", "data.json" } })),
    ("query", () => new ParsedCommand("query", new Dictionary<string, string> { { "source", "sensors" } })),
    ("status", () => new ParsedCommand("status"))
};
parser.RegisterCommands(commandRegistrations);

// Parse command line arguments into a structured command
var parsedCommand = parser.ParseCommand(new[] { "ingest", "--file", "sensor_data.json" });
Console.WriteLine($"Parsed command: {parsedCommand.CommandName}");

// Attempt to parse and execute command with error handling
int exitCode = parser.TryParseAndExecute(new[] { "ingest", "--file", "data.json" });
Console.WriteLine($"Command executed with exit code: {exitCode}");
```

Example usage:

```csharp
using DotNetRealtimePipeline.CLI;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// Assume executor is an initialized instance of CommandExecutor
var executor = new CommandExecutor();

// Execute a command and check for success
var command = new ParsedCommand("ingest", new Dictionary<string, string> { { "file", "data.json" } });
bool success = await executor.ExecuteSuccessfullyAsync(command);
Console.WriteLine($"Command executed successfully: {success}");

// Ingest data points from a JSON file and get the count
int ingestedCount = await executor.IngestFromFileAsync("sensor_data.json", "json");
Console.WriteLine($"Ingested {ingestedCount} data points");

// Query data points within a time range
var dataPoints = await executor.QueryDataAsync(
    startMs: DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds(),
    endMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    source: "temperature-sensor",
    minQuality: 80
);
Console.WriteLine($"Found {dataPoints.Count} data points");

// Get pipeline status as a dictionary
var status = await executor.GetStatusAsync();
Console.WriteLine($"Pipeline health: {status.GetValueOrDefault("health_status")}");

// Count data points in a file without ingesting
int fileCount = await executor.CountDataPointsAsync("sensor_data.json", "json");
Console.WriteLine($"File contains {fileCount} data points");

// Export data to a CSV file and get the count
int exportedCount = await executor.ExportToFileAsync(
    startMs: DateTimeOffset.UtcNow.AddHours(-24).ToUnixTimeMilliseconds(),
    endMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    outputPath: "exported_data.csv",
    format: "csv"
);
Console.WriteLine($"Exported {exportedCount} data points to CSV");

// Count data points in an exported file
int exportedFileCount = await executor.CountExportedDataPointsAsync("exported_data.csv", "csv");
Console.WriteLine($"Exported file contains {exportedFileCount} data points");

// Get a formatted status summary
string statusSummary = await executor.GetStatusSummaryAsync();
Console.WriteLine(statusSummary);
```

## PipelineHttpClientFactoryExtensions
The `PipelineHttpClientFactoryExtensions` class provides convenient extension methods for `PipelineHttpClientFactory`, allowing for simplified HTTP client creation, configuration, and data exchange. It includes methods for setting up clients with base addresses, applying custom timeout/retry policies, and executing asynchronous GET and POST requests.

Example usage:
```csharp
using DotNetRealtimePipeline.Integration;
using System.Net.Http;
using System.Text;

// Assume factory is an initialized instance of PipelineHttpClientFactory
var factory = new PipelineHttpClientFactory();

// Create a client with a specific base address
var client = factory.CreateClientWithBaseAddress("https://api.example.com");

// Create a configured service client with retry and compression
var serviceClient = factory.CreateConfiguredServiceClient(
    serviceName: "MyService",
    timeout: TimeSpan.FromSeconds(30),
    maxRetries: 5,
    useCompression: true
);

// Execute a GET request
string result = await factory.GetStringAsync("https://api.example.com/data");

// Execute a POST request with JSON
var content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json");
string postResult = await factory.PostJsonAsync("https://api.example.com/ingest", content);
```

## BackpressureContextJsonExtensions

The `BackpressureContextJsonExtensions` class provides System.Text.Json serialization extensions for `BackpressureContext`, enabling easy serialization to JSON strings and deserialization from JSON strings. This is particularly useful for persisting backpressure context state or transmitting it across process boundaries.

Example usage:

```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Create a backpressure context with sample data
var context = new BackpressureContext(
    pipelineStageName: "DataProcessing",
    maxBufferCapacity: 10000,
    maxConcurrentConsumers: 4
);

// Add some buffer data
context.BufferSize = 8500;
context.ItemsInBuffer = new Queue<long>(new[] { 1000L, 2000L, 3000L, 4000L, 5000L });
context.BufferFillPercent = 85.0;
context.BufferedItemsBySource = new Dictionary<string, long> {
    ["sensor-ingest"] = 5000,
    ["api-ingest"] = 3500
};
context.LastBackpressureActivated = DateTime.UtcNow.AddMinutes(-2);

// Serialize to JSON string (compact format)
string jsonCompact = context.ToJson();
Console.WriteLine(jsonCompact);

// Serialize to JSON string (indented for readability)
string jsonIndented = context.ToJson(indented: true);
Console.WriteLine(jsonIndented);

// Deserialize from JSON string
string json = @"
{
    "pipelineStageName": "DataProcessing",
    "maxBufferCapacity": 10000,
    "maxConcurrentConsumers": 4,
    "bufferSize": 8500,
    "itemsInBuffer": [1000,2000,3000,4000,5000],
    "bufferFillPercent": 85.0,
    "bufferedItemsBySource": {
        "sensor-ingest": 5000,
        "api-ingest": 3500
    },
    "lastBackpressureActivated": "2024-07-19T12:30:00.000Z"
}";

BackpressureContext? deserializedContext = BackpressureContextJsonExtensions.FromJson(json);
Console.WriteLine($"Deserialized stage: {deserializedContext?.PipelineStageName}");

// Try to deserialize with error handling
if (BackpressureContextJsonExtensions.TryFromJson(json, out var tryDeserializedContext))
{
    Console.WriteLine("Successfully deserialized context");
}
else
{
    Console.WriteLine("Failed to deserialize context");
}

// Handle null/empty JSON gracefully
BackpressureContext? nullContext = BackpressureContextJsonExtensions.FromJson(null);
Console.WriteLine($"Null JSON result: {nullContext}"); // Output: Null JSON result:
```

## BackpressureMetricsCollectorTests
The `BackpressureMetricsCollectorTests` class provides unit tests for the `BackpressureMetricsCollector` class, verifying its ability to track and report backpressure metrics across pipeline stages. It includes tests for handling unknown stages, recording manual events, aggregating metrics, and resetting collected data.

Example usage:
```csharp
using DotNetRealtimePipeline.Tests.Unit;

var tests = new BackpressureMetricsCollectorTests();

// Test unknown stage behavior
tests.GetStageMetrics_UnknownStage_ReturnsNull();

// Test activation event recording
tests.RecordManualEvent_Activation_IncrementsActivationCount();
tests.RecordManualEvent_TwoActivations_CountIsTwo();

// Test snapshot aggregation
tests.GetSnapshot_WithNoEvents_ReturnsEmptySnapshot();
tests.GetSnapshot_AggregatesAcrossStages();

// Test event retrieval
tests.GetRecentEvents_ReturnsUpToRequestedCount();
tests.GetStageEvents_ReturnsOnlyEventsForThatStage();

// Test reset functionality
tests.Reset_ClearsAllMetricsAndEvents();

// Test integration with backpressure activation
tests.Poll_AfterBackpressureActivated_RecordsActivationEvent();
```

## BackpressureServiceTests
The `BackpressureServiceTests` class provides unit tests for the `BackpressureService` class, verifying its ability to manage backpressure across pipeline stages. It includes tests for creating contexts, adding to buffers, applying backpressure, and removing from buffers. 

Example usage:
```csharp
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Enums;

var service = new BackpressureService();

// Create a context
service.CreateContext("TestStage", 1000);

// Add to buffer
var result = service.TryAddToBuffer("TestStage", 500);

// Apply backpressure with block strategy
var response = service.ApplyBackpressureAsync(
    "TestStage",
    BackpressureStrategy.Block,
    timeoutMs: 1000
).Result;

// Remove from buffer
service.RemoveFromBuffer("TestStage", 200);
```

## PipelineVisualizerTests
The `PipelineVisualizerTests` class provides unit tests for the `PipelineVisualizer` class, verifying its ability to visualize pipeline stages and their relationships. It includes tests for building nodes, rendering pipeline visualizations, and computing health labels for pipeline nodes. 

Example usage:
```csharp
var visualizer = new PipelineVisualizerTests();
visualizer.BuildNodes_WithValidConfig_ReturnsOneNodePerStage();
visualizer.BuildNodes_EdgesAreLinkedSequentially();
visualizer.Render_ContainsPipelineName();
visualizer.Render_ContainsAllStageNames();
visualizer.RenderCompact_ContainsSeparators();
visualizer.PipelineVisualizationNode_ComputeHealthLabel_BackpressuredIsCritical();
visualizer.PipelineVisualizationNode_ComputeHealthLabel_HighBufferIsWarning();
visualizer.PipelineVisualizationNode_ComputeHealthLabel_NormalIsHealthy();
```

## PipelineVisualizationNodeExtensions
The `PipelineVisualizationNodeExtensions` class provides convenient extension methods for `PipelineVisualizationNode` to simplify common visualization and analysis operations. It includes methods for checking node health states, formatting metrics, and retrieving downstream stage information.

Example usage:
```csharp
using DotNetRealtimePipeline.Visualization;
using System;

// Assume node is a PipelineVisualizationNode from a pipeline visualization
var node = new PipelineVisualizationNode(
    stageName: "DataProcessing",
    stageType: "Processor",
    healthLabel: "HEALTHY",
    throughputEps: 12500,
    bufferFillPercent: 45.2,
    droppedItems: 23,
    isBackpressured: false
);

// Check health states
bool isHealthy = node.IsHealthy();
bool isWarning = node.IsWarning();
bool isCritical = node.IsCritical();

// Get downstream stage information
IReadOnlyList<string> downstream = node.GetDownstreamStages();
bool hasDownstream = node.HasDownstream();

// Format metrics for display
string throughput = node.FormatThroughput(); // "12.50K eps"
string bufferFill = node.FormatBufferFill(); // "45.2%"
string healthColor = node.GetHealthColor(); // "#008000" for HEALTHY

// Get a comprehensive status summary for tooltips
string statusSummary = node.GetStatusSummary();
Console.WriteLine(statusSummary);
// Output: "DataProcessing (Processor) | HEALTHY | Buffer: 45.2% | Throughput: 12.50K eps | Dropped: 23 | Backpressure: INACTIVE"
```

## BackpressureMetricsCollectorValidation
The `BackpressureMetricsCollectorValidation` static class provides validation helpers for backpressure metrics types (`StageBackpressureMetrics` and `BackpressureMetricsSnapshot`). It includes methods for comprehensive validation, validity checks, and throwing exceptions on invalid states.

Example usage:

```csharp
using DotNetRealtimePipeline.Metrics;
using System;
using System.Collections.Generic;

// Create stage backpressure metrics with valid values
var stageMetrics = new StageBackpressureMetrics
{
    StageName = "DataProcessing",
    ActivationCount = 5,
    TotalActiveDurationMs = 1250,
    PeakBufferFillPercent = 85.5,
    CurrentBufferFillPercent = 72.3,
    TotalDroppedItems = 12,
    LastActivationAt = DateTime.UtcNow.AddMinutes(-2)
};

// Validate and check if metrics are valid
var validationErrors = stageMetrics.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check validity using IsValid extension method
bool isValid = stageMetrics.IsValid();
Console.WriteLine($"Stage metrics are valid: {isValid}");

// Ensure validity (throws ArgumentException if invalid)
stageMetrics.EnsureValid();

// Create a metrics snapshot with multiple stage metrics
var snapshot = new BackpressureMetricsSnapshot
{
    StageMetrics = new List<StageBackpressureMetrics>
    {
        new StageBackpressureMetrics
        {
            StageName = "Ingestion",
            ActivationCount = 3,
            TotalActiveDurationMs = 800,
            PeakBufferFillPercent = 65.2,
            CurrentBufferFillPercent = 58.7,
            TotalDroppedItems = 0
        },
        new StageBackpressureMetrics
        {
            StageName = "Transformation",
            ActivationCount = 8,
            TotalActiveDurationMs = 2100,
            PeakBufferFillPercent = 92.1,
            CurrentBufferFillPercent = 88.4,
            TotalDroppedItems = 8,
            LastActivationAt = DateTime.UtcNow.AddMinutes(-1)
        }
    },
    TotalActivations = 16,
    TotalDroppedItems = 20,
    ActiveBackpressureStages = 2,
    SnapshotAt = DateTime.UtcNow
};

// Validate snapshot
var snapshotErrors = snapshot.Validate();
if (snapshotErrors.Count > 0)
{
    Console.WriteLine("Snapshot validation failed:");
    foreach (var error in snapshotErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check snapshot validity
bool snapshotIsValid = snapshot.IsValid();
Console.WriteLine($"Snapshot is valid: {snapshotIsValid}");

// Ensure snapshot validity
snapshot.EnsureValid();
```

## BackpressureEventExtensions
The `BackpressureEventExtensions` class provides extension methods for `BackpressureEvent` that simplify backpressure event analysis and formatting. It includes methods for checking critical buffer states, determining severity levels, formatting events as readable strings, and identifying activation/release events.

Example usage:

```csharp
using DotNetRealtimePipeline.Metrics;
using System;

// Assume event is a BackpressureEvent instance from pipeline monitoring
var backpressureEvent = new BackpressureEvent
{
    Timestamp = DateTimeOffset.UtcNow,
    StageName = "DataProcessing",
    BufferFillPercent = 85.5,
    IsActivation = true,
    DroppedItems = 12
};

// Check if this event represents a critical situation
bool isCritical = backpressureEvent.IsCritical(thresholdPercent: 80.0);
Console.WriteLine($"Is critical: {isCritical}"); // Output: Is critical: True

// Determine severity level based on buffer fill percentage
var severity = backpressureEvent.GetSeverityLevel();
Console.WriteLine($"Severity level: {severity}"); // Output: Severity level: High

// Format the event as a human-readable string
string formatted = backpressureEvent.ToFormattedString();
Console.WriteLine(formatted);
// Output: BackpressureEvent [Timestamp=2024-07-19T14:30:00.0000000Z, Stage=DataProcessing, BufferFill=85.50%, IsActivation=True, DroppedItems=12]

// Check if this is a new activation event
bool isNewActivation = backpressureEvent.IsNewActivation();
Console.WriteLine($"Is new activation: {isNewActivation}"); // Output: Is new activation: True

// Check if this is a release event (backpressure ended)
bool isRelease = backpressureEvent.IsRelease();
Console.WriteLine($"Is release: {isRelease}"); // Output: Is release: False
```

## ExportServiceExtensions

The `ExportServiceExtensions` class provides extension methods for `ExportService` that enhance data export operations with additional functionality. It includes methods for validating output directories, exporting data with automatic retry logic, streaming exports, file size estimation, and metadata-enhanced exports.

Example usage:

```csharp
using DotNetRealtimePipeline.Data;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// Assume exportService is an initialized instance of ExportService
var exportService = new ExportService();

// Validate output directory and create if needed
bool directoryValid = exportService.ValidateOutputDirectory("exports/data_export.csv");
Console.WriteLine($"Directory valid: {directoryValid}");

// Prepare sample data points
var dataPoints = new List<DataPoint>
{
    new DataPoint { Id = 1, Source = "TemperatureSensor", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Value = 23.5, Quality = 95 },
    new DataPoint { Id = 2, Source = "TemperatureSensor", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Value = 24.1, Quality = 92 },
    new DataPoint { Id = 3, Source = "HumiditySensor", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Value = 45.2, Quality = 88 }
};

// Export with automatic retry (3 attempts by default)
var exportResult = await exportService.ExportWithRetryAsync(
    dataPoints,
    "exports/sensor_data.csv",
    OutputFormat.Csv,
    maxRetries: 3
);
Console.WriteLine($"Export successful: {exportResult.Success}, Records: {exportResult.RecordCount}");

// Estimate file size before export
string estimatedSize = await exportService.EstimateFileSizeAsync(dataPoints, OutputFormat.Csv);
Console.WriteLine($"Estimated file size: {estimatedSize}");

// Export to stream (e.g., for HTTP response)
using var memoryStream = new MemoryStream();
var streamResult = await exportService.ExportToStreamAsync(
    dataPoints,
    memoryStream,
    OutputFormat.Json
);
Console.WriteLine($"Stream export successful: {streamResult.Success}");

// Export with metadata in filename (includes timestamp and record count)
var metadataResult = await exportService.ExportWithMetadataAsync(
    dataPoints,
    "exports/data_export",
    OutputFormat.Csv,
    timestamp: DateTime.UtcNow,
    includeRecordCount: true
);
Console.WriteLine($"Metadata export path: {metadataResult.OutputPath}");
```

## BackpressureContextExtensions
The `BackpressureContextExtensions` class provides extension methods for `BackpressureContext` to simplify common operations and add domain-specific functionality for pipeline backpressure management. It includes methods for estimating time to capacity, checking critical buffer states, formatting metrics, and managing backpressure events.

Example usage:
```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Assume context is a BackpressureContext instance from a pipeline stage
var context = new BackpressureContext(
    pipelineStageName: "DataProcessing",
    maxBufferCapacity: 10000,
    maxConcurrentConsumers: 4
);

// Estimate time until buffer reaches capacity based on consumption rate
long timeToCapacityMs = context.EstimateTimeToCapacity(consumptionRatePerSecond: 500);
Console.WriteLine($"Time to capacity: {timeToCapacityMs}ms");

// Check if buffer is critically full (90% threshold by default)
bool isCritical = context.IsCriticallyFull(percentageThreshold: 90);
Console.WriteLine($"Critical state: {isCritical}");

// Get formatted backpressure duration
string duration = context.GetBackpressureDurationFormatted();
Console.WriteLine($"Backpressure duration: {duration}");

// Record a backpressure event with metadata
var metadata = new Dictionary<string, string> {
    ["Source"] = "HighThroughputDetector",
    ["ItemsInBuffer"] = "9500",
    ["Threshold"] = "9000"
};
context.RecordBackpressureEvent("HighBuffer", metadata);

// Get comprehensive buffer metrics summary
string metricsSummary = context.GetBufferMetricsSummary();
Console.WriteLine(metricsSummary);

// Safely remove items from buffer
long removed = context.SafeRemoveFromBuffer(200);
Console.WriteLine($"Removed {removed} items from buffer");

// Check if sufficient capacity exists for a batch
bool hasCapacity = context.HasSufficientCapacityForBatch(batchSize: 1000, requiredCapacityPercent: 20);
Console.WriteLine($"Has capacity for batch: {hasCapacity}");
```

## ProcessingResultExtensions
The `ProcessingResultExtensions` class provides extension methods for `ProcessingResult` to enhance pipeline processing scenarios. It includes methods for checking failure retryability, merging output data from multiple results, converting results to dictionaries for serialization, updating processing times, and detecting timeout scenarios.

Example usage:
```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Assume result is a ProcessingResult from a pipeline stage
var result = new ProcessingResult(
    stageName: "DataProcessing",
    success: true,
    outputData: new Dictionary<string, object> { { "processedItems", 100 }, { "throughput", 5000 } }
);

// Check if a failure is retryable
bool isRetryable = result.IsRetryableFailure(maxRetryCount: 3);
Console.WriteLine($"Is retryable: {isRetryable}");

// Merge output data from another result
var sourceResult = new ProcessingResult(
    stageName: "Validation",
    success: true,
    outputData: new Dictionary<string, object> { { "validationPassed", true }, { "errorsFound", 0 } }
);
result.MergeOutputData(sourceResult, overwriteExisting: false);

// Convert result to dictionary for serialization
var resultDict = result.ToDictionary();
Console.WriteLine($"Result has {resultDict.Count} properties");

// Create a new result with updated processing time
var updatedResult = result.WithProcessingTime(processingTimeMs: 125);
Console.WriteLine($"Original processing time: {result.ProcessingTimeMs}ms");
Console.WriteLine($"Updated processing time: {updatedResult.ProcessingTimeMs}ms");

// Check if processing timed out
bool isTimeout = updatedResult.IsTimeout(timeoutThresholdMs: 100);
Console.WriteLine($"Timeout detected: {isTimeout}");
```

## StreamEventExtensions
The `StreamEventExtensions` class provides extension methods for `StreamEvent` to simplify common stream processing operations. It includes methods for filtering payloads, type-safe payload extraction, stage tracking, event copying, staleness detection, priority formatting, JSON serialization, failure detection, and progress calculation.

Example usage:

```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Create a sample stream event with payload
var streamEvent = new StreamEvent
{
    EventId = Guid.NewGuid().ToString(),
    DataPointId = 123,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    EventType = "SensorReading",
    Priority = 2, // High priority
    SourceSystem = "TemperatureMonitor",
    Payload = new Dictionary<string, object>
    {
        ["sensorId"] = "temp-001",
        ["temperature"] = 23.5,
        ["unit"] = "Celsius",
        ["location"] = "Room A101",
        ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    },
    ProcessedByStages = new List<string> { "validation", "normalization" }
};

// Filter payload to only include specific keys
var filteredPayload = streamEvent.FilterPayload(new[] { "sensorId", "temperature", "unit" });
Console.WriteLine($"Filtered payload count: {filteredPayload.Count}");

// Get payload value with type safety
var temperature = streamEvent.GetPayload<double>("temperature", defaultValue: 0.0);
Console.WriteLine($"Temperature: {temperature}°C");

// Check if event has been processed by specific stages
bool processedByValidation = streamEvent.HasBeenProcessedByAnyStage(new[] { "validation", "transformation" });
Console.WriteLine($"Processed by validation or transformation: {processedByValidation}");

// Get remaining stages count
int remainingStages = streamEvent.GetRemainingStagesCount(new[] { "validation", "normalization", "transformation", "aggregation" });
Console.WriteLine($"Remaining stages: {remainingStages}");

// Create a deep copy of the event
var eventCopy = streamEvent.DeepCopy();
Console.WriteLine($"Original and copy have same EventId: {streamEvent.EventId == eventCopy.EventId}");

// Check if event is stale (older than 5 minutes)
bool isStale = streamEvent.IsStale(maxAgeMs: 300000);
Console.WriteLine($"Is stale: {isStale}");

// Get priority as formatted string
string priorityString = streamEvent.GetPriorityString();
Console.WriteLine($"Priority: {priorityString}");

// Get payload value as JSON string
string temperatureJson = streamEvent.GetPayloadAsJson("temperature");
Console.WriteLine($"Temperature as JSON: {temperatureJson}");

// Check if event has failed
bool hasFailed = streamEvent.HasFailed();
Console.WriteLine($"Has failed: {hasFailed}");

// Calculate processing completion percentage
int completionPercentage = streamEvent.GetProcessingCompletionPercentage(totalStages: 4);
Console.WriteLine($"Completion: {completionPercentage}%");
```

## DataPointExtensions
The `DataPointExtensions` class provides convenient extension methods for `DataPoint` to enhance data processing capabilities. It includes methods for creating new data point instances with updated properties, working with metadata, formatting for logging, checking staleness, and managing IDs.

Example usage:
```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Assume dataPoint is a DataPoint instance
var dataPoint = new DataPoint
{
    Id = 1,
    Source = "TemperatureSensor",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    Value = 23.5,
    Quality = 95,
    Tags = "sensor,temperature",
    Metadata = new Dictionary<string, object>
    {
        ["Unit"] = "Celsius",
        ["Location"] = "Room A101"
    }
};

// Create a new data point with updated value
var updatedValue = dataPoint.WithValue(24.1);
Console.WriteLine($"Updated value: {updatedValue.Value}"); // Output: Updated value: 24.1

// Create a new data point with updated timestamp
var newTimestamp = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds();
var updatedTimestamp = dataPoint.WithTimestamp(newTimestamp);
Console.WriteLine($"Updated timestamp: {DateTimeOffset.FromUnixTimeMilliseconds(updatedTimestamp.Timestamp):O}");

// Create a new data point with updated quality
var highQuality = dataPoint.WithQuality(98);
Console.WriteLine($"Updated quality: {highQuality.Quality}%"); // Output: Updated quality: 98%

// Add additional tags to the data point
var tagged = dataPoint.WithTags("priority,critical");
Console.WriteLine($"Tags: {tagged.Tags}"); // Output: Tags: sensor,temperature,priority,critical

// Get all metadata values of a specific type
var stringMetadata = dataPoint.GetMetadataValues<string>();
Console.WriteLine($"Metadata count: {stringMetadata.Count}"); // Output: Metadata count: 2

// Try to get a specific metadata value
if (dataPoint.TryGetMetadataValue("Unit", out string unit))
{
    Console.WriteLine($"Unit: {unit}"); // Output: Unit: Celsius
}

// Format the data point for logging
string logEntry = dataPoint.ToLogString(includeMetadata: true);
Console.WriteLine(logEntry);
// Output: DataPoint[1] - Source: TemperatureSensor, Timestamp: 2024-07-19T14:30:00.0000000Z, Value: 23.5, Quality: 95% | Metadata[2]

// Check if data point is stale (older than 5 minutes)
bool isStale = dataPoint.IsStale(maxAgeMs: 300000);
Console.WriteLine($"Is stale: {isStale}");

// Create a shallow copy with a new ID
var copied = dataPoint.WithId(2);
Console.WriteLine($"Original ID: {dataPoint.Id}, New ID: {copied.Id}"); // Output: Original ID: 1, New ID: 2
```

## PipelineConfigExtensions
The `PipelineConfigExtensions` class provides convenient extension methods for `PipelineConfig` to simplify common operations on pipeline configurations and their stages. It includes methods for querying stage counts, checking stage existence, retrieving stage definitions, and filtering stages by various criteria.

Example usage:
```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Linq;

// Assume config is a configured PipelineConfig instance
var config = new PipelineConfig {
    Stages = new List<PipelineStageDef> {
        new PipelineStageDef { StageName = "ingestion", StageType = "source", Enabled = true },
        new PipelineStageDef { StageName = "validation", StageType = "filter", Enabled = true },
        new PipelineStageDef { StageName = "transformation", StageType = "transform", Enabled = false },
        new PipelineStageDef { StageName = "aggregation", StageType = "aggregate", Enabled = true }
    }
};

// Get the total number of stages
int totalStages = config.GetTotalStages();
Console.WriteLine($"Total stages: {totalStages}"); // Output: Total stages: 4

// Check if the configuration has any stages
bool hasStages = config.HasStages();
Console.WriteLine($"Has stages: {hasStages}"); // Output: Has stages: True

// Get all stage names
var stageNames = config.GetStageNames();
Console.WriteLine("Stage names: " + string.Join(", ", stageNames));
// Output: Stage names: ingestion, validation, transformation, aggregation

// Check if a specific stage exists
bool hasIngestion = config.HasStage("ingestion");
Console.WriteLine($"Has ingestion stage: {hasIngestion}"); // Output: Has ingestion stage: True

// Get a stage by name
var ingestionStage = config.GetStageByName("ingestion");
Console.WriteLine($"Ingestion stage type: {ingestionStage?.StageType}"); // Output: Ingestion stage type: source

// Find the first stage matching a predicate
var firstFilter = config.FindStage(s => s.StageType == "filter");
Console.WriteLine($"First filter stage: {firstFilter?.StageName}"); // Output: First filter stage: validation

// Find all stages matching a predicate
var allTransforms = config.FindStages(s => s.StageType == "transform");
Console.WriteLine($"Transform stages count: {allTransforms.Count()}"); // Output: Transform stages count: 1

// Get all enabled stages
var enabledStages = config.GetEnabledStages();
Console.WriteLine($"Enabled stages count: {enabledStages.Count()}"); // Output: Enabled stages count: 3

// Get all stages of a specific type
var aggregateStages = config.GetStagesByType("aggregate");
Console.WriteLine($"Aggregate stages count: {aggregateStages.Count()}"); // Output: Aggregate stages count: 1
```

## WindowEventValidation
The `WindowEventValidation` static class provides validation helpers for `WindowEvent` instances, ensuring window events meet business rules and data integrity constraints. It includes methods for comprehensive validation, duration checks, data point quality assessment, and timestamp validation.

Example usage:
```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Assume windowEvent is a WindowEvent instance
var windowEvent = new WindowEvent
{
    WindowId = 1,
    WindowStartMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    WindowEndMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 300000, // 5 minutes later
    AggregationType = "Average",
    CreatedAt = DateTime.UtcNow,
    CreatedAtTicks = DateTime.UtcNow.Ticks,
    DataPoints = new List<DataPoint>
    {
        new DataPoint { Id = "1", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Value = 42.5, Quality = 95 },
        new DataPoint { Id = "2", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1000, Value = 43.1, Quality = 92 }
    },
    IsComplete = true
};

// Validate window event
var validationErrors = windowEvent.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if valid
bool isValid = windowEvent.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Ensure validity (throws if invalid)
windowEvent.EnsureValid();

// Check duration validity
bool isDurationValid = windowEvent.IsDurationValid(maxDurationMs: 3600000); // 1 hour
Console.WriteLine($"Duration valid: {isDurationValid}");

// Check data point count
bool hasSufficientData = windowEvent.HasSufficientDataPoints(minDataPoints: 2);
Console.WriteLine($"Has sufficient data points: {hasSufficientData}");

// Check supported aggregation type
var supportedTypes = new List<string> { "Average", "Sum", "Count", "Min", "Max" };
bool hasSupportedAggregation = windowEvent.HasSupportedAggregationType(supportedTypes);
Console.WriteLine($"Has supported aggregation: {hasSupportedAggregation}");

// Check data point quality
bool hasQualityData = windowEvent.HasQualityDataPoints(qualityThreshold: 85);
Console.WriteLine($"Has quality data points: {hasQualityData}");

// Check if complete and valid
bool isCompleteAndValid = windowEvent.IsCompleteAndValid();
Console.WriteLine($"Is complete and valid: {isCompleteAndValid}");

// Check timestamp reasonableness
bool hasReasonableTimestamps = windowEvent.HasReasonableTimestamps(maxFutureMs: 7200000); // 2 hours
Console.WriteLine($"Has reasonable timestamps: {hasReasonableTimestamps}");
```

## PipelineBenchmarks
The `PipelineBenchmarks` class provides performance benchmarks for the dotnet-realtime-pipeline library. It measures throughput and memory allocation for critical pipeline operations.

To use `PipelineBenchmarks`, create an instance and call the `Setup` method to initialize the benchmark environment. Run individual benchmarks using their respective methods.

Example usage:
```csharp
using DotNetRealtimePipeline.Benchmarks;

var benchmarks = new PipelineBenchmarks();
benchmarks.Setup();

await benchmarks.IngestSingleDataPoint();
await benchmarks.ProcessBatch(100);
benchmarks.ProcessDataPointsThroughWindowing(1000);
await benchmarks.GenerateHealthReport();
benchmarks.BackpressureBufferOperations();
await benchmarks.EndToEndThroughput();
await benchmarks.MemoryAllocationBenchmark();

benchmarks.Cleanup();
```

## ExportServiceValidation

The `ExportServiceValidation` static class provides validation helpers for `ExportResult` and `BatchExportResult` instances. It includes methods for comprehensive validation of export results, checking validity status, and throwing exceptions when invalid states are detected. This ensures data integrity when working with pipeline export operations.

Example usage:

```csharp
using DotNetRealtimePipeline.Data;
using System;
using System.IO;

// Create a valid ExportResult instance
var exportResult = new ExportResult
{
    Success = true,
    OutputPath = Path.GetFullPath("exported_data.csv"),
    RecordCount = 1000,
    FileSizeBytes = 15000,
    ErrorMessage = null,
    StartTime = DateTime.UtcNow.AddMinutes(-5),
    EndTime = DateTime.UtcNow
};

// Validate the export result
var validationErrors = exportResult.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Export validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if valid using IsValid extension method
bool isValid = exportResult.IsValid();
Console.WriteLine($"Export result is valid: {isValid}");

// Ensure validity (throws ArgumentException if invalid)
exportResult.EnsureValid();

// Create a BatchExportResult instance
var batchResult = new BatchExportResult
{
    Success = true,
    ExportedRecords = 5000,
    BatchFiles = new List<string> { "export_part1.csv", "export_part2.csv" },
    ErrorMessage = null,
    StartTime = DateTime.UtcNow.AddMinutes(-10),
    EndTime = DateTime.UtcNow
};

// Validate batch export result
var batchErrors = batchResult.Validate();
if (batchErrors.Count > 0)
{
    Console.WriteLine("Batch export validation failed:");
    foreach (var error in batchErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check batch validity
bool batchIsValid = batchResult.IsValid();
Console.WriteLine($"Batch export result is valid: {batchIsValid}");

// Ensure batch validity
batchResult.EnsureValid();
```

## WindowingServiceExtensions

The `WindowingServiceExtensions` class provides extension methods for `WindowingService` that enhance windowing operations with additional functionality. It includes methods for creating custom duration windows, processing data points with state tracking, calculating combined window statistics, and accessing window information.

Example usage:

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using System;
using System.Collections.Generic;

// Assume service is an initialized instance of WindowingService
var service = new WindowingService(windowSizeMs: 300000); // 5-minute windows

// Create a custom duration window (e.g., for special processing or debugging)
var customWindow = service.CreateCustomDurationWindow(
    windowStartMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    customDurationMs: 600000 // 10-minute custom window
);
Console.WriteLine($"Created custom window with duration: {customWindow.GetDurationMs()}ms");

// Process data points and get both emitted results and remaining active windows
var dataPoints = new List<DataPoint>
{
    new DataPoint { Id = "1", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Value = 23.5, Quality = 95 },
    new DataPoint { Id = "2", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1000, Value = 24.1, Quality = 92 }
};

var (emitted, activeWindows) = service.ProcessDataPointsWithState(dataPoints);
Console.WriteLine($"Processed {dataPoints.Count} data points");
Console.WriteLine($"Emitted {emitted.Count} window results");
Console.WriteLine($"Active windows remaining: {activeWindows.Count}");

// Get all active windows
var allActiveWindows = service.GetActiveWindows();
Console.WriteLine($"Total active windows: {allActiveWindows.Count}");

// Get complete windows (windows that have finished collecting data)
var completeWindows = service.GetCompleteWindows();
Console.WriteLine($"Complete windows: {completeWindows.Count()}");

// Calculate combined statistics across multiple windows
var windows = service.GetActiveWindows();
var combinedStats = service.CalculateCombinedWindowStatistics(windows);
Console.WriteLine($"Combined stats - Data points: {combinedStats.DataPointCount}, " +
                $"Avg: {combinedStats.Average:F2}, Min: {combinedStats.Min:F2}, Max: {combinedStats.Max:F2}");

// Get the next window ID that would be assigned
long nextWindowId = service.GetNextWindowId();
Console.WriteLine($"Next window ID: {nextWindowId}");
```

## DynamicScalingServiceExtensions

The `DynamicScalingServiceExtensions` class provides convenient extension methods for `DynamicScalingService` to simplify common scaling operations, state queries, and configuration management. It includes methods for scaling pipeline stages up or down, retrieving scaling configuration, and accessing scaling state information.

Example usage:

```csharp
using DotNetRealtimePipeline.Services;
using System;
using System.Threading.Tasks;

// Assume service is an initialized instance of DynamicScalingService
var service = new DynamicScalingService(
    minConsumers: 2,
    maxConsumers: 16,
    scaleUpThresholdPercent: 75.0,
    scaleDownThresholdPercent: 30.0
);

// Get scaling configuration
int minConsumers = service.GetMinConsumers(); // Returns 2
int maxConsumers = service.GetMaxConsumers(); // Returns 16
string thresholdsInfo = service.GetScalingThresholdsInfo(); // Returns "Min: 2, Max: 16, Scale-up: 75%, Scale-down: 30%"

// Scale operations
bool scaledUp = await service.TryScaleUpAsync("DataProcessingStage");
Console.WriteLine($"Scale-up successful: {scaledUp}");

bool scaledDown = await service.TryScaleDownAsync("DataProcessingStage");
Console.WriteLine($"Scale-down successful: {scaledDown}");

// Get scaling states
var statesList = service.GetScalingStatesList();
Console.WriteLine($"Total scaling states: {statesList.Count}");

var state = service.GetOrCreateScalingState("NewStage");
Console.WriteLine($"Stage {state.StageName} has {state.CurrentConsumers} consumers");
```

## BackpressureServiceExtensions

The `BackpressureServiceExtensions` class provides extension methods for `BackpressureService` that simplify backpressure management, buffer monitoring, and system health reporting across pipeline stages. It includes methods for creating and retrieving backpressure contexts, safely managing buffer operations, checking backpressure conditions, and generating comprehensive status reports.

Example usage:

```csharp
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Threading.Tasks;

// Assume service is an initialized instance of BackpressureService
var service = new BackpressureService();

// Create or get a backpressure context for a pipeline stage
var context = service.GetOrCreateContext("DataProcessing", maxBufferCapacity: 10000);
Console.WriteLine($"Created context for stage: {context.PipelineStageName}");

// Safely add items to buffer (returns false if capacity would be exceeded)
bool added = service.SafeAddToBuffer("DataProcessing", 500);
Console.WriteLine($"Added items to buffer: {added}");

// Check current buffer fill percentage
double fillPercent = service.GetBufferFillPercentage("DataProcessing");
Console.WriteLine($"Buffer fill: {fillPercent:N2}%");

// Check if backpressure should be applied
bool shouldBackpressure = service.ShouldApplyBackpressure("DataProcessing");
Console.WriteLine($"Should apply backpressure: {shouldBackpressure}");

// Get dropped item count (indicates data loss from overflow)
long droppedCount = service.GetDroppedItemCount("DataProcessing");
Console.WriteLine($"Dropped items: {droppedCount}");

// Get a comprehensive buffer status report
string report = service.GetBufferStatusReport();
Console.WriteLine(report);

// Register a consumer with timeout support (wait up to 1 second)
bool consumerRegistered = await service.TryRegisterConsumerAsync("DataProcessing", timeoutMs: 1000);
Console.WriteLine($"Consumer registered: {consumerRegistered}");

// Get enhanced system status with derived metrics
var (status, metrics) = service.GetEnhancedSystemStatus();
Console.WriteLine($"System health: {status.GetHealthStatus()}");
Console.WriteLine($"Healthy stages: {metrics.HealthyStages}, Warning: {metrics.WarningStages}, Critical: {metrics.CriticalStages}");

// Record a custom buffer metric
service.RecordBufferMetric("DataProcessing", "CustomMetric", 42);

// Get backpressure frequency (events per minute)
double frequency = service.GetBackpressureFrequency("DataProcessing");
Console.WriteLine($"Backpressure frequency: {frequency:N2} events/min");
```

## EventSubscriberBaseExtensions
The `EventSubscriberBaseExtensions` class provides utility extension methods for working with event subscribers in the real-time pipeline. It offers safe unsubscription handling, subscriber identification, metrics collection, and status monitoring capabilities. These methods enable consistent management of different subscriber types while providing type-specific metrics and health indicators.

Example usage:
```csharp
using DotNetRealtimePipeline.Events;
using DotNetRealtimePipeline.Domain.Models;

// Create a processing completion subscriber
var processingSubscriber = new ProcessingCompletionSubscriber("DataProcessingStage");

// Safe unsubscription (handles already unsubscribed subscribers gracefully)
processingSubscriber.SafeUnsubscribe();

// Get subscriber type name for logging
string subscriberType = processingSubscriber.GetSubscriberTypeName();
Console.WriteLine($"Subscriber type: {subscriberType}");

// Access metrics from different subscriber types
var backpressureSubscriber = new BackpressureAlertSubscriber("BufferMonitor");
int backpressureEvents = backpressureSubscriber.GetBackpressureEventCount();

var metricsSubscriber = new MetricsAggregationSubscriber("PerformanceTracker");
double avgProcessingTime = metricsSubscriber.GetAverageProcessingTime();
int metricsCount = metricsSubscriber.GetMetricsCount();

var errorSubscriber = new ErrorAlertSubscriber("ErrorHandler");
int errorCount = errorSubscriber.GetErrorCount();

// Check if subscriber is in critical state
double successRate = processingSubscriber.GetSuccessRatePercent();
bool isCritical = processingSubscriber.IsInCriticalState();

// Get formatted status string for monitoring
string statusString = processingSubscriber.GetStatusString();
Console.WriteLine($"Status: {statusString}");

// Convert collection to read-only
var subscribers = new List<EventSubscriberBase> { processingSubscriber, backpressureSubscriber };
IReadOnlyList<EventSubscriberBase> readOnlySubscribers = subscribers.AsReadOnly();
```

## PipelineEventPublisherExtensions
The `PipelineEventPublisherExtensions` class provides a set of extension methods for pipeline event publishers, simplifying the process of publishing various pipeline events such as data ingestion, processing completion, and error notifications. It also includes utility methods to monitor subscriber status and inspect active subscription counts across the pipeline.

Example usage:
```csharp
using DotNetRealtimePipeline.Events;
using DotNetRealtimePipeline.Domain.Models;

// Assuming 'publisher' is an initialized instance of PipelineEventPublisher
var dataPoint = new DataPoint { /* ... */ };

// Check subscriber status
if (PipelineEventPublisherExtensions.HasSubscribers(publisher, nameof(DataIngestedEvent)))
{
    var counts = publisher.GetAllSubscriberCounts();
    Console.WriteLine($"Subscribers for DataIngested: {counts[nameof(DataIngestedEvent)]}");
}

// Publish events
await publisher.PublishDataIngestedAsync(dataPoint, new Dictionary<string, object> { { "key", "value" } });
await publisher.PublishProcessingCompletedAsync("123", success: true, stageName: "ProcessingStage");
await publisher.PublishBackpressureDetectedAsync("ProcessingStage", 50, 100, true);
await publisher.PublishPipelineErrorAsync("IngestionOperation", new Exception("Something went wrong"));

// Batch processing
await publisher.PublishDataIngestedBatchAsync(new[] { dataPoint });
```

## RetryHelperJsonExtensions

The `RetryHelperJsonExtensions` class provides System.Text.Json serialization extensions for `RetryHelper`, `RetryPolicy`, and `RetryStatistics` types. It enables serialization to JSON strings and deserialization from JSON strings, which is useful for persisting retry configuration state or transmitting it across process boundaries.

Example usage:

```csharp
using DotNetRealtimePipeline.Utilities;
using Polly;
using System;

// Create retry policy and statistics for serialization
var retryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (exception, delay, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} of 3. Waiting {delay.TotalSeconds}s. Error: {exception.Message}");
        }
    );

var retryStatistics = new RetryStatistics
{
    TotalRetries = 5,
    SuccessfulRetries = 4,
    FailedRetries = 1,
    AverageRetryDelayMs = 1500,
    LastRetryAt = DateTime.UtcNow.AddMinutes(-2)
};

// Serialize retry policy to JSON string (compact format)
string policyJsonCompact = retryPolicy.ToJson();
Console.WriteLine(policyJsonCompact);

// Serialize retry statistics to JSON string (indented for readability)
string statisticsJsonIndented = retryStatistics.ToJson(indented: true);
Console.WriteLine(statisticsJsonIndented);

// Deserialize retry policy from JSON string
string policyJson = @"{
    \"Type\": \"AsyncRetryPolicy\",
    \"RetryCount\": 3,
    \"DelayProvider\": \"ExponentialBackoff\"
}";

var deserializedPolicy = RetryHelperJsonExtensions.FromJsonPolicy(policyJson);
Console.WriteLine($"Deserialized policy type: {deserializedPolicy?.GetType().Name}");

// Try to deserialize retry statistics with error handling
string statsJson = @"{
    \"TotalRetries\": 5,
    \"SuccessfulRetries\": 4,
    \"FailedRetries\": 1,
    \"AverageRetryDelayMs\": 1500
}";

if (RetryHelperJsonExtensions.TryFromJsonStatistics(statsJson, out var tryDeserializedStats))
{
    Console.WriteLine("Successfully deserialized retry statistics");
    Console.WriteLine($"Total retries: {tryDeserializedStats.TotalRetries}");
}

// Handle null/empty JSON gracefully
RetryHelper? nullHelper = RetryHelperJsonExtensions.FromJson(null);
Console.WriteLine($"Null JSON result: {nullHelper}");
```

## ServiceCollectionExtensionsJsonExtensions

The `ServiceCollectionExtensionsJsonExtensions` class provides System.Text.Json serialization extensions for working with `ServiceCollectionExtensions` configuration. It enables serialization to JSON strings and deserialization from JSON strings, which is useful for persisting configuration state or transmitting it across process boundaries.

Example usage:

```csharp
using DotNetRealtimePipeline.Configuration;
using System;

// Serialize ServiceCollectionExtensions configuration to JSON
string jsonCompact = ServiceCollectionExtensionsJsonExtensions.ToJson(null);
Console.WriteLine(jsonCompact);

// Serialize with indentation for readability
string jsonIndented = ServiceCollectionExtensionsJsonExtensions.ToJson(null, indented: true);
Console.WriteLine(jsonIndented);

// Deserialize from JSON string
string json = @"
{
  "type": "ServiceCollectionExtensions",
  "isStaticClass": true,
  "supportsAddPipelineServices": true
}";

var deserializedConfig = ServiceCollectionExtensionsJsonExtensions.FromJson(json);
Console.WriteLine($"Deserialized type: {deserializedConfig?.Type}");

// Try to deserialize with error handling
if (ServiceCollectionExtensionsJsonExtensions.TryFromJson(json, out var tryDeserializedConfig))
{
    Console.WriteLine("Successfully deserialized configuration");
}
else
{
    Console.WriteLine("Failed to deserialize configuration");
}

// Handle null/empty JSON gracefully
var nullConfig = ServiceCollectionExtensionsJsonExtensions.FromJson(null);
Console.WriteLine($"Null JSON result: {nullConfig}");
```

## DateTimeExtensionsJsonExtensions

The `DateTimeExtensionsJsonExtensions` class provides System.Text.Json serialization extensions for DateTime-related types, enabling easy serialization to JSON strings and deserialization from JSON strings using Unix milliseconds timestamps. This is particularly useful for persisting DateTime values or transmitting them across process boundaries.

Example usage:

```csharp
using DotNetRealtimePipeline.Utilities;
using System;

// Serialize DateTime to JSON string
var dateTime = DateTime.UtcNow;
string jsonCompact = dateTime.ToJson(); // Compact format
string jsonIndented = dateTime.ToJson(indented: true); // Indented for readability
Console.WriteLine(jsonCompact);

// Serialize DateTimeOffset to JSON string
var dateTimeOffset = DateTimeOffset.UtcNow;
string offsetJson = dateTimeOffset.ToJson();
Console.WriteLine(offsetJson);

// Serialize Unix timestamp to JSON string
long unixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
string timestampJson = unixMilliseconds.ToJson();
Console.WriteLine(timestampJson);

// Deserialize JSON string to DateTime
string dateTimeJson = "1719369600000"; // 2024-06-25 00:00:00 UTC
DateTime? deserializedDateTime = DateTimeExtensionsJsonExtensions.FromJsonToDateTime(dateTimeJson);
Console.WriteLine($"Deserialized DateTime: {deserializedDateTime?.ToString("O")}");

// Deserialize JSON string to DateTimeOffset
dateTimeJson = "1719369600000";
DateTimeOffset? deserializedOffset = DateTimeExtensionsJsonExtensions.FromJsonToDateTimeOffset(dateTimeJson);
Console.WriteLine($"Deserialized DateTimeOffset: {deserializedOffset?.ToString("O")}");

// Deserialize JSON string to Unix timestamp
string timestampJsonInput = "1719369600000";
long? deserializedTimestamp = DateTimeExtensionsJsonExtensions.FromJsonToUnixMilliseconds(timestampJsonInput);
Console.WriteLine($"Deserialized timestamp: {deserializedTimestamp}");

// Try to deserialize with error handling
string invalidJson = "invalid";
if (DateTimeExtensionsJsonExtensions.TryFromJsonToDateTime(invalidJson, out var tryDateTime))
{
    Console.WriteLine("Successfully deserialized (should not reach here)");
}
else
{
    Console.WriteLine("Failed to deserialize invalid JSON");
}

// Handle null/empty JSON gracefully
DateTime? nullDateTime = DateTimeExtensionsJsonExtensions.FromJsonToDateTime(null);
Console.WriteLine($"Null JSON result: {nullDateTime}"); // Output: Null JSON result:
```

## DateTimeExtensionsValidation

The `DateTimeExtensionsValidation` static class provides validation helpers for `DateTimeExtensions` static class methods. It includes extension methods for validating DateTime values and Unix timestamp parameters used in time-based pipeline operations, ensuring they meet business rules and data integrity constraints before conversion or processing.

Example usage:

```csharp
using DotNetRealtimePipeline.Utilities;
using System;

// Validate a DateTime value for conversion to Unix milliseconds
var dateTime = DateTime.UtcNow;
var validationErrors = dateTime.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("DateTime validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if DateTime is valid for Unix milliseconds conversion
bool isValid = dateTime.IsValid();
Console.WriteLine($"DateTime is valid: {isValid}");

// Ensure validity (throws ArgumentException if invalid)
dateTime.EnsureValid();

// Validate Unix timestamp in milliseconds for time window operations
long timestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
long windowSizeMs = 300000; // 5-minute window

var timestampErrors = timestampMs.Validate(windowSizeMs);
if (timestampErrors.Count > 0)
{
    Console.WriteLine("Timestamp validation failed:");
    foreach (var error in timestampErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if timestamp and window size are valid
bool timestampIsValid = timestampMs.IsValid(windowSizeMs);
Console.WriteLine($"Timestamp is valid: {timestampIsValid}");

// Ensure timestamp validity (throws ArgumentException if invalid)
timestampMs.EnsureValid(windowSizeMs);

// Validate with parameter name for window boundary rounding
string paramName = nameof(timestampMs);
var paramErrors = timestampMs.Validate(windowSizeMs, paramName);
if (paramErrors.Count > 0)
{
    Console.WriteLine("Parameter validation failed:");
    foreach (var error in paramErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Ensure parameter validity (throws ArgumentException if invalid)
timestampMs.EnsureValid(windowSizeMs, paramName);
```

## InMemoryMetricsRepositoryExtensions

The `InMemoryMetricsRepositoryExtensions` class provides extension methods for `InMemoryMetricsRepository` that enhance metric querying capabilities with additional convenience methods for working with metric data. It includes methods for retrieving metrics by type and time range, calculating processing time statistics, and filtering metrics by various criteria.

Example usage:

```csharp
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

// Assume repository is an initialized instance of InMemoryMetricsRepository
var repository = new InMemoryMetricsRepository();

// Add some sample metrics for demonstration
await repository.AddOrUpdateAsync(new MetricAggregation
{
    MetricId = "metrics-001",
    MetricType = "PipelinePerformance",
    TimeWindowStartMs = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeMilliseconds(),
    TimeWindowEndMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    TotalItemsProcessed = 5000,
    TotalItemsFailed = 50,
    AverageProcessingTimeMs = 45.2,
    MaxProcessingTimeMs = 250,
    MinProcessingTimeMs = 5,
    P95ProcessingTimeMs = 120,
    P99ProcessingTimeMs = 180,
    ComputedAt = DateTime.UtcNow
});

// Get the latest metric of a specific type
var latestMetric = await repository.GetLatestByTypeAsync("PipelinePerformance");
Console.WriteLine($"Latest metric: {latestMetric?.MetricId}");

// Get metrics filtered by type and time range
var metricsInRange = await repository.GetByTypeAndTimeRangeAsync(
    "PipelinePerformance",
    DateTimeOffset.UtcNow.AddMinutes(-30).ToUnixTimeMilliseconds(),
    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
);
Console.WriteLine($"Metrics in range: {metricsInRange.Count}");

// Calculate processing time statistics
var avgProcessingTime = await repository.GetAverageProcessingTimeAsync(
    "PipelinePerformance",
    DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds(),
    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
);
Console.WriteLine($"Average processing time: {avgProcessingTime}ms");

var maxProcessingTime = await repository.GetMaxProcessingTimeAsync(
    "PipelinePerformance",
    DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds(),
    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
);
Console.WriteLine($"Max processing time: {maxProcessingTime}ms");

var minProcessingTime = await repository.GetMinProcessingTimeAsync(
    "PipelinePerformance",
    DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds(),
    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
);
Console.WriteLine($"Min processing time: {minProcessingTime}ms");

// Get metrics filtered by multiple types
var metricsByTypes = await repository.GetByTypesAsync(new[] { "PipelinePerformance", "SystemHealth" });
Console.WriteLine($"Metrics across multiple types: {metricsByTypes.Count}");

// Get metrics with processing time filtering
var filteredMetrics = await repository.GetByTypeAndTimeRangeWithProcessingTimeFilterAsync(
    "PipelinePerformance",
    DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds(),
    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    10,    // min processing time
    100    // max processing time
);
Console.WriteLine($"Filtered metrics: {filteredMetrics.Count}");

// Get the last N metrics across all types
var lastNMetrics = await repository.GetLastNMetricsAsync(10);
Console.WriteLine($"Last {lastNMetrics.Count} metrics: {string.Join(", ", lastNMetrics.Select(m => m.MetricId))}");
```

## CompressionHelperValidation

The `CompressionHelperValidation` static class provides validation helpers for compression operations within the pipeline. It includes methods for validating compression/decompression operations across different algorithms (GZip, Deflate), file operations, and compression ratio calculations. This ensures data integrity when working with compressed data streams and files.

Example usage:

```csharp
using DotNetRealtimePipeline.Utilities;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

// Create sample data for compression
var originalData = Encoding.UTF8.GetBytes(
    "This is a sample text that will be compressed and decompressed to demonstrate the CompressionHelperValidation usage. " +
    "The validation ensures that compression operations maintain data integrity throughout the pipeline processing."
);

// Validate compression/decompression operations
var gzipValidationErrors = CompressionHelperValidation.ValidateForCompressGzip(originalData);
if (gzipValidationErrors.Count > 0)
{
    Console.WriteLine("GZip compression validation failed:");
    foreach (var error in gzipValidationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if compression is valid
bool isGzipValid = CompressionHelperValidation.IsValidForCompression(originalData);
Console.WriteLine($"GZip compression is valid: {isGzipValid}");

// Ensure compression validity (throws if invalid)
CompressionHelperValidation.EnsureValidForCompression(originalData);

// Compress and decompress data
byte[] compressedData = CompressionHelper.CompressGzip(originalData);
byte[] decompressedData = CompressionHelper.DecompressGzip(compressedData);

// Validate decompression
var decompressionErrors = CompressionHelperValidation.ValidateForDecompressGzip(compressedData);
if (decompressionErrors.Count > 0)
{
    Console.WriteLine("Decompression validation failed:");
    foreach (var error in decompressionErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check decompression validity
bool isDecompressionValid = CompressionHelperValidation.IsValidForDecompression(compressedData);
Console.WriteLine($"Decompression is valid: {isDecompressionValid}");

// Ensure decompression validity
CompressionHelperValidation.EnsureValidForDecompression(compressedData);

// Validate file operations
var filePath = "test_compression.dat";
await File.WriteAllBytesAsync(filePath, originalData);

var fileValidationErrors = CompressionHelperValidation.ValidateForCompressFileAsync(filePath);
if (fileValidationErrors.Count > 0)
{
    Console.WriteLine("File compression validation failed:");
    foreach (var error in fileValidationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Calculate and validate compression ratio
var ratioValidationErrors = CompressionHelperValidation.ValidateForCalculateCompressionRatio(originalData, compressedData);
if (ratioValidationErrors.Count > 0)
{
    Console.WriteLine("Compression ratio validation failed:");
    foreach (var error in ratioValidationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate Deflate compression
var deflateValidationErrors = CompressionHelperValidation.ValidateForCompressDeflate(originalData);
if (deflateValidationErrors.Count > 0)
{
    Console.WriteLine("Deflate compression validation failed:");
    foreach (var error in deflateValidationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate comprehensive compression analysis
var analysisValidationErrors = CompressionHelperValidation.ValidateForAnalyzeCompression(originalData);
if (analysisValidationErrors.Count > 0)
{
    Console.WriteLine("Compression analysis validation failed:");
    foreach (var error in analysisValidationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Compare different compression algorithms
var comparisonValidationErrors = CompressionHelperValidation.ValidateForCompareAlgorithms(originalData);
if (comparisonValidationErrors.Count > 0)
{
    Console.WriteLine("Algorithm comparison validation failed:");
    foreach (var error in comparisonValidationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

Console.WriteLine("All compression validations completed successfully!");
```

## BatchProcessorValidation

The `BatchProcessorValidation` static class provides validation helpers for `BatchProcessingProgress` and `DataPointBatchProcessor` instances. It includes methods for comprehensive validation of batch processing progress, checking validity status, and throwing exceptions when invalid states are detected. This ensures batch processor configurations and progress tracking are properly validated before use.

Example usage:

```csharp
using DotNetRealtimePipeline.Utilities;
using System;
using System.Collections.Generic;

// Create a batch processing progress instance
var progress = new BatchProcessingProgress
{
    TotalBatches = 10,
    ProcessedBatches = 3,
    TotalItems = 10000,
    ProcessedItems = 3500,
    StartTime = DateTime.UtcNow.AddMinutes(-5),
    LastUpdateTime = DateTime.UtcNow
};

// Validate batch processing progress
var validationErrors = progress.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Batch processing progress validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if progress is valid using IsValid extension method
bool isValid = progress.IsValid();
Console.WriteLine($"Batch processing progress is valid: {isValid}");

// Ensure validity (throws ArgumentException if invalid)
progress.EnsureValid();

// Create a DataPointBatchProcessor instance (default batchSize=1000, maxDegreeOfParallelism=4)
var batchProcessor = new DataPointBatchProcessor();

// Validate the batch processor instance
var processorErrors = batchProcessor.Validate();
if (processorErrors.Count > 0)
{
    Console.WriteLine("Batch processor validation failed:");
    foreach (var error in processorErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if batch processor is valid
bool processorIsValid = batchProcessor.IsValid();
Console.WriteLine($"DataPointBatchProcessor is valid: {processorIsValid}");

// Ensure batch processor validity (throws if invalid)
batchProcessor.EnsureValid();
```

## CacheServiceExtensions

The `CacheServiceExtensions` class provides extension methods for `CacheService<TKey, TValue>` that enhance cache operations with batch operations, time-based caching, and convenient utility methods. It includes methods for getting/setting multiple values in a single operation, conditional value retrieval with factory methods, and cache statistics reporting.

Example usage:

```csharp
using DotNetRealtimePipeline.Caching;
using System;
using System.Collections.Generic;
using System.Linq;

// Initialize cache with a maximum capacity of 1000 items
var cache = new CacheService<string, SensorData>(maxCapacity: 1000);

// Get or add a value with automatic creation if missing
var sensorData = cache.GetOrAdd("sensor-001", key => new SensorData(key, 23.5, DateTime.UtcNow));
Console.WriteLine($"Retrieved sensor data: {sensorData.Temperature}°C");

// Get or add with custom TTL (5 minutes)
var cachedResult = cache.GetOrAdd("expensive-calculation", TimeSpan.FromMinutes(5), key => 
    PerformExpensiveCalculation(key));
Console.WriteLine($"Cached result: {cachedResult}");

// Set multiple values at once
var itemsToAdd = new Dictionary<string, SensorData>
{
    ["sensor-002"] = new SensorData("sensor-002", 24.1, DateTime.UtcNow),
    ["sensor-003"] = new SensorData("sensor-003", 22.8, DateTime.UtcNow),
    ["sensor-004"] = new SensorData("sensor-004", 25.3, DateTime.UtcNow)
};
cache.SetRange(itemsToAdd);
Console.WriteLine($"Added {itemsToAdd.Count} items to cache");

// Get multiple values in a single operation
var requestedKeys = new[] { "sensor-001", "sensor-002", "sensor-005" };
var retrievedData = cache.GetRange(requestedKeys);
Console.WriteLine($"Retrieved {retrievedData.Count} items from cache");

// Remove multiple keys in a single operation
var keysToRemove = new[] { "sensor-002", "sensor-003" };
int removedCount = cache.RemoveRange(keysToRemove);
Console.WriteLine($"Removed {removedCount} items from cache");

// Get cache statistics for monitoring
string statsString = cache.ToPerformanceCounterString();
Console.WriteLine($"Cache stats: {statsString}");

// Get value or default (returns null instead of throwing if not found)
var missingValue = cache.GetValueOrDefault("non-existent-key");
Console.WriteLine($"Missing value: {missingValue}");

// Get cache utilization ratio (0.0 to 1.0)
double utilization = cache.GetUtilizationRatio();
Console.WriteLine($"Cache utilization: {utilization:P0}");

// Helper method for example
static SensorData PerformExpensiveCalculation(string sensorId)
{
    // Simulate expensive operation
    return new SensorData(sensorId, 42.0, DateTime.UtcNow);
}

public record SensorData(string SensorId, double Temperature, DateTime Timestamp);
```

## MetricAggregationExtensions
The `MetricAggregationExtensions` class provides extension methods for `MetricAggregation` to simplify common metric calculations, aggregations, and filtering operations. It includes methods for calculating success rates, error rates, time window durations, and source-specific aggregations.

Example usage:
```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Linq;

// Assume aggregation is a MetricAggregation from pipeline metrics collection
var aggregation = new MetricAggregation
{
    MetricId = "pipeline-metrics-20240719",
    MetricType = "PipelinePerformance",
    TimeWindowStartMs = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds(),
    TimeWindowEndMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    TotalItemsProcessed = 10000,
    TotalItemsFailed = 250,
    TotalItemsSkipped = 120,
    BackpressureEvents = 8,
    TotalBackpressureMs = 1500,
    AverageProcessingTimeMs = 45.2,
    MinProcessingTimeMs = 5,
    MaxProcessingTimeMs = 250,
    P95ProcessingTimeMs = 120,
    P99ProcessingTimeMs = 180,
    CountBySource = new Dictionary<string, long>
    {
        ["sensor-ingest"] = 4500,
        ["api-ingest"] = 3200,
        ["batch-ingest"] = 2300
    },
    ErrorRateByStage = new Dictionary<string, double>
    {
        ["validation"] = 0.02,
        ["transformation"] = 0.01,
        ["aggregation"] = 0.005
    },
    ComputedAt = DateTime.UtcNow
};

// Calculate success rate (0.0 to 1.0)
double successRate = aggregation.CalculateSuccessRate();
Console.WriteLine($"Success rate: {successRate:P2}"); // Output: Success rate: 97.50%

// Calculate combined error rate across all stages
double errorRate = aggregation.CalculateCombinedErrorRate();
Console.WriteLine($"Combined error rate: {errorRate:P2}"); // Output: Combined error rate: 1.17%

// Get time window duration in milliseconds and TimeSpan
long durationMs = aggregation.GetTimeWindowDurationMs();
TimeSpan duration = aggregation.GetTimeWindowDuration();
Console.WriteLine($"Duration: {durationMs}ms ({duration.TotalSeconds:F1}s)");

// Get source names and total items across all sources
var sourceNames = aggregation.GetSourceNames().ToList();
long totalItems = aggregation.GetTotalItemsFromSources();
Console.WriteLine($"Sources: {string.Join(", ", sourceNames)} | Total items: {totalItems}");

// Get stages with errors and backpressure percentage
var errorStages = aggregation.GetStagesWithErrors().ToList();
double backpressurePercent = aggregation.GetBackpressurePercentage();
Console.WriteLine($"Stages with errors: {string.Join(", ", errorStages)}");
Console.WriteLine($"Backpressure time: {backpressurePercent:F1}% of window");

// Get average percentile and combine multiple aggregations
var percentileAvg = aggregation.GetAveragePercentile();
Console.WriteLine($"Average percentile (P95/P99): {percentileAvg:F0}ms");

// Combine multiple aggregations
var combined = new[] { aggregation, aggregation }.Combine();
Console.WriteLine($"Combined total items: {combined.TotalItemsProcessed}");

// Filter by specific sources
var filtered = aggregation.FilterBySource(source => source.StartsWith("sensor"));
Console.WriteLine($"Filtered total items (sensor only): {filtered.TotalItemsProcessed}");
```

## ApiEndpointHandlerExtensions
The `ApiEndpointHandlerExtensions` class provides helper methods to streamline the creation of structured API responses, including standard successful and error results. It also simplifies the construction of paginated responses and allows for the seamless inclusion of batch processing statistics in the API output.

Example usage:
```csharp
using DotNetRealtimePipeline.API;

// Successful response
var okResponse = ApiEndpointHandlerExtensions.Ok(new { Id = 1, Name = "Success" });

// Error response
var errorResponse = ApiEndpointHandlerExtensions.Error<object>("Failed to process");

// Paginated response
var items = new List<string> { "item1", "item2", "item3" };
var paginated = items.ToPaginatedResponse(page: 1, pageSize: 10, totalCount: 3);

// Accessing pagination metadata
int totalPages = paginated.Data.TotalPages;
int currentPage = paginated.Data.Page;
```

## BackgroundProcessingWorkerValidation

The `BackgroundProcessingWorkerValidation` static class provides validation helpers for `BackgroundProcessingWorker` and related worker types. It includes extension methods for validating worker instances, checking validity status, and throwing exceptions when invalid states are detected. This ensures background processing workers are properly configured before execution.

Example usage:

```csharp
using DotNetRealtimePipeline.Workers;
using System;
using System.Threading.Tasks;

// Assume worker is an initialized instance of BackgroundProcessingWorker
var worker = new BackgroundProcessingWorker(
    stageName: "DataProcessing",
    intervalMs: 1000,
    maxConcurrentTasks: 4
);

// Validate the worker instance
var validationErrors = worker.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Worker validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if worker is valid using IsValid extension method
bool isValid = worker.IsValid();
Console.WriteLine($"Worker is valid: {isValid}");

// Ensure validity (throws ArgumentException if invalid)
worker.EnsureValid();
```

## ApiEndpointHandlerValidation
The `ApiEndpointHandlerValidation` static class provides a set of extension methods for validating common API-related objects within the pipeline, such as `ApiEndpointHandler.ApiResponse<T>`, `BatchIngestResult`, and `PipelineStatusInfo`. It allows for concise validation of these objects using `Validate` to retrieve errors, `IsValid` to check status, or `EnsureValid` to throw an exception upon invalid state.

Example usage:
```csharp
using DotNetRealtimePipeline.API;

// Example using PipelineStatusInfo
var status = new PipelineStatusInfo {
    PipelineName = "MyPipeline",
    Version = "v1.0.0",
    TotalProcessed = 100,
    TotalFailed = 0,
    Pending = 0,
    HealthStatus = "Healthy"
};

// Check if the object is valid
if (status.IsValid())
{
    // Process the status
}
else
{
    // Retrieve validation errors
    var errors = status.Validate();
    Console.WriteLine($"Validation failed: {string.Join(", ", errors)}");
}

// Or, ensure validity by throwing an exception if invalid
status.EnsureValid();
```

## DeadLetterQueueExtensions
The `DeadLetterQueueExtensions` class provides extension methods for working with dead-letter entries in the pipeline. It includes methods for processing failed entries for retry, finding entries by various criteria, and generating comprehensive reports of dead-letter queue state.

Example usage:
```csharp
using DotNetRealtimePipeline.DeadLetter;
using System;
using System.Threading.Tasks;

// Assume queue is an initialized instance of DeadLetterQueue
var queue = new DeadLetterQueue();

// Process entries for retry (up to 50 entries)
var result = await queue.ProcessForRetryAsync(
    maxCount: 50,
    processEntry: async entry => {
        // Your retry logic here
        try {
            // Attempt to reprocess the data point
            await ProcessDataPointAsync(entry.DataPoint);
            return true; // Success
        } catch {
            return false; // Requeue for another attempt
        }
    }
);

Console.WriteLine($"Processed {result.TotalProcessed} entries: " +
            $"{result.SuccessfullyProcessed} succeeded, " +
            $"{result.FailedProcessing} failed");

// Find entries by failure stage
var failedStageEntries = await queue.FindByStageAsync("DataProcessingStage", maxCount: 100);

// Find entries matching a custom predicate
var recentFailures = await queue.FindAsync(
    entry => entry.EnqueuedAt > DateTime.UtcNow.AddHours(-1),
    maxCount: 50
);

// Generate a detailed report
string report = await queue.GetReportAsync(includeDetails: true);
Console.WriteLine(report);

// Access processing result properties
int totalProcessed = result.TotalProcessed;
int successful = result.SuccessfullyProcessed;
int failed = result.FailedProcessing;
IReadOnlyList<DeadLetterEntry> processedEntries = result.EntriesProcessed;
```

## WebhookHandlerExtensions
The `WebhookHandlerExtensions` class provides convenient extension methods for `WebhookHandler` to simplify webhook subscription management, event dispatching, and subscription inspection. It includes methods for subscribing to specific event types, unsubscribing, checking subscription status, managing failed subscriptions, and retrieving subscription statistics.

Example usage:
```csharp
using DotNetRealtimePipeline.Integration;
using System;
using System.Threading.Tasks;

// Assume handler is an initialized instance of WebhookHandler
var handler = new WebhookHandler();

// Subscribe to a single event type
var webhookUrl = "https://api.example.com/webhooks/events";
handler.SubscribeTo(webhookUrl, WebhookEventType.DataIngested);

// Subscribe to multiple event types with a secret for verification
var eventTypes = new[] { WebhookEventType.DataIngested, WebhookEventType.ProcessingCompleted };
handler.SubscribeTo(webhookUrl, eventTypes, "my-secret-key");

// Check if there are active subscriptions for an event type
bool hasSubscribers = handler.HasSubscriptions(WebhookEventType.DataIngested);
Console.WriteLine($"Active subscriptions: {hasSubscribers}");

// Get subscription count for an event type
int subscriptionCount = handler.GetSubscriptionCount(WebhookEventType.DataIngested);
Console.WriteLine($"Subscription count: {subscriptionCount}");

// Get all subscriptions for a specific event type
var subscriptions = handler.GetSubscriptionsFor(WebhookEventType.DataIngested);
foreach (var subscription in subscriptions)
{
    Console.WriteLine($"Subscription: {subscription.Url}, Active: {subscription.IsActive}");
}

// Send a webhook event to all subscribers
await handler.SendWebhookEventAsync(WebhookEventType.DataIngested, new { Message = "Data ingestion completed", Timestamp = DateTime.UtcNow });

// Unsubscribe from all subscriptions matching a URL
bool unsubscribed = handler.UnsubscribeFrom(webhookUrl);
Console.WriteLine($"Unsubscribed: {unsubscribed}");

// Disable subscriptions that have failed too many times
int disabledCount = handler.DisableFailedSubscriptions(failureThreshold: 3);
Console.WriteLine($"Disabled {disabledCount} failed subscriptions");

// Get statistics about subscriptions
var oldestActive = handler.GetOldestActiveSubscription();
var mostRecentDelivery = handler.GetMostRecentDelivery();
```

## RateLimitingMiddlewareValidation
The `RateLimitingMiddlewareValidation` static class provides validation helpers for `RateLimitingMiddleware` and related rate limiting components. It includes methods for validating middleware instances, checking validity status, and validating rate limit parameters to ensure proper configuration before use.

Example usage:
```csharp
using DotNetRealtimePipeline.Middleware;
using System;

// Create a rate limiting middleware instance with default configuration
var middleware = new RateLimitingMiddleware(
    tokensPerSecond: 100,
    maxBurstSize: 200
);

// Validate the middleware instance
var validationErrors = middleware.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Middleware validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if middleware is valid
bool isValid = middleware.IsValid();
Console.WriteLine($"Middleware is valid: {isValid}");

// Ensure middleware is valid (throws if invalid)
middleware.EnsureValid();

// Validate rate limit parameters before attempting to acquire tokens
var parameterErrors = RateLimitingMiddlewareValidation.ValidateParameters(
    identifier: "user-123",
    tokensRequired: 5
);
if (parameterErrors.Count > 0)
{
    Console.WriteLine("Parameter validation failed:");
    foreach (var error in parameterErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate RateLimitStatus instance
var status = new RateLimitStatus
{
    AvailableTokens = 150,
    Capacity = 200,
    ResetTime = DateTime.UtcNow.AddMinutes(1)
};

var statusErrors = status.Validate();
if (statusErrors.Count > 0)
{
    Console.WriteLine("Status validation failed:");
    foreach (var error in statusErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

status.EnsureValid();
```

## WebhookHandlerValidation
The `WebhookHandlerValidation` static class provides validation extension methods for webhook-related components, including `WebhookHandler`, `WebhookSubscription`, and `WebhookPayload`. It allows for concise validation of these components using `Validate` to retrieve errors, `IsValid` to check status, or `EnsureValid` to throw an exception upon invalid state.

Example usage:
```csharp
using DotNetRealtimePipeline.Integration;

// Example using WebhookSubscription
var subscription = new WebhookSubscription {
    Id = Guid.NewGuid().ToString(),
    Url = "https://api.example.com/webhook",
    EventTypes = 1, // Example enum value
    CreatedAt = DateTime.UtcNow
};

// Check if the object is valid
if (subscription.IsValid())
{
    // Process the subscription
}
else
{
    // Retrieve validation errors
    var errors = subscription.Validate();
    Console.WriteLine($"Validation failed: {string.Join(", ", errors)}");
}

// Or, ensure validity by throwing an exception if invalid
subscription.EnsureValid();
```

## LoggingMiddlewareValidation

The `LoggingMiddlewareValidation` static class provides validation helpers for `LoggingMiddleware` and related middleware classes. It includes methods for validating data points, processing results, backpressure contexts, metrics, error logging parameters, performance warnings, correlation IDs, and correlation operations. These validation methods help ensure that logging operations receive valid parameters before attempting to log data.

Example usage:

```csharp
using DotNetRealtimePipeline.Middleware;
using DotNetRealtimePipeline.Domain.Models;
using System;

// Validate a DataPoint for logging
var dataPoint = new DataPoint {
    Id = 1,
    Source = "TemperatureSensor",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    Value = 23.5,
    Quality = 95
};

// Validate parameters (returns list of problems)
var validationErrors = dataPoint.Validate("DataIngestion");
if (validationErrors.Count > 0)
{
    Console.WriteLine("DataPoint validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Check if valid (returns boolean)
bool isValid = dataPoint.IsValid("DataIngestion");
Console.WriteLine($"DataPoint is valid: {isValid}");

// Ensure validity (throws if invalid)
dataPoint.EnsureValid("DataIngestion");

// Validate ProcessingResult for logging
var result = new ProcessingResult(
    stageName: "DataProcessing",
    success: true,
    processedAt: DateTime.UtcNow
);

// Validate elapsed time parameter
var resultErrors = result.Validate(elapsedMs: 125);
if (resultErrors.Count > 0)
{
    Console.WriteLine("ProcessingResult validation failed:");
    foreach (var error in resultErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate backpressure context
var context = new BackpressureContext(
    pipelineStageName: "DataProcessing",
    maxBufferCapacity: 10000,
    maxConcurrentConsumers: 4
);

var contextErrors = LoggingMiddlewareValidation.Validate("DataProcessing", context);
if (contextErrors.Count > 0)
{
    Console.WriteLine("BackpressureContext validation failed:");
    foreach (var error in contextErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate metric parameters
var metricErrors = LoggingMiddlewareValidation.Validate(
    metricName: "ProcessingTime",
    value: 125,
    unit: "ms"
);
if (metricErrors.Count > 0)
{
    Console.WriteLine("Metric validation failed:");
    foreach (var error in metricErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Validate error logging parameters
var error = new Exception("Something went wrong");
var errorErrors = LoggingMiddlewareValidation.Validate(
    operationName: "DataProcessing",
    ex: error,
    context: "Processing pipeline stage"
);
if (errorErrors.Count > 0)
{
    Console.WriteLine("Error logging validation failed:");
    foreach (var err in errorErrors)
    {
        Console.WriteLine($"- {err}");
    }
}

// Validate performance warning parameters
var perfErrors = LoggingMiddlewareValidation.Validate(
    operationName: "DataProcessing",
    elapsedMs: 125,
    thresholdMs: 100
);
if (perfErrors.Count > 0)
{
    Console.WriteLine("Performance warning validation failed:");
    foreach (var err in perfErrors)
    {
        Console.WriteLine($"- {err}");
    }
}

// Validate correlation ID
var correlationId = Guid.NewGuid().ToString();
var correlationErrors = LoggingMiddlewareValidation.Validate(correlationId);
if (correlationErrors.Count > 0)
{
    Console.WriteLine("Correlation ID validation failed:");
    foreach (var err in correlationErrors)
    {
        Console.WriteLine($"- {err}");
    }
}

// Validate correlation operation
Func<string, Task<int>> operation = async correlationId => {
    await Task.Delay(100);
    return 42;
};

var operationErrors = LoggingMiddlewareValidation.Validate(operation);
if (operationErrors.Count > 0)
{
    Console.WriteLine("Correlation operation validation failed:");
    foreach (var err in operationErrors)
    {
        Console.WriteLine($"- {err}");
    }
}
```
