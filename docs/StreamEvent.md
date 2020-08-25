# StreamEvent

`StreamEvent` is a transport and processing unit in a real-time data pipeline. It carries a payload through successive processing stages, tracks its own lifecycle (creation, completion, retries), and records the path it has taken. The type is intended to be immutable except for the small set of members that record processing state, allowing safe sharing across threads and stages.

## API

### `public long EventId`
Unique identifier for the event within the pipeline. Assigned once when the event is created and never changed afterwards.

### `public long DataPointId`
Identifier of the underlying data point that this event represents. Used to correlate events back to the source data.

### `public long Timestamp`
Unix-epoch millisecond timestamp (UTC) indicating when the original data point was generated. Immutable after construction.

### `public string EventType`
Semantic type of the event (for example, `"Temperature"`, `"Alert"`, `"StateChange"`). Determines which processing stages will handle the event.

### `public int Priority`
Relative priority of the event. Higher values indicate higher urgency; used by the scheduler to order work.

### `public string? SourceSystem`
Optional identifier of the system that produced the original data point. May be `null` if the source is unknown or not tracked.

### `public string? CorrelationId`
Optional identifier used to correlate this event with related events in other systems or workflows. May be `null`.

### `public string? CausationId`
Optional identifier of the event or command that triggered the creation of this event. May be `null`.

### `public Dictionary<string, object> Payload`
Immutable key/value bag containing the actual data of the event. Keys are strings; values may be any serializable object. The dictionary is never mutated after construction.

### `public List<string> ProcessedByStages`
Ordered list of processing stage names that have successfully handled this event. Stages append their name when they complete work. The list is mutable only via the `MarkProcessedByStage` method.

### `public DateTime CreatedAt`
Timestamp indicating when the event was instantiated. Immutable after construction.

### `public DateTime? CompletedAt`
Timestamp indicating when the event finished all required processing. `null` if the event has not yet completed. Set automatically by the pipeline when the last stage finishes.

### `public bool IsRetry`
Indicates whether this event is a retry of a previously failed event. `true` if this is a subsequent attempt; `false` for the initial attempt.

### `public int RetryAttempt`
Number of retry attempts made for this event. Zero for the initial attempt; increments each time the event is re-enqueued after a failure.

### `public string? LastErrorMessage`
Optional error message from the most recent processing failure. `null` if the event has never failed or has completed successfully.

### `public StreamEvent(long eventId, long dataPointId, long timestamp, string eventType, int priority, string? sourceSystem, string? correlationId, string? causationId, Dictionary<string, object> payload)`
Constructs a new `StreamEvent`. All parameters except `sourceSystem`, `correlationId`, `causationId`, and `payload` are required. The constructor makes defensive copies of mutable parameters to ensure immutability of the initial state.

### `public void MarkProcessedByStage(string stageName)`
Appends `stageName` to `ProcessedByStages` if it is not already present. This method is the only way to mutate the processing history; it is safe to call concurrently from multiple threads because the underlying list is append-only and the method uses simple synchronization.

### `public bool HasBeenProcessedByStage(string stageName)`
Returns `true` if `stageName` is present in `ProcessedByStages`; otherwise `false`. Thread-safe read-only operation.

### `public string GetProcessingPath()`
Returns a comma-separated string of stage names in the order they processed this event (for example, `"Ingest,Validate,Enrich"`). Returns an empty string if no stages have processed the event yet. Thread-safe read-only operation.

## Usage
