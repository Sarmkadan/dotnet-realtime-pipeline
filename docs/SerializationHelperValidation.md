# SerializationHelperValidation

Utility class providing validation methods for serialized data structures used in the real-time pipeline. The class offers validation against schema rules and helper methods to enforce validity at runtime.

## API

### `ValidateSerialization`

Validates the structure and content of a serialized object against expected schema rules.

- **Parameters**: `object? data` – The object to validate.
- **Returns**: `IReadOnlyList<string>` – List of validation error messages; empty if valid.
- **Throws**: `ArgumentNullException` if `data` is null.

### `ValidateDataPoint`

Validates a single data point for correctness and completeness.

- **Parameters**: `object? dataPoint` – The data point to validate.
- **Returns**: `IReadOnlyList<string>` – List of validation error messages; empty if valid.
- **Throws**: `ArgumentNullException` if `dataPoint` is null.

### `ValidateProcessingResult`

Validates a processing result object for structural and semantic correctness.

- **Parameters**: `object? result` – The processing result to validate.
- **Returns**: `IReadOnlyList<string>` – List of validation error messages; empty if valid.
- **Throws**: `ArgumentNullException` if `result` is null.

### `ValidateMetricAggregation`

Validates a metric aggregation object for schema compliance and data integrity.

- **Parameters**: `object? aggregation` – The aggregation to validate.
- **Returns**: `IReadOnlyList<string>` – List of validation error messages; empty if valid.
- **Throws**: `ArgumentNullException` if `aggregation` is null.

### `ValidateDataPoints`

Validates a collection of data points for uniformity and correctness.

- **Parameters**: `IEnumerable? dataPoints` – The collection of data points to validate.
- **Returns**: `IReadOnlyList<string>` – List of validation error messages; empty if valid.
- **Throws**: `ArgumentNullException` if `dataPoints` is null.

### `ValidateProcessingResults`

Validates a collection of processing results for consistency and completeness.

- **Parameters**: `IEnumerable? results` – The collection of results to validate.
- **Returns**: `IReadOnlyList<string>` – List of validation error messages; empty if valid.
- **Throws**: `ArgumentNullException` if `results` is null.

### `IsValid` (overload 1)

Checks whether a serialized object conforms to expected schema rules.

- **Parameters**: `object? data` – The object to check.
- **Returns**: `bool` – `true` if valid; otherwise `false`.
- **Throws**: `ArgumentNullException` if `data` is null.

### `IsValid` (overload 2)

Checks whether a data point is structurally correct.

- **Parameters**: `object? dataPoint` – The data point to check.
- **Returns**: `bool` – `true` if valid; otherwise `false`.
- **Throws**: `ArgumentNullException` if `dataPoint` is null.

### `IsValid` (overload 3)

Checks whether a processing result is valid.

- **Parameters**: `object? result` – The result to check.
- **Returns**: `bool` – `true` if valid; otherwise `false`.
- **Throws**: `ArgumentNullException` if `result` is null.

### `IsValid` (overload 4)

Checks whether a metric aggregation is valid.

- **Parameters**: `object? aggregation` – The aggregation to check.
- **Returns**: `bool` – `true` if valid; otherwise `false`.
- **Throws**: `ArgumentNullException` if `aggregation` is null.

### `EnsureValid` (overload 1)

Validates a serialized object and throws if invalid.

- **Parameters**: `object? data` – The object to validate.
- **Throws**: `ArgumentNullException` if `data` is null.
- **Throws**: `InvalidOperationException` if validation fails.

### `EnsureValid` (overload 2)

Validates a data point and throws if invalid.

- **Parameters**: `object? dataPoint` – The data point to validate.
- **Throws**: `ArgumentNullException` if `dataPoint` is null.
- **Throws**: `InvalidOperationException` if validation fails.

### `EnsureValid` (overload 3)

Validates a processing result and throws if invalid.

- **Parameters**: `object? result` – The result to validate.
- **Throws**: `ArgumentNullException` if `result` is null.
- **Throws**: `InvalidOperationException` if validation fails.

### `EnsureValid` (overload 4)

Validates a metric aggregation and throws if invalid.

- **Parameters**: `object? aggregation` – The aggregation to validate.
- **Throws**: `ArgumentNullException` if `aggregation` is null.
- **Throws**: `InvalidOperationException` if validation fails.

## Usage
