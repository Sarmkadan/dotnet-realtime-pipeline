# PipelineVisualizerTestsExtensions
The `PipelineVisualizerTestsExtensions` class provides a set of extension methods for testing pipeline visualizations. It offers functionality to create pipeline configurations and visualization nodes, as well as assertions to verify the health and structure of pipeline visualizations. These extensions are designed to simplify the process of writing unit tests for pipeline visualizations, making it easier to ensure their correctness and reliability.

## API
* `public static PipelineConfig CreatePipelineConfig`: Creates a new pipeline configuration. This method returns a `PipelineConfig` object and does not throw any exceptions.
* `public static PipelineVisualizationNode CreateVisualizationNode`: Creates a new pipeline visualization node. This method returns a `PipelineVisualizationNode` object and does not throw any exceptions.
* `public static void ShouldHaveHealthLabel`: Asserts that a pipeline visualization node has a health label. This method throws an exception if the node does not have a health label.
* `public static void ShouldHaveStageCount`: Asserts that a pipeline visualization has a specific number of stages. This method throws an exception if the visualization does not have the expected number of stages.
* `public static void ShouldContainStageNames`: Asserts that a pipeline visualization contains specific stage names. This method throws an exception if the visualization does not contain the expected stage names.
* `public static void ShouldContainSeparatorCount`: Asserts that a pipeline visualization contains a specific number of separators. This method throws an exception if the visualization does not contain the expected number of separators.

## Usage
The following examples demonstrate how to use the `PipelineVisualizerTestsExtensions` class:
```csharp
// Create a pipeline configuration and visualization node
var pipelineConfig = PipelineVisualizerTestsExtensions.CreatePipelineConfig();
var visualizationNode = PipelineVisualizerTestsExtensions.CreateVisualizationNode();

// Assert that the visualization node has a health label
PipelineVisualizerTestsExtensions.ShouldHaveHealthLabel(visualizationNode);

// Create a pipeline visualization and assert that it has a specific number of stages
var pipelineVisualization = new PipelineVisualization(pipelineConfig);
PipelineVisualizerTestsExtensions.ShouldHaveStageCount(pipelineVisualization, 5);
```
```csharp
// Create a pipeline visualization and assert that it contains specific stage names
var pipelineVisualization = new PipelineVisualization(pipelineConfig);
PipelineVisualizerTestsExtensions.ShouldContainStageNames(pipelineVisualization, new[] { "Stage1", "Stage2", "Stage3" });

// Assert that the pipeline visualization contains a specific number of separators
PipelineVisualizerTestsExtensions.ShouldContainSeparatorCount(pipelineVisualization, 2);
```

## Notes
When using the `PipelineVisualizerTestsExtensions` class, note that the `ShouldHaveHealthLabel`, `ShouldHaveStageCount`, `ShouldContainStageNames`, and `ShouldContainSeparatorCount` methods will throw exceptions if the assertions fail. This can be useful for writing unit tests, but may not be desirable in other contexts. Additionally, the `CreatePipelineConfig` and `CreateVisualizationNode` methods do not throw exceptions, but may return null or default values if the creation process fails. The `PipelineVisualizerTestsExtensions` class is designed to be thread-safe, but the underlying pipeline visualization objects may not be. Therefore, it is recommended to use these extensions in a single-threaded context or to synchronize access to the pipeline visualization objects as needed.
