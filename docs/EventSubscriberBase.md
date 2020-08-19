# EventSubscriberBase

`EventSubscriberBase` is an abstract base class that provides common functionality for subscribers in a real-time data processing pipeline. It defines the core contract for subscribing to and unsubscribing from events while offering derived classes the ability to extend or override behavior. The class is designed to support metrics collection and backpressure monitoring in high-throughput scenarios.

## API

### `public virtual void Subscribe()`

Initiates the subscription process for the event subscriber. This method may be overridden by derived classes to implement custom subscription logic while maintaining the base functionality.

**Parameters**
None

**Return Value**
None

**Exceptions**
May throw exceptions if the underlying subscription mechanism fails (e.g., network issues, permission errors). Derived classes should document specific exceptions they may throw.

---

### `public virtual void Unsubscribe()`

Terminates an active subscription. This method provides cleanup for resources allocated during subscription. Derived classes may extend this method to perform additional cleanup.

**Parameters**
None

**Return Value**
None

**Exceptions**
May throw exceptions if the unsubscription process encounters errors (e.g., resource cleanup failures). Derived classes should document specific exceptions they may throw.

---

### `public DataIngestSubscriber Subscribe()`

Creates and returns a new `DataIngestSubscriber` instance with default configuration. This override initializes a subscriber optimized for data ingestion pipelines.

**Parameters**
None

**Return Value**
A new `DataIngestSubscriber` instance.

**Exceptions**
May throw exceptions if the subscriber cannot be initialized (e.g., configuration errors, resource constraints).

---

### `public ProcessingCompletionSubscriber Subscribe()`

Creates and returns a new `ProcessingCompletionSubscriber` instance with default configuration. This override initializes a subscriber optimized for tracking the completion of processing stages.

**Parameters**
None

**Return Value**
A new `ProcessingCompletionSubscriber` instance.

**Exceptions**
May throw exceptions if the subscriber cannot be initialized (e.g., configuration errors, resource constraints).

---
### `public double GetSuccessRatePercent()`

Calculates and returns the percentage of successful operations relative to the total operations processed by this subscriber.

**Parameters**
None

**Return Value**
A `double` representing the success rate as a percentage (e.g., `99.5` for 99.5%).

**Exceptions**
None

---
### `public BackpressureAlertSubscriber Subscribe()`

Creates and returns a new `BackpressureAlertSubscriber` instance with default configuration. This override initializes a subscriber that monitors and alerts on backpressure conditions in the pipeline.

**Parameters**
None

**Return Value**
A new `BackpressureAlertSubscriber` instance.

**Exceptions**
May throw exceptions if the subscriber cannot be initialized (e.g., configuration errors, resource constraints).

---
### `public int GetBackpressureEventCount()`

Returns the total number of backpressure events detected by this subscriber.

**Parameters**
None

**Return Value**
An `int` representing the count of backpressure events.

**Exceptions**
None

---
### `public MetricsAggregationSubscriber Subscribe()`

Creates and returns a new `MetricsAggregationSubscriber` instance with default configuration. This override initializes a subscriber that aggregates and reports metrics from the pipeline.

**Parameters**
None

**Return Value**
A new `MetricsAggregationSubscriber` instance.

**Exceptions**
May throw exceptions if the subscriber cannot be initialized (e.g., configuration errors, resource constraints).

---
### `public double GetAverageProcessingTime()`

Calculates and returns the average processing time per event in milliseconds.

**Parameters**
None

**Return Value**
A `double` representing the average processing time in milliseconds.

**Exceptions**
None

---
### `public int GetMetricsCount()`

Returns the total number of metrics collected by this subscriber.

**Parameters**
None

**Return Value**
An `int` representing the count of metrics collected.

**Exceptions**
None

---
### `public ErrorAlertSubscriber Subscribe()`

Creates and returns a new `ErrorAlertSubscriber` instance with default configuration. This override initializes a subscriber that monitors and alerts on errors in the pipeline.

**Parameters**
None

**Return Value**
A new `ErrorAlertSubscriber` instance.

**Exceptions**
May throw exceptions if the subscriber cannot be initialized (e.g., configuration errors, resource constraints).

---
### `public int GetErrorCount()`

Returns the total number of errors detected by this subscriber.

**Parameters**
None

**Return Value**
An `int` representing the count of errors.

**Exceptions**
None

## Usage

### Example 1: Basic Subscription and Metrics Collection
```csharp
var metricsSubscriber = EventSubscriberBase.Subscribe() as MetricsAggregationSubscriber;
if (metricsSubscriber != null)
{
    // Simulate processing events
    for (int i = 0; i < 100; i++)
    {
        // Process events...
    }

    Console.WriteLine($"Processed {metricsSubscriber.GetMetricsCount()} events");
    Console.WriteLine($"Average processing time: {metricsSubscriber.GetAverageProcessingTime()} ms");
    Console.WriteLine($"Success rate: {metricsSubscriber.GetSuccessRatePercent()}%");

    metricsSubscriber.Unsubscribe();
}
```

### Example 2: Monitoring Backpressure and Errors
```csharp
var backpressureSubscriber = EventSubscriberBase.Subscribe() as BackpressureAlertSubscriber;
var errorSubscriber = EventSubscriberBase.Subscribe() as ErrorAlertSubscriber;

if (backpressureSubscriber != null && errorSubscriber != null)
{
    // Simulate pipeline operations
    for (int i = 0; i < 50; i++)
    {
        // Simulate backpressure or errors
        if (i % 10 == 0)
        {
            backpressureSubscriber.RecordBackpressureEvent();
            errorSubscriber.RecordError(new Exception("Simulated error"));
        }
    }

    Console.WriteLine($"Backpressure events: {backpressureSubscriber.GetBackpressureEventCount()}");
    Console.WriteLine($"Total errors: {errorSubscriber.GetErrorCount()}");

    backpressureSubscriber.Unsubscribe();
    errorSubscriber.Unsubscribe();
}
```

## Notes

- **Thread Safety**: The base implementation does not guarantee thread safety for derived classes. If multiple threads access the same subscriber instance, derived classes must implement their own synchronization mechanisms (e.g., `lock` statements) for methods that modify shared state (e.g., `Subscribe`, `Unsubscribe`, or metric collection methods).
- **State Management**: Subscribers are expected to be long-lived objects. Reusing a subscriber instance across multiple pipeline operations is supported, but derived classes should ensure proper state reset or accumulation logic where applicable.
- **Error Handling**: Derived classes should handle and log exceptions internally where possible to avoid disrupting the pipeline. Unhandled exceptions in `Subscribe` or `Unsubscribe` may lead to resource leaks or inconsistent states.
- **Metrics Accuracy**: Metrics such as success rate and average processing time are calculated based on data available at the time of query. For real-time accuracy, derived classes should ensure metrics are updated atomically with event processing.
- **Default Subscribers**: The factory-style `Subscribe` methods return default-configured instances. Derived classes requiring custom configuration should extend these methods or use constructor injection where supported.
