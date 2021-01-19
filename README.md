Pipeline EPS : 0.0
```

## PipelineVisualizationNode

`PipelineVisualizationNode` represents a single stage node in the pipeline visualization graph. It carries display-time state such as buffer fill percentage, health status, throughput, dropped items, and downstream connections. The class provides methods for computing health labels and rendering compact single-line summaries.

## PipelineException

`PipelineException` is the base exception type for pipeline-specific errors. It carries additional error context through three public properties: `ErrorCode`, `ErrorDetails`, and `StageName`. Use it to wrap domain-specific failures while preserving structured error information for logging, retry policies, and observability tooling.

```csharp
using DotNetRealtimePipeline.Domain.Exceptions;

// Create a pipeline exception with just a message
var exception = new PipelineException("Data ingestion failed due to network timeout");
Console.WriteLine(exception.Message); // Data ingestion failed due to network timeout

// Create with error code and details
var detailedException = new PipelineException("Invalid data point received", "INVALID_DATA_POINT", new { DataPoint = "invalid-value", ExpectedType = "double" });
Console.WriteLine(detailedException.ErrorCode); // INVALID_DATA_POINT
Console.WriteLine(detailedException.ErrorDetails); // { DataPoint = invalid-value, ExpectedType = double }

// Create a stage-specific exception
var stageException = new StageProcessingException("Processing window timed out", "WINDOW_TIMEOUT", "DataProcessing", 3);
Console.WriteLine(stageException.StageName); // DataProcessing
Console.WriteLine(stageException.RetryCount); // 3

// Create a backpressure exception
var backpressureException = new BackpressureException("Buffer capacity exceeded", 1024, 512);
Console.WriteLine(backpressureException.BufferSize); // 1024
Console.WriteLine(backpressureException.MaxCapacity); // 512

// Create a timeout exception
var timeoutException = new ProcessingTimeoutException("Window processing exceeded timeout", 5000);
Console.WriteLine(timeoutException.TimeoutMs); // 5000
```

```csharp
using DotNetRealtimePipeline.Visualization;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();

// Create a visualization node for an ingestion stage
var ingestionNode = new PipelineVisualizationNode
{
    StageName = "DataIngestion",
    StageType = "SOURCE",
    BufferFillPercent = 45.2,
    IsBackpressured = false,
    ThroughputEps = 1250.5,
    DroppedItems = 2,
    DownstreamStages = new List<string> { "DataProcessing", "DataValidation" }
};

// Compute health label based on buffer state
string healthLabel = ingestionNode.ComputeHealthLabel();
Console.WriteLine($"Health: {healthLabel}"); // HEALTHY, WARNING, or CRITICAL

// Render a compact single-line summary
string inlineSummary = ingestionNode.ToInlineString();
Console.WriteLine(inlineSummary);
// Output: [DataIngestion | HEALTHY | buf=45% | 1250.5 eps]

// Create a node for a processing stage with backpressure
var processingNode = new PipelineVisualizationNode
{
    StageName = "DataProcessing",
    StageType = "TRANSFORM",
    BufferFillPercent = 85.7,
    IsBackpressured = true,
    ThroughputEps = 890.2,
    DroppedItems = 15,
    DownstreamStages = new List<string> { "WindowingService" }
};

