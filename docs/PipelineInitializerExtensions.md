# PipelineInitializerExtensions

`PipelineInitializerExtensions` provides convenience methods for initializing, starting, retrying, stopping, and inspecting a `PipelineInitializer`. For the underlying lifecycle API and construction details, see [`PipelineInitializer`](PipelineInitializer.md).

All five methods throw `ArgumentNullException` when `initializer` is `null`.

## `InitializeAndStartAsync`

```csharp
public static Task<InitializationResult> InitializeAndStartAsync(
    this PipelineInitializer initializer)
```

Initializes the pipeline and, when initialization succeeds, starts it automatically. If initialization succeeds but `StartAsync` returns `false`, the returned result is changed to a failed result with the error message `Initialization succeeded but automatic start failed`.

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `initializer` | `PipelineInitializer` | None | The initializer to initialize and start. |

Returns the `InitializationResult` produced by initialization, updated to report failure if the automatic start fails.

## `InitializeWithRetryAsync`

```csharp
public static Task<InitializationResult> InitializeWithRetryAsync(
    this PipelineInitializer initializer,
    int maxAttempts = 3,
    int delayBetweenAttempts = 1000)
```

Calls `InitializeAsync` until initialization succeeds or the maximum number of attempts is reached. The delay is applied only between failed attempts, not after the final attempt.

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `initializer` | `PipelineInitializer` | None | The initializer on which to attempt initialization. |
| `maxAttempts` | `int` | `3` | Maximum number of initialization attempts. Must be at least `1`. |
| `delayBetweenAttempts` | `int` | `1000` | Delay between attempts, in milliseconds. Must be non-negative. |

Returns the first successful `InitializationResult`, or the result from the final failed attempt. It throws `ArgumentOutOfRangeException` when `maxAttempts` is less than `1` or `delayBetweenAttempts` is negative.

## `SafeStopAsync`

```csharp
public static Task<bool> SafeStopAsync(this PipelineInitializer initializer)
```

Attempts to stop the pipeline. Exceptions raised while calling `StopAsync` are intentionally caught and converted to a `false` result, making the method suitable for best-effort cleanup.

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `initializer` | `PipelineInitializer` | None | The initializer whose pipeline should be stopped. |

Returns the value returned by `StopAsync`, or `false` if stopping throws an exception.

## `IsInitialized`

```csharp
public static bool IsInitialized(this PipelineInitializer initializer)
```

Reads the initializer's current initialization status.

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `initializer` | `PipelineInitializer` | None | The initializer to inspect. |

Returns `true` when the pipeline has been initialized; otherwise, `false`.

## `GetPipelineState`

```csharp
public static string GetPipelineState(this PipelineInitializer initializer)
```

Provides a short, display-oriented representation of the initialization status.

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `initializer` | `PipelineInitializer` | None | The initializer whose state should be described. |

Returns `"Initialized"` when the pipeline has been initialized, or `"Not Initialized"` otherwise.

## Example

The following example retries initialization, inspects the resulting state, starts the pipeline, and performs best-effort cleanup. It assumes `initializer` has already been created with the services described in the [`PipelineInitializer` documentation](PipelineInitializer.md).

```csharp
using DotNetRealtimePipeline.Initialization;

InitializationResult result = await initializer.InitializeWithRetryAsync(
    maxAttempts: 3,
    delayBetweenAttempts: 500);

if (!result.Success)
{
    Console.WriteLine($"Initialization failed: {result.ErrorMessage}");
    return;
}

Console.WriteLine(initializer.GetPipelineState()); // "Initialized"
Console.WriteLine($"Initialized: {initializer.IsInitialized()}");

bool started = await initializer.StartAsync();
if (!started)
{
    Console.WriteLine("The pipeline could not be started.");
    return;
}

try
{
    // Use the running pipeline.
}
finally
{
    bool stopped = await initializer.SafeStopAsync();
    Console.WriteLine($"Stopped cleanly: {stopped}");
}
```

When retry behavior is not needed, `InitializeAndStartAsync` combines initialization and startup into one call:

```csharp
InitializationResult result = await initializer.InitializeAndStartAsync();
if (!result.Success)
{
    Console.WriteLine(result.ErrorMessage);
}
```
