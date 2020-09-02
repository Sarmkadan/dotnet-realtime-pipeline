# BackpressureService

A service that implements backpressure mechanisms to control data flow in real-time processing pipelines, preventing buffer overflows by dynamically managing consumer registration, buffer capacity, and throttling strategies.

## API

### `BackpressureContext CreateContext()`
Creates a new backpressure context for managing flow control in a specific processing stage. The context tracks buffer occupancy, consumer registrations, and applied backpressure state. Returns a new `BackpressureContext` instance initialized with default thresholds and zeroed counters.

### `BackpressureContext? GetContext()`
Retrieves the current active backpressure context, if one exists. Returns `null` if no context has been created or if the system has been reset. The returned context reflects the latest buffer occupancy, consumer registrations, and backpressure state.

### `bool TryAddToBuffer()`
Attempts to add an item to the internal buffer. Returns `true` if the item was accepted; returns `false` if the buffer is full or backpressure is actively applied. This operation is atomic and thread-safe.

### `void RemoveFromBuffer()`
Removes an item from the internal buffer. Must only be called when an item is successfully processed and removed from the buffer. Not thread-safe; caller must ensure exclusive access to the buffer.

### `async Task<BackpressureResponse> ApplyBackpressureAsync()`
Applies backpressure to the system by throttling producers and signaling registered consumers to slow down. Returns a `BackpressureResponse` indicating whether backpressure was successfully applied and the strategy used. This method is idempotent and thread-safe.

### `bool IsBackpressured`
Gets a value indicating whether backpressure is currently active in the system. Returns `true` if `ApplyBackpressureAsync` has been called and not yet reset; otherwise, returns `false`.

### `bool TryRegisterConsumer()`
Registers a new consumer with the backpressure system. Returns `true` if registration succeeded; returns `false` if the system is already at maximum consumer capacity or if backpressure is applied. Thread-safe.

### `void UnregisterConsumer()`
Removes a consumer from the backpressure system. Must only be called when a consumer is no longer active. Not thread-safe; caller must ensure exclusive access to the consumer registry.

### `BackpressureSystemStatus GetSystemStatus()`
Retrieves the current system-wide status, including buffer occupancy, consumer count, and backpressure state. Returns a snapshot of the system’s health and capacity usage.

### `void ResetBackpressure()`
Resets the backpressure system to its initial state, clearing all buffer contents, consumer registrations, and backpressure flags. Not thread-safe; caller must ensure no concurrent operations are in progress.

### `long GetDroppedItemCount()`
Returns the total number of items dropped due to buffer overflow or backpressure. The count is cumulative and not reset by `ResetBackpressure`.

### `Dictionary<string, long> GetBufferStatus()`
Returns a dictionary mapping buffer names (if applicable) to their current item counts. The dictionary reflects the latest buffer occupancy across all stages.

### `void Clear()`
Clears all buffer contents and resets internal counters, but preserves consumer registrations and backpressure state. Not thread-safe; caller must ensure exclusive access.

### `bool Applied`
Gets a value indicating whether backpressure has been applied to the system. Equivalent to `IsBackpressured`.

### `string Reason`
Gets the reason backpressure was applied, if applicable. Returns an empty string if no backpressure is active.

### `double BufferFillPercent`
Gets the current buffer occupancy as a percentage of total capacity. Returns `0.0` if no buffer is in use or if the system is reset.

### `string StrategyUsed`
Gets the name of the backpressure strategy currently in use (e.g., "fixed", "adaptive"). Returns an empty string if no strategy is active.

### `int TotalStages`
Gets the total number of processing stages registered with the backpressure system.

### `int BackpressuredStages`
Gets the number of stages currently under backpressure.

### `double AverageBufferFillPercent`
Gets the average buffer occupancy percentage across all registered stages. Returns `0.0` if no stages are registered or the system is reset.

## Usage
