#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
/// Extension methods for <see cref="PerformanceHelper"/> classes to provide additional functionality
/// for performance benchmarking, memory analysis, and performance trending.
/// </summary>
public static class PerformanceHelperExtensions
{
	private static double GetMedian(List<long> values)
	{
		ArgumentNullException.ThrowIfNull(values);
		ArgumentOutOfRangeException.ThrowIfLessThan(values.Count, 1);

		var sorted = values.OrderBy(v => v).ToList();
		int count = sorted.Count;

		if (count % 2 == 0)
		{
			return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
		}

		return sorted[count / 2];
	}

	private static double GetPercentile(List<long> values, double percentile)
	{
		ArgumentNullException.ThrowIfNull(values);
		ArgumentOutOfRangeException.ThrowIfLessThan(percentile, 0.0);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(percentile, 100.0);

		var sorted = values.OrderBy(v => v).ToList();
		int index = (int)Math.Ceiling(sorted.Count * (percentile / 100.0)) - 1;
		return sorted[Math.Max(0, Math.Min(index, sorted.Count - 1))];
	}

	/// <summary>
	/// Measures the execution time of an operation with custom iterations.
	/// </summary>
	/// <param name="helper">The performance helper instance.</param>
	/// <param name="operation">The operation to benchmark.</param>
	/// <param name="iterations">Number of iterations to run.</param>
	/// <returns>A benchmark result containing timing statistics.</returns>
	/// <exception cref="ArgumentNullException">Thrown when operation is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when iterations is less than 1.</exception>
	public static BenchmarkResult Benchmark(
		this PerformanceHelper _,
		Action operation,
		int iterations = 1000)
	{
		ArgumentNullException.ThrowIfNull(operation);
		ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);

		var measurements = new List<long>();

