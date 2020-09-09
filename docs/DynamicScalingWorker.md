# DynamicScalingWorker

A long-lived background service that dynamically adjusts the number of concurrent processing tasks based on workload pressure. It monitors a backlog of work items and scales the active task count within configurable minimum and maximum bounds, aiming to maintain throughput without over-provisioning resources.

## API

### `DynamicScalingWorker`

Instantiates a new worker. The constructor accepts configuration parameters that govern scaling behavior, including the minimum and maximum concurrency levels, the polling interval, and the scaling threshold logic. Exact constructor parameters are determined by the internal dependency injection registration.

### `public void Start`

Begins the worker’s execution loop. Once started, the worker continuously evaluates the current workload and adjusts the active task count accordingly. This method is synchronous and returns immediately after initiating the background processing; the actual work runs on separate tasks. Calling `Start` on an already-running worker has no effect.

### `public async Task StopAsync`

Signals the worker to cease processing and returns a `Task` that completes when all in-flight work items have finished and associated resources have been released. This is a graceful shutdown: no new work is picked up after the stop signal is received, but items already being processed are allowed to complete. The returned `Task` should be awaited to ensure a clean exit.

### `public void Dispose`

Releases all managed and unmanaged resources held by the worker. If the worker is still running, `Dispose` will internally trigger a stop before disposing. Calling `Dispose` multiple times is safe; subsequent calls after the first have no effect. After disposal, the instance must not be reused.

### `public static IServiceCollection AddDynamicScaling`

An extension method on `IServiceCollection` that registers `DynamicScalingWorker` and its required dependencies into the dependency injection container. It returns the modified `IServiceCollection` to support fluent chaining. This method configures the worker’s lifetime (typically as a singleton or hosted service) and wires up any necessary options, monitors, or work-item providers.

## Usage

### Example 1: Hosted Service in an ASP.NET Application

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register the dynamic scaling worker with default options.
builder.Services.AddDynamicScaling(options =>
{
    options.MinConcurrency = 1;
    options.MaxConcurrency = 8;
    options.PollingInterval = TimeSpan.FromSeconds(2);
});

var app = builder.Build();

// The worker starts automatically as a hosted service.
app.Run();
```

### Example 2: Manual Lifetime Management in a Console Application

```csharp
var services = new ServiceCollection();
services.AddDynamicScaling();
var provider = services.BuildServiceProvider();

var worker = provider.GetRequiredService<DynamicScalingWorker>();

worker.Start();
Console.WriteLine("Worker started. Press any key to stop...");
Console.ReadKey();

await worker.StopAsync();
Console.WriteLine("Worker stopped gracefully.");

worker.Dispose();
```

## Notes

- **Thread safety:** `Start`, `StopAsync`, and `Dispose` use internal synchronization to prevent race conditions during state transitions. The scaling logic itself evaluates workload on a single timer-driven loop, avoiding concurrent modification of the active task count.
- **Idempotency:** Calling `Start` on an already-started worker is a no-op. `StopAsync` can be awaited safely even if the worker is already stopping or fully stopped. `Dispose` tolerates multiple invocations.
- **Graceful shutdown:** `StopAsync` does not abort in-flight work. Callers should account for the time required for the maximum number of concurrent tasks to drain when awaiting the returned `Task`.
- **Disposal order:** If both `StopAsync` and `Dispose` are called, `Dispose` internally ensures a stop is initiated. However, explicitly awaiting `StopAsync` before `Dispose` is recommended to avoid blocking the disposal path on long-running work.
- **Scaling boundaries:** The worker will never launch fewer than the configured minimum or more than the configured maximum tasks, regardless of workload spikes or idle periods.
- **Unobserved exceptions:** Exceptions thrown by individual work-processing tasks are handled internally and surfaced through the worker’s diagnostic channel (e.g., logging or health checks) rather than propagating to the caller of `Start` or `StopAsync`.
