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

## Additional Classes

### ThroughputCounter

Tracks events-per-second throughput using a sliding time window.

```csharp
namespace DotNetRealtimePipeline.Metrics;

public sealed class ThroughputCounter : IPipelineMetrics
{
    // Lifecycle Management
    public ThroughputCounter(int windowSeconds = 60);

    // Throughput Recording
    public void RecordEvents(long count);
    public void RecordEvents(string stageName, long count);

    // Throughput Query
    public double GetThroughput();
    public double GetThroughput(string stageName);
}
```

#### Methods

##### `ThroughputCounter(int windowSeconds = 60)`
- **Purpose**: Initialize the throughput counter with a sliding window
- **Parameters**:
  - `windowSeconds`: Length of the sliding window in seconds (default 60)
- **Exceptions**: `ArgumentOutOfRangeException` if windowSeconds <= 0

##### `RecordEvents(long count)`
- **Purpose**: Record events for the global counter
- **Parameters**:
  - `count`: Number of events to record
- **Example**:
```csharp
throughputCounter.RecordEvents(1);
```

##### `RecordEvents(string stageName, long count)`
- **Purpose**: Record events for a specific stage
- **Parameters**:
  - `stageName`: Name of the pipeline stage
  - `count`: Number of events to record
- **Exceptions**: `ArgumentException` if stageName is empty
- **Example**:
```csharp
throughputCounter.RecordEvents("Ingestion", 100);
```

##### `GetThroughput()`
- **Purpose**: Get global events-per-second throughput
- **Returns**: `double` - events per second
- **Example**:
```csharp
double rate = throughputCounter.GetThroughput();
Console.WriteLine($"Throughput: {rate:F2}/sec");
```

##### `GetThroughput(string stageName)`
- **Purpose**: Get events-per-second throughput for a specific stage
- **Parameters**:
  - `stageName`: Name of the pipeline stage
- **Returns**: `double` - events per second for the stage
- **Example**:
```csharp
double ingestionRate = throughputCounter.GetThroughput("Ingestion");
```

### ExponentialBackoffRetryPolicy

An `IRetryPolicy` that retries transient failures with exponential backoff and random jitter.

```csharp
namespace DotNetRealtimePipeline.DeadLetter;

public sealed class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    // Lifecycle Management
    public ExponentialBackoffRetryPolicy(RetryPolicyOptions? options = null);

    // IRetryPolicy Implementation
    public int MaxAttempts { get; }
    public bool IsTransient(Exception exception);
    public TimeSpan GetBackoffDelay(int attemptNumber);
    public Task<RetryResult<T>> ExecuteAsync<T>(Func<int, Task<T>> operation, CancellationToken cancellationToken = default);
}
```

#### Methods

##### `ExponentialBackoffRetryPolicy(RetryPolicyOptions? options = null)`
- **Purpose**: Initialize the retry policy with optional configuration
- **Parameters**:
  - `options`: The retry configuration; when omitted, defaults are used
- **Example**:
```csharp
var policy = new ExponentialBackoffRetryPolicy();
var policyWithOptions = new ExponentialBackoffRetryPolicy(options);
```

##### `IsTransient(Exception exception)`
- **Purpose**: Determine if an exception is transient and worth retrying
- **Parameters**:
  - `exception`: The exception to check
- **Returns**: `bool` - true if the exception is transient
- **Exceptions**: `ArgumentNullException` if exception is null
- **Example**:
```csharp
if (policy.IsTransient(exception))
{
    // Retry the operation
}
```

##### `GetBackoffDelay(int attemptNumber)`
- **Purpose**: Calculate the backoff delay for a given attempt number
- **Parameters**:
  - `attemptNumber`: The attempt number (1-based)
- **Returns**: `TimeSpan` - the delay to wait before retrying
- **Exceptions**: `ArgumentOutOfRangeException` if attemptNumber < 1
- **Example**:
```csharp
TimeSpan delay = policy.GetBackoffDelay(3); // Delay for 3rd attempt
```

