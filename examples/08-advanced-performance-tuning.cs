// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Configuration;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace DotNetRealtimePipeline.Examples;

/// <summary>
/// Advanced performance tuning example demonstrating configuration profiles
/// for different throughput and latency requirements.
/// </summary>
public class AdvancedPerformanceTuningExample
{
    private record PerformanceProfile(string Name, Action<PipelineConfig> Configure);

    public static async Task RunAsync()
    {
        Console.WriteLine("=== Advanced Performance Tuning Example ===\n");

        var profiles = new[]
        {
            new PerformanceProfile("High Throughput", ConfigureHighThroughput),
            new PerformanceProfile("Low Latency", ConfigureLowLatency),
            new PerformanceProfile("Balanced", ConfigureBalanced),
            new PerformanceProfile("Resource Constrained", ConfigureResourceConstrained)
        };

        foreach (var profile in profiles)
        {
            Console.WriteLine($"\n{'=', 60}");
            Console.WriteLine($"Testing Profile: {profile.Name}");
            Console.WriteLine($"{'=', 60}");

            await RunProfileBenchmarkAsync(profile.Configure);
            Console.WriteLine();
        }

        // Detailed analysis
        await RunDetailedAnalysisAsync();
    }

    private static async Task RunProfileBenchmarkAsync(Action<PipelineConfig> configurer)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPipelineServices(configurer);

        var provider = services.BuildServiceProvider();
        var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

        await orchestrator.StartAsync();

        try
        {
            var sw = Stopwatch.StartNew();
            var dataPointCount = 10000;

            // Ingest test data
            for (int i = 0; i < dataPointCount; i++)
            {
                var point = new DataPoint(
                    id: i,
                    timestamp: DateTime.UtcNow.Ticks,
                    value: Random.Shared.NextDouble() * 100,
                    source: $"Sensor-{i % 10}"
                );

                bool accepted = await orchestrator.IngestDataPointAsync(point);
                if (!accepted && i % 100 == 0)
                {
                    await Task.Delay(10);
                }
            }

            sw.Stop();

            // Wait for processing
            await Task.Delay(2000);

            var status = orchestrator.GetStatus();
            var health = await orchestrator.GetHealthReportAsync();

            Console.WriteLine($"\nResults:");
            Console.WriteLine($"  Total Points: {dataPointCount}");
            Console.WriteLine($"  Ingestion Time: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Throughput: {(dataPointCount / sw.Elapsed.TotalSeconds):F0} items/sec");
            Console.WriteLine($"  Memory Used: {GC.GetTotalMemory(false) / 1024 / 1024}MB");
            Console.WriteLine($"  Processed: {status.TotalDataPointsProcessed}");
            Console.WriteLine($"  Pipeline Throughput: {health.ThroughputItemsPerSecond:F0} items/sec");
            Console.WriteLine($"  Avg Latency: {health.AverageLatencyMs:F2}ms");
            Console.WriteLine($"  Health: {health.Status}");
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }

    private static async Task RunDetailedAnalysisAsync()
    {
        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("Detailed Latency Analysis");
        Console.WriteLine(new string('=', 60));

        var services = new ServiceCollection();
        services.AddPipelineServices(ConfigureLowLatency);
        var provider = services.BuildServiceProvider();
        var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

        await orchestrator.StartAsync();

        try
        {
            var latencies = new List<long>();

            for (int i = 0; i < 1000; i++)
            {
                var sw = Stopwatch.StartNew();

                var point = new DataPoint(
                    id: i,
                    timestamp: DateTime.UtcNow.Ticks,
                    value: i * 0.5m,
                    source: "Sensor-1"
                );

                await orchestrator.IngestDataPointAsync(point);
                sw.Stop();
                latencies.Add(sw.ElapsedMilliseconds);
            }

            var sorted = latencies.OrderBy(x => x).ToList();
            var avg = sorted.Average();
            var min = sorted.First();
            var max = sorted.Last();
            var p50 = sorted[(int)(sorted.Count * 0.50)];
            var p95 = sorted[(int)(sorted.Count * 0.95)];
            var p99 = sorted[(int)(sorted.Count * 0.99)];

            Console.WriteLine($"\nLatency Distribution (ms):");
            Console.WriteLine($"  Min:     {min:F3}");
            Console.WriteLine($"  Average: {avg:F3}");
            Console.WriteLine($"  P50:     {p50:F3}");
            Console.WriteLine($"  P95:     {p95:F3}");
            Console.WriteLine($"  P99:     {p99:F3}");
            Console.WriteLine($"  Max:     {max:F3}");

            // Histogram
            Console.WriteLine($"\nLatency Histogram:");
            var buckets = new[] { 1, 2, 5, 10, 20, 50, 100 };
            foreach (var bucket in buckets)
            {
                var count = sorted.Count(x => x <= bucket);
                var percent = (count * 100.0) / sorted.Count;
                var bar = new string('█', (int)(percent / 5));
                Console.WriteLine($"  <{bucket:3}ms: {bar} {percent:F1}%");
            }
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }

    // Configuration profiles
    private static void ConfigureHighThroughput(PipelineConfig config)
    {
        config.MaxBufferSize = 500000;
        config.BufferFlushIntervalMs = 1000;
        config.MaxConcurrentConsumers = Environment.ProcessorCount;
        config.WindowSizeMs = 10000;
        config.WindowSlideMs = 5000;
        config.EnableQualityAnalysis = false;
        config.MetricsHistorySize = 100;
    }

    private static void ConfigureLowLatency(PipelineConfig config)
    {
        config.MaxBufferSize = 10000;
        config.BufferFlushIntervalMs = 100;
        config.MaxConcurrentConsumers = 4;
        config.WindowSizeMs = 1000;
        config.WindowSlideMs = 500;
        config.EnableQualityAnalysis = true;
        config.MetricsHistorySize = 1000;
    }

    private static void ConfigureBalanced(PipelineConfig config)
    {
        config.MaxBufferSize = 50000;
        config.BufferFlushIntervalMs = 1000;
        config.MaxConcurrentConsumers = 8;
        config.WindowSizeMs = 5000;
        config.WindowSlideMs = 1000;
        config.EnableQualityAnalysis = true;
        config.MetricsHistorySize = 1000;
    }

    private static void ConfigureResourceConstrained(PipelineConfig config)
    {
        config.MaxBufferSize = 5000;
        config.BufferFlushIntervalMs = 2000;
        config.MaxConcurrentConsumers = 2;
        config.WindowSizeMs = 5000;
        config.WindowSlideMs = 5000;
        config.EnableQualityAnalysis = false;
        config.MetricsHistorySize = 50;
    }
}
