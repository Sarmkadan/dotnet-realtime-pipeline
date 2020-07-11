# Examples - dotnet-realtime-pipeline

Complete working examples demonstrating various features and use cases of the pipeline.

## Quick Start

### Run an Example

```bash
# Build all examples
dotnet build examples/

# Run a specific example
dotnet run --project examples/ -- 01-simple-ingestion
```

## Examples Overview

### 1. Simple Data Ingestion (`01-simple-ingestion.cs`)

**Purpose**: Learn the basics of data ingestion and status reporting

**Features Demonstrated**:
- Setting up the pipeline
- Ingesting individual data points
- Retrieving pipeline status
- Getting health reports
- Graceful shutdown

**Key Classes Used**:
- `PipelineOrchestrator` - Main pipeline control
- `DataPoint` - Individual data records
- `HealthReport` - Pipeline health metrics

**When to Use**: Starting with the pipeline, basic integration

**Expected Output**:
```
Starting pipeline...
Ingesting 1,000 data points...
Ingestion completed in 250ms
Pipeline Status:
  Total Processed: 1000
  Total Failed: 0
  Is Running: True
  Buffer Utilization: 5.23%
```

---

### 2. Multi-Source Processing (`02-multi-source-processing.cs`)

**Purpose**: Handle data from multiple sources concurrently

**Features Demonstrated**:
- Concurrent data ingestion from multiple sensors
- Managing different data types and ranges
- Multi-source analysis
- Realistic sensor data generation
- Performance reporting

**Key Classes Used**:
- `PipelineOrchestrator` - Multi-source orchestration
- `MetricsService` - Performance tracking
- Custom configuration for multiple streams

**When to Use**: IoT applications, multi-sensor systems, distributed data sources

**Real-World Scenario**:
Weather station collecting temperature, humidity, and pressure from multiple locations

**Expected Output**:
```
=== Multi-Source Processing Example ===

Ingesting data from multiple sources...
✓ Data ingestion from 4 sources completed

=== Processing Results ===

Total Data Points Processed: 1,000
Success Rate: 99.80%

Pipeline Health Status: HEALTHY
Throughput: 2,500.00 items/sec
Average Latency: 5.23 ms
```

---

### 3. Windowing & Aggregation (`03-windowing-aggregation.cs`)

**Purpose**: Demonstrate time-based windowing and statistical aggregation

**Features Demonstrated**:
- Creating time windows (tumbling, sliding, session)
- Assigning data to windows
- Calculating window statistics
- Handling time-series data
- Quality analysis

**Key Classes Used**:
- `WindowingService` - Window management and aggregation
- `WindowEvent` - Window data container
- `WindowStatistics` - Statistical results

**Window Types**:
- **Tumbling**: 10-second fixed windows
- **Sliding**: 10-second window sliding every 5 seconds
- **Session**: Activity-based windows
- **Global**: All data in one window

**When to Use**: Time-series analysis, aggregations, sliding window calculations

**Real-World Scenario**:
Analyzing server metrics (CPU, memory) in 5-minute windows to detect anomalies

**Expected Output**:
```
Window [14:30:00-14:30:10]
  Count: 500
  Sum: 25,000.00
  Average: 50.00
  Min: 0.12
  Max: 99.87
  StdDev: 28.94
  Percentiles: P50=45.23, P95=92.15, P99=98.76
```

---

### 4. Backpressure Handling (`04-backpressure-handling.cs`)

**Purpose**: Learn to handle buffer overflow and flow control

**Features Demonstrated**:
- Three backpressure strategies: Block, Throttle, Drop
- Buffer monitoring and management
- Handling high-load scenarios
- Strategy comparison
- Performance under load

**Key Classes Used**:
- `BackpressureService` - Flow control management
- `BackpressureContext` - Buffer state
- `BackpressureStrategy` - Strategy enumeration

**Backpressure Strategies**:

