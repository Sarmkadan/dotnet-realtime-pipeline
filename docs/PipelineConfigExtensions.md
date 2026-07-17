# PipelineConfigExtensions

Provides extension methods for `PipelineConfig` that simplify querying and retrieving pipeline stage definitions.

## API

### `GetTotalStages`

Gets the total number of stages defined in the pipeline configuration.

- **Returns**: `int` – The total number of stages.
- **Throws**: `ArgumentNullException` if the pipeline configuration is `null`.

### `HasStages`

Determines whether the pipeline configuration contains any stages.

- **Returns**: `bool` – `true` if the pipeline contains one or more stages; otherwise, `false`.
- **Throws**: `ArgumentNullException` if the pipeline configuration is `null`.

### `GetStageNames`

Retrieves the names of all stages defined in the pipeline configuration.

- **Returns**: `IEnumerable<string>` – An enumerable of stage names.
- **Throws**: `ArgumentNullException` if the pipeline configuration is `null`.

### `HasStage`

Determines whether a stage with the specified name exists in the pipeline configuration.

- **Parameters**:
  - `name` (`string`) – The name of the stage to check.
- **Returns**: `bool` – `true` if a stage with the given name exists; otherwise, `false`.
- **Throws**:
  - `ArgumentNullException` if the pipeline configuration or `name` is `null`.
  - `ArgumentException` if `name` is an empty or whitespace string.

### `GetStageByName`

Gets the stage definition with the specified name from the pipeline configuration.

- **Parameters**:
  - `name` (`string`) – The name of the stage to retrieve.
- **Returns**: `PipelineStageDef?` – The stage definition if found; otherwise, `null`.
- **Throws**:
  - `ArgumentNullException` if the pipeline configuration or `name` is `null`.
  - `ArgumentException` if `name` is an empty or whitespace string.

### `FindStage`

Finds the first stage definition matching the specified predicate in the pipeline configuration.

- **Parameters**:
  - `predicate` (`Func<PipelineStageDef, bool>`) – A function to test each stage.
- **Returns**: `PipelineStageDef?` – The first matching stage definition if found; otherwise, `null`.
- **Throws**:
  - `ArgumentNullException` if the pipeline configuration or `predicate` is `null`.

### `FindStages`

Finds all stage definitions matching the specified predicate in the pipeline configuration.

- **Parameters**:
  - `predicate` (`Func<PipelineStageDef, bool>`) – A function to test each stage.
- **Returns**: `IEnumerable<PipelineStageDef>` – An enumerable of matching stage definitions.
- **Throws**: `ArgumentNullException` if the pipeline configuration or `predicate` is `null`.

### `GetEnabledStages`

Retrieves all enabled stage definitions from the pipeline configuration.

- **Returns**: `IEnumerable<PipelineStageDef>` – An enumerable of enabled stage definitions.
- **Throws**: `ArgumentNullException` if the pipeline configuration is `null`.

### `GetStagesByType`

Retrieves all stage definitions of a specific type from the pipeline configuration.

- **Parameters**:
  - `stageType` (`Type`) – The type of stages to retrieve.
- **Returns**: `IEnumerable<PipelineStageDef>` – An enumerable of stage definitions of the specified type.
- **Throws**:
  - `ArgumentNullException` if the pipeline configuration or `stageType` is `null`.
  - `ArgumentException` if `stageType` is not a valid stage type.

## Usage
