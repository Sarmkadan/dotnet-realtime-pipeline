using BenchmarkDotNet.Attributes;
using DotNetRealtimePipeline.Benchmarks;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

namespace DotNetRealtimePipeline.Benchmarks;

/// <summary>
/// Extension methods for <see cref="PipelineBenchmarks"/> to simplify benchmark execution and analysis.
/// </summary>
public static class PipelineBenchmarksExtensions
{
    /// <summary>
    /// Runs a series of benchmarks to test the end-to-end throughput of the pipeline with varying batch sizes.
    /// </summary>
    /// <param name="benchmarks">The <see cref="PipelineBenchmarks"/> instance.</param>
    /// <param name="totalItems">The total number of items to process.</param>
    /// <param name="batchSizes">The batch sizes to test.</param>
    public static async Task RunEndToEndThroughputBenchmarks(this PipelineBenchmarks benchmarks, int totalItems, params int[] batchSizes)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        foreach (var batchSize in batchSizes)
        {
            Console.WriteLine($"Running end-to-end throughput benchmark with batch size {batchSize}");

            var dataPoints = new List<DataPoint>();
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
                    await benchmarks.IngestSingleDataPoint();
                }
            }

            // Allow processing to complete
            await Task.Delay(500);
            stopwatch.Stop();

            Console.WriteLine($"Batch size {batchSize}: {totalItems / stopwatch.Elapsed.TotalSeconds:F2} items/sec");
        }
    }

    /// <summary>
    /// Runs a series of benchmarks to test the windowing performance with varying data point counts.
    /// </summary>
    /// <param name="benchmarks">The <see cref="PipelineBenchmarks"/> instance.</param>
    /// <param name="dataPointCounts">The data point counts to test.</param>
    public static void RunWindowingBenchmarks(this PipelineBenchmarks benchmarks, params int[] dataPointCounts)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        foreach (var dataPointCount in dataPointCounts)
        {
            Console.WriteLine($"Running windowing benchmark with {dataPointCount} data points");

            var dataPoints = new List<DataPoint>();
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
            benchmarks.ProcessDataPointsThroughWindowing(dataPointCount);
            stopwatch.Stop();

            Console.WriteLine($"Processed {dataPointCount} data points in {stopwatch.Elapsed.TotalSeconds:F2} sec");
        }
    }
}
