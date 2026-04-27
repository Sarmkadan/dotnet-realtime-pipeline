# API Reference

Complete technical reference for dotnet-realtime-pipeline's public API.

## Core Classes

### PipelineOrchestrator

Main entry point for pipeline operations.

```csharp
namespace DotNetRealtimePipeline.Services;

public class PipelineOrchestrator
{
    // Lifecycle Management
    public Task StartAsync();
    public Task StopAsync();
    public bool IsRunning { get; }

    // Data Ingestion
    public Task<bool> IngestDataPointAsync(DataPoint dataPoint);
    public Task<int> IngestBatchAsync(IEnumerable<DataPoint> dataPoints);

    // Status and Monitoring
    public PipelineStatus GetStatus();
    public Task<HealthReport> GetHealthReportAsync();
    public List<MetricAggregation> GetMetricsHistory(int count = 100);

    // Query
    public Task<IEnumerable<DataPoint>> QueryDataPointsAsync(
        long startTimeMs, long endTimeMs, string source = null);
}
```

#### Methods

##### `StartAsync()`
- **Purpose**: Initialize and start the pipeline
- **Returns**: `Task` (asynchronous operation)
- **Throws**: `InvalidOperationException` if already running
- **Example**:
```csharp
await orchestrator.StartAsync();
```

##### `StopAsync()`
- **Purpose**: Gracefully stop the pipeline
- **Returns**: `Task` (asynchronous operation)
- **Throws**: `InvalidOperationException` if not running
- **Example**:
```csharp
await orchestrator.StopAsync();
```

##### `IngestDataPointAsync(DataPoint dataPoint)`
- **Purpose**: Ingest a single data point
- **Parameters**:
  - `dataPoint`: The data to ingest
- **Returns**: `Task<bool>` - true if accepted, false if rejected
- **Throws**: `PipelineException` on unrecoverable error
- **Example**:
```csharp
var point = new DataPoint(1, DateTime.UtcNow.Ticks, 42.5m, "Sensor-1");
bool accepted = await orchestrator.IngestDataPointAsync(point);
if (!accepted) { /* handle backpressure */ }
```

##### `IngestBatchAsync(IEnumerable<DataPoint> dataPoints)`
- **Purpose**: Ingest multiple data points efficiently
- **Parameters**:
  - `dataPoints`: Collection of data points
- **Returns**: `Task<int>` - number of successfully ingested points
- **Example**:
```csharp
var points = new[] { point1, point2, point3 };
int ingested = await orchestrator.IngestBatchAsync(points);
```

##### `GetStatus()`
- **Purpose**: Get current pipeline status (synchronous)
- **Returns**: `PipelineStatus` object
- **Example**:
```csharp
var status = orchestrator.GetStatus();
Console.WriteLine($"Processed: {status.TotalDataPointsProcessed}");
```

##### `GetHealthReportAsync()`
- **Purpose**: Generate comprehensive health report
- **Returns**: `Task<HealthReport>` with detailed metrics
- **Example**:
```csharp
var health = await orchestrator.GetHealthReportAsync();
if (health.Status == HealthStatus.UNHEALTHY) { /* alert */ }
```

### DataProcessingService

Handles data validation, quality scoring, and transformation.

```csharp
namespace DotNetRealtimePipeline.Services;

public class DataProcessingService
{
    // Processing
    public Task<ProcessingResult> ProcessDataPointAsync(DataPoint dataPoint);
    public Task<List<ProcessingResult>> ProcessBatchAsync(IEnumerable<DataPoint> dataPoints);

    // Analysis
    public DataQualityAnalysis AnalyzeDataQuality(IEnumerable<DataPoint> dataPoints);
    public ValidationResult ValidateDataPoint(DataPoint dataPoint);
    public bool IsOutlier(decimal value, decimal threshold = 2.0m);
}
```

#### Methods

