# LoggingMiddleware

A middleware component for instrumenting data ingestion, processing, and backpressure events in a .NET realtime pipeline. It provides structured logging, performance measurement, and correlation tracking across asynchronous operations.

## API

### `LoggingMiddleware`
The base middleware class that provides logging and correlation capabilities. Inherited by `PerformanceLoggingMiddleware` to add performance measurement.

### `void LogDataIngestion(object payload, string source)`
Logs the arrival of a new data payload in the pipeline.

- **Parameters**
  - `payload`: The data payload being ingested.
  - `source`: A string identifier for the source of the payload (e.g., "Kafka", "WebSocket").
- **Throws**
  - `ArgumentNullException`: If `payload` or `source` is `null`.

### `void LogProcessingCompletion(TimeSpan duration, bool success, string stage)`
Logs the completion of a processing stage.

- **Parameters**
  - `duration`: The time taken to complete the processing.
  - `success`: Indicates whether the processing succeeded.
  - `stage`: A string identifier for the processing stage (e.g., "Validation", "Transformation").
- **Throws**
  - `ArgumentNullException`: If `stage` is `null`.

### `void LogBackpressureEvent(int pendingItems, string stage)`
Logs a backpressure event when the pipeline is overwhelmed.

- **Parameters**
  - `pendingItems`: The number of items pending processing.
  - `stage`: A string identifier for the stage where backpressure occurred.
- **Throws**
  - `ArgumentNullException`: If `stage` is `null`.

### `void LogMetricsCollection(IReadOnlyDictionary<string, object> metrics, string context)`
Logs collected metrics from the pipeline.

- **Parameters**
  - `metrics`: A dictionary of metric names and values.
  - `context`: A string identifier for the context of the metrics (e.g., "Memory", "Throughput").
- **Throws**
  - `ArgumentNullException`: If `metrics` or `context` is `null`.

### `void LogError(Exception exception, string context)`
Logs an error that occurred in the pipeline.

- **Parameters**
  - `exception`: The exception to log.
  - `context`: A string identifier for the context in which the error occurred.
- **Throws**
  - `ArgumentNullException`: If `exception` or `context` is `null`.

### `void LogPerformanceWarning(TimeSpan duration, string operation, string context)`
Logs a performance warning when an operation exceeds expected thresholds.

- **Parameters**
  - `duration`: The duration of the operation.
  - `operation`: A string identifier for the operation being measured.
  - `context`: A string identifier for the context of the operation.
- **Throws**
  - `ArgumentNullException`: If `operation` or `context` is `null`.

### `PerformanceLoggingMiddleware`
A subclass of `LoggingMiddleware` that adds performance measurement capabilities to the base logging functionality.

### `async Task<T> MeasureAsync<T>(Func<Task<T>> operation, string operationName, string context)`
Measures the execution time of an asynchronous operation and logs performance metrics.

- **Parameters**
  - `operation`: The asynchronous operation to measure.
  - `operationName`: A string identifier for the operation being measured.
  - `context`: A string identifier for the context of the operation.
- **Returns**
  - `Task<T>`: The result of the measured operation.
- **Throws**
  - `ArgumentNullException`: If `operation`, `operationName`, or `context` is `null`.

### `T Measure<T>(Func<T> operation, string operationName, string context)`
Measures the execution time of a synchronous operation and logs performance metrics.

- **Parameters**
  - `operation`: The synchronous operation to measure.
  - `operationName`: A string identifier for the operation being measured.
  - `context`: A string identifier for the context of the operation.
- **Returns**
  - `T`: The result of the measured operation.
- **Throws**
  - `ArgumentNullException`: If `operation`, `operationName`, or `context` is `null`.

### `static string GetCorrelationId()`
Retrieves the current correlation ID for the executing context.

- **Returns**
  - `string`: The current correlation ID, or `null` if none is set.

### `static void SetCorrelationId(string correlationId)`
Sets the correlation ID for the executing context.

- **Parameters**
  - `correlationId`: The correlation ID to set.
- **Throws**
  - `ArgumentNullException`: If `correlationId` is `null`.

### `static void ClearCorrelationId()`
Clears the current correlation ID for the executing context.

### `async Task<T> WithCorrelationAsync<T>(string correlationId, Func<Task<T>> operation)`
Executes an asynchronous operation with a specified correlation ID, restoring the original ID afterward.

- **Parameters**
  - `correlationId`: The correlation ID to use for the operation.
  - `operation`: The asynchronous operation to execute.
- **Returns**
  - `Task<T>`: The result of the operation.
- **Throws**
  - `ArgumentNullException`: If `correlationId` or `operation` is `null`.

## Usage

### Basic Logging
