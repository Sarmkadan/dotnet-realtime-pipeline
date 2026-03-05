# Getting Started with dotnet-realtime-pipeline

This guide walks you through setting up and running your first real-time data processing pipeline.

## Prerequisites

- **.NET 10 SDK** - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com)
- **Terminal/Command Prompt** - For running CLI commands
- **Text Editor or IDE** - Visual Studio, VS Code, JetBrains Rider, or similar
- **Basic C# knowledge** - Familiarity with async/await patterns helpful

## Installation Steps

### Step 1: Clone the Repository

```bash
git clone https://github.com/Sarmkadan/dotnet-realtime-pipeline.git
cd dotnet-realtime-pipeline
```

### Step 2: Restore Dependencies

```bash
dotnet restore
```

This downloads all NuGet packages specified in the .csproj file.

### Step 3: Build the Project

```bash
dotnet build
```

Verify the build completes without errors. You'll see output like:

```
Build succeeded in 5.23s
```

### Step 4: Run the Application

```bash
dotnet run
```

The application will start with the default configuration. You'll see console output showing pipeline initialization.

## Your First Pipeline

Create a new console application or add to Program.cs:

```csharp
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddPipelineServices();
var provider = services.BuildServiceProvider();

// Get the orchestrator
var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

// Start the pipeline
Console.WriteLine("Starting pipeline...");
await orchestrator.StartAsync();

// Create and ingest a data point
var dataPoint = new DataPoint(
    id: 1,
    timestamp: DateTime.UtcNow.Ticks,
    value: 42.5m,
    source: "FirstSensor"
);

Console.WriteLine("Ingesting data point...");
bool accepted = await orchestrator.IngestDataPointAsync(dataPoint);
Console.WriteLine($"Data accepted: {accepted}");

// Wait for processing
await Task.Delay(1000);

// Get pipeline status
var status = orchestrator.GetStatus();
Console.WriteLine($"\nPipeline Status:");
Console.WriteLine($"  Processed: {status.TotalDataPointsProcessed}");
Console.WriteLine($"  Failed: {status.TotalDataPointsFailed}");
Console.WriteLine($"  Is Running: {status.IsRunning}");

// Stop the pipeline
Console.WriteLine("\nStopping pipeline...");
await orchestrator.StopAsync();
Console.WriteLine("Pipeline stopped.");
```

Save this as `Program.cs` and run:

```bash
dotnet run
```

Expected output:
```
Starting pipeline...
Ingesting data point...
Data accepted: True

Pipeline Status:
  Processed: 1
  Failed: 0
  Is Running: False

Stopping pipeline...
Pipeline stopped.
```

## Understanding the Architecture

The pipeline consists of several layers:

### 1. **Ingestion Layer**
Where data enters the system. Data can come from:
- Direct API calls (`IngestDataPointAsync`)
- Batch imports (`IngestBatchAsync`)
- REST endpoints
- External data sources

### 2. **Processing Layer**
Validates and transforms data:
- Quality scoring (0-100)
- Data type validation
- Business rule enforcement
- Outlier detection

### 3. **Windowing Layer**
Groups data into time windows:
- **Tumbling**: Non-overlapping fixed windows
- **Sliding**: Overlapping windows that move over time
- **Session**: Windows based on periods of activity
- **Global**: Single window for all data

### 4. **Aggregation Layer**
Calculates statistics within windows:
- Count, Sum, Average
- Min, Max, Standard Deviation
- Percentiles (P50, P95, P99)

### 5. **Storage Layer**
Persists data and metrics:
- In-memory repositories (default)
- Thread-safe concurrent access
- Rolling history management

### 6. **Query Layer**
Retrieves and analyzes data:
- Time-range searches
- Source filtering
- Quality-based filtering
- Trend analysis

## Common Tasks

### Task 1: Ingest Multiple Data Points

```csharp
var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();
await orchestrator.StartAsync();

// Create a list of data points
var dataPoints = new List<DataPoint>();
for (int i = 0; i < 100; i++)
{
    dataPoints.Add(new DataPoint(
        id: i,
        timestamp: DateTime.UtcNow.Ticks,
        value: Random.Shared.NextDouble() * 100,
        source: "Sensor-1"
    ));
}

// Ingest them
int ingested = await orchestrator.IngestBatchAsync(dataPoints);
Console.WriteLine($"Ingested {ingested} points");

await orchestrator.StopAsync();
```

### Task 2: Check Health Status

