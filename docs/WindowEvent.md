# WindowEvent

Represents a time-bounded aggregation window that collects `DataPoint` instances over a specified interval. The type tracks window identity, temporal boundaries, aggregation strategy, and completion state, and exposes methods for adding data points, computing statistical aggregates, and finalizing the window.

## API

### Constructors

- **`WindowEvent()`**  
  Parameterless constructor. Initializes a new, empty window with default values. The window is not complete upon construction.

- **`WindowEvent(long windowId, long windowStartMs, long windowEndMs, string aggregationType, string? description = null)`**  
  Creates a window with the given identity, time range, and aggregation type.  
  *Parameters*:  
  `windowId` — Unique identifier for this window.  
  `windowStartMs` — Inclusive start of the window in milliseconds.  
  `windowEndMs` — Exclusive end of the window in milliseconds.  
  `aggregationType` — A string label denoting the aggregation strategy (e.g., `"Average"`, `"Sum"`).  
  `description` — Optional human-readable description.  
  *Throws*: `ArgumentNullException` when `aggregationType` is null.

### Properties

- **`long WindowId`**  
  Unique identifier of the window.

- **`long WindowStartMs`**  
  Inclusive start boundary of the window, expressed in milliseconds.

- **`long WindowEndMs`**  
  Exclusive end boundary of the window, expressed in milliseconds.

- **`string AggregationType`**  
  Label indicating the aggregation strategy applied to data points within this window.

- **`List<DataPoint> DataPoints`**  
  The collection of data points currently held by the window. Returns the internal list reference; modifications affect the window state.

- **`DateTime CreatedAt`**  
  UTC timestamp of when the window instance was created.

- **`string? Description`**  
  Optional human-readable description. May be null.

- **`bool IsComplete`**  
  Indicates whether the window has been finalized. Once true, further data point additions should be rejected.

- **`long CreatedAtTicks`**  
  Creation timestamp expressed as ticks (100-nanosecond intervals since `DateTime.MinValue`).

### Methods

- **`long GetDurationMs()`**  
  Returns the configured duration of the window in milliseconds (`WindowEndMs - WindowStartMs`).  
  *Returns*: Non-negative `long` representing the window span.

- **`int GetDataPointCount()`**  
  Returns the number of data points currently in the window.  
  *Returns*: Zero or positive integer.

- **`bool TryAddDataPoint(DataPoint dataPoint)`**  
  Attempts to add a data point to the window.  
  *Parameters*: `dataPoint` — The data point to add. Must not be null.  
  *Returns*: `true` if the point was accepted and added; `false` if the window is already complete or the point falls outside the window’s time boundaries.  
  *Throws*: `ArgumentNullException` when `dataPoint` is null.

- **`double CalculateAverage()`**  
  Computes the arithmetic mean of all values in `DataPoints`.  
  *Returns*: The average, or `0.0` when the window is empty.  
  *Throws*: `InvalidOperationException` if `AggregationType` does not support this calculation (implementation-defined).

- **`double CalculateSum()`**  
  Computes the sum of all values in `DataPoints`.  
  *Returns*: The total sum, or `0.0` when the window is empty.  
  *Throws*: `InvalidOperationException` if `AggregationType` does not support this calculation.

- **`double CalculateMin()`**  
  Returns the minimum value among `DataPoints`.  
  *Returns*: The smallest value, or `double.MaxValue` when the window is empty.  
  *Throws*: `InvalidOperationException` if `AggregationType` does not support this calculation.

- **`double CalculateMax()`**  
  Returns the maximum value among `DataPoints`.  
  *Returns*: The largest value, or `double.MinValue` when the window is empty.  
  *Throws*: `InvalidOperationException` if `AggregationType` does not support this calculation.

- **`double CalculateStandardDeviation()`**  
  Computes the population standard deviation of values in `DataPoints`.  
  *Returns*: The standard deviation, or `0.0` when the window contains fewer than two data points.  
  *Throws*: `InvalidOperationException` if `AggregationType` does not support this calculation.