##### `ProcessDataPointAsync(DataPoint dataPoint)`
- **Purpose**: Process and validate a single point
- **Returns**: `Task<ProcessingResult>` with validation status
- **Example**:
```csharp
var result = await processingService.ProcessDataPointAsync(dataPoint);
Console.WriteLine($"Status: {result.Status}");
Console.WriteLine($"Quality: {result.QualityScore:P2}");
```

##### `AnalyzeDataQuality(IEnumerable<DataPoint> dataPoints)`
- **Purpose**: Analyze quality metrics for a collection
- **Returns**: `DataQualityAnalysis` with statistics
- **Properties**:
  - `AverageQualityScore`: 0-1 decimal
  - `ValidPointsCount`: Count of valid points
  - `InvalidPointsCount`: Count of invalid points
  - `PointQualityDetails`: Per-point analysis
- **Example**:
```csharp
var analysis = processingService.AnalyzeDataQuality(dataPoints);
if (analysis.AverageQualityScore < 0.8m) { /* low quality alert */ }
```

### WindowingService

Manages time-based windows and aggregations.

```csharp
namespace DotNetRealtimePipeline.Services;

public class WindowingService
{
    // Window Creation
    public WindowEvent CreateWindow(long startTimeMs);
    public List<WindowEvent> AssignDataPointsToWindows(IEnumerable<DataPoint> dataPoints);

    // Statistics
    public WindowStatistics CalculateWindowStatistics(WindowEvent window);
    public List<WindowEvent> GetActiveWindows();

    // Configuration
    public void UpdateWindowConfiguration(long sizeMs, long slideMs);
    public WindowType CurrentWindowType { get; set; }
}
```

#### Methods

##### `CreateWindow(long startTimeMs)`
- **Purpose**: Create a new window starting at given time
- **Parameters**:
  - `startTimeMs`: Start time in milliseconds (Unix epoch)
- **Returns**: `WindowEvent` object
- **Example**:
```csharp
var window = windowingService.CreateWindow(1609459200000);
```

##### `AssignDataPointsToWindows(IEnumerable<DataPoint> dataPoints)`
- **Purpose**: Assign collection to appropriate windows
- **Returns**: `List<WindowEvent>` with assigned data
- **Example**:
```csharp
var windows = windowingService.AssignDataPointsToWindows(dataPoints);
foreach (var window in windows)
{
    var stats = windowingService.CalculateWindowStatistics(window);
    // Process stats
}
```

##### `CalculateWindowStatistics(WindowEvent window)`
- **Purpose**: Compute statistics for a window
- **Returns**: `WindowStatistics` with aggregations
- **Properties**:
  - `Count`: Number of points
  - `Sum`: Total of all values
  - `Average`: Mean value
  - `Minimum`: Smallest value
  - `Maximum`: Largest value
  - `StandardDeviation`: Spread metric
  - `Percentile50/95/99`: Percentiles
- **Example**:
```csharp
var stats = windowingService.CalculateWindowStatistics(window);
Console.WriteLine($"Avg: {stats.Average}, P95: {stats.Percentile95}");
```

### MetricsService

Collects and analyzes pipeline performance.

```csharp
namespace DotNetRealtimePipeline.Services;

public class MetricsService
{
    // Health
    public Task<HealthReport> GenerateHealthReportAsync();
    public Task<PerformanceTrend> AnalyzePerformanceTrendAsync();

    // Metrics Management
    public void RecordMetric(MetricAggregation metric);
    public List<MetricAggregation> GetMetricsHistory(int count = 100);
    public void ClearMetricsHistory();
}
```

#### Methods

##### `GenerateHealthReportAsync()`
- **Purpose**: Create comprehensive health snapshot
- **Returns**: `Task<HealthReport>`
- **Properties of HealthReport**:
  - `Status`: HEALTHY, DEGRADED, or UNHEALTHY
  - `ThroughputItemsPerSecond`: Current rate
  - `AverageLatencyMs`: Processing time
  - `ErrorRate`: Percentage of failures
  - `MemoryUsageMb`: RAM consumption
  - `Alerts`: List of issues
