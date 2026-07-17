using BenchmarkDotNet.Attributes;
using DotNetRealtimePipeline.Benchmarks;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

namespace DotNetRealtimePipeline.Benchmarks;

/// <summary>
/// Extension methods for <see cref="PipelineBenchmarks"/> to simplify benchmark execution and analysis.
/// </summary>
pub static class PipelineBenchmarksExtensions
{
	/// <summary>
	/// Runs a series of benchmarks to test the end-to-end throughput of the pipeline with varying batch sizes.
	/// </summary>
	/// <param name="benchmarks">The <see cref="PipelineBenchmarks"/> instance.</param>
	/// <param name="totalItems">The total number of items to process. Must be positive.</param>
	/// <param name="batchSizes">The batch sizes to test. Each must be positive.</param>
	/// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="totalItems"/> or any batch size is not positive.</exception>
	public static async Task RunEndToEndThroughputBenchmarks(this PipelineBenchmarks benchmarks, int totalItems, params int[] batchSizes)
	{
		ArgumentNullException.ThrowIfNull(benchmarks);
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(totalItems, 0);

		foreach (var batchSize in batchSizes)
		{
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0);

			Console.WriteLine($"Running end-to-end throughput benchmark with batch size {batchSize}");

			var dataPoints = new List<DataPoint>(totalItems);
			var now = DateTime.UtcNow.Ticks;

			// Pre-generate data points
			for (int i = 0; i < totalItems; i++)
			{
				dataPoints.Add(new DataPoint(
					id: i,
					timestamp: now + i * 100,
					value: i * 1.5,
					source: "ThroughputSensor"
				));
			}

			// Ingest in batches
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			for (int i = 0; i < totalItems; i += batchSize)
			{
				var batch = dataPoints.Skip(i).Take(batchSize).ToList();
				foreach (var point in batch)
				{
					_ = benchmarks.IngestSingleDataPoint();
				}
			}

			// Allow processing to complete
			await Task.Delay(500);
			stopwatch.Stop();

			Console.WriteLine($"Batch size {batchSize}: {totalItems / stopwatch.Elapsed.TotalSeconds:F2} items/sec");
		}

	/// <summary>
	/// Runs a series of benchmarks to test the windowing performance with varying data point counts.
	/// </summary>
	/// <param name="benchmarks">The <see cref="PipelineBenchmarks"/> instance.</param>
	/// <param name="dataPointCounts">The data point counts to test. Each must be positive.</param>
	/// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when any data point count is not positive.</exception>
	public static async Task RunWindowingBenchmarks(this PipelineBenchmarks benchmarks, params int[] dataPointCounts)
	{
		ArgumentNullException.ThrowIfNull(benchmarks);

		foreach (var dataPointCount in dataPointCounts)
		{
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(dataPointCount, 0);

			Console.WriteLine($"Running windowing benchmark with {dataPointCount} data points");

			var dataPoints = new List<DataPoint>(dataPointCount);
			var now = DateTime.UtcNow.Ticks;

			// Pre-generate data points
			for (int i = 0; i < dataPointCount; i++)
			{
				dataPoints.Add(new DataPoint(
					id: i,
					timestamp: now + i * 100,
					value: i * 1.5,
					source: "WindowingSensor"
				));
			}

			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			await benchmarks.ProcessDataPointsThroughWindowing(dataPointCount);
			stopwatch.Stop();

			Console.WriteLine($"Processed {dataPointCount} data points in {stopwatch.Elapsed.TotalSeconds:F2} sec");
		}
}