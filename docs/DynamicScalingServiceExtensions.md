# DynamicScalingServiceExtensions

Provides extension methods and state-access utilities for the dynamic scaling service, which governs automatic consumer count adjustments in a real-time pipeline. The type exposes operations to attempt scale-up and scale-down actions, query configured scaling boundaries, retrieve human-readable threshold information, and inspect or initialise per-stage scaling state.

## API

### TryScaleUpAsync

```csharp
public static async Task<bool> TryScaleUpAsync(
    this IDynamicScalingService scalingService,
    string stageName,
    CancellationToken cancellationToken = default)
```

Attempts to increase the consumer count for the specified pipeline stage by one, subject to the configured maximum consumer limit and current scaling thresholds.

- **Parameters**:
  - `scalingService`: The dynamic scaling service instance being extended.
  - `stageName`: The unique name of the pipeline stage to scale up.
  - `cancellationToken`: A token that can cancel the asynchronous operation.
- **Returns**: `true` if a scale-up was successfully requested and accepted; `false` if scaling was blocked by the maximum consumer limit, threshold conditions, or other guard logic.
- **Exceptions**: Throws `ArgumentNullException` when `stageName` is `null` or whitespace. Throws `InvalidOperationException` if the scaling service has not been initialised. Propagates any transport-level exceptions from the underlying consumer provisioning mechanism.

### TryScaleDownAsync

```csharp
public static async Task<bool> TryScaleDownAsync(
    this IDynamicScalingService scalingService,
    string stageName,
    CancellationToken cancellationToken = default)
```

Attempts to decrease the consumer count for the specified pipeline stage by one, subject to the configured minimum consumer limit and current scaling thresholds.

- **Parameters**:
  - `scalingService`: The dynamic scaling service instance being extended.
  - `stageName`: The unique name of the pipeline stage to scale down.
  - `cancellationToken`: A token that can cancel the asynchronous operation.
- **Returns**: `true` if a scale-down was successfully requested and accepted; `false` if scaling was blocked by the minimum consumer limit, threshold conditions, or other guard logic.
- **Exceptions**: Throws `ArgumentNullException` when `stageName` is `null` or whitespace. Throws `InvalidOperationException` if the scaling service has not been initialised. Propagates any transport-level exceptions from the underlying consumer decommissioning mechanism.

### GetMinConsumers

```csharp
public static int GetMinConsumers(
    this IDynamicScalingService scalingService,
    string stageName)
```

Retrieves the absolute minimum number of consumers permitted for the given stage, as defined in the pipeline configuration.

- **Parameters**:
  - `scalingService`: The dynamic scaling service instance being extended.
  - `stageName`: The unique name of the pipeline stage.
- **Returns**: The configured minimum consumer count. Returns `1` if no stage-specific override exists.
- **Exceptions**: Throws `ArgumentNullException` when `stageName` is `null` or whitespace. Throws `KeyNotFoundException` if the stage name is not recognised in the current pipeline topology.

### GetMaxConsumers

```csharp
public static int GetMaxConsumers(
    this IDynamicScalingService scalingService,
    string stageName)
```

Retrieves the absolute maximum number of consumers permitted for the given stage, as defined in the pipeline configuration.

- **Parameters**:
  - `scalingService`: The dynamic scaling service instance being extended.
  - `stageName`: The unique name of the pipeline stage.
- **Returns**: The configured maximum consumer count. Returns `Int32.MaxValue` if no stage-specific cap exists.
- **Exceptions**: Throws `ArgumentNullException` when `stageName` is `null` or whitespace. Throws `KeyNotFoundException` if the stage name is not recognised in the current pipeline topology.

### GetScalingThresholdsInfo

```csharp
public static string GetScalingThresholdsInfo(
    this IDynamicScalingService scalingService,
    string stageName)
```

Produces a human-readable string summarising the current scaling thresholds, limits, and active consumer count for the specified stage. Intended for diagnostics, logging, and dashboard display.

- **Parameters**:
  - `scalingService`: The dynamic scaling service instance being extended.
  - `stageName`: The unique name of the pipeline stage.
- **Returns**: A formatted string containing threshold values (e.g., backlog depth triggers, latency ceilings), the current consumer count, and the min/max boundaries.
- **Exceptions**: Throws `ArgumentNullException` when `stageName` is `null` or whitespace. Throws `KeyNotFoundException` if the stage name is not recognised.

### GetScalingStatesList

```csharp
public static IReadOnlyList<StageScalingState> GetScalingStatesList(
    this IDynamicScalingService scalingService)
```