- **Example**:
```csharp
var health = await metricsService.GenerateHealthReportAsync();
if (health.ErrorRate > 0.05) { /* high error rate */ }
```

##### `AnalyzePerformanceTrendAsync()`
- **Purpose**: Detect performance trends
- **Returns**: `Task<PerformanceTrend>`
- **Properties**:
  - `Direction`: UP, DOWN, STABLE, OSCILLATING
  - `SlopeValue`: Trend magnitude
  - `DataPoints`: Trend points
- **Example**:
```csharp
var trend = await metricsService.AnalyzePerformanceTrendAsync();
if (trend.Direction == "DOWN") { /* degrading */ }
```

### BackpressureService

Manages buffer and flow control.

```csharp
namespace DotNetRealtimePipeline.Services;

public class BackpressureService
{
    // Context
    public BackpressureContext CreateContext(string stageName, int maxCapacity);

    // Buffer Management
    public bool TryAddToBuffer(string stageName, int itemCount);
    public Dictionary<string, int> GetBufferStatus();

    // Backpressure Handling
    public Task<BackpressureResponse> ApplyBackpressureAsync(
        string stageName, BackpressureStrategy strategy, int timeoutMs);
}
```

#### Methods

##### `TryAddToBuffer(string stageName, int itemCount)`
- **Purpose**: Check if items can be added to buffer
- **Parameters**:
  - `stageName`: Pipeline stage identifier
  - `itemCount`: Number of items to add
- **Returns**: `bool` - true if space available, false otherwise
- **Example**:
```csharp
if (backpressureService.TryAddToBuffer("Ingestion", 100))
{
    // Add items
}
else
{
    // Handle backpressure
}
```

##### `ApplyBackpressureAsync(string stageName, BackpressureStrategy strategy, int timeoutMs)`
- **Purpose**: Apply backpressure handling strategy
- **Parameters**:
  - `stageName`: Pipeline stage
  - `strategy`: Block, Throttle, or Drop
  - `timeoutMs`: Maximum wait time
- **Returns**: `Task<BackpressureResponse>` with outcome
- **Example**:
```csharp
var response = await backpressureService.ApplyBackpressureAsync(
    "Ingestion", BackpressureStrategy.Block, 5000);
```

### QueryService

Provides data search and analysis.

```csharp
namespace DotNetRealtimePipeline.Services;

public class QueryService
{
    // Search
    public Task<IEnumerable<DataPoint>> SearchDataPointsAsync(
        long startTime, long endTime, string source = null, decimal minQualityScore = 0);

    // Analysis
    public Task<AggregateStatistics> GetAggregateStatisticsAsync(long startMs, long endMs);
    public Task<List<TrendPoint>> AnalyzeTrendsAsync(long startMs, long endMs, long intervalMs);
    public Task<List<DataPoint>> GetOutliersAsync(long startMs, long endMs, decimal threshold = 2.0m);
}
```

#### Methods

##### `SearchDataPointsAsync(...)`
- **Purpose**: Search for data with filters
- **Parameters**:
  - `startTime`: Start time (ticks)
  - `endTime`: End time (ticks)
  - `source`: Optional source filter
  - `minQualityScore`: Minimum quality (0-1)
- **Returns**: `Task<IEnumerable<DataPoint>>`
- **Example**:
```csharp
var results = await queryService.SearchDataPointsAsync(
    startTime: oneHourAgo,
    endTime: now,
    source: "Sensor-1",
    minQualityScore: 0.8m
);
```

##### `GetAggregateStatisticsAsync(long startMs, long endMs)`
- **Purpose**: Get statistics for time range
- **Returns**: `Task<AggregateStatistics>` with aggregates
- **Example**:
```csharp
var stats = await queryService.GetAggregateStatisticsAsync(startMs, endMs);
Console.WriteLine($"Average: {stats.Average}");
```

