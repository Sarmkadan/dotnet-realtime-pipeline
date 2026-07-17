# WindowingServiceExtensions

The `WindowingServiceExtensions` class provides a set of static utility methods designed to manage the lifecycle, state, and statistical aggregation of time-based windows within the real-time data processing pipeline. It serves as the primary interface for creating custom window definitions, processing incoming data points against existing window states, retrieving active or completed windows, and generating combined statistical summaries without maintaining internal mutable state itself.

## API

### `CreateCustomDurationWindow`
```csharp
public static WindowEvent CreateCustomDurationWindow(...)
```
Initializes a new `WindowEvent` instance with a user-defined duration that may differ from standard pipeline configuration intervals. This method constructs the window metadata, including start and end timestamps and a unique identifier, preparing it for ingestion into the processing stream.
*   **Parameters**: Accepts configuration arguments defining the window's temporal bounds and context (specific parameters depend on the overload used for duration and context injection).
*   **Returns**: A fully initialized `WindowEvent` object ready for processing.
*   **Throws**: Throws an `ArgumentException` if the specified duration is non-positive or if the temporal bounds are invalid (e.g., end time precedes start time).

### `ProcessDataPointsWithState`
```csharp
public static (IReadOnlyList<WindowEmissionResult> Emitted, IReadOnlyList<WindowEvent> Active) ProcessDataPointsWithState(...)
```
Executes the core windowing logic by applying a sequence of data points to a collection of active windows. It evaluates whether data points trigger window closures based on watermarks or completion criteria.
*   **Parameters**: Requires the current list of active `WindowEvent` instances and the incoming sequence of data points to be processed.
*   **Returns**: A tuple containing two lists: `Emitted`, representing `WindowEmissionResult` objects for windows that have closed and are ready for downstream consumption, and `Active`, representing the updated list of windows that remain open.
*   **Throws**: Throws an `ArgumentNullException` if the input lists or data sequences are null.

### `CalculateCombinedWindowStatistics`
```csharp
public static WindowStatistics CalculateCombinedWindowStatistics(...)
```
Aggregates statistical metrics across a provided collection of `WindowEvent` instances to produce a single summary object. This is useful for generating high-level health metrics or batch reporting on a group of windows.
*   **Parameters**: Takes an `IEnumerable<WindowEvent>` containing the windows to analyze.
*   **Returns**: A `WindowStatistics` object containing aggregated counts, average durations, and other cumulative metrics.
*   **Throws**: Throws an `ArgumentNullException` if the input collection is null. Returns a zero-valued statistics object if the collection is empty.

### `GetCompleteWindows`
```csharp
public static IEnumerable<WindowEvent> GetCompleteWindows(...)
```
Filters a provided collection of windows to return only those that have met their completion criteria (e.g., watermark passed or duration expired) but have not yet been emitted.
*   **Parameters**: Accepts the current state of windows to evaluate.
*   **Returns**: An `IEnumerable<WindowEvent>` containing only the completed windows.
*   **Throws**: Throws an `ArgumentNullException` if the input collection is null.

### `GetActiveWindows`
```csharp
public static IReadOnlyList<WindowEvent> GetActiveWindows(...)
```
Retrieves a read-only list of windows that are currently open and accepting data points. This method filters out any windows that have already been marked as complete or emitted.
*   **Parameters**: Accepts the master list of windows to filter.
*   **Returns**: An `IReadOnlyList<WindowEvent>` containing only active windows.
*   **Throws**: Throws an `ArgumentNullException` if the input collection is null.

### `GetNextWindowId`
```csharp
public static long GetNextWindowId(...)
```
Generates the next unique sequential identifier for a new window instance. This ensures global uniqueness of window IDs across the pipeline execution context.
*   **Parameters**: Typically requires a reference to the current ID counter or state holder.
*   **Returns**: A `long` representing the next available unique ID.
*   **Throws**: Throws an `OverflowException` if the internal counter exceeds `long.MaxValue`.

## Usage

### Example 1: Processing a Batch of Data Points
This example demonstrates how to maintain window state across multiple batches of incoming data, emitting results only when windows close.

```csharp
// Initialize with an empty list of active windows
var activeWindows = new List<WindowEvent>();
var newDataPoints = GetDataStreamBatch();

// Process the new data against existing windows
var result = WindowingServiceExtensions.ProcessDataPointsWithState(activeWindows, newDataPoints);

// Handle emitted windows (e.g., send to sink)
foreach (var emission in result.Emitted)
{
    SinkService.Write(emission);
}

// Update the local state with the remaining active windows
activeWindows = result.Active.ToList();
```

### Example 2: Generating Aggregate Statistics for Monitoring
This example shows how to calculate combined statistics for all currently active windows to report on system load or latency.

```csharp
var currentWindows = WindowingServiceExtensions.GetActiveWindows(allTrackedWindows);

if (currentWindows.Any())
{
    var stats = WindowingServiceExtensions.CalculateCombinedWindowStatistics(currentWindows);
    
    Console.WriteLine($"Active Windows: {stats.TotalCount}");
    Console.WriteLine($"Average Window Duration: {stats.AverageDurationMs}ms");
}
```

## Notes

*   **Thread Safety**: As a static extension class containing pure functions that operate on passed-in collections, `WindowingServiceExtensions` itself does not maintain internal mutable state. However, the collections passed to methods like `ProcessDataPointsWithState` must be managed by the caller. If multiple threads access the same list of `WindowEvent` objects concurrently, external synchronization (e.g., `lock` statements) is required around the method calls and the subsequent state updates.
*   **Immutability**: Methods returning `IReadOnlyList<T>` ensure that the internal structure of the returned window collections cannot be modified directly by the consumer, enforcing that state changes only occur through explicit reassignment or dedicated processing methods.
*   **ID Overflow**: The `GetNextWindowId` method relies on a sequential counter. In extremely long-running processes with high window churn, callers should be prepared to handle potential `OverflowException` scenarios, although this is unlikely in standard operational timeframes.
*   **Empty Collections**: Passing empty collections to statistical or filtering methods will not throw exceptions but will return empty results or zero-valued statistics objects, allowing for safe usage in initialization phases.
