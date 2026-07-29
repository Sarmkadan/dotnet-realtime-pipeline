using System;
using DotNetRealtimePipeline.Services;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

public class SlidingWindowAggregatorExtensionsTests
{
    [Fact]
    public void AddWithCurrentTimestamp_HappyPath_AddsDataPoint()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator();

        // Act
        aggregator.AddWithCurrentTimestamp(10.5);

        // Assert
        Assert.Single(aggregator.DataPoints);
    }

    [Fact]
    public void AddWithCurrentTimestamp_NullAggregator_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((SlidingWindowAggregator)null)!.AddWithCurrentTimestamp(10.5));
    }

    [Fact]
    public void AddRangeWithCurrentTimestamps_HappyPath_AddsDataPoints()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator();

        // Act
        aggregator.AddRangeWithCurrentTimestamps(new[] { 10.5, 20.5, 30.5 });

        // Assert
        Assert.Equal(3, aggregator.DataPoints.Count);
    }

    [Fact]
    public void AddRangeWithCurrentTimestamps_NullAggregator_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((SlidingWindowAggregator)null)!.AddRangeWithCurrentTimestamps(new[] { 10.5, 20.5, 30.5 }));
    }

    [Fact]
    public void AddRangeWithCurrentTimestamps_NullValues_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((SlidingWindowAggregator)null)!.AddRangeWithCurrentTimestamps(null));
    }

    [Fact]
    public void AddRangeWithCurrentTimestamps_EmptyValues_DoesNothing()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator();

        // Act
        aggregator.AddRangeWithCurrentTimestamps(new double[0]);

        // Assert
        Assert.Empty(aggregator.DataPoints);
    }

    [Fact]
    public void ToCsv_HappyPath_ReturnsCsvString()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator();
        aggregator.AddWithCurrentTimestamp(10.5);
        aggregator.AddWithCurrentTimestamp(20.5);

        // Act
        var csv = aggregator.ToCsv();

        // Assert
        Assert.Contains("WindowId", csv);
        Assert.Contains("WindowStartMs", csv);
        Assert.Contains("WindowEndMs", csv);
        Assert.Contains("WindowSizeMs", csv);
        Assert.Contains("StepIntervalMs", csv);
        Assert.Contains("DataPointCount", csv);
        Assert.Contains("Average", csv);
        Assert.Contains("Sum", csv);
        Assert.Contains("Min", csv);
        Assert.Contains("Max", csv);
        Assert.Contains("Trend", csv);
    }

    [Fact]
    public void ToCsv_NullAggregator_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((SlidingWindowAggregator)null)!.ToCsv());
    }
}
