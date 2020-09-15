# PerformanceHelper
The `PerformanceHelper` class provides a set of methods and properties to measure the execution time of code blocks, benchmark performance, and collect memory statistics. It allows developers to analyze the performance of their applications and identify potential bottlenecks.

## API
### MeasureExecution
* Purpose: Measures the execution time of a synchronous code block.
* Parameters: A lambda expression or a method that returns a value of type `T`.
* Return Value: A tuple containing the result of the execution and the elapsed time in milliseconds.
* Throws: No exceptions are explicitly thrown, but any exceptions thrown by the measured code block will be propagated.

### MeasureExecutionAsync
* Purpose: Measures the execution time of an asynchronous code block.
* Parameters: A lambda expression or a method that returns a `Task` that yields a value of type `T`.
* Return Value: A `Task` that yields a tuple containing the result of the execution and the elapsed time in milliseconds.
* Throws: No exceptions are explicitly thrown, but any exceptions thrown by the measured code block will be propagated.

### Benchmark
* Purpose: Provides a benchmark result.
* Parameters: None.
* Return Value: A `BenchmarkResult` object.
* Throws: No exceptions are explicitly thrown.

### GetMemoryStats
* Purpose: Provides memory statistics.
* Parameters: None.
* Return Value: A `MemoryStats` object.
* Throws: No exceptions are explicitly thrown.

### Properties
* `Iterations`: The number of iterations performed during benchmarking.
* `Measurements`: A list of elapsed times in milliseconds.
* `AverageMs`: The average elapsed time in milliseconds.
* `MinMs`: The minimum elapsed time in milliseconds.
* `MaxMs`: The maximum elapsed time in milliseconds.
* `MedianMs`: The median elapsed time in milliseconds.
* `P95Ms`: The 95th percentile elapsed time in milliseconds.
* `P99Ms`: The 99th percentile elapsed time in milliseconds.
* `WorkingSetMb`: The working set memory usage in megabytes.
* `PrivateMemoryMb`: The private memory usage in megabytes.
* `PeakWorkingSetMb`: The peak working set memory usage in megabytes.
* `GC0Collections`, `GC1Collections`, `GC2Collections`: The number of garbage collections performed.
* `TotalMemoryMb`: The total memory usage in megabytes.

### ToString
* Purpose: Returns a string representation of the `PerformanceHelper` object.
* Parameters: None.
* Return Value: A string representation of the object.
* Throws: No exceptions are explicitly thrown.

## Usage
```csharp
// Example 1: Measuring the execution time of a synchronous code block
var (result, elapsedMs) = PerformanceHelper.MeasureExecution(() =>
{
    // Code block to measure
    for (int i = 0; i < 1000000; i++) { }
    return true;
});
Console.WriteLine($"Elapsed time: {elapsedMs}ms, Result: {result}");

// Example 2: Measuring the execution time of an asynchronous code block
var (result, elapsedMs) = await PerformanceHelper.MeasureExecutionAsync(async () =>
{
    // Asynchronous code block to measure
    await Task.Delay(100);
    return true;
});
Console.WriteLine($"Elapsed time: {elapsedMs}ms, Result: {result}");
```

## Notes
* The `MeasureExecution` and `MeasureExecutionAsync` methods are not thread-safe and should not be used concurrently.
* The `Benchmark` and `GetMemoryStats` methods are thread-safe, but the results may vary depending on the system's current state.
* The `Measurements` list and the `Iterations` property are updated after each measurement, and the `AverageMs`, `MinMs`, `MaxMs`, `MedianMs`, `P95Ms`, and `P99Ms` properties are recalculated based on the updated measurements.
* The `WorkingSetMb`, `PrivateMemoryMb`, `PeakWorkingSetMb`, `GC0Collections`, `GC1Collections`, `GC2Collections`, and `TotalMemoryMb` properties provide a snapshot of the system's memory usage and garbage collection statistics at the time of measurement.
