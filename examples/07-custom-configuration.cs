#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Configuration;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetRealtimePipeline.Examples;

/// <summary>
/// Custom configuration example demonstrating various configuration approaches for different scenarios.
/// </summary>
public class CustomConfigurationExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Custom Configuration Example ===\n");

        // Scenario 1: High Performance Configuration
        Console.WriteLine("Scenario 1: High Performance (100k+ items/sec)\n");
        await RunScenarioAsync(CreateHighPerformanceConfig());

        // Scenario 2: Low Latency Configuration
        Console.WriteLine("\nScenario 2: Low Latency (< 10ms processing)\n");
        await RunScenarioAsync(CreateLowLatencyConfig());

        // Scenario 3: Resource Constrained Configuration
        Console.WriteLine("\nScenario 3: Resource Constrained (1GB RAM limit)\n");
        await RunScenarioAsync(CreateResourceConstrainedConfig());

        Console.WriteLine("\n✓ Custom configuration example completed successfully");
    }

    private static async Task RunScenarioAsync(Action<PipelineConfigurationBuilder> configAction)
    {
        var services = new ServiceCollection();
        services.AddPipelineServices(config =>
        {
            // Apply scenario-specific configuration
            var builder = new PipelineConfigurationBuilder("ScenarioPipeline", "1.0.0");
            configAction(builder);
            var pipelineConfig = builder.Build();

            config.MaxBufferSize = pipelineConfig.MaxBufferSize;
            config.MaxConcurrentConsumers = pipelineConfig.MaxConcurrentConsumers;
            config.BufferFlushIntervalMs = pipelineConfig.BufferFlushIntervalMs;
            config.WindowSizeMs = pipelineConfig.WindowSizeMs;
            config.WindowSlideMs = pipelineConfig.WindowSlideMs;
            config.MaxRetries = pipelineConfig.MaxRetries;
            config.ProcessingTimeoutMs = pipelineConfig.ProcessingTimeoutMs;
        });

        var serviceProvider = services.BuildServiceProvider();
        var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();
        var metricsService = serviceProvider.GetRequiredService<MetricsService>();

        try
        {
            await orchestrator.StartAsync();

            // Ingest 1000 points
            for (int i = 0; i < 1000; i++)
            {
                var dataPoint = new DataPoint(
                    id: i,
                    timestamp: DateTime.UtcNow.Ticks,
                    value: Random.Shared.Next(0, 100),
                    source: "TestSensor"
                );
                await orchestrator.IngestDataPointAsync(dataPoint);
            }

            await Task.Delay(2000);

            // Report metrics
            var status = orchestrator.GetStatus();
            var health = await metricsService.GenerateHealthReportAsync();

            Console.WriteLine($"  Processed: {status.TotalDataPointsProcessed:N0}");
            Console.WriteLine($"  Throughput: {health.ThroughputItemsPerSecond:F2} items/sec");
            Console.WriteLine($"  Avg Latency: {health.AverageLatencyMs:F2} ms");
            Console.WriteLine($"  Status: {health.Status}");
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }

    private static void CreateHighPerformanceConfig(PipelineConfigurationBuilder builder)
    {
        builder
            .WithHighPerformanceDefaults()
            .WithBufferConfiguration(
                maxBufferSize: 500000,
                flushIntervalMs: 500,
                concurrentConsumers: 16)
            .WithWindowingConfiguration(
                windowSizeMs: 1000,
                windowSlideMs: 500,
                windowType: "SLIDING")
            .WithStage("Ingestion", "SOURCE")
            .WithStage("Processing", "TRANSFORM")
            .WithStage("Aggregation", "AGGREGATE")
            .WithStage("Export", "SINK");
    }

    private static Action<PipelineConfigurationBuilder> CreateHighPerformanceConfig()
    {
        return builder =>
        {
            builder
                .WithHighPerformanceDefaults()
                .WithBufferConfiguration(500000, 500, 16)
                .WithWindowingConfiguration(1000, 500, "SLIDING");
        };
    }

    private static Action<PipelineConfigurationBuilder> CreateLowLatencyConfig()
    {
        return builder =>
        {
            builder
                .WithHighPerformanceDefaults()
                .WithBufferConfiguration(10000, 100, 4)
                .WithWindowingConfiguration(1000, 500, "SLIDING");
        };
    }

    private static Action<PipelineConfigurationBuilder> CreateResourceConstrainedConfig()
    {
        return builder =>
        {
            builder
                .WithHighPerformanceDefaults()
                .WithBufferConfiguration(5000, 1000, 2)
                .WithWindowingConfiguration(5000, 2500, "TUMBLING");
        };
    }
}
