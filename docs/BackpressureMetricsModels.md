# Backpressure metrics models

The types in `src/Metrics/BackpressureMetricsModels.cs` describe individual backpressure transitions, per-stage measurements, and pipeline-wide aggregate snapshots. They are populated by [`BackpressureMetricsCollector`](BackpressureMetricsCollector.md).

## `BackpressureEvent`

A time-stamped backpressure event recorded by `BackpressureMetricsCollector`.

| Name | Type | Meaning |
| --- | --- | --- |
| `Timestamp` | `DateTime` | UTC time the event was captured. |
| `StageName` | `string` | Name of the pipeline stage that triggered backpressure. |
| `BufferFillPercent` | `double` | Buffer fill percentage at the moment of the event, from 0 to 100. |
| `IsActivation` | `bool` | `true` when backpressure was activated; `false` when it was released. |
| `DroppedItems` | `long` | Number of items dropped at this stage when the event was captured. |

## `StageBackpressureMetrics`

A snapshot of backpressure metrics for one pipeline stage.

| Name | Type | Meaning |
| --- | --- | --- |
| `StageName` | `string` | Stage name. |
| `ActivationCount` | `long` | Number of times backpressure was activated for this stage. |
| `TotalActiveDurationMs` | `long` | Cumulative milliseconds that backpressure was active. |
| `PeakBufferFillPercent` | `double` | Peak buffer fill percentage observed. |
| `CurrentBufferFillPercent` | `double` | Current buffer fill percentage. |
| `TotalDroppedItems` | `long` | Total items dropped at this stage. |
| `LastActivationAt` | `DateTime?` | UTC timestamp of the last activation event. |

## `BackpressureMetricsSnapshot`

Aggregated backpressure metrics across the whole pipeline.

| Name | Type | Meaning |
| --- | --- | --- |
| `StageMetrics` | `List<StageBackpressureMetrics>` | Per-stage metrics. |
| `TotalActivations` | `long` | Total backpressure activation events across all stages. |
| `TotalDroppedItems` | `long` | Total items dropped across all stages. |
| `ActiveBackpressureStages` | `int` | Number of stages currently under backpressure. |
| `SnapshotAt` | `DateTime` | UTC time the snapshot was taken. |

## How the collector produces the models

The [`BackpressureMetricsCollector`](BackpressureMetricsCollector.md) maintains internal tracking state for each stage and a bounded event history.

- `Poll()` reads all registered stages from `BackpressureService`. Each poll refreshes the current fill percentage and dropped-item count, updates the observed peak, and detects changes between inactive and active backpressure states.
- On an inactive-to-active transition, the collector increments the stage activation count, records the UTC activation time, and appends an activation `BackpressureEvent`. On an active-to-inactive transition, it adds the completed active interval to `TotalActiveDurationMs` and appends a release event.
- `RecordManualEvent(...)` appends an event immediately and updates the corresponding stage state without waiting for a poll. It creates the state when the stage has not previously been tracked.
- `GetStageMetrics(stageName)` maps the collector's current internal state to a new `StageBackpressureMetrics` instance, or returns `null` for an untracked stage.
- `GetSnapshot()` creates one `StageBackpressureMetrics` per tracked stage, sums activation and dropped-item totals, counts stages whose current state is backpressured, and stamps the resulting `BackpressureMetricsSnapshot` with the current UTC time.
- `GetRecentEvents(count)` returns up to the requested number of most recently appended `BackpressureEvent` objects in chronological insertion order. `GetStageEvents(stageName)` filters the retained history for a stage using a case-insensitive name comparison. When the configured history limit is exceeded, the oldest events are removed.
- `Reset()` clears both the per-stage tracking state and event history.
