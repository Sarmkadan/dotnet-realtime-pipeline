# BackpressureMetricsCollector

A metrics collector that tracks backpressure events and buffer utilization for a processing stage in a real-time data pipeline. It records activation counts, buffer fill levels, dropped items, and recent events to help diagnose performance bottlenecks and backpressure conditions.

## API

### `BackpressureMetricsCollector`
Initializes a new instance of the metrics collector for a specific stage.

### `void Poll()`
Triggers a polling cycle to update internal metrics. This should be called periodically by the pipeline runtime to ensure metrics reflect the current state.

### `void RecordManualEvent(BackpressureEvent @event)`
Records a manual backpressure event with the provided details.

- **Parameters**
  - `@event`: The backpressure event to record.
- **Throws**
  - `ArgumentNullException`: If `@event` is `null`.

### `StageBackpressureMetrics? GetStageMetrics()`
Gets the latest backpressure metrics for the stage, or `null` if no metrics have been recorded.

- **Returns**
  A `StageBackpressureMetrics` object containing peak and current buffer fill percentages, activation count, total active duration, and dropped item count.

### `BackpressureMetricsSnapshot GetSnapshot()`
Generates a snapshot of the current backpressure state, including recent events and stage metrics.

- **Returns**
  A `BackpressureMetricsSnapshot` containing stage metrics, recent events, and stage name.

### `IReadOnlyList<BackpressureEvent> GetRecentEvents()`
Gets a read-only list of the most recent backpressure events, ordered from newest to oldest.

- **Returns**
  An `IReadOnlyList<BackpressureEvent>` of recent events.

### `IReadOnlyList<BackpressureEvent> GetStageEvents()`
Gets a read-only list of all recorded backpressure events for the stage, ordered from newest to oldest.

- **Returns**
  An `IReadOnlyList<BackpressureEvent>` of all events.

### `void Reset()`
Resets all recorded metrics and events to their initial state. Useful for reusing the collector in test scenarios or after a pipeline restart.

### `string StageName`
Gets the name of the stage this collector is monitoring.

- **Returns**
  The stage name as a `string`.

### `bool WasBackpressured`
Indicates whether the stage has experienced backpressure since the last reset.

- **Returns**
  `true` if backpressure was detected; otherwise, `false`.

### `long ActivationCount`
Gets the total number of activations (processing cycles) for the stage.

- **Returns**
  The activation count as a `long`.

### `long TotalActiveDurationMs`
Gets the total duration (in milliseconds) the stage has been actively processing since the last reset.

- **Returns**
  The total active duration as a `long`.

### `double PeakBufferFillPercent`
Gets the highest observed buffer fill percentage during the current monitoring period.

- **Returns**
  The peak buffer fill percentage as a `double`.

### `double CurrentBufferFillPercent`
Gets the current buffer fill percentage.

- **Returns**
  The current buffer fill percentage as a `double`.

### `long TotalDroppedItems`
Gets the total number of items dropped due to backpressure since the last reset.

- **Returns**
  The total dropped items count as a `long`.

### `DateTime? LastActivationAt`
Gets the timestamp of the most recent activation, or `null` if the stage has never been activated.

- **Returns**
  The last activation time as a nullable `DateTime`.

## Usage

### Example 1: Monitoring a Stage