## Domain Models

### DataPoint

```csharp
public class DataPoint
{
    public long Id { get; set; }
    public long Timestamp { get; set; }  // UTC ticks
    public decimal Value { get; set; }
    public string Source { get; set; }
    public decimal Quality { get; set; }  // 0-1
}
```

### WindowEvent

```csharp
public class WindowEvent
{
    public Guid WindowId { get; set; }
    public long StartTimeMs { get; set; }
    public long EndTimeMs { get; set; }
    public WindowType Type { get; set; }
    public List<DataPoint> DataPoints { get; set; }
}
```

### HealthReport

```csharp
public class HealthReport
{
    public HealthStatus Status { get; set; }
    public double ThroughputItemsPerSecond { get; set; }
    public double AverageLatencyMs { get; set; }
    public double MinLatencyMs { get; set; }
    public double MaxLatencyMs { get; set; }
    public double ErrorRate { get; set; }
    public double MemoryUsageMb { get; set; }
    public List<string> Alerts { get; set; }
}
```

## Enums

### WindowType
- `TUMBLING`: Non-overlapping fixed windows
- `SLIDING`: Overlapping sliding windows
- `SESSION`: Activity-based windows
- `GLOBAL`: Single window for all data

### BackpressureStrategy
- `Block`: Pauses ingestion
- `Throttle`: Reduces ingestion rate
- `Drop`: Discards oldest items

### HealthStatus
- `HEALTHY`: Normal operation
- `DEGRADED`: Performance issues
- `UNHEALTHY`: Non-operational

## Configuration

### PipelineConfig

```csharp
var config = new PipelineConfig
{
    MaxBufferSize = 10000,
    BufferFlushIntervalMs = 1000,
    MaxConcurrentConsumers = 4,
    WindowSizeMs = 5000,
    WindowSlideMs = 1000,
    WindowType = WindowType.SLIDING,
    MaxRetries = 3,
    ProcessingTimeoutMs = 30000,
    BackpressureThreshold = 0.8m,
    BackpressureStrategy = BackpressureStrategy.Block,
    MinQualityScore = 0.5m,
    EnableQualityAnalysis = true,
    EnableMetrics = true,
    MetricsHistorySize = 1000
};
```

## Dependency Injection

```csharp
// Register all services
services.AddPipelineServices();

// Register with custom configuration
services.AddPipelineServices(config =>
{
    config.MaxBufferSize = 50000;
    config.WindowSizeMs = 10000;
});
```

## Exception Hierarchy

```
Exception
└─ PipelineException (base)
   ├─ ProcessingException
   ├─ ValidationException
   ├─ ConfigurationException
   └─ TimeoutException
```

## Thread Safety

All public methods are **thread-safe**:
- Multiple concurrent ingestion tasks: ✅ Safe
- Concurrent reads and writes: ✅ Safe (lock-based)
- Metric collection from multiple threads: ✅ Safe
- Concurrent window assignments: ✅ Safe

## Performance Characteristics

| Operation | Latency |
|---|---|
| Data ingestion | 0.1-1ms |
| Buffer status check | <0.1ms |
| Validation | 0.5-2ms |
| Window assignment | 1-5ms |
| Statistics calculation | 5-20ms |
| Query (in-memory) | <1ms |
| Health report generation | 10-50ms |

## Async/Await Guidelines

All I/O operations use `async/await`:
- Never block on `.Result` or `.Wait()`
- Always use `await` for `Task` returns
- Prefer `Task<T>` over `Task`
- Configure `.ConfigureAwait(false)` in library code

## Backward Compatibility

This library maintains semantic versioning:
- **Major version** changes may break API
- **Minor version** adds features, maintains compatibility
- **Patch version** fixes bugs, maintains compatibility