##### `ExecuteAsync<T>(Func<int, Task<T>> operation, CancellationToken cancellationToken = default)`
- **Purpose**: Execute an operation with retry logic
- **Parameters**:
  - `operation`: The operation to execute, receiving the attempt number
  - `cancellationToken`: Optional cancellation token
- **Returns**: `Task<RetryResult<T>>` - the result of the operation
- **Example**:
```csharp
var result = await policy.ExecuteAsync<int>(async attempt =>
{
    // Your operation here
    return await SomeOperationAsync();
});
if (result.IsSuccess)
{
    // Use result.Value
}
else
{
    // Handle result.Error
}
```

### RetryPolicyOptions

Configures how `ExponentialBackoffRetryPolicy` retries a failing operation.

```csharp
namespace DotNetRealtimePipeline.DeadLetter;

public sealed class RetryPolicyOptions
{
    // Configuration Properties
    public int MaxAttempts { get; set; }
    public TimeSpan BaseDelay { get; set; }
    public TimeSpan MaxDelay { get; set; }
    public double JitterFactor { get; set; }
    public Func<Exception, bool> TransientExceptionPredicate { get; set; }
}
```

#### Properties

##### `MaxAttempts`
- **Purpose**: Gets or sets the maximum number of attempts (including the first one)
- **Default**: 3
- **Exceptions**: `ArgumentOutOfRangeException` if value < 1
- **Example**:
```csharp
var options = new RetryPolicyOptions { MaxAttempts = 5 };
```

##### `BaseDelay`
- **Purpose**: Gets or sets the delay before the second attempt; each subsequent attempt doubles it
- **Default**: 200 milliseconds
- **Exceptions**: `ArgumentOutOfRangeException` if value is negative
- **Example**:
```csharp
var options = new RetryPolicyOptions { BaseDelay = TimeSpan.FromSeconds(1) };
```

##### `MaxDelay`
- **Purpose**: Gets or sets the upper bound applied to the computed backoff delay
- **Default**: 30 seconds
- **Exceptions**: `ArgumentOutOfRangeException` if value is negative
- **Example**:
```csharp
var options = new RetryPolicyOptions { MaxDelay = TimeSpan.FromMinutes(1) };
```

##### `JitterFactor`
- **Purpose**: Gets or sets the proportion of random jitter applied to each delay (0 = none, 0.25 = up to ±25% of the computed delay)
- **Default**: 0.25
- **Exceptions**: `ArgumentOutOfRangeException` if value is outside [0, 1]
- **Example**:
```csharp
var options = new RetryPolicyOptions { JitterFactor = 0.1 };
```

##### `TransientExceptionPredicate`
- **Purpose**: Gets or sets the predicate that decides whether an exception is transient
- **Default**: `DefaultTransientPredicate` (timeouts, I/O, socket and HTTP transport failures)
- **Example**:
```csharp
var options = new RetryPolicyOptions
{
    TransientExceptionPredicate = ex => ex is IOException || ex is TimeoutException
};
```

### WorkerOptions

Configuration options for background workers.

```csharp
namespace DotNetRealtimePipeline.Configuration;

public sealed class WorkerOptions
{
    // Configuration Properties
    public int MetricsAggregationIntervalMs { get; set; }
    public int HealthCheckIntervalMs { get; set; }
    public bool EnableProcessingWorker { get; set; }
    public bool EnableMetricsWorker { get; set; }
    public bool EnableHealthCheckWorker { get; set; }
}
```

#### Properties

##### `MetricsAggregationIntervalMs`
- **Purpose**: Gets or sets the interval for metrics aggregation in milliseconds
- **Default**: 5000 (5 seconds)
- **Example**:
```csharp
var options = new WorkerOptions { MetricsAggregationIntervalMs = 10000 };
```

