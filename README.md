// existing content ...

## ValidationHelper
The `ValidationHelper` class provides validation methods for pipeline entities like data points, pipeline configurations, and processing results. It includes utility methods for time range checks and value bounds validation.

Example usage:
```csharp
// Validate data points
var helper = new ValidationHelper();
var dataPoints = new List<DataPoint>
{
    new DataPoint { Id = 1, Timestamp = 1620000000000, Value = 10.5 },
    new DataPoint { Id = 2, Timestamp = 1620000000001, Value = 20.3 }
};
var validationResult = helper.ValidateDataPoints(dataPoints);
if (!validationResult.IsValid)
{
    Console.WriteLine(validationResult.GetSummary());
    foreach (var invalidId in validationResult.InvalidIndices)
    {
        Console.WriteLine($"Invalid data point ID: {invalidId}");
    }
}

// Validate pipeline configuration
var config = new PipelineConfig { /* ... */ };
var configResult = ValidationHelper.ValidatePipelineConfig(config);
if (!configResult.IsValid)
{
    Console.WriteLine(configResult.GetSummary());
}

// Check if a data point is within a time range
bool isInRange = ValidationHelper.IsInTimeRange(dataPoints[0], 1620000000000, 1620000000005);
Console.WriteLine($"Is in range: {isInRange}");

// Check if a value is within bounds
bool isWithinBounds = ValidationHelper.IsWithinBounds(15.0, 10.0, 20.0);
Console.WriteLine($"Is within bounds: {isWithinBounds}");
```

This example demonstrates validating data points, pipeline configurations, time range checks, and value bounds validation. The `ValidationResult` object provides detailed information about validation outcomes through its `IsValid`, `ErrorMessage`, and `InvalidIndices` properties.
