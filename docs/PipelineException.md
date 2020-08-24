# PipelineException

`PipelineException` is a base exception type used within the `dotnet-realtime-pipeline` project to represent errors that occur during pipeline processing. It provides structured error information to facilitate debugging and error handling in real-time data processing scenarios.

## API

### `public string? ErrorCode`
A string identifier for the type of error that occurred. This code can be used to programmatically handle specific error conditions in a consistent manner across the pipeline.

### `public object? ErrorDetails`
An optional payload containing additional context about the error. This may include structured data such as input records, configuration values, or intermediate state that contributed to the failure.

### `public PipelineException(string message) : base(message)`
Constructs a new `PipelineException` with a human-readable error message. This is the primary constructor for creating exceptions with descriptive context.

### `public PipelineException`
Default constructor. Creates an exception with no message or additional context.

### `public PipelineException(string message, Exception? innerException)`
Constructs a new `PipelineException` with a message and an inner exception that caused this exception.

### `public PipelineException(Exception? innerException)`
Constructs a new `PipelineException` with an inner exception and no message.

### `public InvalidDataPointException`
A derived exception type indicating that a data point processed by the pipeline was invalid according to schema or validation rules.

### `public InvalidDataPointException(string message)`
Constructs an `InvalidDataPointException` with a descriptive message about the invalid data.

### `public InvalidDataPointException(string message, Exception? innerException)`
Constructs an `InvalidDataPointException` with a message and an inner exception.

### `public long BufferSize`
A derived exception property indicating the configured buffer size at the time of failure, relevant when buffer overflows or capacity issues occur.

### `public long MaxCapacity`
A derived exception property indicating the maximum allowed capacity of a buffer or collection when an overflow or capacity-related error occurs.

### `public BackpressureException`
A derived exception type indicating that backpressure mechanisms were triggered due to downstream consumers being unable to keep up with the data rate.

### `public string? StageName`
A derived exception property identifying the pipeline stage where the error occurred. Useful for isolating failures to specific processing components.

### `public int RetryCount`
A derived exception property indicating how many retry attempts were made before the error occurred, relevant in retryable operation contexts.

### `public StageProcessingException`
A derived exception type indicating that an error occurred during the processing logic of a specific pipeline stage.

### `public StageProcessingException(string message)`
Constructs a `StageProcessingException` with a message describing the processing failure.

### `public StageProcessingException(string message, Exception? innerException)`
Constructs a `StageProcessingException` with a message and an inner exception.

### `public long WindowId`
A derived exception property identifying the window or batch identifier associated with the failure, used in windowed or stateful processing.

### `public WindowingException`
A derived exception type indicating that an error occurred during windowing operations, such as window creation, aggregation, or eviction.

### `public WindowingException(string message)`
Constructs a `WindowingException` with a message describing the windowing failure.

### `public WindowingException(string message, Exception? innerException)`
Constructs a `WindowingException` with a message and an inner exception.

### `public long TimeoutMs`
A derived exception property indicating the timeout duration in milliseconds that was exceeded, relevant for operations with bounded execution time.

### `public ProcessingTimeoutException`
A derived exception type indicating that a processing operation exceeded its allowed execution time.