##### `HealthCheckIntervalMs`
- **Purpose**: Gets or sets the interval for health checks in milliseconds
- **Default**: 10000 (10 seconds)
- **Example**:
```csharp
var options = new WorkerOptions { HealthCheckIntervalMs = 30000 };
```

##### `EnableProcessingWorker`
- **Purpose**: Gets or sets whether the processing worker is enabled
- **Default**: true
- **Example**:
```csharp
var options = new WorkerOptions { EnableProcessingWorker = false };
```

##### `EnableMetricsWorker`
- **Purpose**: Gets or sets whether the metrics worker is enabled
- **Default**: true
- **Example**:
```csharp
var options = new WorkerOptions { EnableMetricsWorker = false };
```

##### `EnableHealthCheckWorker`
- **Purpose**: Gets or sets whether the health check worker is enabled
- **Default**: true
- **Example**:
```csharp
var options = new WorkerOptions { EnableHealthCheckWorker = false };
```

### OutputFormatterFactory

Factory for creating output formatters.

```csharp
namespace DotNetRealtimePipeline.Formatters;

public static class OutputFormatterFactory
{
    // Factory Methods
    public static IOutputFormatter CreateJsonFormatter();
    public static IOutputFormatter CreateCsvFormatter();
    public static IOutputFormatter CreateXmlFormatter();
}
```

#### Methods

##### `CreateJsonFormatter()`
- **Purpose**: Create a JSON output formatter
- **Returns**: `IOutputFormatter` - a formatter that outputs JSON
- **Example**:
```csharp
var formatter = OutputFormatterFactory.CreateJsonFormatter();
string json = formatter.Format(dataPoint);
```

##### `CreateCsvFormatter()`
- **Purpose**: Create a CSV output formatter
- **Returns**: `IOutputFormatter` - a formatter that outputs CSV
- **Example**:
```csharp
var formatter = OutputFormatterFactory.CreateCsvFormatter();
string csv = formatter.Format(dataPoint);
```

##### `CreateXmlFormatter()`
- **Purpose**: Create an XML output formatter
- **Returns**: `IOutputFormatter` - a formatter that outputs XML
- **Example**:
```csharp
var formatter = OutputFormatterFactory.CreateXmlFormatter();
string xml = formatter.Format(dataPoint);
```

### MetricsExporterFactory

Factory for creating metrics exporters.

```csharp
namespace DotNetRealtimePipeline.Integration;

public static class MetricsExporterFactory
{
    // Factory Methods
    public static IMetricsExporter CreatePrometheus(ILogger<PrometheusMetricsExporter> logger);
    public static IMetricsExporter CreateHttp(string endpoint, HttpClient client, ILogger<HttpMetricsExporter> logger);
    public static CompositeMetricsExporter CreateComposite(ILogger<CompositeMetricsExporter> logger);
}
```

#### Methods

##### `CreatePrometheus(ILogger<PrometheusMetricsExporter> logger)`
- **Purpose**: Create a Prometheus metrics exporter
- **Parameters**:
  - `logger`: Logger for the exporter
- **Returns**: `IMetricsExporter` - a Prometheus metrics exporter
- **Example**:
```csharp
var exporter = MetricsExporterFactory.CreatePrometheus(logger);
await exporter.ExportAsync(metrics);
```

##### `CreateHttp(string endpoint, HttpClient client, ILogger<HttpMetricsExporter> logger)`
- **Purpose**: Create an HTTP metrics exporter
- **Parameters**:
  - `endpoint`: The HTTP endpoint to export metrics to
  - `client`: The HTTP client to use for requests
  - `logger`: Logger for the exporter
- **Returns**: `IMetricsExporter` - an HTTP metrics exporter
- **Exceptions**: `ArgumentNullException` if endpoint is null or whitespace, or if client is null
- **Example**:
```csharp
var exporter = MetricsExporterFactory.CreateHttp(
    "https://metrics.example.com/import",
    new HttpClient(),
    logger
);
await exporter.ExportAsync(metrics);
```

