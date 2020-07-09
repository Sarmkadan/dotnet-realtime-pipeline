# Real-Time Data Processing Pipeline for .NET

A high-performance, production-grade real-time data processing pipeline built with .NET 10. Features backpressure management, time-window aggregation, comprehensive metrics collection, and graceful degradation under load.

## Features

- **Real-Time Processing**: Low-latency data point ingestion and processing through configurable pipeline stages
- **Backpressure Management**: Automatic buffer flow control with configurable strategies (Block, Throttle, Drop)
- **Time-Window Aggregation**: Tumbling, sliding, session, and global window types with statistical aggregations
- **Metrics & Monitoring**: Comprehensive throughput, latency, and error rate tracking with health reporting
- **Quality Control**: Data validation, quality scoring, and configurable quality thresholds
- **Flexible Architecture**: Modular service layer with dependency injection support
- **In-Memory State Management**: Efficient in-memory repositories with thread-safe operations
- **Statistical Analysis**: Advanced analytics including percentiles, moving averages, trend analysis, and outlier detection

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│ Application Layer                                           │
│ └─ PipelineOrchestrator (orchestrates all services)         │
├─────────────────────────────────────────────────────────────┤
│ Service Layer                                               │
│ ├─ DataProcessingService (validation, transformation)      │
│ ├─ WindowingService (aggregation, windowing)               │
│ ├─ MetricsService (monitoring, health)                     │
│ ├─ BackpressureService (flow control)                      │
│ └─ QueryService (data analysis and retrieval)              │
├─────────────────────────────────────────────────────────────┤
│ Data Access Layer                                           │
│ ├─ IDataPointRepository (data persistence)                 │
│ ├─ IMetricsRepository (metrics persistence)                │
│ └─ InMemory Implementations                                │
├─────────────────────────────────────────────────────────────┤
│ Domain Layer                                                │
│ ├─ Models (DataPoint, WindowEvent, ProcessingResult, etc)  │
│ ├─ Enums (WindowType, ProcessingStatus, HealthStatus, etc) │
│ ├─ Exceptions (PipelineException hierarchy)                │
│ └─ Constants (PipelineConstants)                           │
└─────────────────────────────────────────────────────────────┘
```

## Core Components

### Domain Models

- **DataPoint**: Individual data records with timestamps, values, and quality scores
- **WindowEvent**: Time-windowed aggregations of data points
- **ProcessingResult**: Results and metrics from pipeline processing stages
- **MetricAggregation**: Aggregated performance metrics (throughput, latency, error rates)
- **BackpressureContext**: Buffer state and backpressure management for pipeline stages
- **PipelineConfig**: Complete pipeline configuration with stages and parameters
- **StreamEvent**: Event wrapper with processing metadata and payload
- **PipelineStatus**: Current status snapshot of the entire pipeline

### Services

#### DataProcessingService
Handles individual data point and batch processing with validation and retry logic.

```csharp
var result = await processingService.ProcessDataPointAsync(dataPoint);
var results = await processingService.ProcessBatchAsync(dataPoints);
var analysis = processingService.AnalyzeDataQuality(dataPoints);
```

#### WindowingService
Manages time-based windowing and statistical aggregations.

```csharp
var window = windowingService.CreateWindow(startMs);
var windows = windowingService.AssignDataPointsToWindows(dataPoints);
var stats = windowingService.CalculateWindowStatistics(window);
```

#### MetricsService
Collects and analyzes pipeline performance metrics.

```csharp
var health = await metricsService.GenerateHealthReportAsync();
var trend = await metricsService.AnalyzePerformanceTrendAsync();
```

#### BackpressureService
Manages flow control and buffer overflow prevention.

```csharp
var context = backpressureService.CreateContext("StageName", maxCapacity);
bool accepted = backpressureService.TryAddToBuffer("StageName", itemCount);
var response = await backpressureService.ApplyBackpressureAsync("StageName", strategy, timeoutMs);
```

#### QueryService
Provides data analysis and retrieval operations.

```csharp
var points = await queryService.SearchDataPointsAsync(startTime, endTime, source, minQuality);
var stats = await queryService.GetAggregateStatisticsAsync(startMs, endMs);
var trend = await queryService.AnalyzeTrendsAsync(startMs, endMs, intervalMs);
```

### Repositories

- **IDataPointRepository**: CRUD operations and queries for data points
- **IMetricsRepository**: Storage and retrieval of metrics aggregations
- **InMemoryDataPointRepository**: Thread-safe in-memory implementation
- **InMemoryMetricsRepository**: Rolling history with size limits

## Configuration

### Using Configuration Builder

```csharp
var config = new PipelineConfigurationBuilder("MyPipeline", "1.0.0")
    .WithHighPerformanceDefaults()
    .WithBufferConfiguration(100000, 500, 16)
    .WithWindowingConfiguration(1000, 500, "SLIDING")
    .WithStage("Ingestion", "SOURCE")
    .WithStage("Processing", "TRANSFORM")
    .Build();
