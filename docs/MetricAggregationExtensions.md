# MetricAggregationExtensions

Provides extension methods for aggregating, filtering, and deriving summary statistics from `MetricAggregation` instances. This class serves as the primary query surface for telemetry data collected across pipeline stages, enabling consumers to compute success rates, error distributions, backpressure percentages, and other key observability signals without directly manipulating the underlying metric structures.

## API

### CalculateSuccessRate

```csharp
public static double CalculateSuccessRate(this MetricAggregation aggregation)
```

Computes the ratio of successfully processed items to total items across all sources in the aggregation. The result is a value between `0.0` and `1.0`, where `1.0` indicates no failures were recorded.

**Parameters:**
- `aggregation` — The metric aggregation to evaluate.

**Returns:** A `double` representing the success rate. Returns `1.0` if no items were processed (vacuous truth).

**Throws:** `ArgumentNullException` if `aggregation` is `null`.

---

### CalculateCombinedErrorRate

```csharp
public static double CalculateCombinedErrorRate(this MetricAggregation agg)
```

Computes the combined error rate across all error categories and stages. This aggregates all failure modes into a single ratio relative to total items processed.

**Parameters:**
- `aggregation` — The metric aggregation to evaluate.

**Returns:** A `double` between `0.0` and `1.0` representing the error rate. Returns `0.0` if no items were processed.

**Throws:** `ArgumentNullException` if `aggregation` is `null`.

---

### GetTimeWindowDurationMs

```csharp
public static long GetTimeWindowDurationMs(this MetricAggregation agg)
```

Extracts the duration of the metric aggregation time window in milliseconds.

**Parameters:**
- `aggregation` — The metric aggregation to query.

**Returns:** A `long` representing the window duration in milliseconds.

**Throws:** `ArgumentNullException` if `aggregation` is `null`.

---

### GetTimeWindowDuration

```csharp
public static TimeSpan GetTimeWindowDuration(this MetricAggregation agg)
```

Extracts the duration of the metric aggregation time window as a `TimeSpan`.

**Parameters:**
- `aggregation` — The metric aggregation to query.

**Returns:** A `TimeSpan` representing the window duration.

**Throws:** `ArgumentNullException` if `aggregation` is `null`.

---

### GetSourceNames

```csharp
public static IEnumerable<string> GetSourceNames(this MetricAggregation agg)
```

Enumerates the distinct source names present in the aggregation. Each source corresponds to an upstream data origin that contributed metrics during the time window.

**Parameters:**
- `aggregation` — The metric aggregation to query.

**Returns:** An `IEnumerable<string>` of source names. Returns an empty enumeration if no sources are recorded.

**Throws:** `ArgumentNullException` if `aggregation` is `null`.

---

### GetStagesWithErrors

```csharp
public static IEnumerable<string> GetStagesWithErrors(this MetricAggregation agg)
```

Enumerates the names of pipeline stages that recorded at least one error during the aggregation window. Useful for quickly identifying problematic stages without scanning all metrics.

**Parameters:**
- `aggregation` — The metric aggregation to query.

**Returns:** An `IEnumerable<string>` of stage names with errors. Returns an empty enumeration if no errors occurred.

**Throws:** `ArgumentNullException` if `aggregation` is `null`.

---

### GetTotalItemsFromSources

```csharp
public static long GetTotalItemsFromSources(this MetricAggregation agg)
```

Calculates the total number of items ingested from all sources during the aggregation window. This is the sum of items received at the pipeline entry points.

**Parameters:**
- `aggregation` — The metric aggregation to query.

**Returns:** A `long` representing the total item count from all sources.

**Throws:** `ArgumentNullException` if `aggregation` is `null`.

---

### GetBackpressurePercentage

```csharp
public static double GetBackpressurePercentage(this MetricAggregation agg)
```

Computes the percentage of time that backpressure was active during the aggregation window. The value is expressed as a percentage between `0.0` and `100.0`.

**Parameters:**
- `aggregation` — The metric aggregation to evaluate.

**Returns:** A `double` representing the backpressure percentage.

**Throws:** `ArgumentNullException` if `aggregation` is `null`.

---

### GetAveragePercentile

```csharp
public static double GetAveragePercentile(this MetricAggregation agg, double percentile)
```

Computes the average value of a specified percentile across all stages that track latency or duration metrics. This provides a single-number summary of pipeline performance at the given percentile threshold.

