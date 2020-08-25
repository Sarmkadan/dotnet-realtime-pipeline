# ProcessingResult

A lightweight record-style class used to encapsulate the outcome of a single processing stage within a real-time data pipeline. It tracks success or failure, timing, retry behavior, output data, and correlation identifiers to support observability and error handling across pipeline stages.

## API

### Properties

- **`ResultId`**
  A unique identifier for this result. Intended to correlate with external systems or logs.

- **`Success`**
  Indicates whether the processing stage completed successfully. Defaults to `true`.

- **`StageName`**
  The name of the pipeline stage that produced this result (e.g., "Validation", "Enrichment").

- **`ProcessingTimeMs`**
  The duration of the processing stage in milliseconds. Must be non-negative.

- **`ErrorMessage`**
  A human-readable message describing the failure, if any. Optional.

- **`Exception`**
  The exception thrown during processing, if any. Optional.

- **`ProcessedAt`**
  The UTC timestamp when the stage completed processing.

- **`OutputData`**
  A dictionary of key-value pairs representing the structured output of the stage. Never `null`.

- **`RetryCount`**
  The number of times this result has been retried. Starts at zero.

- **`CorrelationId`**
  An optional identifier used to correlate this result with upstream or downstream operations.

### Constructors

- **`ProcessingResult()`**
  Initializes a new result with default values:
  `ResultId` auto-generated,
  `Success` = `true`,
  `ProcessedAt` = `DateTime.UtcNow`,
  `OutputData` = empty dictionary,
  `RetryCount` = 0.

- **`ProcessingResult(string stageName)`**
  Initializes a new result with the specified `stageName` and default values for all other fields.

### Methods

- **`MarkFailure(string errorMessage, Exception? exception = null)`**
  Marks the result as failed, sets `ErrorMessage` and `Exception`, and updates `ProcessedAt` to the current UTC time.
  *Throws:* `ArgumentNullException` if `errorMessage` is `null` or empty.

- **`MarkSuccess()`**
  Marks the result as successful, clears `ErrorMessage` and `Exception`, and updates `ProcessedAt` to the current UTC time.

- **`AddOutput(string key, object value)`**
  Adds a key-value pair to `OutputData`.
  *Throws:* `ArgumentNullException` if `key` is `null` or empty.
  *Throws:* `ArgumentException` if `key` already exists in `OutputData`.

- **`GetOutput(string key)`**
  Retrieves the value associated with `key` from `OutputData`.
  *Returns:* The stored value, or `null` if the key is not found.
  *Throws:* `ArgumentNullException` if `key` is `null` or empty.

- **`IncrementRetryCount()`**
  Increases `RetryCount` by one.

- **`IsValid()`**
  Determines whether the result is internally consistent.
  *Returns:* `true` if `Success` is `true` and `ErrorMessage` and `Exception` are both `null`; otherwise `false`.

- **`GetSummary()`**
  Generates a concise, human-readable summary of the result including `ResultId`, `StageName`, `Success`, `ProcessedAt`, `ErrorMessage`, and `RetryCount`.
  *Returns:* A formatted string suitable for logging.

- **`Clone()`**
  Creates a deep copy of the result, including a new `OutputData` dictionary with the same key-value pairs.
  *Returns:* A new `ProcessingResult` instance with identical values.

## Usage
