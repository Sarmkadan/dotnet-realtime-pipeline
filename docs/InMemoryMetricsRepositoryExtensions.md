# InMemoryMetricsRepositoryExtensions

Provides extension methods for the `IInMemoryMetricsRepository` interface to query and retrieve metric aggregations from an in-memory store. These methods simplify common querying patterns for metrics data, including time-range filtering, type-based retrieval, and processing-time analysis.

## API

### `GetLatestByTypeAsync`

Retrieves the most recent metric aggregation for a specified metric type.

- **Parameters**
  - `repository`: The `IInMemoryMetricsRepository` instance.
  - `metricType`: The type of metric to retrieve (e.g., "Throughput", "Latency").
- **Return value**
  Returns a `Task<MetricAggregation?>` containing the latest aggregation for the given type, or `null` if none exists.
- **Exceptions**
  Throws `ArgumentNullException` if `repository` or `metricType` is `null`.

---

### `GetByTypeAndTimeRangeAsync`

Retrieves all metric aggregations of a specified type within a given time range.

- **Parameters**
  - `repository`: The `IInMemoryMetricsRepository` instance.
  - `metricType`: The type of metric to filter by.
  - `startUtc`: The start of the time range (inclusive).
  - `endUtc`: The end of the time range (exclusive).
- **Return value**
  Returns a `Task<IReadOnlyList<MetricAggregation>>` containing all matching aggregations, possibly empty.
- **Exceptions**
  Throws `ArgumentNullException` if `repository` or `metricType` is `null`.
  Throws `ArgumentOutOfRangeException` if `startUtc` is not before `endUtc`.

---

### `GetAverageProcessingTimeAsync`

Calculates the average processing time across all metric aggregations.

- **Parameters**
  - `repository`: The `IInMemoryMetricsRepository` instance.
- **Return value**
  Returns a `Task<double?>` containing the average processing time in milliseconds, or `null` if no aggregations exist.
- **Exceptions**
  Throws `ArgumentNullException` if `repository` is `null`.

---

### `GetMaxProcessingTimeAsync`

Retrieves the maximum processing time from all metric aggregations.

- **Parameters**
  - `repository`: The `IInMemoryMetricsRepository` instance.
- **Return value**
  Returns a `Task<double?>` containing the maximum processing time in milliseconds, or `null` if no aggregations exist.
- **Exceptions**
  Throws `ArgumentNullException` if `repository` is `null`.

---
### `GetMinProcessingTimeAsync`

Retrieves the minimum processing time from all metric aggregations.

- **Parameters**
  - `repository`: The `IInMemoryMetricsRepository` instance.
- **Return value**
  Returns a `Task<double?>` containing the minimum processing time in milliseconds, or `null` if no aggregations exist.
- **Exceptions**
  Throws `ArgumentNullException` if `repository` is `null`.

---
### `GetByTypesAsync`

Retrieves the latest metric aggregation for each of the specified metric types.

- **Parameters**
  - `repository`: The `IInMemoryMetricsRepository` instance.
  - `metricTypes`: A collection of metric types to retrieve.
- **Return value**
  Returns a `Task<IReadOnlyList<MetricAggregation>>` containing the latest aggregation for each requested type, possibly empty.
- **Exceptions**
  Throws `ArgumentNullException` if `repository` or `metricTypes` is `null`.

---
### `GetByTypeAndTimeRangeWithProcessingTimeFilterAsync`

Retrieves metric aggregations of a specified type within a time range, filtered by processing time.

- **Parameters**
  - `repository`: The `IInMemoryMetricsRepository` instance.
  - `metricType`: The type of metric to filter by.
  - `startUtc`: The start of the time range (inclusive).
  - `endUtc`: The end of the time range (exclusive).
  - `minProcessingTimeMs`: The minimum processing time in milliseconds to include (inclusive).
  - `maxProcessingTimeMs`: The maximum processing time in milliseconds to include (inclusive).
- **Return value**
  Returns a `Task<IReadOnlyList<MetricAggregation>>` containing all matching aggregations, possibly empty.
- **Exceptions**
  Throws `ArgumentNullException` if `repository` or `metricType` is `null`.
  Throws `ArgumentOutOfRangeException` if `startUtc` is not before `endUtc` or if `minProcessingTimeMs` is greater than `maxProcessingTimeMs`.

---
### `GetLastNMetricsAsync`

Retrieves the most recent `N` metric aggregations across all types.

- **Parameters**
  - `repository`: The `IInMemoryMetricsRepository` instance.
  - `count`: The number of metrics to retrieve.
- **Return value**
  Returns a `Task<IReadOnlyList<MetricAggregation>>` containing the most recent `count` aggregations, possibly empty.
- **Exceptions**
  Throws `ArgumentNullException` if `repository` is `null`.
  Throws `ArgumentOutOfRangeException` if `count` is negative.

## Usage

```csharp
// Example 1: Retrieve the latest throughput metric
var throughput = await repository.GetLatestByTypeAsync("Throughput");
Console.WriteLine($"Latest throughput: {throughput?.Value ?? 0}");

// Example 2: Fetch latency metrics from the last 5 minutes with processing time filtering
var endTime = DateTime.UtcNow;
var startTime = endTime.AddMinutes(-5);
var latencyMetrics = await repository.GetByTypeAndTimeRangeWithProcessingTimeFilterAsync(
    "Latency",
    startTime,
    endTime,
    minProcessingTimeMs: 10,
    maxProcessingTimeMs: 500
);
foreach (var metric in latencyMetrics)
{
    Console.WriteLine($"Latency: {metric.Value}ms, Processing time: {metric.ProcessingTimeMs}ms");
}
```

## Notes

- All methods are thread-safe and may be called concurrently without additional synchronization.
- Methods returning `null` (e.g., `GetAverageProcessingTimeAsync`) do so only when no data exists; they do not indicate failure.
- Time-range methods (`GetByTypeAndTimeRangeAsync`, `GetByTypeAndTimeRangeWithProcessingTimeFilterAsync`) use exclusive upper bounds for `endUtc`; ensure the caller accounts for this when constructing ranges.
- Filtering by processing time (`GetByTypeAndTimeRangeWithProcessingTimeFilterAsync`) includes values equal to the bounds; adjust bounds if strict exclusion is required.
