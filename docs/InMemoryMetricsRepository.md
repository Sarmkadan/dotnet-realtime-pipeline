# InMemoryMetricsRepository

An in-memory implementation of `IMetricsRepository` that stores `MetricAggregation` instances in a concurrent dictionary for fast, non-persistent metric operations. Designed for scenarios requiring low-latency access to recent or frequently updated metrics without external dependencies.

## API

### `Task<MetricAggregation?> GetByIdAsync(Guid id)`

Retrieves a metric aggregation by its unique identifier. Returns `null` if no metric with the specified `id` exists. Throws `ArgumentException` if `id` is `Guid.Empty`.

### `Task<List<MetricAggregation>> GetByTimeRangeAsync(DateTimeOffset start, DateTimeOffset end)`

Fetches all metric aggregations whose timestamp falls within the specified range `[start, end)`. The range is inclusive of `start` and exclusive of `end`. Returns an empty list if no metrics fall within the range. Throws `ArgumentOutOfRangeException` if `start` is after `end`.

### `Task<List<MetricAggregation>> GetByTypeAsync(string type)`

Returns all metric aggregations matching the given `type` string. The comparison is case-sensitive. Returns an empty list if no metrics of the specified type exist. Throws `ArgumentNullException` if `type` is `null`.

### `Task<MetricAggregation> SaveAsync(MetricAggregation metric)`

Persists the provided `metric` in the repository. If the metric's `Id` is `Guid.Empty`, a new `Guid` is assigned. If a metric with the same `Id` already exists, it is replaced. Returns the saved metric. Throws `ArgumentNullException` if `metric` is `null`.

### `Task<bool> DeleteAsync(Guid id)`

Removes the metric with the specified `id` from the repository. Returns `true` if a metric was found and removed; otherwise, returns `false`. Throws `ArgumentException` if `id` is `Guid.Empty`.

### `Task<MetricAggregation> GetLatestAsync()`

Retrieves the most recently saved metric aggregation based on its timestamp. If multiple metrics share the latest timestamp, the first one encountered is returned. Throws `InvalidOperationException` if the repository is empty.

### `Task<List<MetricAggregation>> GetHistoryAsync()`

Returns all metric aggregations in the repository, ordered by timestamp in descending order (newest first). Returns an empty list if no metrics are stored.

### `void Clear()`

Removes all metric aggregations from the repository. This operation is synchronous and affects all threads accessing the repository.

## Usage
