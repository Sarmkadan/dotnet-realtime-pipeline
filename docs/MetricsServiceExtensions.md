# MetricsService Extensions

`MetricsServiceExtensions` adds convenience overloads and reporting helpers to [`MetricsService`](MetricsService.md). The extensions create automatically timed aggregations, format health reports, expose current processing-time statistics, and validate performance-trend history depth.

Import the service namespace to make the extension methods available:

```csharp
using DotNetRealtimePipeline.Services;
```

## `CreateMetricAggregationAsync`

```csharp
Task<MetricAggregation> CreateMetricAggregationAsync(
    this MetricsService service,
    long itemsProcessed,
    long itemsFailed,
    long itemsSkipped,
    int windowSeconds = 60)
```

Creates and persists a `MetricAggregation` for a window ending at the current UTC time. The start is calculated by subtracting `windowSeconds` from the current time. Processing-time values previously recorded by the service are used for the aggregation's average, minimum, maximum, P95, and P99 latency fields.

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `service` | `MetricsService` | — | Service instance that creates and persists the aggregation. Cannot be `null`. |
| `itemsProcessed` | `long` | — | Number of successfully processed items to store in the aggregation. |
| `itemsFailed` | `long` | — | Number of failed items to store in the aggregation. |
| `itemsSkipped` | `long` | — | Number of skipped items to store in the aggregation. |
| `windowSeconds` | `int` | `60` | Length of the aggregation window in seconds. Must be greater than zero. |

Returns the persisted `MetricAggregation`. It throws `ArgumentNullException` when `service` is `null` and `ArgumentOutOfRangeException` when `windowSeconds` is zero or negative. Repository exceptions from the underlying service are propagated.

## `GenerateHealthReportStringAsync`

```csharp
Task<string> GenerateHealthReportStringAsync(
    this MetricsService service,
    bool includeDetails = true)
```

Calls `GenerateHealthReportAsync` and formats its result as a multi-line, invariant-culture report for logs or dashboards. The report always contains status, message, generation time, throughput, and success rate. Detailed output additionally contains error rate, average processing time, P95 and P99 latency, backpressure, total processed, and total failed.

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `service` | `MetricsService` | — | Service instance used to generate the health report. Cannot be `null`. |
| `includeDetails` | `bool` | `true` | When `true`, appends latency, error, backpressure, and item-count fields. |

Returns the formatted report. It throws `ArgumentNullException` when `service` is `null`.

## `GetProcessingTimeStatistics`

```csharp
IReadOnlyDictionary<string, double> GetProcessingTimeStatistics(
    this MetricsService service)
```

Returns a read-only snapshot of the processing times currently retained by the service.

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `service` | `MetricsService` | — | Service instance whose recorded processing times are summarized. Cannot be `null`. |

The returned dictionary contains these keys:

| Key | Description |
| --- | --- |
| `Count` | Number of retained processing-time samples. |
| `AverageMs` | Arithmetic mean in milliseconds. |
| `MinimumMs` | Smallest sample in milliseconds. |
| `MaximumMs` | Largest sample in milliseconds. |
| `P95Ms` | Nearest-rank 95th-percentile sample in milliseconds. |
| `P99Ms` | Nearest-rank 99th-percentile sample in milliseconds. |

All values are `0` when no processing times have been recorded. The method throws `ArgumentNullException` when `service` is `null`. It can throw `InvalidOperationException` if the service's internal processing-time collection cannot be accessed.

## `AnalyzePerformanceTrendAsync`

```csharp
Task<PerformanceTrend> AnalyzePerformanceTrendAsync(
    this MetricsService service,
    int historyCount = 10)
```

Validates the requested history depth and delegates trend analysis to `MetricsService`. The underlying service compares the oldest and newest metrics returned from the requested history.

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `service` | `MetricsService` | — | Service instance that performs the trend analysis. Cannot be `null`. |
| `historyCount` | `int` | `10` | Maximum number of historical metric records to analyze. Must be at least `2`. |

Returns a `PerformanceTrend` containing the direction, percentage changes, sample count, and covered time span. If the repository supplies fewer than two metrics, the returned direction is `INSUFFICIENT_DATA`. The method throws `ArgumentNullException` when `service` is `null` and `ArgumentOutOfRangeException` when `historyCount` is less than `2`. Repository exceptions are propagated.

## Example

The example assumes `metricsService` has been created with an `IMetricsRepository`, such as by dependency injection.

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;

static async Task WriteMetricsAsync(MetricsService metricsService)
{
    metricsService.RecordProcessingTime(18);
    metricsService.RecordProcessingTime(27);
    metricsService.RecordProcessingTime(42);

    MetricAggregation aggregation = await metricsService.CreateMetricAggregationAsync(
        itemsProcessed: 120,
        itemsFailed: 3,
        itemsSkipped: 2,
        windowSeconds: 30);

    IReadOnlyDictionary<string, double> statistics =
        metricsService.GetProcessingTimeStatistics();

    string healthReport = await metricsService.GenerateHealthReportStringAsync(
        includeDetails: true);

    PerformanceTrend trend = await metricsService.AnalyzePerformanceTrendAsync(
        historyCount: 10);

    Console.WriteLine($"Metric {aggregation.MetricId}: {statistics["AverageMs"]:F2} ms average");
    Console.WriteLine(healthReport);
    Console.WriteLine($"Trend: {trend.TrendDirection}");
}
```