Console.WriteLine(processingNode.ToInlineString());
// Output: [DataProcessing | CRITICAL | buf=86% | 890.2 eps]
```

## StreamEvent

`StreamEvent` represents an event flowing through the stream at a specific point in time. It wraps data points with metadata about their processing context, including timestamps, priority levels, source systems, correlation identifiers, and processing state. This type is used throughout the pipeline to track events as they move through various stages and provides methods for monitoring processing progress.

```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Create a stream event for a data point
var dataEvent = new StreamEvent(1, 1001, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data")
{
    SourceSystem = "IoTDevice",
    Priority = 7,
    CorrelationId = "corr-iot-001",
    Payload = new Dictionary<string, object>
    {
        { "sensorType", "temperature" },
        { "value", 23.5 },
        { "unit", "celsius" }
    }
};

Console.WriteLine(dataEvent.GetSummary());
// Output: StreamEvent[Id=1, Type=data, Priority=7, Stages=0, Completed=False]

// Mark event as processed by ingestion stage
dataEvent.MarkProcessedByStage("DataIngestion");

// Mark event as processed by processing stage
dataEvent.MarkProcessedByStage("DataProcessing");

Console.WriteLine(dataEvent.GetProcessingPath());
// Output: DataIngestion -> DataProcessing

// Check if processed by specific stage
bool processedByIngestion = dataEvent.HasBeenProcessedByStage("DataIngestion");
Console.WriteLine(processedByIngestion); // True

// Complete processing
var processingTime = dataEvent.GetTotalProcessingTimeMs();
dataEvent.CompleteProcessing();

// Create a retry event after failure
var retryEvent = new StreamEvent(2, 1002, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "error")
{
    SourceSystem = "IoTDevice",
    Priority = 8,
    IsRetry = true,
    RetryAttempt = 1,
    LastErrorMessage = "Sensor timeout"
};

// Create a child event derived from parent
var childEvent = dataEvent.CreateChildEvent(3, "derived");
Console.WriteLine(childEvent.CausationId); // 1 (parent EventId)
Console.WriteLine(childEvent.CorrelationId); // Same as parent correlation ID
```

## ProcessingResult

`ProcessingResult` represents the outcome of processing a data point or window through the pipeline. It tracks success/failure status, processing metrics (time, retries), error details, and output data. This type is used throughout the pipeline to propagate results between stages and provide observability into processing operations.

```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Create a successful processing result
var successResult = new ProcessingResult(1, true, "DataProcessing")
{
    ProcessingTimeMs = 150,
    CorrelationId = "corr-12345",
    OutputData = new Dictionary<string, object> { { "processedItems", 42 }, { "throughput", 890.2 } }
};

Console.WriteLine(successResult.GetSummary());
// Output: Result[Id=1, Stage=DataProcessing, Success=True, ProcessingTime=150ms, Retries=0]

// Mark as successful explicitly
successResult.MarkSuccess();

// Create a failed processing result
var failedResult = new ProcessingResult(2, false, "DataValidation")
{
    ProcessingTimeMs = 250,
    RetryCount = 2,
    CorrelationId = "corr-67890"
};
failedResult.MarkFailure("Validation failed: negative value detected", 
    new ArgumentException("Value must be positive"));

Console.WriteLine(failedResult.ErrorMessage);
Console.WriteLine(failedResult.Exception?.Message);

// Add output data dynamically
failedResult.AddOutput("validationErrors", new List<string> { "Negative value at index 5", "Invalid format" });
failedResult.AddOutput("rejectedItems", 15);

// Retrieve output data
var errors = failedResult.GetOutput("validationErrors") as List<string>;
var rejectedCount = failedResult.GetOutput("rejectedItems") as int?;

// Increment retry count
failedResult.IncrementRetryCount();

// Validate result
bool isValid = failedResult.IsValid();

// Clone with new ID
var clonedResult = failedResult.Clone(3);
Console.WriteLine(clonedResult.ResultId); // 3
```

## BackpressureContext

`BackpressureContext` tracks backpressure state within a pipeline stage, providing real-time metrics about buffer capacity, consumer activity, and backpressure events. It captures when backpressure begins, how long it persists, the number of dropped items, and current buffer utilization. This context is used by pipeline stages to make throttling decisions and by monitoring systems to visualize system health.

```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Create a backpressure context for a processing stage
var backpressureContext = new BackpressureContext(
    contextId: 1,
    pipelineStageName: "DataProcessing",
    bufferSize: 1000,
    maxBufferCapacity: 2000
);

Console.WriteLine($"Context ID: {backpressureContext.ContextId}");
Console.WriteLine($"Stage: {backpressureContext.PipelineStageName}");
Console.WriteLine($"Buffer Size: {backpressureContext.BufferSize}");
Console.WriteLine($"Max Capacity: {backpressureContext.MaxBufferCapacity}");
Console.WriteLine($"Created At: {backpressureContext.CreatedAt}");

// Add an item to the buffer
bool added = backpressureContext.TryAddToBuffer(42);
Console.WriteLine($"Item added: {added}");
Console.WriteLine($"Current fill: {backpressureContext.GetBufferFillPercentage()}%");

// Apply backpressure when buffer reaches 90%
if (backpressureContext.ShouldApplyBackpressure())
{
    backpressureContext.StartBackpressure();
    Console.WriteLine($"Backpressure started at: {backpressureContext.BackpressureStartTimeMs}");
    Console.WriteLine($"Is backpressured: {backpressureContext.IsBackpressured}");
}

// Remove an item from buffer
backpressureContext.RemoveFromBuffer(42);

// Record backpressure event
backpressureContext.RecordBackpressureEvent();
Console.WriteLine($"Total backpressure time: {backpressureContext.TotalBackpressureTimeMs}ms");
Console.WriteLine($"Dropped items: {backpressureContext.DroppedItemCount}");

// Track buffer metrics
backpressureContext.BufferMetrics["high_water_mark"] = 1800;
Console.WriteLine($"High water mark: {backpressureContext.BufferMetrics["high_water_mark"]}");

// Track active consumers
backpressureContext.ActiveConsumers = 3;
backpressureContext.MaxConcurrentConsumers = 5;
Console.WriteLine($"Consumers: {backpressureContext.ActiveConsumers}/{backpressureContext.MaxConcurrentConsumers}");

// Log event timestamps
backpressureContext.BackpressureEventTimestamps.Enqueue(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
Console.WriteLine($"Event timestamps recorded: {backpressureContext.BackpressureEventTimestamps.Count}");
```

## LoggingMiddleware

`LoggingMiddleware` provides comprehensive logging for pipeline operations, tracking data ingestion, processing completion, backpressure events, metrics collection, errors, and performance warnings. It supports correlation IDs for distributed tracing and provides both structured logging methods and performance measurement utilities.

```csharp
using DotNetRealtimePipeline.Middleware;
using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddSingleton<LoggingMiddleware>();
services.AddSingleton<PerformanceLoggingMiddleware>();
services.AddSingleton<CorrelationMiddleware>();
var provider = services.BuildServiceProvider();

// Get the logging middleware
var logger = provider.GetRequiredService<LoggingMiddleware>();

// Log data ingestion
var dataPoint = new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 23.5, "IoTSensor-001")
{
    Quality = 95
};
logger.LogDataIngestion(dataPoint, "DataIngestion");

// Log processing completion
var result = new ProcessingResult(1, true, "DataProcessing")
{
    ProcessingTimeMs = 150,
    CorrelationId = "corr-12345"
};
logger.LogProcessingCompletion(result, 150);

// Log backpressure event
var backpressureContext = new BackpressureContext(1, "DataProcessing", 920, 1000);
backpressureContext.StartBackpressure();
logger.LogBackpressureEvent("DataProcessing", backpressureContext);

// Log metrics collection
logger.LogMetricsCollection("throughput_eps", 1250, " items/s");

// Log error
try
{
    throw new InvalidOperationException("Data validation failed");
}
catch (Exception ex)
{
    logger.LogError("DataValidation", ex, "Temperature validation failed");
}

// Log performance warning
logger.LogPerformanceWarning("DataProcessing", 1500, 1000);

// Use PerformanceLoggingMiddleware for timing operations
var perfLogger = provider.GetRequiredService<PerformanceLoggingMiddleware>();

// Measure async operation
var asyncResult = await perfLogger.MeasureAsync<int>("ComplexDataProcessing", async () =>
{
    await Task.Delay(200);
    return 42;
});

// Measure sync operation
var syncResult = perfLogger.Measure("SimpleValidation", () => true);

// Use CorrelationMiddleware for distributed tracing
var correlation = provider.GetRequiredService<CorrelationMiddleware>();

// Get/set correlation ID
string correlationId = CorrelationMiddleware.GetCorrelationId();
Console.WriteLine($"Current correlation ID: {correlationId}");

CorrelationMiddleware.SetCorrelationId("custom-correlation-123");

// Execute operation with correlation context
await correlation.WithCorrelationAsync<string>(async (ctx) =>
{
    Console.WriteLine($"Operation correlation ID: {ctx}");
    return "processed";
});

CorrelationMiddleware.ClearCorrelationId();
```

## PipelineStateManager

`PipelineStateManager` manages pipeline state and lifecycle transitions. It tracks the current state, maintains a complete history of state transitions, and provides methods to query state duration and register change listeners. This class is essential for monitoring pipeline health and coordinating state-dependent operations.

The manager supports valid state transitions (Stopped → Running → Paused → Stopped, etc.) and automatically records each transition with timestamps and optional reasons. It also includes nested `ConfigurationStateManager` for runtime configuration overrides and `OperationMetricsTracker` for collecting performance metrics.

```csharp
using DotNetRealtimePipeline.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddSingleton<PipelineStateManager>();
services.AddSingleton<ConfigurationStateManager>();
services.AddSingleton<OperationMetricsTracker>();
var provider = services.BuildServiceProvider();

// Get the state manager
var stateManager = provider.GetRequiredService<PipelineStateManager>();

// Register a state change listener
stateManager.RegisterStateChangeListener((oldState, newState) =>
{
    Console.WriteLine($"State changed: {oldState} -> {newState}");
    Console.WriteLine($"Current state duration: {stateManager.GetCurrentStateDuration().TotalSeconds:F1}s");
});

// Start the pipeline
Console.WriteLine($"Initial state: {stateManager.CurrentState}");
bool started = stateManager.TransitionTo(PipelineState.Running, "Starting pipeline for data processing");
Console.WriteLine($"Transition to Running successful: {started}");
Console.WriteLine($"Current state: {stateManager.CurrentState}");

// Check if operational
if (stateManager.IsOperational)
{
    Console.WriteLine("Pipeline is operational and ready to process data");
}

// Simulate some work
System.Threading.Thread.Sleep(1000);

// Pause the pipeline for maintenance
stateManager.TransitionTo(PipelineState.Paused, "Maintenance window scheduled");
Console.WriteLine($"Current state: {stateManager.CurrentState}");
Console.WriteLine($"State duration: {stateManager.GetCurrentStateDuration().TotalSeconds:F1}s");

// Resume operation
stateManager.TransitionTo(PipelineState.Running, "Maintenance completed");

// Get state history
var history = stateManager.GetStateHistory();
Console.WriteLine($"\nState transition history ({history.Count} transitions):");
foreach (var transition in history)
{
    Console.WriteLine($"  {transition.Timestamp:HH:mm:ss} - {transition.FromState} -> {transition.ToState}");
    if (!string.IsNullOrEmpty(transition.Reason))
        Console.WriteLine($"    Reason: {transition.Reason}");
}

// Use ConfigurationStateManager for runtime overrides
var configManager = provider.GetRequiredService<ConfigurationStateManager>();
configManager.SetOverride("maxBufferSize", 5000);
configManager.SetOverride("enableCompression", true);

var maxBuffer = configManager.GetOverride<int>("maxBufferSize", 1000);
Console.WriteLine($"\nMax buffer size override: {maxBuffer}");

// Use OperationMetricsTracker to record operations
var metricsTracker = provider.GetRequiredService<OperationMetricsTracker>();
metricsTracker.RecordOperation("DataIngestion", 125, true);
metricsTracker.RecordOperation("DataProcessing", 45, true);
metricsTracker.RecordOperation("DataValidation", 89, false);

// Get operation metrics
var ingestionMetrics = metricsTracker.GetOperationMetrics("DataIngestion");
Console.WriteLine($"\nDataIngestion metrics:");
Console.WriteLine($"  Total executions: {ingestionMetrics.TotalExecutions}");
Console.WriteLine($"  Success rate: {ingestionMetrics.SuccessRate:F1}%");
Console.WriteLine($"  Average duration: {ingestionMetrics.AverageDurationMs:F1}ms");

// Stop the pipeline
stateManager.TransitionTo(PipelineState.Stopped, "Pipeline shutdown requested");
```

## DataProcessingService

`DataProcessingService` is the core service for processing data points through the pipeline. It handles validation, transformation, quality analysis, and persistence of data points. The service provides methods for processing individual data points or batches, analyzing data quality, filtering by quality thresholds, and retrieving processing statistics.

The service integrates with the pipeline's repositories and configuration to ensure consistent data processing according to configured thresholds and policies.

```csharp
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();

// Get the data processing service
var processingService = provider.GetRequiredService<DataProcessingService>();

// Process a single data point
var dataPoint = new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 23.5, "IoTSensor-001")
{
    Quality = 95,
    Metadata = new Dictionary<string, object>
    {
        { "sensorType", "temperature" },
        { "unit", "celsius" }
    }
};

var result = await processingService.ProcessDataPointAsync(dataPoint);
Console.WriteLine($"Processing result: Success={result.Success}, Time={result.ProcessingTimeMs}ms");

// Process a batch of data points
var batch = new List<DataPoint>
{
    new DataPoint(2, DateTimeOffset.UtcNow.AddSeconds(-10).ToUnixTimeMilliseconds(), 24.1, "IoTSensor-002") { Quality = 92 },
    new DataPoint(3, DateTimeOffset.UtcNow.AddSeconds(-9).ToUnixTimeMilliseconds(), 22.8, "IoTSensor-003") { Quality = 97 },
    new DataPoint(4, DateTimeOffset.UtcNow.AddSeconds(-8).ToUnixTimeMilliseconds(), 23.9, "IoTSensor-004") { Quality = 88 }
};

var batchResults = await processingService.ProcessBatchAsync(batch);
Console.WriteLine($"Processed {batchResults.Count} data points in batch");

// Analyze data quality
var qualityAnalysis = processingService.AnalyzeDataQuality(batch);
Console.WriteLine($"Quality analysis: Total={qualityAnalysis.TotalPoints}, " +
                 $"HighQuality={qualityAnalysis.HighQualityCount}, " +
                 $"PassRate={qualityAnalysis.PassRate:F1}%");

// Filter data points by quality threshold
var highQualityPoints = await processingService.FilterByQualityAsync(90);
Console.WriteLine($"High quality points (>=90): {highQualityPoints.Count}");