##### `CreateComposite(ILogger<CompositeMetricsExporter> logger)`
- **Purpose**: Create a composite metrics exporter that can manage multiple exporters
- **Parameters**:
  - `logger`: Logger for the composite exporter
- **Returns**: `CompositeMetricsExporter` - a composite metrics exporter
- **Example**:
```csharp
var composite = MetricsExporterFactory.CreateComposite(logger);
composite.AddExporter(MetricsExporterFactory.CreatePrometheus(logger));
composite.AddExporter(MetricsExporterFactory.CreateHttp(endpoint, client, logger));
await composite.ExportAsync(metrics);
```

### DataSourceManager

Manages external data sources for the pipeline.

```csharp
namespace DotNetRealtimePipeline.Integration;

public sealed class DataSourceManager
{
    // Lifecycle Management
    public DataSourceManager();

    // Data Source Registration
    public void RegisterDataSource<T>(string name, T dataSource) where T : IExternalDataSource;
    public bool UnregisterDataSource(string name);

    // Data Source Access
    public T GetDataSource<T>(string name) where T : IExternalDataSource;
    public IEnumerable<IExternalDataSource> GetAllDataSources();

    // Data Fetching
    public Task<object?> FetchDataAsync(string name, CancellationToken cancellationToken = default);
}
```

#### Methods

##### `DataSourceManager()`
- **Purpose**: Initialize the data source manager
- **Example**:
```csharp
var manager = new DataSourceManager();
```

##### `RegisterDataSource<T>(string name, T dataSource)`
- **Purpose**: Register an external data source
- **Parameters**:
  - `name`: Unique name for the data source
  - `dataSource`: The data source implementation
- **Type Parameters**:
  - `T`: The type of the data source, must implement IExternalDataSource
- **Example**:
```csharp
var manager = new DataSourceManager();
manager.RegisterDataSource("weatherApi", new WeatherDataSource());
```

##### `UnregisterDataSource(string name)`
- **Purpose**: Unregister a data source by name
- **Parameters**:
  - `name`: The name of the data source to unregister
- **Returns**: `bool` - true if the data source was found and removed, false otherwise
- **Example**:
```csharp
bool removed = manager.UnregisterDataSource("weatherApi");
```

##### `GetDataSource<T>(string name)`
- **Purpose**: Get a registered data source by name
- **Parameters**:
  - `name`: The name of the data source to retrieve
- **Type Parameters**:
  - `T`: The type of the data source to retrieve, must implement IExternalDataSource
- **Returns**: `T` - the data source implementation
- **Exceptions**: `InvalidOperationException` if no data source is registered with the given name
- **Example**:
```csharp
var weatherSource = manager.GetDataSource<IWeatherDataSource>("weatherApi");
```

##### `GetAllDataSources()`
- **Purpose**: Get all registered data sources
- **Returns**: `IEnumerable<IExternalDataSource>` - all registered data sources
- **Example**:
```csharp
foreach (var source in manager.GetAllDataSources())
{
    Console.WriteLine($"Registered source: {source.GetType().Name}");
}
```

##### `FetchDataAsync(string name, CancellationToken cancellationToken = default)`
- **Purpose**: Fetch data from a registered data source
- **Parameters**:
  - `name`: The name of the data source to fetch from
  - `cancellationToken`: Optional cancellation token
- **Returns**: `Task<object?>` - the fetched data, or null if the source is not found
- **Example**:
```csharp
var data = await manager.FetchDataAsync("weatherApi");
if (data != null)
{
    // Process the fetched data
}
```

### HookManager

Manages plugin hooks in the extension system.

```csharp
namespace DotNetRealtimePipeline.Plugins;

public sealed class HookManager
{
    // Lifecycle Management
    public HookManager();

    // Hook Registration
    public void RegisterHook<THook>(THook hook) where THook : class;
    public void UnregisterHook<THook>(THook hook) where THook : class;

    // Hook Invocation
    public Task InvokeHookAsync<THook>(Func<THook, Task> hookAction, CancellationToken cancellationToken = default) where THook : class;
    public Task InvokeHookAsync<THook, TArg>(Func<THook, TArg, Task> hookAction, TArg arg, CancellationToken cancellationToken = default) where THook : class;
}
```

