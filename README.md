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
