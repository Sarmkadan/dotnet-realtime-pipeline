# BackpressureServiceExtensions

Provides extension methods for configuring and monitoring backpressure behavior in real-time data pipelines. These methods enable runtime inspection of buffer health, consumer registration, and backpressure metrics across pipeline stages.

## API

### `public static BackpressureContext GetOrCreateContext(IServiceProvider services)`
Creates or retrieves a `BackpressureContext` instance from the dependency injection container. The context manages buffer state and metrics for backpressure decisions.

- **Returns**: A `BackpressureContext` instance.
- **Throws**: `InvalidOperationException` if the service is not registered or cannot be resolved.

---

### `public static bool SafeAddToBuffer<T>(IBuffer<T> buffer, T item)`
Safely attempts to add an item to the buffer, handling overflow conditions gracefully.

- **Parameters**:
  - `buffer`: The target buffer.
  - `item`: The item to add.
- **Returns**: `true` if the item was added; `false` if the buffer rejected the item due to backpressure or capacity limits.
- **Throws**: `ArgumentNullException` if `buffer` is `null`.

---

### `public static double GetBufferFillPercentage(IBuffer buffer)`
Calculates the current fill percentage of the buffer.

- **Parameters**:
  - `buffer`: The buffer to inspect.
- **Returns**: A value between `0.0` and `1.0` representing the fill percentage. Returns `0.0` if the buffer is empty or `null`.
- **Throws**: `ArgumentNullException` if `buffer` is `null`.

---

### `public static bool ShouldApplyBackpressure(IBuffer buffer)`
Determines whether backpressure should be applied based on the buffer's current state.

- **Parameters**:
  - `buffer`: The buffer to evaluate.
- **Returns**: `true` if backpressure should be applied; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `buffer` is `null`.

---
### `public static long GetDroppedItemCount(IBuffer buffer)`
Retrieves the total number of items dropped by the buffer due to backpressure or capacity constraints.

- **Parameters**:
  - `buffer`: The buffer to query.
- **Returns**: The count of dropped items. Returns `0` if the buffer is `null` or has no drop tracking.
- **Throws**: `ArgumentNullException` if `buffer` is `null`.

---
### `public static string GetBufferStatusReport(IBuffer buffer)`
Generates a human-readable status report for the buffer, including fill level, dropped items, and health indicators.

- **Parameters**:
  - `buffer`: The buffer to report on.
- **Returns**: A formatted string with buffer metrics. Returns `null` if the buffer is `null`.
- **Throws**: `ArgumentNullException` if `buffer` is `null`.

---
### `public static async Task<bool> TryRegisterConsumerAsync(IBuffer buffer, string consumerId, int maxItems)`
Attempts to register a consumer with the buffer, optionally limiting the number of items it can consume.

- **Parameters**:
  - `buffer`: The buffer to register with.
  - `consumerId`: A unique identifier for the consumer.
  - `maxItems`: The maximum number of items the consumer may process (use `-1` for unlimited).
- **Returns**: A `Task<bool>` that resolves to `true` if registration succeeded; otherwise, `false`.
- **Throws**:
  - `ArgumentNullException` if `buffer` or `consumerId` is `null`.
  - `ArgumentException` if `consumerId` is empty or whitespace.

---
### `public static (BackpressureSystemStatus Status, BackpressureMetrics Metrics) GetEnhancedSystemStatus(IServiceProvider services)`
Retrieves a comprehensive view of the backpressure system, including status and detailed metrics.

- **Parameters**:
  - `services`: The service provider containing the backpressure context.
- **Returns**: A tuple with `Status` (system-wide backpressure state) and `Metrics` (detailed metrics).
- **Throws**: `InvalidOperationException` if the backpressure context is not available.

---
### `public static void RecordBufferMetric(IBuffer buffer, double fillRatio)`
Records a buffer fill metric for monitoring and scaling decisions.

- **Parameters**:
  - `buffer`: The buffer to record against.
  - `fillRatio`: The current fill ratio (0.0 to 1.0).
- **Throws**:
  - `ArgumentNullException` if `buffer` is `null`.
  - `ArgumentOutOfRangeException` if `fillRatio` is outside the valid range.

---
### `public static double GetBackpressureFrequency(IServiceProvider services)`
Calculates the frequency of backpressure events across the system over a recent window.

- **Parameters**:
  - `services`: The service provider containing the backpressure context.
- **Returns**: A value representing the rate of backpressure events per second. Returns `0.0` if no events occurred.
- **Throws**: `InvalidOperationException` if the backpressure context is not available.

---
### `public int TotalStages { get; }`
Gets the total number of pipeline stages monitored by the backpressure system.

---
### `public int BackpressuredStages { get; }`
Gets the number of stages currently experiencing backpressure.

---
### `public int HealthyStages { get; }`
Gets the number of stages operating within normal parameters (no backpressure).

---
### `public int WarningStages { get; }`
Gets the number of stages operating near capacity thresholds (warning state).

---
### `public int CriticalStages { get; }`
Gets the number of stages under critical load (high backpressure risk).

---
### `public double AverageBufferFillPercent { get; }`
Gets the average buffer fill percentage across all monitored stages.

---
### `public long TotalBackpressureTimeMs { get; }`
Gets the cumulative duration (in milliseconds) that stages have been under backpressure.

---
### `public long TotalDroppedItems { get; }`
Gets the total number of items dropped system-wide due to backpressure.

---
### `public bool IsSystemBackpressured { get; }`
Gets a value indicating whether the system is currently under backpressure.

---
### `public string HealthStatus { get; }`
Gets a human-readable health status summarizing the system's backpressure state (e.g., "Healthy", "Warning", "Critical").

## Usage

### Example 1: Monitoring Buffer Health
