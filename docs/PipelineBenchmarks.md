# PipelineBenchmarks

The `PipelineBenchmarks` class provides a suite of benchmarking utilities for evaluating the performance characteristics of real-time data processing pipelines in C#. It measures throughput, memory allocation, backpressure handling, and windowing operations under controlled conditions, enabling comparison of different pipeline configurations or implementations.

## API

### `public void Setup()`

Initializes the benchmarking environment before each benchmark run. This method configures default pipeline settings, resets internal state, and prepares any required test data structures. It should be called once per benchmark iteration.

**Parameters:** None
**Return value:** None
**Exceptions:** Throws `InvalidOperationException` if the pipeline is already initialized or if required resources cannot be allocated.

---

### `public void Cleanup()`

Releases resources and resets the benchmarking environment after each benchmark run. This method cleans up any dynamically allocated buffers, disposes of pipeline components, and ensures a clean state for subsequent benchmarks.

**Parameters:** None
**Return value:** None
**Exceptions:** Throws `ObjectDisposedException` if called on a disposed benchmark instance.

---

### `public async Task IngestSingleDataPoint()`

Simulates the ingestion of a single data point into the pipeline. Measures the latency and overhead of processing one unit of data through the pipeline’s input stage.

**Parameters:** None
**Return value:** `Task` representing the asynchronous operation.
**Exceptions:** Throws `InvalidOperationException` if the pipeline is not initialized or if ingestion is not supported in the current configuration.

---

### `public async Task ProcessBatch()`

Processes a predefined batch of data points through the pipeline. Measures throughput and CPU utilization when handling multiple data points in a single batch operation.

**Parameters:** None
**Return value:** `Task` representing the asynchronous operation.
**Return value type:** `Task`
**Exceptions:** Throws `InvalidOperationException` if the pipeline is not initialized or if batch processing is disabled.

---

### `public void ProcessDataPointsThroughWindowing()`

Evaluates the performance of windowing operations within the pipeline. Measures how efficiently the pipeline aggregates, emits, or transforms data over time-based or count-based windows.

**Parameters:** None
**Return value:** None
**Exceptions:** Throws `InvalidOperationException` if windowing is not configured or if the pipeline is not initialized.

---

### `public async Task GenerateHealthReport()`

Generates a performance and health report for the current pipeline state. Includes metrics such as throughput, latency percentiles, memory usage, and backpressure indicators.

**Parameters:** None
**Return value:** `Task` representing the asynchronous operation.
**Exceptions:** Throws `InvalidOperationException` if the pipeline has not been executed or if reporting data is unavailable.

---
### `public void BackpressureBufferOperations()`

Tests the pipeline’s behavior under backpressure conditions by simulating high load and measuring buffer growth, spillover, and recovery behavior.

**Parameters:** None
**Return value:** None
**Exceptions:** Throws `InvalidOperationException` if backpressure monitoring is not enabled or if the pipeline is not running.

---
### `public async Task EndToEndThroughput()`

Measures the end-to-end throughput of the pipeline by ingesting a large dataset and measuring the time to complete processing. Includes serialization, transformation, and output stages.

**Parameters:** None
**Return value:** `Task` representing the asynchronous operation.
**Exceptions:** Throws `InvalidOperationException` if the pipeline is not initialized or if required data sources are unavailable.

---
### `public async Task MemoryAllocationBenchmark()`

Quantifies memory allocations during pipeline execution, including object allocations, buffer usage, and garbage collection pressure. Useful for identifying memory leaks or high allocation hotspots.

**Parameters:** None
**Return value:** `Task` representing the asynchronous operation.
**Exceptions:** Throws `InvalidOperationException` if memory tracking is not enabled or if the pipeline is not running.

---

## Usage

### Example 1: Basic Benchmarking Loop
