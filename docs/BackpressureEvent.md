# BackpressureEvent

Represents a snapshot of back‑pressure diagnostics collected from a stage in the real‑time pipeline. It aggregates timing, buffer utilisation, activation counts and dropped‑item statistics for monitoring and alerting purposes.

## API

| Member | Type | Purpose | Get/Set | Exceptions |
|--------|------|---------|---------|------------|
| **Timestamp** | `DateTime` | The moment the snapshot was taken. | get; set; | None. |
| **StageName** | `string` | Identifier of the pipeline stage to which the metrics belong. | get; set; | Throws `ArgumentNullException` if set to `null`. |
| **BufferFillPercent** | `double` | Current buffer utilisation as a percentage (0‑100). | get; set; | None; values outside 0‑100 are considered invalid but not enforced by the type. |
| **IsActivation** | `bool` | Indicates whether the snapshot corresponds to an activation event (true) or a regular sample (false). | get; set; | None. |
| **DroppedItems** | `long` | Number of items dropped since the previous snapshot for this stage. | get; set; | None. |
| **ActivationCount** | `long` | Cumulative count of activation events observed for the stage up to this snapshot. | get; set; | None. |
| **TotalActiveDurationMs** | `long` | Total time, in milliseconds, the stage has spent in an active (non‑idle) state. | get; set; | None. |
| **PeakBufferFillPercent** | `double` | Highest buffer utilisation percentage observed since monitoring began. | get; set; | None. |
| **CurrentBufferFillPercent** | `double` | Alias for `BufferFillPercent`; retained for compatibility. | get; set; | None. |
| **TotalDroppedItems** | `long` | Cumulative number of items dropped by the stage since the start of the pipeline. | get; set; | None. |
| **LastActivationAt** | `DateTime?` | Timestamp of the most recent activation event; `null` if no activation has occurred. | get; set; | None. |
| **StageMetrics** | `List<StageBackpressureMetrics>` | Detailed per‑sub‑stage metrics when the stage aggregates multiple sub‑components. | get; set; | Throws `ArgumentNullException` if set to `null`. Individual elements may be `null`; callers should validate. |
| **TotalActivations** | `long` | Total number of activation events recorded for the stage (same semantic as `ActivationCount` but kept for backward compatibility). | get; set; | None. |
| **ActiveBackpressureStages** | `int` | Number of stages currently experiencing back‑pressure (buffer fill above a configured threshold) within the pipeline at snapshot time. | get; set; | None. |
| **SnapshotAt** | `DateTime` | The exact UTC time when the entire pipeline snapshot was captured. | get; set; | None. |

## Usage

### Example 1: Logging a back‑pressure snapshot

```csharp
var evt = new BackpressureEvent
{
    Timestamp = DateTime.UtcNow,
    StageName = "Ingress",
    BufferFillPercent = 78.5,
    IsActivation = false,
    DroppedItems = 0,
    ActivationCount = 12,
    TotalActiveDurationMs = 345000,
    PeakBufferFillPercent = 92.0,
    CurrentBufferFillPercent = 78.5,
    TotalDroppedItems = 3,
    LastActivationAt = DateTime.UtcNow.AddMinutes(-5),
    StageMetrics = new List<StageBackpressureMetrics> { /* … */ },
    TotalActivations = 12,
    ActiveBackpressureStages = 2,
    SnapshotAt = DateTime.UtcNow
};

logger.LogInformation(
    "Stage {Stage} buffer fill: {Percent}% (dropped {Dropped})",
    evt.StageName,
    evt.BufferFillPercent,
    evt.DroppedItems);
```

### Example 2: Detecting sustained back‑pressure

```csharp
// Assume a rolling list of recent snapshots
var recent = backpressureSnapshots.Where(s => s.StageName == "Processor")
                                  .OrderByDescending(s => s.Timestamp)
                                  .Take(5)
                                  .ToList();

if (recent.All(s => s.BufferFillPercent > 85.0) &&
    recent.All(s => s.IsActivation))
{
    alert.Raise(
        $"Stage {recent.First().StageName} has been in high back‑pressure for {recent.Count} consecutive activations.");
}
```

## Notes

- The type does not enforce value ranges (e.g., `BufferFillPercent` between 0 and 100). Consumers should validate data if correctness is critical.
- `StageName` and `StageMetrics` setters throw `ArgumentNullException` when supplied with `null`; all other members accept any value of their type without throwing.
- The class is mutable; concurrent reads and writes from multiple threads are not thread‑safe. External synchronization (e.g., locks or immutable copies) is required when sharing instances across threads.
- `StageMetrics` may be empty but should never be `null` after construction; setting it to `null` will cause an exception.
- Duplicate member names in the source listing (`StageName`, `TotalDroppedItems`) refer to the same property; the documentation treats them as a single logical member.
