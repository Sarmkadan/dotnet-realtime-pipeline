# StageScalingState

`StageScalingState` is the mutable, per-stage record that [DynamicScalingService](DynamicScalingService.md) keeps while it adjusts consumer limits. There is one instance per pipeline stage that the service has evaluated. The instance holds the stage's current consumer limit, the last decision that was actually applied, the time of that action, and lifetime scale-up and scale-down counters.

The class is defined in `src/Domain/Models/ScalingDecision.cs`, next to [ScalingDecision](ScalingDecision.md) and `ScalingDirection`. It is `sealed` and has no constructor logic; every field is set through object initialisers or by the service.

```csharp
using DotNetRealtimePipeline.Domain.Models;

// The service creates and updates these objects internally.
// Callers normally read them through the service:
StageScalingState? state = scalingService.GetScalingState("transform");

if (state is not null)
{
    Console.WriteLine($"{state.StageName}: {state.CurrentConsumers} consumers, " +
                      $"{state.ScaleUpCount} up / {state.ScaleDownCount} down");
}
```

## Public Members

| Member | Type | Setter | Description |
|--------|------|--------|-------------|
| `StageName` | `string` | `init` | Name of the monitored stage. Defaults to `string.Empty`. Fixed once the object is created. |
| `CurrentConsumers` | `int` | `set` | The consumer limit the service last wrote for the stage. |
| `LastDecision` | `ScalingDecision?` | `set` | The most recent *applied* scaling decision. `null` until the first action. |
| `LastScalingActionAt` | `DateTime` | `set` | UTC time of the last applied action. Defaults to `DateTime.MinValue`. |
| `ScaleUpCount` | `int` | `set` | Number of scale-up actions applied to the stage since the state was created. |
| `ScaleDownCount` | `int` | `set` | Number of scale-down actions applied to the stage since the state was created. |
| `string ToString()` | `string` | — | Single-line summary, for example `transform CurrentConsumers=4 LastScalingActionAt=2026-11-04T10:15:00.0000000Z ScaleUpCount=3 ScaleDownCount=0 LastDecision=transform Up 3->4 ...`. Shows `LastDecision=none` when no action has been applied. |

