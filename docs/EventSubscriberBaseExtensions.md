# EventSubscriberBaseExtensions

Provides a set of static extension methods for `EventSubscriberBase` instances and collections thereof. These methods enable safe unsubscription, diagnostic metric retrieval, status inspection, and conversion to read‑only lists. They are designed to simplify common monitoring and lifecycle tasks without exposing internal subscriber state directly.

## API

### `SafeUnsubscribe`
Unsubscribes the subscriber from its event source in a safe manner, handling any exceptions that may occur during the process.

- **Parameters**  
  `this EventSubscriberBase subscriber` – The subscriber to unsubscribe.
- **Return value**  
  `void`
- **Throws**  
  `ArgumentNullException` if `subscriber` is `null`.

### `GetSubscriberTypeName`
Returns the type name of the subscriber’s concrete implementation.

- **Parameters**  
  `this EventSubscriberBase subscriber` – The subscriber whose type name is requested.
- **Return value**  
  `string` – The fully qualified or simplified type name (implementation‑specific).
- **Throws**  
  `ArgumentNullException` if `subscriber` is `null`.

### `AsReadOnly`
Converts an enumerable collection of subscribers into a read‑only list.

- **Parameters**  
  `this IEnumerable<EventSubscriberBase> subscribers` – The source collection.
- **Return value**  
  `IReadOnlyList<EventSubscriberBase>` – A read‑only snapshot of the collection.
- **Throws**  
  `ArgumentNullException` if `subscribers` is `null`.

### `GetSuccessRatePercent`
Retrieves the success rate of the subscriber’s event processing as a percentage.

- **Parameters**  
  `this EventSubscriberBase subscriber` – The subscriber to query.
- **Return value**  
  `double` – A value between 0.0 and 100.0 representing the success rate.
- **Throws**  
  `ArgumentNullException` if `subscriber` is `null`.

### `GetBackpressureEventCount`
Returns the current number of events that are queued due to backpressure.

- **Parameters**  
  `this EventSubscriberBase subscriber` – The subscriber to query.
- **Return value**  
  `int` – The count of backpressured events.
- **Throws**  
  `ArgumentNullException` if `subscriber` is `null`.

### `GetAverageProcessingTime`
Gets the average processing time (in milliseconds) for events handled by the subscriber.

- **Parameters**  
  `this EventSubscriberBase subscriber` – The subscriber to query.
- **Return value**  
  `double` – The average processing time in milliseconds.
- **Throws**  
  `ArgumentNullException` if `subscriber` is `null`.

### `GetMetricsCount`
Returns the number of distinct metrics currently tracked for the subscriber.

- **Parameters**  
  `this EventSubscriberBase subscriber` – The subscriber to query.
- **Return value**  
  `int` – The count of tracked metrics.
- **Throws**  
  `ArgumentNullException` if `subscriber` is `null`.

### `GetErrorCount`
Returns the total number of errors that have occurred during the subscriber’s event processing.

- **Parameters**  
  `this EventSubscriberBase subscriber` – The subscriber to query.
- **Return value**  
  `int` – The error count.
- **Throws**  
  `ArgumentNullException` if `subscriber` is `null`.

### `IsInCriticalState`
Indicates whether the subscriber is currently in a critical state (e.g., exceeding error thresholds or experiencing severe backpressure).

- **Parameters**  
  `this EventSubscriberBase subscriber` – The subscriber to inspect.
- **Return value**  
  `bool` – `true` if the subscriber is in a critical state; otherwise `false`.
- **Throws**  
  `ArgumentNullException` if `subscriber` is `null`.

### `GetStatusString`
Returns a human‑readable string summarizing the subscriber’s current status, including key metrics and state information.

- **Parameters**  
  `this EventSubscriberBase subscriber` – The subscriber to describe.
- **Return value**  
  `string` – A formatted status summary.
- **Throws**  
  `ArgumentNullException` if `subscriber` is `null`.

## Usage

### Example 1 – Monitoring a single subscriber

```csharp
using System;
using DotNetRealtimePipeline;

public class MonitorExample
{
    public void InspectSubscriber(EventSubscriberBase subscriber)
    {
        Console.WriteLine($"Subscriber type: {subscriber.GetSubscriberTypeName()}");
        Console.WriteLine($"Success rate: {subscriber.GetSuccessRatePercent():F1}%");
        Console.WriteLine($"Average processing time: {subscriber.GetAverageProcessingTime():F2} ms");
        Console.WriteLine($"Errors: {subscriber.GetErrorCount()}");
        Console.WriteLine($"Backpressure events: {subscriber.GetBackpressureEventCount()}");
        Console.WriteLine($"Critical state: {subscriber.IsInCriticalState()}");
        Console.WriteLine($"Status: {subscriber.GetStatusString()}");
    }
}
```

### Example 2 – Batch operations on a collection

```csharp
using System.Collections.Generic;
using System.Linq;
using DotNetRealtimePipeline;

public class BatchExample
{
    public void ProcessSubscribers(IEnumerable<EventSubscriberBase> subscribers)
    {
        // Convert to read‑only list for safe iteration
        IReadOnlyList<EventSubscriberBase> snapshot = subscribers.AsReadOnly();

        // Unsubscribe all subscribers that are in a critical state
        foreach (var subscriber in snapshot.Where(s => s.IsInCriticalState()))
        {
            subscriber.SafeUnsubscribe();
        }

        // Log average success rate across all subscribers
        double avgSuccess = snapshot.Average(s => s.GetSuccessRatePercent());
        Console.WriteLine($"Average success rate across all subscribers: {avgSuccess:F1}%");
    }
}
```

## Notes

- All methods throw `ArgumentNullException` if the provided subscriber (or collection) is `null`. Always validate inputs before calling.
- The metric‑related methods (`GetSuccessRatePercent`, `GetBackpressureEventCount`, `GetAverageProcessingTime`, `GetMetricsCount`, `GetErrorCount`, `IsInCriticalState`, `GetStatusString`) return snapshot values. They are safe to call concurrently from multiple threads, but the returned data may become stale immediately after the call if the subscriber’s internal state is being updated by another thread.
- `SafeUnsubscribe` is designed to be idempotent and thread‑safe. It will not throw if the subscriber is already unsubscribed; any exceptions thrown by the underlying unsubscribe logic are caught and suppressed.
- `AsReadOnly` creates a new list copy of the source collection. Modifications to the original enumerable after the call do not affect the returned read‑only list.
- When a subscriber is in a critical state, `IsInCriticalState` returns `true`. The exact criteria for critical state are implementation‑defined and may depend on error thresholds, backpressure levels, or processing time limits.
- The `GetStatusString` output format is not guaranteed to be stable across versions; use it for logging or display purposes only, not for programmatic parsing.
