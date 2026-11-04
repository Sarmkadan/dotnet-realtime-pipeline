# WorkerOptions

`WorkerOptions` is a plain, mutable settings object that describes how the background workers in `src/Workers/` should be scheduled and which of them should run. It is declared in `src/Configuration/EventServiceConfiguration.cs` in the global namespace (the file has no `namespace` declaration), so it is visible from any namespace without a `using` directive.

It exposes three toggles and two polling intervals. Two static constants hold the default interval values. The class is `sealed`, has a parameterless constructor, and overrides `ToString()` to produce a one-line summary of its current values.

> **Important:** in the current code base nothing reads a `WorkerOptions` instance to start or configure workers. The workers are constructed with plain `int` arguments (see [Relationship to the workers](#relationship-to-the-workers)), and `WorkerCoordinator.StartAll()` starts all three workers unconditionally. The `Enable*Worker` flags therefore have no effect today unless calling code reads them itself. Validation (`Validate`, `IsValid`, `EnsureValid`) is the only consumer.

## Members

### `DefaultMetricsAggregationIntervalMs` (constant)

```csharp
public const int DefaultMetricsAggregationIntervalMs = 5000;
```

The default value of `MetricsAggregationIntervalMs`: 5000 ms (5 seconds).

### `DefaultHealthCheckIntervalMs` (constant)

```csharp
public const int DefaultHealthCheckIntervalMs = 10000;
```

The default value of `HealthCheckIntervalMs`: 10000 ms (10 seconds).

### `MetricsAggregationIntervalMs`

| | |
|---|---|
| Type | `int` |
| Default | `5000` (`DefaultMetricsAggregationIntervalMs`) |
| Used by | `MetricsAggregationWorker` |

Milliseconds the metrics worker waits between aggregation passes. Each pass calls `MetricsService.GenerateHealthReportAsync()`, logs the status, processed and failed counts, throughput, and average and p95 processing times at `Information` level, and then waits. The wait starts after the pass finishes, so the actual period is the pass duration plus this interval.

The value is not checked when a worker is constructed. Validation rejects zero and negative values. Passing a value that is only invalid at runtime is not guarded: `0` makes the loop run back to back, and `-1` makes `Task.Delay` wait indefinitely.

### `HealthCheckIntervalMs`

| | |
|---|---|
| Type | `int` |
| Default | `10000` (`DefaultHealthCheckIntervalMs`) |
| Used by | `HealthCheckWorker` |

Milliseconds the health check worker waits between passes. Each pass calls `PipelineOrchestrator.GetHealthReportAsync()` and logs the status and throughput in items per second. Like the metrics interval, the wait begins after each pass completes. The same validation and runtime caveats apply.

### `EnableProcessingWorker`

| | |
|---|---|
| Type | `bool` |
| Default | `true` |
| Intended for | `BackgroundProcessingWorker` |

Intended to switch the background processing worker on or off. Nothing in the repository consults this flag. `WorkerCoordinator.StartAll()` always starts the processing worker. Code that wants to honor the flag must check it before calling `Start()` on the worker.

### `EnableMetricsWorker`

| | |
|---|---|
| Type | `bool` |
| Default | `true` |
| Intended for | `MetricsAggregationWorker` |

Intended to switch the metrics aggregation worker on or off. Same status as `EnableProcessingWorker`: it is not read anywhere, and `StartAll()` always starts the worker.

### `EnableHealthCheckWorker`

| | |
|---|---|
| Type | `bool` |
| Default | `true` |
| Intended for | `HealthCheckWorker` |

Intended to switch the health check worker on or off. Same status as the other two flags.

### `ToString()`

Returns a single line in this form:

```
WorkerOptions { MetricsAggregationIntervalMs = 5000, HealthCheckIntervalMs = 10000, EnableProcessingWorker = True, EnableMetricsWorker = True, EnableHealthCheckWorker = True }
```

Useful for logging the effective configuration at startup.

## Validation

Validation helpers are extension methods in `src/Configuration/EventServiceConfigurationValidation.cs` (namespace `DotNetRealtimePipeline.Configuration`).

| Method | Behavior |
|---|---|
| `IReadOnlyList<string> Validate(this WorkerOptions)` | Returns the list of problems; empty when valid. Throws `ArgumentNullException` for a null instance. |
| `bool IsValid(this WorkerOptions)` | `true` when `Validate` returns no problems. |
| `void EnsureValid(this WorkerOptions)` | Throws `ArgumentException` with all problems joined into the message. |

The rules are:

- `MetricsAggregationIntervalMs` must be greater than 0.
- `HealthCheckIntervalMs` must be greater than 0.

The `Enable*Worker` flags are not validated, since any `bool` is acceptable.

```csharp
var options = new WorkerOptions
{
    MetricsAggregationIntervalMs = 1_000,
    HealthCheckIntervalMs = 5_000,
};

options.EnsureValid();               // passes
Console.WriteLine(options);          // logs the effective values

var bad = new WorkerOptions { HealthCheckIntervalMs = 0 };
bool ok = bad.IsValid();             // false
IReadOnlyList<string> problems = bad.Validate();
// ["HealthCheckIntervalMs must be a positive integer (got 0)."]
```

## Relationship to the workers

Each worker takes its interval as a constructor argument. The values in `WorkerOptions` are not passed to them automatically.

| Worker (`src/Workers/BackgroundProcessingWorker.cs`) | Schedule | Effect |
|---|---|---|
| `BackgroundProcessingWorker` | No configurable interval. Loops with a fixed `Task.Delay(100)` (100 ms). On an exception it logs and waits 1000 ms before retrying. | Reads `PipelineOrchestrator.GetStatus()` each tick and logs at `Debug` when `PendingItemsInQueue > 0`. It does not drain the queue itself. |
| `MetricsAggregationWorker(MetricsService, ILogger, int intervalMs = 5000)` | `intervalMs`, default 5000. Waits after each pass. | Aggregates and logs metrics. On an exception the error is logged and the loop starts the next pass immediately, without waiting `intervalMs`. |
| `HealthCheckWorker(PipelineOrchestrator, ILogger, int intervalMs = 10000)` | `intervalMs`, default 10000. Waits after each pass. | Logs the health report. On an exception the error is logged and the next pass starts immediately, without waiting `intervalMs`. |
| `DynamicScalingWorker(DynamicScalingService, ILogger, int intervalMs = 5000)` | `intervalMs`, default 5000, clamped to at least 500 ms in the constructor. Waits after each pass. | Calls `DynamicScalingService.EvaluateScalingAsync`. Configured through `AddDynamicScaling(evaluationIntervalMs: ...)`, not through `WorkerOptions`. |

`WorkerCoordinator` (same file) groups the first three workers. `StartAll()` starts them, and `StopAllAsync()` stops them concurrently. `DynamicScalingWorker` is not part of the coordinator.

The defaults in `WorkerOptions` match the constructor defaults of `MetricsAggregationWorker` (5000 ms) and `HealthCheckWorker` (10000 ms), so the two are consistent. Changing one does not change the other.

## Example: wiring options into the workers

Until the workers read `WorkerOptions` directly, the wiring has to be done by hand. The example below honors the toggles in calling code:

```csharp
var options = new WorkerOptions { MetricsAggregationIntervalMs = 1_000 };
options.EnsureValid();

var processing = new BackgroundProcessingWorker(orchestrator, processingLogger);
var metrics = new MetricsAggregationWorker(metricsService, metricsLogger, options.MetricsAggregationIntervalMs);
var health = new HealthCheckWorker(orchestrator, healthLogger, options.HealthCheckIntervalMs);

var coordinator = processing.ToCoordinator(metrics, health, coordinatorLogger);
coordinator.StartAll();              // starts all three; the Enable* flags are not applied here
```

To honor the flags, call `Start()` only on the workers whose `Enable*Worker` property is `true`, and stop only those workers in shutdown code. `WorkerCoordinator` has no per-worker switch.

## Notes

- `WorkerOptions` is not thread-safe. Set its properties before passing it to code that runs workers.
- Values are not clamped. Unlike `DynamicScalingWorker`, the metrics and health workers do not raise small intervals to a minimum.
- An interval below `-1` makes `Task.Delay` throw `ArgumentOutOfRangeException`. The worker's `catch` logs it and, since the metrics and health loops do not wait on errors, retries at once, which produces a fast stream of error logs. Validate the options before starting the workers.
- Because the flags are unused, a configuration that disables a worker through `WorkerOptions` will still start that worker unless the calling code checks the flag.
