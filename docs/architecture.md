# Architecture Overview

This document provides a detailed technical overview of the dotnet-realtime-pipeline architecture.

## System Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│ Presentation & Integration Layer                                │
│                                                                  │
│  REST API Handler      CLI Command Executor    Webhook Handler  │
│  └─ HTTP endpoints     └─ CLI commands        └─ Event webhooks │
│     /api/datapoints       query                  /webhooks       │
│     /api/status           health                                 │
│     /api/metrics          trends                                 │
└──────────────────────────────────────────────────────────────────┘
                                 │
                    ┌────────────┴────────────┐
                    │                         │
┌───────────────────────────────────────────────────────────────────┐
│ Application Orchestration Layer                                  │
│                                                                   │
│  PipelineOrchestrator                                            │
│  ├─ Manages service lifecycle                                    │
│  ├─ Coordinates data flow                                        │
│  ├─ Exposes high-level API                                       │
│  └─ Handles graceful shutdown                                    │
└───────────────────────────────────────────────────────────────────┘
                                 │
        ┌────────────┬───────────┼───────────┬────────────┐
        │            │           │           │            │
┌───────────────────────────────────────────────────────────────────┐
│ Service Layer                                                     │
│                                                                   │
│ ┌──────────────────┐  ┌──────────────────┐  ┌─────────────────┐ │
│ │ DataProcessing   │  │ Windowing        │  │ Metrics         │ │
│ │ Service          │  │ Service          │  │ Service         │ │
│ ├──────────────────┤  ├──────────────────┤  ├─────────────────┤ │
│ │ • Validation     │  │ • Window mgmt    │  │ • Health track  │ │
│ │ • Quality score  │  │ • Aggregation    │  │ • Trend analysis│ │
│ │ • Transformation │  │ • Statistics     │  │ • Performance   │ │
│ │ • Retry logic    │  │ • Outlier detect │  │ • Error tracking│ │
│ └──────────────────┘  └──────────────────┘  └─────────────────┘ │
│                                                                   │
│ ┌──────────────────┐  ┌──────────────────┐  ┌─────────────────┐ │
│ │ Backpressure     │  │ Query            │  │ State           │ │
│ │ Service          │  │ Service          │  │ Manager         │ │
│ ├──────────────────┤  ├──────────────────┤  ├─────────────────┤ │
│ │ • Buffer mgmt    │  │ • Data retrieval │  │ • Persistence   │ │
│ │ • Flow control   │  │ • Filtering      │  │ • Recovery      │ │
│ │ • Strategies     │  │ • Trend analysis │  │ • Snapshots     │ │
│ │ • Monitoring     │  │ • Aggregation    │  │ • Serialization │ │
│ └──────────────────┘  └──────────────────┘  └─────────────────┘ │
└───────────────────────────────────────────────────────────────────┘
                                 │
┌───────────────────────────────────────────────────────────────────┐
│ Integration & Support Layer                                       │
│                                                                   │
│ ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────┐ │
│ │ Event        │  │ External     │  │ Middleware & Utilities  │ │
│ │ Publisher    │  │ Data Source  │  ├──────────────────────────┤ │
│ ├──────────────┤  ├──────────────┤  │ • Error Handling        │ │
│ │ • Pub/sub    │  │ • Integration│  │ • Logging               │ │
│ │ • Events     │  │ • Fetch data │  │ • Rate Limiting         │ │
│ │ • Subscribers│  │ • Transform  │  │ • Serialization         │ │
│ └──────────────┘  └──────────────┘  │ • Compression           │ │
│                                       │ • Validation            │ │
│                                       │ • DateTime Helpers      │ │
│                                       │ • Statistics Calcs      │ │
│ ┌──────────────────────────────────────────────────────────────┐ │
│ │ Metrics Exporter      HTTP Client Factory                    │ │
│ ├──────────────────────────────────────────────────────────────┤ │
│ │ • Format metrics      • Connection pooling                   │ │
│ │ • Export data         • Request retry                        │ │
│ │ • Integration         • Timeout handling                     │ │
│ └──────────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────────┘
                                 │
┌───────────────────────────────────────────────────────────────────┐
│ Data Access Layer                                                 │
│                                                                   │
│ ┌───────────────────────────┐  ┌────────────────────────────────┐│
│ │ IDataPointRepository      │  │ IMetricsRepository             ││
│ ├───────────────────────────┤  ├────────────────────────────────┤│
│ │ • Add / GetById           │  │ • Add / GetLatest              ││
│ │ • GetAll / GetRange       │  │ • GetByTimeRange               ││
│ │ • GetBySource / Query     │  │ • GetAverageMetrics            ││
│ │ • Delete / Clear          │  │ • Aggregation                  ││
│ │                           │  │                                ││
│ │ InMemoryDataPointRepo     │  │ InMemoryMetricsRepo            ││
│ │ └─ Thread-safe dict       │  │ └─ Rolling history             ││
│ │    with locks             │  │    with size limits            ││
│ └───────────────────────────┘  └────────────────────────────────┘│
└───────────────────────────────────────────────────────────────────┘
                                 │
