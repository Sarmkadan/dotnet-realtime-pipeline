#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetRealtimePipeline.Examples;

/// <summary>
/// Multi-source processing example demonstrating ingestion from multiple data sources simultaneously.
/// </summary>
public class MultiSourceProcessingExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Multi-Source Processing Example ===\n");

        // Setup with custom configuration
        var services = new ServiceCollection();
        services.AddPipelineServices(config =>
        {
            config.MaxBufferSize = 50000;
            config.MaxConcurrentConsumers = 8;
            config.WindowSizeMs = 5000;
            config.EnableQualityAnalysis = true;
        });
        var serviceProvider = services.BuildServiceProvider();

        var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();
        var metricsService = serviceProvider.GetRequiredService<MetricsService>();

        try
        {
            await orchestrator.StartAsync();
            Console.WriteLine("Pipeline started\n");

            // Define multiple sensors with different characteristics
            var sensors = new Dictionary<string, (double minValue, double maxValue, string unit)>
            {
                { "Temperature-1", (15.0, 35.0, "°C") },
                { "Temperature-2", (18.0, 32.0, "°C") },
                { "Humidity-1", (30.0, 80.0, "%") },
                { "Pressure-1", (950.0, 1050.0, "hPa") },
            };

            // Ingest data from all sources concurrently
            Console.WriteLine("Ingesting data from multiple sources...");
            var ingestTasks = sensors.Select(async sensor =>
            {
                await IngestSensorDataAsync(orchestrator, sensor.Key, sensor.Value.minValue, sensor.Value.maxValue, 250);
            });

            await Task.WhenAll(ingestTasks);
            Console.WriteLine($"✓ Data ingestion from {sensors.Count} sources completed\n");

            // Wait for processing
            await Task.Delay(3000);

            // Generate comprehensive report
            Console.WriteLine("=== Processing Results ===\n");

            var status = orchestrator.GetStatus();
            Console.WriteLine($"Total Data Points Processed: {status.TotalDataPointsProcessed:N0}");
            Console.WriteLine($"Total Data Points Failed: {status.TotalDataPointsFailed:N0}");
            Console.WriteLine($"Success Rate: {(1.0 - (double)status.TotalDataPointsFailed / status.TotalDataPointsProcessed):P2}\n");

            var health = await metricsService.GenerateHealthReportAsync();
            Console.WriteLine($"Pipeline Health Status: {health.Status}");
            Console.WriteLine($"Throughput: {health.ThroughputItemsPerSecond:F2} items/sec");
            Console.WriteLine($"Average Latency: {health.AverageLatencyMs:F2} ms");
            Console.WriteLine($"Error Rate: {health.ErrorRate:P2}\n");

            var trend = await metricsService.AnalyzePerformanceTrendAsync();
            Console.WriteLine($"Performance Trend: {trend.Direction}");
            Console.WriteLine($"Trend Magnitude: {trend.SlopeValue:F4}");

            Console.WriteLine("\n✓ Multi-source example completed successfully");
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }

    private static async Task IngestSensorDataAsync(
        PipelineOrchestrator orchestrator,
        string sensorName,
        double minValue,
        double maxValue,
        int pointCount)
    {
        var random = Random.Shared;
        for (int i = 0; i < pointCount; i++)
        {
            // Generate realistic sensor data with occasional noise
            double value = minValue + (maxValue - minValue) * random.NextDouble();
            if (random.NextDouble() < 0.05) // 5% chance of noise
            {
                value += (random.NextDouble() - 0.5) * (maxValue - minValue) * 0.2;
            }

            var dataPoint = new DataPoint(
                id: i,
                timestamp: DateTime.UtcNow.Ticks,
                value: (decimal)value,
                source: sensorName
            );

            await orchestrator.IngestDataPointAsync(dataPoint);

            // Simulate sensor reading interval
            if (i % 10 == 0)
            {
                await Task.Delay(5);
            }
        }
    }
}