// Get processing statistics
var stats = await processingService.GetStatisticsAsync();
Console.WriteLine($"Total data points processed: {stats.TotalDataPoints}");
Console.WriteLine($"Configured max retries: {stats.ConfiguredMaxRetries}");
Console.WriteLine($"Quality threshold: {stats.QualityThreshold}%");
```

## PipelineInitializer

`PipelineInitializer` is responsible for initializing and configuring the real-time data pipeline. It validates configuration, sets up required services and repositories, and prepares the pipeline for operation. The initializer handles dependency injection setup, configuration validation, and provides detailed initialization results including success status, component initialization counts, and error messages.

```csharp
using DotNetRealtimePipeline.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddPipelineServices();

// Create and initialize the pipeline initializer
var initializer = new PipelineInitializer(services);

Console.WriteLine($"Initial state - Success: {initializer.Success}");
Console.WriteLine($"Components initialized: {initializer.ComponentsInitialized}");

// Initialize the pipeline asynchronously
var initializationResult = await initializer.InitializeAsync();

Console.WriteLine($"\nInitialization result:");
Console.WriteLine($" Success: {initializer.Success}");
Console.WriteLine($" Components initialized: {initializer.ComponentsInitialized}");
Console.WriteLine($" Error message: {initializer.ErrorMessage}");
Console.WriteLine($" Start time: {initializer.StartTime}");
Console.WriteLine($" End time: {initializer.EndTime}");

if (initializer.Success)
{
    Console.WriteLine("\nPipeline initialized successfully!");
    Console.WriteLine($"Configuration: {initializer}");
    
    // Start the pipeline
    bool started = await initializer.StartAsync();
    Console.WriteLine($"\nPipeline started: {started}");
    
    // Monitor initialization metrics
    Console.WriteLine($"\nInitialization metrics:");
    Console.WriteLine($" Duration: {(initializer.EndTime - initializer.StartTime).TotalMilliseconds}ms");
    Console.WriteLine($" Total components: {initializer.ComponentsInitialized}");
}
else
{
    Console.WriteLine($"\nInitialization failed: {initializer.ErrorMessage}");
}
```

## ThroughputCounter

`ThroughputCounter` tracks events-per-second throughput using a sliding time window with thread-safe operations. It provides both global throughput measurement and stage-specific metrics, making it ideal for monitoring pipeline performance and identifying bottlenecks.

```csharp
using DotNetRealtimePipeline.Metrics;

// Create a throughput counter with a 60-second sliding window
var throughputCounter = new ThroughputCounter(windowSeconds: 60);

// Record events globally
throughputCounter.RecordEvents(150);

// Record events for a specific pipeline stage
throughputCounter.RecordEvents("DataProcessing", 200);

// Get current throughput (events per second)
double globalThroughput = throughputCounter.GetThroughput();
Console.WriteLine($"Global throughput: {globalThroughput:F2} eps");

// Get throughput for a specific stage
double stageThroughput = throughputCounter.GetThroughput("DataProcessing");
Console.WriteLine($"DataProcessing stage throughput: {stageThroughput:F2} eps");
```

## PipelineOrchestrator

`PipelineOrchestrator` is the central orchestrator for the real-time data pipeline, managing the end-to-end data flow from ingestion through processing to query and monitoring. It coordinates pipeline stages, handles data ingestion, batch processing, and provides comprehensive observability through health monitoring, throughput tracking, and performance analysis. The orchestrator maintains pipeline state, manages backpressure scenarios, and exposes metrics for operational decision-making.

```csharp
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddPipelineServices();
var provider = services.BuildServiceProvider();

// Get the pipeline orchestrator
var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

// Start the pipeline
Console.WriteLine($"Initial state: {orchestrator.GetStatus()}");
bool started = await orchestrator.StartAsync();
Console.WriteLine($"Pipeline started: {started}");
Console.WriteLine($"Current state: {orchestrator.GetStatus()}");

// Ingest data points individually
bool ingestionSuccess1 = await orchestrator.IngestDataPointAsync(
    new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 23.5, "IoTSensor-001")
    { Quality = 95 }
);
Console.WriteLine($"Data point 1 ingested: {ingestionSuccess1}");

bool ingestionSuccess2 = await orchestrator.IngestDataPointAsync(
    new DataPoint(2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 24.1, "IoTSensor-002")
    { Quality = 92 }
);
Console.WriteLine($"Data point 2 ingested: {ingestionSuccess2}");

// Process a batch of data points
var batch = new List<DataPoint>
{
    new DataPoint(3, DateTimeOffset.UtcNow.AddSeconds(-1).ToUnixTimeMilliseconds(), 22.8, "IoTSensor-003") { Quality = 97 },
    new DataPoint(4, DateTimeOffset.UtcNow.AddSeconds(-2).ToUnixTimeMilliseconds(), 23.9, "IoTSensor-004") { Quality = 88 },
    new DataPoint(5, DateTimeOffset.UtcNow.AddSeconds(-3).ToUnixTimeMilliseconds(), 21.5, "IoTSensor-005") { Quality = 94 }
};

var batchResult = await orchestrator.ProcessBatchDataPointsAsync(batch);
Console.WriteLine($"Batch processed: Success={batchResult.Success}, ItemsProcessed={batchResult.ItemsProcessed}");

// Get query service for data retrieval
var queryService = orchestrator.GetQueryService();
var recentData = await queryService.SearchDataPointsAsync(
    startTime: DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds(),
    endTime: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
);
Console.WriteLine($"Found {recentData.Count} data points in last 5 minutes");

// Monitor pipeline health and performance
var healthReport = await orchestrator.GetHealthReportAsync();
Console.WriteLine($"Health status: {healthReport.Status}");
Console.WriteLine($"Throughput: {orchestrator.GetThroughput()} eps");
Console.WriteLine($"Total processed: {orchestrator.TotalDataPointsProcessed}");
Console.WriteLine($"Total failed: {orchestrator.TotalDataPointsFailed}");

// Get performance trend analysis
var performanceTrend = await orchestrator.GetPerformanceTrendAsync();
Console.WriteLine($"Performance trend: {performanceTrend.TrendDirection}");
Console.WriteLine($"Throughput change: {performanceTrend.ThroughputChangePercent:+0.00;-0.00}%");

// Monitor pipeline state and metrics
Console.WriteLine($"Pipeline running: {orchestrator.IsRunning}");
Console.WriteLine($"Pending items: {orchestrator.PendingItemsInQueue}");
Console.WriteLine($"Configuration: {orchestrator.ConfigurationName} v{orchestrator.ConfigurationVersion}");
Console.WriteLine($"Last updated: {orchestrator.Timestamp}");

// Stop the pipeline when done
bool stopped = await orchestrator.StopAsync();
Console.WriteLine($"Pipeline stopped: {stopped}");
```

## QueryService

`QueryService` provides querying and analysis capabilities for data points in the real-time pipeline. It offers methods for searching data points by various criteria, computing aggregated statistics, analyzing trends, and decomposing time series data. The service integrates with the pipeline's repositories to provide efficient data retrieval and comprehensive analytical operations.

The service returns strongly-typed results including `DataAggregateStatistics`, `TrendAnalysis`, and `TimeSeriesDecomposition` objects that contain detailed metrics and insights about the queried data.

```csharp
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();

// Get the query service
var queryService = provider.GetRequiredService<QueryService>();

// Search for data points by time range
var dataPoints = await queryService.SearchDataPointsAsync(
    startTime: DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds(),
    endTime: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
);
Console.WriteLine($"Found {dataPoints.Count} data points in the last hour");
```

## BackpressureService

`BackpressureService` manages backpressure and flow control within the pipeline to prevent buffer overflow and enforce graceful degradation under load. It creates and tracks backpressure contexts for each pipeline stage, monitors buffer utilization, and applies appropriate backpressure strategies when configured thresholds are exceeded. The service provides real-time metrics about system-wide backpressure status, dropped items, and consumer activity.

```csharp
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();

// Get the backpressure service
var backpressureService = provider.GetRequiredService<BackpressureService>();

// Create a backpressure context for a processing stage with 10,000 item capacity
var processingContext = backpressureService.CreateContext("DataProcessing", maxBufferCapacity: 10_000);

// Register a consumer to process items
bool consumerRegistered = backpressureService.TryRegisterConsumer("DataProcessing");
Console.WriteLine($"Consumer registered: {consumerRegistered}");

// Add items to the buffer (returns false if buffer is full)
bool added = backpressureService.TryAddToBuffer("DataProcessing", itemCount: 500);
Console.WriteLine($"Items added to buffer: {added}");
Console.WriteLine($"Current buffer fill: {processingContext.GetBufferFillPercentage()}%");

// Check if backpressure is active
bool isBackpressured = backpressureService.IsBackpressured("DataProcessing");
Console.WriteLine($"Is backpressured: {isBackpressured}");

// Apply backpressure strategy when buffer reaches critical level
var response = await backpressureService.ApplyBackpressureAsync(
    stageName: "DataProcessing",
    strategy: BackpressureStrategy.Throttle,
    timeoutMs: 200
);

Console.WriteLine($"Backpressure applied: {response.Applied}");
Console.WriteLine($"Reason: {response.Reason}");
Console.WriteLine($"Buffer fill: {response.BufferFillPercent}%");
Console.WriteLine($"Strategy used: {response.StrategyUsed}");