┌───────────────────────────────────────────────────────────────────┐
│ Domain Layer                                                      │
│                                                                   │
│ Models:          Enums:              Exceptions:                 │
│ • DataPoint      • WindowType        • PipelineException         │
│ • WindowEvent    • BackpressureStr   • ProcessingException       │
│ • MetricAgg      • HealthStatus      • ValidationException       │
│ • PipelineConfig • ProcessingStatus  • ConfigException           │
│ • StreamEvent    • AggregationType   • TimeoutException          │
│                                                                   │
│ Constants: PipelineConstants (magic numbers, defaults, limits)   │
└───────────────────────────────────────────────────────────────────┘
```

## Data Flow

### Ingestion Path

```
User Code
    │
    ├─> IngestDataPointAsync(dataPoint)
    │
    └─> PipelineOrchestrator
            │
            ├─> BackpressureService.TryAddToBuffer()
            │   ├─ Check if buffer has capacity
            │   └─ Return false if full (respects strategy)
            │
            ├─> DataProcessingService.ProcessDataPointAsync()
            │   ├─ Validate data point
            │   ├─ Calculate quality score (0-100)
            │   ├─ Detect outliers
            │   └─ Apply retry logic
            │
            ├─> WindowingService.AssignToWindow()
            │   ├─ Determine window based on timestamp
            │   └─ Add to appropriate window
            │
            ├─> IDataPointRepository.AddAsync()
            │   └─ Persist to in-memory store
            │
            └─> PipelineEventPublisher.PublishAsync()
                └─ Notify subscribers of new data

[Timeline: ~5-10ms per point under normal conditions]
```

### Query Path

```
User Code (Query Request)
    │
    └─> QueryService.SearchDataPointsAsync()
            │
            ├─> Parse filter parameters
            │   ├─ Time range
            │   ├─ Source
            │   └─ Quality threshold
            │
            ├─> IDataPointRepository.Query()
            │   ├─ Filter in-memory collection
            │   └─ Return matching points
            │
            └─> Return Results to User

[Timeline: <1ms for typical queries (in-memory)]
```

### Aggregation Path

```
Background Task (Periodic)
    │
    └─> WindowingService.CalculateWindowStatistics()
            │
            ├─ Get all data points in window
            │
            ├─ Calculate aggregations:
            │  ├─ Count, Sum, Average
            │  ├─ Min, Max, StdDev
            │  ├─ Percentiles (P50, P95, P99)
            │  └─ Trending
            │
            ├─> MetricsService.RecordMetric()
            │   └─ Store aggregation
            │
            └─> IMetricsRepository.AddAsync()
                └─ Persist metrics

[Timeline: Window-dependent, typically 5-10s]
```

## Concurrency Model

### Thread Safety

1. **InMemoryDataPointRepository**
   - Uses `ReaderWriterLockSlim` for concurrent reads with exclusive writes
   - Supports multiple concurrent readers
   - Exclusive access for additions
   - Minimal lock contention under read-heavy loads

2. **BackpressureService**
   - Uses `ConcurrentDictionary` for stage contexts
   - Lock-free for buffer status checks
   - Atomic operations for counter updates

3. **MetricsService**
   - Thread-safe metric collection using `lock`
   - Synchronous aggregation during report generation
   - Minimal contention due to short critical sections

### Parallelism Strategy

- **MaxConcurrentConsumers**: Controls how many threads process data simultaneously
- **Batch Processing**: Groups items for efficient parallel processing
- **Asynchronous Operations**: Uses async/await throughout for I/O efficiency
- **No Busy Waiting**: Event-based signaling between components

```csharp
// Example: Batch processing with parallelism
var tasks = dataPoints
    .Chunk(batchSize)
    .Select(batch => processingService.ProcessBatchAsync(batch));

await Task.WhenAll(tasks);
```

## Backpressure Management

### Buffer Model

```
┌─────────────────────────────────────────────┐
│ Buffer (MaxBufferSize = 10,000)             │
├─────────────────────────────────────────────┤
│ Incoming Data        ↓                      │
│ ─────────────────────────────────────────── │
│ [Item1] [Item2] [Item3] ... [ItemN]         │
│ ─────────────────────────────────────────── │
│                        ↓ Processing         │
│ 80% Threshold = 8,000 items                 │
│                                             │
│ Current: 9,000 items (90% full)             │
│ Status: BACKPRESSURE TRIGGERED ⚠️           │
└─────────────────────────────────────────────┘
```

### Backpressure Strategies

1. **Block Strategy** (Default)
   ```
   Incoming Data ──→ Is Buffer Full? ──→ YES ──→ Wait/Retry
                        │
                        NO
                        ↓
                      Accept
   ```
   - Pauses ingestion until buffer drains
   - No data loss
   - May cause upstream delays
   - Best for: Important data that can't be lost

2. **Throttle Strategy**
   ```
   Incoming Rate ──→ 100% ──→ 75% ──→ 50% ──→ Resume
                    when buffer fills
   ```
   - Gradually reduces ingestion rate
   - Balanced loss vs. performance
   - Maintains throughput
   - Best for: Data where some loss is acceptable

3. **Drop Strategy**
   ```
   Incoming Data ──→ Is Buffer Full? ──→ YES ──→ Discard Oldest
                        │
                        NO
                        ↓
                      Accept
   ```
   - Always accepts new data
   - Removes oldest items to make room
   - Most recent data prioritized
   - Best for: Real-time feeds (weather, stock prices)

## Window Types

### Tumbling Windows

```
Time ─────────────────────────────────────────────>

