# SubscriberChannel

`src/Events/SubscriberChannel.cs` and `src/Events/SubscriberChannelManager.cs` implement the publisher's internal, per-subscriber dispatch layer. Every registration receives its own bounded `Channel`, consumer task, queue policy, dispatch mode, and error count. As a result, a slow or failing handler does not execute inline with the publisher and does not directly stop other subscribers from receiving or processing events.

These types are `internal`; applications normally configure them through subscriber registration on `PipelineEventPublisher`. See [`SubscriberOptions`](SubscriberOptions.md) for the public queue-size, dispatch, concurrency, error-policy, and diagnostic-name settings.

## `SubscriberChannel`

A `SubscriberChannel` belongs to one handler for one event name. Its constructor creates a bounded queue whose capacity is `SubscriberOptions.MaxQueueSize`. The consumer is not started by construction; the manager starts it as part of the publisher lifecycle.

### `bool TryPost(PipelineEventArgs args)`

Creates a work item and attempts to write it to the subscriber's queue without waiting for handler execution. It returns the result from the channel writer. Posting after disposal logs a warning and returns `false`.

Queue-full behavior uses the following implementation mapping:

| `MaxQueueSizeBehavior` | Channel full mode | Observable posting behavior |
|------------------------|-------------------|-----------------------------|
| `DropNew` | `DropNewest` | The channel makes room by dropping the newest queued item and accepts the attempted write. |
| `Block` | `Wait` | Because posting uses `TryWrite`, it does not wait; a full queue causes the attempted write to return `false`. |
| `Reject` | `DropOldest` | The channel makes room by dropping the oldest queued item and accepts the attempted write. |

The manager currently does not inspect the returned values, so a queue-full outcome is not propagated by `PostEventAsync`.

### `void Start()`

Creates a cancellation source and starts the background consumer. With `SubscriberDispatchMode.Sequential`, work items are handled one at a time in queue order. With `Parallel`, `Parallel.ForEachAsync` processes them with `MaxDegreeOfParallelism` as its concurrency limit.

Calling `Start()` while a consumer task is already assigned logs a warning and leaves the existing consumer running.

### `Task StopAsync()`

Cancels the consumer, completes the writer, and awaits the consumer task. Cancellation during shutdown is treated as expected; other stop-time exceptions are logged. The cancellation source is then disposed and the consumer fields are cleared.

`StopAsync()` returns immediately when the channel is disposed or has not been started. Completing the writer means the channel cannot be restarted for subsequent posting, even though the consumer-task field is cleared.

### `int GetErrorCount()`

Returns the number of non-cancellation exceptions caught from this subscriber's handler. The counter is incremented atomically before the configured error policy is applied.

### `void Dispose()`

Performs best-effort synchronous cleanup: it cancels and disposes the cancellation source, completes the writer, and marks the channel disposed. It does not await the consumer task; use `StopAsync()` first when graceful asynchronous shutdown is required. Repeated disposal has no effect.

## Error policies

Handler exceptions other than `OperationCanceledException` affect only that subscriber's consumer:

| `SubscriberErrorPolicy` | Channel behavior |
|-------------------------|------------------|
| `SwallowAndCount` | Counts and logs the failure, then continues consuming. |
| `DeadLetter` | Counts and logs the failure, then continues. The current implementation logs that the event is being sent to dead-letter but does not enqueue a dead-letter entry. |
| `FailFast` | Counts and logs the failure, then rethrows it, faulting that subscriber's consumer task. |

A fault under `FailFast` does not fault another subscriber's consumer. It can, however, surface later when that channel is stopped and awaited; `SubscriberChannel.StopAsync()` logs and absorbs that stop-time exception.

## `SubscriberChannelManager`

`SubscriberChannelManager` groups subscriber channels by event name. `RegisterSubscriber<T>` creates an isolated channel for a typed handler, using the supplied options or the manager defaults, and adds it to the event's channel list. The diagnostic subscriber name comes from `options.Name`, then the handler's declaring type, and finally `AnonymousSubscriber`.

### `Task PostEventAsync(string eventName, PipelineEventArgs args)`

Takes a snapshot of the channels registered for the event and invokes each channel's `TryPost` on a separate task. The returned task completes after all enqueue attempts finish; it does not wait for subscribers to handle the event. Unknown event names are ignored. Posting after manager disposal logs a warning and returns.

### `void StartAll()`

Starts every registered channel. A failure to start one channel is logged and does not prevent the manager from attempting to start the others. Calling it after disposal logs a warning and returns.

### `Task StopAllAsync()`

Calls `StopAsync()` for every registered channel and awaits all stop tasks together. It returns immediately after manager disposal.

### `int GetTotalErrorCount(string eventName)`

Sums the current handler error counts for all channels registered under the event name. It returns `0` when the event has no registered channels.

### `int GetSubscriberCount(string eventName)`

Returns the number of channels registered under the event name, or `0` when the event is unknown. Multiple registrations of the same handler are counted separately.

### `ValueTask DisposeAsync()` and `void Dispose()`

`DisposeAsync()` stops all channels, disposes each channel, clears the registry, and marks the manager disposed. Shutdown errors are handled as best-effort cleanup. Repeated calls have no effect.

`Dispose()` synchronously waits for `DisposeAsync()` to finish.

## Subscriber isolation

Fan-out occurs at enqueue time rather than by invoking handlers serially. Each subscriber therefore has an independent pressure boundary and processing loop:

- A slow subscriber fills only its own bounded queue; other subscribers continue consuming from theirs.
- Sequential or parallel dispatch is selected independently for each subscriber.
- A handler failure is counted and handled according to that subscriber's `ErrorPolicy`.
- A `FailFast` handler faults only its own consumer task, while the manager continues posting to other channels.

Isolation does not guarantee delivery. When one queue reaches `MaxQueueSize`, its configured full-mode behavior determines which event is rejected or discarded, without requiring another subscriber to slow down. Review [`SubscriberOptions`](SubscriberOptions.md) when choosing queue capacity, `MaxQueueSizeBehavior`, `DispatchMode`, `MaxDegreeOfParallelism`, and `ErrorPolicy`.
