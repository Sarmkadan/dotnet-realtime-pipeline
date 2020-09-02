# QueryService

`QueryService` provides a consolidated interface for executing temporal and statistical queries against the real-time data pipeline. It exposes methods to search raw data points, compute aggregate statistics, perform trend analysis, decompose time series into structural components, retrieve recent metric aggregations, and obtain total counts over specified time windows. The service also surfaces precomputed summary fields (`StartMs`, `EndMs`, `Count`, `Sum`, `Average`, `Min`, `Max`, `StdDev`, `Median`, `P95`, `P99`, `UniqueSourceCount`, `AverageQuality`) that describe the result set from the most recent query operation.

## API

### Constructors

- **`QueryService()`**
  Initializes a new instance of `QueryService`. The constructor accepts configuration through the ambient pipeline context and prepares the underlying query engine for subsequent operations.

### Methods

- **`public async Task<List<DataPoint>> SearchDataPointsAsync(long startMs, long endMs, string[] sourceIds, string[] metricNames, int maxResults)`**
  Searches for raw `DataPoint` records within the inclusive time range `[startMs, endMs]`. Results can be filtered by one or more `sourceIds` and `metricNames`. The `maxResults` parameter caps the number of returned items. Returns a `List<DataPoint>` ordered by timestamp ascending. Throws `ArgumentException` if `startMs` is greater than `endMs` or if `maxResults` is less than 1. Throws `InvalidOperationException` if the query engine is not initialized.

- **`public async Task<DataAggregateStatistics> GetAggregateStatisticsAsync(long startMs, long endMs, string[] sourceIds, string[] metricNames)`**
  Computes aggregate statistics over the specified time window and optional source/metric filters. Returns a `DataAggregateStatistics` object containing `Count`, `Sum`, `Average`, `Min`, `Max`, `StdDev`, `Median`, `P95`, `P99`, `UniqueSourceCount`, and `AverageQuality`. Throws `ArgumentException` if `startMs` > `endMs`. Throws `InvalidOperationException` if no data points match the criteria.

- **`public async Task<TrendAnalysis> AnalyzeTrendsAsync(long startMs, long endMs, string metricName, int windowSizeMs)`**
  Performs trend detection on a single metric over the given time range using a sliding window of `windowSizeMs` milliseconds. Returns a `TrendAnalysis` object describing direction, slope, and confidence intervals. Throws `ArgumentException` if `windowSizeMs` is zero or negative, or if `metricName` is null or empty. Throws `InvalidOperationException` when insufficient data exists for the requested window count.

- **`public async Task<TimeSeriesDecomposition> DecomposeTimeSeriesAsync(long startMs, long endMs, string metricName)`**
  Decomposes the time series for the specified metric into trend, seasonal, and residual components. Returns a `TimeSeriesDecomposition` containing the separated series arrays. Throws `ArgumentException` if `metricName` is null or empty. Throws `InvalidOperationException` if fewer than two full seasonal cycles of data are available.

- **`public async Task<List<MetricAggregation>> GetRecentMetricsAsync(int lookbackSeconds, int maxMetrics)`**
  Retrieves the most recent `MetricAggregation` entries from the pipeline cache, limited to the last `lookbackSeconds` seconds and at most `maxMetrics` items. Returns a `List<MetricAggregation>` in reverse chronological order. Throws `ArgumentException` if `lookbackSeconds` or `maxMetrics` is less than 1.

- **`public async Task<long> GetDataPointCountAsync(long startMs, long endMs, string[] sourceIds, string[] metricNames)`**
  Returns the total count of `DataPoint` records matching the time range and optional filters. Throws `ArgumentException` if `startMs` > `endMs`.

### Properties

The following properties reflect the result of the most recently executed query operation (search, aggregate, or count). Their values are reset when a new query is initiated and populated upon successful completion.

