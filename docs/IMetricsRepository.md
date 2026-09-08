# IMetricsRepository

`IMetricsRepository` defines the asynchronous persistence and query contract for `MetricAggregation` records. Implementations provide lookup, filtering, saving, deletion, and history access for pipeline metrics.

For the repository's in-memory implementation, see [InMemoryMetricsRepository](InMemoryMetricsRepository.md).

## Contract

### `Task<MetricAggregation?> GetByIdAsync(long metricId)`

Retrieves the metric aggregation identified by `metricId`. Returns the matching aggregation when found, or `null` when it does not exist.

### `Task<List<MetricAggregation>> GetByTimeRangeAsync(long startMs, long endMs)`

Retrieves metric aggregations within the time range described by `startMs` and `endMs`, both expressed as timestamps in milliseconds. Returns the aggregations within that range.

### `Task<List<MetricAggregation>> GetByTypeAsync(string metricType)`

Retrieves metric aggregations whose type matches `metricType`, such as `"hourly"` or `"daily"`. Returns the matching aggregations.

### `Task<MetricAggregation> SaveAsync(MetricAggregation metric)`

Saves the supplied metric aggregation and returns the saved aggregation.

### `Task<bool> DeleteAsync(long metricId)`

Deletes the metric aggregation identified by `metricId`. Returns `true` when deletion succeeds and `false` otherwise.

### `Task<MetricAggregation> GetLatestAsync()`

Retrieves the most recent metric aggregation.

### `Task<List<MetricAggregation>> GetHistoryAsync(int count)`

Retrieves `count` historical metric aggregations. The returned list is in chronological order.

## Example

```csharp
using System.Collections.Generic;
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;

IMetricsRepository repository = new InMemoryMetricsRepository();

var metric = new MetricAggregation(
    metricId: 42,
    startMs: 1_725_753_600_000,
    endMs: 1_725_757_200_000,
    metricType: "hourly")
{
    TotalItemsProcessed = 10_000,
    TotalItemsFailed = 12
};

await repository.SaveAsync(metric);

MetricAggregation? saved = await repository.GetByIdAsync(42);
List<MetricAggregation> hourly = await repository.GetByTypeAsync("hourly");
List<MetricAggregation> inRange = await repository.GetByTimeRangeAsync(
    metric.TimeWindowStartMs,
    metric.TimeWindowEndMs);
MetricAggregation latest = await repository.GetLatestAsync();
List<MetricAggregation> history = await repository.GetHistoryAsync(count: 10);
bool deleted = await repository.DeleteAsync(42);
```

Consumers should program against `IMetricsRepository` so the persistence implementation can be replaced without changing calling code.
