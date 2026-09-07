# WindowEventExtensions

Provides analysis and formatting extension methods for [`WindowEvent`](WindowEvent.md). Import `DotNetRealtimePipeline.Domain.Models` to call these methods with instance-method syntax.

## API

### `TimeSpan GetDuration()`

Returns the configured window length as a `TimeSpan`. The value is calculated from `WindowEvent.GetDurationMs()` and therefore represents `WindowEndMs - WindowStartMs` milliseconds.

Throws `ArgumentNullException` when the `WindowEvent` is `null`.

### `IReadOnlyList<DataPoint> GetDataPointsSortedByTimestamp()`

Returns the window's data points ordered by `DataPoint.Timestamp` in ascending order. The result is a new read-only list; sorting it does not reorder the original `DataPoints` collection. Points with equal timestamps retain their original relative order.

Throws `ArgumentNullException` when the `WindowEvent` or its `DataPoints` collection is `null`.

### `double GetPercentile(double percentile)`

Calculates a percentile from the data point values. `percentile` must be between `0` and `100`, inclusive. Values are sorted in ascending order, and a percentile falling between two values is calculated using linear interpolation with the rank:

```text
(percentile / 100) * (count - 1)
```

Consequently, percentile `0` returns the minimum and percentile `100` returns the maximum. An empty window returns `0.0`.

Throws `ArgumentNullException` when the `WindowEvent` is `null`, and `ArgumentOutOfRangeException` when `percentile` is below `0` or above `100`.

### `string ToSummaryString()`

Returns a concise, culture-invariant summary containing the window ID, start and end timestamps, duration in milliseconds, point count, average, minimum, maximum, and population standard deviation. Statistical values are formatted with two decimal places:

```text
Window 7 [1000-5000] (4000 ms): Count=4, Avg=25.00, Min=10.00, Max=40.00, StdDev=11.18
```

Throws `ArgumentNullException` when the `WindowEvent` is `null`.

## Example

```csharp
using DotNetRealtimePipeline.Domain.Models;

var window = new WindowEvent(
    windowId: 7,
    startMs: 1_000,
    endMs: 5_000,
    aggregationType: "tumbling");

window.TryAddDataPoint(new DataPoint(1, 4_000, 40.0, "sensor-a"));
window.TryAddDataPoint(new DataPoint(2, 1_500, 10.0, "sensor-a"));
window.TryAddDataPoint(new DataPoint(3, 3_000, 30.0, "sensor-a"));
window.TryAddDataPoint(new DataPoint(4, 2_000, 20.0, "sensor-a"));

TimeSpan duration = window.GetDuration();                 // 4 seconds
IReadOnlyList<DataPoint> sorted =
    window.GetDataPointsSortedByTimestamp();              // timestamps: 1500, 2000, 3000, 4000
double median = window.GetPercentile(50);                 // 25.0
double upperQuartile = window.GetPercentile(75);          // 32.5
string summary = window.ToSummaryString();
// Window 7 [1000-5000] (4000 ms): Count=4, Avg=25.00, Min=10.00, Max=40.00, StdDev=11.18
```

## Notes

- These methods do not modify the window or its data points.
- `GetDataPointsSortedByTimestamp` returns a snapshot; later changes to `WindowEvent.DataPoints` are not reflected in that result.
- `GetPercentile` uses data point values rather than timestamps.
- `ToSummaryString` uses invariant culture, so its decimal separator and formatting are stable across environments.
