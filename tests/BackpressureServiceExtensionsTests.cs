namespace DotNetRealtimePipeline.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Services;
using Xunit;

public class BackpressureServiceExtensionsTests
{
    [Fact]
    public void GetOrCreateContext_HappyPath_ReturnsContext()
    {
        // Arrange
        var service = new BackpressureService();

        // Act
        var context = BackpressureServiceExtensions.GetOrCreateContext(service, "stageName", 1000);

        // Assert
        Assert.NotNull(context);
    }

    [Fact]
    public void SafeAddToBuffer_HappyPath_ReturnsTrue()
    {
        // Arrange
        var service = new BackpressureService();

        // Act
        var result = BackpressureServiceExtensions.SafeAddToBuffer(service, "stageName", 10);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetBufferFillPercentage_HappyPath_ReturnsPercentage()
    {
        // Arrange
        var service = new BackpressureService();

        // Act
        var percentage = BackpressureServiceExtensions.GetBufferFillPercentage(service, "stageName");

        // Assert
        Assert.InRange(percentage, 0, 100);
    }

    [Fact]
    public async Task TryRegisterConsumerAsync_HappyPath_ReturnsTrue()
    {
        // Arrange
        var service = new BackpressureService();

        // Act
        var result = await BackpressureServiceExtensions.TryRegisterConsumerAsync(service, "stageName");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetBufferStatusReport_HappyPath_ReturnsReport()
    {
        // Arrange
        var service = new BackpressureService();

        // Act
        var report = BackpressureServiceExtensions.GetBufferStatusReport(service);

        // Assert
        Assert.NotEmpty(report);
    }

    [Fact]
    public void GetOrCreateContext_NullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BackpressureServiceExtensions.GetOrCreateContext(null, "stageName", 1000));
    }

    [Fact]
    public void SafeAddToBuffer_NullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BackpressureServiceExtensions.SafeAddToBuffer(null, "stageName", 10));
    }

    [Fact]
    public void GetBufferFillPercentage_NullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BackpressureServiceExtensions.GetBufferFillPercentage(null, "stageName"));
    }

    [Fact]
    public async Task TryRegisterConsumerAsync_NullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => BackpressureServiceExtensions.TryRegisterConsumerAsync(null, "stageName"));
    }
}
