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
