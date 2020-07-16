# Examples Guide - dotnet-realtime-pipeline

## Overview

This document provides a comprehensive guide to all examples in the dotnet-realtime-pipeline project. Each example demonstrates specific features and use cases.

## Running Examples

### Quick Start

```bash
# Build in Release mode for accurate performance
dotnet build -c Release

# Run all examples
make examples

# Run specific example
make example-01
make example-02
```

## Example Catalog

### Example 1: Simple Data Ingestion ⭐ Start Here
**File**: `examples/01-simple-ingestion.cs`

**Learn**: Basic pipeline setup, data ingestion, status reporting

**Best for**: First-time users, understanding core concepts

**Key Code**:
```csharp
await orchestrator.StartAsync();
await orchestrator.IngestDataPointAsync(dataPoint);
var status = orchestrator.GetStatus();
await orchestrator.StopAsync();
```

**Run**:
```bash
dotnet run -c Release --project examples/ -- 01-simple-ingestion
```

**Expected Output**:
```
Processed: 1000
Throughput: 2500.00 items/sec
Status: HEALTHY
```

---

### Example 2: Multi-Source Processing
**File**: `examples/02-multi-source-processing.cs`

**Learn**: Concurrent ingestion from multiple sources, realistic sensor data

**Best for**: IoT applications, multi-sensor systems

**Key Concepts**:
- Concurrent task processing
- Multiple data sources
- Realistic Gaussian distribution data
- Performance metrics

**Key Code**:
```csharp
var tasks = sensors.Select(async sensor =>
{
    for (int i = 0; i < 500; i++)
    {
        await orchestrator.IngestDataPointAsync(point);
    }
});
await Task.WhenAll(tasks);
```

**Run**:
```bash
dotnet run -c Release --project examples/ -- 02-multi-source-processing
```

**Expected Output**:
```
Processing Results
Total Data Points: 1,000
Success Rate: 99.80%
Throughput: 2,500.00 items/sec
```

---

### Example 3: Windowing & Aggregation
**File**: `examples/03-windowing-aggregation.cs`

**Learn**: Time-window creation, statistical aggregation, window types

**Best for**: Time-series analysis, analytics dashboards

**Key Concepts**:
- Tumbling windows (fixed, non-overlapping)
- Sliding windows (overlapping, configured slide)
- Window statistics (sum, average, min, max, percentiles)
- Statistical calculations

**Window Types**:
```
Tumbling:  |---W1---|---W2---|---W3---|
Sliding:   |--W1--|--W2--|--W3--|--W4--|
           (overlapping intervals)
```

**Key Code**:
```csharp
var windows = windowingService.AssignDataPointsToWindows(points);
foreach (var window in windows)
{
    var stats = windowingService.CalculateWindowStatistics(window);
    Console.WriteLine($"Average: {stats.Average}, Max: {stats.Maximum}");
}
```

**Run**:
```bash
dotnet run -c Release --project examples/ -- 03-windowing-aggregation
```

**Expected Output**:
```
Window [14:30:00-14:30:10]
  Count: 500
  Average: 50.00
  Min: 0.12
  Max: 99.87
  P95: 92.15
```

---

### Example 4: Backpressure Handling
**File**: `examples/04-backpressure-handling.cs`

**Learn**: Flow control strategies, buffer management, handling high load

**Best for**: High-frequency systems, preventing data loss

**Backpressure Strategies**:
```
Block:    [BUFFER]━━━ (waits if full)
Throttle: [BUFFER]🔽🔽 (reduces ingestion rate)
Drop:     [BUFFER]🗑️  (discards oldest)
```

**Key Code**:
```csharp
bool accepted = backpressureService.TryAddToBuffer("Stage", 100);
if (!accepted)
{
    var response = await backpressureService.ApplyBackpressureAsync(
        "Stage",
        BackpressureStrategy.Block,
        timeoutMs: 1000
    );
}
```

**Run**:
```bash
dotnet run -c Release --project examples/ -- 04-backpressure-handling
```

**Expected Output**:
```
Test 1: Block Strategy
  Items to ingest: 20,000
  Accepted: 10,000
  Rejected: 10,000
  Success Rate: 50.00%
```

---

### Example 5: Querying Data
**File**: `examples/05-querying-data.cs`

**Learn**: Data search, filtering, analysis, trend detection

**Best for**: Analytics, reporting, debugging

**Query Types**:
```csharp
// Time range search
var points = await queryService.SearchDataPointsAsync(
    startTime, endTime, source: "Sensor-1"
);

// Aggregate statistics
var stats = await queryService.GetAggregateStatisticsAsync(startMs, endMs);

// Trend analysis
var trends = await queryService.AnalyzeTrendsAsync(startMs, endMs, intervalMs);
```

**Run**:
```bash
dotnet run -c Release --project examples/ -- 05-querying-data
```

**Expected Output**:
```
Found 50000 points in last hour
Found 15000 from Temperature-1
Found 45000 high-quality points

Aggregate Statistics:
  Average: 50.50
  Min: 0.12
  Max: 99.99
```

---

### Example 6: Health Monitoring
**File**: `examples/06-health-monitoring.cs`

**Learn**: Real-time health checks, performance trending, SLA monitoring

**Best for**: Production monitoring, performance baselining

