# HealthCheckService

A service for performing health checks on pipeline components and reporting system-wide health status. It tracks component states, throughput, success rates, and provides both quick status snapshots and comprehensive health reports.

## API

### `HealthCheckService`
The main service class for health monitoring. Provides methods to register components, perform health checks, and retrieve status information.

### `void RegisterComponent(IHealthCheckable component)`
Registers a component for health monitoring.
- **Parameters**: `component` – The component implementing `IHealthCheckable` to be monitored.
- **Throws**: `ArgumentNullException` if `component` is `null`.

### `async Task<SystemHealthReport> PerformCompleteHealthCheckAsync()`
Performs a full health assessment of all registered components and system metrics.
- **Returns**: A `SystemHealthReport` containing detailed health data including component statuses, throughput, success rates, and overall system health.
- **Throws**: `InvalidOperationException` if no components are registered.

### `async Task<QuickHealthStatus> GetQuickStatusAsync()`
Retrieves a lightweight health status snapshot without full component checks.
- **Returns**: A `QuickHealthStatus` object with high-level health indicators (`IsHealthy`, `Message`, `OverallStatus`).
- **Throws**: `InvalidOperationException` if no components are registered.

### `ComponentStatus GetComponentStatus(string componentName)`
Gets the current status of a specific registered component.
- **Parameters**: `componentName` – The name of the component to query.
- **Returns**: The `ComponentStatus` of the specified component.
- **Throws**: `KeyNotFoundException` if the component is not registered.

### `bool IsHealthy`
Gets whether the system is currently in a healthy state.
- **Returns**: `true` if the system is healthy; otherwise, `false`.

### `string Message`
Gets a descriptive health message summarizing the current state.
- **Returns**: A string describing the health status (e.g., "All systems operational" or "High error rate detected").

### `DateTime CheckedAt`
Gets the timestamp of the last health check.
- **Returns**: The `DateTime` when the last check was performed.

### `Dictionary<string, object> Details`
Gets additional diagnostic details from the last health check.
- **Returns**: A dictionary of supplementary health metrics (e.g., queue depths, retry counts).

### `SystemHealth OverallStatus`
Gets the overall health status of the system.
- **Returns**: A `SystemHealth` enum value indicating the system's health (e.g., `Healthy`, `Degraded`, `Unhealthy`).

### `Dictionary<string, ComponentHealth> Components`
Gets the health status of all registered components.
- **Returns**: A dictionary mapping component names to their `ComponentHealth` statuses.

### `string PipelineStatus`
Gets the current pipeline operational status.
- **Returns**: A string describing the pipeline state (e.g., "Running", "Paused", "Failed").

### `double Throughput`
Gets the current throughput metric (items processed per unit time).
- **Returns**: A `double` representing the throughput rate.

### `double SuccessRate`
Gets the success rate of processed items (0.0 to 1.0).
- **Returns**: A `double` between 0.0 and 1.0 indicating the success ratio.

### `bool IsRunning`
Gets whether the pipeline is actively processing items.
- **Returns**: `true` if the pipeline is running; otherwise, `false`.

### `string HealthStatus`
Gets a human-readable health status summary.
- **Returns**: A string summarizing the system's health (e.g., "Healthy", "Warning: High error rate").

### `int PendingItems`
Gets the number of items currently queued for processing.
- **Returns**: The count of pending items.

### `bool ThroughputOk`
Gets whether the current throughput meets acceptable thresholds.
- **Returns**: `true` if throughput is within expected bounds; otherwise, `false`.

### `bool ErrorRateAcceptable`
Gets whether the current error rate is within acceptable limits.
- **Returns**: `true` if the error rate is acceptable; otherwise, `false`.

## Usage

### Example 1: Registering Components and Performing a Health Check