#### Methods

##### `HookManager()`
- **Purpose**: Initialize the hook manager
- **Example**:
```csharp
var hookManager = new HookManager();
```

##### `RegisterHook<THook>(THook hook)`
- **Purpose**: Register a hook implementation
- **Parameters**:
  - `hook`: The hook implementation to register
- **Type Parameters**:
  - `THook`: The type of the hook interface
- **Example**:
```csharp
var hookManager = new HookManager();
hookManager.RegisterHook<IDataProcessingPlugin>(new MyDataProcessor());
```

##### `UnregisterHook<THook>(THook hook)`
- **Purpose**: Unregister a hook implementation
- **Parameters**:
  - `hook`: The hook implementation to unregister
- **Type Parameters**:
  - `THook`: The type of the hook interface
- **Example**:
```csharp
hookManager.UnregisterHook<IDataProcessingPlugin>(myProcessor);
```

##### `InvokeHookAsync<THook>(Func<THook, Task> hookAction, CancellationToken cancellationToken = default)`
- **Purpose**: Invoke a hook with no arguments
- **Parameters**:
  - `hookAction`: Action to perform on each registered hook
  - `cancellationToken`: Optional cancellation token
- **Type Parameters**:
  - `THook`: The type of the hook interface
- **Returns**: `Task` - completes when all hooks have been invoked
- **Example**:
```csharp
await hookManager.InvokeHookAsync<IDataProcessingPlugin>(async plugin =>
{
    await plugin.OnPipelineStartedAsync();
});
```

##### `InvokeHookAsync<THook, TArg>(Func<THook, TArg, Task> hookAction, TArg arg, CancellationToken cancellationToken = default)`
- **Purpose**: Invoke a hook with an argument
- **Parameters**:
  - `hookAction`: Action to perform on each registered hook with the argument
  - `arg`: The argument to pass to each hook
  - `cancellationToken`: Optional cancellation token
- **Type Parameters**:
  - `THook`: The type of the hook interface
  - `TArg`: The type of the argument
- **Returns**: `Task` - completes when all hooks have been invoked
- **Example**:
```csharp
await hookManager.InvokeHookAsync<IDataTransformPlugin, DataPoint>(
    async (plugin, point) => await plugin.TransformAsync(point),
    dataPoint
);
```

### PluginRegistry

Manages registered plugins in the extension system.

```csharp
namespace DotNetRealtimePipeline.Plugins;

public sealed class PluginRegistry
{
    // Lifecycle Management
    public PluginRegistry();

    // Plugin Registration
    public void RegisterPlugin<TPlugin>(TPlugin plugin) where TPlugin : IPipelinePlugin;
    public bool UnregisterPlugin<TPlugin>(TPlugin plugin) where TPlugin : IPipelinePlugin;

    // Plugin Query
    public TPlugin GetPlugin<TPlugin>() where TPlugin : IPipelinePlugin;
    public IEnumerable<TPlugin> GetPlugins<TPlugin>() where TPlugin : IPipelinePlugin;
    public int GetPluginCount<TPlugin>() where TPlugin : IPipelinePlugin;
}
```

#### Methods

##### `PluginRegistry()`
- **Purpose**: Initialize the plugin registry
- **Example**:
```csharp
var registry = new PluginRegistry();
```

##### `RegisterPlugin<TPlugin>(TPlugin plugin)`
- **Purpose**: Register a plugin implementation
- **Parameters**:
  - `plugin`: The plugin implementation to register
- **Type Parameters**:
  - `TPlugin`: The type of the plugin, must implement IPipelinePlugin
- **Example**:
```csharp
var registry = new PluginRegistry();
registry.RegisterPlugin<IDataProcessingPlugin>(new MyDataProcessor());
```

