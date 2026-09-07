# PipelineStateManager extensions

`PipelineStateManagerExtensions` adds history queries and formatting helpers to [`PipelineStateManager`](PipelineStateManager.md). All four methods throw `ArgumentNullException` when the manager is `null`.

## API

### `GetTransitionsTo`

```csharp
public static IReadOnlyList<StateTransition> GetTransitionsTo(
    this PipelineStateManager manager,
    PipelineState targetState)
```

Returns a snapshot containing every recorded transition whose `ToState` equals `targetState`. Matches retain the order returned by `GetStateHistory`; an empty read-only list is returned when there are no matches.

### `GetLastTransition`

```csharp
public static StateTransition? GetLastTransition(
    this PipelineStateManager manager)
```

Returns the last transition in the recorded history, or `null` when the history is empty.

### `GetTotalTimeInState`

```csharp
public static TimeSpan GetTotalTimeInState(
    this PipelineStateManager manager,
    PipelineState state)
```

Returns the cumulative time spent in `state`, based on transition timestamps sorted from oldest to newest. For each pair of transitions, the interval is assigned to the state entered by the earlier transition. If the manager is currently in the requested state, the method also includes the interval from the latest transition through `DateTime.UtcNow`.

When no transitions have been recorded, the method returns `GetCurrentStateDuration()` if `state` is the current state and `TimeSpan.Zero` otherwise.

The period before the first recorded transition is not included in the total.

### `ToHistoryString`

```csharp
public static string ToHistoryString(this PipelineStateManager manager)
```

Returns the transition history sorted by timestamp, with one transition per line in this format:

```text
Timestamp | FromState → ToState | Reason
```

Timestamps use the round-trip (`O`) format with invariant culture. A missing or empty reason is rendered as `<no reason>`. Lines are joined with the platform newline, and an empty history produces `string.Empty`.

## Example

```csharp
using DotNetRealtimePipeline.State;
using Microsoft.Extensions.Logging.Abstractions;

var manager = new PipelineStateManager(
    NullLogger<PipelineStateManager>.Instance);

manager.TransitionTo(PipelineState.Running, "Pipeline started");
manager.TransitionTo(PipelineState.Paused, "Maintenance window");
manager.TransitionTo(PipelineState.Running, "Maintenance complete");

IReadOnlyList<StateTransition> entriesIntoRunning =
    manager.GetTransitionsTo(PipelineState.Running);

StateTransition? latest = manager.GetLastTransition();
TimeSpan runningTime = manager.GetTotalTimeInState(PipelineState.Running);
string history = manager.ToHistoryString();

Console.WriteLine($"Entries into Running: {entriesIntoRunning.Count}");
Console.WriteLine($"Latest destination: {latest?.ToState}");
Console.WriteLine($"Recorded Running time: {runningTime}");
Console.WriteLine(history);
```

Because the timestamps are generated when each transition occurs, the exact durations and history timestamps vary between runs.
