#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace DotNetRealtimePipeline.Examples;

/// <summary>
/// Simple ingestion example demonstrating basic data point ingestion and status reporting.
/// </summary>
public class SimpleIngestionsExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Simple Data Ingestion Example ===\n");

        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddPipelineServices();
        var serviceProvider = services.BuildServiceProvider();

        // Get orchestrator
        var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();

        try
        {
            // Start pipeline
            Console.WriteLine("Starting pipeline...");
            await orchestrator.StartAsync();

            // Ingest data points
            Console.WriteLine("\nIngesting 1,000 data points...");
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
            {
                var dataPoint = new DataPoint(
                    id: i,
                    timestamp: DateTime.UtcNow.Ticks,
                    value: Random.Shared.NextDouble() * 100,
                    source: "Sensor-1"
                );

                bool accepted = await orchestrator.IngestDataPointAsync(dataPoint);
                if (!accepted)
                {
                    Console.WriteLine($"Warning: Data point {i} was not accepted");
                }

                // Simulate some delay between ingestions
                if (i % 100 == 0 && i > 0)
                {
                    await Task.Delay(10);
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"Ingestion completed in {stopwatch.ElapsedMilliseconds}ms");

            // Wait for processing
            await Task.Delay(2000);

            // Get status
            var status = orchestrator.GetStatus();
            Console.WriteLine($"\nPipeline Status:");
            Console.WriteLine($"  Total Processed: {status.TotalDataPointsProcessed}");
            Console.WriteLine($"  Total Failed: {status.TotalDataPointsFailed}");
            Console.WriteLine($"  Is Running: {status.IsRunning}");
            Console.WriteLine($"  Buffer Utilization: {status.BufferUtilization:P2}");

            // Get health report
            var health = await orchestrator.GetHealthReportAsync();
            Console.WriteLine($"\nHealth Report:");
            Console.WriteLine($"  Status: {health.Status}");
            Console.WriteLine($"  Throughput: {health.ThroughputItemsPerSecond:F2} items/sec");
            Console.WriteLine($"  Avg Latency: {health.AverageLatencyMs:F2} ms");
            Console.WriteLine($"  Error Rate: {health.ErrorRate:P2}");

            Console.WriteLine("\n✓ Example completed successfully");
        }
        finally
        {
            // Stop the pipeline
            Console.WriteLine("\nStopping pipeline...");
            await orchestrator.StopAsync();
        }
    }
}