// Get system-wide backpressure status
var systemStatus = backpressureService.GetSystemStatus();
Console.WriteLine($"\nSystem status:");
Console.WriteLine($"  Total stages: {systemStatus.TotalStages}");
Console.WriteLine($"  Backpressured stages: {systemStatus.BackpressuredStages}");
Console.WriteLine($"  Average buffer fill: {systemStatus.AverageBufferFillPercent}%");
Console.WriteLine($"  Health: {systemStatus.GetHealthStatus()}");

// Get dropped item count (indicates data loss)
long droppedItems = backpressureService.GetDroppedItemCount("DataProcessing");
Console.WriteLine($"\nDropped items: {droppedItems}");

// Reset backpressure when load decreases
backpressureService.ResetBackpressure("DataProcessing");
Console.WriteLine("Backpressure reset");
```

## SlidingWindowAggregator

`SlidingWindowAggregator` implements sliding-window aggregation over a stream of `DataPoint` values. It emits aggregated results at regular intervals, producing overlapping windows that provide continuous analysis capabilities such as rolling averages, anomaly detection, and trend analysis. This is particularly useful for real-time monitoring scenarios where you need to analyze data trends over time without waiting for complete batches.
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();

// Get the query service
var queryService = provider.GetRequiredService<QueryService>();

// Search for data points by time range
var dataPoints = await queryService.SearchDataPointsAsync(
    startTime: DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds(),
    endTime: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
);
Console.WriteLine($"Found {dataPoints.Count} data points in the last hour");

// Get aggregated statistics for a time range
var stats = await queryService.GetAggregateStatisticsAsync(
    startMs: DateTimeOffset.UtcNow.AddHours(-24).ToUnixTimeMilliseconds(),
    endMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
);
Console.WriteLine($"Statistics: Count={stats.Count}, Avg={stats.Average:F2}, Min={stats.Min:F2}, Max={stats.Max:F2}");
Console.WriteLine($"Quality: Avg={stats.AverageQuality:F1}%, Sources={stats.UniqueSourceCount}");

// Analyze trends over time
var trendAnalysis = await queryService.AnalyzeTrendsAsync(
    startMs: DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeMilliseconds(),
    endMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    intervalMs: TimeSpan.FromHours(1).TotalMilliseconds
);
Console.WriteLine($"Trend: {trendAnalysis.Direction} ({trendAnalysis.ChangePercent:+0.00;-0.00}%)");

// Decompose time series to analyze trend and seasonality
var decomposition = await queryService.DecomposeTimeSeriesAsync(
    startMs: DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds(),
    endMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    movingAverageWindow: 7
);
Console.WriteLine($"Decomposition: Trend strength={decomposition.TrendStrength:F1}%, Seasonality={decomposition.SeasonalityStrength:F1}");

// Get recent metrics history
var recentMetrics = await queryService.GetRecentMetricsAsync(count: 20);
Console.WriteLine($"Found {recentMetrics.Count} recent metric aggregations");

// Get total data point count
var totalCount = await queryService.GetDataPointCountAsync();
Console.WriteLine($"Total data points in repository: {totalCount:N0}");
```

## SlidingWindowAggregator

`SlidingWindowAggregator` implements sliding-window aggregation over a stream of `DataPoint` values. It emits aggregated results at regular intervals, producing overlapping windows that provide continuous analysis capabilities such as rolling averages, anomaly detection, and trend analysis. This is particularly useful for real-time monitoring scenarios where you need to analyze data trends over time without waiting for complete batches.

The aggregator maintains a buffer of data points and emits new window results whenever the step interval is reached, covering the most recent window of data. Older data points are automatically pruned from the buffer to maintain memory efficiency.

```csharp
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Models;
using System;

// Create a sliding window aggregator with 10-second windows emitted every 2 seconds
var aggregator = new SlidingWindowAggregator(windowSizeMs: 10_000, stepIntervalMs: 2_000);

// Add data points to the aggregator (typically from your data source)
aggregator.Add(new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 23.5, "IoTSensor-001") { Quality = 95 });
aggregator.Add(new DataPoint(2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 24.1, "IoTSensor-002") { Quality = 92 });
aggregator.Add(new DataPoint(3, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 22.8, "IoTSensor-003") { Quality = 97 });

// Flush windows that are due for emission (using current time)
var windows = aggregator.FlushDueWindows();

foreach (var window in windows)
{
    Console.WriteLine($"Window {window.WindowId}:");
    Console.WriteLine($"  Time range: {window.WindowStartMs}ms to {window.WindowEnd}ms");
    Console.WriteLine($"  Data points: {window.DataPointCount}");
    Console.WriteLine($"  Average: {window.Average:F2}");
    Console.WriteLine($"  Sum: {window.Sum:F2}");
    Console.WriteLine($"  Min: {window.Min:F2}");
    Console.WriteLine($"  Max: {window.Max:F2}");
    Console.WriteLine($"  Trend: {window.Trend:+0.00;-0.00}");
    Console.WriteLine($"  Emitted at: {window.EmittedAt:HH:mm:ss.fff}");
    
    // Access aggregated data dictionary for additional metrics
    Console.WriteLine($"  Window type: {window.AggregatedData[\"WindowType\"]}");
    Console.WriteLine($"  Window size: {window.AggregatedData[\"WindowSizeMs\"]}ms");
    Console.WriteLine($"  Step interval: {window.AggregatedData[\"StepIntervalMs\"]}ms");
}

// Add more data points over time
for (int i = 0; i < 10; i++)
{
    aggregator.Add(new DataPoint(i + 10, 
        DateTimeOffset.UtcNow.AddSeconds(i).ToUnixTimeMilliseconds(), 
        20.0 + Math.Sin(i * 0.5) * 5.0, 
        "IoTSensor-001"));
    
    // Periodically flush windows
    var currentWindows = aggregator.FlushDueWindows();
    if (currentWindows.Count > 0)
    {
        Console.WriteLine($"Emitted {currentWindows.Count} new window(s) at step {currentWindows[^1].WindowId}");
    }
}
```

## MetricAggregation

`MetricAggregation` represents aggregated metrics for monitoring pipeline performance. It tracks throughput, latency, error rates, backpressure indicators, and processing statistics across time windows. This type is used throughout the pipeline to provide observability into pipeline health and performance characteristics.

```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Create a metric aggregation for an hourly window
var metrics = new MetricAggregation(
    metricId: 1,
    startMs: DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds(),
    endMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    metricType: "hourly"
);

// Record processing statistics
metrics.TotalItemsProcessed = 42875;
metrics.TotalItemsFailed = 124;
metrics.TotalItemsSkipped = 89;
metrics.AverageProcessingTimeMs = 45.2;
metrics.MinProcessingTimeMs = 5.1;
metrics.MaxProcessingTimeMs = 245.8;
metrics.P95ProcessingTimeMs = 120.5;
metrics.P99ProcessingTimeMs = 185.3;

// Record backpressure metrics
metrics.BackpressureEvents = 15;
metrics.TotalBackpressureMs = 12500;

// Record metrics by source
metrics.RecordSourceMetric("IoTSensor-001", 21500);
metrics.RecordSourceMetric("IoTSensor-002", 18300);
metrics.RecordSourceMetric("IoTSensor-003", 3075);

// Record error rates by stage
metrics.RecordStageErrorRate("DataIngestion", 0.8);
metrics.RecordStageErrorRate("DataProcessing", 2.1);
metrics.RecordStageErrorRate("DataValidation", 0.3);

// Calculate derived metrics
Console.WriteLine($"Throughput: {metrics.CalculateThroughput():F2} items/s");
Console.WriteLine($"Success Rate: {metrics.CalculateSuccessRate():F2}%");
Console.WriteLine($"Error Rate: {metrics.CalculateErrorRate():F2}%");
Console.WriteLine($"Backpressure Ratio: {metrics.CalculateBackpressureRatio():F2}%");

// Get summary for reporting
Console.WriteLine(metrics.GetSummary());
// Output: MetricAggregation[Type=hourly, Throughput=119.10 items/s, SuccessRate=99.44%, AvgLatency=45.20ms, P95=120.50ms, Backpressure=0.35%]
```

## BackpressureEvent

`BackpressureEvent` represents a time-stamped backpressure event captured by the pipeline's backpressure monitoring system. It records when backpressure is activated or released, the buffer fill percentage at that moment, and the number of dropped items. This type is used throughout the pipeline to track backpressure events for observability, alerting, and performance analysis.

```csharp
using DotNetRealtimePipeline.Metrics;
using System;

// Create a backpressure event when backpressure is activated
var activationEvent = new BackpressureEvent
{
    Timestamp = DateTime.UtcNow,
    StageName = "DataProcessing",
    BufferFillPercent = 92.5,
    IsActivation = true,
    DroppedItems = 15
};

Console.WriteLine($"Backpressure activated at {activationEvent.Timestamp:HH:mm:ss.fff}");
Console.WriteLine($"Stage: {activationEvent.StageName}");
Console.WriteLine($"Buffer fill: {activationEvent.BufferFillPercent}%");
Console.WriteLine($"Dropped items: {activationEvent.DroppedItems}");

// Create a backpressure event when backpressure is released
var releaseEvent = new BackpressureEvent
{
    Timestamp = DateTime.UtcNow.AddSeconds(30),
    StageName = "DataProcessing",
    BufferFillPercent = 45.2,
    IsActivation = false,
    DroppedItems = 15  // Same count as when activated
};

Console.WriteLine($"\nBackpressure released at {releaseEvent.Timestamp:HH:mm:ss.fff}");
Console.WriteLine($"Stage: {releaseEvent.StageName}");
Console.WriteLine($"Buffer fill: {releaseEvent.BufferFillPercent}%");
Console.WriteLine($"Dropped items: {releaseEvent.DroppedItems}");

// Track backpressure events in a collection
var backpressureEvents = new List<BackpressureEvent> { activationEvent, releaseEvent };
Console.WriteLine($"\nTotal backpressure events: {backpressureEvents.Count}");
Console.WriteLine($"Total dropped items: {backpressureEvents.Sum(e => e.DroppedItems)}");
```