```

### Using Service Collection Extension

```csharp
services.AddPipelineServices(config => {
    config.MaxBufferSize = 50000;
    config.WindowSizeMs = 5000;
    config.MaxRetries = 3;
});
```

## Usage Example

```csharp
// Setup
var services = new ServiceCollection();
services.AddPipelineServices();
var serviceProvider = services.BuildServiceProvider();
var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();

// Start pipeline
await orchestrator.StartAsync();

// Ingest data
var dataPoint = new DataPoint(1, timestamp, value: 42.5, source: "Sensor-1");
bool accepted = await orchestrator.IngestDataPointAsync(dataPoint);

// Get status
var status = orchestrator.GetStatus();
Console.WriteLine($"Processed: {status.TotalDataPointsProcessed}");
Console.WriteLine($"Failed: {status.TotalDataPointsFailed}");

// Get health report
var health = await orchestrator.GetHealthReportAsync();
Console.WriteLine($"Health: {health.Status}");
Console.WriteLine($"Throughput: {health.ThroughputItemsPerSecond:F2} items/sec");

// Stop pipeline
await orchestrator.StopAsync();
```

## Performance Defaults

| Configuration | Value |
|---|---|
| Max Buffer Size | 10,000 items |
| Buffer Flush Interval | 1,000 ms |
| Window Size | 5,000 ms |
| Window Slide | 1,000 ms |
| Max Concurrent Consumers | 4 |
| Max Retries | 3 |
| Processing Timeout | 30,000 ms |
| Backpressure Trigger | 80% buffer fill |

## Prerequisites

- .NET 10 SDK
- C# 13+

## Building

```bash
cd dotnet-realtime-pipeline
dotnet build
```

## Running

```bash
dotnet run
```

## Testing

```bash
dotnet test
```

## Project Structure

```
src/
├── Configuration/          # DI setup, configuration builders
├── Constants/             # Global pipeline constants
├── Data/
│   └── Repositories/      # Data access interfaces and implementations
├── Domain/
│   ├── Enums/            # Pipeline enums (WindowType, AggregationType, etc)
│   ├── Exceptions/       # Custom exception hierarchy
│   └── Models/           # Domain entities (DataPoint, WindowEvent, etc)
├── Services/             # Service layer (processing, windowing, metrics, etc)
├── Utilities/            # Helper utilities (DateTime, Validation, Statistics)
└── Program.cs            # Application entry point
```

## Design Principles

- **Domain-Driven Design**: Clean domain layer with rich models
- **Dependency Injection**: Fully configurable via IServiceCollection
- **Thread-Safe**: All shared state protected with locks
- **Observable**: Comprehensive metrics and health reporting
- **Resilient**: Retry logic, graceful degradation, backpressure handling
- **Extensible**: Interface-based repositories and service implementations

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

## Author

[Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect
