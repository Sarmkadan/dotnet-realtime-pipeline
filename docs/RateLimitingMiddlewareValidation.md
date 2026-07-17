# RateLimitingMiddlewareValidation

`RateLimitingMiddlewareValidation` is a static utility type that provides centralized validation logic for the rate‑limiting middleware pipeline. It exposes methods to check configuration parameters, assert validity, and collect diagnostic messages, ensuring that the middleware is always constructed with consistent and permissible settings before being activated in the request pipeline.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate { get; }
public static IReadOnlyList<string> Validate { get; }
```

**Purpose**  
Returns a read‑only list of validation error messages accumulated during the most recent validation pass. The property is populated by calling one of the `ValidateParameters` or `EnsureValid` overloads.

**Return Value**  
`IReadOnlyList<string>` – A collection of human‑readable error descriptions. An empty list indicates that no validation failures were detected.

**Remarks**  
The property is a snapshot of the last validation result. It does not perform validation itself; it merely exposes the outcome of the most recent call to a validation method.

---

### IsValid

```csharp
public static bool IsValid { get; }
```

**Purpose**  
Indicates whether the current rate‑limiting middleware configuration is valid, based on the most recent validation pass.

**Return Value**  
`true` if the last validation produced no error messages; otherwise `false`.

**Remarks**  
This property is a convenience wrapper that returns `Validate.Count == 0`. It reflects the state after the last call to `ValidateParameters` or `EnsureValid`.

---

### EnsureValid

```csharp
public static void EnsureValid();
public static void EnsureValid();
```

**Purpose**  
Performs a full validation of the current middleware configuration and throws an exception if any validation errors are present.

**Exceptions**  
`InvalidOperationException` – Thrown when the validation errors list is not empty after the validation pass. The exception message typically includes the concatenated error messages.

**Remarks**  
This method is a guard clause intended to be called at the point where the middleware is about to be used. It does not return a value; it either succeeds silently or throws.

---

### ValidateParameters

```csharp
public static IReadOnlyList<string> ValidateParameters(/* parameters inferred from context */);
public static IReadOnlyList<string> ValidateParameters(/* parameters inferred from context */);
```

**Purpose**  
Runs the validation logic against the provided rate‑limiting parameters and returns the resulting error messages. This is the primary entry point for populating the `Validate` property.

**Parameters**  
The overloads accept the middleware configuration parameters (e.g., rate‑limit counts, window durations, queue limits). The exact signatures are determined by the middleware’s configuration model.

**Return Value**  
`IReadOnlyList<string>` – The list of validation error messages. An empty list indicates a valid configuration.

**Remarks**  
Calling this method updates the internal state that is exposed through `Validate` and `IsValid`. It does not throw on invalid parameters; instead, it returns the errors for inspection.

---

## Usage

### Example 1: Validating configuration before registration

```csharp
var options = new RateLimitingMiddlewareOptions
{
    PermitLimit = 100,
    Window = TimeSpan.FromMinutes(1),
    QueueLimit = 0
};

// Run validation and inspect errors
var errors = RateLimitingMiddlewareValidation.ValidateParameters(options);
if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
    return;
}

// Configuration is valid; proceed with registration
app.UseRateLimitingMiddleware(options);
```

### Example 2: Using EnsureValid as a guard clause

```csharp
public void ConfigureMiddleware(RateLimitingMiddlewareOptions options)
{
    // Validate and throw immediately if invalid
    RateLimitingMiddlewareValidation.ValidateParameters(options);
    RateLimitingMiddlewareValidation.EnsureValid();

    // At this point the configuration is guaranteed to be valid
    app.UseRateLimitingMiddleware(options);
}
```

---

## Notes

- **Statefulness** – The `Validate` and `IsValid` properties reflect the result of the *last* call to `ValidateParameters` or `EnsureValid`. They are not automatically updated when the underlying configuration changes; a new validation call must be made explicitly.
- **Thread safety** – The class is static and its internal state may be mutated by any thread calling `ValidateParameters` or `EnsureValid`. In multi‑threaded scenarios, the values returned by `Validate` and `IsValid` are only meaningful immediately after a validation call on the same thread. If multiple threads validate different configurations concurrently, the last writer wins, and the exposed state may not correspond to the configuration a particular thread just validated.
- **Error accumulation** – `ValidateParameters` replaces the entire error list; it does not append to previous results. Each call provides a fresh snapshot.
- **Exception details** – `EnsureValid` throws `InvalidOperationException` with a message that aggregates all current error messages. If the error list is empty, no exception is thrown.
- **Overloads** – The two overloads of `ValidateParameters` and the two overloads of `EnsureValid` exist to accommodate different parameter sets (e.g., individual parameters versus an options object). They share the same underlying validation logic and update the same internal state.