##### `UnregisterPlugin<TPlugin>(TPlugin plugin)`
- **Purpose**: Unregister a plugin implementation
- **Parameters**:
  - `plugin`: The plugin implementation to unregister
- **Type Parameters**:
  - `TPlugin`: The type of the plugin, must implement IPipelinePlugin
- **Returns**: `bool` - true if the plugin was found and removed, false otherwise
- **Example**:
```csharp
bool removed = registry.UnregisterPlugin<IDataProcessingPlugin>(myProcessor);
```

##### `GetPlugin<TPlugin>()`
- **Purpose**: Get a registered plugin implementation
- **Type Parameters**:
  - `TPlugin`: The type of the plugin to retrieve, must implement IPipelinePlugin
- **Returns**: `TPlugin` - the plugin implementation
- **Exceptions**: `InvalidOperationException` if no plugin is registered of the given type
- **Example**:
```csharp
var processor = registry.GetPlugin<IDataProcessingPlugin>();
```

##### `GetPlugins<TPlugin>()`
- **Purpose**: Get all registered plugins of a specific type
- **Type Parameters**:
  - `TPlugin`: The type of the plugins to retrieve, must implement IPipelinePlugin
- **Returns**: `IEnumerable<TPlugin>` - all registered plugins of the specified type
- **Example**:
```csharp
foreach (var plugin in registry.GetPlugins<IDataTransformPlugin>())
{
    // Process each transform plugin
}
```

##### `GetPluginCount<TPlugin>()`
- **Purpose**: Get the count of registered plugins of a specific type
- **Type Parameters**:
  - `TPlugin`: The type of the plugins to count, must implement IPipelinePlugin
- **Returns**: `int` - the number of registered plugins of the specified type
- **Example**:
```csharp
int count = registry.GetPluginCount<IOutputPlugin>();
```

### HttpClientBuilder

Builds configured HttpClient instances for external service communication.

```csharp
namespace DotNetRealtimePipeline.Integration;

public sealed class HttpClientBuilder
{
    // Lifecycle Management
    public HttpClientBuilder();

    // Configuration
    public HttpClientBuilder WithBaseAddress(Uri baseAddress);
    public HttpClientBuilder WithTimeout(TimeSpan timeout);
    public HttpClientBuilder WithDefaultHeaders(Action<HttpRequestHeaders> headersAction);
    public HttpClientBuilder WithMessageHandler(HttpMessageHandler handler);

    // Build
    public HttpClient Build();
}
```

#### Methods

##### `HttpClientBuilder()`
- **Purpose**: Initialize the HTTP client builder
- **Example**:
```csharp
var builder = new HttpClientBuilder();
```

##### `WithBaseAddress(Uri baseAddress)`
- **Purpose**: Set the base address for HTTP requests
- **Parameters**:
  - `baseAddress`: The base address for the HTTP client
- **Returns**: `HttpClientBuilder` - the same builder instance for chaining
- **Exceptions**: `ArgumentNullException` if baseAddress is null
- **Example**:
```csharp
var client = new HttpClientBuilder()
    .WithBaseAddress(new Uri("https://api.example.com/"))
    .Build();
```

##### `WithTimeout(TimeSpan timeout)`
- **Purpose**: Set the timeout for HTTP requests
- **Parameters**:
  - `timeout`: The timeout for HTTP requests
- **Returns**: `HttpClientBuilder` - the same builder instance for chaining
- **Exceptions**: `ArgumentOutOfRangeException` if timeout is negative
- **Example**:
```csharp
var client = new HttpClientBuilder()
    .WithTimeout(TimeSpan.FromSeconds(30))
    .Build();
```

##### `WithDefaultHeaders(Action<HttpRequestHeaders> headersAction)`
- **Purpose**: Configure default headers for HTTP requests
- **Parameters**:
  - `headersAction`: Action to configure the default headers
