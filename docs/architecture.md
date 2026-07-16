# Architecture Overview

This document provides a detailed technical overview of the dotnet-realtime-pipeline architecture.

## What This Project Actually Is

dotnet-realtime-pipeline is an **in-process, in-memory data pipeline library** with a
console demo entry point (`Program.cs`). There is no hosted HTTP server: the classes under
`src/API/` (`ApiEndpointHandler`, `DataIngestionHandler`, `StatusHandler`, `QueryHandler`)
are plain in-process handler classes that wrap `PipelineOrchestrator` calls in an
`ApiResponse<T>` envelope. To expose them over HTTP you would host them yourself (e.g. map
them to ASP.NET Core endpoints). The same applies to the "CLI" and "Webhook" components -
they are library classes, not running processes.

Everything is wired through `Microsoft.Extensions.DependencyInjection` via
`ServiceCollectionExtensions.AddPipelineServices(...)` (see
`src/Configuration/ServiceCollectionExtensions.cs`), which registers all services,
repositories and the shared `PipelineConfig` as **singletons**. Singleton lifetimes are
correct here because the services hold long-lived mutable state (buffers, metric counters,
window state) that must be shared across the whole pipeline.

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
    └─> PipelineOrchestrator.IngestDataPointAsync(dataPoint)
            │
            ├─> BackpressureService.TryAddToBuffer("Ingestion", 1)
            │   ├─ Buffer full → ApplyBackpressureAsync (Block, delay) and return false
            │   └─ Otherwise → enqueue into the in-memory ingestion queue, return true
            │
            └─ (returns immediately; processing is asynchronous)

Background loop (ProcessingLoopAsync, started by StartAsync):
    │
    ├─ Dequeue up to 100 points per iteration (Task.Delay(100) when idle)
    │
    ├─> DataProcessingService.ProcessBatchAsync(batch)
    │   ├─ Validate each point (DataPoint.IsValid)
    │   ├─ Quality check against MinDataQualityThreshold
    │   └─ Persist accepted points via IDataPointRepository
    │
    ├─> MetricsService: record processing time + throughput per result
    │
    ├─> BackpressureService.TryAddToBuffer("Windowing", n)
    │   └─> WindowingService.ProcessDataPoints(points)
    │       └─ Assign to tumbling/sliding windows, emit closed windows
    │       (buffer is drained again after the hand-off)
    │
    └─> BackpressureService.RemoveFromBuffer("Ingestion", batch.Count)

[Latency for a single point is dominated by the 100ms polling interval]
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
   - Uses a single private lock object (`lock (_lockObject)`) around every operation
   - Reads and writes are both exclusive - simple and correct, but readers do contend
     with each other under load

2. **BackpressureService**
   - Uses a plain `Dictionary<string, BackpressureContext>` guarded by one lock object
   - All buffer checks, additions and removals take that lock (not lock-free)

3. **PipelineOrchestrator**
   - Ingestion enqueues into a `Queue<DataPoint>` guarded by `lock`; a single
     background loop (`ProcessingLoopAsync`, started fire-and-forget from `StartAsync`)
     drains it in batches of up to 100
   - Success/failure counters are updated with `Interlocked.Increment`

4. **MetricsService**
   - Thread-safe metric collection using `lock`
   - Synchronous aggregation during report generation

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

### Implementation Status

The window type is selected via the string `PipelineConfig.WindowType`
(default `"TUMBLING"`). In `WindowingService`:

- **Tumbling** and **Sliding** window *assignment* are implemented: `"SLIDING"` assigns a
  point to all overlapping windows, anything else falls back to tumbling assignment.
- **Session** windows have an aggregation branch (`"SESSION"`) but no session-gap
  assignment logic - points are still bucketed by fixed window start.
- **Global** windows exist only as a value of the `WindowType` enum; there is no
  corresponding implementation.

The `WindowType` enum (`Tumbling/Sliding/Session/Global`) is declared in
`src/Domain/Enums/PipelineEnums.cs` but the windowing code keys off the config string,
not the enum.

## Quality Scoring

The pipeline does **not compute** a quality score. `DataPoint.Quality` is an `int`
property (0-100, default `100`) that the **producer supplies** when creating the point.
What the pipeline does with it:

