# DeadLetterQueueStats

`DeadLetterQueueStats` is a point-in-time snapshot of the state of a dead-letter queue. It is returned by `IDeadLetterQueue.GetStatsAsync()` and is consumed by `DeadLetterQueueExtensions.GetReportAsync()` to render a human-readable report. The snapshot is produced by `DeadLetterQueue` from the entries it currently holds plus a running counter of successfully resolved entries.

The type lives in `src/DeadLetter/DeadLetterEntry.cs`, next to `DeadLetterEntry` and the `DeadLetterStatus` enum it reports on.

## DeadLetterStatus

`DeadLetterStatus` describes the disposition of a `DeadLetterEntry`. The counts in `DeadLetterQueueStats` are derived from it.

| Value | Int | Meaning |
|-------|-----|---------|
| `Pending` | 0 | Waiting to be retried. Counted in `PendingEntries`. |
| `InRetry` | 1 | Handed out by `DequeueForRetryAsync()` and currently being retried. Counted in `InRetryEntries`. |
| `Resolved` | 2 | Successfully reprocessed. The entry is removed from the queue, so it is never counted in the status totals. |
| `PermanentFailure` | 3 | No further retries will be attempted. Counted in `PermanentFailureEntries`. |

## API

### DeadLetterQueueStats

A plain mutable class with public settable properties. It has no constructor parameters; `GeneratedAt` defaults to `DateTime.UtcNow` at construction time. Instances are created by `DeadLetterQueue.GetStatsAsync()`, but callers may also construct or modify them (for example, in tests).

- **int TotalEntries**
  Number of entries currently held by the queue. Equal to `DeadLetterQueue.Count` at the time the snapshot was taken.

- **int PendingEntries**
  Number of held entries whose `Status` is `Pending`.

- **int InRetryEntries**
  Number of held entries whose `Status` is `InRetry`, i.e. entries returned by `DequeueForRetryAsync()` that have not yet been acknowledged.

- **int PermanentFailureEntries**
  Number of held entries whose `Status` is `PermanentFailure`. These stay in the queue until they are evicted to make room for new entries.

- **long TotalResolved**
  Cumulative number of entries acknowledged as successful via `AcknowledgeSuccessAsync()` since the queue instance was created. It is a running total, not the number of entries currently in the queue, and it is not reset by eviction or replay.

- **DateTime GeneratedAt**
  UTC time at which the snapshot was generated.

- **string ToString()**
  Returns a single line listing every property, with `GeneratedAt` in round-trip ("O") UTC format.
  **Return value:** A non-null string suitable for logging.
  **Throws:** None.

### Validation

`DeadLetterQueueValidation` checks a stats instance for internal consistency. The rules are:

- `TotalEntries`, `PendingEntries`, `InRetryEntries`, `PermanentFailureEntries` and `TotalResolved` must be `>= 0`; each status count must not exceed `TotalEntries`.
- `GeneratedAt` must not be `default` and must not be more than 5 minutes in the future.
- `PendingEntries + InRetryEntries + PermanentFailureEntries` must equal `TotalEntries`.

Snapshots produced by `DeadLetterQueue` satisfy all of these rules, because the status counts are computed from the same locked view of the entries as `TotalEntries`.

## How the stats are produced

`DeadLetterQueue.GetStatsAsync()` takes the queue's internal lock once and builds the snapshot from that consistent state:

1. `TotalEntries` is `_entries.Count`.
2. `PendingEntries`, `InRetryEntries` and `PermanentFailureEntries` are counts of the held entries grouped by `Status`.
3. `TotalResolved` is the private `_totalResolved` counter.
4. `GeneratedAt` is `DateTime.UtcNow` at the moment of the call.

The counters are therefore derived in two different ways:

- **Status counts are computed on demand.** Nothing is cached. Each call scans the held entries, so the cost grows with queue size (default capacity is 1000).
- **`TotalResolved` is incremented incrementally.** It changes only in `AcknowledgeSuccessAsync()`, and only when the entry id is actually present in the queue. Acknowledging an unknown id does nothing.

Status transitions that feed the counts:

