using Xunit;
using DotNetRealtimePipeline.Workers;
using System.Threading.Tasks;

namespace DotNetRealtimePipeline.Tests.Workers;

public class DynamicScalingWorkerExtensionsTests
{
    [Fact]
    public async Task StartAndWaitAsync_HappyPath()
    {
        // Arrange
        var worker = new DynamicScalingWorker();

        // Act
        await worker.StartAndWaitAsync();

        // Assert
        Assert.True(worker.IsRunning);
    }

    [Fact]
    public async Task StartAndWaitAsync_NullWorker_ThrowsArgumentNullException()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => DynamicScalingWorkerExtensions.StartAndWaitAsync(null));
    }

    [Fact]
    public async Task StartAndWaitAsync_WorkerAlreadyRunning_ThrowsInvalidOperationException()
    {
        // Arrange
        var worker = new DynamicScalingWorker();
        worker.Start();

        // Act and Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => DynamicScalingWorkerExtensions.StartAndWaitAsync(worker));
    }

    [Fact]
    public async Task StartAndWaitAsync_WorkerFailedToStart_ThrowsInvalidOperationException()
    {
        // Arrange
        var worker = new DynamicScalingWorker();
        worker.Start();

        // Act and Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => DynamicScalingWorkerExtensions.StartAndWaitAsync(worker));
    }

    [Fact]
    public async Task StopAndWaitAsync_HappyPath()
    {
        // Arrange
        var worker = new DynamicScalingWorker();
        worker.Start();

        // Act
        await worker.StopAndWaitAsync();

        // Assert
        Assert.False(worker.IsRunning);
    }

    [Fact]
    public async Task StopAndWaitAsync_NullWorker_ThrowsArgumentNullException()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => DynamicScalingWorkerExtensions.StopAndWaitAsync(null));
    }

    [Fact]
    public async Task StopAndWaitAsync_WorkerNotRunning_ThrowsInvalidOperationException()
    {
        // Arrange
        var worker = new DynamicScalingWorker();

        // Act and Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => DynamicScalingWorkerExtensions.StopAndWaitAsync(worker));
    }

    [Fact]
    public async Task RestartAsync_HappyPath()
    {
        // Arrange
        var worker = new DynamicScalingWorker();
        worker.Start();

        // Act
        await worker.RestartAsync();

        // Assert
        Assert.True(worker.IsRunning);
    }

    [Fact]
    public async Task RestartAsync_NullWorker_ThrowsArgumentNullException()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => DynamicScalingWorkerExtensions.RestartAsync(null));
    }

    [Fact]
    public async Task RestartAsync_WorkerNotRunning_ThrowsInvalidOperationException()
    {
        // Arrange
        var worker = new DynamicScalingWorker();

        // Act and Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => DynamicScalingWorkerExtensions.RestartAsync(worker));
    }
}
