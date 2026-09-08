# `ITimestampExtractor`

`ITimestampExtractor` defines how an event-time timestamp is obtained from a `DataPoint` for time-based windowing. It and its provided implementations are declared in `DotNetRealtimePipeline.Domain.Models`.

## Interface contract

```csharp
public interface ITimestampExtractor
{
    long ExtractTimestamp(DataPoint dataPoint);
}
```

`ExtractTimestamp` returns a Unix timestamp in milliseconds. Implementations must reject a `null` `dataPoint` with `ArgumentNullException`. Callers should also ensure that the returned value uses the same Unix-millisecond time scale as the window sizes, slide intervals, and flush times supplied elsewhere in the pipeline.

The interface only extracts a timestamp; it does not mutate the data point, create a window, or assign the point to a window. Any additional validation required by a timestamp source is the responsibility of the implementation.

## Provided implementations

### `EventTimeExtractor`

`EventTimeExtractor` returns `dataPoint.Timestamp` unchanged. It is the default choice when `DataPoint.Timestamp` already represents event time. The class is stateless and exposes the reusable singleton `EventTimeExtractor.Instance`.

```csharp
long timestamp = EventTimeExtractor.Instance.ExtractTimestamp(dataPoint);
```

### `CustomTimestampExtractor`

`CustomTimestampExtractor` adapts a `Func<DataPoint, long>` to the interface. Its constructor rejects a `null` delegate, and `ExtractTimestamp` rejects a `null` data point before invoking the delegate. Exceptions raised by the delegate propagate to the caller.

```csharp
var extractor = new CustomTimestampExtractor(point =>
    new DateTimeOffset(point.CreatedAt).ToUnixTimeMilliseconds());
```

Use this implementation for concise extraction rules. Implement `ITimestampExtractor` directly when the rule benefits from a named type, dependencies, or more substantial validation.

## Window assignment

The current `SlidingWindowAggregator` and `WindowingService` APIs do not take an `ITimestampExtractor` and do not call `ExtractTimestamp`. They use `DataPoint.Timestamp` directly. Consequently, `EventTimeExtractor` produces exactly the timestamp those services already use, while timestamps obtained from another source must be placed into `DataPoint.Timestamp` before the point is submitted.

### `WindowingService`

`ProcessDataPoints` assigns each point according to its `Timestamp`:

- For a tumbling window, the start is aligned to the window-size grid: `(timestamp / WindowSizeMs) * WindowSizeMs`.
- For a sliding window, the service finds every slide-aligned window whose half-open interval `[start, start + WindowSizeMs)` contains the timestamp. A point can therefore belong to multiple overlapping windows.
- An unknown window type falls back to tumbling assignment.

The created `WindowEvent` also enforces its half-open time range when `TryAddDataPoint` is called.

### `SlidingWindowAggregator`

`Add` buffers points and keeps them ordered by `DataPoint.Timestamp`. During `FlushDueWindows(currentTimeMs)`, emission boundaries are aligned to `StepIntervalMs`, and each result includes points satisfying:

```text
timestamp >= windowStart && timestamp < windowEnd
```

Thus, a point exactly at the start is included, while a point exactly at the end belongs to a later window. Because windows may overlap, one timestamp can contribute to several results. The aggregator also uses timestamps to choose the initial emission grid and to prune points older than `currentTimeMs - WindowSizeMs`.

`currentTimeMs` is the watermark-like cutoff used to decide which boundaries are due; it should be expressed in the same Unix-millisecond domain as extracted event timestamps.

## Custom extractor example

The following extractor reads an event timestamp stored as metadata and validates its type. The example then creates a copy whose `Timestamp` contains the extracted value before passing it to the existing windowing APIs.

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;

public sealed class MetadataTimestampExtractor : ITimestampExtractor
{
    public long ExtractTimestamp(DataPoint dataPoint)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        if (!dataPoint.Metadata.TryGetValue("eventTimeMs", out object? value) ||
            value is not long timestamp)
        {
            throw new InvalidOperationException(
                "Metadata must contain a long 'eventTimeMs' Unix timestamp.");
        }

        return timestamp;
    }
}

ITimestampExtractor extractor = new MetadataTimestampExtractor();

var incoming = new DataPoint(1, timestamp: 0, value: 42.5, source: "sensor-7");
incoming.Metadata["eventTimeMs"] = 1_725_000_000_000L;

var windowedPoint = incoming.Clone(incoming.Id);
windowedPoint.Timestamp = extractor.ExtractTimestamp(incoming);

var aggregator = new SlidingWindowAggregator(
    windowSizeMs: 10_000,
    stepIntervalMs: 2_000);

aggregator.Add(windowedPoint);
IReadOnlyList<SlidingWindowResult> results =
    aggregator.FlushDueWindows(windowedPoint.Timestamp + 2_000);
```

The same normalized `windowedPoint` can be supplied in a list to `WindowingService.ProcessDataPoints`. Copying is optional, but it preserves the original point when its transport timestamp and event timestamp have different meanings.