- **Returns**: `HttpClientBuilder` - the same builder instance for chaining
- **Exceptions**: `ArgumentNullException` if headersAction is null
- **Example**:
```csharp
var client = new HttpClientBuilder()
    .WithDefaultHeaders(headers =>
    {
        headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .Build();
```

##### `WithMessageHandler(HttpMessageHandler handler)`
- **Purpose**: Set a custom message handler for the HTTP client
- **Parameters**:
  - `handler`: The HTTP message handler to use
- **Returns**: `HttpClientBuilder` - the same builder instance for chaining
- **Exceptions**: `ArgumentNullException` if handler is null
- **Example**:
```csharp
var handler = new LoggingHandler(new HttpClientHandler());
var client = new HttpClientBuilder()
    .WithMessageHandler(handler)
    .Build();
```

##### `Build()`
- **Purpose**: Build the configured HttpClient instance
- **Returns**: `HttpClient` - the configured HTTP client
- **Example**:
```csharp
var client = new HttpClientBuilder()
    .WithBaseAddress(new Uri("https://api.example.com/"))
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithDefaultHeaders(headers =>
    {
        headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .Build();
```

### WorkerCoordinator

Coordinates background workers in the pipeline system.

```csharp
namespace DotNetRealtimePipeline.Workers;

public sealed class WorkerCoordinator : IDisposable
{
    // Lifecycle Management
    public WorkerCoordinator();
    public Task StartAsync(CancellationToken cancellationToken = default);
    public Task StopAsync(CancellationToken cancellationToken = default);

    // Worker Management
    public void RegisterWorker(IBackgroundWorker worker);
    public void UnregisterWorker(IBackgroundWorker worker);
    public IEnumerable<IBackgroundWorker> GetRegisteredWorkers();

    // Status
    public bool IsRunning { get; }
    public int WorkerCount { get; }
}
```

#### Methods

##### `WorkerCoordinator()`
- **Purpose**: Initialize the worker coordinator
- **Example**:
```csharp
var coordinator = new WorkerCoordinator();
```

##### `StartAsync(CancellationToken cancellationToken = default)`
- **Purpose**: Start all registered workers
- **Parameters**:
  - `cancellationToken`: Optional cancellation token
- **Returns**: `Task` - completes when all workers have started
- **Example**:
```csharp
await coordinator.StartAsync();
```

##### `StopAsync(CancellationToken cancellationToken = default)`
- **Purpose**: Stop all registered workers gracefully
- **Parameters**:
  - `cancellationToken`: Optional cancellation token
- **Returns**: `Task` - completes when all workers have stopped
- **Example**:
```csharp
await coordinator.StopAsync();
```

##### `RegisterWorker(IBackgroundWorker worker)`
- **Purpose**: Register a background worker with the coordinator
- **Parameters**:
  - `worker`: The worker to register
- **Exceptions**: `ArgumentNullException` if worker is null
- **Example**:
```csharp
var coordinator = new WorkerCoordinator();
coordinator.RegisterWorker(new ProcessingWorker());
```

##### `UnregisterWorker(IBackgroundWorker worker)`
- **Purpose**: Unregister a background worker from the coordinator
- **Parameters**:
  - `worker`: The worker to unregister
- **Exceptions**: `ArgumentNullException` if worker is null
- **Example**:
```csharp
coordinator.UnregisterWorker(processingWorker);
```

##### `GetRegisteredWorkers()`
- **Purpose**: Get all registered workers
- **Returns**: `IEnumerable<IBackgroundWorker>` - all registered workers
- **Example**:
```csharp
foreach (var worker in coordinator.GetRegisteredWorkers())
{
    Console.WriteLine($"Registered worker: {worker.GetType().Name}");
}
```

##### `IsRunning`
- **Purpose**: Gets whether the worker coordinator is currently running
- **Returns**: `bool` - true if the coordinator is running, false otherwise
- **Example**:
```csharp
if (coordinator.IsRunning)
{
    // Coordinator is active
}
```

