// tests/HealthCheckServiceTests.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using DotNetRealtimePipeline.Monitoring;

namespace DotNetRealtimePipeline.Tests.Monitoring;

public class HealthCheckServiceTests
{
    [Fact]
    public async Task PerformCompleteHealthCheckAsync_HappyPath_ReturnsHealthyReport()
    {
        // Arrange
        var orchestrator = new Mock<PipelineOrchestrator>();
        var logger = new Mock<ILogger<HealthCheckService>>();
        var healthCheckService = new HealthCheckService(orchestrator.Object, logger.Object);

        // Act
        var report = await healthCheckService.PerformCompleteHealthCheckAsync();

        // Assert
        Assert.NotNull(report);
        Assert.Equal(SystemHealth.Healthy, report.OverallStatus);
    }

    [Fact]
    public async Task PerformCompleteHealthCheckAsync_NullOrchestrator_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = new Mock<ILogger<HealthCheckService>>();
        var healthCheckService = new HealthCheckService(null, logger.Object);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => healthCheckService.PerformCompleteHealthCheckAsync());
    }

    [Fact]
    public async Task PerformCompleteHealthCheckAsync_EmptyComponents_ReturnsUnknownReport()
    {
        // Arrange
        var orchestrator = new Mock<PipelineOrchestrator>();
        var logger = new Mock<ILogger<HealthCheckService>>();
        var healthCheckService = new HealthCheckService(orchestrator.Object, logger.Object);

        // Act
        var report = await healthCheckService.PerformCompleteHealthCheckAsync();

        // Assert
        Assert.NotNull(report);
        Assert.Equal(SystemHealth.Unknown, report.OverallStatus);
    }

    [Fact]
    public async Task GetQuickStatusAsync_HappyPath_ReturnsQuickStatus()
    {
        // Arrange
        var orchestrator = new Mock<PipelineOrchestrator>();
        var logger = new Mock<ILogger<HealthCheckService>>();
        var healthCheckService = new HealthCheckService(orchestrator.Object, logger.Object);

        // Act
        var status = await healthCheckService.GetQuickStatusAsync();

        // Assert
        Assert.NotNull(status);
        Assert.True(status.IsRunning);
    }

    [Fact]
    public async Task GetComponentStatus_HappyPath_ReturnsHealthyStatus()
    {
        // Arrange
        var orchestrator = new Mock<PipelineOrchestrator>();
        var logger = new Mock<ILogger<HealthCheckService>>();
        var healthCheckService = new HealthCheckService(orchestrator.Object, logger.Object);

        // Act
        var status = healthCheckService.GetComponentStatus("TestComponent");

        // Assert
        Assert.NotNull(status);
        Assert.Equal(ComponentStatus.Healthy, status);
    }

    [Fact]
    public async Task GetComponentStatus_NullComponentName_ReturnsUnknownStatus()
    {
        // Arrange
        var orchestrator = new Mock<PipelineOrchestrator>();
        var logger = new Mock<ILogger<HealthCheckService>>();
        var healthCheckService = new HealthCheckService(orchestrator.Object, logger.Object);

        // Act
        var status = healthCheckService.GetComponentStatus(null);

        // Assert
        Assert.NotNull(status);
        Assert.Equal(ComponentStatus.Unknown, status);
    }
}
