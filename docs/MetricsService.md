# MetricsService

`MetricsService` provides real-time monitoring and aggregation of pipeline throughput, processing latency, and error rates. It exposes synchronous and asynchronous methods for recording individual events, computing statistical summaries, generating health reports, and analyzing performance trends over configurable time windows.

## API

### Constructors

- **`public MetricsService()`**  
  Initializes a new instance of the service with default counters and empty processing-time history. No external configuration is required.

### Throughput Recording

- **`public void RecordThroughput()`**  
  Increments the internal throughput counter by one item. Thread-safe.

- **`public void RecordThroughput(int count)`**  
  Increments the internal throughput counter by `count` items.  
  *Parameters*: `count` – number of items processed (must be non-negative).  
  *Throws*: `ArgumentOutOfRangeException` when `count` is negative.

- **`public double GetThroughput()`**  
  Returns the current throughput in items per second, calculated over the most recent complete sampling window. Returns `0.0` if no items have been recorded.

- **`public double GetThroughput(TimeSpan window)`**  
  Returns the throughput in items per second calculated over the specified `window` ending at the current time.  
  *Parameters*: `window` – the look-back duration (must be positive).  
  *Throws*: `ArgumentOutOfRangeException` when `window` is zero or negative.

### Processing-Time Recording

- **`public void RecordProcessingTime(TimeSpan duration)`**  
  Appends a single processing-time sample to the internal reservoir.  
  *Parameters*: `duration` – the measured processing time (must be non-negative).  
  *Throws*: `ArgumentOutOfRangeException` when `duration` is negative.

- **`public void ClearProcessingTimes()`**  
  Removes all stored processing-time samples. Throughput and failure counters are unaffected.

### Failure Recording

- **`public void RecordFailure()`**  
  Increments the failure counter by one. This affects `ErrorRatePercent` and `SuccessRatePercent`.

### Aggregation & Reporting

- **`public async Task<MetricAggregation> CreateMetricAggregationAsync()`**  
  Asynchronously builds a `MetricAggregation` snapshot containing current throughput, success/error rates, and latency percentiles (average, P95, P99). The task completes when all underlying counters have been read consistently.  
  *Returns*: a `MetricAggregation` object with the latest values.

- **`public async Task<HealthReport> GenerateHealthReportAsync()`**  
  Asynchronously produces a `HealthReport` that evaluates the pipeline status against predefined thresholds (e.g., error rate > 5% triggers a degraded status).  
  *Returns*: a `HealthReport` with a status enumeration, a human-readable message, and the underlying metric snapshot.

- **`public async Task<PerformanceTrend> AnalyzePerformanceTrendAsync()`**  
  Asynchronously computes a `PerformanceTrend` by comparing recent throughput and latency against historical baselines stored internally.  
  *Returns*: a `PerformanceTrend` indicating whether performance is improving, stable, or degrading.

- **`public async Task<MetricDistribution> GetMetricDistributionAsync()`**  
  Asynchronously returns a `MetricDistribution` that describes the histogram of recorded processing times (buckets, counts, min, max).  
  *Returns*: a `MetricDistribution` object built from the current sample reservoir.

### Properties

- **`public string Status`**  
  Gets a textual status label derived from the most recent health evaluation (e.g., `"Healthy"`, `"Degraded"`, `"Unhealthy"`). Returns `"Unknown"` before the first health report is generated.

- **`public string Message`**  
  Gets a human-readable message associated with the current status. Returns an empty string if no health report has been generated.

- **`public double ThroughputItemsPerSecond`**  
  Gets the most recently calculated throughput value (items per second). Updated on each call to `GetThroughput()` or aggregation methods.

- **`public double SuccessRatePercent`**  
  Gets the percentage of successful operations (0–100) based on recorded throughput and failures since the last reset or service start.

- **`public double ErrorRatePercent`**  
  Gets the percentage of failed operations (0–100). Always equal to `100.0 - SuccessRatePercent`.

- **`public double AverageProcessingTimeMs`**  
  Gets the arithmetic mean of recorded processing times in milliseconds. Returns `0.0` when no samples exist.

- **`public double P95ProcessingTimeMs`**  
  Gets the 95th percentile of recorded processing times in milliseconds. Returns `0.0` when fewer than the minimum required samples are available.

- **`public double P99ProcessingTimeMs`**  
  Gets the 99th percentile of recorded processing times in milliseconds. Returns `0.0` when fewer than the minimum required samples are available.

## Usage

### Example 1: Basic Pipeline Monitoring

```csharp
var metrics = new MetricsService();

// Simulate processing 100 items with per-item timing
for (int i = 0; i < 100; i++)
{
    var sw = Stopwatch.StartNew();
    ProcessItem(i);
    sw.Stop();

    metrics.RecordThroughput();
    metrics.RecordProcessingTime(sw.Elapsed);

    if (i % 20 == 0 && RandomFailure())
    {
        metrics.RecordFailure();
    }
}

// Retrieve current snapshot
MetricAggregation agg = await metrics.CreateMetricAggregationAsync();
Console.WriteLine($"Throughput: {metrics.ThroughputItemsPerSecond:F2} items/s");
Console.WriteLine($"Success Rate: {metrics.SuccessRatePercent:F1}%");
Console.WriteLine($"P99 Latency: {metrics.P99ProcessingTimeMs:F2} ms");
```

### Example 2: Health Reporting with Trend Analysis

```csharp
var metrics = new MetricsService();

// Batch record throughput
metrics.RecordThroughput(500);

// Record processing times from a pre-collected list
foreach (var duration in collectedDurations)
{
    metrics.RecordProcessingTime(duration);
}

// Generate a health report
HealthReport report = await metrics.GenerateHealthReportAsync();
Console.WriteLine($"Status: {report.Status}");
Console.WriteLine($"Message: {report.Message}");

if (report.Status == HealthStatus.Degraded)
{
    PerformanceTrend trend = await metrics.AnalyzePerformanceTrendAsync();
    Console.WriteLine($"Trend: {trend.Direction}");

    MetricDistribution dist = await metrics.GetMetricDistributionAsync();
    Console.WriteLine($"Latency spread: {dist.MinMs}–{dist.MaxMs} ms");
}

// Reset latency data for next window
metrics.ClearProcessingTimes();
```

## Notes

- **Thread safety**: All `Record*` methods and property getters use lightweight synchronization (e.g., `Interlocked` operations or fine-grained locks). Concurrent calls to `RecordThroughput`, `RecordProcessingTime`, and `RecordFailure` are safe without external locking.
- **Async aggregation consistency**: `CreateMetricAggregationAsync`, `GenerateHealthReportAsync`, `AnalyzePerformanceTrendAsync`, and `GetMetricDistributionAsync` internally acquire a consistent snapshot. They do not block recording methods while executing.
- **Empty-state behavior**: Before any data is recorded, throughput and latency properties return `0.0`, `Status` returns `"Unknown"`, and `Message` returns `""`. Percentile properties (`P95`, `P99`) require a minimum sample count (implementation-defined) to produce meaningful values; otherwise they return `0.0`.
- **`ClearProcessingTimes` scope**: Only removes latency samples. Throughput counters, failure counters, and derived rates are preserved. To fully reset the service, create a new instance.
- **Windowed throughput**: `GetThroughput(TimeSpan window)` relies on internally timestamped throughput events. If no events fall within the requested window, it returns `0.0`.
- **Health thresholds**: The specific thresholds used by `GenerateHealthReportAsync` (e.g., error rate, P99 latency ceiling) are implementation-defined and may be configurable via constructor overloads or configuration injection (not shown in the public surface documented here).
