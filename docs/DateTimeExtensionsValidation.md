# DateTimeExtensionsValidation

Provides centralized validation logic for date and time values used throughout the pipeline. This static utility class exposes methods to check whether a given `DateTime` or `DateTimeOffset` meets domain-specific constraints—such as being in the past, falling within an allowed range, or having a valid kind—and to raise descriptive errors when validation fails. It is intended to guard entry points in scheduling, time‑window calculations, and event timestamp processing.

## API

### Validate (DateTime)

```csharp
public static IReadOnlyList<string> Validate(DateTime value, string parameterName)
```

Validates a `DateTime` value against all configured rules and returns a list of error messages.

- **Parameters:**
  - `value` (`DateTime`): The value to validate.
  - `parameterName` (`string`): The name of the parameter or field being validated, used in error messages.
- **Returns:** `IReadOnlyList<string>` containing zero or more error descriptions. An empty list indicates the value is valid.
- **Exceptions:** None. All errors are returned as strings.

### Method (DateTimeOffset)

```csharp
public static IReadOnlyList<string> Validate(DateTimeOffset value, string parameterName)
```

Validates a `DateTimeOffset` value against all standard rules and returns a list of error messages.

- **Parameters:**
  - `value` (`DateTimeOffset`): The value to validate.
  - `parameterName` (`string`): The parameter name for error messages.
- **Returns:** `IReadOnlyList<string>` of error descriptions; empty when valid.
- **Exceptions:** None.

### Method (DateTime with custom rules)

```csharp
public static IReadOnlyList<string> Validate(DateTime value, string parameterName, DateTimeValidationRules rules)
```

Validates a `DateTime` value using a specific set of rules.

- **Parameters:**
  - `value` (`DateTime`): The value to validate.
  - `parameterName` (`string`): The parameter name for error messages.
  - `rules` (`DateTimeValidationRules`): A flags enumeration specifying which checks to perform (e.g., `PastOnly`, `UtcOnly`, `NotDefault`).
- **Returns:** `IReadOnlyList<string>` of error messages; empty when valid.
- **Exceptions:** None.

### Method (DateTimeOffset with custom rules)

```csharp
public static IReadOnlyList<string> Validate(DateTimeOffset value, string parameterName, DateTimeValidationRules rules)
```

Validates a `DateTimeOffset` value using a custom set of rules.

- **Parameters:**
  - `value` (`DateTimeOffset`): The value to validate.
  - `parameterName` (`string`): The parameter name for error messages.
  - `rules` (`DateTimeValidationRules`): Flags enumeration specifying which checks to apply.
- **Returns:** `IReadOnlyList<string>` of error messages; empty when valid.
- **Exceptions:** None.

### IsValid (DateTime)

```csharp
public static bool IsValid(DateTime value)
```

Checks whether a `DateTime` value passes all default validation rules.

- **Parameters:**
  - `value` (`DateTime`): The value to check.
- **Returns:** `true` if the value satisfies all default constraints; otherwise `false`.
- **Exceptions:** None.

### IsValid (DateTimeOffset)

```csharp
public static bool IsValid(DateTimeOffset value)
```

Checks whether a `DateTimeOffset` value passes all default validation rules.

- **Parameters:**
  - `value` (`DateTimeOffset`): The value to check.
- **Returns:** `true` if valid; otherwise `false`.
- **Exceptions:** None.

### IsValid (with custom rules)

```csharp
public static bool IsValid(DateTime value, DateTimeValidationRules rules)
```

Checks whether a `DateTime` value passes a custom set of validation rules.

- **Parameters:**
  - `value` (`DateTime`): The value to check.
  - `rules` (`DateTimeValidationRules`): The specific rules to apply.
- **Returns:** `true` if the value satisfies all specified rules; otherwise `false`.
- **Exceptions:** None.

### EnsureValid (DateTime)

```csharp
public static void EnsureValid(DateTime value, string parameterName)
```

Throws an exception if the `DateTime` value fails any default validation rule.

- **Parameters:**
  - `value` (`DateTime`): The value to validate.
  - `parameterName` (`string`): The parameter name included in the exception message.
- **Exceptions:** Throws `ArgumentException` or a derived exception when validation fails.

### EnsureValid (DateTimeOffset)

```csharp
public static void EnsureValid(DateTimeOffset value, string parameterName)
```

Throws an exception if the `DateTimeOffset` value fails any default validation rule.

- **Parameters:**
  - `value` (`DateTimeOffset`): The value to validate.
  - `parameterName` (`string`): The parameter name included in the exception message.
- **Exceptions:** Throws `ArgumentException` or a derived type on validation failure.

### EnsureValid (with custom rules)

```csharp
public static void EnsureValid(DateTime value, string parameterName, DateTimeValidationRules rules)
```

Throws an exception if the `DateTime` value fails any of the specified custom validation rules.

- **Parameters:**
  - `value` (`DateTime`): The value to validate.
  - `parameterName` (`string`): The parameter name included in the exception message.
  - `rules` (`DateTimeValidationRules`): The specific rules to enforce.
- **Exceptions:** Throws `ArgumentException` or a derived type on the first encountered failure.

## Usage

### Example 1: Validating a pipeline event timestamp before processing

```csharp
public void EnqueueEvent(DateTimeOffset eventTimestamp, string payload)
{
    // Ensure the timestamp is not default and is in the past
    DateTimeExtensionsValidation.EnsureValid(
        eventTimestamp,
        nameof(eventTimestamp),
        DateTimeValidationRules.NotDefault | DateTimeValidationRules.PastOnly);

    // Proceed with enqueueing
    _eventQueue.Enqueue(new PipelineEvent(eventTimestamp, payload));
}
```

### Example 2: Collecting all validation errors for a batch of timestamps

```csharp
public IReadOnlyList<string> ValidateBatch(IEnumerable<DateTime> timestamps)
{
    var errors = new List<string>();
    int index = 0;

    foreach (var ts in timestamps)
    {
        var result = DateTimeExtensionsValidation.Validate(
            ts,
            $"timestamps[{index}]",
            DateTimeValidationRules.UtcOnly | DateTimeValidationRules.NotDefault);

        errors.AddRange(result);
        index++;
    }

    return errors;
}
```

## Notes

- All methods are static and thread-safe; they hold no mutable state and perform only read-only operations on their arguments.
- The `Validate` overloads never throw. They are suitable for scenarios where multiple errors must be aggregated before reporting.
- `EnsureValid` throws on the first rule violation encountered. The order of rule evaluation is an implementation detail and should not be relied upon.
- When `DateTimeValidationRules` is not supplied, the default rule set is applied. The exact composition of the default set is defined internally and may include checks such as `NotDefault` and `UtcOnly`.
- Passing `null` for `parameterName` will likely result in an `ArgumentNullException` from the underlying guard logic; callers should always supply a meaningful parameter name.
- `DateTimeOffset` overloads treat `Offset` as part of the value. Rules such as `UtcOnly` verify that the offset is zero, not just that the `DateTime` portion represents UTC.
- No validation is performed on `DateTimeKind` for `DateTime` values unless the rule set explicitly includes a kind check (e.g., `UtcOnly`).
