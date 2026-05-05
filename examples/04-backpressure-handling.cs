#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetRealtimePipeline.Examples;

/// <summary>
/// Backpressure handling example demonstrating buffer management under high load conditions.
/// </summary>
public sealed class BackpressureHandlingExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Backpressure Handling Example ===\n");

        var services = new ServiceCollection();
        services.AddPipelineServices(config =>
        {
            config.MaxBufferSize = 10000;
            config.BackpressureThreshold = 0.8m; // Trigger at 80%
        });
        var serviceProvider = services.BuildServiceProvider();

        var backpressureService = serviceProvider.GetRequiredService<BackpressureService>();
        var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();

        try
        {
            await orchestrator.StartAsync();
            Console.WriteLine("Testing backpressure strategies...\n");

            // Test 1: Block Strategy
            Console.WriteLine("Test 1: Block Strategy (Pauses ingestion)");
            await TestBackpressureStrategy(
                serviceProvider,
                BackpressureStrategy.Block,
                20000);

            // Test 2: Throttle Strategy
            Console.WriteLine("\nTest 2: Throttle Strategy (Reduces ingestion rate)");
            await TestBackpressureStrategy(
                serviceProvider,
                BackpressureStrategy.Throttle,
                20000);

            // Test 3: Drop Strategy
            Console.WriteLine("\nTest 3: Drop Strategy (Discards old data)");
            await TestBackpressureStrategy(
                serviceProvider,
                BackpressureStrategy.Drop,
                20000);

            Console.WriteLine("\n✓ Backpressure example completed successfully");
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }

    private static async Task TestBackpressureStrategy(
        IServiceProvider serviceProvider,
        BackpressureStrategy strategy,
        int itemsToIngest)
    {
        var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();
        var backpressureService = serviceProvider.GetRequiredService<BackpressureService>();

        Console.WriteLine($"  Strategy: {strategy}");
        Console.WriteLine($"  Items to ingest: {itemsToIngest}\n");

        // Create buffer context
        string stageName = $"Stage_{strategy}";
        var context = backpressureService.CreateContext(stageName, maxCapacity: 10000);

        int acceptedCount = 0;
        int rejectedCount = 0;
        int droppedCount = 0;

        // Attempt to ingest large volume
        for (int i = 0; i < itemsToIngest; i++)
        {
            bool accepted = backpressureService.TryAddToBuffer(stageName, itemCount: 1);

            if (accepted)
            {
                acceptedCount++;

                // Actually ingest the data
                var dataPoint = new DataPoint(
                    id: i,
                    timestamp: DateTime.UtcNow.Ticks,
                    value: Random.Shared.Next(0, 100),
                    source: "HighLoad-Sensor"
                );
                await orchestrator.IngestDataPointAsync(dataPoint);
            }
            else
            {
                // Buffer is full, apply backpressure strategy
                rejectedCount++;

                var response = await backpressureService.ApplyBackpressureAsync(
                    stageName,
                    strategy,
                    timeoutMs: 1000);

                if (response.Status == "dropped")
                {
                    droppedCount++;
                }

                // Small delay to allow buffer to drain
                await Task.Delay(10);
            }
        }

        // Report results
        var bufferStatus = backpressureService.GetBufferStatus();
        var pipelineStatus = orchestrator.GetStatus();

        Console.WriteLine($"  Results:");
        Console.WriteLine($"    Accepted: {acceptedCount:N0}");
        Console.WriteLine($"    Rejected: {rejectedCount:N0}");
        Console.WriteLine($"    Dropped: {droppedCount:N0}");
        Console.WriteLine($"    Success Rate: {(double)acceptedCount / itemsToIngest:P2}");
        Console.WriteLine($"    Current Buffer Fill: {bufferStatus[stageName]}/10000");
        Console.WriteLine($"    Total Processed: {pipelineStatus.TotalDataPointsProcessed:N0}");
    }
}
