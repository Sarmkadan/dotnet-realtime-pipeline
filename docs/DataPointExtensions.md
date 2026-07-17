# DataPointExtensions

Provides extension methods for `DataPoint` to enable fluent-style updates, metadata access, logging, and staleness detection. All methods return new instances rather than modifying the original, ensuring immutability for pipeline processing scenarios.

## API

### WithValue

Creates a new `DataPoint` with an updated numeric value while preserving all other properties including ID, timestamp, source, quality, tags, and metadata.

- **Parameters:**
  - `dataPoint` – The source data point to update (required).
  - `newValue` – The new numeric value to assign.

- **Returns:** A new `DataPoint` instance with the updated value.

- **Throws:**
  - `ArgumentNullException` if `dataPoint` is null.

**Example:**
```csharp
var original = new DataPoint(1, 1000L, 25.5, "temperature-sensor");
var updated = original.WithValue(37.2);
// original.Value == 25.5, updated.Value == 37.2
```

---

### WithTimestamp

Creates a new `DataPoint` with an updated timestamp in milliseconds since Unix epoch, preserving all other properties. Updates the `CreatedAt` field to the current UTC time to reflect when the modification occurred.

- **Parameters:**
  - `dataPoint` – The source data point to update (required).
  - `newTimestamp` – The new timestamp in milliseconds since Unix epoch.

- **Returns:** A new `DataPoint` instance with the updated timestamp and `CreatedAt` set to `DateTime.UtcNow`.


- **Throws:**
  - `ArgumentNullException` if `dataPoint` is null.

**Example:**
```csharp
var original = new DataPoint(1, 1000L, 42.0, "pressure-sensor");
var updated = original.WithTimestamp(2000L);
// original.Timestamp == 1000L, updated.Timestamp == 2000L
// updated.CreatedAt reflects when WithTimestamp was called
```

---

### WithQuality

Creates a new `DataPoint` with an updated quality score (0–100) while preserving all other properties. Quality scores outside the valid range are rejected to maintain data integrity.

- **Parameters:**
  - `dataPoint` – The source data point to update (required).
  - `newQuality` – The new quality score between 0 and 100 inclusive.

- **Returns:** A new `DataPoint` instance with the updated quality score.

- **Throws:**
  - `ArgumentNullException` if `dataPoint` is null.
  - `ArgumentOutOfRangeException` if `newQuality` is < 0 or > 100.

**Example:**
```csharp
var original = new DataPoint(1, 1000L, 100.0, "sensor");
var updated = original.WithQuality(87);
// original.Quality == 100, updated.Quality == 87

try
{
    original.WithQuality(-5); // throws ArgumentOutOfRangeException
}
catch (ArgumentOutOfRangeException) { /* handled */ }
```

---

### WithTags

Creates a new `DataPoint` with additional tags appended to the existing comma-separated tags string. If the original has no tags, the new tags become the value; otherwise, they are concatenated with a comma.

- **Parameters:**
  - `dataPoint` – The source data point to update (required).
  - `newTags` – Comma-separated tags to append (required, non-empty).

- **Returns:** A new `DataPoint` instance with updated tags.

- **Throws:**
  - `ArgumentNullException` if `dataPoint` is null.
  - `ArgumentException` if `newTags` is null or whitespace.

**Example:**
```csharp
var original = new DataPoint(1, 1000L, 50.0, "flow-meter") { Tags = "unit:A" };
var updated = original.WithTags("location:west");
// original.Tags == "unit:A"
// updated.Tags == "unit:A,location:west"

var noTags = new DataPoint(2, 2000L, 25.0, "sensor");
var firstTags = noTags.WithTags("env:prod");
// firstTags.Tags == "env:prod"
```

---

### GetMetadataValues<T>

Retrieves all metadata values of a specified type as a read-only list. Useful for extracting strongly-typed metadata from a data point without manual casting or iteration.

- **Type Parameters:**
  - `T` – The expected type of metadata values to retrieve.

- **Parameters:**
  - `dataPoint` – The data point containing metadata (required).

- **Returns:** A read-only list of values that match the requested type; empty if none found.

- **Throws:**
  - `ArgumentNullException` if `dataPoint` is null.

**Example:**
```csharp
var point = new DataPoint(1, 1000L, 1.0, "sensor");
point.AddMetadata("region", "us-east-1");
point.AddMetadata("az", "us-east-1a");
point.AddMetadata("threshold", 95.5); // stored as double

var thresholds = point.GetMetadataValues<double>();
// thresholds == [95.5]

var strings = point.GetMetadataValues<string>();
// strings == ["us-east-1", "us-east-1a"]
```

---

### TryGetMetadataValue<T>

Attempts to retrieve a metadata value by key with compile-time type safety. Returns false if the key is missing or the value is not of type T, avoiding exceptions for expected absence.

- **Type Parameters:**
  - `T` – The expected type of the metadata value.

- **Parameters:**
  - `dataPoint` – The data point containing metadata (required).
  - `key` – The metadata key to retrieve (required, non-empty).
  - `value` – Output parameter receiving the typed value if found.

- **Returns:** `true` if the key exists and the value is of type `T`; otherwise `false`.

- **Throws:**
  - `ArgumentNullException` if `dataPoint` is null.
  - `ArgumentException` if `key` is null or whitespace.

