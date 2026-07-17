# BackpressureContextJsonExtensions

Provides JSON serialization and deserialization support for `BackpressureContext` objects. The class combines static convenience methods (`ToJson`, `FromJson`, `TryFromJson`) with overridable instance methods that handle reading and writing individual components of a `BackpressureContext` (a queue of timestamps, a dictionary of counters, and a timestamp) during custom JSON conversion.

## API

### `public static string ToJson(BackpressureContext? context)`

Serializes a `BackpressureContext` instance to its JSON string representation.

- **Parameters**  
  `context` – The `BackpressureContext` to serialize. Can be `null`.
- **Returns**  
  A JSON string representing the `context`. If `context` is `null`, returns the JSON literal `null`.
- **Throws**  
  `JsonException` – if the serialization fails (e.g., due to invalid internal state).

### `public static BackpressureContext? FromJson(string json)`

Deserializes a JSON string into a `BackpressureContext` instance.

- **Parameters**  
  `json` – A JSON string representing a `BackpressureContext`. Must not be `null` or empty.
- **Returns**  
  A `BackpressureContext` instance if deserialization succeeds; `null` if the JSON value is `null`.
- **Throws**  
  `ArgumentNullException` – if `json` is `null`.  
  `JsonException` – if the JSON is malformed or cannot be mapped to a valid `BackpressureContext`.

### `public static bool TryFromJson(string json, [NotNullWhen(true)] out BackpressureContext? result)`

Attempts to deserialize a JSON string into a `BackpressureContext` without throwing exceptions.

- **Parameters**  
  `json` – A JSON string to deserialize.  
  `result` – When this method returns `true`, contains the deserialized `BackpressureContext`; otherwise, `null`.
- **Returns**  
  `true` if deserialization succeeded; `false` otherwise.
- **Throws**  
  `ArgumentNullException` – if `json` is `null`.

### `public override Queue<long> Read(...)` (overload)

Reads a `Queue<long>` from the JSON input. Typically used to deserialize the internal timestamp queue of a `BackpressureContext`.

- **Parameters** (inherited from base class)  
  `ref Utf8JsonReader reader` – the JSON reader positioned at the start of the queue.  
  `Type typeToConvert` – the target type (not used).  
  `JsonSerializerOptions options` – serialization options.
- **Returns**  
  A `Queue<long>` containing the deserialized timestamps.
- **Throws**  
  `JsonException` – if the JSON token is not an array or contains non‑integer values.

### `public override void Write(...)` (overload for `Queue<long>`)

Writes a `Queue<long>` to the JSON output. Used to serialize the timestamp queue of a `BackpressureContext`.

- **Parameters** (inherited from base class)  
  `Utf8JsonWriter writer` – the JSON writer.  
  `Queue<long> value` – the queue to serialize.  
  `JsonSerializerOptions options` – serialization options.
- **Throws**  
  `ArgumentNullException` – if `writer` or `value` is `null`.

### `public override Dictionary<string, long> Read(...)` (overload)

Reads a `Dictionary<string, long>` from the JSON input. Used to deserialize the counter dictionary of a `BackpressureContext`.

- **Parameters** (inherited from base class)  
  `ref Utf8JsonReader reader` – the JSON reader positioned at the start of the object.  
  `Type typeToConvert` – the target type.  
  `JsonSerializerOptions options` – serialization options.
- **Returns**  
  A `Dictionary<string, long>` containing the deserialized key‑value pairs.
- **Throws**  
  `JsonException` – if the JSON token is not an object or contains non‑string keys or non‑integer values.

### `public override void Write(...)` (overload for `Dictionary<string, long>`)

Writes a `Dictionary<string, long>` to the JSON output. Used to serialize the counter dictionary of a `BackpressureContext`.

- **Parameters** (inherited from base class)  
  `Utf8JsonWriter writer` – the JSON writer.  
  `Dictionary<string, long> value` – the dictionary to serialize.  
  `JsonSerializerOptions options` – serialization options.
