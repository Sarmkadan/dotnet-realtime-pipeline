# dotnet-realtime-pipeline Benchmarks

This project contains performance benchmarks for the dotnet-realtime-pipeline library using [BenchmarkDotNet](https://benchmarkdotnet.org/).

## Overview

The benchmarks measure critical pipeline operations including:

- **Ingestion throughput**: Single and batch data point ingestion
- **Processing performance**: Data point validation, transformation, and persistence
- **Windowing operations**: Time-based window assignment and statistics calculation
- **Monitoring overhead**: Health report generation and metrics collection
- **Backpressure management**: Buffer capacity checks and flow control
- **Memory allocation**: Object allocation patterns during processing

## Running Benchmarks

### Prerequisites

- .NET 10.0 SDK or later
- dotnet-realtime-pipeline project built

### Build and Run

```bash
# Navigate to benchmarks directory
cd dotnet-realtime-pipeline.Benchmarks

# Restore dependencies
dotnet restore

# Build the project
dotnet build -c Release

# Run all benchmarks (default: summary view)
dotnet run -c Release

# Run specific benchmark category
# Example: Run only ingestion benchmarks
dotnet run -c Release -- --filter "*Ingestion*"

# Run with detailed output
# Example: Export to CSV and JSON
# dotnet run -c Release -- --exporters csv,json --filter "*Throughput*"

# Run with memory diagnostics
# Example: Full memory report
dotnet run -c Release -- --memory
```

### Common Commands

```bash
# Run all benchmarks and save results to files
cd dotnet-realtime-pipeline.Benchmarks
dotnet run -c Release -- --save true

# Export results to markdown
cd dotnet-realtime-pipeline.Benchmarks
dotnet run -c Release -- --exporters markdown

# Run with warmup count and iteration count adjustments
# (for CI environments with limited time)
dotnet run -c Release -- --warmup 3 --iterationCount 5
```

## Benchmark Categories

### 1. Ingestion Benchmarks
Measures the end-to-end latency and throughput for individual data point ingestion.

- **IngestSingleDataPoint**: Single data point ingestion through PipelineOrchestrator
- **IngestBatch**: Batch ingestion with varying batch sizes (100, 1,000, 10,000 items)

### 2. Processing Benchmarks
Measures throughput for batch processing operations.

- **ProcessBatch**: Batch data point processing with varying batch sizes

### 3. Windowing Benchmarks
Measures the performance of windowing operations.

- **ProcessDataPointsThroughWindowing**: Window assignment and statistics calculation

### 4. Monitoring Benchmarks
Measures the overhead of generating comprehensive health reports.

- **GenerateHealthReport**: Health report generation performance

### 5. Backpressure Benchmarks
Measures the performance of buffer management and backpressure strategies.

- **BackpressureBufferOperations**: Buffer capacity checks and management

### 6. Throughput Benchmarks
Measures the maximum sustainable throughput of the entire pipeline.

- **EndToEndThroughput**: End-to-end pipeline throughput test

### 7. Memory Benchmarks
Measures memory allocations during processing operations.

- **MemoryAllocationBenchmark**: Memory allocation patterns

## Configuration

Benchmarks use the following default configuration:

```csharp
var config = new PipelineConfig
{
    PipelineName = "BenchmarkPipeline",
    MaxBufferSize = 100_000,
    MaxConcurrentConsumers = Environment.ProcessorCount,
    WindowSizeMs = 5_000,
    WindowSlideMs = 1_000,
    BackpressureStrategy = BackpressureStrategy.Block,
    EnableMetrics = true,
    ValidateOnIngestion = false,
    EnableQualityAnalysis = false
};
```

To run benchmarks with different configurations, modify the `Setup()` method in `PipelineBenchmarks.cs`.

## Performance Metrics

Each benchmark measures:

- **Mean**: Arithmetic mean of operation duration
- **Error**: Standard error
- **StdDev**: Standard deviation
- **Median**: 50th percentile
- **Gen 0/1/2**: Garbage collection generations
- **Allocated**: Total memory allocated per operation
- **Operations/sec**: Throughput (higher is better)

## Best Practices

1. **Use Release build**: Always run benchmarks with `-c Release` for accurate results
2. **Warm up**: BenchmarkDotNet automatically warms up the runtime
3. **Isolated environment**: Run benchmarks on an otherwise idle system
4. **Multiple runs**: For consistent results, run benchmarks multiple times
5. **Compare configurations**: Use benchmarks to compare different pipeline configurations

## Example Output

```
BenchmarkDotNet=v0.13.12, OS=ubuntu 22.04
Intel Core i7-12700, 1 CPU, 24 logical and 12 physical cores
.NET SDK=10.0.0
  [Host]     : .NET 10.0.10 (10.0.1024.47715), X64 RyuJIT
  Job-HEYJXN : .NET 10.0.10 (10.0.1024.47715), X64 RyuJIT

|              Method | batchSize |     Mean |    Error |   StdDev |  Gen 0 | Allocated |
|-------------------- |---------- |---------: |---------: |---------: |-------: |----------: |
|          ProcessBatch |       100 | 1.234 ms | 0.012 ms | 0.011 ms |  0.123 |   1.2 KB  |
|          ProcessBatch |    1,000 | 9.876 ms | 0.089 ms | 0.083 ms |  1.456 |  12.3 KB  |
|          ProcessBatch |   10,000 | 95.432 ms | 0.987 ms | 0.923 ms | 14.567 | 123.4 KB  |
```

## Integration with CI/CD

Benchmarks can be integrated into your CI/CD pipeline to track performance regressions:

```yaml
# Example GitHub Actions workflow snippet
- name: Run Performance Benchmarks
  run: |
    cd dotnet-realtime-pipeline.Benchmarks
    dotnet run -c Release -- --save true --exporters json
    
    # Compare with baseline (requires previous run artifacts)
    dotnet run -c Release -- --compare previous.json
```

## Troubleshooting

### Benchmarks are too slow
- Reduce the number of iterations in `BenchmarkSwitcher`
- Use `--iterationCount 5` to limit iterations
- Reduce batch sizes in benchmark arguments

### High memory allocations
- Check for unnecessary object allocations in benchmark code
- Review pipeline configuration (disable quality analysis, validation)
- Consider using `ArrayPool<T>` for large collections

### Inconsistent results
- Run on an isolated system
- Increase warmup count: `--warmup 5`
- Disable CPU frequency scaling on your system
- Ensure consistent .NET runtime version

## Additional Resources

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/articles/overview.html)
- [dotnet-realtime-pipeline Documentation](https://github.com/sarmkadan/dotnet-realtime-pipeline)
- [Performance Optimization Guide](https://learn.microsoft.com/en-us/dotnet/core/performance/)

---

**Built by Vladyslav Zaiets | CTO & Software Architect**