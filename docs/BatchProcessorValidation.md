# BatchProcessorValidation

Provides centralized validation logic for batch processing configurations, ensuring that batch size, concurrency limits, and timeout values fall within acceptable operational boundaries. This type exposes both boolean checks and assertion-style methods, allowing callers to either query validity or enforce constraints with immediate exceptions.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(int batchSize, int maxConcurrency, TimeSpan timeout)
public static IReadOnlyList<string> Validate(int batchSize, int maxConcurrency, TimeSpan timeout, int maxRetries)
```

Validates the supplied batch processing parameters and returns a collection of human-readable error messages describing every constraint violation found. An empty list indicates full validity.

**Parameters**
- `batchSize` — The number of items to process in a single batch. Must be greater than zero and not exceed the system-defined maximum.
- `maxConcurrency` — The maximum number of batches allowed to execute concurrently. Must be greater than zero.
- `timeout` — The maximum duration allowed for a single batch operation. Must be positive and not exceed the maximum allowed timeout.
- `maxRetries` — The number of retry attempts permitted per batch. Must be zero or greater and not exceed the maximum allowed retries.

**Return Value**
An `IReadOnlyList<string>` containing zero or more error descriptions. An empty list means the configuration is valid.

**Exceptions**
None. This method never throws.

---

### IsValid

```csharp
public static bool IsValid(int batchSize, int maxConcurrency, TimeSpan timeout)
public static bool IsValid(int batchSize, int maxConcurrency, TimeSpan timeout, int maxRetries)
```

Determines whether the given batch processing parameters satisfy all validation constraints.

**Parameters**
Same as the corresponding `Validate` overloads.

**Return Value**
`true` if all parameters are within acceptable ranges; otherwise `false`.

**Exceptions**
None thrown.

---

### EnsureValid

```csharp
public static void EnsureValid(int batchSize, int maxConcurrency, TimeSpan timeout)
public static void EnsureValid(int batchSize, int maxConcurrency, TimeSpan timeout, int maxRetries)
```

Enforces that the supplied batch processing parameters are valid by throwing an exception if any constraint is violated.

**Parameters**
Same as the corresponding `Validate` overloads.

**Return Value**
None (void).

**Exceptions**
- `ArgumentException` — Thrown when one or more parameters fall outside their permitted ranges. The exception message aggregates all validation failures.

---

## Usage

**Example 1: Guarding configuration at startup**

```csharp
var batchSize = 50;
var maxConcurrency = 4;
var timeout = TimeSpan.FromSeconds(30);

if (!BatchProcessorValidation.IsValid(batchSize, maxConcurrency, timeout))
{
    var errors = BatchProcessorValidation.Validate(batchSize, maxConcurrency, timeout);
    throw new InvalidOperationException(
        $"Invalid batch processor configuration: {string.Join("; ", errors)}");
}

// Proceed with pipeline initialization
var processor = new BatchProcessor(batchSize, maxConcurrency, timeout);
```

**Example 2: Fail-fast with EnsureValid in a factory method**

```csharp
public BatchProcessor CreateProcessor(int batchSize, int maxConcurrency, TimeSpan timeout, int maxRetries)
{
    BatchProcessorValidation.EnsureValid(batchSize, maxConcurrency, timeout, maxRetries);

    return new BatchProcessor(
        batchSize,
        maxConcurrency,
        timeout,
        maxRetries);
}
```

## Notes

- All members are static and thread-safe; they operate purely on their input arguments without accessing shared state.
- The overloads accepting `maxRetries` treat zero as a valid value (no retries), while negative values produce a validation error.
- `Validate` always returns an empty collection for valid inputs rather than `null`; callers can safely iterate over the result without null checks.
- `EnsureValid` throws `ArgumentException` (not a custom exception type), so callers should catch that specific type if graceful handling is required.
- Edge cases such as `TimeSpan.Zero` for the timeout or a `batchSize` of `int.MaxValue` are rejected by the internal constraint ranges and will appear in the error list or trigger an exception.
- The maximum allowed values for `batchSize`, `maxConcurrency`, `timeout`, and `maxRetries` are implementation-defined constants within the type and may vary between releases; consult the project's configuration documentation for exact limits.
