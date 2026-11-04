# SubscriberChannelManager

`src/Events/SubscriberChannelManager.cs` owns the per-event collection of [`SubscriberChannel`](SubscriberChannel.md) instances used by [`PipelineEventPublisher`](PipelineEventPublisher.md). It registers each handler on its own channel, fans a posted event out to every channel registered for that event name, starts and stops the channels together, and reports per-event subscriber counts and handler error totals.

The type is `internal sealed` and implements `IDisposable`. Its `DisposeAsync()` method follows the `IAsyncDisposable` shape but the class does not declare that interface. Application code does not create it directly; the publisher owns one instance for its lifetime.

## Purpose

- Keep subscribers for an event name isolated: each registration gets its own `SubscriberChannel` with its own queue, consumer, and error count.
- Make posting non-blocking with respect to handler execution. `PostEventAsync` only waits for enqueue attempts, not for handlers to finish.
- Provide a single lifecycle point (`StartAll`, `StopAllAsync`, `DisposeAsync`/`Dispose`) for all channels.

Registration is done by `RegisterSubscriber<T>(string eventName, Func<T, Task> handler, SubscriberOptions? options = null)`, which is covered in the table below together with the rest of the public surface.

## Public surface

| Member | Summary |
|--------|---------|
| `SubscriberChannelManager(ILogger logger, SubscriberOptions? defaultOptions = null)` | Creates an empty manager. |
| `void RegisterSubscriber<T>(string eventName, Func<T, Task> handler, SubscriberOptions? options = null) where T : PipelineEventArgs` | Adds a channel for `eventName`. |
| `Task PostEventAsync(string eventName, PipelineEventArgs args)` | Enqueues `args` on every channel for `eventName`. |
| `void StartAll()` | Starts every registered channel. |
| `Task StopAllAsync()` | Stops every registered channel and awaits them. |
| `int GetTotalErrorCount(string eventName)` | Sums handler error counts for an event. |
| `int GetSubscriberCount(string eventName)` | Counts channels registered for an event. |
| `ValueTask DisposeAsync()` / `void Dispose()` | Stops, disposes, and clears all channels. |

## Constructor

```csharp
public SubscriberChannelManager(ILogger logger, SubscriberOptions? defaultOptions = null)
```

- `logger` is required; `null` throws `ArgumentNullException`. It is used for registration, start, stop, and disposal diagnostics, and it is passed to every channel.
- `defaultOptions` is used by `RegisterSubscriber` when a registration does not supply its own options. When omitted, the manager uses `new SubscriberOptions()`, so the default queue size is `SubscriberOptions.DefaultMaxQueueSize` (1000) and dispatch is `Sequential`. See [`SubscriberOptions`](SubscriberOptions.md).

Registration does not start anything. Channels begin consuming only after `StartAll()`.

### `RegisterSubscriber<T>`

- `eventName` must be non-null and non-whitespace, otherwise `ArgumentException` is thrown.
- `handler` must be non-null, otherwise `ArgumentNullException` is thrown.
- The handler is wrapped as `args => handler((T)args)`, so a posted event whose runtime type is not `T` faults that subscriber's handling with an `InvalidCastException`, not the registration.
- The diagnostic name is `options?.Name`, then `handler.Method.DeclaringType?.Name`, then `"AnonymousSubscriber"`. Note that the name is taken from the `options` argument itself, not from the effective options, so a `Name` set only on `defaultOptions` is not used.
- Each call adds a new channel. Registering the same handler twice creates two channels, and both receive every event.

## `PostEventAsync`

```csharp
public async Task PostEventAsync(string eventName, PipelineEventArgs args)
```

1. Validates `eventName` (`ArgumentException.ThrowIfNullOrWhiteSpace`) and `args` (`ArgumentNullException.ThrowIfNull`).
2. If the manager is disposed, logs a warning and returns without throwing.
3. Looks up the channel list for `eventName`. If there is none, it returns without doing anything.
4. Copies the list under the list's lock, so later registrations do not affect this post.
5. Starts one `Task.Run` per channel that calls `channel.TryPost(args)`, then awaits `Task.WhenAll`.