Only `StageName` is immutable. The other properties have public setters, which is how the service updates them. The service does this while holding its internal lock, but the setters themselves do not lock. See [Thread Safety and Mutability](#thread-safety-and-mutability).

### StageName

The key under which the service stores the state. It matches `PipelineStageDef.StageName` in the pipeline configuration and the name passed to `BackpressureService.GetContext`.

### CurrentConsumers

The consumer limit that the service last applied to the stage's backpressure context. On creation it is initialised from `context.MaxConcurrentConsumers`, so it reflects the limit at first evaluation. After that it changes only when a scaling action is applied, and it always equals the `ToConsumers` value of the applied decision.

Because it is only written when the service acts, it can become stale if something else changes `MaxConcurrentConsumers` on the context. The live value is always on the backpressure context itself.

### LastDecision

The `ScalingDecision` for the most recent action that was actually applied. Decisions with `Direction == ScalingDirection.None` are never stored, so `LastDecision` is never a "no change" record. It keeps the decision's `Reason`, `FromConsumers`, `ToConsumers`, `BufferFillPercent`, `BackpressureFrequency`, and `DecidedAt` values, which describe the metrics that triggered the action.

### LastScalingActionAt

The UTC time at which the service applied the most recent action. The cooldown check compares the current time with this value:

```
if (DateTime.UtcNow - state.LastScalingActionAt < cooldown) → skip the stage
```

It starts at `DateTime.MinValue`, so the first evaluation of a new stage is never blocked by cooldown. It is set to `DateTime.UtcNow` at the moment an action is applied, which is slightly later than `LastDecision.DecidedAt`.

### ScaleUpCount and ScaleDownCount

Lifetime counters. Each is incremented once per applied action of its direction. They are never reset and are not persisted, so they start at zero each time the process starts. A stage whose decisions were all `None` keeps both counters at zero.

## Lifecycle

The state object is created, updated, and read as follows. The steps refer to [DynamicScalingService](DynamicScalingService.md).

### 1. Creation

`EvaluateScalingAsync` calls `EvaluateStage` for each enabled stage in the pipeline configuration. If the stage has no backpressure context yet (`GetContext` returns `null`), the method returns and no state is created. Otherwise, if `_states` has no entry for the stage, the service creates one:

```csharp
state = new StageScalingState
{
    StageName = stageName,
    CurrentConsumers = context.MaxConcurrentConsumers
};
_states[stageName] = state;
```

The state is stored in the service's dictionary and is not removed afterwards. A stage that is later disabled keeps its last state.

### 2. Cooldown gate

The state's `LastScalingActionAt` is compared with the configured cooldown (15 seconds by default). If the cooldown has not elapsed, the stage is skipped for this pass and no fields change.

### 3. Decision

`ComputeDecision` uses the buffer fill percentage and backpressure frequency from the context. The rules are:

| Direction | Condition | Target |
|-----------|-----------|--------|
| `Up` | fill ≥ scale-up threshold (default 75%) and current < max | current + 1 |
| `Down` | fill ≤ scale-down threshold (default 30%), frequency < 0.5 per minute, and current > min | current − 1 |
| `None` | neither condition holds | no change |

The `None` result is discarded. Nothing on the state object changes, apart from the creation in step 1.

### 4. Application

For an `Up` or `Down` decision, the service applies the change in this order:

1. Sets `context.MaxConcurrentConsumers` to `ToConsumers`. New consumers now respect the new limit at once.
2. Sets `state.CurrentConsumers` to `ToConsumers`.
3. Sets `state.LastDecision` to the decision.
4. Sets `state.LastScalingActionAt` to `DateTime.UtcNow`.
5. Increments `ScaleUpCount` or `ScaleDownCount`.
6. Logs the change at `Information` level.

Steps 2 to 5 all happen while the service's `_lock` is held. The state therefore moves between consistent values from the service's point of view.

### 5. Reading

Callers read the state in two ways:

- `GetScalingState(stageName)` returns the live instance for the stage, or `null` if the stage has never been evaluated.
- `GetAllScalingStates()` returns a new dictionary with a copy of the entries. The `StageScalingState` objects in it are the same instances the service updates.

The extension method `GetOrCreateScalingState` in `DynamicScalingServiceExtensions` returns the stored state if there is one. If there is none, it returns a new, unregistered instance with `CurrentConsumers = 1`. That value is a fallback and does not reflect the real limit on the context. The new instance is not stored, so changes to it are never seen by the service.

## Usage

### Example 1: Reporting scaling activity

```csharp
var states = scalingService.GetAllScalingStates();

foreach (var state in states.Values)
{
    Console.WriteLine(
        $"{state.StageName,-16} consumers={state.CurrentConsumers,3} " +
        $"up={state.ScaleUpCount,3} down={state.ScaleDownCount,3} " +
        $"last={(state.LastDecision?.Direction.ToString() ?? "none")}");
}
```

### Example 2: Checking whether a stage recently scaled

```csharp
var state = scalingService.GetScalingState("transform");

if (state?.LastDecision is { Direction: not ScalingDirection.None } decision &&
    DateTime.UtcNow - state.LastScalingActionAt < TimeSpan.FromMinutes(1))
{
    Console.WriteLine($"Stage scaled {decision.Direction} at {state.LastScalingActionAt:O}: {decision.Reason}");
}
```

## Thread Safety and Mutability

- `DynamicScalingService` guards its dictionary and all state updates with a private lock. `GetScalingState` also reads under that lock.
- The properties on `StageScalingState` are plain auto-properties with public setters. Nothing stops a caller from changing them, and the service's cooldown and counter logic depends on them. Treat instances returned from the service as read-only. Do not write to them.
- Because `GetAllScalingStates` returns the same objects, a snapshot taken this way is not a point-in-time copy. Values can change after the call returns, and reading several properties may mix values from different scaling actions. Copy the values you need if you want a consistent view.
- Reads of individual properties are not synchronised with the service's lock, so a read made during an evaluation pass can see a partially applied action.

## Related Types

- [DynamicScalingService](DynamicScalingService.md) creates, updates, and exposes these states.
- [ScalingDecision](ScalingDecision.md) describes the actions that are copied into `LastDecision`.
- `ScalingDirection` (defined alongside `ScalingDecision`) gives the direction of each action.
- `DynamicScalingServiceExtensions` provides `GetScalingStatesList` and `GetOrCreateScalingState` helpers.
