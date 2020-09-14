# ValidationHelper

The `ValidationHelper` class provides a centralized utility for validating data integrity, configuration settings, and processing results within the real-time pipeline. It offers static methods for verifying specific domain objects such as data points, pipeline configurations, and window events, returning structured `ValidationResult` instances that detail success or failure states. Additionally, the class includes helper methods for range and bounds checking, along with instance properties and methods to inspect validation outcomes and retrieve human-readable summaries of errors.

## API

### Static Validation Methods

#### `ValidateDataPoints`
```csharp
public static ValidationResult ValidateDataPoints(...)
```
Validates a collection of incoming data points for structural integrity, null values, and type consistency required by the pipeline.
*   **Parameters**: Accepts the collection of data points to be verified (specific parameter types inferred from implementation context).
*   **Returns**: A `ValidationResult` object indicating success or containing a list of `InvalidIndices` and an `ErrorMessage` if validation fails.
*   **Throws**: Throws an exception if the input collection itself is null or fundamentally malformed before iteration can begin.

#### `ValidatePipelineConfig`
```csharp
public static ValidationResult ValidatePipelineConfig(...)
```
Ensures that the pipeline configuration object contains all mandatory fields, valid connection strings, and logical setting combinations.
*   **Parameters**: Accepts the configuration object to be validated.
*   **Returns**: A `ValidationResult` indicating whether the configuration is ready for pipeline initialization.
*   **Throws**: Throws an exception if the configuration object is null.

#### `ValidateProcessingResults`
```csharp
public static ValidationResult ValidateProcessingResults(...)
```
Verifies the output of a processing stage to ensure results meet expected schemas and value constraints before downstream consumption.
*   **Parameters**: Accepts the processing result object or collection.
*   **Returns**: A `ValidationResult` detailing any anomalies found in the processed data.
*   **Throws**: Throws an exception if the input is null.

#### `ValidateWindowEvent`
```csharp
public static ValidationResult ValidateWindowEvent(...)
```
Validates time-windowed events to ensure start/end times are logical, the window duration is positive, and associated metadata is present.
*   **Parameters**: Accepts the window event object.
*   **Returns**: A `ValidationResult` confirming the event's validity for aggregation or storage.
*   **Throws**: Throws an exception if the event object is null.

### Static Helper Methods

#### `IsInTimeRange`
```csharp
public static bool IsInTimeRange(...)
```
Determines if a specific timestamp falls within a defined start and end time range.
*   **Parameters**: Accepts the target timestamp and the range boundaries.
*   **Returns**: `true` if the timestamp is within the inclusive/exclusive bounds defined by the implementation; otherwise `false`.
*   **Throws**: Generally does not throw unless invalid date structures are passed.

#### `IsWithinBounds`
```csharp
public static bool IsWithinBounds(...)
```
Checks if a numeric value or index resides within a specified minimum and maximum limit.
*   **Parameters**: Accepts the value to check and the boundary limits.
*   **Returns**: `true` if the value is within bounds; otherwise `false`.
*   **Throws**: Does not throw; returns `false` for invalid comparisons.

### Instance Members (ValidationResult Context)

*Note: The following members appear to be instance members of the result object returned by the static methods or the helper class itself when instantiated for a specific context.*

#### `IsValid`
```csharp
public bool IsValid
```
Gets a boolean flag indicating whether the last validation operation succeeded.
*   **Returns**: `true` if no errors were recorded; `false` otherwise.

#### `ErrorMessage`
```csharp
public string ErrorMessage
```
Gets a consolidated string message describing the primary reason for validation failure.
*   **Returns**: A descriptive error string if `IsValid` is `false`; otherwise, an empty string or null.

#### `InvalidIndices`
```csharp
public List<long> InvalidIndices
```
Gets a list of zero-based indices identifying specific items in a collection that failed validation.
*   **Returns**: A list of `long` integers representing the positions of invalid data points. Returns an empty list if validation passed or if the error was not index-specific.

#### `GetSummary`
```csharp
public string GetSummary
```
Generates a comprehensive summary of the validation state, including error counts and specific failure details.
*   **Returns**: A formatted string suitable for logging or user display.
*   **Note**: Defined as a property in the signature but functionally acts as a generator; ensure it is accessed as a property (`GetSummary`) not a method (`GetSummary()`).

## Usage

### Example 1: Validating Incoming Data Points
This example demonstrates how to validate a batch of sensor data points before processing. It checks the `IsValid` flag and iterates over `InvalidIndices` to log specific failures.

```csharp
using DotNetRealtimePipeline;

var dataPoints = GetSensorReadings(); // Assume this returns a list of data points

// Perform validation
var result = ValidationHelper.ValidateDataPoints(dataPoints);

if (!result.IsValid)
{
    Console.WriteLine($"Validation failed: {result.ErrorMessage}");
    
    // Log specific indices that caused the failure
    foreach (var index in result.InvalidIndices)
    {
        Console.WriteLine($"Invalid data point detected at index: {index}");
    }
    
    // Retrieve full summary for audit logging
    logger.Warn(result.GetSummary);
    return;
}

// Proceed with processing if valid
ProcessBatch(dataPoints);
```

### Example 2: Verifying Pipeline Configuration and Time Ranges
This example shows validating the pipeline configuration at startup and using the static time range helper to filter events.

```csharp
using DotNetRealtimePipeline;

// Validate configuration before starting the pipeline
var config = LoadConfiguration();
var configResult = ValidationHelper.ValidatePipelineConfig(config);

if (!configResult.IsValid)
{
    throw new InvalidOperationException($"Pipeline startup aborted: {configResult.GetSummary}");
}

// Process an incoming event
var eventTimestamp = DateTime.UtcNow;
var windowStart = new DateTime(2023, 10, 01, 0, 0, 0);
var windowEnd = new DateTime(2023, 10, 01, 1, 0, 0);

// Check if the event falls within the current processing window
if (ValidationHelper.IsInTimeRange(eventTimestamp, windowStart, windowEnd))
{
    var eventResult = ValidationHelper.ValidateWindowEvent(currentEvent);
    if (eventResult.IsValid)
    {
        AggregateEvent(currentEvent);
    }
    else
    {
        Console.WriteLine($"Event rejected: {eventResult.ErrorMessage}");
    }
}
```

## Notes

*   **Thread Safety**: The static validation methods (`ValidateDataPoints`, `ValidatePipelineConfig`, etc.) and helper methods (`IsInTimeRange`, `IsWithinBounds`) are designed to be stateless and thread-safe, allowing concurrent calls from multiple pipeline threads. However, the instance members (`IsValid`, `ErrorMessage`, `InvalidIndices`, `GetSummary`) belong to the returned `ValidationResult` instance. While reading these properties from a single result object is generally safe, do not share a single `ValidationResult` instance across threads for modification if the underlying implementation supports mutable state.
*   **Empty Collections**: Passing an empty collection to `ValidateDataPoints` or `ValidateProcessingResults` typically returns a valid result (`IsValid = true`) unless the business logic explicitly requires a minimum count of items.
*   **Index Precision**: The `InvalidIndices` property uses `long` rather than `int`, supporting validation of extremely large datasets that may exceed the `int32` maximum value.
*   **Error Aggregation**: The `ErrorMessage` property usually contains the first critical error encountered, whereas `GetSummary` should be used when a complete list of all validation failures is required for debugging or reporting.
*   **Null Handling**: While the static methods throw exceptions for null inputs to prevent silent failures, the `IsInTimeRange` and `IsWithinBounds` helpers generally return `false` for invalid inputs rather than throwing, facilitating use in filter expressions.