Returns a snapshot of the scaling state for every known pipeline stage. Each entry includes the stage name, current consumer count, min/max limits, last evaluation timestamp, and the most recent scaling recommendation.

- **Parameters**:
  - `scalingService`: The dynamic scaling service instance being extended.
- **Returns**: An immutable list of `StageScalingState` instances, ordered by stage registration sequence. The list is empty if no stages are registered.
- **Exceptions**: Throws `InvalidOperationException` if the scaling service has not been initialised.

### GetOrCreateScalingState

```csharp
public static StageScalingState GetOrCreateScalingState(
    this IDynamicScalingService scalingService,
    string stageName)
```

Obtains the existing `StageScalingState` for the given stage, or atomically creates and registers a default state if none exists. This is the canonical entry point for ensuring a stage is tracked by the scaling subsystem before any scale attempts are made.

- **Parameters**:
  - `scalingService`: The dynamic scaling service instance being extended.
  - `stageName`: The unique name of the pipeline stage.
- **Returns**: The existing or newly created `StageScalingState` for the stage. The returned instance is the live object shared across the scaling service; mutations to it affect future scaling decisions.
- **Exceptions**: Throws `ArgumentNullException` when `stageName` is `null` or whitespace.

## Usage

### Example 1: Conditionally Scaling a Stage Based on Backpressure

```csharp
var scalingService = pipeline.Services.GetRequiredService<IDynamicScalingService>();
var stageName = "order-enrichment";

// Retrieve current state to inspect the last recommendation
var state = scalingService.GetOrCreateScalingState(stageName);
var info = scalingService.GetScalingThresholdsInfo(stageName);
logger.LogInformation("Scaling status for {Stage}: {Info}", stageName, info);

if (state.LastRecommendation == ScaleRecommendation.Up)
{
    bool scaled = await scalingService.TryScaleUpAsync(stageName, CancellationToken.None);
    if (scaled)
    {
        logger.LogInformation("Successfully scaled up {Stage}", stageName);
    }
    else
    {
        logger.LogWarning("Scale-up blocked for {Stage} (max consumers or threshold guard)", stageName);
    }
}
```

### Example 2: Draining Consumers During Maintenance Window

```csharp
var scalingService = pipeline.Services.GetRequiredService<IDynamicScalingService>();
var stages = scalingService.GetScalingStatesList();

foreach (var stage in stages)
{
    int min = scalingService.GetMinConsumers(stage.StageName);
    int max = scalingService.GetMaxConsumers(stage.StageName);

    // Scale down to the minimum while preserving at least one consumer
    while (stage.CurrentConsumerCount > min)
    {
        bool scaledDown = await scalingService.TryScaleDownAsync(
            stage.StageName,
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        if (!scaledDown)
        {
            logger.LogWarning("Scale-down halted for {Stage} at {Count} consumers",
                stage.StageName, stage.CurrentConsumerCount);
            break;
        }

        // Refresh state after each successful scale-down
        stage = scalingService.GetOrCreateScalingState(stage.StageName);
    }
}
```

## Notes

- **Thread Safety**: `GetOrCreateScalingState` guarantees atomic initialisation of per-stage state; concurrent callers for the same stage name will receive the same `StageScalingState` instance without duplication. The returned `StageScalingState` object is not inherently synchronised—external locking is required if multiple threads mutate its fields directly.
- **Idempotency of Scale Attempts**: `TryScaleUpAsync` and `TryScaleDownAsync` are safe to call repeatedly. They return `false` when the operation would violate configured boundaries or when the scaling evaluator determines no action is warranted, rather than throwing exceptions.
- **Cancellation Behaviour**: If a cancellation token is triggered during a scale attempt, the method may return `false` even if scaling conditions were otherwise favourable, because the underlying provisioning call was aborted. The scaling state is not partially mutated in this scenario.
- **Unregistered Stages**: `GetMinConsumers`, `GetMaxConsumers`, and `GetScalingThresholdsInfo` throw `KeyNotFoundException` for stage names that have not been registered in the pipeline topology. Always call `GetOrCreateScalingState` first if a stage may be dynamically introduced.
- **Snapshot Semantics**: `GetScalingStatesList` returns a point-in-time snapshot. The list itself is immutable, but the individual `StageScalingState` references point to live objects that may change immediately after the call.
- **Default Boundaries**: When no explicit min/max is configured, `GetMinConsumers` defaults to `1` and `GetMaxConsumers` defaults to `Int32.MaxValue`. Scaling operations still respect threshold evaluations, so a default maximum does not imply unbounded scale-up in practice.