## BackpressureMetricsCollector

`BackpressureMetricsCollector` collects and tracks backpressure-related metrics for a specific pipeline stage. It monitors buffer utilization, activation patterns, and backpressure events to provide comprehensive observability into system health and performance under load. The collector maintains historical data about backpressure events, buffer fill levels, and consumer activity, enabling analysis of system behavior during periods of high load.

```csharp
using DotNetRealtimePipeline.Metrics;
using Microsoft.Extensions.DependencyInjection;
using System;

// Setup dependency injection
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();

// Get the backpressure metrics collector for a specific stage
var collector = new BackpressureMetricsCollector("DataProcessing");

// Poll for current metrics
collector.Poll();

// Manually record a backpressure event
collector.RecordManualEvent("Buffer capacity exceeded during peak load");

// Get stage-specific backpressure metrics
var stageMetrics = collector.GetStageMetrics();
if (stageMetrics != null)
{
    Console.WriteLine($"Stage: {stageMetrics.StageName}");
    Console.WriteLine($"Backpressured: {stageMetrics.WasBackpressured}");
    Console.WriteLine($"Peak buffer fill: {stageMetrics.PeakBufferFillPercent}%");
    Console.WriteLine($"Total dropped items: {stageMetrics.TotalDroppedItems}");
}

// Get a complete snapshot of backpressure metrics
var snapshot = collector.GetSnapshot();
Console.WriteLine($"Current buffer fill: {snapshot.CurrentBufferFillPercent}%");
Console.WriteLine($"Activation count: {snapshot.ActivationCount}");
Console.WriteLine($"Total active duration: {snapshot.TotalActiveDurationMs}ms");
Console.WriteLine($"Last activation: {snapshot.LastActivationAt}");

// Get recent backpressure events
var recentEvents = collector.GetRecentEvents();
Console.WriteLine($"Recent events: {recentEvents.Count}");

// Get stage-specific backpressure events
var stageEvents = collector.GetStageEvents();
Console.WriteLine($"Stage events: {stageEvents.Count}");

// Reset metrics when backpressure situation resolves
collector.Reset();

// Access public properties
Console.WriteLine($"Stage name: {collector.StageName}");
Console.WriteLine($"Was backpressured: {collector.WasBackpressured}");
Console.WriteLine($"Peak buffer fill: {collector.PeakBufferFillPercent}%");
Console.WriteLine($"Current buffer fill: {collector.CurrentBufferFillPercent}%");
Console.WriteLine($"Total dropped items: {collector.TotalDroppedItems}");
```

## MetricsService

`MetricsService` collects, aggregates, and analyzes pipeline metrics. It tracks throughput, latency, error rates, and backpressure across pipeline stages, providing real-time monitoring and historical analysis capabilities. The service maintains sliding window metrics for recent performance and generates health reports, trend analysis, and distribution statistics.

```csharp
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();

// Get the metrics service
var metricsService = provider.GetRequiredService<MetricsService>();

// Record throughput for processing 1000 events
metricsService.RecordThroughput(1000);

// Record processing time for an operation
metricsService.RecordProcessingTime(45);

// Create a metric aggregation for the last minute
var aggregation = await metricsService.CreateMetricAggregationAsync(
    windowStartMs: DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeMilliseconds(),
    windowEndMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    itemsProcessed: 58472,
    itemsFailed: 124,
    itemsSkipped: 89
);

Console.WriteLine($"Metric ID: {aggregation.MetricId}");
Console.WriteLine($"Throughput: {aggregation.CalculateThroughput():F2} items/s");
Console.WriteLine($"Success Rate: {aggregation.CalculateSuccessRate():F2}%");

// Generate a health report
var healthReport = await metricsService.GenerateHealthReportAsync();
Console.WriteLine($"Status: {healthReport.Status}");
Console.WriteLine($"Message: {healthReport.Message}");
Console.WriteLine($"Throughput: {healthReport.ThroughputItemsPerSecond:F2} items/s");
Console.WriteLine($"Success Rate: {healthReport.SuccessRatePercent:F2}%");

// Analyze performance trend over last 10 aggregations
var trend = await metricsService.AnalyzePerformanceTrendAsync(historyCount: 10);
Console.WriteLine($"Trend: {trend.TrendDirection}");
Console.WriteLine($"Throughput Change: {trend.ThroughputChangePercent:+0.00;-0.00}%");

// Get metric distribution across sources
var distribution = await metricsService.GetMetricDistributionAsync();
Console.WriteLine($"Total Sources: {distribution.TotalSources}");
Console.WriteLine($"Source Breakdown: {string.Join(", ", distribution.SourceBreakdown.Select(kvp => $"{kvp.Key}={kvp.Value}")}");
```

```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Create a metric aggregation for an hourly window
var metrics = new MetricAggregation(
    metricId: 1,
    startMs: DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds(),
    endMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    metricType: "hourly"
);

// Record processing statistics
metrics.TotalItemsProcessed = 42875;
metrics.TotalItemsFailed = 124;
metrics.TotalItemsSkipped = 89;
metrics.AverageProcessingTimeMs = 45.2;
metrics.MinProcessingTimeMs = 5.1;
metrics.MaxProcessingTimeMs = 245.8;
metrics.P95ProcessingTimeMs = 120.5;
metrics.P99ProcessingTimeMs = 185.3;

// Record backpressure metrics
metrics.BackpressureEvents = 15;
metrics.TotalBackpressureMs = 12500;

// Record metrics by source
metrics.RecordSourceMetric("IoTSensor-001", 21500);
metrics.RecordSourceMetric("IoTSensor-002", 18300);
metrics.RecordSourceMetric("IoTSensor-003", 3075);

// Record error rates by stage
metrics.RecordStageErrorRate("DataIngestion", 0.8);
metrics.RecordStageErrorRate("DataProcessing", 2.1);
metrics.RecordStageErrorRate("DataValidation", 0.3);

// Calculate derived metrics
Console.WriteLine($"Throughput: {metrics.CalculateThroughput():F2} items/s");
Console.WriteLine($"Success Rate: {metrics.CalculateSuccessRate():F2}%");
Console.WriteLine($"Error Rate: {metrics.CalculateErrorRate():F2}%");
Console.WriteLine($"Backpressure Ratio: {metrics.CalculateBackpressureRatio():F2}%");

// Get summary for reporting
Console.WriteLine(metrics.GetSummary());
// Output: MetricAggregation[Type=hourly, Throughput=119.10 items/s, SuccessRate=99.44%, AvgLatency=45.20ms, P95=120.50ms, Backpressure=0.35%]
```

## PipelineConfig

`PipelineConfig` defines the configuration for a real-time data pipeline, controlling buffer sizes, concurrency limits, retry policies, windowing behavior, data quality thresholds, and backpressure thresholds. It serves as the central configuration object that pipeline stages use to coordinate their behavior and provides extensibility through custom settings for domain-specific pipeline requirements.

```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Create a pipeline configuration for a high-throughput data processing pipeline
var config = new PipelineConfig
{
    PipelineName = "IoTDataProcessingPipeline",
    Version = "2.1.0",
    MaxBufferSize = 10_000,
    BufferFlushIntervalMs = 500,
    MaxConcurrentConsumers = 8,
    WindowSizeMs = 5000,
    WindowSlideMs = 1000,
    WindowType = "tumbling",
    MaxRetries = 3,
    RetryDelayMs = 100,
    ProcessingTimeoutMs = 10_000,
    BackpressureTriggerThreshold = 0.90,
    MinDataQualityThreshold = 85,
    ValidateOnIngestion = true,
    EnableMetricsCollection = true,
    Stages = new List<PipelineStageDef>
    {
        new PipelineStageDef { StageName = "DataIngestion", StageType = "SOURCE" },
        new PipelineStageDef { StageName = "DataProcessing", StageType = "TRANSFORM" },
        new PipelineStageDef { StageName = "DataValidation", StageType = "VALIDATION" },
        new PipelineStageDef { StageName = "WindowingService", StageType = "WINDOW" }
    },
    CustomSettings = new Dictionary<string, object>
    {
        { "enableCompression", true },
        { "compressionThreshold", 1024 },
        { "enableMonitoring", true },
        { "monitoringSamplingRate", 0.1 }
    }
};

Console.WriteLine($"Pipeline: {config.PipelineName} v{config.Version}");
Console.WriteLine($"Buffer: {config.MaxBufferSize} items, flush every {config.BufferFlushIntervalMs}ms");
Console.WriteLine($"Concurrency: up to {config.MaxConcurrentConsumers} consumers");
Console.WriteLine($"Window: {config.WindowType} {config.WindowSizeMs}ms/{config.WindowSlideMs}ms");
Console.WriteLine($"Retries: max {config.MaxRetries} with {config.RetryDelayMs}ms delay");
Console.WriteLine($"Backpressure threshold: {config.BackpressureTriggerThreshold * 100}%");
Console.WriteLine($"Data quality threshold: {config.MinDataQualityThreshold}%");
Console.WriteLine($"Metrics enabled: {config.EnableMetricsCollection}");
```

