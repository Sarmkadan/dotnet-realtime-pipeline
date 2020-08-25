# PipelineConfig

`PipelineConfig` represents the complete declarative specification for a real-time data processing pipeline within the `dotnet-realtime-pipeline` framework. It defines the pipeline’s identity, buffering semantics, windowing strategy, concurrency limits, fault-tolerance parameters, quality constraints, and the ordered sequence of processing stages. An instance of this type is typically deserialized from a persistent store and validated before being used to construct and bootstrap a running pipeline.

## API

### `long ConfigId`
Unique numeric identifier for this configuration record. Used for persistence lookups, audit trails, and correlating runtime metrics with a specific configuration version.

### `string PipelineName`
Human-readable logical name assigned to the pipeline. Must be unique within a deployment scope. Used in logs, dashboards, and management APIs.

### `string Version`
Semantic version string (e.g., `"1.2.3"`) that distinguishes revisions of the same logical pipeline. The runtime uses this to detect configuration drift and to enforce compatibility checks during rolling updates.

### `long MaxBufferSize`
Maximum number of unprocessed items the pipeline’s internal buffers may hold before backpressure is applied to upstream producers. Expressed as an item count. A value of zero disables buffering entirely (every item is immediately offered to the next stage).

### `long BufferFlushIntervalMs`
Interval in milliseconds at which buffered items are flushed downstream when the buffer has not reached `MaxBufferSize`. Ensures low-latency delivery for sparse traffic. A value of zero causes immediate flush on every enqueue.

### `int MaxConcurrentConsumers`
Upper bound on the number of concurrent consumer tasks that process items within a single stage. Controls parallelism per stage instance. Must be greater than zero; setting it to 1 enforces strict sequential processing.

### `long WindowSizeMs`
Duration of a tumbling or sliding window in milliseconds. Defines the time span over which items are aggregated before being emitted. Required when `WindowType` is not `None`.

### `long WindowSlideMs`
Advance interval for sliding windows in milliseconds. Must be less than or equal to `WindowSizeMs`. Ignored for tumbling windows. A value equal to `WindowSizeMs` effectively produces tumbling behavior.

### `string WindowType`
Identifies the windowing strategy. Accepted values include `"Tumbling"`, `"Sliding"`, `"Session"`, and `"None"`. The runtime selects the corresponding window implementation at pipeline construction time.

### `int MaxRetries`
Maximum number of delivery or processing retry attempts for a single item before it is routed to the dead-letter channel. Applies per-stage. A value of zero disables retries.

### `long RetryDelayMs`
Fixed delay in milliseconds between consecutive retry attempts. Applied before the first retry and between subsequent attempts. No exponential backoff is applied unless implemented by a custom stage.

### `long ProcessingTimeoutMs`
Per-item processing deadline in milliseconds. If a stage does not complete processing within this window, the item is cancelled, the attempt is considered failed, and retry logic (if configured) is invoked.

### `double BackpressureTriggerThreshold`
Fraction of `MaxBufferSize` (between 0.0 and 1.0) at which the pipeline begins signalling backpressure to upstream producers. For example, a value of `0.8` triggers backpressure when the buffer reaches 80% capacity.

### `int MinDataQualityThreshold`
Minimum quality score (0–100) an item must possess to be accepted into the pipeline when `ValidateOnIngestion` is enabled. Items scoring below this threshold are rejected at ingestion and routed to a quarantine stream.

### `bool ValidateOnIngestion`
When `true`, every ingested item is validated against its schema and quality rules before entering the first stage. Invalid items are rejected early, reducing wasted processing downstream.

### `bool EnableMetricsCollection`
When `true`, the pipeline runtime emits detailed per-stage metrics (latency histograms, throughput counters, error rates) to the configured metrics sink. Disabling this reduces overhead for pipelines where observability is not required.

### `List<PipelineStageDef> Stages`
Ordered list of stage definitions that constitute the pipeline topology. Each `PipelineStageDef` specifies a stage type, its configuration parameters, and output routing rules. The list order determines the processing sequence; the first element is the entry stage, the last is the terminal stage.

### `Dictionary<string, object> CustomSettings`
Arbitrary key-value pairs for extension points or custom stage implementations. Values are stored as untyped objects; consuming code is responsible for casting to the expected type. Keys are case-sensitive.

### `DateTime CreatedAt`
UTC timestamp indicating when this configuration version was first persisted. Set by the storage layer and treated as immutable after creation.

### `DateTime LastModifiedAt`
UTC timestamp of the most recent modification to this configuration record. Updated by the storage layer on every write. Used for optimistic concurrency control during updates.

## Usage

### Example 1: Defining a Simple Two-Stage Pipeline with Tumbling Windows

