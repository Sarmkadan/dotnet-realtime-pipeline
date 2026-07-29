namespace DotNetRealtimePipeline.Tests;

using System;
using System.Threading.Tasks;
using Xunit;

public class PipelineInitializerExtensionsTests
{
    [Fact]
    public async Task InitializeAndStartAsync_HappyPath_ReturnsInitializationResult()
    {
        // Arrange
        var initializer = new PipelineInitializer(new ServiceProvider(), new Logger(), new StateManager());
        var result = await PipelineInitializerExtensions.InitializeAndStartAsync(initializer);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task InitializeAndStartAsync_NullInitializer_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => PipelineInitializerExtensions.InitializeAndStartAsync(null));
    }

    [Fact]
    public async Task InitializeWithRetryAsync_HappyPath_ReturnsInitializationResult()
    {
        // Arrange
        var initializer = new PipelineInitializer(new ServiceProvider(), new Logger(), new StateManager());
        var result = await PipelineInitializerExtensions.InitializeWithRetryAsync(initializer);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task InitializeWithRetryAsync_NullInitializer_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => PipelineInitializerExtensions.InitializeWithRetryAsync(null));
    }

    [Fact]
    public async Task InitializeWithRetryAsync_MaxAttemptsZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var initializer = new PipelineInitializer(new ServiceProvider(), new Logger(), new StateManager());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => PipelineInitializerExtensions.InitializeWithRetryAsync(initializer, 0));
    }

    [Fact]
    public async Task InitializeWithRetryAsync_DelayBetweenAttemptsNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var initializer = new PipelineInitializer(new ServiceProvider(), new Logger(), new StateManager());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => PipelineInitializerExtensions.InitializeWithRetryAsync(initializer, 3, -100));
    }

    [Fact]
    public async Task SafeStopAsync_HappyPath_ReturnsTrue()
    {
        // Arrange
        var initializer = new PipelineInitializer(new ServiceProvider(), new Logger(), new StateManager());

        // Act
        var result = await PipelineInitializerExtensions.SafeStopAsync(initializer);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SafeStopAsync_NullInitializer_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => PipelineInitializerExtensions.SafeStopAsync(null));
    }

    [Fact]
    public void IsInitialized_HappyPath_ReturnsTrue()
    {
        // Arrange
        var initializer = new PipelineInitializer(new ServiceProvider(), new Logger(), new StateManager());
        initializer.IsInitialized = true;

        // Act
        var result = PipelineInitializerExtensions.IsInitialized(initializer);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInitialized_NullInitializer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineInitializerExtensions.IsInitialized(null));
    }

    [Fact]
    public void GetPipelineState_HappyPath_ReturnsInitializedString()
    {
        // Arrange
        var initializer = new PipelineInitializer(new ServiceProvider(), new Logger(), new StateManager());
        initializer.IsInitialized = true;

        // Act
        var result = PipelineInitializerExtensions.GetPipelineState(initializer);

        // Assert
        Assert.Equal("Initialized", result);
    }

    [Fact]
    public void GetPipelineState_NullInitializer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineInitializerExtensions.GetPipelineState(null));
    }
}