```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;

// Create a minimal configuration for a development pipeline
var devConfig = new PipelineConfig
{
    PipelineName = "DevPipeline",
    Version = "1.0.0-dev",
    MaxBufferSize = 1000,
    MaxConcurrentConsumers = 2,
    WindowSizeMs = 1000,
    WindowType = "sliding",
    MaxRetries = 2,
    ProcessingTimeoutMs = 5000,
    BackpressureTriggerThreshold = 0.85,
    MinDataQualityThreshold = 70,
    ValidateOnIngestion = false,
    EnableMetricsCollection = false,
    Stages = new List<PipelineStageDef>
    {
        new PipelineStageDef { StageName = "Ingest", StageType = "SOURCE" },
        new PipelineStageDef { StageName = "Process", StageType = "TRANSFORM" }
    }
};

Console.WriteLine($"Dev pipeline created: {devConfig.PipelineName}");
```

## DataPoint

`DataPoint` represents a single data point in the pipeline stream. It serves as the core entity for processing, carrying essential information such as a unique identifier, timestamp, measured value, source system, and quality metrics. Data points include metadata for extensibility and tags for categorization, making them suitable for tracking real-time measurements across various sources.

```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Create a data point for temperature measurement from an IoT sensor
var dataPoint = new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 23.5, "IoTSensor-001")
{
    Quality = 95,
    Tags = "temperature,environmental,sensor-001",
    Metadata = new Dictionary<string, object>
    {
        { "sensorType", "temperature" },
        { "unit", "celsius" },
        { "location", "room-101" },
        { "calibrationDate", DateTime.UtcNow.AddDays(-7) }
    }
};

Console.WriteLine($"DataPoint ID: {dataPoint.Id}");
Console.WriteLine($"Timestamp: {dataPoint.Timestamp}");
Console.WriteLine($"Value: {dataPoint.Value}");
Console.WriteLine($"Source: {dataPoint.Source}");
Console.WriteLine($"Quality: {dataPoint.Quality}");
Console.WriteLine($"Age: {dataPoint.GetAgeMs()}ms");
Console.WriteLine($"Tags: {dataPoint.Tags}");

// Add additional metadata dynamically
var metadata = new Dictionary<string, object>
{
    { "deviceId", "sensor-001" },
    { "firmwareVersion", "2.1.4" },
    { "lastCalibration", DateTime.UtcNow.AddDays(-3) }
};

foreach (var kvp in metadata)
{
    dataPoint.AddMetadata(kvp.Key, kvp.Value);
}

// Check quality threshold
bool meetsThreshold = dataPoint.MeetsQualityThreshold(90);
Console.WriteLine($"Meets quality threshold 90%: {meetsThreshold}");

// Validate data point
bool isValid = dataPoint.Validate();
Console.WriteLine($"Is valid: {isValid}");

// Clone with new ID for processing
var clonedPoint = dataPoint.Clone(2);
Console.WriteLine($"Cloned data point ID: {clonedPoint.Id}");
Console.WriteLine($"Cloned value: {clonedPoint.Value}");
```

## WindowingService

`WindowingService` is the core service for managing time-based windows of data points in the real-time pipeline. It handles window creation, data aggregation, window emission, and provides comprehensive statistics about windowed data processing. The service supports both tumbling and sliding windows, enabling continuous analysis of data trends over configurable time intervals.

The service provides methods for adding data points to windows, processing batches of data points, calculating window statistics, and emitting completed windows for downstream processing. It tracks window state, completion status, and provides detailed metrics about window throughput and performance.

```csharp
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

// Setup dependency injection
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();

// Get the windowing service
var windowingService = provider.GetRequiredService<WindowingService>();

// Create a new window for processing
var windowEvent = windowingService.CreateWindow(
    windowType: "tumbling",
    windowSizeMs: 5000,
    slideIntervalMs: 5000
);

Console.WriteLine($"Created window: {windowEvent.WindowId}");

// Add data points to the window
bool added1 = windowingService.TryAddDataPointToWindow(
    windowId: windowEvent.WindowId,
    dataPoint: new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 23.5, "IoTSensor-001") { Quality = 95 }
);

bool added2 = windowingService.TryAddDataPointToWindow(
    windowId: windowEvent.WindowId,
    dataPoint: new DataPoint(2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 24.1, "IoTSensor-002") { Quality = 92 }
);

Console.WriteLine($"Data points added: {windowingService.DataPointCount} (added1={added1}, added2={added2})");

// Process data points through the windowing service
var results = windowingService.ProcessDataPoints(
    new List<DataPoint> {
        new DataPoint(3, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 22.8, "IoTSensor-003") { Quality = 97 },
        new DataPoint(4, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 21.5, "IoTSensor-004") { Quality = 94 }
    }
);

Console.WriteLine($"Processed {results.Count} window emissions");

// Aggregate window data
var aggregatedData = windowingService.AggregateWindow(windowEvent.WindowId);
Console.WriteLine($"Window aggregated: Count={aggregatedData["DataPointCount"]}, Sum={aggregatedData["Sum"]}, Avg={aggregatedData["Average"]}");

// Calculate window statistics
var stats = windowingService.CalculateWindowStatistics(windowEvent.WindowId);
Console.WriteLine($"Window statistics: Min={stats.Min:F2}, Max={stats.Max:F2}, StdDev={stats.StdDev:F2}");

// Check if window is complete
bool isComplete = windowingService.IsWindowComplete(windowEvent.WindowId);
Console.WriteLine($"Window complete: {isComplete}");

// Emit the completed window
var emissionResult = windowingService.EmitWindow(windowEvent.WindowId);
Console.WriteLine($"Window emitted: WindowId={emissionResult.WindowId}, Type={emissionResult.WindowType}");

// Get windowing summary
var summary = windowingService.GetWindowingSummary();
Console.WriteLine($"\nWindowing summary:");
Console.WriteLine($"  Total windows: {summary.TotalWindows}");
Console.WriteLine($"  Active windows: {summary.ActiveWindows}");
Console.WriteLine($"  Completed windows: {summary.CompletedWindows}");
Console.WriteLine($"  Total data points: {summary.TotalDataPoints}");
Console.WriteLine($"  Average throughput: {summary.AverageThroughput:F2} eps");
```

## WindowEvent

`WindowEvent` represents a time-bounded aggregation of data points collected during a specific interval. It tracks window boundaries, aggregation type, and provides statistical calculations over the contained data points. This type is used throughout the pipeline's windowing service to manage tumbling/sliding windows and produce aggregated outputs.

```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

// Create a tumbling window for 5-second intervals
var window = new WindowEvent(1, 1715616000000, 1715616005000, "tumbling")
{
    Description = "Temperature readings from IoT sensors"
};

// Add data points within the window
window.TryAddDataPoint(new DataPoint(1, 1715616001000, 23.5, "IoTSensor-001") { Quality = 95 });
window.TryAddDataPoint(new DataPoint(2, 1715616002000, 24.1, "IoTSensor-002") { Quality = 92 });
window.TryAddDataPoint(new DataPoint(3, 1715616003000, 22.8, "IoTSensor-003") { Quality = 97 });

// Calculate window statistics
Console.WriteLine($"Window duration: {window.GetDurationMs()}ms");
Console.WriteLine($"Data points: {window.GetDataPointCount()}");
Console.WriteLine($"Average: {window.CalculateAverage():F2}");
Console.WriteLine($"Sum: {window.CalculateSum():F2}");
Console.WriteLine($"Min: {window.CalculateMin():F2}");
Console.WriteLine($"Max: {window.CalculateMax():F2}");
Console.WriteLine($"Standard deviation: {window.CalculateStandardDeviation():F2}");

// Mark window as complete for output
window.MarkComplete();

// Retrieve metadata for reporting
var metadata = window.GetMetadata();
foreach (var kvp in metadata)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
```

## IPipelinePlugin

`IPipelinePlugin` defines the contract for pipeline plugins that can be loaded dynamically to extend pipeline functionality. Plugins can provide data processing, transformation, and output capabilities, and are managed through a `PluginManager`. Plugins support initialization and shutdown hooks, configuration management, and dependency declaration for proper loading order.

