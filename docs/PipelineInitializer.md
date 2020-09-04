# PipelineInitializer

`PipelineInitializer` orchestrates the sequential startup and shutdown of a real-time data pipeline. It provides asynchronous initialization, start, and stop operations with detailed outcome tracking, including success status, component counts, timing information, and error messages.

## API

### `public PipelineInitializer`

Constructs a new instance of the pipeline initializer. The constructor performs no I/O or allocation beyond creating the object itself; all substantive work is deferred to `InitializeAsync` and `StartAsync`.

### `public async Task<InitializationResult> InitializeAsync`

Prepares all pipeline components for operation. This method performs resource allocation, configuration validation, and dependency resolution across the pipeline stages.

- **Returns:** An `InitializationResult` containing the outcome of the initialization phase.
- **Throws:** `InvalidOperationException` if called when the pipeline is already initialized or running. May propagate exceptions from underlying component initialization failures.

### `public async Task<bool> StartAsync`

Activates the pipeline after successful initialization, beginning data flow through all components.

- **Returns:** `true` if the pipeline started successfully; `false` if the pipeline was not in a valid state to start (e.g., not yet initialized or already running).
- **Throws:** `ObjectDisposedException` if the pipeline has been disposed. May propagate exceptions from component start failures.

### `public async Task<bool> StopAsync`

Gracefully halts data flow and deactivates all pipeline components. Components are stopped in reverse initialization order.

- **Returns:** `true` if the pipeline stopped successfully; `false` if the pipeline was already stopped or not running.
- **Throws:** `ObjectDisposedException` if the pipeline has been disposed. May propagate exceptions from component stop failures.

### `public bool Success`

Gets a value indicating whether the most recent operation (`InitializeAsync`, `StartAsync`, or `StopAsync`) completed without errors. This property reflects the outcome of the last invoked lifecycle method.

### `public int ComponentsInitialized`

Gets the number of pipeline components that were successfully initialized during the last `InitializeAsync` call. A value of zero after initialization indicates either no components were registered or initialization failed before any component completed.

### `public string ErrorMessage`

Gets the error message from the most recent failed operation. Returns `null` or an empty string if the last operation succeeded. The message captures the first encountered failure during initialization, start, or stop.

### `public DateTime StartTime`

Gets the UTC timestamp when the most recent lifecycle operation began. This is set at the entry point of `InitializeAsync`, `StartAsync`, or `StopAsync`.

### `public DateTime EndTime`

Gets the UTC timestamp when the most recent lifecycle operation completed, regardless of success or failure. This is set immediately before the method returns.

### `public override string ToString`

Returns a string representation of the current pipeline state, including the success status, number of initialized components, any error message, and the start/end timestamps of the last operation.

## Usage

### Example 1: Full lifecycle with error handling

```csharp
var initializer = new PipelineInitializer();

// Initialize
var initResult = await initializer.InitializeAsync();
if (!initializer.Success)
{
    Console.WriteLine($"Initialization failed: {initializer.ErrorMessage}");
    Console.WriteLine($"Components initialized before failure: {initializer.ComponentsInitialized}");
    return;
}

Console.WriteLine($"Initialized {initializer.ComponentsInitialized} components");

// Start
bool started = await initializer.StartAsync();
if (!started || !initializer.Success)
{
    Console.WriteLine($"Start failed: {initializer.ErrorMessage}");
    await initializer.StopAsync(); // Attempt cleanup
    return;
}

Console.WriteLine($"Pipeline running. Started at {initializer.StartTime:O}");

// Stop when done
bool stopped = await initializer.StopAsync();
Console.WriteLine($"Pipeline stopped: {stopped}. Ended at {initializer.EndTime:O}");
```

### Example 2: Conditional restart with state inspection

```csharp
var initializer = new PipelineInitializer();

await initializer.InitializeAsync();

if (initializer.ComponentsInitialized < 3)
{
    Console.WriteLine($"Insufficient components: {initializer.ComponentsInitialized}. Aborting.");
    return;
}

bool started = await initializer.StartAsync();
if (!started)
{
    // Check if already running from a previous attempt
    Console.WriteLine(initializer.ToString());
    await initializer.StopAsync();
    
    // Retry start
    started = await initializer.StartAsync();
    Console.WriteLine($"Retry start result: {started}, Success: {initializer.Success}");
}
```

## Notes

- **Operation ordering:** `InitializeAsync` must complete successfully before `StartAsync` can succeed. Calling `StartAsync` before initialization returns `false` without modifying state. `StopAsync` can be called at any time but only has effect when the pipeline is running.
- **Idempotency:** `StartAsync` returns `false` if already running; `StopAsync` returns `false` if already stopped. Neither throws in these cases.
- **State properties:** `Success`, `ComponentsInitialized`, `ErrorMessage`, `StartTime`, and `EndTime` are overwritten by each subsequent lifecycle call. Read these values immediately after the operation of interest before invoking another method.
- **Thread safety:** Instance members are not thread-safe. Concurrent calls to `InitializeAsync`, `StartAsync`, or `StopAsync` on the same instance will result in race conditions and undefined behavior. Synchronize external access if multiple threads may interact with a single `PipelineInitializer`.
- **Partial initialization:** If `InitializeAsync` fails partway through, `ComponentsInitialized` reflects the count of components that completed initialization before the failure. The pipeline cannot be started in this state; re-initialization is required.
- **Exception propagation:** Exceptions from underlying components are surfaced through the async methods. The `ErrorMessage` property captures the message of the first such exception; callers should inspect both the exception and the property for diagnostic details.
