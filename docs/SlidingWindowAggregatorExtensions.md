# SlidingWindowAggregator extensions

`SlidingWindowAggregatorExtensions` provides convenience methods for adding values with UTC timestamps and exporting a `SlidingWindowResult` as CSV. For the underlying window behavior, see [SlidingWindowAggregator](SlidingWindowAggregator.md).

## API

### `AddWithCurrentTimestamp(double value)`

Adds one `DataPoint` to the aggregator. Its `Timestamp` is generated from `DateTimeOffset.UtcNow` as Unix time in milliseconds, and its `Value` is the supplied value.

The method throws `ArgumentNullException` when the aggregator is `null`.

### `AddRangeWithCurrentTimestamps(IEnumerable<double> values)`

Adds a sequence of values as `DataPoint` instances. The method captures the current UTC Unix-millisecond timestamp once, so every point in a non-empty batch receives the same timestamp. An empty sequence makes no change to the aggregator.

The method throws `ArgumentNullException` when either the aggregator or `values` is `null`.

The sequence is checked for emptiness before it is projected into data points, so a non-empty `IEnumerable<double>` is enumerated once for the check and again when the aggregator consumes the projected sequence.

### `ToCsv()`

Converts one `SlidingWindowResult` into a CSV string containing a header row and one data row, separated by `Environment.NewLine`. `Average`, `Sum`, `Min`, `Max`, and `Trend` are formatted with `CultureInfo.InvariantCulture`.

The method throws `ArgumentNullException` when the result is `null`. It does not quote or escape fields; all fields currently exported are numeric.

## Example

```csharp
using DotNetRealtimePipeline.Services;

var aggregator = new SlidingWindowAggregator(
    windowSizeMs: 10_000,
    stepIntervalMs: 1_000);

aggregator.AddWithCurrentTimestamp(12.5);
aggregator.AddRangeWithCurrentTimestamps(new[] { 15.0, 17.5, 20.0 });

// Advance by one step so the newly timestamped values are included in a due window.
long flushTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 1_000;
foreach (SlidingWindowResult result in aggregator.FlushDueWindows(flushTimeMs))
{
    Console.WriteLine(result.ToCsv());
}
```

## CSV columns

The columns are written in this exact order:

| Column | Description |
| --- | --- |
| `WindowId` | Sequential identifier assigned to the emitted window. |
| `WindowStartMs` | Inclusive start of the window as Unix time in milliseconds. |
| `WindowEndMs` | Exclusive end of the window as Unix time in milliseconds. |
| `WindowSizeMs` | Configured window duration in milliseconds. |
| `StepIntervalMs` | Configured interval between window emissions in milliseconds. |
| `DataPointCount` | Number of data points included in the window. |
| `Average` | Arithmetic mean of the included values. |
| `Sum` | Sum of the included values. |
| `Min` | Minimum included value. |
| `Max` | Maximum included value. |
| `Trend` | Difference between the average of the second half and the average of the first half of the window's points; `0` when fewer than two points are present. |

The header row is:

```text
WindowId,WindowStartMs,WindowEndMs,WindowSizeMs,StepIntervalMs,DataPointCount,Average,Sum,Min,Max,Trend
```