		for (int i = 0; i < iterations; i++)
		{
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
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
	/// Measures the execution time of an asynchronous operation with custom iterations.
	/// </summary>
	/// <param name="helper">The performance helper instance.</param>
	/// <param name="operation">The operation to benchmark.</param>
	/// <param name="iterations">Number of iterations to run.</param>
	/// <returns>A benchmark result containing timing statistics.</returns>
	/// <exception cref="ArgumentNullException">Thrown when operation is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when iterations is less than 1.</exception>
	public static async System.Threading.Tasks.Task<BenchmarkResult> BenchmarkAsync(
		this PerformanceHelper _,
		Func<System.Threading.Tasks.Task> operation,
		int iterations = 1000)
	{
		ArgumentNullException.ThrowIfNull(operation);
		ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);

		var measurements = new List<long>();

		for (int i = 0; i < iterations; i++)
		{
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			await operation();
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
	/// Gets a formatted string representation of benchmark results with all statistics.
	/// </summary>
	/// <param name="result">The benchmark result.</param>
	/// <returns>A formatted string with all benchmark statistics.</returns>
	/// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
	public static string ToDetailedString(this BenchmarkResult result)
	{
		ArgumentNullException.ThrowIfNull(result);

		return string.Create(CultureInfo.InvariantCulture, $@"Benchmark Results:\n Iterations: {result.Iterations:N0}\n Average: {result.AverageMs:F3} ms\n Min: {result.MinMs:N0} ms\n Max: {result.MaxMs:N0} ms\n Median: {result.MedianMs:F3} ms\n P95: {result.P95Ms:F3} ms\n P99: {result.P99Ms:F3} ms\n Range: {result.MaxMs - result.MinMs:N0} ms")
			.Replace("\\n", "\n");
	}

	/// <summary>
	/// Gets a compact string representation of benchmark results suitable for logging.
	/// </summary>
	/// <param name="result">The benchmark result.</param>
	/// <returns>A compact formatted string with key statistics.</returns>
	/// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
	public static string ToCompactString(this BenchmarkResult result)
	{
		ArgumentNullException.ThrowIfNull(result);

		return $"Benchmark: {result.Iterations:N0} iters, {result.AverageMs:F2}ms avg, {result.P95Ms:F2}ms P95, {result.P99Ms:F2}ms P99";
	}

	/// <summary>
	/// Gets the standard deviation of the benchmark measurements.
	/// </summary>
	/// <param name="result">The benchmark result.</param>
	/// <returns>The standard deviation of the measurements.</returns>
	/// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when measurements count is less than 2.</exception>
	public static double GetStandardDeviation(this BenchmarkResult result)
	{
		ArgumentNullException.ThrowIfNull(result);
		ArgumentOutOfRangeException.ThrowIfLessThan(result.Measurements.Count, 2);

		var measurements = result.Measurements;
		var mean = measurements.Average();
		var variance = measurements.Sum(v => Math.Pow(v - mean, 2)) / (measurements.Count - 1);
		return Math.Sqrt(variance);
	}

	/// <summary>
	/// Gets a formatted string representation of memory statistics.
	/// </summary>
	/// <param name="stats">The memory statistics.</param>
	/// <returns>A formatted string with memory usage information.</returns>
	/// <exception cref="ArgumentNullException">Thrown when stats is null.</exception>
	public static string ToDetailedString(this MemoryStats stats)
	{
		ArgumentNullException.ThrowIfNull(stats);

		return string.Create(CultureInfo.InvariantCulture, $@"Memory Usage:\n Working Set: {stats.WorkingSetMb:F2} MB\n Private Memory: {stats.PrivateMemoryMb:F2} MB\n Peak Working: {stats.PeakWorkingSetMb:F2} MB\n Total Memory: {stats.TotalMemoryMb:F2} MB\n GC Collections: 0={stats.GC0Collections}, 1={stats.GC1Collections}, 2={stats.GC2Collections}")
			.Replace("\\n", "\n");
	}

	/// <summary>
	/// Gets a compact string representation of memory statistics suitable for logging.
	/// </summary>
	/// <param name="stats">The memory statistics.</param>
	/// <returns>A compact formatted string with key memory statistics.</returns>
	/// <exception cref="ArgumentNullException">Thrown when stats is null.</exception>
	public static string ToCompactString(this MemoryStats stats)
	{
		ArgumentNullException.ThrowIfNull(stats);

		return $"Memory: WS={stats.WorkingSetMb:F1}MB, Private={stats.PrivateMemoryMb:F1}MB, Peak={stats.PeakWorkingSetMb:F1}MB, GC0={stats.GC0Collections}, GC1={stats.GC1Collections}, GC2={stats.GC2Collections}";
	}

	/// <summary>
	/// Gets the total garbage collection pressure as a weighted sum of all GC generations.
	/// </summary>
	/// <param name="stats">The memory statistics.</param>
	/// <returns>A weighted GC pressure score (higher values indicate more GC activity).</returns>
	/// <exception cref="ArgumentNullException">Thrown when stats is null.</exception>
	public static double GetGcPressureScore(this MemoryStats stats)
	{
		ArgumentNullException.ThrowIfNull(stats);

		// Weighted sum: GC2 has highest impact, GC0 lowest
		return stats.GC2Collections * 10.0 +
			stats.GC1Collections * 3.0 +
			stats.GC0Collections * 1.0;
	}

	/// <summary>
	/// Determines if the memory usage indicates potential memory pressure.
	/// </summary>
	/// <param name="stats">The memory statistics.</param>
	/// <param name="workingSetThresholdMb">Threshold in MB for working set.</param>
	/// <param name="gcPressureThreshold">Threshold for GC pressure score.</param>
	/// <returns>True if memory pressure is detected; otherwise false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when stats is null.</exception>
	public static bool HasMemoryPressure(
		this MemoryStats stats,
		double workingSetThresholdMb = 512.0,
		double gcPressureThreshold = 15.0)
	{
		ArgumentNullException.ThrowIfNull(stats);

		double gcPressure = stats.GetGcPressureScore();
		return stats.WorkingSetMb > workingSetThresholdMb || gcPressure > gcPressureThreshold;
	}

	/// <summary>
	/// Compares two benchmark results and returns a performance improvement/degradation percentage.
	/// </summary>
	/// <param name="current">The current benchmark result.</param>
	/// <param name="baseline">The baseline benchmark result.</param>
	/// <returns>Percentage improvement (positive) or degradation (negative).</returns>
	/// <exception cref="ArgumentNullException">Thrown when either parameter is null.</exception>
	public static double ComparePerformance(this BenchmarkResult current, BenchmarkResult baseline)
	{
		ArgumentNullException.ThrowIfNull(current);
		ArgumentNullException.ThrowIfNull(baseline);

		// Compare average execution time
		double baselineAvg = baseline.AverageMs;
		double currentAvg = current.AverageMs;

		if (baselineAvg == 0)
		{
			return 0;
		}

		return ((baselineAvg - currentAvg) / baselineAvg) * 100.0;
	}

	/// <summary>
	/// Gets a read-only view of the benchmark measurements.
	/// </summary>
	/// <param name="result">The benchmark result.</param>
	/// <returns>Read-only list of measurements.</returns>
	/// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
	public static System.Collections.Generic.IReadOnlyList<long> GetMeasurements(
		this BenchmarkResult result)
	{
		ArgumentNullException.ThrowIfNull(result);
		return result.Measurements.AsReadOnly();
	}
}