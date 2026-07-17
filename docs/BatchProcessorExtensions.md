# BatchProcessorExtensions

Provides extension methods for batching, processing, and analyzing sequences of data with configurable batch sizes and aggregation. Designed for high-throughput pipelines where memory efficiency and predictable latency are required.

## API

### `ProcessAsync<TInput, TOutput>`
```csharp
public static async Task<IReadOnlyList<TOutput>> ProcessAsync<TInput, TOutput>(
    this IEnumerable<TInput> source,
    Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> processor,
    int batchSize = 1000)
```
Processes the input sequence in batches using the provided async processor. Each batch is passed to the processor function, which returns a collection of outputs. The results are aggregated into a single read-only list.

- **source**: The input sequence to process.
- **processor**: Async function that processes a batch and returns outputs.
- **batchSize**: Maximum number of items per batch (default: 1000).
- **Returns**: A task that resolves to a read-only list of all output items.
- **Throws**: `ArgumentNullException` if `source` or `processor` is null. `ArgumentOutOfRangeException` if `batchSize` ≤ 0.

---

### `BatchSelect<TInput, TOutput, TResult>`
```csharp
public static IEnumerable<TResult> BatchSelect<TInput, TOutput, TResult>(
    this IEnumerable<TInput> source,
    Func<IEnumerable<TInput>, IEnumerable<TOutput>> selector,
    Func<TOutput, TResult> resultSelector,
    int batchSize = 1000)
```
Transforms each batch of inputs into outputs and then maps each output to a final result. Useful when intermediate outputs are required for final projection.

- **source**: The input sequence to transform.
- **selector**: Synchronous function that converts a batch of inputs into outputs.
- **resultSelector**: Function that maps each output to the final result type.
- **batchSize**: Maximum number of items per batch (default: 1000).
- **Returns**: An enumerable of final results.
- **Throws**: `ArgumentNullException` if `source`, `selector`, or `resultSelector` is null. `ArgumentOutOfRangeException` if `batchSize` ≤ 0.

---

### `ProcessAsync<TInput, TOutput, TAggregate>`
```csharp
public static async Task<TAggregate> ProcessAsync<TInput, TOutput, TAggregate>(
    this IEnumerable<TInput> source,
    Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> processor,
    Func<TAggregate, TOutput, Task<TAggregate>> aggregator,
    TAggregate seed,
    int batchSize = 1000)
```
Processes input in batches and aggregates results incrementally using an async aggregator. Suitable for reducing large datasets into a single state (e.g., sum, count, model).

- **source**: The input sequence to process.
- **processor**: Async function that processes a batch and returns outputs.
- **aggregator**: Async function that merges each output into the aggregate.
- **seed**: Initial value of the aggregate.
- **batchSize**: Maximum number of items per batch (default: 1000).
- **Returns**: A task that resolves to the final aggregated value.
- **Throws**: `ArgumentNullException` if `source`, `processor`, or `aggregator` is null. `ArgumentOutOfRangeException` if `batchSize` ≤ 0.

---

### `GetEstimatedProcessingTime<TInput, TOutput>`
```csharp
public static TimeSpan GetEstimatedProcessingTime<TInput, TOutput>(
    this IEnumerable<TInput> source,
    Func<IEnumerable<TInput>, Task<IEnumerable<TOutput>>> processor,
    int batchSize = 1000)
```
Estimates total processing time by measuring the duration of a single batch and scaling linearly. Useful for SLA planning and backpressure tuning.

- **source**: The input sequence (used only to infer count).
- **processor**: Async function used to process a batch.
- **batchSize**: Batch size to simulate (default: 1000).
- **Returns**: Estimated total time to process all items.
- **Throws**: `ArgumentNullException` if `source` or `processor` is null. `ArgumentOutOfRangeException` if `batchSize` ≤ 0.

---

### `ProcessBatchAsync`
```csharp
public static async Task<IReadOnlyList<Domain.Models.ProcessingResult>> ProcessBatchAsync(
    this IEnumerable<Domain.Models.DataPoint> source,
    Func<IEnumerable<Domain.Models.DataPoint>, Task<IEnumerable<Domain.Models.DataPoint>>> processor,
    int batchSize = 1000)
```
Processes a sequence of `DataPoint` items in batches and returns detailed processing results per batch, including success/failure counts and timing metadata.

