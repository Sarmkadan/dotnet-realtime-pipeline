# DataPoint

The `DataPoint` type represents a single telemetry measurement within a real-time data pipeline. It encapsulates a numeric value together with rich metadata—such as timestamps, source identification, quality indicators, and extensible key-value pairs—to support ingestion, validation, routing, and archival workflows. Instances are designed to flow through pipeline stages where they can be validated, enriched, cloned, and filtered based on quality thresholds.

## API

### Constructors

```csharp
public DataPoint()
public DataPoint(long id, long timestamp, double value, string source, int quality = 100)
```

- **Parameterless constructor**: Creates an empty `DataPoint` with default values. `Id` and `Timestamp` are set to `0`, `Value` to `0.0`, `Source` to `null`, `Quality` to `0`, `CreatedAt` to `DateTime.UtcNow`, and `Metadata` to an empty dictionary. `Tags` remains `null`.
- **Parameterized constructor**: Initializes a `DataPoint` with the given identity, timestamp, value, source, and optional quality level. `CreatedAt` is set to `DateTime.UtcNow`. `Metadata` is initialized as an empty dictionary. `Tags` remains `null`.

### Properties

```csharp
public long Id { get; set; }
```
Unique identifier for the data point, typically assigned by the originating system or ingestion layer.

```csharp
public long Timestamp { get; set; }
```
Epoch-based timestamp (e.g., Unix milliseconds) indicating when the measurement was captured at the source.

```csharp
public double Value { get; set; }
```
The numeric measurement value carried by this data point.

```csharp
public string Source { get; set; }
```
Identifies the originating system, sensor, or feed that produced this data point.

```csharp
public Dictionary<string, object> Metadata { get; set; }
```
Extensible dictionary for arbitrary key-value pairs. Used to attach domain-specific context such as units, calibration data, or routing hints. Values are stored as `object` and must be cast upon retrieval.

```csharp
public DateTime CreatedAt { get; set; }
```
UTC timestamp indicating when this `DataPoint` instance was constructed within the pipeline. Distinct from `Timestamp`, which reflects the original measurement time.

```csharp
public string? Tags { get; set; }
```
Optional, nullable string containing tags or labels associated with the data point. The format is not enforced by the type itself; consumers may use comma-separated values, JSON, or other conventions.

```csharp
public int Quality { get; set; }
```
Integer quality indicator. The exact scale is domain-defined, but the `MeetsQualityThreshold` method interprets higher values as better quality. Defaults to `100` when using the parameterized constructor.

### Methods

```csharp
public bool Validate()
```
Performs a basic sanity check on the data point. Returns `true` if `Source` is not null or empty and `Value` is not `NaN` or infinity; otherwise returns `false`. Does not throw.

```csharp
public long GetAgeMs()
```
Computes the age of this data point relative to the current UTC time. Returns the difference in milliseconds between `DateTime.UtcNow` and `CreatedAt`. Does not throw.

```csharp
public void AddMetadata(string key, object value)
```
Adds or overwrites an entry in the `Metadata` dictionary. If `Metadata` is `null`, it is initialized to an empty dictionary before insertion.
- **Parameters**: `key` — the dictionary key (must not be null; behavior with null keys follows `Dictionary<string, object>` semantics and will throw `ArgumentNullException`). `value` — the object to store.
- **Throws**: `ArgumentNullException` if `key` is `null`.

```csharp
public bool MeetsQualityThreshold(int threshold)
```
Returns `true` if `Quality` is greater than or equal to the specified `threshold`; otherwise `false`. Does not throw.

```csharp
public DataPoint Clone()
```
Creates a shallow copy of the current instance. The new `DataPoint` has the same property values, but `Metadata` is copied to a new dictionary (the dictionary itself is new; the objects stored as values are shared references). `CreatedAt` on the clone is set to `DateTime.UtcNow` at the time of cloning. Does not throw.

## Usage

### Example 1: Ingesting and validating a sensor reading

```csharp
// Simulate receiving a raw measurement from a temperature sensor.
var reading = new DataPoint(
    id: 42001,
    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    value: 23.7,
    source: "sensor-temp-04",
    quality: 95
);

reading.AddMetadata("unit", "celsius");
reading.AddMetadata("location", "warehouse-2");

if (reading.Validate())
{
    Console.WriteLine($"Ingested valid reading {reading.Id} from {reading.Source}");
}
else
{
    Console.WriteLine("Reading failed validation — discarding.");
}
```

### Example 2: Filtering and cloning for downstream routing

```csharp
// A pipeline stage that forwards only high-quality points to a critical path.
var original = new DataPoint(
    id: 42002,
    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    value: 0.87,
    source: "vibration-probe-12",
    quality: 78
);

const int criticalThreshold = 90;

if (original.MeetsQualityThreshold(criticalThreshold))
{
    var clone = original.Clone();
    clone.AddMetadata("routing", "critical-path");
    // Forward clone to critical processing channel...
    Console.WriteLine($"Routed clone {clone.Id} to critical path. Age: {clone.GetAgeMs()} ms");
}
else
{
    Console.WriteLine(
        $"Point {original.Id} quality {original.Quality} below threshold {criticalThreshold} — skipping."
    );
}
```

## Notes

- **`Validate` behavior**: The method only checks `Source` and `Value`. A data point with `Quality` of `0` or `Timestamp` of `0` can still pass validation if those fields are valid. Callers needing stricter checks must layer additional logic.
- **`Clone` semantics**: The `Metadata` dictionary is structurally copied, but the objects it contains are shared by reference. Modifying a mutable object stored in the original’s `Metadata` will affect the clone and vice versa. `CreatedAt` is reset on the clone; `Id` and `Timestamp` are preserved as-is.
- **`GetAgeMs` precision**: The method uses `DateTime.UtcNow`, which has system-clock resolution. In high-frequency pipelines, successive calls may return identical values or very small differences. Do not rely on it for sub-millisecond precision.
- **`AddMetadata` null handling**: Passing a `null` key throws `ArgumentNullException`. Passing a `null` value is permitted and stored as a dictionary entry with a `null` value.
- **Thread safety**: This type is not thread-safe. Concurrent reads and writes to properties, `Metadata`, or `Tags` from multiple threads without external synchronization may lead to data races, torn reads, or dictionary corruption. Instances are intended to be owned by a single pipeline stage at a time or protected by the pipeline’s concurrency model.
