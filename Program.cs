// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Configuration;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Set up dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Configure pipeline with custom settings
services.AddPipelineServices(config =>
{
    config.ConfigId = 1;
    config.PipelineName = "RealTime-DataPipeline";
    config.Version = "1.0.0";
    config.MaxBufferSize = 50000;
    config.BufferFlushIntervalMs = 2000;
    config.WindowSizeMs = 5000;
    config.WindowSlideMs = 1000;
    config.MaxConcurrentConsumers = 8;
    config.MinDataQualityThreshold = 70;
    config.MaxRetries = 3;
});

var serviceProvider = services.BuildServiceProvider();

// Get the orchestrator
var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Starting Real-Time Data Processing Pipeline");
logger.LogInformation("Pipeline: {Name} v{Version}", "RealTime-DataPipeline", "1.0.0");

try
{
    // Start the pipeline
    await orchestrator.StartAsync();
    logger.LogInformation("Pipeline started successfully");

    // Generate sample data and ingest it
    await RunDemoAsync(orchestrator, logger);
}
catch (Exception ex)
{
    logger.LogError(ex, "Pipeline error: {Message}", ex.Message);
}
finally
{
    await orchestrator.StopAsync();
    logger.LogInformation("Pipeline stopped");
}

async Task RunDemoAsync(PipelineOrchestrator pipeline, ILogger<Program> log)
{
    log.LogInformation("Starting demo data ingestion");

    // Create and ingest sample data
    var random = new Random(42);
    var sources = new[] { "Sensor-1", "Sensor-2", "Sensor-3", "Sensor-4" };

    for (int i = 0; i < 500; i++)
    {
        var source = sources[i % sources.Length];
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - (500 - i) * 100;

        var dataPoint = new DataPoint(
            id: i + 1,
            timestamp: timestamp,
            value: 20 + random.NextDouble() * 30,
            source: source
        )
        {
            Quality = 80 + random.Next(0, 20),
            Tags = $"Region-{random.Next(1, 4)}"
        };

        dataPoint.AddMetadata("DeviceId", $"Device-{source}");
        dataPoint.AddMetadata("Location", $"Location-{random.Next(1, 10)}");

        bool accepted = await pipeline.IngestDataPointAsync(dataPoint);

        if (!accepted && i % 50 == 0)
        {
            log.LogWarning("Backpressure detected, waiting before retrying");
            await Task.Delay(100);
        }

        if (i % 100 == 0 && i > 0)
        {
            var status = pipeline.GetStatus();
            log.LogInformation("Processed {Count} items", status.TotalDataPointsProcessed);
        }
    }

    // Allow processing to complete
    await Task.Delay(2000);

    // Get final status
    var finalStatus = pipeline.GetStatus();
    log.LogInformation("=== Final Status ===");
    log.LogInformation("Total Processed: {Count}", finalStatus.TotalDataPointsProcessed);
    log.LogInformation("Total Failed: {Count}", finalStatus.TotalDataPointsFailed);
    log.LogInformation("Pending: {Count}", finalStatus.PendingItemsInQueue);

    try
    {
        var health = await pipeline.GetHealthReportAsync();
        log.LogInformation("=== Health Report ===");
        log.LogInformation("Status: {Status}", health.Status);
        log.LogInformation("Message: {Message}", health.Message);
        log.LogInformation("Throughput: {Throughput:F2} items/sec", health.ThroughputItemsPerSecond);
        log.LogInformation("Success Rate: {Rate:F2}%", health.SuccessRatePercent);
        log.LogInformation("Avg Latency: {Latency:F2}ms", health.AverageProcessingTimeMs);
    }
    catch (Exception ex)
    {
        log.LogWarning(ex, "Could not generate health report");
    }

    try
    {
        var trend = await pipeline.GetPerformanceTrendAsync();
        log.LogInformation("=== Performance Trend ===");
        log.LogInformation("Trend Direction: {Direction}", trend.TrendDirection);
        log.LogInformation("Samples: {Count}", trend.SamplesAnalyzed);
    }
    catch (Exception ex)
    {
        log.LogWarning(ex, "Could not analyze performance trend");
    }

    log.LogInformation("=== Demo Complete ===");
}
