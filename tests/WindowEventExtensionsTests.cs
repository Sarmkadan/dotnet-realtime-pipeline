using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;
using DotNetRealtimePipeline.Domain.Models;

namespace DotNetRealtimePipeline.Tests.Domain.Models;

public class WindowEventExtensionsTests
{
    private static WindowEvent CreateSampleWindow()
    {
        return new WindowEvent
        {
            WindowId = "win-1",
            WindowStartMs = 1_000,
            WindowEndMs = 6_000,
            DataPoints = new List<DataPoint>
            {
                new DataPoint { Timestamp = DateTime.UtcNow.AddSeconds(1), Value = 10.0 },
                new DataPoint { Timestamp = DateTime.UtcNow.AddSeconds(2), Value = 20.0 },
                new DataPoint { Timestamp = DateTime.UtcNow.AddSeconds(3), Value = 30.0 },
                new DataPoint { Timestamp = DateTime.UtcNow.AddSeconds(4), Value = 40.0 },
                new DataPoint { Timestamp = DateTime.UtcNow.AddSeconds(5), Value = 50.0 }
            }
        };
    }

    [Fact]
    public void GetDuration_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var window = new WindowEvent
        {
            WindowStartMs = 1_000,
            WindowEndMs = 6_000,
            DataPoints = new List<DataPoint>()
        };

        // Act
        var duration = window.GetDuration();

        // Assert
        Assert.Equal(TimeSpan.FromMilliseconds(5_000), duration);
    }

    [Fact]
    public void GetDuration_NullWindow_ThrowsArgumentNullException()
    {
        // Arrange
        WindowEvent? window = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => window!.GetDuration());
    }

    [Fact]
    public void GetDataPointsSortedByTimestamp_ReturnsPointsInChronologicalOrder()
    {
        // Arrange
        var window = new WindowEvent
        {
            DataPoints = new List<DataPoint>
            {
                new DataPoint { Timestamp = new DateTime(2023, 1, 1, 0, 0, 5), Value = 5 },
                new DataPoint { Timestamp = new DateTime(2023, 1, 1, 0, 0, 1), Value = 1 },
                new DataPoint { Timestamp = new DateTime(2023, 1, 1, 0, 0, 3), Value = 3 }
            }
        };

        // Act
        var sorted = window.GetDataPointsSortedByTimestamp();

        // Assert
        Assert.Equal(3, sorted.Count);
        Assert.Equal(new DateTime(2023, 1, 1, 0, 0, 1), sorted[0].Timestamp);
        Assert.Equal(new DateTime(2023, 1, 1, 0, 0, 3), sorted[1].Timestamp);
        Assert.Equal(new DateTime(2023, 1, 1, 0, 0, 5), sorted[2].Timestamp);
    }

    [Fact]
    public void GetDataPointsSortedByTimestamp_NullWindow_ThrowsArgumentNullException()
    {
        WindowEvent? window = null;
        Assert.Throws<ArgumentNullException>(() => window!.GetDataPointsSortedByTimestamp());
    }

    [Fact]
    public void GetDataPointsSortedByTimestamp_NullDataPoints_ThrowsArgumentNullException()
    {
        var window = new WindowEvent { DataPoints = null! };
        Assert.Throws<ArgumentNullException>(() => window.GetDataPointsSortedByTimestamp());
    }

    [Fact]
    public void GetPercentile_HappyPath_ReturnsCorrectValues()
    {
        // Arrange
        var window = CreateSampleWindow();

        // Act / Assert
        Assert.Equal(10.0, window.GetPercentile(0.0));
        Assert.Equal(30.0, window.GetPercentile(50.0));
        Assert.Equal(50.0, window.GetPercentile(100.0));
    }

    [Fact]
    public void GetPercentile_EmptyDataPoints_ReturnsZero()
    {
        var window = new WindowEvent { DataPoints = new List<DataPoint>() };
        var result = window.GetPercentile(75.0);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void GetPercentile_NullWindow_ThrowsArgumentNullException()
    {
        WindowEvent? window = null;
        Assert.Throws<ArgumentNullException>(() => window!.GetPercentile(50.0));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(100.1)]
    public void GetPercentile_OutOfRange_ThrowsArgumentOutOfRangeException(double percentile)
    {
        var window = CreateSampleWindow();
        Assert.Throws<ArgumentOutOfRangeException>(() => window.GetPercentile(percentile));
    }

    [Fact]
    public void ToSummaryString_ReturnsExpectedFormattedString()
    {
        // Arrange
        var window = CreateSampleWindow();

        // Expected string built using the same format as the implementation
        var expected = string.Format(
            CultureInfo.InvariantCulture,
            "Window {0} [{1}-{2}] ({3} ms): Count={4}, Avg={5:F2}, Min={6:F2}, Max={7:F2}, StdDev={8:F2}",
            window.WindowId,
            window.WindowStartMs,
            window.WindowEndMs,
            window.GetDurationMs(),
            window.GetDataPointCount(),
            window.CalculateAverage(),
            window.CalculateMin(),
            window.CalculateMax(),
            window.CalculateStandardDeviation());

        // Act
        var actual = window.ToSummaryString();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToSummaryString_NullWindow_ThrowsArgumentNullException()
    {
        WindowEvent? window = null;
        Assert.Throws<ArgumentNullException>(() => window!.ToSummaryString());
    }
}