- **`long StartMs`** — The inclusive start timestamp (milliseconds) of the last query window.
- **`long EndMs`** — The inclusive end timestamp (milliseconds) of the last query window.
- **`int Count`** — The number of data points matched by the last query.
- **`double Sum`** — The sum of all values in the last query result.
- **`double Average`** — The arithmetic mean of values in the last query result.
- **`double Min`** — The minimum value in the last query result.
- **`double Max`** — The maximum value in the last query result.
- **`double StdDev`** — The population standard deviation of values in the last query result.
- **`double Median`** — The median value in the last query result.
- **`double P95`** — The 95th percentile value in the last query result.
- **`double P99`** — The 99th percentile value in the last query result.
- **`int UniqueSourceCount`** — The number of distinct source identifiers in the last query result.
- **`double AverageQuality`** — The mean quality score (0.0–1.0) across data points in the last query result.

## Usage

### Example 1: Retrieve aggregates and inspect summary properties

```csharp
var queryService = new QueryService();

// Define a 1-hour window and filter for CPU metrics from two hosts.
long start = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds();
long end = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
string[] sources = { "host-alpha", "host-beta" };
string[] metrics = { "cpu.user", "cpu.system" };

DataAggregateStatistics stats = await queryService.GetAggregateStatisticsAsync(
    start, end, sources, metrics);

// The summary properties on QueryService now reflect this query.
Console.WriteLine($"Data points: {queryService.Count}");
Console.WriteLine($"Average CPU: {queryService.Average:F2}");
Console.WriteLine($"P99 CPU: {queryService.P99:F2}");
Console.WriteLine($"Unique hosts: {queryService.UniqueSourceCount}");
Console.WriteLine($"Avg quality: {queryService.AverageQuality:F3}");
```

### Example 2: Search raw data points and decompose a time series

```csharp
var queryService = new QueryService();

long start = DateTimeOffset.UtcNow.AddMinutes(-30).ToUnixTimeMilliseconds();
long end = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

// First, fetch raw data points for a specific metric.
List<DataPoint> points = await queryService.SearchDataPointsAsync(
    start, end,
    sourceIds: new[] { "sensor-42" },
    metricNames: new[] { "temperature" },
    maxResults: 10_000);

Console.WriteLine($"Retrieved {points.Count} raw data points.");

// Decompose the temperature time series into trend, seasonal, and residual.
TimeSeriesDecomposition decomp = await queryService.DecomposeTimeSeriesAsync(
    start, end, "temperature");

Console.WriteLine($"Trend length: {decomp.Trend.Length}");
Console.WriteLine($"Seasonal length: {decomp.Seasonal.Length}");
Console.WriteLine($"Residual length: {decomp.Residual.Length}");
```

## Notes

- **Property lifecycle**: The summary properties (`Count`, `Sum`, `Average`, `Min`, `Max`, `StdDev`, `Median`, `P95`, `P99`, `UniqueSourceCount`, `AverageQuality`) are populated only after a successful call to `SearchDataPointsAsync`, `GetAggregateStatisticsAsync`, or `GetDataPointCountAsync`. Reading them before any query has completed yields default values (zero for numeric fields). Their values are overwritten on each subsequent query invocation, regardless of whether the new query uses the same time window or filters.
- **Empty result sets**: When a query matches zero data points, `GetAggregateStatisticsAsync` throws `InvalidOperationException`. `SearchDataPointsAsync` and `GetDataPointCountAsync` return an empty list and zero, respectively, and the summary properties are set to zero. `AnalyzeTrendsAsync` and `DecomposeTimeSeriesAsync` throw `InvalidOperationException` if the available data is insufficient for their algorithms.
- **Time range validation**: All methods accepting `startMs` and `endMs` enforce that `startMs <= endMs`. Reversed ranges cause an `ArgumentException`.
- **Thread safety**: `QueryService` is not thread-safe. Concurrent calls from multiple threads may interleave query execution and corrupt the summary property state. Instances should be used within a single synchronization context, or a dedicated instance should be created per thread/task.
- **Metric name matching**: Metric name filters are case-sensitive and must exactly match the names stored in the pipeline index. Wildcards and partial matches are not supported.
- **Window size constraints**: `AnalyzeTrendsAsync` requires that the overall time range be at least twice the `windowSizeMs` to produce a meaningful trend. Smaller ranges result in an `InvalidOperationException`.
