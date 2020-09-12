# BatchProcessor

A utility class for partitioning input data into batches and processing them asynchronously, tracking progress and performance metrics. Designed for scenarios requiring controlled batching of large datasets with progress monitoring and fault tolerance.

## API

### `public BatchProcessor`
Initializes a new instance of the `BatchProcessor` class. No parameters are required; all configuration is done via method calls.

### `public async Task<List<TOutput>> ProcessAsync<TInput, TOutput>(IEnumerable<TInput> input, Func<List<TInput>, Task<List<TOutput>>> batchProcessor)`
Processes the provided input data in batches using the supplied processing function. Returns a list of outputs collected from all batches.

- **Parameters**
  - `input`: The collection of input items to be processed.
  - `batchProcessor`: An asynchronous function that processes a batch of inputs and returns a list of outputs.
- **Return Value**: A `Task` that resolves to a list of outputs from all batches.
- **Exceptions**: Throws `BatchProcessingException` if processing fails for any batch.

### `public IEnumerable<List<TInput>> CreateBatches<TInput>(IEnumerable<TInput> input, int batchSize)`
Partitions the input collection into sublists of the specified size.

- **Parameters**
  - `input`: The collection to partition.
  - `batchSize`: The maximum number of items per batch.
- **Return Value**: An enumerable sequence of batches, each a list of inputs.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `batchSize` is less than 1.

### `public int GetBatchCount<TInput>(IEnumerable<TInput> input, int batchSize)`
Calculates the total number of batches that would be created for the given input and batch size.

- **Parameters**
  - `input`: The collection to evaluate.
  - `batchSize`: The batch size used for partitioning.
- **Return Value**: The total number of batches.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `batchSize` is less than 1.

### `public BatchProcessingException`
A custom exception thrown when batch processing encounters unrecoverable errors.

### `public int TotalBatches`
Gets the total number of batches to be processed.

### `public int ProcessedBatches`
Gets the number of batches that have been successfully processed.

### `public int TotalItems`
Gets the total number of input items across all batches.

### `public int ProcessedItems`
Gets the total number of input items that have been successfully processed.

### `public DateTime StartTime`
Gets the timestamp when processing began.

### `public DateTime LastUpdateTime`
Gets the timestamp of the last progress update.

### `public override string ToString()`
Returns a human-readable summary of processing progress, including batch and item counts, start time, and last update time.

### `public DataPointBatchProcessor`
A specialized `BatchProcessor` subclass for processing `DataPoint` objects, returning `ProcessingResult` objects.

### `public async Task<List<Domain.Models.ProcessingResult>> ProcessBatchAsync(IEnumerable<Domain.Models.DataPoint> input)`
Processes a sequence of `DataPoint` objects in batches, returning a list of processing results.

- **Parameters**
  - `input`: The collection of `DataPoint` objects to process.
- **Return Value**: A `Task` that resolves to a list of `ProcessingResult` objects.
- **Exceptions**: Throws `BatchProcessingException` if processing fails.

### `public IEnumerable<List<Domain.Models.DataPoint>> CreateBatches(IEnumerable<Domain.Models.DataPoint> input, int batchSize)`
Partitions a sequence of `DataPoint` objects into batches of the specified size.

- **Parameters**
  - `input`: The collection of `DataPoint` objects to partition.
  - `batchSize`: The maximum number of items per batch.
- **Return Value**: An enumerable sequence of batches, each a list of `DataPoint` objects.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `batchSize` is less than 1.

## Usage

### Example 1: Basic Batch Processing
```csharp
var data = Enumerable.Range(1, 1000).ToList();
var processor = new BatchProcessor();

var results = await processor.ProcessAsync(
    data,
    async batch => await Task.FromResult(batch.Select(x => x * 2).ToList())
);

Console.WriteLine($"Processed {results.Count} items.");
```

### Example 2: Specialized DataPoint Processing
```csharp
var points = Enumerable.Range(1, 500)
    .Select(i => new Domain.Models.DataPoint { Id = i, Value = i * 10.5 })
    .ToList();

var dataProcessor = new DataPointBatchProcessor();
var processingResults = await dataProcessor.ProcessBatchAsync(points);

Console.WriteLine($"Processed {processingResults.Count} data points.");
```

## Notes

- Batch sizes must be positive integers; zero or negative values will throw `ArgumentOutOfRangeException`.
- Progress metrics (`TotalBatches`, `ProcessedBatches`, etc.) are updated during processing and reflect the latest state.
- `ProcessAsync` and `ProcessBatchAsync` are thread-safe for concurrent calls, but progress tracking reflects the most recent operation.
- If a batch fails, a `BatchProcessingException` is thrown immediately; no partial results are returned.
- `LastUpdateTime` reflects the most recent progress change, which may lag slightly behind actual processing due to asynchronous timing.
- The `DataPointBatchProcessor` is not thread-safe across multiple concurrent operations; reuse across threads requires external synchronization.
