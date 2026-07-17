# BackgroundProcessingWorkerValidation

The `BackgroundProcessingWorkerValidation` class provides a centralized set of static utility methods for verifying the configuration and state of background processing workers within the realtime pipeline. It exposes validation logic to check for common misconfigurations, returning detailed error messages or throwing exceptions to fail fast during application startup or dynamic reconfiguration scenarios.

## API

### Validate
```csharp
public static IReadOnlyList<string> Validate()
```
Executes a comprehensive validation check against the current background worker configuration.
*   **Purpose**: To inspect the environment and configuration for invalid states without halting execution.
*   **Parameters**: None.
*   **Return Value**: An `IReadOnlyList<string>` containing descriptive error messages for each validation failure detected. If the configuration is valid, the list is empty.
*   **Throws**: This method does not throw exceptions for validation failures; it aggregates them into the return list.

### IsValid
```csharp
public static bool IsValid()
```
Determines whether the current background worker configuration passes all validation rules.
*   **Purpose**: To provide a boolean flag indicating the health of the worker configuration.
*   **Parameters**: None.
*   **Return Value**: `true` if the configuration is valid; otherwise, `false`.
*   **Throws**: None.

### EnsureValid
```csharp
public static void EnsureValid()
```
Validates the background worker configuration and throws an exception if any errors are found.
*   **Purpose**: To enforce strict validation, typically used during application initialization to prevent starting with an invalid setup.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Throws**: Throws an `InvalidOperationException` (or a specific validation exception defined in the core library) containing a concatenated message of all validation errors if `IsValid` returns `false`.

## Usage

### Example 1: Startup Validation
Use `EnsureValid` during application startup to prevent the host from running if the background worker is misconfigured.

```csharp
using DotNetRealtimePipeline.Validation;

public class Startup
{
    public void Configure()
    {
        // Halt application startup if background worker configuration is invalid
        BackgroundProcessingWorkerValidation.EnsureValid();
        
        // Proceed with host initialization...
    }
}
```

### Example 2: Diagnostic Check
Use `Validate` and `IsValid` to inspect configuration health at runtime or within a diagnostic endpoint without interrupting service flow.

```csharp
using System;
using System.Linq;
using DotNetRealtimePipeline.Validation;

public class DiagnosticService
{
    public string GetWorkerStatus()
    {
        if (BackgroundProcessingWorkerValidation.IsValid())
        {
            return "Background worker configuration is valid.";
        }

        var errors = BackgroundProcessingWorkerValidation.Validate();
        return $"Configuration invalid: {string.Join("; ", errors)}";
    }
}
```

## Notes

*   **Thread Safety**: As all members are static and operate on immutable return types (`IReadOnlyList`, `bool`, `void`), the class is inherently thread-safe for read operations. It is safe to call these methods concurrently from multiple threads.
*   **Side Effects**: The `Validate` and `IsValid` methods are pure functions regarding the application state; they do not modify configuration or trigger side effects. `EnsureValid` will terminate the current execution flow via exception if validation fails.
*   **Empty Results**: When `Validate` returns an empty list, it guarantees that `IsValid` will return `true` and `EnsureValid` will complete without throwing.
*   **Error Aggregation**: The `Validate` method attempts to collect all possible errors in a single pass rather than failing on the first encountered issue, allowing for comprehensive debugging of complex configuration states.
