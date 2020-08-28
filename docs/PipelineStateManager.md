# PipelineStateManager

The `PipelineStateManager` provides centralized state management for real-time pipelines in .NET, enabling explicit state transitions, history tracking, and runtime overrides. It integrates with `ConfigurationStateManager` for dynamic configuration and `OperationMetricsTracker` for performance monitoring, ensuring deterministic pipeline behavior under varying conditions.

## API

### `public PipelineStateManager`

Constructs a new state manager with default initial state `PipelineState.Unknown`. The manager starts with no active overrides, empty state history, and zero recorded metrics.

### `public bool TransitionTo(PipelineState toState, string reason = null, Action<PipelineState, PipelineState> callback = null)`

Initiates a state transition from the current state to `toState`. If the transition is valid and not blocked by an override, the state is updated, the transition is recorded in history, and the optional callback is invoked with the previous and new states.

- **toState**: The target state to transition into.
- **reason**: Optional context explaining the transition (e.g., "network recovery").
- **callback**: Optional action invoked after the state change completes.
- **Returns**: `true` if the transition was accepted and executed; `false` otherwise (e.g., invalid transition or override prevents change).
- **Throws**: `ArgumentException` if `toState` is invalid or unsupported.

### `public void RegisterStateChangeListener(Action<StateTransition> listener)`

Registers a callback to be invoked whenever a state transition occurs.

- **listener**: A delegate that receives the transition details.
- **Throws**: `ArgumentNullException` if `listener` is `null`.

### `public List<StateTransition> GetStateHistory()`

Returns an immutable snapshot of all state transitions recorded since the manager was created, ordered chronologically from oldest to newest.

- **Returns**: A new list containing all historical transitions. Modifications to the returned list do not affect internal state.

### `public TimeSpan GetCurrentStateDuration()`

Calculates the elapsed time since the current state was entered.

- **Returns**: The duration the pipeline has remained in the current state. Returns `TimeSpan.Zero` if no state has been set or if the state was just entered.

### `public PipelineState FromState`

Gets the source state of the most recent transition. This is the state the pipeline was in immediately prior to the current state.

- **Returns**: The previous state. Returns `PipelineState.Unknown` if no transitions have occurred.

### `public PipelineState ToState`

Gets the current state of the pipeline after the most recent transition.

- **Returns**: The active state. Defaults to `PipelineState.Unknown` if no transitions have occurred.

### `public DateTime Timestamp`

Gets the UTC timestamp of the most recent state transition.

- **Returns**: The time when the current state was entered. Returns `DateTime.MinValue` if no transitions have occurred.

### `public string Reason`

Gets the optional reason provided for the most recent state transition.

- **Returns**: The transition reason, or `null` if none was specified.

### `public Action<PipelineState, PipelineState> Callback`

Gets the optional callback associated with the most recent state transition.

- **Returns**: The callback delegate, or `null` if none was provided.

### `public ConfigurationStateManager ConfigurationStateManager`

Gets the configuration manager used to resolve state-specific settings and overrides.

- **Returns**: The active `ConfigurationStateManager` instance.

### `public void SetOverride<T>(string key, T value)`

Registers a runtime override for a configuration value of type `T` identified by `key`. Overrides take precedence over configuration values and persist until explicitly removed.

- **key**: The configuration key to override.
- **value**: The value to use instead of the configured one.
- **Throws**: `ArgumentNullException` if `key` is `null`.

### `public T GetOverride<T>(string key)`

Retrieves the overridden value for the specified configuration key, if present.

- **key**: The configuration key to query.
- **Returns**: The overridden value of type `T`, or the default value for `T` if no override exists.
- **Throws**: `ArgumentNullException` if `key` is `null`.

### `public bool RemoveOverride(string key)`

Removes a previously set override for the given configuration key.

- **key**: The configuration key whose override should be removed.
- **Returns**: `true` if an override existed and was removed; `false` otherwise.
- **Throws**: `ArgumentNullException` if `key` is `null`.

### `public Dictionary<string, object> GetAllOverrides()`

Returns a snapshot of all active configuration overrides.

- **Returns**: A new dictionary mapping keys to their overridden values. The values are boxed and may require casting to the expected type.

### `public void ClearAllOverrides()`

Removes all active configuration overrides, reverting to configuration-based values.

### `public OperationMetricsTracker OperationMetricsTracker`

Gets the metrics tracker used to record and retrieve performance data for pipeline operations.

- **Returns**: The active `OperationMetricsTracker` instance.

### `public void RecordOperation(string operationName, TimeSpan duration)`

Records the duration of an operation within the pipeline.

- **operationName**: The name of the operation being measured.
- **duration**: The elapsed time of the operation.
- **Throws**: `ArgumentNullException` if `operationName` is `null` or empty.

### `public OperationMetrics GetOperationMetrics(string operationName)`

Retrieves performance metrics for a specific operation.

- **operationName**: The name of the operation to query.
- **Returns**: An `OperationMetrics` object containing duration statistics, or `null` if no metrics exist for the operation.
- **Throws**: `ArgumentNullException` if `operationName` is `null` or empty.

### `public Dictionary<string, OperationMetrics> GetAllMetrics()`

Returns a snapshot of all recorded operation metrics.

- **Returns**: A new dictionary mapping operation names to their metrics. The dictionary is immutable to external modifications.

## Usage
