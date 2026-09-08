# SubscriberOptions

This document describes the `SubscriberOptions` class defined in `src/Events/SubscriberOptions.cs`.

## Properties

| Property | Default Value | Meaning |
|----------|---------------|---------|
| `MaxQueueSize` | `1000` | Gets or sets the maximum number of events that can be queued per subscriber. When the queue is full, new events will be dropped or rejected based on `MaxQueueSizeBehavior`. |
| `MaxQueueSizeBehavior` | `DropNew` | Gets or sets the behavior when the queue is full. |
| `DispatchMode` | `Sequential` | Gets or sets the dispatch mode for event handling. |
| `ErrorPolicy` | `SwallowAndCount` | Gets or sets the error handling policy when an event handler throws. |
| `MaxDegreeOfParallelism` | `1` | Gets or sets the maximum degree of parallelism when `DispatchMode` is `Parallel`. Default: 1 (sequential). |
| `Name` | `null` | Gets or sets a custom name for this subscriber configuration. Useful for logging and diagnostics. |

## Enums

### SubscriberErrorPolicy

Defines error handling policies for subscriber error recovery.

| Value | Meaning |
|-------|---------|
| `SwallowAndCount` | Swallow the exception, log it, and continue processing subsequent events. |
| `DeadLetter` | Swallow the exception, log it, and send the failed event to a dead-letter queue. |
| `FailFast` | Propagate the exception to the caller, failing fast. |

### MaxQueueSizeBehavior

Defines behaviors when a subscriber's queue reaches maximum capacity.

| Value | Meaning |
|-------|---------|
| `DropNew` | Drop new events when the queue is full. |
| `Block` | Block and wait when the queue is full (may cause backpressure). |
| `Reject` | Reject new events with an exception when the queue is full. |

### SubscriberDispatchMode

Defines dispatch modes for subscriber event handling.

| Value | Meaning |
|-------|---------|
| `Sequential` | Process events sequentially in a single background task. |
| `Parallel` | Process events in parallel using multiple worker tasks. |

## SubscriberDeadLetterEntry

Represents a dead-letter entry for an event that failed to be processed by a subscriber.

| Property | Meaning |
|----------|---------|
| `EventName` | Gets the event name. |
| `SerializedEventArgs` | Gets the serialized event arguments. |
| `SubscriberName` | Gets the subscriber name. |
| `Timestamp` | Gets the timestamp when the event was sent to dead-letter. |
| `Exception` | Gets the exception that caused the failure. |

## Configuration Example

```csharp
var options = new SubscriberOptions
{
    MaxQueueSize = 5000,
    MaxQueueSizeBehavior = MaxQueueSizeBehavior.Block,
    DispatchMode = SubscriberDispatchMode.Parallel,
    MaxDegreeOfParallelism = 4,
    ErrorPolicy = SubscriberErrorPolicy.DeadLetter,
    Name = "high-priority-subscriber"
};
```