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

## Backpressure Metrics