**Parameters:**
- `aggregation` — The metric aggregation to evaluate.
- `percentile` — The target percentile, typically between `0.0` and `100.0` (e.g., `95.0` for P95).

**Returns:** A `double` representing the average of the requested percentile across stages.

**Throws:**
- `ArgumentNullException` if `aggregation` is `null`.
- `ArgumentOutOfRangeException` if `percentile` is less than `0.0` or greater than `100.0`.

---

### Combine

```csharp
public static MetricAggregation Combine(this MetricAggregation first, MetricAggregation second)
```

Merges two `MetricAggregation` instances into a single combined aggregation. The resulting aggregation spans the union of their time windows and consolidates all stage metrics, source counts, and error records.

**Parameters:**
- `first` — The first metric aggregation.
- `second` — The second metric aggregation.

**Returns:** A new `MetricAggregation` instance representing the combined data.

**Throws:**
- `ArgumentNullException` if either `first` or `second` is `null`.

---

### FilterBySource

```csharp
public static MetricAggregation FilterBySource(this MetricAggregation agg, string sourceName)
```

Creates a new `MetricAggregation` containing only the metrics associated with the specified source name. All other sources and their associated stage data are excluded from the result.

**Parameters:**
- `aggregation` — The metric aggregation to filter.
- `sourceName` — The source name to retain.

**Returns:** A new `MetricAggregation` instance scoped to the given source. Returns an aggregation with zeroed metrics if the source is not found.

**Throws:**
- `ArgumentNullException` if `aggregation` is `null`.
- `ArgumentException` if `sourceName` is `null` or whitespace.

---

## Usage

### Example 1: Monitoring Pipeline Health

```csharp
MetricAggregation currentWindow = pipeline.CollectMetrics();

double successRate = currentWindow.CalculateSuccessRate();
double errorRate = currentWindow.CalculateCombinedErrorRate();
double backpressure = currentWindow.GetBackpressurePercentage();

if (successRate < 0.99)
{
    var failingStages = currentWindow.GetStagesWithErrors();
    Console.WriteLine($"Warning: Success rate dropped to {successRate:P2}. " +
                      $"Stages with errors: {string.Join(", ", failingStages)}");
}

if (backpressure > 80.0)
{
    Console.WriteLine($"Critical: Backpressure at {backpressure:F1}%");
}
```

### 2: Aggregating and Filtering Across Time Windows

```csharp
MetricAggregation morning = collector.GetWindow(TimeSpan.FromHours(6));
MetricAggregation afternoon = collector.GetWindow(TimeSpan.FromHours(6));

MetricAggregation daily = morning.Combine(afternoon);

var sources = daily.GetSourceNames();
foreach (string source in sources)
{
    MetricAggregation sourceMetrics = daily.FilterBySource(source);
    long items = sourceMetrics.GetTotalItemsFromSources();
    double p95 = sourceMetrics.GetAveragePercentile(95.0);

    Console.WriteLine($"Source '{source}': {items} items, P95 latency: {p95:F2} ms");
}
```

## Notes

- **Null handling:** All methods throw `ArgumentNullException` when the primary `MetricAggregation` argument is `null`. Callers should guard against null aggregations, particularly when retrieving windows that may not exist.
- **Empty aggregations:** Methods that return computed rates (`CalculateSuccessRate`, `CalculateCombinedErrorRate`) return `0.0` when no items have been processed, avoiding division-by-zero errors. `GetSourceNames` and `GetStagesWithErrors` return empty enumerables in this case.
- **Immutability:** `Combine` and `FilterBySource` create new `MetricAggregation` instances rather than mutating the originals. The original instances remain unchanged and safe for concurrent reads.
- **Thread safety:** All methods are static and operate on their input arguments without shared mutable state. The thread safety of the underlying `MetricAggregation` instance depends on its own implementation; these extensions do not introduce additional threading concerns.
- **Percentile bounds:** `GetAveragePercentile` enforces that the percentile argument falls within `[0.0, 100.0]`. Values outside this range throw `ArgumentOutOfRangeException`.
- **FilterBySource with unknown sources:** When the specified source name does not exist in the aggregation, the method returns a valid but empty `MetricAggregation` rather than throwing. Callers should check `GetTotalItemsFromSources` on the result to confirm data was retained.