```csharp
using DotNetRealtimePipeline.Plugins;
using DotNetRealtimePipeline.Plugins.ExtensionSystem;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

// Create and register a custom pipeline plugin
public class CustomDataPlugin : IPipelinePlugin
{
    public string Name => "CustomDataPlugin";
    public string Version => "1.0.0";
    public bool Enabled { get; set; } = true;
    public Dictionary<string, object> Settings { get; set; } = new();
    public List<string> Dependencies { get; } = new();

    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine($"Initializing {Name} v{Version}");
        // Initialize plugin dependencies
    }

    public async Task ShutdownAsync()
    {
        Console.WriteLine($"Shutting down {Name} v{Version}");
        // Cleanup resources
    }

    public List<IDataProcessingPlugin> GetProcessingPlugins() => new();
    public List<IDataTransformPlugin> GetTransformPlugins() => new();
    public List<IOutputPlugin> GetOutputPlugins() => new();
}

// Setup dependency injection and register the plugin
var services = new ServiceCollection();
services.AddPipelineServices();
services.AddSingleton<IPipelinePlugin, CustomDataPlugin>();

var provider = services.BuildServiceProvider();
var pluginManager = provider.GetRequiredService<PluginManager>();

// Register plugin configuration
pluginManager.RegisterConfiguration<CustomDataPlugin>(config =>
{
    config.Enabled = true;
    config.Settings["threshold"] = 0.95;
    config.Settings["batchSize"] = 100;
});

// Access registered plugins
var allPlugins = pluginManager.GetAllPlugins();
var processingPlugins = pluginManager.GetProcessingPlugins();
var transformPlugins = pluginManager.GetTransformPlugins();
var outputPlugins = pluginManager.GetOutputPlugins();

Console.WriteLine($"Registered {allPlugins.Count} plugins");
```

## ScalingDecision

`ScalingDecision` represents an auto-scaling directive issued by the pipeline's consumer scaling service. It captures the scaling intent (up or down), the rationale, current and target consumer counts, buffer state, backpressure frequency, and timing metadata. Pipeline stages use these decisions to adjust parallelism in response to load changes, ensuring throughput while preventing resource exhaustion.

```csharp
using DotNetRealtimePipeline.Domain.Models;
using System;

// Create a scaling decision to increase consumers due to high backpressure
var scaleUpDecision = new ScalingDecision(
    stageName: "DataProcessing",
    direction: ScalingDirection.ScaleUp,
    reason: "High backpressure detected with buffer at 92% capacity",
    fromConsumers: 4,
    toConsumers: 8,
    bufferFillPercent: 92.3,
    backpressureFrequency: 15.2
);

Console.WriteLine($"Decision for {scaleUpDecision.StageName}: {scaleUpDecision.Direction}");
Console.WriteLine($"Reason: {scaleUpDecision.Reason}");
Console.WriteLine($"From {scaleUpDecision.FromConsumers} to {scaleUpDecision.ToConsumers} consumers");
Console.WriteLine($"Buffer: {scaleUpDecision.BufferFillPercent}%, Backpressure: {scaleUpDecision.BackpressureFrequency}/s");
Console.WriteLine($"Decided at: {scaleUpDecision.DecidedAt}");

// Create a scaling decision to decrease consumers due to low load
var scaleDownDecision = new ScalingDecision(
    stageName: "DataValidation",
    direction: ScalingDirection.ScaleDown,
    reason: "Load decreased, buffer utilization below threshold",
    fromConsumers: 6,
    toConsumers: 3,
    bufferFillPercent: 25.1,
    backpressureFrequency: 0.8
);

Console.WriteLine($"\nDecision for {scaleDownDecision.StageName}: {scaleDownDecision.Direction}");
Console.WriteLine($"Reason: {scaleDownDecision.Reason}");
Console.WriteLine($"From {scaleDownDecision.FromConsumers} to {scaleDownDecision.ToConsumers} consumers");

// Track scaling history
var previousDecision = scaleUpDecision;
var nextDecision = new ScalingDecision(
    stageName: "DataProcessing",
    direction: ScalingDirection.ScaleUp,
    reason: "Sustained high throughput requiring additional capacity",
    fromConsumers: 8,
    toConsumers: 12,
    bufferFillPercent: 88.7,
    backpressureFrequency: 18.5,
    lastDecision: previousDecision,
    lastScalingActionAt: DateTime.UtcNow.AddMinutes(-5)
);

Console.WriteLine($"\nPrevious decision: {previousDecision.ToConsumers} consumers");
Console.WriteLine($"Current decision: {nextDecision.ToConsumers} consumers");
Console.WriteLine($"Scale-up count: {nextDecision.ScaleUpCount}");
Console.WriteLine($"Scale-down count: {nextDecision.ScaleDownCount}");
```

## ErrorHandlingMiddleware

`ErrorHandlingMiddleware` provides centralized error handling and exception transformation for pipeline operations. It converts exceptions to standardized error responses with proper logging and recovery information, enabling consistent error handling across the pipeline.

The middleware supports both synchronous and asynchronous operations, automatically mapping known exception types to appropriate error codes and messages. It distinguishes between recoverable and non-recoverable errors, providing appropriate logging levels for observability.

```csharp
using DotNetRealtimePipeline.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddSingleton<ErrorHandlingMiddleware>();
var provider = services.BuildServiceProvider();

// Get the error handling middleware
var errorHandler = provider.GetRequiredService<ErrorHandlingMiddleware>();

// Execute an operation with error handling (async)
var result = await errorHandler.ExecuteWithErrorHandlingAsync<int>(
    "DataProcessingOperation",
    async () =>
    {
        // Simulate successful operation
        await Task.Delay(100);
        return 42;
    }
);

if (result.Success)
{
    Console.WriteLine($"Success! Data: {result.Data}");
}
else
{
    Console.WriteLine($"Error {result.ErrorCode}: {result.Message}");
    Console.WriteLine($"Recoverable: {result.IsRecoverable}");
    Console.WriteLine($"Details: {result.Details}");
}

// Execute a synchronous operation with error handling
var syncResult = errorHandler.ExecuteWithErrorHandling(
    "ValidationOperation",
    () =>
    {
        // Simulate validation that throws
        if (DateTime.Now.Second % 2 == 0)
            throw new InvalidOperationException("Validation failed");
        return true;
    }
);

Console.WriteLine($"Success: {syncResult.Success}, ErrorCode: {syncResult.ErrorCode}");
```

## RateLimitingMiddleware

`RateLimitingMiddleware` provides token bucket-based rate limiting for controlling the throughput of operations within the pipeline. It supports flexible rate control with configurable tokens per second and burst capacity, making it suitable for managing API calls, data ingestion rates, and processing throughput.

The middleware maintains separate rate limit buckets for different identifiers (e.g., API keys, client IDs, or pipeline stages), allowing for granular control over resource consumption. Each bucket automatically refills tokens over time based on the configured rate, preventing resource exhaustion while allowing bursts up to the maximum capacity.

```csharp
using DotNetRealtimePipeline.Middleware;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddSingleton<RateLimitingMiddleware>();
services.AddSingleton<StageRateLimitingMiddleware>();
var provider = services.BuildServiceProvider();

// Get the rate limiting middleware
var rateLimiter = provider.GetRequiredService<RateLimitingMiddleware>();

// Configure rate limits: 1000 tokens per second with 5000 burst capacity
var limiter = new RateLimitingMiddleware(tokensPerSecond: 1000, maxBurstSize: 5000);

// Check if an operation is allowed (consume 1 token)
bool allowed = limiter.TryAcquire("api-client-123");
Console.WriteLine($"API call allowed: {allowed}");

// Check rate limit status for a client
var status = limiter.GetStatus("api-client-123");
Console.WriteLine($"Available tokens: {status.AvailableTokens}/{status.Capacity}");
Console.WriteLine($"Reset time: {status.ResetTime}");

// Consume multiple tokens for batch operations
bool batchAllowed = limiter.TryAcquire("data-ingestion", tokensRequired: 10);
Console.WriteLine($"Batch ingestion allowed: {batchAllowed}");

// Reset rate limits for a client
limiter.Reset("api-client-123");

// Get all rate limit statuses
var allStatuses = limiter.GetAllStatuses();
foreach (var kvp in allStatuses)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value.AvailableTokens}/{kvp.Value.Capacity} tokens");
}

// Use StageRateLimitingMiddleware for pipeline stage-specific limits
var stageLimiter = provider.GetRequiredService<StageRateLimitingMiddleware>();
stageLimiter.RegisterStageLimit("DataIngestion", itemsPerSecond: 500, burstSize: 2000);

// Check if a stage can process items
bool canProcess = stageLimiter.CanProcessInStage("DataIngestion", itemCount: 1);
Console.WriteLine($"Stage can process: {canProcess}");

// Get status for all stages
var stageStatuses = stageLimiter.GetStageLimitStatuses();
foreach (var kvp in stageStatuses)
{
    Console.WriteLine($"Stage {kvp.Key}: {kvp.Value.AvailableTokens} available");
}
```

```csharp
using DotNetRealtimePipeline.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddSingleton<ErrorHandlingMiddleware>();
var provider = services.BuildServiceProvider();

// Get the error handling middleware
var errorHandler = provider.GetRequiredService<ErrorHandlingMiddleware>();

// Execute an operation with error handling (async)
var result = await errorHandler.ExecuteWithErrorHandlingAsync<int>(
    "DataProcessingOperation",
    async () => 
    {
        // Simulate successful operation
        await Task.Delay(100);
        return 42;
    }
);

if (result.Success)
{
    Console.WriteLine($"Success! Data: {result.Data}");
}
else
{
    Console.WriteLine($"Error {result.ErrorCode}: {result.Message}");
    Console.WriteLine($"Recoverable: {result.IsRecoverable}");
    Console.WriteLine($"Details: {result.Details}");
}

// Execute a synchronous operation with error handling
var syncResult = errorHandler.ExecuteWithErrorHandling(
    "ValidationOperation",
    () => 
    {
        // Simulate validation that throws
        if (DateTime.Now.Second % 2 == 0)
            throw new InvalidOperationException("Validation failed");
        return true;
    }
);

Console.WriteLine($"Success: {syncResult.Success}, ErrorCode: {syncResult.ErrorCode}");
```

