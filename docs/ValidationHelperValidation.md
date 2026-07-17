# ValidationHelperValidation

A utility class providing static methods for validating data and enforcing validation rules in C# applications. It offers a mix of validation checks, result aggregation, and exception-throwing helpers designed for pipeline and real-time processing scenarios where input integrity is critical.

## API

### `public static ValidationResult Validate(object? value, string paramName)`

Validates the provided `value` against common validation rules. The validation includes null checks, empty checks for strings, and basic type validation. If validation fails, a `ValidationResult` is returned with error details; otherwise, a success result is returned.

- **Parameters**
  - `value` (`object?`): The value to validate.
  - `paramName` (`string`): The name of the parameter being validated, used in error messages.
- **Returns**
  - `ValidationResult`: A result indicating success or failure with associated error messages.
- **Throws**
  - `ArgumentNullException`: If `paramName` is `null` or empty.

### `public static ValidationResult Validate(string? value, string paramName)`

Validates a string value for null, empty, or whitespace content. Returns a `ValidationResult` indicating whether the string passes validation.

- **Parameters**
  - `value` (`string?`): The string to validate.
  - `paramName` (`string`): The name of the parameter being validated.
- **Returns**
  - `ValidationResult`: A result indicating success or failure with error details.
- **Throws**
  - `ArgumentNullException`: If `paramName` is `null` or empty.

### `public static ValidationResult Validate<T>(T value, Func<T, bool> predicate, string paramName, string? errorMessage = null)`

Validates a value of type `T` using a custom predicate. If the predicate returns `false`, validation fails with an optional custom error message.

- **Parameters**
  - `value` (`T`): The value to validate.
  - `predicate` (`Func<T, bool>`): The validation function.
  - `paramName` (`string`): The name of the parameter being validated.
  - `errorMessage` (`string?`, *optional*): Custom error message if validation fails.
- **Returns**
  - `ValidationResult`: A result indicating success or failure.
- **Throws**
  - `ArgumentNullException`: If `paramName` or `predicate` is `null`.

### `public static ValidationResult Validate<T>(IEnumerable<T> values, Func<T, bool> predicate, string paramName, string? errorMessage = null)`

Validates each item in an enumerable using a custom predicate. Returns a `ValidationResult` with aggregated error messages for all failing items.

- **Parameters**
  - `values` (`IEnumerable<T>`): The collection of values to validate.
  - `predicate` (`Func<T, bool>`): The validation function applied to each item.
  - `paramName` (`string`): The name of the parameter being validated.
  - `errorMessage` (`string?`, *optional*): Custom error message prefix for failed items.
- **Returns**
  - `ValidationResult`: A result indicating success or failure with aggregated error messages.
- **Throws**
  - `ArgumentNullException`: If `paramName` or `predicate` is `null`.

### `public static bool IsInTimeRange(DateTime value, DateTime start, DateTime end)`

Determines whether the provided `value` falls within the specified time range `[start, end]`. The comparison is inclusive of both endpoints.

- **Parameters**
  - `value` (`DateTime`): The datetime to check.
  - `start` (`DateTime`): The start of the time range.
  - `end` (`DateTime`): The end of the time range.
- **Returns**
  - `bool`: `true` if `value` is within the range; otherwise, `false`.

### `public static bool IsWithinBounds<T>(T value, T min, T max) where T : IComparable<T>`

Checks whether `value` lies within the inclusive bounds defined by `min` and `max`. The comparison uses the natural ordering defined by `IComparable<T>`.

- **Parameters**
  - `value` (`T`): The value to check.
  - `min` (`T`): The lower bound.
  - `max` (`T`): The upper bound.
- **Returns**
  - `bool`: `true` if `value` is between `min` and `max` (inclusive); otherwise, `false`.
- **Type Parameters**
  - `T`: A type that implements `IComparable<T>`.

### `public static IReadOnlyList<string> Validate<T>(IEnumerable<T> values, Func<T, bool> predicate, string errorMessage)`

Validates each item in an enumerable using a predicate and collects all error messages for items that fail validation. Returns an immutable list of error messages.

- **Parameters**
  - `values` (`IEnumerable<T>`): The collection of values to validate.
  - `predicate` (`Func<T, bool>`): The validation function applied to each item.
  - `errorMessage` (`string`): The base error message to prepend for each failed item.
- **Returns**
  - `IReadOnlyList<string>`: An immutable list of error messages for failed items. Empty if all items pass.
- **Throws**
  - `ArgumentNullException`: If `predicate` or `errorMessage` is `null`.

### `public static bool IsValid(ValidationResult result)`

Determines whether the provided `ValidationResult` represents a successful validation.

- **Parameters**
  - `result` (`ValidationResult`): The validation result to check.
- **Returns**
  - `bool`: `true` if the result is valid (no errors); otherwise, `false`.

### `public static void EnsureValid(ValidationResult result)`

Throws an `ArgumentException` if the provided `ValidationResult` is invalid (i.e., contains errors). Otherwise, the method returns without action.

- **Parameters**
  - `result` (`ValidationResult`): The validation result to validate.
- **Throws**
  - `ArgumentException`: If `result` is invalid.

## Usage
