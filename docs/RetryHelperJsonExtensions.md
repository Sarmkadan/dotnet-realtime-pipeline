# RetryHelperJsonExtensions

Provides JSON serialization and deserialization extensions for `RetryHelper`, `RetryPolicy`, and `RetryStatistics` types, enabling easy conversion to and from JSON strings for storage or transmission.

## API

### `ToJson(RetryHelper retryHelper)`
Serializes a `RetryHelper` instance to a JSON string.

- **Parameters**
  - `retryHelper`: The `RetryHelper` instance to serialize.
- **Return value**
  - A JSON string representation of the `RetryHelper`.
- **Exceptions**
  - Throws `ArgumentNullException` if `retryHelper` is `null`.

---

### `FromJson(string json)`
Deserializes a JSON string into a `RetryHelper` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Return value**
  - A `RetryHelper` instance if deserialization succeeds; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `json` is `null`.

---

### `TryFromJson(string json, out RetryHelper? retryHelper)`
Attempts to deserialize a JSON string into a `RetryHelper` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `retryHelper`: Output parameter receiving the deserialized `RetryHelper` if successful.
- **Return value**
  - `true` if deserialization succeeds; otherwise, `false`.

---

### `ToJson(RetryPolicy retryPolicy)`
Serializes a `RetryPolicy` instance to a JSON string.

- **Parameters**
  - `retryPolicy`: The `RetryPolicy` instance to serialize.
- **Return value**
  - A JSON string representation of the `RetryPolicy`.
- **Exceptions**
  - Throws `ArgumentNullException` if `retryPolicy` is `null`.

---
### `FromJsonPolicy(string json)`
Deserializes a JSON string into a `RetryPolicy` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Return value**
  - A `RetryPolicy` instance if deserialization succeeds; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `json` is `null`.

---
### `TryFromJsonPolicy(string json, out RetryPolicy? retryPolicy)`
Attempts to deserialize a JSON string into a `RetryPolicy` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `retryPolicy`: Output parameter receiving the deserialized `RetryPolicy` if successful.
- **Return value**
  - `true` if deserialization succeeds; otherwise, `false`.

---
### `ToJson(RetryStatistics retryStatistics)`
Serializes a `RetryStatistics` instance to a JSON string.

- **Parameters**
  - `retryStatistics`: The `RetryStatistics` instance to serialize.
- **Return value**
  - A JSON string representation of the `RetryStatistics`.
- **Exceptions**
  - Throws `ArgumentNullException` if `retryStatistics` is `null`.

---
### `FromJsonStatistics(string json)`
Deserializes a JSON string into a `RetryStatistics` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Return value**
  - A `RetryStatistics` instance if deserialization succeeds; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `json` is `null`.

---
### `TryFromJsonStatistics(string json, out RetryStatistics? retryStatistics)`
Attempts to deserialize a JSON string into a `RetryStatistics` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `retryStatistics`: Output parameter receiving the deserialized `RetryStatistics` if successful.
- **Return value**
  - `true` if deserialization succeeds; otherwise, `false`.

## Usage

```csharp
// Example 1: Serialize and deserialize RetryHelper
var retryHelper = new RetryHelper(
    maxRetryCount: 3,
    delay: TimeSpan.FromSeconds(1),
    backoffFactor: 2.0);

string json = RetryHelperJsonExtensions.ToJson(retryHelper);
RetryHelper? deserialized = RetryHelperJsonExtensions.FromJson(json);

if (deserialized != null)
{
    Console.WriteLine($"Deserialized RetryHelper: MaxRetries={deserialized.MaxRetryCount}");
}

// Example 2: Use TryFromJson for safe deserialization
if (RetryHelperJsonExtensions.TryFromJson(json, out var safeDeserialized))
{
    Console.WriteLine("Successfully deserialized RetryHelper.");
}
```

## Notes

- **Thread Safety**: All methods are thread-safe and may be called concurrently.
- **Null Handling**: Methods throwing `ArgumentNullException` will fail fast if null inputs are provided.
- **Performance**: Serialization and deserialization involve JSON parsing; avoid frequent calls in performance-critical paths.
- **Compatibility**: JSON format must match the expected schema for deserialization to succeed; invalid JSON will result in `null` or `false` return values.
