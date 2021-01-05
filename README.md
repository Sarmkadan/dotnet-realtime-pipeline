![Build](https://github.com/sarmkadan/dotnet-realtime-pipeline/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/dotnet-realtime-pipeline)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

# Real-Time Data Processing Pipeline for .NET

A high-performance, production-grade real-time data processing pipeline built with .NET 10. Designed for systems requiring low-latency ingestion, intelligent backpressure management, time-window aggregation, and comprehensive metrics collection.

## Table of Contents

- [Features](#features)
- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Core Components](#core-components)
- [Installation](#installation)
- [Usage Examples](#usage-examples)
- [Configuration Reference](#configuration-reference)
- [API Reference](#api-reference)
- [CLI Reference](#cli-reference)
- [Troubleshooting](#troubleshooting)
- [Performance](#performance)
- [Testing](#testing)
- [Related Projects](#related-projects)
- [Contributing](#contributing)
- [License](#license)

## Features

### Real-Time Processing
- **Low-latency data ingestion** with configurable pipeline stages
- **Asynchronous processing** leveraging System.IO.Pipelines for efficient memory usage
- **Stream-based architecture** for handling unbounded data sequences
- **Concurrent batch processing** with configurable parallelism

### Backpressure Management
- **Automatic buffer flow control** with three strategies: Block, Throttle, Drop
- **Dynamic pressure monitoring** based on buffer fill percentage
- **Configurable thresholds** for triggering backpressure policies
- **Graceful degradation** under peak load conditions
- **Timeout-aware handling** for blocking operations

### Time-Window Aggregation
- **Four window types**: Tumbling, Sliding, Session, Global
- **Statistical aggregations**: Sum, Average, Count, Min, Max, StdDev, Percentiles
- **Configurable window sizes and slide intervals** for flexible aggregation periods
- **Efficient window assignment** for high-throughput scenarios
- **Support for out-of-order data** within configurable lateness bounds

### Metrics & Monitoring
- **Throughput tracking** (items/sec) with real-time calculation
- **Latency metrics** including min, max, and average processing times
- **Error rate monitoring** with categorized failure tracking
- **Health check system** with detailed status reporting
- **Performance trending** with historical analysis
- **Custom event publishing** for integration with external monitoring systems
- **Backpressure metrics collection** with time-series history and per-stage activation counts *(new)*

### Pipeline Visualization
- **ASCII topology diagrams** showing all stages with live buffer levels and throughput
- **Per-stage health indicators** (HEALTHY / WARNING / CRITICAL) computed from buffer state
- **Compact single-line summaries** suitable for embedding in log lines or dashboards
- **CLI `visualize` command** for on-demand terminal rendering *(new)*

### Dead Letter Handling
- **Automatic capture** of failed data points routed to a dedicated dead-letter queue
- **Configurable retry budget** per entry with automatic backoff and exhaustion tracking
- **Inspect and replay** failed entries via `PeekAsync` and `DequeueForRetryAsync`
- **Permanent failure acknowledgement** with resolution notes for auditing
- **Queue statistics** (pending / in-retry / permanent-failure / total-resolved counts) *(new)*

### Quality Control
- **Data validation** with configurable validation rules
- **Quality scoring** (0-100) based on completeness and correctness
- **Configurable thresholds** for accepting/rejecting data
- **Outlier detection** using statistical methods
- **Data lineage tracking** for debugging and auditing

### Flexible Architecture
- **Modular service layer** with clear separation of concerns
- **Dependency injection support** via Microsoft.Extensions.DependencyInjection
- **Interface-based repositories** for pluggable data storage
- **Plugin system** for custom processing stages
- **Event-driven architecture** with pub/sub capabilities
- **REST API support** for external integrations
- **CLI tools** for operational tasks

### In-Memory State Management
- **Thread-safe repositories** with lock-based synchronization
- **Efficient data structures** optimized for read-heavy workloads
- **Rolling history management** with configurable retention policies
- **Snapshot capabilities** for state persistence and recovery

### Statistical Analysis
- **Percentile calculations** (P50, P95, P99, P999) for latency analysis
- **Moving averages** for trend smoothing
- **Trend analysis** with slope calculation and direction detection
- **Outlier detection** using Z-score and IQR methods
- **Variance and standard deviation** calculations

## Quick Start

### Installation

```bash
# Clone the repository
git clone https://github.com/Sarmkadan/dotnet-realtime-pipeline.git
cd dotnet-realtime-pipeline

# Build the project
dotnet build

# Run the application
dotnet run
```

### Basic Usage

```csharp
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddPipelineServices();
var serviceProvider = services.BuildServiceProvider();

// Get the orchestrator
var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();

// Start the pipeline
await orchestrator.StartAsync();

// Ingest a data point
var dataPoint = new DataPoint(
    id: 1,
    timestamp: DateTime.UtcNow.Ticks,
    value: 42.5,
    source: "Sensor-1"
);

bool accepted = await orchestrator.IngestDataPointAsync(dataPoint);
Console.WriteLine($"Data point accepted: {accepted}");

// Get current status
var status = orchestrator.GetStatus();
Console.WriteLine($"Processed: {status.TotalDataPointsProcessed}");
Console.WriteLine($"Failed: {status.TotalDataPointsFailed}");

// Get health report
var health = await orchestrator.GetHealthReportAsync();
Console.WriteLine($"Pipeline Health: {health.Status}");
Console.WriteLine($"Throughput: {health.ThroughputItemsPerSecond:F2} items/sec");

// Stop the pipeline
await orchestrator.StopAsync();
```

## Architecture

### System Design

```
┌──────────────────────────────────────────────────────────────────┐
│ Presentation Layer                                               │
│ ├─ REST API (RestApiHandler)                                    │
│ ├─ CLI (CommandExecutor, CommandLineParser)                     │
│ └─ Webhooks (WebhookHandler)                                    │
├──────────────────────────────────────────────────────────────────┤
│ Application Layer                                                │
│ └─ PipelineOrchestrator (orchestrates all services)             │
├──────────────────────────────────────────────────────────────────┤
│ Service Layer                                                    │
│ ├─ DataProcessingService (validation, transformation, quality) │
│ ├─ WindowingService (aggregation, windowing, statistics)        │
│ ├─ MetricsService (monitoring, health, trending)               │
│ ├─ BackpressureService (flow control, buffer management)        │
│ ├─ QueryService (data analysis and retrieval)                  │
│ └─ StateManager (state persistence)                            │
├──────────────────────────────────────────────────────────────────┤
│ Integration Layer                                                │
│ ├─ ExternalDataSource (upstream integrations)                  │
│ ├─ MetricsExporter (downstream integrations)                   │
│ ├─ HttpClientFactory (HTTP communication)                      │
│ └─ EventPublisher (pub/sub messaging)                          │
├──────────────────────────────────────────────────────────────────┤
│ Data Access Layer                                                │
│ ├─ IDataPointRepository                                         │
│ ├─ IMetricsRepository                                           │
│ └─ In-Memory Implementations (thread-safe)                      │
├──────────────────────────────────────────────────────────────────┤
│ Domain Layer                                                     │
│ ├─ Models (DataPoint, WindowEvent, MetricAggregation, etc)    │
│ ├─ Enums (WindowType, BackpressureStrategy, HealthStatus)     │
│ ├─ Exceptions (PipelineException hierarchy)                    │
│ └─ Constants (PipelineConstants)                               │
├──────────────────────────────────────────────────────────────────┤
│ Cross-Cutting Concerns                                           │
│ ├─ Middleware (ErrorHandling, Logging, RateLimiting)          │
│ ├─ Utilities (DateTime, Validation, Statistics, Serialization) │
│ └─ Monitoring (HealthCheckService, MetricsExporter)            │
└──────────────────────────────────────────────────────────────────┘
```

### Data Flow

1. **Ingestion**: Data enters via REST API, CLI, or external sources
2. **Validation**: DataProcessingService validates incoming data
3. **Buffering**: Data queued with backpressure management
4. **Windowing**: WindowingService assigns data to time windows
5. **Aggregation**: Statistics calculated over windowed data
6. **Storage**: Results persisted to repositories
7. **Monitoring**: Metrics collected and published
8. **Retrieval**: QueryService provides data access for consumers

### Backpressure Strategy

The pipeline automatically manages buffer overflow using three strategies:

- **Block**: Pauses ingestion until buffer drains (default)
- **Throttle**: Gradually reduces ingestion rate
- **Drop**: Silently discards oldest buffered items

## Core Components

### Services

#### DataProcessingService
Handles individual and batch data point processing with validation, quality scoring, and retry logic.

```csharp
// Single item processing
var result = await processingService.ProcessDataPointAsync(dataPoint);

// Batch processing
var results = await processingService.ProcessBatchAsync(dataPoints);

// Quality analysis
var analysis = processingService.AnalyzeDataQuality(dataPoints);
Console.WriteLine($"Quality Score: {analysis.AverageQualityScore}");
```

#### WindowingService
Manages time-based windowing with configurable window types and statistical aggregations.

```csharp
// Create a tumbling window
var window = windowingService.CreateWindow(startTimeMs);

// Assign data to windows
var windowedData = windowingService.AssignDataPointsToWindows(dataPoints);

// Calculate statistics
var stats = windowingService.CalculateWindowStatistics(window);
Console.WriteLine($"Average: {stats.Average}, Max: {stats.Maximum}");
```

#### MetricsService
Collects and analyzes pipeline performance metrics with health reporting and trend analysis.

```csharp
// Generate health report
var health = await metricsService.GenerateHealthReportAsync();
Console.WriteLine($"Status: {health.Status}");
Console.WriteLine($"Throughput: {health.ThroughputItemsPerSecond:F2} items/sec");

// Analyze performance trends
var trend = await metricsService.AnalyzePerformanceTrendAsync();
Console.WriteLine($"Trend Direction: {trend.Direction}");
```

#### BackpressureService
Manages flow control with configurable strategies and buffer monitoring.

```csharp
// Create buffer context
var context = backpressureService.CreateContext("StageName", maxCapacity: 10000);

// Check buffer availability
bool accepted = backpressureService.TryAddToBuffer("StageName", itemCount: 100);

// Apply backpressure strategy
var response = await backpressureService.ApplyBackpressureAsync(
    "StageName",
    BackpressureStrategy.Block,
    timeoutMs: 5000
);
```

#### QueryService
Provides data analysis and flexible retrieval operations.

```csharp
// Search with filters
var dataPoints = await queryService.SearchDataPointsAsync(
    startTime: startTicks,
    endTime: endTicks,
    source: "Sensor-1",
    minQualityScore: 0.8m
);

// Get aggregated statistics
var stats = await queryService.GetAggregateStatisticsAsync(startMs, endMs);

// Analyze trends
var trends = await queryService.AnalyzeTrendsAsync(startMs, endMs, intervalMs: 1000);
```

### Repositories

#### IDataPointRepository
Manages data point persistence and retrieval.

```csharp
// Add data
await repository.AddAsync(dataPoint);
await repository.AddRangeAsync(dataPoints);

// Retrieve
var point = await repository.GetByIdAsync(id);
var all = await repository.GetAllAsync();

// Query
var recent = await repository.GetBySourceAsync("Sensor-1");
var timeRange = await repository.GetByTimeRangeAsync(startTicks, endTicks);
```

#### IMetricsRepository
Manages metrics storage and retrieval with rolling history.

```csharp
// Store metrics
await repository.AddAsync(aggregation);

// Retrieve
var metrics = await repository.GetLatestAsync(count: 100);
var recent = await repository.GetByTimeRangeAsync(startMs, endMs);

// Analyze
var average = await repository.GetAverageMetricsAsync(startMs, endMs);
```

## Installation

### Prerequisites

- **.NET 10.0 SDK** or higher
- **C# 13+** language features
- 512 MB RAM minimum (2 GB recommended for high-throughput scenarios)
- Linux, macOS, or Windows

### From Source

```bash
# Clone repository
git clone https://github.com/Sarmkadan/dotnet-realtime-pipeline.git
cd dotnet-realtime-pipeline

# Restore dependencies
dotnet restore

# Build Release configuration
dotnet build -c Release

# Run
dotnet run -c Release
```

## Docker

### Build

```bash
# Build the application image
docker build -t dotnet-realtime-pipeline .
```

### Run

```bash
# Run the application container
docker run -p 8080:8080 dotnet-realtime-pipeline
```

### Docker Compose

```bash
# Start all services (Pipeline, Prometheus, Grafana, PostgreSQL)
docker-compose up -d

# View application logs
docker-compose logs -f pipeline
```


## Usage Examples

For quick snippets, see the examples below. For comprehensive, runnable examples, please check the [examples/](examples/) directory:

- [BasicUsage.cs](examples/BasicUsage.cs): Minimal pipeline setup and data ingestion.
- [AdvancedUsage.cs](examples/AdvancedUsage.cs): Custom pipeline configuration and error handling.
- [IntegrationExample.cs](examples/IntegrationExample.cs): Dependency injection setup for ASP.NET Core applications.

### Example 1: Simple Data Ingestion

```csharp
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();
var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

await orchestrator.StartAsync();

for (int i = 0; i < 1000; i++)
{
    var point = new DataPoint(
        id: i,
        timestamp: DateTime.UtcNow.Ticks,
        value: Random.Shared.NextDouble() * 100,
        source: "TestSensor"
    );
    await orchestrator.IngestDataPointAsync(point);
}

var status = orchestrator.GetStatus();
Console.WriteLine($"Processed: {status.TotalDataPointsProcessed}");

await orchestrator.StopAsync();
```

### Example 2: Multi-Source Processing

```csharp
var services = new ServiceCollection();
services.AddPipelineServices(config =>
{
    config.MaxBufferSize = 50000;
    config.WindowSizeMs = 5000;
    config.BackpressureThreshold = 0.8m;
});
var provider = services.BuildServiceProvider();
var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

await orchestrator.StartAsync();

string[] sensors = { "Temp-1", "Temp-2", "Humidity-1", "Pressure-1" };

foreach (var sensor in sensors)
{
    for (int i = 0; i < 500; i++)
    {
        var point = new DataPoint(
            id: i,
            timestamp: DateTime.UtcNow.Ticks,
            value: 20 + Random.Shared.NextGaussian() * 5,
            source: sensor
        );
        await orchestrator.IngestDataPointAsync(point);
        await Task.Delay(100);
    }
}

await Task.Delay(10000);
var health = await orchestrator.GetHealthReportAsync();
Console.WriteLine($"Throughput: {health.ThroughputItemsPerSecond:F2} items/sec");

await orchestrator.StopAsync();
```

### Example 3: Windowing and Aggregation

```csharp
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();
var windowingService = provider.GetRequiredService<WindowingService>();

var config = new PipelineConfig
{
    WindowSizeMs = 10000,
    WindowSlideMs = 5000,
    WindowType = WindowType.SLIDING
};

var dataPoints = new List<DataPoint>();
for (int i = 0; i < 100; i++)
{
    dataPoints.Add(new DataPoint(
        id: i,
        timestamp: DateTime.UtcNow.AddMilliseconds(i * 100).Ticks,
        value: i * 1.5m,
        source: "Sensor-1"
    ));
}

var windows = windowingService.AssignDataPointsToWindows(dataPoints);

foreach (var window in windows)
{
    var stats = windowingService.CalculateWindowStatistics(window);
    Console.WriteLine($"Window [{window.StartTimeMs}-{window.EndTimeMs}]:");
    Console.WriteLine($"  Count: {stats.Count}");
    Console.WriteLine($"  Average: {stats.Average:F2}");
    Console.WriteLine($"  Min: {stats.Minimum:F2}, Max: {stats.Maximum:F2}");
}
```

### Example 4: Backpressure Handling

```csharp
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();
var backpressureService = provider.GetRequiredService<BackpressureService>();

var context = backpressureService.CreateContext("IngestionStage", maxCapacity: 1000);

// Simulate high load
int itemsToAdd = 1500;
for (int i = 0; i < itemsToAdd; i++)
{
    bool accepted = backpressureService.TryAddToBuffer("IngestionStage", itemCount: 1);
    if (!accepted)
    {
        var response = await backpressureService.ApplyBackpressureAsync(
            "IngestionStage",
            BackpressureStrategy.Throttle,
            timeoutMs: 1000
        );
        Console.WriteLine($"Applied backpressure: {response.Status}");
    }
}
```

### Example 5: Health Monitoring

```csharp
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();
var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();
var metricsService = provider.GetRequiredService<MetricsService>();

await orchestrator.StartAsync();

// Ingest data
for (int i = 0; i < 1000; i++)
{
    var point = new DataPoint(i, DateTime.UtcNow.Ticks, i * 0.5m, "Sensor-1");
    await orchestrator.IngestDataPointAsync(point);
}

await Task.Delay(2000);

// Generate health report
var health = await metricsService.GenerateHealthReportAsync();
Console.WriteLine("Health Report:");
Console.WriteLine($"  Status: {health.Status}");
Console.WriteLine($"  Throughput: {health.ThroughputItemsPerSecond:F2} items/sec");
Console.WriteLine($"  Avg Latency: {health.AverageLatencyMs:F2} ms");
Console.WriteLine($"  Error Rate: {health.ErrorRate:P2}");

// Analyze trends
var trend = await metricsService.AnalyzePerformanceTrendAsync();
Console.WriteLine($"  Trend Direction: {trend.Direction}");

await orchestrator.StopAsync();
```

### Example 6: Data Quality Analysis

```csharp
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();
var processingService = provider.GetRequiredService<DataProcessingService>();

var dataPoints = new List<DataPoint>
{
    new(1, DateTime.UtcNow.Ticks, 100, "Sensor-1"),
    new(2, DateTime.UtcNow.Ticks, 105, "Sensor-1"),
    new(3, DateTime.UtcNow.Ticks, -999999, "Sensor-1"), // Outlier
    new(4, DateTime.UtcNow.Ticks, 98, "Sensor-1"),
};

var analysis = processingService.AnalyzeDataQuality(dataPoints);
Console.WriteLine($"Quality Score: {analysis.AverageQualityScore:P2}");
Console.WriteLine($"Valid Points: {analysis.ValidPointsCount}/{dataPoints.Count}");

foreach (var point in analysis.PointQualityDetails)
{
    Console.WriteLine($"  Point {point.Id}: Quality = {point.Quality:P2}");
}
```

### Example 7: Custom Configuration

```csharp
var config = new PipelineConfigurationBuilder("ProductionPipeline", "1.0.0")
    .WithHighPerformanceDefaults()
    .WithBufferConfiguration(maxBufferSize: 100000, flushIntervalMs: 500, concurrentConsumers: 16)
    .WithWindowingConfiguration(windowSizeMs: 1000, windowSlideMs: 500, windowType: "SLIDING")
    .WithStage("Ingestion", "SOURCE")
    .WithStage("Validation", "TRANSFORM")
    .WithStage("Aggregation", "AGGREGATE")
    .WithStage("Export", "SINK")
    .Build();

var services = new ServiceCollection();
services.AddPipelineServices(serviceConfig =>
{
    serviceConfig.MaxBufferSize = config.MaxBufferSize;
    serviceConfig.WindowSizeMs = config.WindowSizeMs;
    serviceConfig.WindowSlideMs = config.WindowSlideMs;
});

var provider = services.BuildServiceProvider();
var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

await orchestrator.StartAsync();
Console.WriteLine($"Pipeline '{config.PipelineName}' started with {config.Stages.Count} stages");
await orchestrator.StopAsync();
```

### Example 8: REST API Integration

```csharp
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();
var apiHandler = provider.GetRequiredService<RestApiHandler>();

// Start HTTP server
var cts = new CancellationTokenSource();
apiHandler.StartServer("http://localhost:5000", cts.Token);

// Ingest via API
using var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

var dataPoint = new DataPoint(1, DateTime.UtcNow.Ticks, 42.5m, "Sensor-1");
var json = JsonSerializer.Serialize(dataPoint);
var content = new StringContent(json, Encoding.UTF8, "application/json");

var response = await client.PostAsync("/api/datapoints", content);
Console.WriteLine($"Response: {response.StatusCode}");

cts.Cancel();
```

## Configuration Reference

### PipelineConfig

```csharp
public class PipelineConfig
{
    // Buffer Configuration
    public int MaxBufferSize { get; set; } = 10000;
    public int BufferFlushIntervalMs { get; set; } = 1000;
    public int MaxConcurrentConsumers { get; set; } = 4;

    // Windowing Configuration
    public long WindowSizeMs { get; set; } = 5000;
    public long WindowSlideMs { get; set; } = 1000;
    public WindowType WindowType { get; set; } = WindowType.TUMBLING;

    // Processing Configuration
    public int MaxRetries { get; set; } = 3;
    public int ProcessingTimeoutMs { get; set; } = 30000;

    // Backpressure Configuration
    public decimal BackpressureThreshold { get; set; } = 0.8m; // 80%
    public BackpressureStrategy BackpressureStrategy { get; set; } = BackpressureStrategy.Block;

    // Quality Control
    public decimal MinQualityScore { get; set; } = 0.5m;
    public bool EnableQualityAnalysis { get; set; } = true;

    // Monitoring
    public bool EnableMetrics { get; set; } = true;
    public int MetricsHistorySize { get; set; } = 1000;

    // Pipeline Metadata
    public string PipelineName { get; set; }
    public string Version { get; set; }
    public List<PipelineStage> Stages { get; set; } = new();
}
```

### EventServiceConfiguration

```csharp
public class EventServiceConfiguration
{
    public bool EnableEventPublishing { get; set; } = true;
    public int EventQueueCapacity { get; set; } = 10000;
    public int MaxSubscribers { get; set; } = 50;
    public int SubscriberTimeoutMs { get; set; } = 5000;
}
```

## ApiEndpointHandler

The `ApiEndpointHandler` is the base class for all REST API endpoint handlers in the pipeline. It provides a standardized response structure (`ApiResponse<T>`) and common error handling patterns for API endpoints. The class includes two concrete implementations:

- **`DataIngestionHandler`**: Handles data point ingestion via REST API, supporting both single data point and batch ingestion operations with comprehensive status tracking
- **`StatusHandler`**: Provides pipeline health and status information including throughput, success rates, and system metrics

Both handlers return structured responses with success status, data payload, message, status code, and timestamp.

### Example: Using DataIngestionHandler

```csharp
using DotNetRealtimePipeline.API;
using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddPipelineServices();

var provider = services.BuildServiceProvider();
var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

// Create the ingestion handler
await orchestrator.StartAsync();
var ingestionHandler = new DataIngestionHandler(orchestrator, loggerFactory.CreateLogger<DataIngestionHandler>());

// Ingest a single data point
var dataPoint = new DataPoint(
    id: 1,
    timestamp: DateTime.UtcNow.Ticks,
    value: 42.5m,
    source: "Sensor-1"
);

var singleResult = await ingestionHandler.IngestAsync(dataPoint);
Console.WriteLine($"Single Ingestion - Success: {singleResult.Success}, Status: {singleResult.StatusCode}");

// Ingest a batch of data points
var batch = new List<DataPoint>();
for (int i = 0; i < 10; i++)
{
    batch.Add(new DataPoint(
        id: i,
        timestamp: DateTime.UtcNow.Ticks,
        value: (decimal)Random.Shared.NextDouble() * 100,
        source: $"Sensor-{i}"
    ));
}

var batchResult = await ingestionHandler.IngestBatchAsync(batch);
Console.WriteLine($"Batch Ingestion - Success: {batchResult.Success}, Total: {batchResult.Data.TotalCount}, Successful: {batchResult.Data.SuccessfulCount}");

await orchestrator.StopAsync();
```

### Example: Using StatusHandler

```csharp
using DotNetRealtimePipeline.API;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddPipelineServices();

var provider = services.BuildServiceProvider();
var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

// Create the status handler
await orchestrator.StartAsync();
var statusHandler = new StatusHandler(orchestrator, loggerFactory.CreateLogger<StatusHandler>());

// Get pipeline status
var statusResult = await statusHandler.GetStatusAsync();

if (statusResult.Success)
{
    var status = statusResult.Data;
    Console.WriteLine($"Pipeline: {status.PipelineName} v{status.Version}");
    Console.WriteLine($"Status: {(status.IsRunning ? "Running" : "Stopped")}");
    Console.WriteLine($"Processed: {status.TotalProcessed} | Failed: {status.TotalFailed}");
    Console.WriteLine($"Health: {status.HealthStatus} | Throughput: {status.Throughput:F2} items/sec");
    Console.WriteLine($"Success Rate: {status.SuccessRate:P2} | Avg Latency: {status.AverageLatency:F2} ms");
}
else
{
    Console.WriteLine($"Error: {statusResult.Message} (Code: {statusResult.StatusCode})");
}

await orchestrator.StopAsync();
```

## API Reference

### PipelineOrchestrator

```csharp
// Lifecycle
Task StartAsync();
Task StopAsync();

// Data Ingestion
Task<bool> IngestDataPointAsync(DataPoint dataPoint);
Task<int> IngestBatchAsync(IEnumerable<DataPoint> dataPoints);

// Status and Monitoring
PipelineStatus GetStatus();
Task<HealthReport> GetHealthReportAsync();
List<MetricAggregation> GetMetricsHistory();

// Query
Task<IEnumerable<DataPoint>> QueryDataPointsAsync(
    long startTimeMs, long endTimeMs, string source = null);
```

### DataProcessingService

```csharp
Task<ProcessingResult> ProcessDataPointAsync(DataPoint dataPoint);
Task<List<ProcessingResult>> ProcessBatchAsync(IEnumerable<DataPoint> dataPoints);
DataQualityAnalysis AnalyzeDataQuality(IEnumerable<DataPoint> dataPoints);
ValidationResult ValidateDataPoint(DataPoint dataPoint);
```

### WindowingService

```csharp
WindowEvent CreateWindow(long startTimeMs);
List<WindowEvent> AssignDataPointsToWindows(IEnumerable<DataPoint> dataPoints);
WindowStatistics CalculateWindowStatistics(WindowEvent window);
List<WindowEvent> GetActiveWindows();
```

### MetricsService

```csharp
Task<HealthReport> GenerateHealthReportAsync();
Task<PerformanceTrend> AnalyzePerformanceTrendAsync();
void RecordMetric(MetricAggregation metric);
List<MetricAggregation> GetMetricsHistory(int count = 100);
```

### BackpressureService

```csharp
BackpressureContext CreateContext(string stageName, int maxCapacity);
bool TryAddToBuffer(string stageName, int itemCount);
Task<BackpressureResponse> ApplyBackpressureAsync(
    string stageName, BackpressureStrategy strategy, int timeoutMs);
Dictionary<string, int> GetBufferStatus();
```

### QueryService

```csharp
Task<IEnumerable<DataPoint>> SearchDataPointsAsync(
    long startTime, long endTime, string source = null, decimal minQualityScore = 0);
Task<AggregateStatistics> GetAggregateStatisticsAsync(long startMs, long endMs);
Task<List<TrendPoint>> AnalyzeTrendsAsync(long startMs, long endMs, long intervalMs);
Task<List<DataPoint>> GetOutliersAsync(long startMs, long endMs, decimal threshold = 2.0m);
```

## CLI Reference

```bash
# Show help
dotnet run -- --help

# Ingest data from file
dotnet run -- ingest --file data.json

# Generate health report
dotnet run -- health

# Query data
dotnet run -- query --start 2026-01-01 --end 2026-01-02 --source Sensor-1

# Export metrics
dotnet run -- export --format csv --output metrics.csv

# Analyze trends
dotnet run -- trends --interval 3600000 --window 86400000

# Render an ASCII pipeline topology diagram with live metrics
dotnet run -- visualize

# Compact single-line summary (useful in scripts / CI logs)
dotnet run -- visualize --compact
```

## Pipeline Visualization

The `PipelineVisualizer` service renders a live ASCII diagram of every pipeline stage
with its current buffer fill, throughput (events/sec), dropped item count, and a health
indicator.

```csharp
var visualizer = serviceProvider.GetRequiredService<PipelineVisualizer>();
var config     = serviceProvider.GetRequiredService<PipelineConfig>();

// Full block diagram
Console.WriteLine(visualizer.Render(config));

// Single-line summary
Console.WriteLine(visualizer.RenderCompact(config));
```

Example output:

```
  Pipeline: DefaultPipeline  (v1.0.0)
  ────────────────────────────────────────────────────────────────────────
  +------------------------------------------------------+
  | + Ingestion           (SOURCE)                       |
  |   Buffer : [....................................] 0.0%|
  |   EPS    :     0.00   Dropped:        0              |
  +------------------------------------------------------+
       │
       ▼
  +------------------------------------------------------+
  | + Windowing           (WINDOW)                       |
  |   Buffer : [....................................] 0.0%|
  |   EPS    :     0.00   Dropped:        0              |
  +------------------------------------------------------+
  ────────────────────────────────────────────────────────────────────────
  System health : HEALTHY
  Pipeline EPS  : 0.0
```

## Backpressure Metrics

`BackpressureMetricsCollector` builds a time-series history of backpressure events
by polling `BackpressureService`.  It tracks activation count, cumulative active
duration, peak buffer fill, and dropped items per stage.

```csharp
var collector = serviceProvider.GetRequiredService<BackpressureMetricsCollector>();

// Poll on a timer (e.g. every second)
collector.Poll();

// Per-stage metrics
var stageMetrics = collector.GetStageMetrics("Ingestion");
Console.WriteLine($"Activations : {stageMetrics?.ActivationCount}");
Console.WriteLine($"Peak buffer : {stageMetrics?.PeakBufferFillPercent:F1}%");
Console.WriteLine($"Dropped     : {stageMetrics?.TotalDroppedItems}");

// Pipeline-wide snapshot
var snapshot = collector.GetSnapshot();
Console.WriteLine($"Total activations : {snapshot.TotalActivations}");
Console.WriteLine($"Total dropped     : {snapshot.TotalDroppedItems}");

// Last 20 events across all stages
var events = collector.GetRecentEvents(20);
foreach (var e in events)
    Console.WriteLine($"{e.Timestamp:HH:mm:ss} | {e.StageName} | {(e.IsActivation ? "ACTIVATED" : "released")} | buf={e.BufferFillPercent:F0}%");
```

## Dead Letter Handling

The `IDeadLetterQueue` / `DeadLetterQueue` pair captures data points that could not
be processed and exposes them for inspection, retry, or permanent failure
acknowledgement.

```csharp
var dlq = serviceProvider.GetRequiredService<IDeadLetterQueue>();

// Enqueue a failed data point
await dlq.EnqueueAsync(dataPoint, stageName: "Transform", failureReason: "schema mismatch");

// Pick up entries for retry
var batch = await dlq.DequeueForRetryAsync(maxCount: 10);
foreach (var entry in batch)
{
    try
    {
        await Reprocess(entry.DataPoint);
        await dlq.AcknowledgeSuccessAsync(entry.EntryId);
    }
    catch (Exception ex)
    {
        entry.RetryFailed(ex.Message);
        if (!entry.CanRetry)
            await dlq.AcknowledgeFailureAsync(entry.EntryId, "exhausted retries");
    }
}

// Check queue health
var stats = await dlq.GetStatsAsync();
Console.WriteLine($"DLQ  pending={stats.PendingEntries}  permanent-failures={stats.PermanentFailureEntries}");
```

## PipelineEventPublisher

The `PipelineEventPublisher` class provides a pub/sub mechanism for decoupling event producers from consumers within the pipeline. It supports publishing various pipeline events such as data ingestion, processing completion, backpressure detection, metrics collection, and pipeline errors.

### Usage Example

```csharp
using DotNetRealtimePipeline.Events;
using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.Logging;

// Initialize the publisher
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<PipelineEventPublisher>();
var publisher = new PipelineEventPublisher(logger);

// Subscribe to data ingestion events
publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent), async (e) =>
{
    Console.WriteLine($"Ingested DataPoint ID: {e.DataPoint.Id} (CorrelationId: {e.CorrelationId})");
    await Task.CompletedTask;
});

// Publish an ingestion event
var dataPoint = new DataPoint(1, DateTime.UtcNow.Ticks, 42.5m, "Sensor-1");
await publisher.PublishDataIngestedAsync(dataPoint);

// Subscribe to processing completed events
publisher.Subscribe<ProcessingCompletedEventArgs>(nameof(ProcessingCompletedEvent), async (e) =>
{
    Console.WriteLine($"Processing completed for stage: {e.StageName}");
    Console.WriteLine($"Processed {e.Result.TotalProcessed} items, Success rate: {e.Result.SuccessRate:P2}");
    await Task.CompletedTask;
});

// Publish a processing completed event
await publisher.PublishProcessingCompletedAsync("DataProcessingStage", new ProcessingResult(100, 95, 5));

// Subscribe to backpressure detected events
publisher.Subscribe<BackpressureDetectedEventArgs>(nameof(BackpressureDetectedEvent), async (e) =>
{
    Console.WriteLine($"Backpressure detected in stage: {e.StageName}");
    Console.WriteLine($"Buffer fill: {e.Context.FillPercentage:P2}, Strategy: {e.Context.Strategy}");
    await Task.CompletedTask;
});

// Publish a backpressure detected event
var backpressureContext = new BackpressureContext("IngestionStage", 0.95m, BackpressureStrategy.Block);
await publisher.PublishBackpressureDetectedAsync(backpressureContext);

// Subscribe to metrics collected events
publisher.Subscribe<MetricsCollectedEventArgs>(nameof(MetricsCollectedEvent), async (e) =>
{
    Console.WriteLine($"Metrics collected at {e.Timestamp:HH:mm:ss}");
    Console.WriteLine($"Throughput: {e.Metrics.ThroughputItemsPerSecond:F2} items/sec");
    await Task.CompletedTask;
});

// Publish metrics collected event
var metrics = new MetricAggregation(1000, 950, 50, 0.98m, DateTime.UtcNow);
await publisher.PublishMetricsCollectedAsync(metrics);

// Subscribe to pipeline error events
publisher.Subscribe<PipelineErrorEventArgs>(nameof(PipelineErrorEvent), async (e) =>
{
    Console.WriteLine($"Pipeline error in operation: {e.OperationName}");
    Console.WriteLine($"Error: {e.Exception.Message}");
    await Task.CompletedTask;
});

// Publish a pipeline error event
try
{
    // Some pipeline operation that might fail
}
catch (Exception ex)
{
    await publisher.PublishPipelineErrorAsync("DataProcessing", ex);
}

// Get subscriber count
int subscriberCount = publisher.GetSubscriberCount();
Console.WriteLine($"Active subscribers: {subscriberCount}");

// Unsubscribe from an event
publisher.Unsubscribe<DataIngestedEventArgs>();
```

## WebhookHandler

The `WebhookHandler` class provides a webhook integration system that allows external services to receive real-time notifications when pipeline events occur. It manages webhook subscriptions, event delivery with HMAC signature verification, and retry logic for failed deliveries.

### Key Features
- **Subscription Management**: Register and manage webhook endpoints with filtering by event types
- **Secure Delivery**: HMAC-SHA256 signature verification using a shared secret
- **Retry Logic**: Automatic retry with exponential backoff for failed deliveries
- **Event Filtering**: Support for filtering events by type (DataIngested, ProcessingCompleted, BackpressureDetected, etc.)
- **Delivery Tracking**: Monitor last delivery time, failure count, and active status

### Usage Example

```csharp
using DotNetRealtimePipeline.Integration;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddPipelineServices();

var provider = services.BuildServiceProvider();
var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

// Create webhook handler with configuration
await orchestrator.StartAsync();
var webhookHandler = new WebhookHandler(
    id: "external-monitoring-service",
    url: "https://monitoring.example.com/api/webhooks/pipeline-events",
    eventTypes: WebhookEventType.DataIngested | WebhookEventType.ProcessingCompleted,
    secret: "your-webhook-secret-key",
    logger: loggerFactory.CreateLogger<WebhookHandler>()
);

// Register the webhook handler
webhookHandler.RegisterHandler();

// Subscribe to pipeline events and forward to webhook
var publisher = provider.GetRequiredService<PipelineEventPublisher>();

publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent), async (e) =>
{
    await webhookHandler.SendWebhookEventAsync(
        eventType: "data_ingested",
        data: new
        {
            dataPointId = e.DataPoint.Id,
            timestamp = e.DataPoint.Timestamp,
            value = e.DataPoint.Value,
            source = e.DataPoint.Source,
            correlationId = e.CorrelationId
        }
    );
});

publisher.Subscribe<ProcessingCompletedEventArgs>(nameof(ProcessingCompletedEvent), async (e) =>
{
    await webhookHandler.SendWebhookEventAsync(
        eventType: "processing_completed",
        data: new
        {
            stageName = e.StageName,
            totalProcessed = e.Result.TotalProcessed,
            successCount = e.Result.SuccessCount,
            failureCount = e.Result.FailureCount,
            successRate = e.Result.SuccessRate,
            timestamp = DateTime.UtcNow
        }
    );
});

// Process an incoming webhook request (for receiving webhooks from external services)
var inboundHandler = new InboundWebhookHandler(
    secret: "your-webhook-secret-key",
    logger: loggerFactory.CreateLogger<InboundWebhookHandler>()
);

// Verify and process an incoming webhook
var isValid = inboundHandler.ProcessWebhookAsync(
    requestBody: webhookPayload,
    signatureHeader: "sha256=your-signature-here",
    expectedEventType: "data_ingested"
);

if (isValid)
{
    // Process the webhook payload
    Console.WriteLine("Webhook verified and processed successfully");
}

// Get current subscriptions
var subscriptions = webhookHandler.GetSubscriptions();
foreach (var sub in subscriptions)
{
    Console.WriteLine($"Subscription: {sub.Id}, URL: {sub.Url}, Active: {sub.IsActive}");
}

// Unsubscribe when done
webhookHandler.Unsubscribe();

await orchestrator.StopAsync();
```

## EventSubscriberBase

The `EventSubscriberBase` class serves as the foundation for all event subscribers in the pipeline's event-driven architecture. It provides a standardized subscription pattern with lifecycle management through `Subscribe()` and `Unsubscribe()` methods. This base class handles common concerns like logging, error handling, and subscription state tracking, allowing derived subscribers to focus on their specific event processing logic.

### Usage Example

```csharp
using DotNetRealtimePipeline.Events;
using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddPipelineServices();

var provider = services.BuildServiceProvider();
var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
var publisher = provider.GetRequiredService<PipelineEventPublisher>();

// Create subscribers for different event types
var dataIngestSubscriber = new DataIngestSubscriber(publisher, loggerFactory.CreateLogger<DataIngestSubscriber>());
var processingSubscriber = new ProcessingCompletionSubscriber(publisher, loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());
var backpressureSubscriber = new BackpressureAlertSubscriber(publisher, loggerFactory.CreateLogger<BackpressureAlertSubscriber>());
var metricsSubscriber = new MetricsAggregationSubscriber(publisher, loggerFactory.CreateLogger<MetricsAggregationSubscriber>());
var errorSubscriber = new ErrorAlertSubscriber(publisher, loggerFactory.CreateLogger<ErrorAlertSubscriber>());

// Subscribe to all relevant events
dataIngestSubscriber.Subscribe();
processingSubscriber.Subscribe();
backpressureSubscriber.Subscribe();
metricsSubscriber.Subscribe();
errorSubscriber.Subscribe();

// Simulate pipeline operations that generate events
var dataPoint = new DataPoint(1, DateTime.UtcNow.Ticks, 42.5m, "Sensor-1");
await publisher.PublishDataIngestedAsync(dataPoint);

await publisher.PublishProcessingCompletedAsync("DataProcessingStage", new ProcessingResult(100, 95, 5));

var backpressureContext = new BackpressureContext("IngestionStage", 0.95m, BackpressureStrategy.Block);
await publisher.PublishBackpressureDetectedAsync(backpressureContext);

var metrics = new MetricAggregation(1000, 950, 50, 0.98m, DateTime.UtcNow);
await publisher.PublishMetricsCollectedAsync(metrics);

// Access metrics from subscribers
Console.WriteLine($"Success Rate: {processingSubscriber.GetSuccessRatePercent():F2}%");
Console.WriteLine($"Average Processing Time: {metricsSubscriber.GetAverageProcessingTime():F2}ms");
Console.WriteLine($"Backpressure Events: {backpressureSubscriber.GetBackpressureEventCount()}");
Console.WriteLine($"Total Errors: {errorSubscriber.GetErrorCount()}");
Console.WriteLine($"Metrics Collected: {metricsSubscriber.GetMetricsCount()}");

// Unsubscribe when done
processingSubscriber.Unsubscribe();
backpressureSubscriber.Unsubscribe();
```

## Troubleshooting

### High Memory Usage

**Issue**: Pipeline consuming excessive memory

**Solutions**:
1. Reduce `MaxBufferSize` in configuration
2. Lower `MetricsHistorySize` to keep fewer historical metrics
3. Increase `BufferFlushIntervalMs` to reduce batch frequency
4. Enable periodic data cleanup

```csharp
services.AddPipelineServices(config =>
{
    config.MaxBufferSize = 5000;        // Reduce from 10000
    config.MetricsHistorySize = 500;    // Reduce from 1000
    config.BufferFlushIntervalMs = 2000; // Increase from 1000
});
```

### Data Points Being Dropped

**Issue**: Backpressure causing data loss

**Solutions**:
1. Change backpressure strategy from Drop to Block or Throttle
2. Increase `MaxBufferSize`
3. Reduce ingestion rate
4. Check downstream consumer performance

```csharp
services.AddPipelineServices(config =>
{
    config.BackpressureStrategy = BackpressureStrategy.Block;
    config.MaxBufferSize = 50000;
    config.BackpressureThreshold = 0.9m; // Trigger later
});
```

### Low Throughput

**Issue**: Pipeline not processing data fast enough

**Solutions**:
1. Increase `MaxConcurrentConsumers`
2. Reduce `ProcessingTimeoutMs`
3. Disable quality analysis if not needed
4. Use Release build instead of Debug

```csharp
services.AddPipelineServices(config =>
{
    config.MaxConcurrentConsumers = 16;  // Increase from 4
    config.ProcessingTimeoutMs = 10000;  // Reduce from 30000
    config.EnableQualityAnalysis = false;
});

// Run with Release configuration
dotnet run -c Release
```

### Window Statistics Empty

**Issue**: Windowing service returning empty statistics

**Solutions**:
1. Verify data timestamps are within current window bounds
2. Check `WindowSizeMs` and `WindowSlideMs` values
3. Ensure data points have correct timestamp format
4. Check window type matches your use case

```csharp
// Verify timestamps are in reasonable range
var now = DateTime.UtcNow.Ticks;
var dataPoint = new DataPoint(1, now, 100, "Sensor-1");

// Use appropriate window type
var config = new PipelineConfig
{
    WindowType = WindowType.SLIDING, // More suitable for continuous streams
    WindowSizeMs = 10000,
    WindowSlideMs = 5000
};
```

### Metrics Not Collecting

**Issue**: Health reports showing zero metrics

**Solutions**:
1. Verify `EnableMetrics = true` in configuration
2. Call `IngestDataPointAsync` before requesting metrics
3. Allow time for metrics collection
4. Check for exceptions in background tasks

```csharp
var config = new EventServiceConfiguration
{
    EnableEventPublishing = true,
    EventQueueCapacity = 10000
};

// Wait for data to be processed
await Task.Delay(2000);

var health = await orchestrator.GetHealthReportAsync();
Console.WriteLine($"Metrics: {health.ThroughputItemsPerSecond}");
```

## Performance

Benchmarks measured on an Intel Core i7-12700 (single-core baseline) with .NET 10, Release build, default configuration unless noted.

| Scenario | Throughput / Latency |
|---|---|
| Single-core ingestion (Block strategy) | **~10,000 events/sec** |
| Batch ingestion, 16 concurrent consumers | **~85,000 events/sec** |
| End-to-end P50 latency (ingest → window → store) | **< 2 ms** |
| End-to-end P99 latency | **< 8 ms** |
| Tumbling window assignment (100 K points) | **< 5 ms** |
| Query + trend analysis (100 K points, 1 s interval) | **< 50 ms** |
| Health report generation | **< 10 ms** |
| Memory footprint (default config, idle) | **~120 MB** |
| Memory footprint (high-throughput, 100 K buffer) | **~350 MB** |

### Running Performance Benchmarks

The project includes comprehensive performance benchmarks using [BenchmarkDotNet](https://benchmarkdotnet.org/). These benchmarks measure critical pipeline operations including ingestion throughput, processing performance, windowing operations, monitoring overhead, and memory allocations.


```bash
# Navigate to benchmarks directory
cd dotnet-realtime-pipeline.Benchmarks

# Run all benchmarks
./bin/Release/net10.0/dotnet-realtime-pipeline.Benchmarks

# Run specific benchmark category
./bin/Release/net10.0/dotnet-realtime-pipeline.Benchmarks --filter "*Throughput*"

# Export results to markdown
./bin/Release/net10.0/dotnet-realtime-pipeline.Benchmarks --exporters markdown
```

See the [benchmarks README](dotnet-realtime-pipeline.Benchmarks/README.md) for detailed instructions and benchmark descriptions.

> See [`docs/PERFORMANCE.md`](docs/PERFORMANCE.md) for full profiling methodology and hardware profiles.

### Tuning for Maximum Throughput

```csharp
services.AddPipelineServices(config =>
{
    config.MaxBufferSize = 100_000;
    config.MaxConcurrentConsumers = Environment.ProcessorCount;
    config.BufferFlushIntervalMs = 250;
    config.EnableQualityAnalysis = false;  // saves ~15% CPU
    config.BackpressureStrategy = BackpressureStrategy.Throttle;
});
```

> See [`docs/PERFORMANCE.md`](docs/PERFORMANCE.md) for full profiling methodology and hardware profiles.

## Testing

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity detailed

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov
```

The test suite covers:

- **Unit tests** (`tests/Unit/`) — service logic, repository operations, and domain model invariants
- **Integration tests** (`tests/Integration/`) — full pipeline lifecycle and end-to-end data flow

See [`docs/TESTING.md`](docs/TESTING.md) for detailed guidance on writing and running tests.

## Related Projects

- [dotnet-event-bus](https://github.com/sarmkadan/dotnet-event-bus) - In-process and distributed event bus for .NET - pub/sub, request/reply, dead letter, polymorphic handlers
- [redis-cache-patterns](https://github.com/sarmkadan/redis-cache-patterns) - Production-ready Redis caching patterns for .NET - cache-aside, write-through, distributed lock

### Integration Examples

**Publish pipeline window events to dotnet-event-bus**

```csharp
// Wire the pipeline's event publisher into the event bus
services.AddPipelineServices();
services.AddEventBus();

// In your application, subscribe to window-closed events and forward them
var eventBus = provider.GetRequiredService<IEventBus>();
var pipelineEvents = provider.GetRequiredService<PipelineEventPublisher>();

pipelineEvents.OnWindowClosed += async windowEvent =>
    await eventBus.PublishAsync(new WindowClosedMessage(windowEvent));
```

**Cache aggregated metrics with redis-cache-patterns**

```csharp
services.AddPipelineServices();
services.AddRedisCachePatterns(opt => opt.ConnectionString = "localhost:6379");

// Cache expensive aggregate queries using cache-aside pattern
var cache = provider.GetRequiredService<ICacheService>();
var stats = await cache.GetOrSetAsync(
    key: $"stats:{startMs}:{endMs}",
    factory: () => queryService.GetAggregateStatisticsAsync(startMs, endMs),
    ttl: TimeSpan.FromSeconds(30));
```

## Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Setup

```bash
# Clone and setup
git clone https://github.com/Sarmkadan/dotnet-realtime-pipeline.git
cd dotnet-realtime-pipeline

# Build and test
dotnet build
dotnet test

# Code formatting
dotnet format

# Run linters
dotnet analyzers
```

### Code Guidelines

- Follow C# naming conventions (PascalCase for public, camelCase for private)
- Add XML documentation for public APIs
- Write unit tests for new features
- Keep files under 200 lines
- Use async/await for I/O operations
- Add comments explaining complex logic

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limited to the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

## Security

If you discover a security vulnerability, please report it responsibly. See [SECURITY.md](SECURITY.md) for the full disclosure policy. Do not open a public GitHub issue for security vulnerabilities.

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