- **`void MarkComplete()`**  
  Finalizes the window, setting `IsComplete` to `true`. After this call, `TryAddDataPoint` returns `false` for any subsequent addition attempts. This operation is irreversible for the instance.

## Usage

### Example 1: Building and finalizing a tumbling window

```csharp
var window = new WindowEvent(
    windowId: 42,
    windowStartMs: 0,
    windowEndMs: 60_000,
    aggregationType: "Average",
    description: "One-minute tumbling window"
);

// Add data points that fall within [0, 60000)
window.TryAddDataPoint(new DataPoint { TimestampMs = 10_000, Value = 12.5 });
window.TryAddDataPoint(new DataPoint { TimestampMs = 30_000, Value = 18.3 });
window.TryAddDataPoint(new DataPoint { TimestampMs = 50_000, Value = 9.7 });

// A point outside the window is rejected
bool added = window.TryAddDataPoint(new DataPoint { TimestampMs = 65_000, Value = 22.0 });
// added == false

window.MarkComplete();

double avg = window.CalculateAverage();   // ~13.5
int count = window.GetDataPointCount();   // 3
```

### Example 2: Using multiple aggregation calculations on a sum window

```csharp
var window = new WindowEvent(
    windowId: 100,
    windowStartMs: 1_000,
    windowEndMs: 11_000,
    aggregationType: "Sum"
);

window.TryAddDataPoint(new DataPoint { TimestampMs = 2_000, Value = 10.0 });
window.TryAddDataPoint(new DataPoint { TimestampMs = 5_000, Value = 20.0 });
window.TryAddDataPoint(new DataPoint { TimestampMs = 9_000, Value = 30.0 });

double sum = window.CalculateSum();                // 60.0
double min = window.CalculateMin();                // 10.0
double max = window.CalculateMax();                // 30.0
double stdDev = window.CalculateStandardDeviation(); // ~8.16

window.MarkComplete();
// Further additions will be rejected
bool retry = window.TryAddDataPoint(new DataPoint { TimestampMs = 3_000, Value = 40.0 });
// retry == false
```

## Notes

- **Empty window behavior**: All statistical methods (`CalculateAverage`, `CalculateSum`, `CalculateMin`, `CalculateMax`, `CalculateStandardDeviation`) handle an empty `DataPoints` collection without throwing. They return neutral values: `0.0` for average, sum, and standard deviation; `double.MaxValue` for min; `double.MinValue` for max. Callers should check `GetDataPointCount()` before relying on results.
- **Standard deviation details**: `CalculateStandardDeviation` returns `0.0` when fewer than two data points are present, as population standard deviation is undefined for a single value. The implementation assumes population variance (division by N, not N-1).
- **AggregationType guard**: Statistical methods may throw `InvalidOperationException` if the window’s `AggregationType` is incompatible with the requested calculation. The exact set of allowed types per method is implementation-defined; callers should either know the window’s type or catch the exception.
- **Time boundary semantics**: `TryAddDataPoint` rejects points whose timestamp is strictly less than `WindowStartMs` or greater than or equal to `WindowEndMs`. The start is inclusive, the end is exclusive.
- **Thread safety**: This type is not thread-safe. Concurrent calls to `TryAddDataPoint`, `MarkComplete`, or any calculation method from multiple threads may lead to race conditions, data corruption, or inconsistent state. External synchronization is required when sharing an instance across threads.
- **Irreversible completion**: `MarkComplete` permanently sets `IsComplete`. There is no mechanism to reopen a window. Create a new instance if a new window with the same boundaries is needed.
- **DataPoints list exposure**: The `DataPoints` property returns the internal `List<DataPoint>` directly. External modifications to the list (add, remove, clear) bypass `TryAddDataPoint` validation and may violate window invariants. Prefer using `TryAddDataPoint` for all additions.
