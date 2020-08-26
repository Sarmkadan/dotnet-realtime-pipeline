# MetricAggregation

Represents a snapshot of aggregated pipeline metrics over a defined time window. This type captures throughput, latency distribution, error rates, backpressure statistics, and per-source/per-stage breakdowns for a single metric identifier. Instances are typically produced by the metrics subsystem and consumed by monitoring dashboards, alerting rules, or logging sinks.

## API

### Constructors

- **`MetricAggregation()`**  
  Default parameterless constructor. Initializes all numeric fields to zero, `ComputedAt` to `DateTime.MinValue`, and both dictionaries to empty collections. Suitable for deserialization or incremental population.

- **`MetricAggregation(long metricId, long timeWindowStartMs, long timeWindowEndMs, string metricType, long totalItemsProcessed, long totalItemsFailed, long totalItemsSkipped, double averageProcessingTimeMs, double minProcessingTimeMs, double maxProcessingTimeMs, double p95ProcessingTimeMs, double p99ProcessingTimeMs, long backpressureEvents, long totalBackpressureMs, DateTime computedAt, Dictionary<string, long> countBySource, Dictionary<string, double> errorRateByStage)`**  
  Fully parameterized constructor. Accepts all fields required to describe a complete aggregation window. The caller must supply non-null dictionaries; passing `null` for `countBySource` or `errorRateByStage` will result in a `NullReferenceException` when the instance is subsequently used. No validation is performed on the logical consistency of the values (e.g., `MinProcessingTimeMs` exceeding `MaxProcessingTimeMs` is stored as given).

### Properties

- **`long MetricId`**  
  Unique identifier for the metric definition this aggregation belongs to. Correlates with a specific pipeline stage, operation, or counter registered in the metrics catalog.

- **`long TimeWindowStartMs`**  
  Start of the aggregation window expressed as milliseconds since the Unix epoch (January 1, 1970 00:00:00 UTC). Inclusive boundary.

- **`long TimeWindowEndMs`**  
  End of the aggregation window expressed as milliseconds since the Unix epoch. Exclusive boundary. The difference `TimeWindowEndMs - TimeWindowStartMs` defines the window duration used for throughput calculations.

- **`string MetricType`**  
  Classifier indicating the kind of metric (e.g., `"ItemProcessing"`, `"QueueDepth"`, `"StageLatency"`). Used by consumers to select appropriate visualization or alerting logic.

- **`long TotalItemsProcessed`**  
  Cumulative count of items that completed processing successfully within the window.

- **`long TotalItemsFailed`**  
  Cumulative count of items that terminated with an error or exception during processing.

- **`long TotalItemsSkipped`**  
  Cumulative count of items intentionally bypassed (e.g., due to filtering rules, deduplication, or early-exit conditions).

- **`double AverageProcessingTimeMs`**  
  Arithmetic mean of per-item processing durations in milliseconds for items that completed (success or failure) during the window.

- **`double MinProcessingTimeMs`**  
  Minimum observed processing duration in milliseconds. Set to `0` if no items were processed.

- **`double MaxProcessingTimeMs`**  
  Maximum observed processing duration in milliseconds. Set to `0` if no items were processed.

- **`double P95ProcessingTimeMs`**  
  95th percentile of the processing time distribution. At least 95% of completed items finished within this duration. Set to `0` if insufficient samples exist.

- **`double P99ProcessingTimeMs`**  
  99th percentile of the processing time distribution. At least 99% of completed items finished within this duration. Set to `0` if insufficient samples exist.

- **`long BackpressureEvents`**  
  Number of discrete backpressure occurrences detected during the window (e.g., queue full events, upstream throttle signals).

- **`long TotalBackpressureMs`**  
  Cumulative time in milliseconds that the pipeline spent in a backpressured state during the window.

- **`DateTime ComputedAt`**  
  UTC timestamp indicating when this aggregation snapshot was finalized and computed. Not necessarily equal to `TimeWindowEndMs`; may reflect a short computation delay.

- **`Dictionary<string, long> CountBySource`**  
  Breakdown of `TotalItemsProcessed` by source identifier (e.g., topic name, partition key, ingress node). Keys are source labels; values are item counts. The dictionary may be empty but is never `null` after construction.

- **`Dictionary<string, double> ErrorRateByStage`**  
  Per-stage error rates expressed as fractions between `0.0` and `1.0`, keyed by stage name. A value of `0.05` indicates a 5% error rate for that stage within the window. The dictionary may be empty but is never `null` after construction.

### Methods

- **`double CalculateThroughput()`**  
  Computes the overall processing throughput for the window. Returns `(TotalItemsProcessed + TotalItemsFailed + TotalItemsSkipped) / (TimeWindowEndMs - TimeWindowStartMs) * 1000.0`, yielding items per second.  
  **Returns:** Throughput as items/second. Returns `0.0` if the window duration is zero or negative. Does not throw.

## Usage

### Example 1: Creating and inspecting an aggregation

