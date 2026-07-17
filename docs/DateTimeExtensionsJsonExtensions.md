# DateTimeExtensionsJsonExtensions

Provides extension methods for serializing and deserializing `DateTime`, `DateTimeOffset`, and Unix millisecond values to and from JSON strings. These utilities are designed for use in real-time data pipelines where consistent date/time representation across JSON payloads is required.

## API

### `ToJson` (DateTime overload)

```csharp
public static string ToJson(this DateTime value)
```

Converts a `DateTime` value to its JSON string representation (ISO 8601 format).

- **Parameters**  
  `value` – The `DateTime` to serialize.
- **Returns**  
  A JSON string representing the date and time.
- **Throws**  
  Does not throw; the conversion is always valid for any `DateTime` value.

### `ToJson` (DateTimeOffset overload)

```csharp
public static string ToJson(this DateTimeOffset value)
```

Converts a `DateTimeOffset` value to its JSON string representation (ISO 8601 format with offset).

- **Parameters**  
  `value` – The `DateTimeOffset` to serialize.
- **Returns**  
  A JSON string representing the date, time, and offset.
- **Throws**  
  Does not throw.

### `ToJson` (Unix milliseconds overload)

```csharp
public static string ToJson(this long? unixMilliseconds)
```

Converts a nullable Unix millisecond timestamp to its JSON representation. A non-null value is serialized as a JSON number; `null` is serialized as `null`.

- **Parameters**  
  `unixMilliseconds` – The number of milliseconds since Unix epoch (1970-01-01T00:00:00Z), or `null`.
- **Returns**  
  A JSON string containing the number or `null`.
- **Throws**  
  Does not throw.

### `FromJsonToDateTime`

```csharp
public static DateTime? FromJsonToDateTime(this string json)
```

Deserializes a JSON string to a nullable `DateTime`. Expects an ISO 8601 formatted date/time string.

- **Parameters**  
  `json` – The JSON string to parse. Must not be `null`.
- **Returns**  
  A `DateTime?` – the parsed value, or `null` if the JSON string is `"null"`.
- **Throws**  
  `ArgumentNullException` if `json` is `null`.  
  `FormatException` if the string is not a valid ISO 8601 representation and is not `"null"`.

### `FromJsonToDateTimeOffset`

```csharp
public static DateTimeOffset? FromJsonToDateTimeOffset(this string json)
```

Deserializes a JSON string to a nullable `DateTimeOffset`. Expects an ISO 8601 formatted string that may include a time zone offset.

- **Parameters**  
  `json` – The JSON string to parse. Must not be `null`.
- **Returns**  
  A `DateTimeOffset?` – the parsed value, or `null` if the JSON string is `"null"`.
- **Throws**  
  `ArgumentNullException` if `json` is `null`.  
  `FormatException` if the string is not a valid ISO 8601 representation and is not `"null"`.

### `TryFromJsonToDateTime`

```csharp
public static bool TryFromJsonToDateTime(this string json, out DateTime? result)
```

Attempts to deserialize a JSON string to a nullable `DateTime` without throwing exceptions.

- **Parameters**  
  `json` – The JSON string to attempt to parse.  
  `result` – When this method returns, contains the parsed `DateTime?` value, or `null` if parsing failed or the input was `"null"`.
- **Returns**  
  `true` if the string was successfully parsed (including `"null"`); `false` if the string is not valid JSON or not a valid date/time representation.
- **Throws**  
  Does not throw.

### `TryFromJsonToDateTimeOffset`

```csharp
public static bool TryFromJsonToDateTimeOffset(this string json, out DateTimeOffset? result)
```

Attempts to deserialize a JSON string to a nullable `DateTimeOffset` without throwing exceptions.

- **Parameters**  
  `json` – The JSON string to attempt to parse.  
  `result` – When this method returns, contains the parsed `DateTimeOffset?` value, or `null` if parsing failed or the input was `"null"`.
- **Returns**  
  `true` if the string was successfully parsed (including `"null"`); `false` otherwise.
- **Throws**  
  Does not throw.

### `FromJsonToUnixMilliseconds`

```csharp
public static long? FromJsonToUnixMilliseconds(this string json)
```

Deserializes a JSON string to a nullable Unix millisecond timestamp. Expects a JSON number (integer) or `"null"`.

- **Parameters**  
  `json` – The JSON string to parse. Must not be `null`.
- **Returns**  
  A `long?` – the number of milliseconds since Unix epoch, or `null` if the JSON string is `"null"`.
- **Throws**  
  `ArgumentNullException` if `json` is `null`.  
  `FormatException` if the string is not a valid integer number and is not `"null"`.  
  `OverflowException` if the parsed number is outside the range of `Int64`.

### `TryFromJsonToUnixMilliseconds`

```csharp
public static bool TryFromJsonToUnixMilliseconds(this string json, out long? result)
```

Attempts to deserialize a JSON string to a nullable Unix millisecond timestamp without throwing exceptions.

- **Parameters**  
  `json` – The JSON string to attempt to parse.  
  `result` – When this method returns, contains the parsed `long?` value, or `null` if parsing failed or the input was `"null"`.
- **Returns**  
  `true` if the string was successfully parsed (including `"null"`); `false` otherwise.
- **Throws**  
  Does not throw.

## Usage

### Example 1: Serializing and deserializing DateTime values

```csharp
using System;
using DotNetRealtimePipeline.Extensions; // assumed namespace

DateTime now = DateTime.UtcNow;
string json = now.ToJson(); // e.g., "2025-03-20T14:30:00.0000000Z"

DateTime? parsed = json.FromJsonToDateTime();
Console.WriteLine(parsed == now); // True

// Handling null
string nullJson = "null";
DateTime? nullResult = nullJson.FromJsonToDateTime();
Console.WriteLine(nullResult == null); // True
```

### Example 2: Working with Unix millisecond timestamps

```csharp
using System;
using DotNetRealtimePipeline.Extensions;

long? timestamp = 1742476200000; // 2025-03-20T14:30:00Z
string json = timestamp.ToJson(); // "1742476200000"

long? parsed = json.FromJsonToUnixMilliseconds();
Console.WriteLine(parsed == timestamp); // True

// TryParse pattern for safe deserialization
string invalidJson = "\"not a number\"";
if (invalidJson.TryFromJsonToUnixMilliseconds(out long? safeResult))
{
    // Not reached
}
else
{
    Console.WriteLine("Parsing failed, safeResult is null");
}
```

## Notes

- All methods are static and thread-safe. They do not modify any shared state and can be called concurrently from multiple threads.
- The `ToJson` overloads always produce valid JSON strings. The `FromJson*` methods expect the JSON value to be a string literal (including quotes) or the literal `null`. Raw numbers are expected for Unix millisecond overloads.
- When deserializing, the `Try*` variants are preferred in scenarios where malformed input is expected, as they avoid exception overhead.
- `DateTime` values are serialized using the ISO 8601 format with the `Z` suffix for UTC kind. `DateTimeOffset` values include the offset (e.g., `2025-03-20T14:30:00+00:00`). The `FromJson*` methods accept any valid ISO 8601 string, including those with or without offset.
- For Unix millisecond conversions, the value is treated as a 64-bit integer. Negative values (before epoch) are supported, but values outside the `long` range will cause an `OverflowException` in the non-try overload.
- Passing `null` as the `json` argument to any `FromJson*` method (except the `Try*` variants) will throw `ArgumentNullException`. The `Try*` methods treat a `null` input as a parse failure and return `false`.