```csharp
var config = new PipelineConfig
{
    ConfigId = 0, // Assigned by the store on first save
    PipelineName = "sensor-aggregator",
    Version = "1.0.0",
    MaxBufferSize = 10_000,
    BufferFlushIntervalMs = 500,
    MaxConcurrentConsumers = 4,
    WindowSizeMs = 60_000,
    WindowSlideMs = 0, // Irrelevant for Tumbling
    WindowType = "Tumbling",
    MaxRetries = 3,
    RetryDelayMs = 200,
    ProcessingTimeoutMs = 30_000,
    BackpressureTriggerThreshold = 0.75,
    MinDataQualityThreshold = 80,
    ValidateOnIngestion = true,
    EnableMetricsCollection = true,
    Stages = new List<PipelineStageDef>
    {
        new PipelineStageDef
        {
            StageType = "Filter",
            Config = new Dictionary<string, object> { ["predicate"] = "value > 0" }
        },
        new PipelineStageDef
        {
            StageType = "Aggregate",
            Config = new Dictionary<string, object> { ["function"] = "avg" }
        }
    },
    CustomSettings = new Dictionary<string, object>
    {
        ["output_topic"] = "sensor.aggregated"
    },
    CreatedAt = DateTime.UtcNow,
    LastModifiedAt = DateTime.UtcNow
};

// Validate and persist (hypothetical repository usage)
var validator = new PipelineConfigValidator();
validator.ValidateOrThrow(config);
await repository.SaveAsync(config);
```

### Example 2: Sliding Window with Backpressure Tuning and No Validation

```csharp
var config = new PipelineConfig
{
    PipelineName = "clickstream-sessionizer",
    Version = "2.1.0",
    MaxBufferSize = 50_000,
    BufferFlushIntervalMs = 100,
    MaxConcurrentConsumers = 8,
    WindowSizeMs = 300_000,   // 5-minute window
    WindowSlideMs = 60_000,   // 1-minute slide
    WindowType = "Sliding",
    MaxRetries = 2,
    RetryDelayMs = 500,
    ProcessingTimeoutMs = 60_000,
    BackpressureTriggerThreshold = 0.6,
    MinDataQualityThreshold = 0, // Irrelevant when validation is off
    ValidateOnIngestion = false,
    EnableMetricsCollection = true,
    Stages = new List<PipelineStageDef>
    {
        new PipelineStageDef { StageType = "Sessionize" },
        new PipelineStageDef { StageType = "Enrich", Config = new Dictionary<string, object> { ["source"] = "crm" } },
        new PipelineStageDef { StageType = "Sink", Config = new Dictionary<string, object> { ["target"] = "warehouse" } }
    },
    CustomSettings = new Dictionary<string, object>
    {
        ["dedup_key"] = "session_id",
        ["compression"] = "gzip"
    }
};

// Apply to a running pipeline manager
var manager = new PipelineManager();
await manager.DeployAsync(config);
```

## Notes

- **Configuration immutability at runtime:** Once a `PipelineConfig` is used to construct a pipeline, modifications to the object have no effect on the running instance. To change behaviour, deploy a new version with an incremented `Version` field.
- **`WindowSlideMs` and `WindowSizeMs` relationship:** When `WindowType` is `"Sliding"`, `WindowSlideMs` must be strictly greater than zero and less than or equal to `WindowSizeMs`. Violations are caught by the configuration validator, not by the property setters.
- **`BackpressureTriggerThreshold` bounds:** Values outside `[0.0, 1.0]` cause a validation error. A threshold of `1.0` means backpressure is only signalled when the buffer is completely full, which may cause upstream blocking with no advance warning.
- **`MinDataQualityThreshold` without validation:** This property is ignored when `ValidateOnIngestion` is `false`. Setting it to a non-zero value in that scenario has no effect but does not cause an error.
- **Thread safety:** `PipelineConfig` is a plain data object with no internal synchronization. Concurrent reads are safe; concurrent writes from multiple threads must be externally synchronized. The typical pattern is to construct the object on a single thread, validate it, and then treat it as read-only.
- **`Stages` ordering:** The list order is significant. The runtime constructs the processing graph sequentially from element 0 to element N-1. An empty list is valid only if the pipeline is intended as a no-op pass-through, which is unusual but permitted.
- **`CustomSettings` type casting:** Consumers of `CustomSettings` must perform their own type checking and casting. A missing key returns `null` (via `TryGetValue` semantics); an incorrectly typed value will cause an `InvalidCastException` at the point of use, not during configuration validation.
- **Timestamps and optimistic concurrency:** `CreatedAt` and `LastModifiedAt` are managed by the persistence layer. Manually setting them before an update may interfere with optimistic concurrency checks that rely on `LastModifiedAt` matching the stored record.