1. **Block** (Default)
   - Pauses ingestion when buffer full
   - No data loss
   - May introduce latency

2. **Throttle**
   - Gradually reduces ingestion rate
   - Balances throughput and loss
   - Smooth degradation

3. **Drop**
   - Discards oldest items when full
   - Always accepts new data
   - Best for real-time feeds

**When to Use**: High-load scenarios, preventing data loss, optimizing throughput

**Real-World Scenario**:
Stock exchange processing trade data at peak market hours

**Expected Output**:
```
Test 1: Block Strategy
  Items to ingest: 20,000
  Accepted: 10,000
  Rejected: 10,000
  Success Rate: 50.00%
  Strategy: Block - pauses until buffer drains
```

---

### 5. Querying Data (`05-querying-data.cs`)

**Purpose**: Search and analyze stored data

**Features Demonstrated**:
- Time-range searches
- Source filtering
- Quality-based filtering
- Aggregate statistics
- Trend analysis
- Outlier detection

**Key Classes Used**:
- `QueryService` - Data search and analysis
- Various filter parameters
- Result aggregation

**Query Types**:

1. **Time Range Search**: Find points within time bounds
2. **Source Filtering**: Get data from specific source
3. **Quality Filtering**: Find high-quality data points
4. **Aggregations**: Calculate statistics over time range
5. **Trend Analysis**: Detect performance trends
6. **Outlier Detection**: Find anomalous data points

**When to Use**: Data analysis, reporting, debugging, anomaly detection

**Real-World Scenario**:
Retrieving user activity data from last 24 hours for analytics dashboard

**Expected Output**:
```
Found 50000 points in the last hour
Found 15000 points from Temperature-1
Found 45000 high-quality points (>0.8)

Aggregate Statistics:
  Total Points: 50000
  Average: 50.50
  Min: 0.12
  Max: 99.99
  StdDev: 28.87
```

---

### 6. Health Monitoring (`06-health-monitoring.cs`)

**Purpose**: Continuous health tracking and performance monitoring

**Features Demonstrated**:
- Real-time health checks
- Throughput and latency monitoring
- Performance trending
- Multi-phase load testing
- Health report analysis

**Key Classes Used**:
- `MetricsService` - Metrics collection and analysis
- `HealthReport` - Comprehensive health snapshot
- `PerformanceTrend` - Trend analysis

**Monitoring Phases**:

1. **Ramp-Up**: Gradually increase load
2. **Steady-State**: Maintain consistent load
3. **Analysis**: Detect trends and issues

**Health Status Levels**:
- 🟢 **HEALTHY**: Normal operation
- 🟡 **DEGRADED**: Performance below expected
- 🔴 **UNHEALTHY**: System not operational

**When to Use**: Production monitoring, performance baseline establishment, SLA tracking

**Real-World Scenario**:
24/7 monitoring of production data pipeline with alerting on degradation

**Expected Output**:
```
[14:30:00] Monitoring Report #1
  Status: HEALTHY
  Processed: 1,500
  Throughput: 2,500.00 items/sec
  Avg Latency: 4.23 ms
  Buffer: 5.20%
```

---

### 7. Custom Configuration (`07-custom-configuration.cs`)

**Purpose**: Configure pipeline for specific workload scenarios

**Features Demonstrated**:
- High-performance configuration
- Low-latency configuration
- Resource-constrained configuration
- Configuration builder pattern
- Scenario-based tuning

**Key Classes Used**:
- `PipelineConfigurationBuilder` - Fluent configuration API
- `PipelineConfig` - Configuration container

**Configuration Scenarios**:

1. **High Performance** (100k+ items/sec)
   - Large buffer (500,000)
   - High parallelism (16 threads)
   - Fast flush (500ms)

2. **Low Latency** (< 10ms processing)
   - Small buffer (10,000)
   - Standard parallelism (4 threads)
   - Frequent flush (100ms)

