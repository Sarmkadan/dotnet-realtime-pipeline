# SerializationHelperJsonExtensions

Provides extension methods for JSON serialization and deserialization of the core data types used in the real‑time pipeline: `DataPoint`, `ProcessingResult`, and `MetricAggregation`. The class includes overloads for converting single instances or collections to JSON strings, as well as safe and unsafe deserialization helpers.

## API

### `ToJson(this DataPoint dataPoint)`
Serializes a single `DataPoint` to its JSON representation.

- **Parameters**  
  `this DataPoint dataPoint` – The data point to serialize.
- **Returns**  
  `string` – A JSON string representing the data point.
- **Throws**  
  `System.Text.Json.JsonException` if serialization fails (e.g., circular reference or invalid value).

### `ToJson(this IEnumerable<DataPoint> dataPoints)`
Serializes a collection of `DataPoint` instances to a JSON array.

- **Parameters**  
  `this IEnumerable<DataPoint> dataPoints` – The data points to serialize.
- **Returns**  
  `string` – A JSON array string.
- **Throws**  
  `System.Text.Json.JsonException` if serialization fails.

### `FromJsonToDataPoint(string json)`
Deserializes a JSON string into a `DataPoint`.

- **Parameters**  
  `string json` – The JSON string to deserialize.
- **Returns**  
  `DataPoint` – The deserialized data point.
- **Throws**  
  `System.Text.Json.JsonException` if the JSON is malformed or cannot be mapped to `DataPoint`.  
  `System.ArgumentNullException` if `json` is `null`.

### `TryFromJsonToDataPoint(string json, out DataPoint result)`
Attempts to deserialize a JSON string into a `DataPoint` without throwing an exception.

- **Parameters**  
  `string json` – The JSON string to deserialize.  
  `out DataPoint result` – When this method returns, contains the deserialized data point if successful, or the default value otherwise.
- **Returns**  
  `bool` – `true` if deserialization succeeded; otherwise `false`.
- **Throws**  
  None. Exceptions during deserialization are caught and the method returns `false`.

### `ToJson(this ProcessingResult processingResult)`
Serializes a single `ProcessingResult` to its JSON representation.

- **Parameters**  
  `this ProcessingResult processingResult` – The processing result to serialize.
- **Returns**  
  `string` – A JSON string.
- **Throws**  
  `System.Text.Json.JsonException` if serialization fails.

### `ToJson(this IEnumerable<ProcessingResult> processingResults)`
Serializes a collection of `ProcessingResult` instances to a JSON array.

- **Parameters**  
  `this IEnumerable<ProcessingResult> processingResults` – The processing results to serialize.
- **Returns**  
  `string` – A JSON array string.
- **Throws**  
  `System.Text.Json.JsonException` if serialization fails.

### `FromJsonToProcessingResult(string json)`
Deserializes a JSON string into a `ProcessingResult`.

- **Parameters**  
  `string json` – The JSON string to deserialize.
- **Returns**  
  `ProcessingResult` – The deserialized processing result.
- **Throws**  
  `System.Text.Json.JsonException` if the JSON is malformed or cannot be mapped to `ProcessingResult`.  
  `System.ArgumentNullException` if `json` is `null`.

### `TryFromJsonToProcessingResult(string json, out ProcessingResult result)`
Attempts to deserialize a JSON string into a `ProcessingResult` without throwing an exception.

- **Parameters**  
  `string json` – The JSON string to deserialize.  
  `out ProcessingResult result` – When this method returns, contains the deserialized result if successful, or the default value otherwise.
- **Returns**  
  `bool` – `true` if deserialization succeeded; otherwise `false`.
- **Throws**  
  None.

### `ToJson(this MetricAggregation metricAggregation)`
Serializes a single `MetricAggregation` to its JSON representation.

- **Parameters**  
  `this MetricAggregation metricAggregation` – The metric aggregation to serialize.
- **Returns**  
  `string` – A JSON string.
- **Throws**  
  `System.Text.Json.JsonException` if serialization fails.

### `ToJson(this IEnumerable<MetricAggregation> metricAggregations)`
Serializes a collection of `MetricAggregation` instances to a JSON array.

- **Parameters**  
  `this IEnumerable<MetricAggregation> metricAggregations` – The metric aggregations to serialize.
- **Returns**  
  `string` – A JSON array string.
- **Throws**  
  `System.Text.Json.JsonException` if serialization fails.

### `FromJsonToMetricAggregation(string json)`
Deserializes a JSON string into a `MetricAggregation`.

- **Parameters**  
  `string json` – The JSON string to deserialize.
- **Returns**  
  `MetricAggregation` – The deserialized metric aggregation.
- **Throws**  
  `System.Text.Json.JsonException` if the JSON is malformed or cannot be mapped to `MetricAggregation`.  
  `System.ArgumentNullException` if `json` is `null`.

### `TryFromJsonToMetricAggregation(string json, out MetricAggregation result)`
Attempts to deserialize a JSON string into a `MetricAggregation` without throwing an exception.

- **Parameters**  
  `string json` – The JSON string to deserialize.  
  `out MetricAggregation result` – When this method returns, contains the deserialized aggregation if successful, or the default value otherwise.
- **Returns**  
  `bool` – `true` if deserialization succeeded; otherwise `false`.
- **Throws**  
  None.

## Usage

### Serializing and deserializing a single DataPoint

```csharp
using DotNetRealtimePipeline;
using System.Text.Json;

var point = new DataPoint { Timestamp = DateTime.UtcNow, Value = 42.5 };

// Serialize to JSON
string json = point.ToJson();
Console.WriteLine(json); // {"Timestamp":"2025-03-28T12:00:00Z","Value":42.5}

// Deserialize back
DataPoint restored = SerializationHelperJsonExtensions.FromJsonToDataPoint(json);
Console.WriteLine(restored.Value); // 42.5
```

### Safe deserialization with TryFromJson

```csharp
string invalidJson = "{ invalid }";

if (SerializationHelperJsonExtensions.TryFromJsonToProcessingResult(invalidJson, out var result))
{
    Console.WriteLine($"Deserialized: {result.Status}");
}
else
{
    Console.WriteLine("Failed to deserialize – invalid JSON.");
}
```

## Notes

- All `ToJson` overloads use `System.Text.Json` with default settings (camelCase naming, case‑insensitive deserialization). Custom converters or options are not applied.
- The `TryFromJsonTo*` methods catch all exceptions thrown during deserialization (including `JsonException`, `ArgumentNullException`, and `InvalidOperationException`) and return `false`. They do not propagate exceptions.
- Passing `null` to any `FromJsonTo*` method throws `ArgumentNullException`. The `TryFromJsonTo*` methods treat `null` as a failure and return `false` without throwing.
- Empty strings (`""`) are considered invalid JSON and will cause deserialization to fail (throw or return `false`).
- Thread safety: All methods are static and do not modify any shared state. They are safe to call concurrently from multiple threads. However, the underlying `System.Text.Json` serializer is thread‑safe for read operations; no additional synchronization is required.
