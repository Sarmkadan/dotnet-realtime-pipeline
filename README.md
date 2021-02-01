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
```