[W1: 0-5s]  [W2: 5-10s]  [W3: 10-15s]  [W4: 15-20s]
  ├─ Data    ├─ Data      ├─ Data       ├─ Data
  └─ Agg.    └─ Agg.      └─ Agg.       └─ Agg.

Non-overlapping, fixed-size windows
Use case: Hourly/daily summaries
```

### Sliding Windows

```
Time ─────────────────────────────────────────────>

[W1: 0-5s]
      [W2: 1-6s]
            [W3: 2-7s]
                  [W4: 3-8s]
                        [W5: 4-9s]

Overlapping windows that slide over time
Use case: Moving averages, trend detection
```

### Session Windows

```
Time ─────────────────────────────────────────────>

[Session 1]  Gap   [Session 2]      Gap      [Session 3]
  ├─ Data           ├─ Data
  ├─ Data           ├─ Data
  └─ Data           └─ Data

Dynamically sized based on data activity
Use case: User session analysis, event grouping
```

### Global Windows

```
Time ─────────────────────────────────────────────>

[────────── Single Global Window ──────────────]
  All data goes into one window
  Aggregated at end or on demand

Use case: Final aggregation, end-of-batch reporting
```

## Quality Scoring

Quality score (0-100) calculated based on:

1. **Data Completeness** (40 points)
   - All required fields present: +40
   - Missing non-critical fields: +20
   - Missing critical fields: +0

2. **Data Validity** (30 points)
   - Value within expected range: +30
   - Value slightly outside range: +15
   - Value far outside range (outlier): +0

3. **Data Freshness** (20 points)
   - Recent data (< 1 hour old): +20
   - Medium age (1-24 hours): +10
   - Stale data (> 24 hours): +0

4. **Consistency** (10 points)
   - No duplicates: +10
   - Duplicate detected: +5
   - Multiple duplicates: +0

```csharp
// Example quality calculation
var quality = 40 + 30 + 20 + 10 = 100 (Perfect)
var quality = 40 + 15 + 10 + 5 = 70 (Good)
var quality = 20 + 0 + 0 + 0 = 20 (Poor)
```

## Performance Characteristics

### Memory

| Configuration | Memory Impact |
|---|---|
| 10,000 items in buffer | ~2-5 MB |
| Per data point | ~200-300 bytes |
| Per window | ~1-2 KB |
| Per metric aggregation | ~500-800 bytes |

### Throughput

| Scenario | Items/sec |
|---|---|
| Single point, no validation | 50,000+ |
| Single point with validation | 10,000-20,000 |
| Batch (100 items) with aggregation | 100,000+ |
| High quality scoring enabled | 5,000-10,000 |

### Latency

| Operation | Latency |
|---|---|
| Data point ingestion | 0.1-1ms |
| Buffer check | <0.1ms |
| Validation | 0.5-2ms |
| Quality score calculation | 1-3ms |
| Window aggregation | 5-20ms |
| Query (in-memory) | <1ms |

## Extension Points

### Adding Custom Services

```csharp
public class CustomProcessingService
{
    public async Task<CustomResult> ProcessAsync(DataPoint point)
    {
        // Custom logic
        return result;
    }
}

// Register in DI
services.AddScoped<CustomProcessingService>();

var customService = serviceProvider.GetRequiredService<CustomProcessingService>();
```

### Custom Repository Implementation

```csharp
public class PostgresDataPointRepository : IDataPointRepository
{
    // Implement interface with database persistence
}

// Register
services.AddScoped<IDataPointRepository, PostgresDataPointRepository>();
```

### Event Subscription

```csharp
var publisher = serviceProvider.GetRequiredService<PipelineEventPublisher>();

publisher.Subscribe<DataPointProcessedEvent>(async @event =>
{
    Console.WriteLine($"Processed point: {@event.DataPointId}");
});
```

## Monitoring and Observability

### Health Check Levels

| Level | Status | Meaning |
|---|---|---|
| HEALTHY | ✅ | System operating normally |
| DEGRADED | ⚠️ | Performance below expected |
| UNHEALTHY | ❌ | System not operational |

### Key Metrics

1. **Throughput**: Items processed per second
2. **Latency**: Time from ingestion to aggregation
3. **Error Rate**: Percentage of failed items
4. **Buffer Fill**: Current buffer capacity usage
5. **Quality Score**: Average data quality

### Trend Analysis

Detects performance trends over time:

- **Upward Trend**: ↗️ Performance improving
- **Downward Trend**: ↘️ Performance degrading
- **Stable Trend**: → Consistent performance
- **Oscillating Trend**: ↔️ Unstable behavior