3. **Resource Constrained** (1GB RAM limit)
   - Minimal buffer (5,000)
   - Low parallelism (2 threads)
   - Limited history (100 metrics)

**When to Use**: Tuning for specific use cases, resource optimization, testing

**Real-World Scenarios**:
- **High Performance**: Financial data processing
- **Low Latency**: Real-time trading algorithms
- **Resource Constrained**: Edge devices, IoT gateways

**Expected Output**:
```
Scenario 1: High Performance
  Processed: 1000
  Throughput: 5000.00 items/sec
  Status: HEALTHY
```

---

## Running Examples

### Individual Example

```bash
cd dotnet-realtime-pipeline
dotnet run --project examples/ -- 01-simple-ingestion
```

### All Examples in Sequence

```bash
for i in {01..07}; do
    echo "Running Example $i..."
    dotnet run --project examples/ -- $i
    echo "✓ Example $i completed\n"
done
```

### With Performance Monitoring

```bash
# Start Prometheus/Grafana
docker-compose up -d prometheus grafana

# Run example
dotnet run --project examples/ -- 02-multi-source-processing

# View metrics
open http://localhost:3000  # Grafana
```

## Creating Your Own Example

1. **Create new file**: `examples/08-your-feature.cs`
2. **Implement ExampleAsync static method**:

```csharp
public class YourFeatureExample
{
    public static async Task RunAsync()
    {
        var services = new ServiceCollection();
        services.AddPipelineServices();
        var serviceProvider = services.BuildServiceProvider();
        
        var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();
        
        await orchestrator.StartAsync();
        // Your example code here
        await orchestrator.StopAsync();
    }
}
```

3. **Update Program.cs to include your example**
4. **Test**: `dotnet run -- 08-your-feature`

## Common Patterns

### Pattern 1: Basic Data Ingestion

```csharp
await orchestrator.StartAsync();

for (int i = 0; i < 1000; i++)
{
    var point = new DataPoint(i, DateTime.UtcNow.Ticks, value, source);
    await orchestrator.IngestDataPointAsync(point);
}

await orchestrator.StopAsync();
```

### Pattern 2: Batch Processing

```csharp
var points = new List<DataPoint>
{
    new(1, DateTime.UtcNow.Ticks, 100, "Sensor-1"),
    new(2, DateTime.UtcNow.Ticks, 105, "Sensor-1"),
};

int ingested = await orchestrator.IngestBatchAsync(points);
```

### Pattern 3: Concurrent Multi-Source

```csharp
var tasks = sources.Select(async source =>
{
    for (int i = 0; i < count; i++)
    {
        await orchestrator.IngestDataPointAsync(CreatePoint(source, i));
    }
});

await Task.WhenAll(tasks);
```

### Pattern 4: Health Monitoring Loop

```csharp
for (int i = 0; i < 10; i++)
{
    await Task.Delay(5000);
    var health = await metricsService.GenerateHealthReportAsync();
    Console.WriteLine($"Status: {health.Status}");
}
```

## Benchmarking Examples

Run with Release configuration for accurate performance metrics:

```bash
dotnet build -c Release
dotnet run -c Release --project examples/ -- 01-simple-ingestion
```

Expected performance (Release build):
- Ingestion: 50,000-100,000 items/sec
- Latency: 0.1-1ms per item
- Memory: < 500MB for 100,000 items

## Troubleshooting

**Example hangs**: Check if Start/Stop is properly balanced
**Low throughput**: Verify Release build, check system resources
**Memory issues**: Reduce batch sizes, enable periodic cleanup
**Data not appearing**: Verify `await Task.Delay()` after ingestion before querying

## Next Steps

- Read [Getting Started Guide](../docs/getting-started.md)
- Study [Architecture Document](../docs/architecture.md)
- Review [API Reference](../docs/api-reference.md)
- Check [FAQ](../docs/faq.md) for common questions

---

**Happy Learning!** 🚀