##### `WorkerCount`
- **Purpose**: Gets the number of registered workers
- **Returns**: `int` - the number of registered workers
- **Example**:
```csharp
int count = coordinator.WorkerCount;
```

##### `Dispose()`
- **Purpose**: Dispose of the worker coordinator and unregister all workers
- **Example**:
```csharp
coordinator.Dispose();
```

### FileSystemMonitor

Monitors file system changes for configuration and data files.

```csharp
namespace DotNetRealtimePipeline.Utilities;

public sealed class FileSystemMonitor : IDisposable
{
    // Lifecycle Management
    public FileSystemMonitor();
    public Task StartAsync(string pathToMonitor, CancellationToken cancellationToken = default);
    public Task StopAsync(CancellationToken cancellationToken = default);

    // Event Handlers
    public event EventHandler<FileSystemEventArgs>? FileCreated;
    public event EventHandler<FileSystemEventArgs>? FileChanged;
    public event EventHandler<FileSystemEventArgs>? FileDeleted;
    public event EventHandler<RenamedEventArgs>? FileRenamed;

    // Status
    public bool IsMonitoring { get; }
}
```

#### Methods

##### `FileSystemMonitor()`
- **Purpose**: Initialize the file system monitor
- **Example**:
```csharp
var monitor = new FileSystemMonitor();
```

##### `StartAsync(string pathToMonitor, CancellationToken cancellationToken = default)`
- **Purpose**: Start monitoring a directory for file system changes
- **Parameters**:
  - `pathToMonitor`: The directory path to monitor
  - `cancellationToken`: Optional cancellation token
- **Returns**: `Task` - completes when the monitor has started
- **Exceptions**: `ArgumentException` if pathToMonitor is invalid, `DirectoryNotFoundException` if directory does not exist
- **Example**:
```csharp
await monitor.StartAsync("config");
```

##### `StopAsync(CancellationToken cancellationToken = default)`
- **Purpose**: Stop monitoring for file system changes
- **Parameters**:
  - `cancellationToken`: Optional cancellation token
- **Returns**: `Task` - completes when the monitor has stopped
- **Example**:
```csharp
await monitor.StopAsync();
```

##### `FileCreated`
- **Purpose**: Occurs when a file is created in the monitored directory
- **Event Type**: `EventHandler<FileSystemEventArgs>`
- **Example**:
```csharp
monitor.FileCreated += (sender, args) =>
{
    Console.WriteLine($"File created: {args.FullPath}");
};
```

##### `FileChanged`
- **Purpose**: Occurs when a file is changed in the monitored directory
- **Event Type**: `EventHandler<FileSystemEventArgs>`
- **Example**:
```csharp
monitor.FileChanged += (sender, args) =>
{
    Console.WriteLine($"File changed: {args.FullPath}");
};
```

##### `FileDeleted`
- **Purpose**: Occurs when a file is deleted in the monitored directory
- **Event Type**: `EventHandler<FileSystemEventArgs>`
- **Example**:
```csharp
monitor.FileDeleted += (sender, args) =>
{
    Console.WriteLine($"File deleted: {args.FullPath}");
};
```

##### `FileRenamed`
- **Purpose**: Occurs when a file is renamed in the monitored directory
- **Event Type**: `EventHandler<RenamedEventArgs>`
- **Example**:
```csharp
monitor.FileRenamed += (sender, args) =>
{
    Console.WriteLine($"File renamed from {args.OldFullPath} to {args.FullPath}");
};
```

##### `IsMonitoring`
- **Purpose**: Gets whether the file system monitor is currently monitoring
- **Returns**: `bool` - true if the monitor is active, false otherwise
- **Example**:
```csharp
if (monitor.IsMonitoring)
{
    // Monitor is active
}
```

##### `Dispose()`
- **Purpose**: Dispose of the file system monitor and stop monitoring
- **Example**:
```csharp
monitor.Dispose();
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