**Example:**
```csharp
var point = new DataPoint(1, 1000L, 1.0, "sensor");
point.AddMetadata("temperature", 23.7);
point.AddMetadata("unit", "celsius");

if (point.TryGetMetadataValue("temperature", out double temp))
{
    Console.WriteLine($"Temperature: {temp}"); // prints 23.7
}

if (point.TryGetMetadataValue("unit", out int notAnInt))
{
    // this block is not executed; type mismatch
}

if (point.TryGetMetadataValue("missing", out string _))
{
    // this block is not executed; key missing
}
```

---

### ToLogString

Formats the data point as a human-readable string suitable for logging. Includes source, timestamp (ISO-8601), value, quality, and optional metadata count.

- **Parameters:**
  - `dataPoint` – The data point to format (required).
  - `includeMetadata` – Whether to append metadata count to the output (optional, default `false`).

- **Returns:** A formatted string representation of the data point.

- **Throws:**
  - `ArgumentNullException` if `dataPoint` is null.

**Example:**
```csharp
var point = new DataPoint(123, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 42.8, "cpu-load");
point.Quality = 92;

Console.WriteLine(point.ToLogString());
// Output: DataPoint[123] - Source: cpu-load, Timestamp: 2026-07-17T14:30:00.0000000+00:00, Value: 42.8, Quality: 92%

Console.WriteLine(point.ToLogString(includeMetadata: true));
// Output includes " | Metadata[0]" if metadata is present
```

---

### IsStale

Determines whether the data point is stale based on its age relative to a maximum allowed age threshold. Useful for filtering outdated telemetry in real-time pipelines.

- **Parameters:**
  - `dataPoint` – The data point to check (required).
  - `maxAgeMs` – Maximum allowed age in milliseconds before the point is considered stale (must be ≥ 0).

- **Returns:** `true` if the data point’s age exceeds `maxAgeMs`; otherwise `false`.

- **Throws:**
  - `ArgumentNullException` if `dataPoint` is null.
  - `ArgumentOutOfRangeException` if `maxAgeMs` is negative.

**Example:**
```csharp
var recent = new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1000, 1.0, "sensor");
var old = new DataPoint(2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 30_000, 1.0, "sensor");

bool recentIsStale = recent.IsStale(maxAgeMs: 5_000);  // false
bool oldIsStale = old.IsStale(maxAgeMs: 5_000);      // true
```

---

### WithId

Creates a shallow copy of a data point with a new unique identifier. All other properties (timestamp, value, source, quality, tags, metadata, and `CreatedAt`) are preserved.

- **Parameters:**
  - `dataPoint` – The source data point to copy (required).
  - `newId` – The new positive identifier for the copied data point.

- **Returns:** A new `DataPoint` instance with the same properties except for `Id`.

- **Throws:**
  - `ArgumentNullException` if `dataPoint` is null.
  - `ArgumentOutOfRangeException` if `newId` is not positive.

**Example:**
```csharp
var original = new DataPoint(1, 1000L, 100.0, "sensor-A");
var copy = original.WithId(999);
// copy.Id == 999
// copy.Value == original.Value, copy.Source == original.Source, etc.

try
{
    original.WithId(0); // throws ArgumentOutOfRangeException
}
catch (ArgumentOutOfRangeException) { /* handled */ }
```

## Usage

### Fluent Updates in a Processing Pipeline

```csharp
// Start with a base data point
var point = new DataPoint(1, 1000L, 25.5, "temperature-sensor");

// Apply a series of transformations fluently
var processed = point
    .WithQuality(95)
    .WithTags("calibrated:true")
    .WithValue(25.8);

Console.WriteLine(processed.ToLogString(includeMetadata: true));
```

### Metadata Extraction and Filtering

```csharp
// Create a data point with typed metadata
var enriched = new DataPoint(1, 1000L, 1.0, "network");
enriched.AddMetadata("region", "us-west-2");
enriched.AddMetadata("threshold", 80.0);

// Extract all thresholds for alerting
var thresholds = enriched.GetMetadataValues<double>();
if (thresholds.Count > 0 && thresholds[0] > 75.0)
{
    Console.WriteLine("High threshold configured");
}

// Safely retrieve a specific metadata value
if (enriched.TryGetMetadataValue("region", out string region))
{
    Console.WriteLine($"Region: {region}");
}
```

## Notes

- **Immutability:** All methods return new `DataPoint` instances; the original remains unmodified. This pattern supports deterministic pipelines and simplifies debugging.
- **Thread Safety:** The extension class itself contains only static methods and no shared state, making it inherently thread-safe. The returned `DataPoint` instances are immutable with respect to updates, but their `Metadata` dictionary is a shallow copy; concurrent modifications to nested objects (e.g., mutating a metadata value) are not prevented unless the caller ensures external synchronization.
- **Timestamp Handling:** `WithTimestamp` updates `CreatedAt` to the current UTC time to reflect when the modification occurred, not when the original data point was created.
- **Staleness Threshold:** `IsStale` uses the current UTC time at invocation; results may vary if called at different times even for the same data point.
- **Metadata Type Safety:** `GetMetadataValues<T>` and `TryGetMetadataValue<T>` use runtime type checking; passing an incorrect type parameter will return an empty list or false, respectively, without throwing.
- **ID Validation:** `WithId` enforces positive IDs; zero or negative values are rejected to maintain referential integrity in repositories and logs.