# BackpressureMetricsSnapshot

`BackpressureMetricsSnapshot` is the pipeline-wide, point-in-time view of backpressure behaviour returned by [`BackpressureMetricsCollector.GetSnapshot()`](BackpressureMetricsCollector.md). It holds one [`StageBackpressureMetrics`](#stagebackpressuremetrics) entry per tracked stage plus aggregate totals. Both types are defined in `src/Metrics/BackpressureMetricsModels.cs`. The full field reference for every model in that file is in [BackpressureMetricsModels](BackpressureMetricsModels.md); this page focuses on the snapshot and its per-stage entries.

## `BackpressureMetricsSnapshot`

A snapshot is an immutable-in-practice copy: `GetSnapshot()` builds new objects from the collector's internal state, so later polls do not change a snapshot you already hold.

| Name | Type | Meaning |
| --- | --- | --- |
| `StageMetrics` | `List<StageBackpressureMetrics>` | One entry per stage the collector has seen. Order follows the collector's internal dictionary and is not guaranteed. |
| `TotalActivations` | `long` | Sum of `ActivationCount` across all stages. |
| `TotalDroppedItems` | `long` | Sum of `TotalDroppedItems` across all stages. |
| `ActiveBackpressureStages` | `int` | Number of stages whose tracked state is currently backpressured (activated and not yet released). |
| `SnapshotAt` | `DateTime` | UTC time at which `GetSnapshot()` ran. |

`ToString()` returns a summary of `SnapshotAt`, `StageCount`, `TotalActivations`, `TotalDroppedItems` and `ActiveBackpressureStages`. It does not include the per-stage entries.

## `StageBackpressureMetrics`

The metrics for one stage, as the collector knows them at the time of the snapshot.

| Name | Type | Meaning |
| --- | --- | --- |
| `StageName` | `string` | Stage name, as passed to `Poll()` (via `BackpressureService`) or `RecordManualEvent(...)`. |
| `ActivationCount` | `long` | Number of inactive-to-active transitions recorded for the stage. |
| `TotalActiveDurationMs` | `long` | Cumulative milliseconds of completed backpressure intervals. See the notes below. |
| `PeakBufferFillPercent` | `double` | Highest buffer fill percentage recorded for the stage (0 to 100). |
| `CurrentBufferFillPercent` | `double` | Buffer fill percentage from the most recent poll or manual event for the stage. |
| `TotalDroppedItems` | `long` | Dropped-item count from the most recent poll or manual event. This is the latest reported value, not a delta. |
| `LastActivationAt` | `DateTime?` | UTC time of the most recent activation, or `null` if the stage has never been activated. |

## When snapshots are taken

Snapshots are never taken automatically. Nothing in the collector stores them or schedules them.

- **On demand.** Each call to `GetSnapshot()` creates a new snapshot stamped with the current UTC time. Dashboards, health checks, and log lines typically call it when they need current numbers.
- **Reflecting the last update.** `GetSnapshot()` does **not** poll `BackpressureService`. The values reflect whatever `Poll()` or `RecordManualEvent(...)` last recorded. If nothing calls `Poll()` on a timer, the snapshot can be arbitrarily stale even though `SnapshotAt` shows the current time.
- **Through the extension helpers.** `BackpressureMetricsCollectorExtensions` (for example, the total activation and dropped-item helpers) calls `GetSnapshot()` internally, so each such call also produces a fresh snapshot.
- **After `Reset()`.** `Reset()` clears all stage state and history. The next snapshot has an empty `StageMetrics` list and zero totals, with `SnapshotAt` set to the time of the call.

No code in `src/` calls `Poll()`. The collector is registered as a singleton in `src/Configuration/ServiceCollectionExtensions.cs`, so the host must call `Poll()` on a schedule for the data to stay current.

```csharp
using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
while (await timer.WaitForNextTickAsync(cancellationToken))
{
    collector.Poll();
}
```

## Notes on interpreting the values

- **`TotalActiveDurationMs` counts only finished intervals.** An ongoing activation adds nothing until `Poll()` observes the release. Its duration is measured from `LastActivationAt` to the poll time.
- **Manual releases do not add duration.** `RecordManualEvent(..., isActivation: false)` clears the active flag but does not update `TotalActiveDurationMs`. Use `Poll()` if you need durations.
- **`ActiveBackpressureStages` reflects the flag, not the fill level.** A stage counts as active only while its flag is set. A stage whose buffer is nearly full but not flagged is not counted.
- **`CurrentBufferFillPercent` and `TotalDroppedItems` are last-seen values.** They are overwritten on every poll or manual event.
- **Peaks are never reset** except by `Reset()`.
- **Stage order is not stable.** Sort by `StageName` if you need deterministic output.

## Example

The example below uses manual events so the numbers are deterministic. The values are illustrative and match what the code computes; `LastActivationAt` and `SnapshotAt` depend on the wall clock.

```csharp
var collector = new BackpressureMetricsCollector(backpressureService);

// "Ingest" activates at 82.5% fill, then releases at 40% after 14 items were dropped.
collector.RecordManualEvent("Ingest", 82.5, droppedItems: 0, isActivation: true);
collector.RecordManualEvent("Ingest", 40.0, droppedItems: 14, isActivation: false);

// "Enrich" activates at 91% fill with 3 items dropped and stays active.
collector.RecordManualEvent("Enrich", 91.0, droppedItems: 3, isActivation: true);

BackpressureMetricsSnapshot snapshot = collector.GetSnapshot();
```

The snapshot then contains:

| Field | Value |
| --- | --- |
| `TotalActivations` | `2` (1 from `Ingest` + 1 from `Enrich`) |
| `TotalDroppedItems` | `17` (14 from `Ingest` + 3 from `Enrich`) |
| `ActiveBackpressureStages` | `1` (only `Enrich` is still flagged) |
| `SnapshotAt` | UTC time of the `GetSnapshot()` call |

Per stage:

| `StageName` | `ActivationCount` | `TotalActiveDurationMs` | `PeakBufferFillPercent` | `CurrentBufferFillPercent` | `TotalDroppedItems` | `LastActivationAt` |
| --- | --- | --- | --- | --- | --- | --- |
| `Ingest` | 1 | 0 (manual release, see notes) | 82.5 | 40.0 | 14 | time of the first `RecordManualEvent` |
| `Enrich` | 1 | 0 | 91.0 | 91.0 | 3 | time of the `Enrich` `RecordManualEvent` |

The snapshot extension methods in `src/Metrics/BackpressureMetricsSnapshotExtensions.cs` operate on these values:

```csharp
bool critical = snapshot.IsCritical(90);        // true: Enrich is at 91% (> 90)
double utilization = snapshot.UtilizationPercent(); // (40.0 + 91.0) / 2 = 65.5
```

## See also

- [BackpressureMetricsModels](BackpressureMetricsModels.md) - field reference for all models in `BackpressureMetricsModels.cs`
- [BackpressureMetricsCollector](BackpressureMetricsCollector.md) - producer of snapshots
- [BackpressureEvent](BackpressureEvent.md) - the event records that feed the history
- [BackpressureService](BackpressureService.md) - source of the stage buffer and drop state polled by `Poll()`
