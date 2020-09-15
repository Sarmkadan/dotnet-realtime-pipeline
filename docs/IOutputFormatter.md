# IOutputFormatter

Defines a contract for formatting output data in a real-time pipeline. Implementations convert values of a specified type into their string representation, optionally applying formatting rules. The interface provides both synchronous and asynchronous formatting methods, as well as a static factory method for creating default instances.

## API

### `Format<T>` (multiple overloads)

Synchronously formats a value of type `T` into a string. The interface exposes several overloads of this method; each overload accepts a value of type `T` and may accept additional parameters (such as format strings, culture info, or options). The exact parameter signatures vary by overload.

- **Type parameters**: `T` – The type of the value to format.
- **Returns**: `string` – The formatted representation of the input value.
- **Throws**: `ArgumentNullException` if the input value is `null` and the overload does not accept `null`; `FormatException` if the formatting rules cannot be applied to the given value.

### `FormatAsync<T>` (multiple overloads)

Asynchronously formats a value of type `T` into a string. Each overload corresponds to a synchronous `Format<T>` overload and accepts the same parameters, but returns a `Task<string>`.

- **Type parameters**: `T` – The type of the value to format.
- **Returns**: `Task<string>` – A task that represents the asynchronous formatting operation. The result is the formatted string.
- **Throws**: Same as the synchronous counterpart, but exceptions are captured in the returned task.

### `static IOutputFormatter Create`

Creates a default instance of `IOutputFormatter`. The returned formatter uses standard formatting rules (typically the invariant culture and the default `ToString` behavior for the given type).

- **Returns**: `IOutputFormatter` – A new formatter instance.
- **Throws**: None.

## Usage

### Example 1: Synchronous formatting of a numeric value

```csharp
using DotNetRealtimePipeline;

IOutputFormatter formatter = IOutputFormatter.Create();

int value = 42;
string formatted = formatter.Format(value);
Console.WriteLine(formatted); // Output: "42"
```

### Example 2: Asynchronous formatting with custom options (using an overload that accepts a format string)

```csharp
using DotNetRealtimePipeline;
using System.Globalization;

IOutputFormatter formatter = IOutputFormatter.Create();

double pi = 3.1415926535;
string formatted = await formatter.FormatAsync(pi, "F2", CultureInfo.InvariantCulture);
Console.WriteLine(formatted); // Output: "3.14"
```

## Notes

- **Thread safety**: Instances of `IOutputFormatter` are not guaranteed to be thread-safe. If the same instance is used concurrently from multiple threads, external synchronization is required. The static `Create` method is safe to call from any thread.
- **Null handling**: Some overloads may accept `null` as a valid input (e.g., for nullable value types or reference types). Others may throw `ArgumentNullException`. Consult the specific overload’s documentation or IntelliSense for exact behavior.
- **Async overhead**: Use the synchronous `Format<T>` overloads when the formatting operation is lightweight and does not involve I/O. The `FormatAsync<T>` overloads are intended for implementations that perform asynchronous work (e.g., network calls or file writes) and should not be used purely for parallelism.
- **Overload resolution**: Because the interface defines multiple overloads with the same name, the compiler selects the correct one based on the provided arguments. Ensure that the argument types match exactly to avoid ambiguity.
