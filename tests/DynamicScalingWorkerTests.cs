// tests/DynamicScalingWorkerTests.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Workers;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Workers;

public class DynamicScalingWorkerTests
{
    private static Mock<DynamicScalingService> CreateMockScalingService()
    {
        // Create mocks for the dependencies required by DynamicScalingService's constructor.
        var backpressureServiceMock = new Mock<BackpressureService>();
        var pipelineConfigMock = new Mock<PipelineConfig>();
        var loggerMock = new Mock<ILogger<DynamicScalingService>>();

        // The real constructor expects many parameters; we pass the mocks (or simple values)
        // and let Moq create a mock of the concrete class without invoking the base constructor.
        var mock = new Mock<DynamicScalingService>(
            backpressureServiceMock.Object,
            pipelineConfigMock.Object,
            loggerMock.Object,
            minConsumers: 1,
            maxConsumers: 4,
            scaleUpThresholdPercent: 75.0,
            scaleDownThresholdPercent: 30.0,
            cooldownSeconds: 5);

        // Ensure EvaluateScalingAsync simply returns a completed task.
        mock.Setup(s => s.EvaluateScalingAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    [Fact]
    public void Constructor_NullScalingService_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = Mock.Of<ILogger<DynamicScalingWorker>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DynamicScalingWorker(null!, logger));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var scalingService = CreateMockScalingService().Object;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DynamicScalingWorker(scalingService, null!));
    }

    [Fact]
    public void Start_WhenNotRunning_SetsIsRunningTrue()
    {
        // Arrange
        var scalingService = CreateMockScalingService().Object;
        var logger = Mock.Of<ILogger<DynamicScalingWorker>>();
        var worker = new DynamicScalingWorker(scalingService, logger, intervalMs: 10);

        // Act
        worker.Start();

        // Assert
        Assert.True(worker.IsRunning);
    }

    [Fact]
    public void Start_WhenAlreadyRunning_LogsWarningAndDoesNotThrow()
    {
        // Arrange
        var scalingService = CreateMockScalingService().Object;
        var loggerMock = new Mock<ILogger<DynamicScalingWorker>>();
        var worker = new DynamicScalingWorker(scalingService, loggerMock.Object, intervalMs: 10);

        // Act
        worker.Start(); // first start
        worker.Start(); // second start – should log a warning

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("already running")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        Assert.True(worker.IsRunning);
    }

    [Fact]
    public async Task StopAsync_WhenRunning_StopsWorker()
    {
        // Arrange
        var scalingService = CreateMockScalingService().Object;
        var logger = Mock.Of<ILogger<DynamicScalingWorker>>();
        var worker = new DynamicScalingWorker(scalingService, logger, intervalMs: 10);

        worker.Start();
        Assert.True(worker.IsRunning);

        // Act
        await worker.StopAsync();

        // Assert
        Assert.False(worker.IsRunning);
    }

    [Fact]
    public void Dispose_WhenRunning_CancelsAndStops()
    {
        // Arrange
        var scalingService = CreateMockScalingService().Object;
        var logger = Mock.Of<ILogger<DynamicScalingWorker>>();
        var worker = new DynamicScalingWorker(scalingService, logger, intervalMs: 10);

        worker.Start();
        Assert.True(worker.IsRunning);

        // Act
        worker.Dispose();

        // Assert
        Assert.False(worker.IsRunning);
    }

    [Fact]
    public void AddDynamicScaling_NullServiceCollection_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddDynamicScaling());
    }

    [Fact]
    public void AddDynamicScaling_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDynamicScaling();

        // Assert
        var descriptors = services.ToList();

        // Expect a registration for DynamicScalingService and DynamicScalingWorker
        Assert.Contains(descriptors, d => d.ServiceType == typeof(DynamicScalingService));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(DynamicScalingWorker));
    }
}