The returned task completes once every enqueue attempt has finished. It does not wait for handlers. The `bool` returned by each `TryPost` is not inspected, so a queue-full result (see the queue behavior table in [`SubscriberChannel`](SubscriberChannel.md)) is not reported to the caller or logged by the manager.

Because each post schedules a `Task.Run` per subscriber, posting is cheap for handlers but not free for the thread pool. Publishing at very high rates with many subscribers increases that overhead.

## `StartAll`

```csharp
public void StartAll()
```

- Synchronous; it does not return a task.
- Calls `Start()` on every channel for every event name. A failure while starting one channel is logged at error level and the loop continues with the remaining channels.
- After disposal it logs a warning and returns.
- Calling it more than once is not guarded by the manager. Each channel's `Start()` decides what happens when it is already running (it logs a warning and keeps the existing consumer).

The completion message is logged as `Started all subscriber channels ({ChannelCount} total)`, but the value passed is the number of **event names** registered (`_channelsByEvent.Count`), not the number of channels. Treat that figure as an event count when reading logs.

## `StopAllAsync`

```csharp
public async Task StopAllAsync()
```

- Returns immediately if the manager is disposed, without logging.
- Collects `StopAsync()` tasks for every channel and awaits them with `Task.WhenAll`, so channels stop concurrently.
- Stopping does not dispose the channels. After `StopAllAsync()` the registrations are still present, so `GetSubscriberCount` still reports them. Posting after a stop is not supported by the channels (their writers are completed); see `SubscriberChannel.StopAsync()`.
- Logs `Stopped all subscriber channels` when all stops have completed.

## `GetTotalErrorCount`

```csharp
public int GetTotalErrorCount(string eventName)
```