- `DataPoint.IsValid()` rejects points whose `Quality` is outside 0-100
- `DataProcessingService` fails a point during its "QualityCheck" stage when
  `Quality < PipelineConfig.MinDataQualityThreshold`
- `DataProcessingService.AnalyzeDataQuality(...)` reports aggregate statistics
  (average/min/max quality, pass rate against the threshold)
- The `DataQuality` enum (`Poor=0, Fair=25, Good=50, Excellent=75, Perfect=100`) provides
  named bands but is not used to derive scores

If you need computed quality (completeness/freshness/etc.), implement it in your producer
or in a custom processing step before ingestion.

## Performance Characteristics

Do not rely on hardcoded numbers - measure on your hardware. The repository ships a
BenchmarkDotNet project (`dotnet-realtime-pipeline.Benchmarks/`, `PipelineBenchmarks`)
covering single-point ingestion, batch processing, windowing throughput, health report
generation, backpressure buffer operations and end-to-end throughput:

```bash
dotnet run -c Release --project dotnet-realtime-pipeline.Benchmarks
```

Structural characteristics worth knowing:

- The processing loop polls the ingestion queue with a `Task.Delay(100)` idle wait and a
  batch size of 100, so end-to-end latency for a single point is dominated by that
  polling interval (up to ~100 ms), not by the per-point work.
- All storage is in-memory behind coarse locks; queries are LINQ over the full
  collection, i.e. O(n) per query.

## Extension Points

### Plugin System

`src/Plugins/ExtensionSystem.cs` defines the plugin surface:

- `IPipelinePlugin` - base plugin contract (with `PipelinePluginBase` as a convenience
  base class)
- `IDataProcessingPlugin`, `IDataTransformPlugin`, `IOutputPlugin` - specialized hooks
- `PluginManager` / `PluginRegistry` - registration and lookup

### Custom Repository Implementation

```csharp
public class PostgresDataPointRepository : IDataPointRepository
{
    // Implement interface with database persistence
}

// Register instead of the default in-memory implementation.
// Use a singleton: the pipeline services that consume the repository are
// singletons themselves, so a scoped registration would be a captive dependency.
services.AddSingleton<IDataPointRepository, PostgresDataPointRepository>();
```

Note: `AddPipelineServices` registers `InMemoryDataPointRepository` with plain
`AddSingleton` (not `TryAddSingleton`), and in Microsoft.Extensions.DependencyInjection
the *last* registration wins for `GetRequiredService<T>`. So call
`services.AddPipelineServices(...)` first, then add your override afterwards.

### Event Subscription

`PipelineEventPublisher.Subscribe<T>(string eventName, Func<T, Task> handler)` subscribes
a typed async handler to a named event (`T : PipelineEventArgs`). Note that
`PipelineEventPublisher` is **not** registered by `AddPipelineServices`; construct it
yourself or add it to your service collection.

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

## Known Limitations

- **In-memory only.** Both repositories (`InMemoryDataPointRepository`,
  `InMemoryMetricsRepository`) hold data in process memory; nothing survives a restart.
  The PostgreSQL service in `docker-compose.yml` is not wired to any repository
  implementation.
- **No hosted endpoints.** Despite the Dockerfile exposing port 8080, the entry point is
  a console demo; the API/webhook/CLI classes must be hosted by consumer code.
- **Best-effort shutdown.** `StopAsync` flips a flag and waits a fixed 500 ms; it does
  not join the processing loop, so in-flight work past that window is abandoned and any
  points still in the ingestion queue are lost.
- **Fire-and-forget processing loop.** `StartAsync` starts `ProcessingLoopAsync` with
  `_ = ...`; an unhandled exception escaping the loop's own try/catch would be
  unobserved.
- **Polling-based loop.** Idle polling with `Task.Delay(100)` bounds single-point
  latency at ~100 ms; there is no event-based wakeup on enqueue.
- **Window types partially implemented.** Session-gap assignment and Global windows are
  not implemented (see "Implementation Status" above).
- **Quality is producer-supplied.** No quality computation happens inside the pipeline
  (see "Quality Scoring" above).