**Key Metrics**:
```csharp
// Health status
Console.WriteLine($"Status: {health.Status}");  // HEALTHY/DEGRADED/UNHEALTHY
Console.WriteLine($"Throughput: {health.ThroughputItemsPerSecond}");
Console.WriteLine($"Avg Latency: {health.AverageLatencyMs}");
Console.WriteLine($"Error Rate: {health.ErrorRate}");

// Trend analysis
var trend = await metricsService.AnalyzePerformanceTrendAsync();
Console.WriteLine($"Direction: {trend.Direction}");  // UP/DOWN/STABLE
```

**Run**:
```bash
dotnet run -c Release --project examples/ -- 06-health-monitoring
```

**Expected Output**:
```
[14:30:00] Health Report #1
  Status: HEALTHY
  Processed: 1,500
  Throughput: 2,500.00 items/sec
  Avg Latency: 4.23 ms
  Buffer: 5.20%
```

---

### Example 7: Custom Configuration
**File**: `examples/07-custom-configuration.cs`

**Learn**: Configuration profiles, pipeline builder pattern, scenario-based tuning

**Best for**: Production deployment, performance optimization

**Configuration Profiles**:
```csharp
// High Performance (100k+ items/sec)
config.MaxBufferSize = 500000;
config.MaxConcurrentConsumers = 16;

// Low Latency (< 10ms processing)
config.MaxBufferSize = 10000;
config.BufferFlushIntervalMs = 100;

// Resource Constrained (1GB RAM limit)
config.MaxBufferSize = 5000;
config.MaxConcurrentConsumers = 2;
```

**Run**:
```bash
dotnet run -c Release --project examples/ -- 07-custom-configuration
```

**Expected Output**:
```
Scenario 1: High Performance
  Processed: 1000
  Throughput: 5000.00 items/sec
  Status: HEALTHY
```

---

### Example 8: Advanced Performance Tuning
**File**: `examples/08-advanced-performance-tuning.cs`

**Learn**: Performance profiling, latency analysis, configuration comparison

**Best for**: Performance optimization, benchmarking

**Features**:
- Compare multiple configuration profiles
- Measure throughput and latency
- Generate latency histograms
- Detailed performance metrics

**Metrics**:
```
Latency Distribution:
  Min: 0.123ms
  P50: 1.234ms
  P95: 5.678ms
  P99: 12.345ms
  Max: 25.678ms
```

**Run**:
```bash
dotnet run -c Release --project examples/ -- 08-advanced-performance-tuning
```

---

### Example 9: External API Integration
**File**: `examples/09-external-api-integration.cs`

**Learn**: Connect to external APIs, multi-source ingestion, metric export

**Best for**: Real-world integration scenarios

**Features**:
- Simulate external data sources
- Multiple concurrent API connections
- Metrics export to external systems
- Webhook notifications
- Error handling and retries

**Key Code**:
```csharp
// Simulate external source
var connector = new CustomExternalSourceConnector(apiUrl, apiKey);
var data = await connector.FetchDataAsync(limit: 100);

// Ingest from external source
foreach (var point in data)
{
    await orchestrator.IngestDataPointAsync(point);
}

// Export metrics
await metricsExporter.ExportAsync(metrics, format: "prometheus");
```

**Run**:
```bash
dotnet run -c Release --project examples/ -- 09-external-api-integration
```

**Expected Output**:
```
Connected to Weather API
Connected to Stock Market API
Connected to IoT Sensor Gateway

✓ All external sources synchronized
Metrics exported to prometheus
Webhook sent successfully
```

---

## Creating Your Own Example

### Template

```csharp
// examples/10-your-feature.cs
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetRealtimePipeline.Examples;

public class YourFeatureExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Your Feature Example ===\n");

        var services = new ServiceCollection();
        services.AddPipelineServices();
        var provider = services.BuildServiceProvider();

        var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

        await orchestrator.StartAsync();
        try
        {
            // Your example code here
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }
}
```

### Steps

1. Create `examples/10-your-feature.cs`
2. Implement `YourFeatureExample.RunAsync()`
3. Update `Program.cs` to register example
4. Update this file with description
5. Test: `dotnet run -c Release --project examples/ -- 10-your-feature`

## Performance Expectations

### Intel Core i7 (4 cores), 16GB RAM

| Example | Throughput | Latency P95 | Duration |
|---------|-----------|-----------|----------|
| 01-Simple Ingestion | 50K/sec | 8ms | ~2s |
| 02-Multi Source | 100K/sec | 10ms | ~5s |
| 03-Windowing | 50K/sec | 12ms | ~3s |
| 04-Backpressure | 25K/sec | 25ms | ~10s |
| 08-Performance Tuning | 200K/sec | 5ms | ~30s |

## Troubleshooting Examples

### Example Hangs
- Check `await orchestrator.StartAsync()` is called
- Verify `await orchestrator.StopAsync()` in finally block
- Ensure no infinite loops

### Low Throughput
- Run in Release mode: `-c Release`
- Check system CPU/memory availability
- Verify no background processes consuming resources

### Memory Issues
- Reduce batch sizes
- Check example doesn't accumulate data indefinitely
- Monitor with `dotnet counters monitor`

## Next Steps

After running examples:
1. Read [Getting Started Guide](./getting-started.md)
2. Study [Architecture](./architecture.md)
3. Review [API Reference](./api-reference.md)
4. Check [Configuration](./api-reference.md#configuration-reference)
5. Review [Testing Guide](./TESTING.md) to add tests

## Resources

- [Source Code](../src/)
- [GitHub Repository](https://github.com/Sarmkadan/dotnet-realtime-pipeline)
- [Documentation](./README.md)

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
