# BackpressureMetricsCollectorTests

`BackpressureMetricsCollectorTests` is a test suite verifying the behavior of the `BackpressureMetricsCollector` class, which tracks and aggregates metrics related to backpressure events within a real-time data pipeline. These tests validate the accurate recording, aggregation, and retrieval of backpressure-related events, such as stage activations, manual events, and snapshots of metrics. The suite ensures the collector correctly handles edge cases, such as unknown stages or empty event histories, and maintains consistency across operations like resets and polling.

## API

### `GetStageMetrics_UnknownStage_ReturnsNull`
**Purpose**: Validates that querying metrics for a non-existent stage returns `null`.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw; test failure indicates a defect.

### `RecordManualEvent_Activation_IncrementsActivationCount`
**Purpose**: Ensures that recording a manual activation event increments the activation count for the specified stage.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw; test failure indicates a defect.

### `RecordManualEvent_TwoActivations_CountIsTwo`
**Purpose**: Confirms that recording two manual activation events for the same stage results in an activation count of two.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw; test failure indicates a defect.

### `GetSnapshot_WithNoEvents_ReturnsEmptySnapshot`
**Purpose**: Verifies that retrieving a snapshot when no events have been recorded returns an empty snapshot.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw; test failure indicates a defect.

### `GetSnapshot_AggregatesAcrossStages`
**Purpose**: Tests that the snapshot aggregates metrics correctly across multiple stages.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw; test failure indicates a defect.

### `GetRecentEvents_ReturnsUpToRequestedCount`
**Purpose**: Ensures that retrieving recent events returns no more than the requested number of events, preserving chronological order.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw; test failure indicates a defect.

### `GetStageEvents_ReturnsOnlyEventsForThatStage`
**Purpose**: Validates that querying events for a specific stage returns only events associated with that stage.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw; test failure indicates a defect.

### `Reset_ClearsAllMetricsAndEvents`
**Purpose**: Confirms that calling `Reset` clears all recorded metrics and events, returning the collector to its initial state.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw; test failure indicates a defect.

### `Poll_AfterBackpressureActivated_RecordsActivationEvent`
**Purpose**: Verifies that polling the collector after backpressure activation records an activation event for the relevant stage.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw; test failure indicates a defect.

## Usage

### Example 1: Recording and Retrieving Manual Events