- **Throws**  
  `ArgumentNullException` – if `writer` or `value` is `null`.

### `public override DateTime Read(...)` (overload)

Reads a `DateTime` from the JSON input. Used to deserialize a timestamp property of a `BackpressureContext`.

- **Parameters** (inherited from base class)  
  `ref Utf8JsonReader reader` – the JSON reader positioned at the date/time value.  
  `Type typeToConvert` – the target type.  
  `JsonSerializerOptions options` – serialization options.
- **Returns**  
  A `DateTime` parsed from the JSON string.
- **Throws**  
  `JsonException` – if the JSON token is not a string or cannot be parsed as a valid `DateTime`.

### `public override void Write(...)` (overload for `DateTime`)

Writes a `DateTime` to the JSON output. Used to serialize a timestamp property of a `BackpressureContext`.

- **Parameters** (inherited from base class)  
  `Utf8JsonWriter writer` – the JSON writer.  
  `DateTime value` – the date/time value to serialize.  
  `JsonSerializerOptions options` – serialization options.
- **Throws**  
  `ArgumentNullException` – if `writer` is `null`.

## Usage

### Example 1: Serialize and deserialize a `BackpressureContext` using static methods

```csharp
using System;
using System.Collections.Generic;
using DotNet.RealtimePipeline;

var context = new BackpressureContext
{
    Timestamps = new Queue<long>(new[] { 100L, 200L }),
    Counters = new Dictionary<string, long> { { "retries", 3 } },
    LastUpdated = DateTime.UtcNow
};

string json = BackpressureContextJsonExtensions.ToJson(context);
Console.WriteLine(json);

if (BackpressureContextJsonExtensions.TryFromJson(json, out var restored))
{
    Console.WriteLine($"Restored timestamps: {restored.Timestamps.Count}");
}
```

### Example 2: Custom serialization using the override methods (inside a custom converter)

```csharp
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNet.RealtimePipeline;

public class BackpressureContextConverter : JsonConverter<BackpressureContext>
{
    private readonly BackpressureContextJsonExtensions _ext = new();

    public override BackpressureContext Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Assume the JSON structure is known; read each component using the extension's Read methods.
        var timestamps = _ext.Read(ref reader, typeof(Queue<long>), options);
        var counters = _ext.Read(ref reader, typeof(Dictionary<string, long>), options);
        var lastUpdated = _ext.Read(ref reader, typeof(DateTime), options);
        return new BackpressureContext { Timestamps = timestamps, Counters = counters, LastUpdated = lastUpdated };
    }

    public override void Write(Utf8JsonWriter writer, BackpressureContext value, JsonSerializerOptions options)
    {
        _ext.Write(writer, value.Timestamps, options);
        _ext.Write(writer, value.Counters, options);
        _ext.Write(writer, value.LastUpdated, options);
    }
}
```

## Notes

- **Thread safety**  
  The static methods `ToJson`, `FromJson`, and `TryFromJson` are thread‑safe. The instance `Read` and `Write` methods are **not** thread‑safe; they rely on the state of the underlying reader/writer and should not be called concurrently on the same instance.
- **Null handling**  
  `ToJson` accepts a `null` context and produces the JSON literal `null`. `FromJson` returns `null` when the input JSON is `null`. `TryFromJson` sets `result` to `null` on failure.
- **Exception behavior**  
  `FromJson` throws on malformed JSON; `TryFromJson` returns `false` instead. Both throw `ArgumentNullException` if the input string is `null`.
- **Overload resolution**  
  The multiple `Read`/`Write` overloads are distinguished by the type of the value being read or written. The base class (typically `JsonConverter<T>`) dispatches to the correct overload based on the `typeToConvert` parameter.
- **Edge cases**  
  - An empty `Queue<long>` is serialized as an empty JSON array.  
  - An empty `Dictionary<string, long>` is serialized as an empty JSON object.  
  - `DateTime` values are serialized using the default `System.Text.Json` format (ISO 8601).  
  - If the JSON input contains unexpected tokens, the `Read` overloads throw `JsonException`.
