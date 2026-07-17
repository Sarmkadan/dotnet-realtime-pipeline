# BackpressureMetricsCollectorValidation

Provides validation utilities for `BackpressureMetricsCollector` configurations. This static class contains methods to verify the correctness of backpressure metric collection settings, ensuring they meet required constraints before being applied to a pipeline. Validation failures typically result in descriptive error messages that can be used for debugging or user feedback.

## API

### `public static IReadOnlyList<string> Validate(BackpressureMetricsCollector collector)`
Validates the given `BackpressureMetricsCollector` instance and returns a list of error messages if any validation rules are violated. If the collector is valid, the returned list is empty.

**Parameters:**
- `collector` – The `BackpressureMetricsCollector` instance to validate.

**Returns:**
- An `IReadOnlyList<string>` containing zero or more error messages describing validation failures. An empty list indicates a valid collector.

**Throws:**
- `ArgumentNullException` – Thrown if `collector` is `null`.

---

### `public static IReadOnlyList<string> Validate(BackpressureMetricsCollector? collector, string paramName)`
Validates the given `BackpressureMetricsCollector` instance and returns a list of error messages if any validation rules are violated. If the collector is valid, the returned list is empty. The provided `paramName` is used in exception messages for better debugging context.

**Parameters:**
- `collector` – The `BackpressureMetricsCollector` instance to validate.
- `paramName` – The name of the parameter being validated, used in exception messages.

**Returns:**
- An `IReadOnlyList<string>` containing zero or more error messages describing validation failures. An empty list indicates a valid collector.

**Throws:**
- `ArgumentNullException` – Thrown if `paramName` is `null` or whitespace.
- `ArgumentException` – Thrown if `collector` is `null` (with `paramName` included in the message).

---

### `public static bool IsValid(BackpressureMetricsCollector collector)`
Determines whether the given `BackpressureMetricsCollector` instance is valid according to the defined validation rules.

**Parameters:**
- `collector` – The `BackpressureMetricsCollector` instance to check.

**Returns:**
- `true` if the collector is valid; otherwise, `false`.

**Throws:**
- `ArgumentNullException` – Thrown if `collector` is `null`.

---

### `public static bool IsValid(BackpressureMetricsCollector? collector, out IReadOnlyList<string> errors)`
Determines whether the given `BackpressureMetricsCollector` instance is valid and provides a list of error messages if validation fails.

**Parameters:**
- `collector` – The `BackpressureMetricsCollector` instance to check.
- `errors` – Output parameter containing zero or more error messages if validation fails. If the collector is valid, this list is empty.

**Returns:**
- `true` if the collector is valid; otherwise, `false`.

**Throws:**
- `ArgumentNullException` – Thrown if `collector` is `null`.

---

### `public static void EnsureValid(BackpressureMetricsCollector collector)`
Ensures the given `BackpressureMetricsCollector` instance is valid by throwing an `ArgumentException` if validation fails. The exception message includes all validation errors.

**Parameters:**
- `collector` – The `BackpressureMetricsCollector` instance to validate.

**Throws:**
- `ArgumentNullException` – Thrown if `collector` is `null`.
- `ArgumentException` – Thrown if validation fails, containing all error messages.

---

### `public static void EnsureValid(BackpressureMetricsCollector? collector, string paramName)`
Ensures the given `BackpressureMetricsCollector` instance is valid by throwing an `ArgumentException` if validation fails. The exception message includes all validation errors and the provided `paramName` for context.

**Parameters:**
- `collector` – The `BackpressureMetricsCollector` instance to validate.
- `paramName` – The name of the parameter being validated, used in exception messages.

**Throws:**
- `ArgumentNullException` – Thrown if `collector` or `paramName` is `null` or whitespace.
- `ArgumentException` – Thrown if validation fails, containing all error messages and `paramName`.

## Usage

### Example 1: Validating a Collector Before Pipeline Initialization
