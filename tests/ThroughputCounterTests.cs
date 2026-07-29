// tests/ThroughputCounterTests.cs
namespace DotNetRealtimePipeline.Tests;

using System;
using Xunit;

public class ThroughputCounterTests
{
    [Fact]
    public void Constructor_HappyPath_ReturnsThroughputCounter()
    {
        // Arrange
        var windowSeconds = 60;

        // Act
        var throughputCounter = new ThroughputCounter(windowSeconds);

        // Assert
        Assert.NotNull(throughputCounter);
    }

    [Fact]
    public void RecordEvents_HappyPath_IncrementsCount()
    {
        // Arrange
        var throughputCounter = new ThroughputCounter();

        // Act
        throughputCounter.RecordEvents(10);

        // Assert
        Assert.Equal(10, throughputCounter.GetThroughput());
    }

    [Fact]
    public void RecordEvents_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ThroughputCounter().RecordEvents(null));
    }

    [Fact]
    public void RecordEvents_EmptyInput_DoesNothing()
    {
        // Arrange
        var throughputCounter = new ThroughputCounter();

        // Act
        throughputCounter.RecordEvents(0);

        // Assert
        Assert.Equal(0, throughputCounter.GetThroughput());
    }

    [Fact]
    public void GetThroughput_HappyPath_ReturnsThroughput()
    {
        // Arrange
        var throughputCounter = new ThroughputCounter();

        // Act
        throughputCounter.RecordEvents(10);

        // Assert
        Assert.Equal(10, throughputCounter.GetThroughput());
    }

    [Fact]
    public void GetThroughput_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ThroughputCounter().GetThroughput(null));
    }

    [Fact]
    public void GetThroughput_EmptyInput_ReturnsZero()
    {
        // Arrange
        var throughputCounter = new ThroughputCounter();

        // Act
        var throughput = throughputCounter.GetThroughput();

        // Assert
        Assert.Equal(0, throughput);
    }
}
