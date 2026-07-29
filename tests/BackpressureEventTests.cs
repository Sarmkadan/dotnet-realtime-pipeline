#nullable enable

using Xunit;
using System;
using DotNetRealtimePipeline.Metrics;

namespace DotNetRealtimePipeline.Metrics.Tests;

public class BackpressureEventTests
{
    [Fact]
    public void Constructor_Defaults_InitializesCorrectly()
    {
        // Arrange & Act
        var backpressureEvent = new BackpressureEvent();

        // Assert
        Assert.Equal(default(DateTime), backpressureEvent.Timestamp);
        Assert.Equal(string.Empty, backpressureEvent.StageName);
        Assert.Equal(0.0, backpressureEvent.BufferFillPercent);
        Assert.False(backpressureEvent.IsActivation);
        Assert.Equal(0L, backpressureEvent.DroppedItems);
    }

    [Fact]
    public void Properties_SetAndGet_WorksCorrectly()
    {
        // Arrange
        var expectedTimestamp = DateTime.UtcNow;
        const string expectedStageName = "IngestionStage";
        const double expectedBufferFill = 85.5;
        const long expectedDropped = 123L;

        // Act
        var backpressureEvent = new BackpressureEvent
        {
            Timestamp = expectedTimestamp,
            StageName = expectedStageName,
            BufferFillPercent = expectedBufferFill,
            IsActivation = true,
            DroppedItems = expectedDropped
        };

        // Assert
        Assert.Equal(expectedTimestamp, backpressureEvent.Timestamp);
        Assert.Equal(expectedStageName, backpressureEvent.StageName);
        Assert.Equal(expectedBufferFill, backpressureEvent.BufferFillPercent);
        Assert.True(backpressureEvent.IsActivation);
        Assert.Equal(expectedDropped, backpressureEvent.DroppedItems);
    }

    [Fact]
    public void BufferFillPercent_AcceptsBoundaryValues()
    {
        var backpressureEvent = new BackpressureEvent();

        // Act & Assert - Lower Bound
        backpressureEvent.BufferFillPercent = 0.0;
        Assert.Equal(0.0, backpressureEvent.BufferFillPercent);

        // Act & Assert - Upper Bound
        backpressureEvent.BufferFillPercent = 100.0;
        Assert.Equal(100.0, backpressureEvent.BufferFillPercent);
    }

    [Fact]
    public void DroppedItems_AcceptsLargeValues()
    {
        var backpressureEvent = new BackpressureEvent();

        // Act
        backpressureEvent.DroppedItems = long.MaxValue;

        // Assert
        Assert.Equal(long.MaxValue, backpressureEvent.DroppedItems);
    }

    [Fact]
    public void IsActivation_TogglesCorrectly()
    {
        var backpressureEvent = new BackpressureEvent();

        // Act & Assert - Default
        Assert.False(backpressureEvent.IsActivation);

        // Act & Assert - True
        backpressureEvent.IsActivation = true;
        Assert.True(backpressureEvent.IsActivation);

        // Act & Assert - False
        backpressureEvent.IsActivation = false;
        Assert.False(backpressureEvent.IsActivation);
    }
}
