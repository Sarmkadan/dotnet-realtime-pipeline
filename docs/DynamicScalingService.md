# DynamicScalingService

DynamicScalingService is responsible for evaluating and managing the scaling state of processing stages within a real-time data pipeline. It provides mechanisms to asynchronously assess scaling requirements and retrieve current scaling states for individual stages or the entire pipeline.

## API

### DynamicScalingService()

Initializes a new instance of the DynamicScalingService. The constructor sets up internal state management required for tracking and evaluating scaling decisions across pipeline stages.

### Task EvaluateScalingAsync()

Triggers an asynchronous evaluation of scaling requirements for all monitored stages. This method assesses current workload metrics and adjusts scaling states accordingly.

**Returns:**  
A Task representing the asynchronous operation. The task completes when the evaluation is finished.

**Exceptions:**  
Throws InvalidOperationException if the service is not properly initialized or if scaling evaluation encounters an unrecoverable configuration error.

### StageScalingState? GetScalingState(string stageName)

Retrieves the current scaling state for a specific pipeline stage.

**Parameters:**  
- `stageName` (string): The name of the stage to query. Case-sensitive.

**Returns:**  
The StageScalingState for the specified stage, or null if the stage is not currently being tracked.

**Exceptions:**  
Throws ArgumentNullException if stageName is null.

### IReadOnlyDictionary<string, StageScalingState> GetAllScalingStates()

Retrieves the scaling states for all currently monitored pipeline stages.

**Returns:**  
A read-only dictionary mapping stage names to their respective StageScalingState instances. Returns an empty dictionary if no stages are being tracked.

**Exceptions:**  
No exceptions are thrown; returns an empty collection if no states exist.

## Usage

```csharp
// Example 1: Triggering scaling evaluation
var scalingService = new DynamicScalingService();
await scalingService.EvaluateScalingAsync();

// Example 2: Querying scaling states
var scalingService = new DynamicScalingService();
var allStates = scalingService.GetAllScalingStates();

foreach (var kvp in allStates)
{
    Console.WriteLine($"Stage: {kvp.Key}, State: {kvp.Value.CurrentScale}");
}

var specificState = scalingService.GetScalingState("DataProcessor");
if (specificState.HasValue)
{
    Console.WriteLine($"Current scale for DataProcessor: {specificState.Value.CurrentScale}");
}
```

## Notes

- GetScalingState returns null for unregistered stages rather than throwing exceptions, allowing safe querying of arbitrary stage names.
- GetAllScalingStates returns a snapshot of states at the time of invocation; modifications during enumeration may not be reflected.
- Concurrent calls to EvaluateScalingAsync may result in overlapping evaluations. Implementations should ensure thread-safe access to shared state.
- Stage names are case-sensitive and must match exactly with registered pipeline stage identifiers.
