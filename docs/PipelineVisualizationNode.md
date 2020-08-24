# PipelineVisualizationNode

`PipelineVisualizationNode` is a data transfer object used to represent a stage in a real-time processing pipeline. It captures key metrics and relationships of a pipeline stage for visualization and monitoring purposes, including stage identity, performance indicators, backpressure status, and downstream dependencies.

## API

### `public string StageName`
Gets the name of the pipeline stage. This is a human-readable identifier for the stage and is used to uniquely identify the stage within the pipeline.

### `public string StageType`
Gets the type of the pipeline stage. This describes the functional role of the stage (e.g., "Filter", "Aggregator", "Sink") and is used for categorization and filtering in visualization tools.

### `public double BufferFillPercent`
Gets the current percentage of the stage's buffer that is filled. This value ranges from 0.0 to 100.0 and indicates how close the stage is to being backpressured due to high input rates or processing delays.

### `public bool IsBackpressured`
Gets a value indicating whether the stage is currently experiencing backpressure. This occurs when the stage's input buffer is full or nearly full, preventing further items from being accepted until processing catches up.

### `public double ThroughputEps`
Gets the stage's throughput in events per second. This metric reflects the rate at which the stage successfully processes and forwards items downstream.

### `public long DroppedItems`
Gets the total number of items dropped by the stage due to backpressure or other failure conditions. This counter is monotonically increasing and resets only when the pipeline restarts.

### `public string HealthLabel`
Gets a label indicating the overall health of the stage. This is a human-readable string such as "Healthy", "Warning", or "Critical", derived from internal health metrics and thresholds.

### `public List<string> DownstreamStages`
Gets the names of the stages that directly consume output from this stage. This list represents the immediate downstream dependencies in the pipeline and is used to render the visualization graph.

### `public string ComputeHealthLabel()`
Computes and returns a health label for the stage based on its current metrics (e.g., buffer fill, throughput, dropped items). The label is derived from internal thresholds and may return values such as "Healthy", "Degraded", or "Failed".

### `public string ToInlineString()`
Generates a compact, single-line string representation of the node suitable for logging or status displays. The format includes the stage name, type, buffer fill percentage, and health label (e.g., `"Filter-1 [85% | Warning]"`).

## Usage

```csharp
// Example 1: Monitoring a pipeline stage
var node = new PipelineVisualizationNode
{
    StageName = "DataCleaner",
    StageType = "Filter",
    BufferFillPercent = 72.5,
    IsBackpressured = false,
    ThroughputEps = 1240.8,
    DroppedItems = 42,
    HealthLabel = "Warning"
};
node.DownstreamStages.Add("Aggregator-1");
node.DownstreamStages.Add("Aggregator-2");

Console.WriteLine(node.ToInlineString());
// Output: "DataCleaner [72.5% | Warning]"
```

```csharp
// Example 2: Building a visualization graph
var nodes = new List<PipelineVisualizationNode>();
var node1 = new PipelineVisualizationNode
{
    StageName = "Source",
    StageType = "Ingress",
    BufferFillPercent = 0.0,
    IsBackpressured = false,
    ThroughputEps = 2000.0,
    DroppedItems = 0,
    HealthLabel = "Healthy"
};
var node2 = new PipelineVisualizationNode
{
    StageName = "Processor",
    StageType = "Transform",
    BufferFillPercent = 45.2,
    IsBackpressured = false,
    ThroughputEps = 1850.0,
    DroppedItems = 5,
    HealthLabel = "Healthy"
};
node1.DownstreamStages.Add("Processor");
nodes.Add(node1);
nodes.Add(node2);

// Render graph edges based on DownstreamStages
foreach (var node in nodes)
{
    foreach (var downstream in node.DownstreamStages)
    {
        Console.WriteLine($"{node.StageName} -> {downstream}");
    }
}
// Output:
// Source -> Processor
```

## Notes

- `StageName` and `StageType` are expected to be non-null and non-empty strings. The `ToInlineString()` method may produce misleading output if these are empty.
- `BufferFillPercent` is a `double` in the range [0.0, 100.0]. Values outside this range are clamped by the visualization layer or ignored.
- `IsBackpressured` is derived from internal buffer thresholds and may lag slightly behind actual buffer state due to asynchronous updates.
- `DroppedItems` is a monotonically increasing counter. It resets only when the pipeline restarts; thread-safe increments are assumed by the monitoring system.
- `DownstreamStages` is a mutable `List<string>` exposed for convenience. Concurrent modifications are not supported; use synchronization if accessed from multiple threads.
- `ComputeHealthLabel()` and `ToInlineString()` are pure functions with no side effects. They are thread-safe provided that the underlying properties (`BufferFillPercent`, `ThroughputEps`, etc.) are not modified during execution.