```csharp
var metricsService = provider.GetRequiredService<MetricsService>();

var health = await metricsService.GenerateHealthReportAsync();

Console.WriteLine($"Status: {health.Status}");
Console.WriteLine($"Throughput: {health.ThroughputItemsPerSecond:F2} items/sec");
Console.WriteLine($"Avg Latency: {health.AverageLatencyMs:F2} ms");
Console.WriteLine($"Error Rate: {health.ErrorRate:P2}");

if (health.Status == HealthStatus.DEGRADED)
{
    Console.WriteLine("Pipeline is degraded! Check system resources.");
}
```

### Task 3: Query Data

```csharp
var queryService = provider.GetRequiredService<QueryService>();

// Search for data in a time range
var startTime = DateTime.UtcNow.AddHours(-1).Ticks;
var endTime = DateTime.UtcNow.Ticks;

var results = await queryService.SearchDataPointsAsync(
    startTime: startTime,
    endTime: endTime,
    source: "Sensor-1",
    minQualityScore: 0.8m
);

Console.WriteLine($"Found {results.Count()} data points");
foreach (var point in results)
{
    Console.WriteLine($"  ID: {point.Id}, Value: {point.Value}, Quality: {point.Quality:P2}");
}
```

### Task 4: Configure the Pipeline

```csharp
var services = new ServiceCollection();

// Custom configuration
services.AddPipelineServices(config =>
{
    config.MaxBufferSize = 50000;
    config.WindowSizeMs = 10000;
    config.WindowSlideMs = 5000;
    config.MaxConcurrentConsumers = 8;
    config.BackpressureThreshold = 0.75m;
    config.MaxRetries = 5;
});

var provider = services.BuildServiceProvider();
var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();
```

## Configuration Quick Reference

| Setting | Default | Purpose |
|---------|---------|---------|
| MaxBufferSize | 10,000 | Maximum items in memory before triggering backpressure |
| BufferFlushIntervalMs | 1,000 | How often to process buffered items |
| WindowSizeMs | 5,000 | Duration of each time window |
| WindowSlideMs | 1,000 | Movement of sliding window |
| MaxConcurrentConsumers | 4 | Parallel processing threads |
| MaxRetries | 3 | Retry attempts for failed items |
| ProcessingTimeoutMs | 30,000 | Max time for processing one item |
| BackpressureThreshold | 0.8 | Trigger backpressure at 80% buffer fill |

## Debugging Tips

### Enable Console Logging

```csharp
services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});
```

### Check Pipeline Status Frequently

```csharp
var status = orchestrator.GetStatus();
if (status.TotalDataPointsFailed > 0)
{
    Console.WriteLine($"Warning: {status.TotalDataPointsFailed} failures");
}
```

### Monitor Metrics in Real-Time

```csharp
for (int i = 0; i < 10; i++)
{
    var health = await metricsService.GenerateHealthReportAsync();
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Throughput: {health.ThroughputItemsPerSecond:F2} items/sec");
    await Task.Delay(1000);
}
```

### Verify Data Quality

```csharp
var processingService = provider.GetRequiredService<DataProcessingService>();
var analysis = processingService.AnalyzeDataQuality(dataPoints);

Console.WriteLine($"Quality Score: {analysis.AverageQualityScore:P2}");
if (analysis.AverageQualityScore < 0.8m)
{
    Console.WriteLine("Warning: Low data quality detected");
}
```

## Next Steps

- **Explore Examples**: Check the `examples/` directory for complete sample applications
- **Read Architecture Guide**: See `docs/architecture.md` for deep technical details
- **Review API Reference**: See `docs/api-reference.md` for complete method documentation
- **Check Deployment Guide**: See `docs/deployment.md` for production setup

## Troubleshooting

**Problem: Build fails with "Framework not found"**
- Solution: Install .NET 10 SDK from dotnet.microsoft.com

**Problem: "Object reference not set" exception**
- Solution: Ensure `await orchestrator.StartAsync()` is called before ingesting data

**Problem: No data appears in queries**
- Solution: Wait for data to be processed (`await Task.Delay(1000)` after ingesting)

**Problem: High memory usage**
- Solution: Reduce `MaxBufferSize` or enable more frequent flushing with smaller `BufferFlushIntervalMs`

## Getting Help

- Check the main README.md for comprehensive documentation
- Review example programs in `examples/` directory
- Search for similar issues on GitHub
- Report bugs at https://github.com/Sarmkadan/dotnet-realtime-pipeline/issues
