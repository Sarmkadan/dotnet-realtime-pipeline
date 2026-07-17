# LoggingMiddlewareValidation

The `LoggingMiddlewareValidation` static class provides validation utilities for logging middleware configurations in the `dotnet-realtime-pipeline` project. It ensures that logging middleware setups meet required constraints before execution, preventing runtime failures due to misconfigured logging components.

## API

### `Validate`

```csharp
public static IReadOnlyList<string> Validate()
```

Validates the current logging middleware configuration and returns a list of validation error messages. If the configuration is valid, the returned list will be empty.

- **Returns**: `IReadOnlyList<string>` – A read-only list of error messages describing any validation failures. Empty if validation passes.
- **Throws**: Does not throw exceptions; returns error messages instead.

### `IsValid`

```csharp
public static bool IsValid()
```

Determines whether the current logging middleware configuration is valid.

- **Returns**: `bool` – `true` if the configuration is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `EnsureValid`

```csharp
public static void EnsureValid()
```

Validates the current logging middleware configuration and throws an exception if validation fails.

- **Throws**: `InvalidOperationException` – If the configuration is invalid, with a message describing the validation failure.

## Usage

### Basic Validation Check

```csharp
if (LoggingMiddlewareValidation.IsValid())
{
    Console.WriteLine("Logging middleware is properly configured.");
}
else
{
    Console.WriteLine("Logging middleware requires configuration updates.");
}
```

### Enforcing Valid Configuration

```csharp
try
{
    LoggingMiddlewareValidation.EnsureValid();
    Console.WriteLine("Logging middleware is ready for execution.");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Failed to start logging middleware: {ex.Message}");
    // Handle invalid configuration
}
```

## Notes

- The validation logic operates on ambient or context-specific state (e.g., configuration objects, environment settings) rather than accepting parameters. This design assumes the validation target is globally or statically accessible within the application context.
- Thread safety depends on the underlying configuration state being immutable or properly synchronized. If the configuration can change at runtime, external synchronization is required to avoid race conditions during validation.
- The `EnsureValid` method throws `InvalidOperationException` to align with .NET conventions for guard clauses, avoiding dependency on custom exception types.