```csharp
var countBySource = new Dictionary<string, long>
{
    ["topic-a"] = 8000,
    ["topic-b"] = 2000
};

var errorRateByStage = new Dictionary<string, double>
{
    ["parse"] = 0.01,
    ["validate"] = 0.005,
    ["enrich"] = 0.0
};

var aggregation = new MetricAggregation(
    metricId: 42,
    timeWindowStartMs: 1700000000000,
    timeWindowEndMs: 1700000060000,   // 60-second window
    metricType: "ItemProcessing",
    totalItemsProcessed: 10000,
    totalItemsFailed: 100,
    totalItemsSkipped: 50,
    averageProcessingTimeMs: 12.5,
    minProcessingTimeMs: 2.0,
    maxProcessingTimeMs: 98.0,
    p95ProcessingTimeMs: 45.0,
    p99ProcessingTimeMs: 72.0,
    backpressureEvents: 3,
    totalBackpressureMs: 1500,
    computedAt: DateTime.UtcNow,
    countBySource: countBySource,
    errorRateByStage: errorRateByStage
);

double throughput = aggregation.CalculateThroughput();
Console.WriteLine($"Throughput: {throughput:F2} items/sec");
Console.WriteLine($"Error rate in 'parse': {aggregation.ErrorRateByStage["parse"]:P1}");
```

### Example 2: Accumulating metrics across multiple windows

```csharp
MetricAggregation CombineWindows(MetricAggregation first, MetricAggregation second)
{
    if (first.MetricId != second.MetricId)
        throw new InvalidOperationException("Cannot combine aggregations for different metrics.");

    var combinedSources = new Dictionary<string, long>(first.CountBySource);
    foreach (var kvp in second.CountBySource)
    {
        combinedSources.TryGetValue(kvp.Key, out long existing);
        combinedSources[kvp.Key] = existing + kvp.Value;
    }

    var combinedErrors = new Dictionary<string, double>(first.ErrorRateByStage);
    foreach (var kvp in second.ErrorRateByStage)
    {
        combinedErrors.TryGetValue(kvp.Key, out double existing);
        combinedErrors[kvp.Key] = Math.Max(existing, kvp.Value);
    }

    return new MetricAggregation(
        metricId: first.MetricId,
        timeWindowStartMs: Math.Min(first.TimeWindowStartMs, second.TimeWindowStartMs),
        timeWindowEndMs: Math.Max(first.TimeWindowEndMs, second.TimeWindowEndMs),
        metricType: first.MetricType,
        totalItemsProcessed: first.TotalItemsProcessed + second.TotalItemsProcessed,
        totalItemsFailed: first.TotalItemsFailed + second.TotalItemsFailed,
        totalItemsSkipped: first.TotalItemsSkipped + second.TotalItemsSkipped,
        averageProcessingTimeMs: (first.AverageProcessingTimeMs + second.AverageProcessingTimeMs) / 2.0,
        minProcessingTimeMs: Math.Min(first.MinProcessingTimeMs, second.MinProcessingTimeMs),
        maxProcessingTimeMs: Math.Max(first.MaxProcessingTimeMs, second.MaxProcessingTimeMs),
        p95ProcessingTimeMs: Math.Max(first.P95ProcessingTimeMs, second.P95ProcessingTimeMs),
        p99ProcessingTimeMs: Math.Max(first.P99ProcessingTimeMs, second.P99ProcessingTimeMs),
        backpressureEvents: first.BackpressureEvents + second.BackpressureEvents,
        totalBackpressureMs: first.TotalBackpressureMs + second.TotalBackpressureMs,
        computedAt: DateTime.UtcNow,
        countBySource: combinedSources,
        errorRateByStage: combinedErrors
    );
}
```

## Notes

- **Thread safety:** `MetricAggregation` is a plain data object with no internal synchronization. Concurrent reads from multiple threads are safe only if no thread is mutating the instance or its dictionaries. Concurrent writes or mixed read/write access require external locking.
- **Dictionary ownership:** The fully parameterized constructor stores the provided `Dictionary<string, long>` and `Dictionary<string, double>` references directly. Modifications to these dictionaries after construction will be visible through the `MetricAggregation` instance. To prevent external mutation, pass copies.
- **Zero-duration windows:** `CalculateThroughput()` returns `0.0` when `TimeWindowEndMs <= TimeWindowStartMs`. No exception is thrown. Consumers should guard against division-by-zero downstream if they perform their own rate calculations.
- **Percentile defaults:** `P95ProcessingTimeMs` and `P99ProcessingTimeMs` are `0.0` when the sample count is too low to compute meaningful percentiles. Callers should treat `0.0` as “unavailable” rather than a literal zero-millisecond measurement.
- **Error rate semantics:** Values in `ErrorRateByStage` are expected to be in `[0.0, 1.0]`, but no range enforcement is performed. Negative values or values exceeding `1.0` can be stored and will propagate to consumers.
- **`ComputedAt` vs window boundaries:** `ComputedAt` records the computation time, which may lag behind `TimeWindowEndMs` due to batching or delayed delivery. Do not use `ComputedAt` for window-boundary arithmetic.
