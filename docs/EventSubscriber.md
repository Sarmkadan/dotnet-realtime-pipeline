# Event subscribers

`src/Events/EventSubscriber.cs` defines the subscriber-side building blocks for the pipeline event system. `EventSubscriberBase` holds the shared publisher and logger, while each concrete subscriber registers one asynchronous handler with a [`PipelineEventPublisher`](PipelineEventPublisher.md).

For related higher-level subscriber APIs, see [`EventSubscriberBase`](EventSubscriberBase.md).

## Lifecycle

All subscriber constructors require a `PipelineEventPublisher` and a typed `ILogger<TSubscriber>`. The base constructor throws `ArgumentNullException` when either dependency is `null`.

### `Subscribe()`

`EventSubscriberBase.Subscribe()` marks the instance as subscribed and writes an information log. Each concrete override calls the base method and then registers its handler with the publisher using the event name, event-argument type, and subscriber options.

The default options are:

- `Name`: the runtime subscriber type name
- `MaxQueueSize`: `1000`
- `MaxQueueSizeBehavior`: `DropNew`
- `DispatchMode`: `Sequential`
- `ErrorPolicy`: `SwallowAndCount`
- `MaxDegreeOfParallelism`: `1`

`BackpressureAlertSubscriber` and `ErrorAlertSubscriber` override these options with `MaxQueueSize = int.MaxValue` and `MaxQueueSizeBehavior = Block`, providing a non-dropping alert path.

Calling `Subscribe()` more than once registers the handler more than once; the base `_isSubscribed` flag is not used to prevent duplicate registration.

### `Unsubscribe()`

`Unsubscribe()` clears the base `_isSubscribed` flag and writes an information log. It does not unregister the handler from `PipelineEventPublisher`, so it does not stop delivery to an already registered subscriber.

### `Start()` and `StopAsync()`

The base implementations are extension points and currently do no work. `StopAsync()` returns a completed task. Event-channel processing is controlled by `PipelineEventPublisher.Start()` and `PipelineEventPublisher.StopAsync()`, not by these subscriber methods.

A typical lifecycle is therefore:

```csharp
var subscriber = new ErrorAlertSubscriber(publisher, errorLogger);

subscriber.Subscribe();
publisher.Start();

await publisher.PublishPipelineErrorAsync(
    "PersistReading",
    new InvalidOperationException("Storage unavailable"));

await publisher.StopAsync();
```

## Concrete subscribers

### `DataIngestSubscriber`

Registers for `DataIngestedEvent` messages with `DataIngestedEventArgs`. Its handler validates the supplied `DataPoint` before calling the protected virtual `ProcessIngestedDataAsync(DataPoint)` extension point.

Validation rejects a missing event or data point, a data point that fails `DataPoint.Validate()`, timestamps more than 24 hours in the future or 10 days in the past, non-finite values, and values outside `-10000` through `10000`. `MaxBatchSize` is exposed as `10000`, although the handler processes one data point per event.

### `ProcessingCompletionSubscriber`

Registers for `ProcessingCompletedEvent` messages with `ProcessingCompletedEventArgs`. It increments an internal success or failure count, logs the outcome, and calls the protected virtual `OnProcessingCompletionAsync(ProcessingResult)` extension point. Exceptions raised while handling an event are caught and logged.

#### `double GetSuccessRatePercent()`

Returns successful completions as a percentage of all counted completions:

```text
success count * 100 / (success count + failure count)
```

It returns `100.0` before any completion events have been counted.

### `BackpressureAlertSubscriber`

Registers for `BackpressureDetectedEvent` messages with `BackpressureDetectedEventArgs`. It counts events, records the timestamp of the first event internally, and logs buffer utilization. When utilization is greater than 95 percent, it calls the protected virtual `OnCriticalBackpressureAsync(BackpressureDetectedEventArgs)` extension point. Handler exceptions are caught and logged.

#### `int GetBackpressureEventCount()`

Returns the number of backpressure events handled by this instance. A new subscriber returns `0`.

### `MetricsAggregationSubscriber`

Registers for `MetricsCollectedEvent` messages with `MetricsCollectedEventArgs`. This sealed subscriber accumulates each event's `AverageProcessingTimeMs` and a metric-event count in striped counters using `Interlocked` operations. Handler exceptions are caught and logged.

#### `double GetAverageProcessingTime()`

Takes a metrics snapshot and returns total accumulated processing time divided by the number of collected metric events. It returns `0.0` when the snapshot contains no metrics.

#### `int GetMetricsCount()`

Returns the number of metric events in the current snapshot.

#### `void Reset()`

Replaces every counter stripe and the cached snapshot with empty values. After a reset, the reported count and average are zero until subsequent metrics are reflected in a new snapshot.

> Implementation note: the current snapshot validity check treats the initial zero-count snapshot as valid. Consequently, the public getters can continue returning their cached zero values even after the striped counters receive events.

### `ErrorAlertSubscriber`

Registers for `PipelineErrorEvent` messages with `PipelineErrorEventArgs`. It increments its error count, logs the operation and exception, and calls the protected virtual `OnErrorDetectedAsync(PipelineErrorEventArgs)` extension point. Handler exceptions are caught and logged.

#### `int GetErrorCount()`

Returns the number of pipeline error events handled by this instance. A new subscriber returns `0`.

## Concurrency and error handling

The publisher dispatches through subscriber-specific channels. The processing-completion, backpressure, and error counters are ordinary fields rather than atomic counters, while the metrics subscriber uses striped `Interlocked` updates. The default sequential dispatch mode limits each registered handler to one event at a time, but callers should not assume every public metric is an atomic snapshot if they change dispatch settings or access an instance concurrently.

Most concrete handlers catch and log their own exceptions. `DataIngestSubscriber` deliberately allows validation and processing exceptions to reach the publisher's subscriber channel, where the configured `SwallowAndCount` policy handles them.
