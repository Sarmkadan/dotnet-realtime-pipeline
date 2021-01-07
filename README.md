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

## Backpressure Metrics