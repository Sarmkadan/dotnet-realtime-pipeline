# ExportServiceValidation

Provides validation logic for export service configurations and runtime state within the pipeline. This static utility class offers methods to check the validity of export-related parameters, determine whether a given export configuration is in a valid state, and enforce validity through guard-style assertions that throw when conditions are not met.

## API

### `Validate` (overload 1)

```csharp
public static IReadOnlyList<string> Validate(ExportServiceConfiguration configuration)
```

Validates a complete export service configuration and returns all discovered issues.

**Parameters:**
- `configuration` — The `ExportServiceConfiguration` instance to inspect.

**Return value:**
A read-only list of error messages. An empty list indicates a valid configuration.

**Exceptions:**
None. All validation failures are returned as strings rather than thrown.

---

### `Validate` (overload 2)

```csharp
public static IReadOnlyList<string> Validate(ExportServiceConfiguration configuration, PipelineContext context)
```

Validates an export service configuration against a specific pipeline context, checking for compatibility between the export definition and the runtime environment.

**Parameters:**
- `configuration` — The `ExportServiceConfiguration` to validate.
- `context` — The `PipelineContext` providing runtime state and metadata.

**Return value:**
A read-only list of error messages. An empty list indicates the configuration is valid within the given context.

**Exceptions:**
None. All failures are returned as strings.

---

### `IsValid` (overload 1)

```csharp
public static bool IsValid(ExportService)
```

Determines whether an export service instance is in a valid operational state.

**Parameters:**
- `exportService` — The `ExportService` instance to check.

**Return value:**
`true` if the service is valid and ready for use; otherwise `false`.

---

### `IsValid` (overload 2)

```csharp
public static bool IsValid(ExportServiceConfiguration)
```

Checks whether a standalone export configuration is structurally valid without requiring a live service instance.

**Parameters:**
- `configuration` — The `ExportServiceConfiguration` to evaluate.

**Return value:**
`true` if the configuration passes all structural checks; otherwise `false`.

---

### `EnsureValid` (overload 1)

```csharp
public static void EnsureValid(ExportService)
```

Asserts that an export service is in a valid state, throwing an exception if it is not.

**Parameters:**
- `exportService` — The `ExportService` instance to validate.

**Exceptions:**
Throws an appropriate exception (e.g., `InvalidOperationException` or `ArgumentException`) when the service is not valid.

---

### `EnsureValid` (overload 2)

```csharp
public static void EnsureValid(ExportServiceConfiguration)
```

Asserts that an export service configuration is structurally valid, throwing an exception if any rules are violated.

**Parameters:**
- `configuration` — The `ExportServiceConfiguration` to validate.

**Exceptions:**
Throws an appropriate exception when the configuration fails validation.

## Usage

### Example 1: Validating a configuration before construction

```csharp
var config = new ExportServiceConfiguration
{
    Destination = "s3://bucket/prefix",
    Format = ExportFormat.Parquet,
    BatchSize = 1000
};

if (!ExportServiceValidation.IsValid(config))
{
    var errors = ExportServiceValidation.Validate(config);
    foreach (var error in errors)
    {
        Console.WriteLine($"Configuration error: {error}");
    }
    return;
}

var service = new ExportService(config);
ExportServiceValidation.EnsureValid(service);
service.Start();
```

### Example 2: Context-aware validation in a pipeline setup

```csharp
public void ConfigureExport(PipelineContext context, ExportServiceConfiguration config)
{
    var issues = ExportServiceValidation.Validate(config, context);

    if (issues.Any())
    {
        throw new PipelineConfigurationException(
            $"Export configuration is incompatible with the current pipeline context: " +
            string.Join("; ", issues));
    }

    var exportService = new ExportService(config);
    ExportServiceValidation.EnsureValid(exportService);
    context.RegisterService(exportService);
}
```

## Notes

- All methods are static and stateless; no instance state is maintained between calls.
- The `Validate` overloads never throw—they always return a list, which callers should check for emptiness to determine validity.
- The `EnsureValid` methods are designed for fail-fast scenarios where an invalid state should immediately halt execution.
- `IsValid` and `EnsureValid` for the same type are logically consistent: `EnsureValid` throws exactly when `IsValid` returns `false`.
- Thread safety is not a concern for this type since it holds no mutable state and all methods operate purely on their input parameters.
- When both a service instance and its configuration are available, prefer validating the configuration first to catch structural issues before runtime state is involved.
- The `Validate` overload that accepts a `PipelineContext` may check for environment-specific constraints such as connectivity, permissions, or feature availability that the configuration-only overload cannot assess.
