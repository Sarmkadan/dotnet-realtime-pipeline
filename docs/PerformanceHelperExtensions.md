# PerformanceHelperExtensions

Provides extension methods for benchmarking and performance analysis of .NET operations, including synchronous and asynchronous code paths, memory pressure detection, and statistical analysis of benchmark results.

## API

### `Benchmark`

```csharp
public static BenchmarkResult Benchmark(
    Action action,
    int iterations = 1,
    int warmupIterations = 0,
    bool throwOnError = true)
```

Measures the execution time of a synchronous action over multiple iterations. Includes optional warmup phase to account for JIT compilation and other startup costs.

- **action**: The delegate to benchmark.
- **iterations**: Number of times to run the action (default: 1).
- **warmupIterations**: Number of warmup runs before actual measurement (default: 0).
- **throwOnError**: Whether to throw if the action throws (default: true).
- **returns**: A `BenchmarkResult` containing timing measurements and statistical analysis.
- **throws**: `ArgumentNullException` if `action` is null; `BenchmarkException` if `throwOnError` is true and the action throws.

---

### `BenchmarkAsync`

```csharp
public static async System.Threading.Tasks.Task<BenchmarkResult> BenchmarkAsync(
    Func<System.Threading.Tasks.Task> asyncAction,
    int iterations = 1,
    int warmupIterations = 0,
    bool throwOnError = true)
```

Measures the execution time of an asynchronous action over multiple iterations. Includes optional warmup phase.

- **asyncAction**: The asynchronous delegate to benchmark.
- **iterations**: Number of times to run the async action (default: 1).
- **warmupIterations**: Number of warmup runs before actual measurement (default: 0).
- **throwOnError**: Whether to throw if the action throws (default: true).
- **returns**: A `BenchmarkResult` containing timing measurements and statistical analysis.
- **throws**: `ArgumentNullException` if `asyncAction` is null; `BenchmarkException` if `throwOnError` is true and the action throws.

---

### `ToDetailedString`

```csharp
public static string ToDetailedString(this BenchmarkResult result)
```

Formats a `BenchmarkResult` into a human-readable string with detailed statistics including mean, standard deviation, min, max, GC pressure, and memory pressure indicators.

- **result**: The benchmark result to format.
- **returns**: A formatted string with detailed performance metrics.

---

### `ToCompactString`

```csharp
public static string ToCompactString(this BenchmarkResult result)
```

Formats a `BenchmarkResult` into a concise string suitable for logs or telemetry, including mean, standard deviation, and GC pressure score.

- **result**: The benchmark result to format.
- **returns**: A compact formatted string with key performance metrics.

---

### `GetStandardDeviation`

```csharp
public static double GetStandardDeviation(this BenchmarkResult result)
```

Computes the standard deviation of the benchmark measurements.

- **result**: The benchmark result containing the measurements.
- **returns**: The standard deviation of the measurements.
- **throws**: `ArgumentNullException` if `result` is null.

---

### `GetGcPressureScore`

```csharp
public static double GetGcPressureScore(this BenchmarkResult result)
```

Estimates the GC pressure based on the number and size of GC collections during benchmarking.

- **result**: The benchmark result containing GC collection data.
- **returns**: A score representing GC pressure (higher values indicate more pressure).
- **throws**: `ArgumentNullException` if `result` is null.

---

### `HasMemoryPressure`

```csharp
public static bool HasMemoryPressure(this BenchmarkResult result)
```

Determines whether significant memory pressure occurred during benchmarking.

- **result**: The benchmark result to analyze.
- **returns**: `true` if memory pressure was detected; otherwise, `false`.
- **throws**: `ArgumentNullException` if `result` is null.

---
### `ComparePerformance`

```csharp
public static double ComparePerformance(
    this BenchmarkResult baseline,
    BenchmarkResult candidate)
```

Compares two benchmark results to determine the relative performance difference.

- **baseline**: The reference benchmark result.
- **candidate**: The benchmark result to compare.
- **returns**: A positive value if `candidate` is faster, negative if slower, or zero if comparable.
- **throws**: `ArgumentNullException` if either `baseline` or `candidate` is null.

---
### `GetMeasurements`

```csharp
public static System.Collections.Generic.IReadOnlyList<long> GetMeasurements(
    this BenchmarkResult result)
```

Retrieves the raw measurement values in nanoseconds.

- **result**: The benchmark result containing the measurements.
- **returns**: An immutable list of measurement values.
- **throws**: `ArgumentNullException` if `result` is null.

## Usage

### Synchronous Benchmarking

```csharp
using var helper = new PerformanceHelperExtensions();

// Benchmark a CPU-bound operation
var result = PerformanceHelperExtensions.Benchmark(
    () => ComputePrimes(1000),
    iterations: 10,
    warmupIterations: 2);

Console.WriteLine(result.ToDetailedString());
```

### Asynchronous Benchmarking

```csharp
using var helper = new PerformanceHelperExtensions();

// Benchmark an async I/O operation
var result = await PerformanceHelperExtensions.BenchmarkAsync(
    async () => await DownloadFileAsync("https://example.com/large-file.zip"),
    iterations: 5,
    warmupIterations: 1);

Console.WriteLine($"Mean: {result.GetMeasurements().Average() / 1_000_000} ms");
```

## Notes

- **Thread Safety**: All methods are thread-safe and do not mutate shared state. Benchmarking methods capture GC and timing data in a thread-local context during execution.
- **JIT Effects**: Warmup iterations are recommended to account for JIT compilation overhead, especially for short-running operations.
- **Memory Pressure**: GC pressure detection relies on `GC.GetTotalMemory(false)` and collection counts. High memory pressure may skew timing results due to GC pauses.
- **Error Handling**: By default, exceptions during benchmarking propagate. Disable via `throwOnError: false` to collect results even if the action fails.
- **Precision**: Timing uses high-resolution timers where available. Results are in nanoseconds but may have platform-specific resolution limits.
- **Statistical Robustness**: For reliable standard deviation and mean calculations, use at least 5–10 iterations. Outliers from GC or system noise are not automatically filtered.
