# PipelineVisualizer

A utility class for visualizing real-time data processing pipelines by generating node-based representations of pipeline structures. It converts pipeline configurations into human-readable diagrams, supporting both detailed and compact rendering formats.

## API

### `PipelineVisualizer`

The default constructor initializes a new instance of the `PipelineVisualizer` class. This instance is stateless and thread-safe for concurrent use.

### `List<PipelineVisualizationNode> BuildNodes`

Generates a list of visualization nodes representing the pipeline structure.

- **Returns**: A `List<PipelineVisualizationNode>` where each node corresponds to a component in the pipeline.
- **Throws**: `InvalidOperationException` if the pipeline is in an invalid state (e.g., missing required components).

### `string Render`

Renders the pipeline visualization as a formatted string with detailed node information.

- **Returns**: A string containing the full visualization, including node details and connections.
- **Throws**: `InvalidOperationException` if `BuildNodes` has not been called or fails.

### `string RenderCompact`

Renders the pipeline visualization as a compact string, omitting non-critical details.

- **Returns**: A string containing a condensed visualization suitable for logging or quick inspection.
- **Throws**: `InvalidOperationException` if `BuildNodes` has not been called or fails.

## Usage
