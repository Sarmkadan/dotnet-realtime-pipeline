// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetRealtimePipeline.Examples;

/// <summary>
/// Health monitoring example demonstrating continuous health tracking and performance trending.
/// </summary>
public class HealthMonitoringExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Health Monitoring Example ===\n");

        var services = new ServiceCollection();
        services.AddPipelineServices(config =>
        {
            config.MaxBufferSize = 50000;
            config.EnableMetrics = true;
        });
        var serviceProvider = services.BuildServiceProvider();

        var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();
        var metricsService = serviceProvider.GetRequiredService<MetricsService>();

        try
        {
            await orchestrator.StartAsync();
            Console.WriteLine("Pipeline started - monitoring health\n");

            // Ingest data gradually while monitoring
            var monitoringTask = MonitorHealthAsync(orchestrator, metricsService);
            var ingestingTask = IngestDataGraduallyAsync(orchestrator);

            await Task.WhenAll(monitoringTask, ingestingTask);

            // Final comprehensive report
            Console.WriteLine("\n=== Final Health Report ===\n");
            var finalHealth = await metricsService.GenerateHealthReportAsync();
            PrintHealthReport(finalHealth);

            var finalTrend = await metricsService.AnalyzePerformanceTrendAsync();
            Console.WriteLine($"\nPerformance Trend:");
            Console.WriteLine($"  Direction: {finalTrend.Direction}");
            Console.WriteLine($"  Slope: {finalTrend.SlopeValue:F4}");

            Console.WriteLine("\n✓ Health monitoring example completed successfully");
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }

    private static async Task MonitorHealthAsync(
        PipelineOrchestrator orchestrator,
        MetricsService metricsService)
    {
        Console.WriteLine("Starting health monitoring (5 seconds interval)\n");

        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(5000);

            var status = orchestrator.GetStatus();
            var health = await metricsService.GenerateHealthReportAsync();

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Monitoring Report #{i + 1}");
            Console.WriteLine($"  Status: {health.Status}");
            Console.WriteLine($"  Processed: {status.TotalDataPointsProcessed:N0}");
            Console.WriteLine($"  Throughput: {health.ThroughputItemsPerSecond:F2} items/sec");
            Console.WriteLine($"  Avg Latency: {health.AverageLatencyMs:F2} ms");
            Console.WriteLine($"  Buffer: {status.BufferUtilization:P2}");

            if (health.Status.ToString() != "HEALTHY")
            {
                Console.WriteLine($"  ⚠️  Pipeline status is {health.Status}");
            }

            Console.WriteLine();
        }
    }

    private static async Task IngestDataGraduallyAsync(PipelineOrchestrator orchestrator)
    {
        int dataPointId = 0;

        // Phase 1: Gradual increase
        Console.WriteLine("Phase 1: Ramping up ingestion\n");
        for (int batch = 0; batch < 5; batch++)
        {
            int batchSize = (batch + 1) * 50;
            for (int i = 0; i < batchSize; i++)
            {
                var dataPoint = new DataPoint(
                    id: dataPointId++,
                    timestamp: DateTime.UtcNow.Ticks,
                    value: Random.Shared.Next(0, 100),
                    source: $"Sensor-{Random.Shared.Next(1, 4)}"
                );
                await orchestrator.IngestDataPointAsync(dataPoint);
            }
            await Task.Delay(3000);
        }

        // Phase 2: Steady state
        Console.WriteLine("Phase 2: Steady-state operation\n");
        for (int i = 0; i < 300; i++)
        {
            var dataPoint = new DataPoint(
                id: dataPointId++,
                timestamp: DateTime.UtcNow.Ticks,
                value: Random.Shared.Next(0, 100),
                source: $"Sensor-{Random.Shared.Next(1, 4)}"
            );
            await orchestrator.IngestDataPointAsync(dataPoint);

            if (i % 50 == 0)
            {
                await Task.Delay(100);
            }
        }
    }

    private static void PrintHealthReport(HealthReport health)
    {
        Console.WriteLine($"Status: {health.Status}");
        Console.WriteLine($"Throughput: {health.ThroughputItemsPerSecond:F2} items/sec");
        Console.WriteLine($"Average Latency: {health.AverageLatencyMs:F2} ms");
        Console.WriteLine($"Min Latency: {health.MinLatencyMs:F2} ms");
        Console.WriteLine($"Max Latency: {health.MaxLatencyMs:F2} ms");
        Console.WriteLine($"Error Rate: {health.ErrorRate:P2}");
        Console.WriteLine($"Memory Usage: {health.MemoryUsageMb:F2} MB");
        Console.WriteLine($"Last Update: {health.LastUpdateTime:o}");

        if (health.Alerts.Any())
        {
            Console.WriteLine($"\nAlerts ({health.Alerts.Count}):");
            foreach (var alert in health.Alerts)
            {
                Console.WriteLine($"  - {alert}");
            }
        }
    }
}