## CommandLineParser

`CommandLineParser` is a command-line argument parsing utility that provides a structured way to define and parse command-line interfaces for .NET applications. It supports registering commands with verbs, defining required and optional options, and validating command-line input before execution. The parser handles command registration, parsing, and provides utilities for checking option presence, retrieving values, and validating required arguments.

```csharp
using DotNetRealtimePipeline.CLI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Create a command line parser
var parser = new CommandLineParser();

// Register a command with a verb
parser.RegisterCommand("ingest", "Ingest data from external sources into the pipeline");

// Define required options for the ingest command
parser.RequiredOptions["ingest"] = new List<string> { "source", "format" };

// Define optional options
parser.Options["ingest"] = new Dictionary<string, string>
{
    { "source", "The data source to ingest from" },
    { "format", "The format of the data (json, csv, parquet)" },
    { "batch-size", "The batch size for ingestion (default: 1000)" },
    { "verbose", "Enable verbose logging" }
};

// Register another command
parser.RegisterCommand("query", "Query data from the pipeline");
parser.RequiredOptions["query"] = new List<string> { "sql" };
parser.Options["query"] = new Dictionary<string, string>
{
    { "sql", "SQL query to execute" },
    { "output", "Output format (json, csv)" },
    { "limit", "Maximum number of results to return" }
};

// Parse command line arguments
var args = new[] { "ingest", "--source", "api-endpoint", "--format", "json", "--batch-size", "5000", "--verbose" };
var parsedCommand = parser.Parse(args);

// Check if parsing was successful
if (!parsedCommand.IsValid)
{
    Console.WriteLine($"Error: {parsedCommand.ErrorMessage}");
    return;
}

// Get the verb (command name)
string verb = parsedCommand.Verb;
Console.WriteLine($"Executing command: {verb}");

// Get option values
string source = parsedCommand.GetOption("source");
string format = parsedCommand.GetOption("format");
int batchSize = int.Parse(parsedCommand.GetOption("batch-size") ?? "1000");
bool verbose = parsedCommand.HasFlag("verbose");

Console.WriteLine($"Source: {source}");
Console.WriteLine($"Format: {format}");
Console.WriteLine($"Batch size: {batchSize}");
Console.WriteLine($"Verbose: {verbose}");

// Check if required options are present
if (parsedCommand.RequiredOptions.Contains("source") && !parsedCommand.HasFlag("source"))
{
    Console.WriteLine("Error: --source is required");
    return;
}

// Simulate command execution
Console.WriteLine($"Ingesting data from {source} in {format} format...");
```

## ExportService

`ExportService` handles the export of pipeline data to various file formats including CSV, JSON, and Parquet. It provides methods for exporting data points, results, metrics, and multi-format outputs, with support for batch processing and detailed export tracking through comprehensive result objects. The service tracks export success/failure status, file paths, record counts, file sizes, and timing information.

```csharp
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// Setup dependency injection
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();

// Get the export service
var exportService = provider.GetRequiredService<ExportService>();

// Export data points to CSV format
var exportResult = await exportService.ExportDataPointsAsync(
    dataPoints: new List<DataPoint>
    {
        new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 23.5, "IoTSensor-001") { Quality = 95 },
        new DataPoint(2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 24.1, "IoTSensor-002") { Quality = 92 },
        new DataPoint(3, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 22.8, "IoTSensor-003") { Quality = 97 }
    },
    outputPath: Path.Combine(Directory.GetCurrentDirectory(), "exports", "data-points.csv"),
    format: "csv"
);

Console.WriteLine($"Export successful: {exportResult.Success}");
Console.WriteLine($"Output path: {exportResult.OutputPath}");
Console.WriteLine($"Records exported: {exportResult.RecordCount}");
Console.WriteLine($"File size: {exportResult.FileSizeBytes} bytes");
Console.WriteLine($"Error message: {exportResult.ErrorMessage}");
Console.WriteLine($"Export duration: {(exportResult.EndTime - exportResult.StartTime).TotalMilliseconds}ms");

// Export results to JSON format
var results = new List<ProcessingResult>
{
    new ProcessingResult(1, true, "DataProcessing") { ProcessingTimeMs = 150 },
    new ProcessingResult(2, true, "DataProcessing") { ProcessingTimeMs = 120 }
};

var resultsExport = await exportService.ExportResultsAsync(
    results: results,
    outputPath: Path.Combine(Directory.GetCurrentDirectory(), "exports", "results.json"),
    format: "json"
);

Console.WriteLine($"\nResults export: Success={resultsExport.Success}, Records={resultsExport.RecordCount}");

// Export metrics to CSV format
var metricsExport = await exportService.ExportMetricsAsync(
    metrics: new List<MetricAggregation>
    {
        new MetricAggregation(1, DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeMilliseconds(), 
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "minute")
        {
            TotalItemsProcessed = 1500,
            TotalItemsFailed = 5,
            AverageProcessingTimeMs = 45.2
        }
    },
    outputPath: Path.Combine(Directory.GetCurrentDirectory(), "exports", "metrics.csv"),
    format: "csv"
);

Console.WriteLine($"\nMetrics export: Success={metricsExport.Success}, Records={metricsExport.RecordCount}");

// Export multiple formats in a single operation
var multiExport = await exportService.ExportMultiFormatAsync(
    dataPoints: new List<DataPoint>
    {
        new DataPoint(4, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 21.5, "IoTSensor-004") { Quality = 94 },
        new DataPoint(5, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 23.9, "IoTSensor-005") { Quality = 88 }
    },
    outputPath: Path.Combine(Directory.GetCurrentDirectory(), "exports"),
    formats: new List<string> { "csv", "json", "parquet" }
);

Console.WriteLine($"\nMulti-format export results:");
foreach (var result in multiExport)
{
    Console.WriteLine($"  Format: {result.OutputPath.Split('.').Last()}, Success: {result.Success}");
}

// Use BatchExportProcessor for large datasets
var batchProcessor = new BatchExportProcessor(exportService, batchSize: 1000);
var batchResult = await batchProcessor.ExportInBatchesAsync(
    dataPoints: Enumerable.Range(1, 5000).Select(i => 
        new DataPoint(i, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 20.0 + i % 10, 
        $"IoTSensor-{i:D4}") { Quality = 90 + i % 10 }),
    outputPath: Path.Combine(Directory.GetCurrentDirectory(), "exports", "batch-data"),
    format: "csv"
);

Console.WriteLine($"\nBatch export completed: Success={batchResult.Success}, Exported={batchResult.ExportedRecords}");
Console.WriteLine($"Generated {batchResult.BatchFiles.Count} batch files:");
foreach (var batchFile in batchResult.BatchFiles)
{
    Console.WriteLine($"  {Path.GetFileName(batchFile)}");
}
```

## CommandExecutor

`CommandExecutor` provides a robust mechanism for executing command-line operations within the pipeline, supporting both synchronous and asynchronous execution patterns. It handles command invocation, output capture, error detection, and provides utilities for data ingestion, querying, status monitoring, and visualization. The executor supports both direct command execution and factory-based creation of data loaders and exporters, making it suitable for pipeline operations that require external tool integration or data processing workflows.

```csharp
using DotNetRealtimePipeline.CLI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Create a command executor for pipeline operations
var executor = new CommandExecutor();

// Execute a command and get the exit code
int exitCode = await executor.ExecuteAsync("dotnet", new[] { "build", "--configuration", "Release" });
Console.WriteLine($"Build exit code: {exitCode}");

// Ingest data from a command output
bool ingestionSuccess = await executor.IngestDataAsync("data-processor", new[] { "--input", "data.json", "--format", "jsonl" });
Console.WriteLine($"Data ingestion successful: {ingestionSuccess}");

// Query data using a command
var queryResults = await executor.QueryDataAsync("SELECT * FROM data WHERE timestamp > 1000");
Console.WriteLine($"Found {queryResults.Count} data points");

// Get pipeline status
var status = await executor.GetStatusAsync();
Console.WriteLine($"Pipeline status: {status["status"]}");

// Export data to a file
bool exportSuccess = await executor.ExportDataAsync("output/data-export.jsonl", "jsonl");
Console.WriteLine($"Export successful: {exportSuccess}");

// Generate visualization from pipeline data
string visualizationPath = await executor.VisualizeAsync("throughput-over-time", new Dictionary<string, object>
{
    { "outputPath", "./visualizations/" },
    { "chartType", "line" },
    { "title", "Pipeline Throughput" }
});
Console.WriteLine($"Visualization saved to: {visualizationPath}");

// Use factory methods to create specialized loaders and exporters
var loader = CommandExecutor.CreateLoader("json");
var dataPoints = await loader.LoadAsync("data/input.jsonl");
Console.WriteLine($"Loaded {dataPoints.Count} data points");

var exporter = CommandExecutor.CreateExporter("csv");
await exporter.ExportAsync("output/export.csv", dataPoints);
Console.WriteLine("Data exported successfully");
```