- **source**: Sequence of data points to process.
- **processor**: Async function that processes a batch of data points.
- **batchSize**: Maximum items per batch (default: 1000).
- **Returns**: A task that resolves to a read-only list of processing results, one per batch.
- **Throws**: `ArgumentNullException` if `source` or `processor` is null. `ArgumentOutOfRangeException` if `batchSize` ≤ 0.

---

### `CreateBatches`
```csharp
public static IEnumerable<List<Domain.Models.DataPoint>> CreateBatches(
    this IEnumerable<Domain.Models.DataPoint> source,
    int batchSize = 1000)
```
Partitions a sequence of `DataPoint` items into lists of the specified size. The last batch may be smaller.

- **source**: Sequence of data points to batch.
- **batchSize**: Maximum number of items per batch (default: 1000).
- **Returns**: An enumerable of batches (each a `List<DataPoint>`).
- **Throws**: `ArgumentNullException` if `source` is null. `ArgumentOutOfRangeException` if `batchSize` ≤ 0.

---

### `GetBatchStatistics<T, TOutput>`
```csharp
public static BatchStatistics GetBatchStatistics<T, TOutput>(
    this IEnumerable<T> source,
    Func<IEnumerable<T>, IEnumerable<TOutput>> selector,
    int batchSize = 1000)
```
Computes statistics about how the source sequence is divided into batches: total items, total batches, last batch size, and whether the last batch is full.

- **source**: The input sequence.
- **selector**: Function that transforms each batch (used only to infer batching behavior).
- **batchSize**: Expected batch size (default: 1000).
- **Returns**: A `BatchStatistics` object with `TotalItems`, `TotalBatches`, `BatchSize`, `LastBatchSize`, and `IsPerfectFit`.

---

### `TotalItems` (property)
```csharp
public int TotalItems { get; }
```
Gets the total number of items processed across all batches.

---

### `TotalBatches` (property)
```csharp
public int TotalBatches { get; }
```
Gets the total number of batches created.

---

### `BatchSize` (property)
```csharp
public int BatchSize { get; }
```
Gets the configured batch size used during processing.

---

### `LastBatchSize` (property)
```csharp
public int LastBatchSize { get; }
```
Gets the number of items in the last batch. Equal to `BatchSize` if the total items are evenly divisible.

---

### `IsPerfectFit` (property)
```csharp
public bool IsPerfectFit { get; }
```
Indicates whether all batches are exactly `BatchSize` in length (i.e., no partial batch).

---

### `ToString` (override)
```csharp
public override string ToString()
```
Returns a human-readable summary of batch statistics including total items, batches, and last batch size.

---

## Usage

### Example 1: Async batch processing with aggregation
```csharp
var dataPoints = LoadDataPoints(); // IEnumerable<DataPoint>
var totalValue = await dataPoints.ProcessAsync(
    async batch => await TransformBatchAsync(batch),
    (agg, output) => agg + output.Value,
    seed: 0.0,
    batchSize: 500
);

Console.WriteLine($"Total aggregated value: {totalValue}");
```

### Example 2: Batch processing with detailed results
```csharp
var results = await dataPoints.ProcessBatchAsync(
    async batch => await CleanseAndValidateAsync(batch),
    batchSize: 250
);

foreach (var result in results)
{
    Console.WriteLine($"Batch {result.BatchIndex}: " +
                     $"{result.SuccessCount} succeeded, " +
                     $"{result.FailureCount} failed, " +
                     $"took {result.Duration.TotalMilliseconds}ms");
}
```

## Notes

- **Memory efficiency**: Batches are materialized as lists only during processing. The `IEnumerable<T>` pipeline remains lazy until enumeration.
- **Thread safety**: All methods are thread-safe with respect to their inputs. Concurrent calls to the same extension method on different sequences are safe. However, the same sequence should not be processed concurrently unless externally synchronized.
- **Edge cases**:
  - Empty sequences return empty results or zero aggregates without error.
  - When `batchSize` exceeds the total items, a single batch is created.
  - If `processor` throws, the entire operation fails; no partial results are returned.
- **Performance**: Avoid large `batchSize` values when processing CPU-bound work to prevent thread pool starvation. Prefer smaller batches for I/O-bound work to maximize throughput.
