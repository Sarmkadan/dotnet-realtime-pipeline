# ApiEndpointHandlerValidation

`ApiEndpointHandlerValidation` is a static utility class providing methods for validating API endpoint handlers and their configurations. It ensures that endpoint definitions conform to expected patterns and constraints, returning validation results or throwing exceptions when invalid states are detected.

## API

### `Validate<T>(T instance)`

Validates an instance of type `T` against predefined endpoint handler rules.

- **Parameters**:  
  - `instance`: The object to validate.
- **Returns**:  
  - `IReadOnlyList<string>`: A list of validation error messages. An empty list indicates validity.
- **Exceptions**:  
  - Does not throw exceptions; returns errors as strings.

---

### `Validate(string endpointPath)`

Validates an endpoint path string for correctness and compliance with routing conventions.

- **Parameters**:  
  - `endpointPath`: The endpoint path to validate.
- **Returns**:  
  - `IReadOnlyList<string>`: A list of validation error messages. An empty list indicates validity.
- **Exceptions**:  
  - Does not throw exceptions; returns errors as strings.

---

### `Validate(object handler)`

Validates a handler object (non-generic) for structural and behavioral compliance.

- **Parameters**:  
  - `handler`: The handler object to validate.
- **Returns**:  
  - `IReadOnlyList<string>`: A list of validation error messages. An empty list indicates validity.
- **Exceptions**:  
  - Does not throw exceptions; returns errors as strings.

---

### `IsValid<T>(T instance)`

Checks whether an instance of type `T` is valid according to endpoint handler rules.

- **Parameters**:  
  - `instance`: The object to validate.
- **Returns**:  
  - `bool`: `true` if valid, `false` otherwise.
- **Exceptions**:  
  - Does not throw exceptions.

---

### `IsValid(string endpointPath)`

Checks whether an endpoint path string is valid.

- **Parameters**:  
  - `endpointPath`: The endpoint path to validate.
- **Returns**:  
  - `bool`: `true` if valid, `false` otherwise.
- **Exceptions**:  
  - Does not throw exceptions.

---

### `IsValid(object handler)`

Checks whether a handler object is valid.

- **Parameters**:  
  - `handler`: The handler object to validate.
- **Returns**:  
  - `bool`: `true` if valid, `false` otherwise.
- **Exceptions**:  
  - Does not throw exceptions.

---

### `EnsureValid<T>(T instance)`

Validates an instance of type `T` and throws an exception if invalid.

- **Parameters**:  
  - `instance`: The object to validate.
- **Returns**:  
  - `void`
- **Exceptions**:  
  - Throws `InvalidOperationException` if validation fails.

---

### `EnsureValid(string endpointPath)`

Validates an endpoint path and throws an exception if invalid.

- **Parameters**:  
  - `endpointPath`: The endpoint path to validate.
- **Returns**:  
  - `void`
- **Exceptions**:  
  - Throws `InvalidOperationException` if validation fails.

---

### `EnsureValid(object handler)`

Validates a handler object and throws an exception if invalid.

- **Parameters**:  
  - `handler`: The handler object to validate.
- **Returns**:  
  - `void`
- **Exceptions**:  
  - Throws `InvalidOperationException` if validation fails.

---

## Usage

### Example 1: Validating a Handler Instance

```csharp
public class MyEndpointHandler
{
    public string Path { get; set; }
    public string Method { get; set; }
}

var handler = new MyEndpointHandler { Path = "/api/data", Method = "GET" };
var errors = ApiEndpointHandlerValidation.Validate(handler);

if (errors.Count > 0)
{
    Console.WriteLine($"Validation errors: {string.Join(", ", errors)}");
}
else
{
    Console.WriteLine("Handler is valid.");
}
```

### Example 2: Ensuring Endpoint Path Validity

```csharp
string endpointPath = "/api/invalid{path";

try
{
    ApiEndpointHandlerValidation.EnsureValid(endpointPath);
    Console.WriteLine("Endpoint path is valid.");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

---

## Notes

- All methods are thread-safe as they are static and do not rely on mutable shared state.
- Null or empty inputs may result in validation errors; callers should ensure inputs are properly initialized.
- `EnsureValid` methods throw `InvalidOperationException` with aggregated error messages when validation fails.
- Generic type constraints on `T` are not explicitly defined in the public API; validation logic may vary based on internal rules.
