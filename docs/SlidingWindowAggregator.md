# SlidingWindowAggregator

Provides incremental aggregation of numeric data points over a sliding time window, enabling real‑time calculation of statistics such as sum, average, min, max, and trend for windows that become due according to a configurable step interval.

## API

### `SlidingWindowAggregator()`
Initializes a new instance with default window parameters. The instance is ready to accept data points via `Add` or `AddRange`.

### `void Add(double value)`
Adds a single data point to the aggregator.

- **Parameters**  
  - `value`: The numeric value to incorporate into the current window calculations.
- **Return value**  
  - None.
- **Exceptions**  
  - Throws `ArgumentException` if `value` is not a finite number.  
  - Throws `InvalidOperationException` if the aggregator has been disposed or is in an invalid state.

### `void AddRange(IEnumerable<double> values)`
Adds a sequence of data points to the aggregator.

- **Parameters**  
  - `values`: The collection of numeric values to add.
- **Return value**  
  - None.
- **Exceptions**  
  - Throws `ArgumentNullException` if `values` is `null`.  
  - Throws `ArgumentException` if any element in `values` is not a finite number.  
  - Throws `InvalidOperationException` if the aggregator has been disposed or is in an invalid state.

### `IReadOnlyList<SlidingWindowResult> FlushDueWindows()`
Returns the results for all windows that have elapsed based on the configured step interval and window size.

- **Parameters**  
  - None.
- **Return value**  
  - A read‑only list containing `SlidingWindowResult` objects for each due window. The list may be empty if no windows are ready.
- **Exceptions**  
  - Throws `InvalidOperationException` if the aggregator has been disposed.

### `IReadOnlyList<SlidingWindowResult> FlushDueWindows(bool clear)`
Returns the results for all due windows and optionally clears the internal state for those windows.

- **Parameters**  
  - `clear`: When `true`, the data for the returned windows is removed from the aggregator; when `false`, the aggregator retains the data.
- **Return value**  
  - A read‑only list containing `SlidingWindowResult` objects for each due window. The list may be empty if no windows are ready.
- **Exceptions**  
  - Throws `InvalidOperationException` if the aggregator has been disposed.

### `long WindowId`
Gets the identifier of the most recently completed window.

- **Return value**  
  - A monotonically increasing long value; zero if no window has been completed yet.

### `long WindowStartMs`
Gets the start timestamp (in milliseconds since Unix epoch) of the most recently completed window.

- **Return value**  
  - The start time; zero if no window has been completed yet.

### `long WindowEndMs`
Gets the end timestamp (in milliseconds since Unix epoch) of the most recently completed window.

- **Return value**  
  - The end time; zero if no window has been completed yet.

### `long WindowSizeMs`
Gets the size of the sliding window in milliseconds.

- **Return value**  
  - The configured window size; never negative.

### `long StepIntervalMs`
Gets the step interval in milliseconds that determines how often the window advances.

- **Return value**  
  - The configured step interval; never negative.

### `int DataPointCount`
Gets the number of data points that have been added to the current (in‑progress) window.

- **Return value**  
  - Zero if no data points have been added to the current window.

### `double Average`
Gets the arithmetic mean of the data points in the current window.

- **Return value**  
  - `0.0` if `DataPointCount` is zero.

### `double Sum`
Gets the sum of the data points in the current window.

- **Return value**  
  - `0.0` if `DataPointCount` is zero.

### `double Min`
Gets the minimum value observed in the current window.

- **Return value**  
  - `double.PositiveInfinity` if `DataPointCount` is zero.

### `double Max`
Gets the maximum value observed in the current window.

- **Return value**  
  - `double.NegativeInfinity` if `DataPointCount` is zero.

### `double Trend`
Gets a simple linear trend estimate (slope) for the data points in the current window.

- **Return value**  
  - `0.0` if fewer than two data points are present.

### `DateTime EmittedAt`
Gets the UTC timestamp when the most recent window result was emitted (i.e., when `FlushDueWindows` last returned a non‑empty list).

- **Return value**  
  - `DateTime.MinValue` if no window has been emitted yet.

### `Dictionary<string, object> AggregatedData`
Gets a dictionary containing all currently computed aggregates for the current window.

- **Return value**  
  - A read‑only dictionary with keys such as `"Average"`, `"Sum"`, `"Min"`, `"Max"`, `"Trend"`, `"DataPointCount"`, `"WindowStartMs"`, `"WindowEndMs"`, and `"WindowId"`. The dictionary is empty if no data points have been added.

## Usage

### Example 1: Basic sliding window aggregation
```csharp
using System;
using System.Collections.Generic;
using RealtimePipeline; // namespace containing SlidingWindowAggregator

var aggregator = new SlidingWindowAggregator
{
    WindowSizeMs = 10_000,   // 10‑second window
    StepIntervalMs = 2_000   // slide every 2 seconds
};

// Simulate incoming data points
var rnd = new Random();
for (int i = 0; i < 50; i++)
{
    double value = rnd.NextDouble() * 100;
    aggregator.Add(value);

    // Check for due windows every few additions
    if (i % 5 == 0)
    {
        var results = aggregator.FlushDueWindows();
        foreach (var res in results)
        {
            Console.WriteLine($"Window {res.WindowId}: Avg={res.Average:F2}, Sum={res.Sum:F2}");
        }
    }
}
```

### Example 2: Bulk addition and manual flush
```csharp
using System;
using System.Linq;
using RealtimePipeline;

var agg = new SlidingWindowAggregator();

// Add a batch of sensor readings
double[] readings = Enumerable.Range(0, 100).Select(i => Math.Sin(i * 0.1) * 50 + 50).ToArray();
agg.AddRange(readings);

// Flush all windows that have become due and clear their state
var due = agg.FlushDueWindows(clear: true);
Console.WriteLine($"Flushed {due.Count} windows.");
if (due.Count > 0)
{
    var last = due.Last();
    Console.WriteLine($"Last window emitted at: {agg.EmittedAt:O}");
    Console.WriteLine($"Aggregates: {string.Join(", ", agg.AggregatedData.Select(kv => $"{kv.Key}={kv.Value}"))}");
}
```

## Notes

- The aggregator is **not thread‑safe**. Concurrent calls to `Add`, `AddRange`, or `FlushDueWindows` from multiple threads must be synchronized externally.
- `FlushDueWindows` with `clear:false` returns the due window results without removing their data; subsequent calls may return the same results until `clear:true` is used or new data shifts the window.
- If no data has been added to the current window, the aggregate properties (`Average`, `Sum`, `Min`, `Max`, `Trend`) return their default neutral values as documented.
- The `WindowId`, `StartMs`, and `EndMs` properties reflect the most recently **completed** window; while a window is in progress they retain the values of the previous window.
- The `AggregatedData` dictionary is recomputed on each property access; frequent enumeration may incur allocation overhead. For performance‑critical scenarios, consider reading the individual properties directly.