- Validates `eventName`.
- Returns the sum of `GetErrorCount()` across the channels for `eventName`, computed while holding the list lock.
- Returns `0` for an unknown event name, and also after disposal (the registry is cleared).
- The count only includes non-cancellation handler exceptions, as described in [`SubscriberChannel`](SubscriberChannel.md#error-policies).

## `GetSubscriberCount`

```csharp
public int GetSubscriberCount(string eventName)
```

- Validates `eventName`.
- Returns the number of channels registered for `eventName` under the list lock, or `0` if the event is unknown or the manager has been disposed.
- Duplicate registrations of the same handler are counted separately.

## Disposal

Disposal is implemented by `DisposeAsync()`, and `Dispose()` is a synchronous wrapper around it.

`DisposeAsync()`:

1. Returns immediately if already disposed.
2. Awaits `StopAllAsync()`. Any exception from it is swallowed; disposal is best effort.
3. Calls `Dispose()` on every channel, holding each list's lock while doing so.
4. Clears the registry and sets the internal `_disposed` flag.

After disposal:

- `PostEventAsync` logs a warning and returns.
- `StartAll` logs a warning and returns.
- `StopAllAsync` returns without logging.
- `GetTotalErrorCount` and `GetSubscriberCount` return `0`.
- A second `DisposeAsync()` or `Dispose()` call has no effect.

Things to be aware of:

- `Dispose()` blocks the calling thread with `GetAwaiter().GetResult()`. Prefer `DisposeAsync()` where an asynchronous caller is available. The awaits inside use `ConfigureAwait(false)`, which keeps the continuation off the captured context, but a blocking caller still occupies its thread until shutdown completes.
- `RegisterSubscriber` does not check `_disposed`. A registration made after disposal adds a channel that will never be started or disposed by this manager. Callers should stop registering before disposing.
- Disposal sets `_disposed` only after the work is done, so a concurrent `DisposeAsync()` call may run the shutdown path more than once. Callers should dispose the manager once, which is what the owning publisher does.

## Thread-safety

- The event-name registry is a `ConcurrentDictionary<string, List<SubscriberChannel>>`. Adding a new event name uses `GetOrAdd`, so two registrations for the same new event cannot create two lists.
- Each channel list is guarded by a `lock` on the list instance. Registration, snapshotting in `PostEventAsync` and `StopAllAsync`, error and count queries, and the per-channel disposal in `DisposeAsync` all take that lock.
- `PostEventAsync` takes a snapshot under the lock and then posts outside it, so a slow enqueue never holds the lock.
- `StartAll` iterates the dictionary directly and copies each list under its lock.
- `_disposed` is a plain `bool` with no `volatile` or `Interlocked` access. Reads in `PostEventAsync`, `StartAll`, and `StopAllAsync` are not synchronized with the write in `DisposeAsync`. A post that overlaps with disposal may therefore be accepted by a channel that is being disposed; the channel itself then rejects it (see [`SubscriberChannel`](SubscriberChannel.md)).
- No lock spans registration and disposal, so the caveat about registering after disposal above applies to concurrent use as well.

In practice the manager is safe for concurrent posts, counts, and registrations for different events, and for concurrent posts against a fixed set of subscribers. Shutdown should be sequenced after producers stop posting.

## Example

The type is `internal`, so the example applies to code inside the same assembly (for example, the publisher or tests granted internal visibility). It shows the lifecycle end to end:

```csharp
using DotNetRealtimePipeline.Events;
using Microsoft.Extensions.Logging;

ILogger logger = loggerFactory.CreateLogger("SubscriberChannelManager");

// Default options apply to registrations that pass no options.
var defaults = new SubscriberOptions
{
    MaxQueueSize = 500,
    MaxQueueSizeBehavior = MaxQueueSizeBehavior.DropNew,
    DispatchMode = SubscriberDispatchMode.Sequential,
    ErrorPolicy = SubscriberErrorPolicy.SwallowAndCount
};

var manager = new SubscriberChannelManager(logger, defaults);

// Two subscribers for the same event. The first gets a custom name for diagnostics
// and a parallel dispatch mode; the second inherits the defaults.
manager.RegisterSubscriber<DataIngestedEventArgs>(
    "DataIngested",
    async args => await IndexAsync(args.DataPoint),
    new SubscriberOptions
    {
        Name = "Indexer",
        DispatchMode = SubscriberDispatchMode.Parallel,
        MaxDegreeOfParallelism = 4,
        ErrorPolicy = SubscriberErrorPolicy.FailFast
    });

manager.RegisterSubscriber<DataIngestedEventArgs>(
    "DataIngested",
    args =>
    {
        metrics.Record(args.DataPoint);
        return Task.CompletedTask;
    });

manager.StartAll();

// Producers post without waiting for handlers.
await manager.PostEventAsync("DataIngested", new DataIngestedEventArgs
{
    DataPoint = dataPoint,
    Timestamp = DateTime.UtcNow
});

// Diagnostics.
int subscribers = manager.GetSubscriberCount("DataIngested"); // 2
int failures = manager.GetTotalErrorCount("DataIngested");

// Graceful shutdown: stop consumers, then release resources.
await manager.StopAllAsync();
await manager.DisposeAsync();
```

`IndexAsync`, `metrics`, `dataPoint`, and `loggerFactory` are placeholders for application code. With `FailFast`, an exception from the indexer faults only that subscriber's consumer, as described in [`SubscriberChannel`](SubscriberChannel.md#error-policies); the metrics subscriber keeps running.

## Related

- [`SubscriberChannel`](SubscriberChannel.md) - the per-subscriber queue, consumer, and error policies managed here.
- [`SubscriberOptions`](SubscriberOptions.md) - queue size, dispatch mode, concurrency, error policy, and name.
- [`PipelineEventPublisher`](PipelineEventPublisher.md) - the public owner of a manager instance.
