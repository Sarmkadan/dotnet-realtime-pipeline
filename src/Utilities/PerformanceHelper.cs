#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

/// <summary>
/// Helper class for performance measurements, profiling, and optimization analysis.
/// Provides utilities for benchmarking, memory tracking, and performance trending.
/// </summary>
public sealed class PerformanceHelper
{
    /// <summary>
    /// Measures the execution time of a synchronous operation.
    /// </summary>
    public static (T Result, long ElapsedMs) MeasureExecution<T>(Func<T> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = operation();
        stopwatch.Stop();
        return (result, stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Measures the execution time of an asynchronous operation.
    /// </summary>
    public static async System.Threading.Tasks.Task<(T Result, long ElapsedMs)> MeasureExecutionAsync<T>(Func<System.Threading.Tasks.Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await operation();
        stopwatch.Stop();
        return (result, stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Benchmarks an operation multiple times and returns statistics.
    /// </summary>
    public static BenchmarkResult Benchmark(Action operation, int iterations = 1000)
    {
        var measurements = new List<long>();

        for (int i = 0; i < iterations; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            operation();
            stopwatch.Stop();
            measurements.Add(stopwatch.ElapsedMilliseconds);
        }

        return new BenchmarkResult
        {
            Iterations = iterations,
            Measurements = measurements,
            AverageMs = measurements.Average(),
            MinMs = measurements.Min(),
            MaxMs = measurements.Max(),
            MedianMs = GetMedian(measurements),
            P95Ms = GetPercentile(measurements, 95),
            P99Ms = GetPercentile(measurements, 99)
        };
    }

    /// <summary>
    /// Gets current memory usage statistics.
    /// </summary>
    public static MemoryStats GetMemoryStats()
    {
        var process = Process.GetCurrentProcess();

        return new MemoryStats
        {
            WorkingSetMb = process.WorkingSet64 / (1024.0 * 1024.0),
            PrivateMemoryMb = process.WorkingSet64 / (1024.0 * 1024.0), // Approximation using working set
            PeakWorkingSetMb = process.PeakWorkingSet64 / (1024.0 * 1024.0),
            GC0Collections = GC.CollectionCount(0),
            GC1Collections = GC.CollectionCount(1),
            GC2Collections = GC.CollectionCount(2),
            TotalMemoryMb = GC.GetTotalMemory(false) / (1024.0 * 1024.0)
        };
    }

    /// <summary>
    /// Calculates the median of a list of values.
    /// </summary>
    private static double GetMedian(List<long> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int count = sorted.Count;

        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }

        return sorted[count / 2];
    }

    /// <summary>
    /// Calculates a percentile value from a list.
    /// </summary>
    private static double GetPercentile(List<long> values, int percentile)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int index = (int)Math.Ceiling(sorted.Count * (percentile / 100.0)) - 1;
        return sorted[Math.Max(0, Math.Min(index, sorted.Count - 1))];
    }
}

/// <summary>
/// Results from a benchmark operation.
/// </summary>
public sealed class BenchmarkResult
{
    public int Iterations { get; set; }
    public List<long> Measurements { get; set; } = new();
    public double AverageMs { get; set; }
    public long MinMs { get; set; }
    public long MaxMs { get; set; }
    public double MedianMs { get; set; }
    public double P95Ms { get; set; }
    public double P99Ms { get; set; }

    public override string ToString()
    {
        return $"Iterations: {Iterations}, Avg: {AverageMs:F2}ms, Min: {MinMs}ms, Max: {MaxMs}ms, P95: {P95Ms:F2}ms, P99: {P99Ms:F2}ms";
    }
}

/// <summary>
/// Memory usage statistics.
/// </summary>
public sealed class MemoryStats
{
    public double WorkingSetMb { get; set; }
    public double PrivateMemoryMb { get; set; }
    public double PeakWorkingSetMb { get; set; }
    public int GC0Collections { get; set; }
    public int GC1Collections { get; set; }
    public int GC2Collections { get; set; }
    public double TotalMemoryMb { get; set; }

    public override string ToString()
    {
        return $"Working: {WorkingSetMb:F2}MB, Private: {PrivateMemoryMb:F2}MB, Peak: {PeakWorkingSetMb:F2}MB, GC0: {GC0Collections}, GC1: {GC1Collections}, GC2: {GC2Collections}";
    }
}

/// <summary>
/// Helper for tracking performance metrics over time.
/// </summary>
public sealed class PerformanceTracker
{
    private readonly List<PerformanceSample> _samples = new();
    private readonly object _lockObject = new();

    /// <summary>
    /// Records a performance sample.
    /// </summary>
    public void Record(string metricName, double value)
    {
        lock (_lockObject)
        {
            _samples.Add(new PerformanceSample
            {
                MetricName = metricName,
                Value = value,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Gets all recorded samples.
    /// </summary>
    public List<PerformanceSample> GetAllSamples()
    {
        lock (_lockObject)
        {
            return new List<PerformanceSample>(_samples);
        }
    }

    /// <summary>
    /// Gets statistics for a metric within a time window.
    /// </summary>
    public PerformanceStats GetStatistics(string metricName, TimeSpan window)
    {
        lock (_lockObject)
        {
            var cutoffTime = DateTime.UtcNow - window;
            var relevant = _samples
                .Where(s => s.MetricName == metricName && s.Timestamp >= cutoffTime)
                .Select(s => s.Value)
                .ToList();

            if (relevant.Count == 0)
            {
                return new PerformanceStats { SampleCount = 0 };
            }

            return new PerformanceStats
            {
                SampleCount = relevant.Count,
                Average = relevant.Average(),
                Min = relevant.Min(),
                Max = relevant.Max(),
                StdDev = CalculateStdDev(relevant),
                P95 = GetPercentile(relevant, 95),
                P99 = GetPercentile(relevant, 99)
            };
        }
    }

    /// <summary>
    /// Clears samples older than a specified time.
    /// </summary>
    public void ClearOldSamples(TimeSpan age)
    {
        lock (_lockObject)
        {
            var cutoffTime = DateTime.UtcNow - age;
            _samples.RemoveAll(s => s.Timestamp < cutoffTime);
        }
    }

    private static double GetPercentile(List<double> values, int percentile)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int index = (int)Math.Ceiling(sorted.Count * (percentile / 100.0)) - 1;
        return sorted[Math.Max(0, Math.Min(index, sorted.Count - 1))];
    }

    private static double CalculateStdDev(List<double> values)
    {
        if (values.Count < 2) return 0;

        var mean = values.Average();
        var variance = values.Sum(v => Math.Pow(v - mean, 2)) / (values.Count - 1);
        return Math.Sqrt(variance);
    }
}

/// <summary>
/// Represents a single performance sample.
/// </summary>
public sealed class PerformanceSample
{
    public string MetricName { get; set; }
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Represents performance statistics for a metric.
/// </summary>
public sealed class PerformanceStats
{
    public int SampleCount { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double StdDev { get; set; }
    public double P95 { get; set; }
    public double P99 { get; set; }
}
