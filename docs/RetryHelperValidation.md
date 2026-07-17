# RetryHelperValidation

Provides static validation methods for retry configuration objects used in the `dotnet-realtime-pipeline` library. The class centralizes checks for common retry parameters such as maximum retry count, delay intervals, and backoff strategies, returning a list of error messages, a boolean validity indicator, or throwing an exception when validation fails.

## API

All members are `public static`.

### Validate

```csharp
public static IReadOnlyList<string> Validate(/* overload-specific parameters */)
```

Validates the supplied retry configuration and returns a read‑only list of error messages. Each overload accepts a different combination of retry parameters (e.g., `RetryOptions`, `int maxRetries`, `TimeSpan delay`, `TimeSpan maxDelay`, `RetryMode mode`). The returned list is empty when the configuration is valid.

- **Returns**: `IReadOnlyList<string>` – zero or more human‑readable error descriptions. Never `null`.

### IsValid

```csharp
public static bool IsValid(/* overload-specific parameters */)
```

Returns `true` if the supplied retry configuration passes all validation rules; otherwise `false`. This is a convenience wrapper around `Validate` that avoids allocating the error list when only a boolean answer is needed.

- **Returns**: `bool` – `true` if no validation errors are found.

### EnsureValid

```csharp
public static void EnsureValid(/* overload-specific parameters */)
```

Validates the supplied retry configuration and throws an `ArgumentException` (or a more specific derived exception) if any validation rule is violated. The exception message contains the concatenated error descriptions.

- **Throws**: `ArgumentException` – when the configuration is invalid. The `ParamName` property is set to the name of the first invalid parameter, if applicable.

## Usage

### Example 1: Validate and inspect errors

```csharp
using DotNetRealtimePipeline.Retry;

var options = new RetryOptions
{
    MaxRetries = -1,
    Delay = TimeSpan.FromSeconds(-5)
};

IReadOnlyList<string> errors = RetryHelperValidation.Validate(options);
if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

### Example 2: Ensure valid configuration before use

```csharp
using DotNetRealtimePipeline.Retry;

public void ConfigurePipeline(RetryOptions options)
{
    RetryHelperValidation.EnsureValid(options);
    // Configuration is guaranteed valid – proceed with pipeline setup.
}
```

## Notes

- All static methods are thread‑safe. They do not modify any shared state and can be called concurrently from multiple threads.
- `Validate` and `IsValid` never throw exceptions under normal conditions. They return an empty list or `false` respectively for valid input.
- `EnsureValid` throws `ArgumentException` only when validation fails. It does not throw for `null` arguments; instead, a null argument is treated as an invalid configuration and will produce an appropriate error message.
- Overloads that accept `int` or `TimeSpan` parameters directly will validate those values independently (e.g., negative retry counts, zero or negative delays). Overloads accepting a `RetryOptions` object validate all properties of that object.
- The returned `IReadOnlyList<string>` from `Validate` is a snapshot; it is not backed by the input object and will not change if the input is later modified.
