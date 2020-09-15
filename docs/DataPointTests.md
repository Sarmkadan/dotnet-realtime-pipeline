# DataPointTests

DataPointTests is a unit test class that validates the behavior of the `DataPoint` type in the `dotnet-realtime-pipeline` project. It covers validation rules, quality threshold checking, cloning semantics, and metadata storage. Each test method exercises a specific scenario to ensure the production code meets its contract.

## API

### `Validate_WithAllValidProperties_ReturnsTrue`
Tests that `DataPoint.Validate()` returns `true` when the instance has a non-zero `Id`, a non-empty `Source`, and a `Quality` value within the allowed range (0.0 to 1.0 inclusive).  
**Parameters:** None  
**Return value:** `void` (asserts the condition)  
**Throws:** `AssertionException` if the validation result is not `true`.

### `Validate_WithZeroId_ReturnsFalse`
Tests that `DataPoint.Validate()` returns `false` when the `Id` property is zero, regardless of other valid properties.  
**Parameters:** None  
**Return value:** `void`  
**Throws:** `AssertionException` if the validation result is not `false`.

### `Validate_WithEmptySource_ReturnsFalse`
Tests that `DataPoint.Validate()` returns `false` when the `Source` property is an empty string or `null`, even if other properties are valid.  
**Parameters:** None  
**Return value:** `void`  
**Throws:** `AssertionException` if the validation result is not `false`.

### `Validate_WithQualityAboveUpperBound_ReturnsFalse`
Tests that `DataPoint.Validate()` returns `false` when the `Quality` property exceeds the upper bound of 1.0.  
**Parameters:** None  
**Return value:** `void`  
**Throws:** `AssertionException` if the validation result is not `false`.

### `MeetsQualityThreshold_WhenQualityEqualsThreshold_ReturnsTrue`
Tests that `DataPoint.MeetsQualityThreshold(double threshold)` returns `true` when the instance’s `Quality` is exactly equal to the provided threshold.  
**Parameters:** None  
**Return value:** `void`  
**Throws:** `AssertionException` if the method does not return `true`.

### `MeetsQualityThreshold_WhenQualityBelowThreshold_ReturnsFalse`
Tests that `DataPoint.MeetsQualityThreshold(double threshold)` returns `false` when the instance’s `Quality` is strictly less than the provided threshold.  
**Parameters:** None  
**Return value:** `void`  
**Throws:** `AssertionException` if the method does not return `false`.

### `Clone_WithNewId_PreservesValueSourceAndQuality`
Tests that `DataPoint.Clone(int newId)` returns a new `DataPoint` instance with the specified `Id`, while the `Value`, `Source`, and `Quality` properties are copied from the original.  
**Parameters:** None  
**Return value:** `void`  
**Throws:** `AssertionException` if the cloned instance does not match the expected property values.

### `AddMetadata_WithValidKeyAndValue_StoresEntry`
Tests that `DataPoint.AddMetadata(string key, string value)` successfully adds a new metadata entry, and that the entry can be retrieved (e.g., via an indexer or `TryGetValue`).  
**Parameters:** None  
**Return value:** `void`  
**Throws:** `AssertionException` if the metadata is not stored correctly.

### `AddMetadata_OverwritesExistingKeyWithNewValue`
Tests that `DataPoint.AddMetadata(string key, string value)` overwrites the value of an existing key, rather than throwing or ignoring the duplicate.  
**Parameters:** None  
**Return value:** `void`  
**Throws:** `AssertionException` if the metadata is not updated.

## Usage

The following examples demonstrate typical usage of the `DataPoint` class as exercised by the tests.

```csharp
// Example 1: Creating and validating a DataPoint
var point = new DataPoint
{
    Id = 42,
    Source = "temperature-sensor-01",
    Value = 23.5,
    Quality = 0.95
};

bool isValid = point.Validate(); // returns true
bool meetsThreshold = point.MeetsQualityThreshold(0.9); // returns true (0.95 >= 0.9)
```

```csharp
// Example 2: Cloning a DataPoint and adding metadata
var original = new DataPoint
{
    Id = 1,
    Source = "pressure-sensor",
    Value = 1013.25,
    Quality = 0.8
};

original.AddMetadata("unit", "hPa");
original.AddMetadata("location", "lab-3");

var clone = original.Clone(2); // new Id = 2, same Value, Source, Quality

// Metadata is not cloned (by design)
bool hasMetadata = clone.TryGetMetadata("unit", out _); // returns false
```

## Notes

- **Edge cases:** The validation tests cover boundary conditions: zero `Id`, empty `Source`, and `Quality` values above 1.0. The threshold tests verify equality and strict inequality. The clone test ensures that only the `Id` changes, while value, source, and quality are preserved. Metadata tests confirm that keys are case-sensitive and that overwriting replaces the previous value without throwing.
- **Thread safety:** The `DataPoint` class is not guaranteed to be thread-safe. The test methods assume single-threaded execution and do not verify concurrent access. If `DataPoint` is used in a multi-threaded context, external synchronization (e.g., locks) should be applied, especially when mutating properties or metadata.
- **Test isolation:** Each test method creates its own `DataPoint` instance; no shared state exists between tests. This ensures deterministic results and avoids order dependencies.
