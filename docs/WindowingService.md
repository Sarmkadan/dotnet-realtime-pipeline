# WindowingService

The `WindowingService` class provides a framework for managing time-based or count-based windows over streaming data points. It supports creating windows, adding data points, aggregating values, computing statistics, emitting completed windows, and merging windows. The service maintains per-instance state for the current window, including running aggregates and metadata such as window duration and throughput.

## API

### `public WindowingService()`
Initializes a new instance of the `WindowingService`. The service starts with no active window; a window must be explicitly created before data points can be added.

### `public WindowEvent CreateWindow(/* parameters */)`
Creates a new window with the specified configuration (e.g., window size, duration, key).  
**Parameters:** Accepts parameters that define the window boundaries and identity.  
**Returns:** A `WindowEvent` representing the newly created window.  
**Throws:** `InvalidOperationException` if a window is already active and cannot be replaced without explicit emission or disposal.

### `public bool TryAddDataPointToWindow(/* dataPoint */)`
Attempts to add a data point to the current window.  
**Parameters:** The data point to add (type depends on implementation, e.g., `double`, `object`).  
**Returns:** `true` if the data point was successfully added; `false` if the window is complete or closed.  
**Throws:** `InvalidOperationException` if no window has been created.

### `public List<WindowEmissionResult> ProcessDataPoints(/* dataPoints */)`
Processes a batch of data points, adding them to the current window and returning any completed window emissions.  
**Parameters:** A collection of data points.  
**Returns:** A list of `WindowEmissionResult` objects, one per emitted window (if any).  
**Throws:** `ArgumentNullException` if the input collection is null.

### `public Dictionary<string, object> AggregateWindow(/* window */)`
Computes a set of named aggregations (e.g., sum, count, custom functions) for the specified window.  
**Parameters:** The window to aggregate (typically a `WindowEvent` or window identifier).  
**Returns:** A dictionary mapping aggregation names to their computed values.  
**Throws:** `ArgumentException` if the window is invalid or not found.

### `public WindowStatistics CalculateWindowStatistics(/* window */)`
Calculates statistical measures (mean, standard deviation, min, max, etc.) for the data points in the specified window.  
**Parameters:** The window to analyze.  
**Returns:** A `WindowStatistics` object containing the computed statistics.  
**Throws:** `InvalidOperationException` if the window contains fewer than two data points (for standard deviation).

### `public bool IsWindowComplete(/* window */)`
Determines whether the specified window has met its completion criteria (e.g., elapsed time, data point count).  
**Parameters:** The window to check.  
**Returns:** `true` if the window is complete; otherwise `false`.

### `public WindowEmissionResult EmitWindow(/* window */)`
Forces emission of the specified window, finalizing its data and triggering downstream processing.  
**Parameters:** The window to emit.  
**Returns:** A `WindowEmissionResult` containing the emitted window data and metadata.  
**Throws:** `InvalidOperationException` if the window has already been emitted.

### `public WindowEvent MergeWindows(WindowEvent first, WindowEvent second)`
Merges two windows into a single window, combining their data points and recomputing aggregates.  
**Parameters:**  
- `first`: The first window to merge.  
- `second`: The second window to merge.  
**Returns:** A new `WindowEvent` representing the merged window.  
**Throws:** `ArgumentException` if the windows have incompatible configurations (e.g., different keys or durations).

### `public WindowingSummary GetWindowingSummary()`
Returns a summary of the current windowing state, including total windows created, emitted, and current throughput metrics.  
**Returns:** A `WindowingSummary` object.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `WindowId` | `long` | The unique identifier of the current active window. |
| `DataPointCount` | `int` | The number of data points added to the current window. |
| `Sum` | `double` | The running sum of all data point values in the current window. |
| `Average` | `double` | The arithmetic mean of data point values in the current window. |
| `Min` | `double` | The minimum data point value in the current window. |
| `Max` | `double` | The maximum data point value in the current window. |
| `StdDev` | `double` | The population standard deviation of data point values in the current window. |
| `WindowDurationMs` | `long` | The configured duration (in milliseconds) of the current window. |
| `Throughput` | `double` | The rate of data points processed per second for the current window. |

## Usage

### Example 1: Basic window creation and emission

```csharp
var service = new WindowingService();
var window = service.CreateWindow(durationMs: 5000); // 5-second window

// Add data points
service.TryAddDataPointToWindow(10.5);
service.TryAddDataPointToWindow(20.3);
service.TryAddDataPointToWindow(15.8);

// Check completion and emit
if (service.IsWindowComplete(window))
{
    var result = service.EmitWindow(window);
    Console.WriteLine($"Emitted window {result.WindowId} with {result.DataPointCount} points.");
}
```

### Example 2: Batch processing and merging

```csharp
var service = new WindowingService();

// Process a batch of data points
var dataPoints = new List<double> { 1.0, 2.0, 3.0, 4.0, 5.0 };
var emissions = service.ProcessDataPoints(dataPoints);

// Merge two windows (if needed)
var windowA = service.CreateWindow(durationMs: 1000);
var windowB = service.CreateWindow(durationMs: 1000);
// ... add data to each ...
var merged = service.MergeWindows(windowA, windowB);

// Get summary
var summary = service.GetWindowingSummary();
Console.WriteLine($"Total windows created: {summary.TotalCreated}");
```

## Notes

- **Thread safety:** The `WindowingService` is not thread-safe. Concurrent access from multiple threads must be synchronized externally (e.g., via locks). The class is designed for single-threaded or serialized usage.
- **Edge cases:**  
  - Calling `TryAddDataPointToWindow` before `CreateWindow` throws `InvalidOperationException`.  
  - `CalculateWindowStatistics` requires at least two data points; otherwise it throws.  
  - `EmitWindow` on an already emitted window throws; use `IsWindowComplete` to check before emitting.  
  - `MergeWindows` expects windows with compatible configurations; merging windows of different durations or keys results in an `ArgumentException`.  
  - Properties such as `Average`, `StdDev`, and `Throughput` return `double.NaN` or `0` when no data points have been added, depending on the implementation.
- **State management:** The service maintains a single active window. Creating a new window without emitting the previous one may overwrite state; callers should ensure proper lifecycle management.
