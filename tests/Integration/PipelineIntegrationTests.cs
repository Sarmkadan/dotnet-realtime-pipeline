#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Configuration;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Integration;

public class PipelineIntegrationTests
{
    private ServiceProvider SetupServices()
    {
        var services = new ServiceCollection();
        services.AddPipelineServices();
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task StartStop_ShouldInitializeAndCleanup()
    {
        // Arrange
        var provider = SetupServices();
        var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

        // Act
        await orchestrator.StartAsync();
        var status = orchestrator.GetStatus();
        await orchestrator.StopAsync();

        // Assert
        Assert.NotNull(status);
    }

    [Fact]
    public async Task IngestDataPoint_ShouldAcceptAndProcess()
    {
        // Arrange
        var provider = SetupServices();
        var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();
        await orchestrator.StartAsync();

        var dataPoint = new DataPoint(
            1,
            DateTime.UtcNow.Ticks,
            42.5m,
            "TestSensor"
        );

        try
        {
            // Act
            bool accepted = await orchestrator.IngestDataPointAsync(dataPoint);

            // Assert
            Assert.True(accepted);
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }

    [Fact]
    public async Task IngestBatch_ShouldProcessMultiplePoints()
    {
        // Arrange
        var provider = SetupServices();
        var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();
        await orchestrator.StartAsync();

        var points = Enumerable.Range(1, 100)
            .Select(i => new DataPoint(i, DateTime.UtcNow.Ticks, i * 1.5m, "Sensor-1"))
            .ToList();

        try
        {
            // Act
            int ingested = await orchestrator.IngestBatchAsync(points);
            await Task.Delay(500);

            // Assert
            Assert.True(ingested > 0);
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }

    [Fact]
    public async Task GetHealthReport_ShouldReturnMetrics()
    {
        // Arrange
        var provider = SetupServices();
        var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();
        await orchestrator.StartAsync();

        try
        {
            // Ingest some data
            for (int i = 0; i < 50; i++)
            {
                var point = new DataPoint(i, DateTime.UtcNow.Ticks, i * 0.5m, "S1");
                await orchestrator.IngestDataPointAsync(point);
            }

            await Task.Delay(1000);

            // Act
            var health = await orchestrator.GetHealthReportAsync();

            // Assert
            Assert.NotNull(health);
            Assert.NotEmpty(health.Status);
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }

    [Fact]
    public async Task QueryDataPoints_ShouldReturnFilteredResults()
    {
        // Arrange
        var provider = SetupServices();
        var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();
        var queryService = provider.GetRequiredService<QueryService>();

        await orchestrator.StartAsync();

        var now = DateTime.UtcNow.Ticks;
        try
        {
            // Ingest data
            for (int i = 0; i < 50; i++)
            {
                var point = new DataPoint(i, now, i * 2m, "Sensor-A");
                await orchestrator.IngestDataPointAsync(point);
            }

            await Task.Delay(1000);

            // Act
            var results = await queryService.SearchDataPointsAsync(
                startTime: now - 100000,
                endTime: now + 100000
            );

            // Assert
            Assert.NotNull(results);
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }

    [Fact]
    public async Task MultipleSourceIngestion_ShouldHandleConcurrentData()
    {
        // Arrange
        var provider = SetupServices();
        var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();
        await orchestrator.StartAsync();

        try
        {
            // Act
            var sources = new[] { "Sensor-1", "Sensor-2", "Sensor-3" };
            var tasks = sources.Select(async source =>
            {
                for (int i = 0; i < 20; i++)
                {
                    var point = new DataPoint(
                        i,
                        DateTime.UtcNow.Ticks,
                        Random.Shared.NextDouble() * 100,
                        source
                    );
                    await orchestrator.IngestDataPointAsync(point);
                }
            });

            await Task.WhenAll(tasks);
            await Task.Delay(1000);

            // Assert
            var status = orchestrator.GetStatus();
            Assert.True(status.TotalDataPointsProcessed > 0);
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }

    [Fact]
    public async Task GetMetricsHistory_ShouldReturnCollectedMetrics()
    {
        // Arrange
        var provider = SetupServices();
        var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();
        await orchestrator.StartAsync();

        try
        {
            // Ingest data to generate metrics
            for (int i = 0; i < 100; i++)
            {
                var point = new DataPoint(i, DateTime.UtcNow.Ticks, i * 0.1m, "S1");
                await orchestrator.IngestDataPointAsync(point);
            }

            await Task.Delay(1500);

            // Act
            var metrics = orchestrator.GetMetricsHistory();

            // Assert
            Assert.NotNull(metrics);
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }
}
