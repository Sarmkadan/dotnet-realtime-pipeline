# PipelineConfigurationBuilder

A builder class for constructing `PipelineConfig` instances with configurable pipeline settings. It provides a fluent interface to sequentially apply various configuration options before finalizing the configuration with the `Build` method.

## API

### `public PipelineConfigurationBuilder()`
Initializes a new instance of the `PipelineConfigurationBuilder` with default settings.

### `public PipelineConfigurationBuilder WithBufferConfiguration(BufferConfiguration bufferConfiguration)`
Applies a buffer configuration to the pipeline.

- **Parameters**
  - `bufferConfiguration`: The buffer settings to apply.
- **Return value**
  - Returns the current `PipelineConfigurationBuilder` instance for method chaining.
- **Throws**
  - `ArgumentNullException`: If `bufferConfiguration` is `null`.

### `public PipelineConfigurationBuilder WithWindowingConfiguration(WindowingConfiguration windowingConfiguration)`
Applies a windowing configuration to the pipeline.

- **Parameters**
  - `windowingConfiguration`: The windowing settings to apply.
- **Return value**
  - Returns the current `PipelineConfigurationBuilder` instance for method chaining.
- **Throws**
  - `ArgumentNullException`: If `windowingConfiguration` is `null`.

### `public PipelineConfigurationBuilder WithPerformanceConfiguration(PerformanceConfiguration performanceConfiguration)`
Applies a performance configuration to the pipeline.

- **Parameters**
  - `performanceConfiguration`: The performance settings to apply.
- **Return value**
  - Returns the current `PipelineConfigurationBuilder` instance for method chaining.
- **Throws**
  - `ArgumentNullException`: If `performanceConfiguration` is `null`.

### `public PipelineConfigurationBuilder WithQualityConfiguration(QualityConfiguration qualityConfiguration)`
Applies a quality configuration to the pipeline.

- **Parameters**
  - `qualityConfiguration`: The quality settings to apply.
- **Return value**
  - Returns the current `PipelineConfigurationBuilder` instance for method chaining.
- **Throws**
  - `ArgumentNullException`: If `qualityConfiguration` is `null`.

### `public PipelineConfigurationBuilder WithStage(Func<PipelineStageBuilder, PipelineStageBuilder> stageBuilder)`
Applies a pipeline stage using a builder function.

- **Parameters**
  - `stageBuilder`: A function that configures a `PipelineStageBuilder` and returns it.
- **Return value**
  - Returns the current `PipelineConfigurationBuilder` instance for method chaining.
- **Throws**
  - `ArgumentNullException`: If `stageBuilder` is `null`.

### `public PipelineConfigurationBuilder WithCustomSetting(string key, object value)`
Applies a custom key-value setting to the pipeline.

- **Parameters**
  - `key`: The setting key.
  - `value`: The setting value.
- **Return value**
  - Returns the current `PipelineConfigurationBuilder` instance for method chaining.
- **Throws**
  - `ArgumentNullException`: If `key` is `null`.

### `public PipelineConfigurationBuilder Activate()`
Activates the pipeline configuration, enabling its settings to take effect.

- **Return value**
  - Returns the current `PipelineConfigurationBuilder` instance for method chaining.

### `public PipelineConfigurationBuilder Deactivate()`
Deactivates the pipeline configuration, disabling its settings.

- **Return value**
  - Returns the current `PipelineConfigurationBuilder` instance for method chaining.

### `public PipelineConfigurationBuilder WithHighPerformanceDefaults()`
Applies high-performance default settings to the pipeline.

- **Return value**
  - Returns the current `PipelineConfigurationBuilder` instance for method chaining.

### `public PipelineConfigurationBuilder WithLowLatencyDefaults()`
Applies low-latency default settings to the pipeline.

- **Return value**
  - Returns the current `PipelineConfigurationBuilder` instance for method chaining.

### `public PipelineConfigurationBuilder WithHighReliabilityDefaults()`
Applies high-reliability default settings to the pipeline.

- **Return value**
  - Returns the current `PipelineConfigurationBuilder` instance for method chaining.

### `public PipelineConfig Build()`
Finalizes and returns the constructed `PipelineConfig` instance.

- **Return value**
  - The configured `PipelineConfig` instance.
- **Throws**
  - `InvalidOperationException`: If required configurations are missing or invalid.

## Usage

### Example 1: Configuring a pipeline with buffer and windowing
