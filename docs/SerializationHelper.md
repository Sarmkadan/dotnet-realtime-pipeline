# SerializationHelper

Utility class providing static methods for serializing and deserializing `DataPoint` objects and collections to/from JSON, CSV, and file formats, along with Unix timestamp and ISO 8601 conversion helpers.

## API

### `public static string ToJson(DataPoint dataPoint)`
Converts a single `DataPoint` instance to its JSON string representation.

- **Parameters**:
  - `dataPoint`: The `DataPoint` instance to serialize.
- **Returns**: A JSON string representing the `DataPoint`.
- **Throws**: `ArgumentNullException` if `dataPoint` is `null`.

---

### `public static DataPoint FromJson(string json)`
Deserializes a JSON string into a `DataPoint` instance.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Returns**: A `DataPoint` instance reconstructed from the JSON.
- **Throws**:
  - `ArgumentNullException` if `json` is `null`.
  - `JsonException` if the JSON is malformed or incompatible with `DataPoint`.

---

### `public static string ToJsonArray(IEnumerable<DataPoint> dataPoints)`
Converts an enumerable collection of `DataPoint` instances to a JSON array string.

- **Parameters**:
  - `dataPoints`: The collection of `DataPoint` instances to serialize.
- **Returns**: A JSON array string representing the collection.
- **Throws**:
  - `ArgumentNullException` if `dataPoints` is `null`.
  - `ArgumentException` if `dataPoints` contains a `null` element.

---
### `public static List<DataPoint> FromJsonArray(string json)`
Deserializes a JSON array string into a `List<DataPoint>`.

- **Parameters**:
  - `json`: The JSON array string to deserialize.
- **Returns**: A `List<DataPoint>` reconstructed from the JSON array.
- **Throws**:
  - `ArgumentNullException` if `json` is `null`.
  - `JsonException` if the JSON is malformed or incompatible with `DataPoint`.

---
### `public static string ToCsv(DataPoint dataPoint)`
Converts a single `DataPoint` instance to a CSV-formatted string.

- **Parameters**:
  - `dataPoint`: The `DataPoint` instance to serialize.
- **Returns**: A CSV string representing the `DataPoint`.
- **Throws**: `ArgumentNullException` if `dataPoint` is `null`.

---
### `public static string ToCsvBatch(IEnumerable<DataPoint> dataPoints)`
Converts an enumerable collection of `DataPoint` instances to a multi-line CSV string.

- **Parameters**:
  - `dataPoints`: The collection of `DataPoint` instances to serialize.
- **Returns**: A CSV string with one line per `DataPoint`.
- **Throws**:
  - `ArgumentNullException` if `dataPoints` is `null`.
  - `ArgumentException` if `dataPoints` contains a `null` element.

---
### `public static string SerializeResults(IEnumerable<DataPoint> dataPoints)`
Serializes a collection of `DataPoint` instances into a structured JSON string with a `"results"` envelope.

- **Parameters**:
  - `dataPoints`: The collection of `DataPoint` instances to serialize.
- **Returns**: A JSON string with a top-level `"results"` array containing the serialized `DataPoint` objects.
- **Throws**:
  - `ArgumentNullException` if `dataPoints` is `null`.
  - `ArgumentException` if `dataPoints` contains a `null` element.

---
### `public static string SerializeMetrics(IEnumerable<DataPoint> dataPoints)`
Serializes a collection of `DataPoint` instances into a structured JSON string with a `"metrics"` envelope.

- **Parameters**:
  - `dataPoints`: The collection of `DataPoint` instances to serialize.
- **Returns**: A JSON string with a top-level `"metrics"` array containing the serialized `DataPoint` objects.
- **Throws**:
  - `ArgumentNullException` if `dataPoints` is `null`.
  - `ArgumentException` if `dataPoints` contains a `null` element.

---
### `public static async Task WriteToFileAsync(string path, string content)`
Writes a string to a file asynchronously.

- **Parameters**:
  - `path`: The file system path to write to.
  - `content`: The string content to write.
- **Returns**: A `Task` representing the asynchronous operation.
- **Throws**:
  - `ArgumentNullException` if `path` or `content` is `null`.
  - `ArgumentException` if `path` is empty or whitespace.
  - `UnauthorizedAccessException` if the caller lacks required permissions.
  - `DirectoryNotFoundException` if the parent directory does not exist.
  - `IOException` on I/O failure.

---
### `public static async Task<List<DataPoint>> ReadFromFileAsync(string path)`
Reads a JSON file containing a serialized `DataPoint` or array of `DataPoint` objects asynchronously.

- **Parameters**:
  - `path`: The file system path to read from.
- **Returns**: A `Task<List<DataPoint>>` containing the deserialized data.
- **Throws**:
  - `ArgumentNullException` if `path` is `null`.
  - `ArgumentException` if `path` is empty or whitespace.
  - `FileNotFoundException` if the file does not exist.
  - `UnauthorizedAccessException` if the caller lacks required permissions.
  - `JsonException` if the file content is invalid JSON or incompatible with `DataPoint`.

---
### `public static string UnixToIso8601(long unixTimestamp)`
Converts a Unix timestamp (seconds since epoch) to an ISO 8601 formatted UTC date-time string.

- **Parameters**:
  - `unixTimestamp`: The Unix timestamp to convert.
- **Returns**: An ISO 8601 formatted UTC date-time string (e.g., `"2024-01-01T00:00:00Z"`).

---
### `public static long Iso8601ToUnix(string iso8601)`
Converts an ISO 8601 formatted UTC date-time string to a Unix timestamp (seconds since epoch).

- **Parameters**:
  - `iso8601`: The ISO 8601 formatted UTC date-time string to convert.
- **Returns**: The corresponding Unix timestamp.
- **Throws**:
  - `ArgumentNullException` if `iso8601` is `null`.
  - `FormatException` if the string is not a valid ISO 8601 UTC date-time.

---
### `public static Dictionary<string, object> ToDictionary(DataPoint dataPoint)`
Converts a `DataPoint` instance into a `Dictionary<string, object>` representation.

- **Parameters**:
  - `dataPoint`: The `DataPoint` instance to convert.
- **Returns**: A dictionary mapping property names to their values.
- **Throws**: `ArgumentNullException` if `dataPoint` is `null`.

---
### `public static Dictionary<string, object> ToDictionary(IEnumerable<KeyValuePair<string, object>> pairs)`
Converts an enumerable of key-value pairs into a `Dictionary<string, object>`.

- **Parameters**:
  - `pairs`: The collection of key-value pairs to convert.
- **Returns**: A dictionary constructed from the pairs.
- **Throws**:
  - `ArgumentNullException` if `pairs` is `null`.
  - `ArgumentException` if `pairs` contains duplicate keys or a `null` key.

## Usage

### Example 1: Serializing and Deserializing a Single DataPoint