| Operation (`DeadLetterQueue`) | Effect on the entry | Effect on stats |
|-------------------------------|---------------------|-----------------|
| `EnqueueAsync()` | New entry with `Status = Pending` | `TotalEntries` and `PendingEntries` increase |
| `DequeueForRetryAsync()` | Eligible entries get `BeginRetry()`, so `Status = InRetry` | `PendingEntries` decreases, `InRetryEntries` increases |
| `AcknowledgeSuccessAsync()` | Entry is removed from the queue | `TotalEntries` decreases, `TotalResolved` increases by 1 |
| `AcknowledgeFailureAsync()` | `Status = PermanentFailure`, `ResolvedAt` set, note stored | `InRetryEntries` decreases, `PermanentFailureEntries` increases |
| `ReplayAsync(filter)` | Matching entries reset to `Pending`, `RetryCount = 0` | Matching entries move into `PendingEntries` |
| Eviction during `EnqueueAsync()` | Entries removed from the queue | `TotalEntries` and the affected status count decrease; `TotalResolved` is unchanged |

Notes on the table:

- `DequeueForRetryAsync()` only selects entries where `CanRetry` is true (`RetryCount < MaxRetries` and `Status == Pending`). Entries already in `InRetry` or `PermanentFailure` are not selected.
- `DeadLetterEntry.RetryFailed(reason)` is defined on the entry itself. `DeadLetterQueue` does not call it, so a failed retry is recorded with `AcknowledgeFailureAsync()`, which sets `PermanentFailure` immediately and does not return the entry to `Pending`.
- Because removal happens in the same locked section that changes `Status`, `Resolved` is never observed inside the queue. That is why `DeadLetterStatus.Resolved` has no counter of its own; successes are reported only through `TotalResolved`.

### Eviction and capacity

When the queue is at `maxCapacity`, `EnqueueAsync()` first removes every `PermanentFailure` entry. If the queue is still full, it removes the oldest entry by `EnqueuedAt`, whatever its status. Both cases lower `TotalEntries` and the matching status count without touching `TotalResolved`, so `PermanentFailureEntries` can go down between two snapshots even though nothing was resolved.

## Usage

```csharp
IDeadLetterQueue queue = new DeadLetterQueue(maxCapacity: 1000, defaultMaxRetries: 3);

await queue.EnqueueAsync(dataPoint, "Enrichment", "Timeout", new TimeoutException());

DeadLetterQueueStats stats = await queue.GetStatsAsync();
Console.WriteLine(stats);
// TotalEntries=1, PendingEntries=1, InRetryEntries=0, PermanentFailureEntries=0, TotalResolved=0, GeneratedAt=...

// Report generation uses the same snapshot
string report = await queue.GetReportAsync(includeDetails: true);
```

```csharp
// Alerting on growth of permanent failures
DeadLetterQueueStats stats = await queue.GetStatsAsync();

if (stats.PermanentFailureEntries > 0)
{
    logger.LogWarning(
        "Dead-letter queue holds {PermanentFailures} permanent failures out of {Total} entries; {Resolved} resolved since start",
        stats.PermanentFailureEntries,
        stats.TotalEntries,
        stats.TotalResolved);
}
```

## Notes

- `DeadLetterQueueStats` is a snapshot. Its values are not updated after `GetStatsAsync()` returns; call it again to observe changes.
- Status counts describe only the entries currently held. Use `TotalResolved` for the lifetime count of successful reprocessing, and remember that it resets when the queue instance is recreated (it is not persisted).
- `TotalResolved` is a `long`; the other counts are `int` and are bounded by the queue capacity.
- `GetStatsAsync()` is safe to call concurrently with other queue operations. The snapshot is consistent with respect to the queue's lock, but it may already be stale by the time the caller reads it.
- `DeadLetterQueue` is in-memory only. Stats do not survive a process restart.

## See also

- [DeadLetterEntry](DeadLetterEntry.md)
- [DeadLetterQueue](DeadLetterQueue.md)
- [IDeadLetterQueue](IDeadLetterQueue.md)
- [DeadLetterQueueExtensions](DeadLetterQueueExtensions.md)
