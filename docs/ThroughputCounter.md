# ThroughputCounter
The `ThroughputCounter` type is designed to measure the rate at which events are processed, providing a way to track and analyze the performance of a system or application. It allows users to record events and calculate the throughput, which can be useful in a variety of scenarios, such as monitoring the efficiency of a data pipeline or evaluating the performance of a real-time system.

## API
* `public ThroughputCounter`: The constructor for the `ThroughputCounter` class, used to create a new instance.
* `public void RecordEvents`: Records a number of events, allowing the throughput to be calculated later. This method does not return any value and does not throw any exceptions based on its signature.
* `public void RecordEvents`: An overload of the `RecordEvents` method, likely allowing for different parameters to be passed in, such as a specific time interval or event count.
* `public double GetThroughput`: Calculates and returns the current throughput, measured in events per unit of time. The return value is a `double`, indicating a precise measurement. This method does not throw any exceptions based on its signature.
* `public double GetThroughput`: An overload of the `GetThroughput` method, possibly allowing for different time intervals or other parameters to be specified.

## Usage
The following examples demonstrate how to use the `ThroughputCounter` class:
```csharp
// Example 1: Simple throughput measurement
var counter = new ThroughputCounter();
counter.RecordEvents(100); // Record 100 events
var throughput = counter.GetThroughput(); // Calculate throughput
Console.WriteLine($"Throughput: {throughput} events per second");
```

```csharp
// Example 2: Measuring throughput over time
var counter = new ThroughputCounter();
for (int i = 0; i < 10; i++)
{
    counter.RecordEvents(100); // Record 100 events every iteration
    Thread.Sleep(1000); // Wait for 1 second
}
var throughput = counter.GetThroughput(); // Calculate average throughput
Console.WriteLine($"Average throughput: {throughput} events per second");
```

## Notes
When using the `ThroughputCounter` class, note that the `RecordEvents` method does not throw any exceptions based on its signature, but it may still throw exceptions if the input parameters are invalid or if an internal error occurs. Additionally, the `GetThroughput` method returns a precise measurement as a `double`, but the actual precision may depend on the system clock and other factors. In terms of thread-safety, the `ThroughputCounter` class does not appear to have any built-in synchronization mechanisms, so users should take care to avoid concurrent access to the same instance from multiple threads.
