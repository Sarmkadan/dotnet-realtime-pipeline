# PipelineVisualizerTests

The `PipelineVisualizerTests` class serves as the comprehensive test suite for validating the logic, rendering output, and health status computations within the pipeline visualization subsystem of the `dotnet-realtime-pipeline` project. It ensures that pipeline nodes are constructed correctly from configuration, that sequential edge linking functions as intended, and that the textual or structural representation of the pipeline accurately reflects stage names, separators, and operational health states such as backpressure or buffer warnings.

## API

### `BuildNodes_WithValidConfig_ReturnsOneNodePerStage`
Verifies the node construction logic when provided with a valid configuration object. This test asserts that the resulting collection of visualization nodes contains exactly one node for every stage defined in the input configuration, ensuring no stages are skipped or duplicated during the build process. It takes no parameters and returns void; it throws an assertion exception if the node count mismatches the stage count.

### `BuildNodes_EdgesAreLinkedSequentially`
Validates the topological integrity of the generated graph by confirming that edges between nodes are linked in the correct sequential order. This ensures that data flow representation moves linearly from the first stage to the last without broken links or incorrect ordering. It takes no parameters and returns void; it throws an assertion exception if the edge sequence does not match the expected stage order.

### `Render_ContainsPipelineName`
Asserts that the full rendering output of the pipeline visualizer includes the specific name assigned to the pipeline instance. This guarantees that the root identifier is present in the final visualization payload. It takes no parameters and returns void; it throws an assertion exception if the pipeline name is missing from the rendered output.

### `Render_ContainsAllStageNames`
Confirms that the rendering output includes the names of all individual stages within the pipeline. This test iterates through expected stage identifiers to ensure complete visibility in the visualization. It takes no parameters and returns void; it throws an assertion exception if any stage name is absent from the rendered result.

### `RenderCompact_ContainsSeparators`
Specifically targets the compact rendering mode to verify the presence of delimiters or separators between pipeline elements. This ensures that even in condensed views, distinct stages remain visually distinguishable. It takes no parameters and returns void; it throws an assertion exception if the expected separators are missing from the compact render string.

### `PipelineVisualizationNode_ComputeHealthLabel_BackpressuredIsCritical`
Tests the `ComputeHealthLabel` logic within the `PipelineVisualizationNode` context under backpressure conditions. It asserts that when a node detects backpressure, the resulting health label is correctly categorized as "Critical". It takes no parameters and returns void; it throws an assertion exception if the computed label is not "Critical".

### `PipelineVisualizationNode_ComputeHealthLabel_HighBufferIsWarning`
Validates the health status computation when buffer usage exceeds standard thresholds but does not yet constitute a failure. It asserts that a high buffer state results in a "Warning" health label. It takes no parameters and returns void; it throws an assertion exception if the computed label is not "Warning".

### `PipelineVisualizationNode_ComputeHealthLabel_NormalIsHealthy`
Ensures that under normal operating conditions, where no backpressure or high buffer usage is detected, the health label defaults to "Healthy". This establishes the baseline behavior for the health computation logic. It takes no parameters and returns void; it throws an assertion exception if the computed label is not "Healthy".

## Usage

The following examples demonstrate how the logic covered by these tests might be invoked in a real-world scenario, typically within a test harness or a diagnostic tool.

**Example 1: Validating Pipeline Structure and Rendering**
This example illustrates constructing nodes from a configuration and verifying the rendering output contains necessary identifiers.

```csharp
var config = new PipelineConfig
{
    Name = "DataIngestionPipeline",
    Stages = new[] { "Extract", "Transform", "Load" }
};

var visualizer = new PipelineVisualizer(config);
var nodes = visualizer.BuildNodes();

// Assert structure matches test expectations
if (nodes.Count != config.Stages.Length)
{
    throw new InvalidOperationException("Node count mismatch.");
}

var renderOutput = visualizer.Render();
if (!renderOutput.Contains(config.Name) || !renderOutput.Contains("Transform"))
{
    throw new InvalidOperationException("Render output missing required metadata.");
}
```

**Example 2: Evaluating Node Health Status**
This example demonstrates checking the health label computation logic for different internal states of a pipeline node.

```csharp
var node = new PipelineVisualizationNode("Stage1");

// Simulate backpressure
node.CurrentState = NodeState.Backpressured;
var healthCritical = node.ComputeHealthLabel();
// Expected: "Critical"

// Simulate high buffer usage
node.CurrentState = NodeState.HighBuffer;
var healthWarning = node.ComputeHealthLabel();
// Expected: "Warning"

// Simulate normal operation
node.CurrentState = NodeState.Normal;
var healthHealthy = node.ComputeHealthLabel();
// Expected: "Healthy"
```

## Notes

*   **Execution Context**: As a test class, `PipelineVisualizerTests` is designed to be executed within a unit testing framework (e.g., xUnit, NUnit, MSTest). The methods are instance members intended to be invoked by a test runner, not directly called in production application logic.
*   **Thread Safety**: The methods listed are stateless test assertions or invoke stateless logic on temporary objects created within the test scope. They do not maintain internal static state, making them inherently safe for parallel test execution, provided the underlying `PipelineVisualizer` and `PipelineVisualizationNode` classes adhere to standard thread-safety practices regarding the objects they instantiate.
*   **Edge Cases**: The tests implicitly cover edge cases such as empty stage lists (via `BuildNodes_WithValidConfig_ReturnsOneNodePerStage` if configured so), single-stage pipelines, and boundary conditions for buffer thresholds. The health label tests specifically isolate the three discrete states (Backpressured, HighBuffer, Normal), implying that undefined or intermediate states should be handled by the implementation to default to a safe category, though such behavior is not explicitly asserted in this specific set of members.
*   **Dependencies**: These tests assume the existence of `PipelineConfig`, `PipelineVisualizer`, and `PipelineVisualizationNode` types with compatible constructors and properties. Changes to the internal structure of these dependencies may require updates to the test implementation, though the public contract verified here should remain stable.
