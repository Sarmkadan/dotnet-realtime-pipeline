# HealthCheckServiceValidation

Provides validation utilities for health check services in the real-time pipeline, ensuring that registered health checks meet required criteria before being used by the pipeline's health monitoring infrastructure.

## API

### `Validate()`
Returns a list of validation messages indicating whether the current health check configuration is valid. Each message describes a specific validation failure; an empty list indicates full validity.

- **Parameters**: None
- **Returns**: `IReadOnlyList<string>` – A read-only list of validation messages. If the list is empty, the configuration is valid.
- **Exceptions**: None

### `IsValid()`
Determines whether the current health check configuration is valid.

- **Parameters**: None
- **Returns**: `bool` – `true` if the configuration is valid; otherwise, `false`.
- **Exceptions**: None

### `EnsureValid()`
Throws an exception if the current health check configuration is invalid, otherwise does nothing.

- **Parameters**: None
- **Returns**: `void`
- **Exceptions**: Throws an `InvalidOperationException` if the configuration is invalid.

## Usage

```csharp
// Example 1: Validating configuration during startup
var validationMessages = HealthCheckServiceValidation.Validate();
if (validationMessages.Count > 0)
{
    foreach (var message in validationMessages)
    {
        Console.WriteLine($"Validation issue: {message}");
    }
    throw new InvalidOperationException("Health check configuration is invalid.");
}

// Example 2: Enforcing validation in a background service
try
{
    HealthCheckServiceValidation.EnsureValid();
    Console.WriteLine("Health check configuration is valid.");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Health check validation failed: {ex.Message}");
    // Optionally trigger recovery or graceful shutdown
}
```

## Notes

- The validation logic is stateless and thread-safe; repeated calls to `Validate()`, `IsValid()`, or `EnsureValid()` will yield consistent results for a given configuration.
- If the health check service is reconfigured at runtime, callers must re-invoke these methods to reflect the updated state.
- `EnsureValid()` throws only when the configuration is invalid; it does not modify state or perform side effects.
