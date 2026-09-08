# QueryService extensions

`QueryServiceExtensions` adds convenience overloads to [`QueryService`](QueryService.md) for UTC `DateTime` ranges, predicate-based searches, and read-only recent-metric results.

Import the service namespace to make the extension methods available:

```csharp
using DotNetRealtimePipeline.Services;
```

## `GetAggregateStatisticsAsync`

```csharp
Task<DataAggregateStatistics> GetAggregateStatisticsAsync(
    this QueryService service,
    DateTime start,
    DateTime end)
```

Converts the inclusive UTC range to Unix timestamps in milliseconds and delegates to `QueryService.GetAggregateStatisticsAsync(long, long)`.

| Parameter | Type | Description |
| --- | --- | --- |
| `service` | `QueryService` | The service used to execute the query. Cannot be `null`. |
| `start` | `DateTime` | Inclusive start of the range, interpreted with a UTC offset. |
| `end` | `DateTime` | Inclusive end of the range, interpreted with a UTC offset. Must be greater than or equal to `start`. |

Returns a `Task<DataAggregateStatistics>` containing statistics for the range. It throws `ArgumentNullException` when `service` is `null` and `ArgumentException` when `end` is earlier than `start`. Use UTC or unspecified `DateTime` values; a local `DateTime` whose local offset is not UTC is not valid for this overload's UTC conversion.

## `SearchDataPointsAsync`

```csharp
Task<IReadOnlyList<DataPoint>> SearchDataPointsAsync(
    this QueryService service,
    Func<DataPoint, bool> predicate)
```

Retrieves the service's unfiltered data-point page and applies `predicate` in memory. The underlying unfiltered query uses the service's default page (`GetPagedAsync(1, 1000)`), so this overload searches up to that page rather than an unlimited repository result set. The returned list is read-only.

| Parameter | Type | Description |
| --- | --- | --- |
| `service` | `QueryService` | The service used to retrieve data points. Cannot be `null`. |
| `predicate` | `Func<DataPoint, bool>` | A function that returns `true` for each data point to include. Cannot be `null`. |

Returns a `Task<IReadOnlyList<DataPoint>>` containing matching data points. It throws `ArgumentNullException` when `service` or `predicate` is `null`. Exceptions raised by the predicate or repository operation propagate to the caller.

## `GetRecentMetricsAsync`

```csharp
Task<IReadOnlyList<MetricAggregation>> GetRecentMetricsAsync(
    this QueryService service,
    int count = 10)
```

Retrieves at most `count` recent metric aggregations and exposes the resulting list through a read-only wrapper.

| Parameter | Type | Description |
| --- | --- | --- |
| `service` | `QueryService` | The service used to retrieve metric history. Cannot be `null`. |
| `count` | `int` | Maximum number of records to request. Defaults to `10` and must be at least `1`. |

Returns a `Task<IReadOnlyList<MetricAggregation>>`. It throws `ArgumentNullException` when `service` is `null` and `ArgumentOutOfRangeException` when `count` is less than `1`.

`QueryService` also has an instance method with the same name and parameters that returns `Task<List<MetricAggregation>>`. C# gives the instance method precedence for `queryService.GetRecentMetricsAsync(...)`. Call `QueryServiceExtensions.GetRecentMetricsAsync(...)` explicitly when the read-only return type and extension validation are required.

## Example

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;

// Assume queryService was created with the application's repositories.
QueryService queryService = GetQueryService();

DateTime end = DateTime.UtcNow;
DateTime start = end.AddHours(-1);

DataAggregateStatistics statistics =
    await queryService.GetAggregateStatisticsAsync(start, end);

IReadOnlyList<DataPoint> highQualityPoints =
    await queryService.SearchDataPointsAsync(point => point.Quality >= 90);

IReadOnlyList<MetricAggregation> recentMetrics =
    await QueryServiceExtensions.GetRecentMetricsAsync(queryService, count: 5);

Console.WriteLine($"Points in range: {statistics.Count}");
Console.WriteLine($"High-quality points on the default page: {highQualityPoints.Count}");
Console.WriteLine($"Recent metric records: {recentMetrics.Count}");
```
