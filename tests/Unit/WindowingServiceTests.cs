// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

public class WindowingServiceTests
{
    private readonly WindowingService _service;
    private readonly PipelineConfig _config;

    public WindowingServiceTests()
    {
        _config = new PipelineConfig
        {
            WindowType = WindowType.TUMBLING,
            WindowSizeMs = 5000,
            WindowSlideMs = 5000
        };
        _service = new WindowingService(_config);
    }

    [Fact]
    public void CreateWindow_WithValidTime_ShouldSucceed()
    {
        // Act
        var window = _service.CreateWindow(1000);

        // Assert
        Assert.NotNull(window);
        Assert.Equal(1000, window.StartTimeMs);
    }

    [Fact]
    public void AssignDataPointsToWindows_WithValidPoints_ShouldAssign()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var points = new[]
        {
            new DataPoint(1, now * 10000, 10, "S1"),
            new DataPoint(2, now * 10000, 20, "S1"),
            new DataPoint(3, (now + 2500) * 10000, 30, "S1")
        };

        // Act
        var windows = _service.AssignDataPointsToWindows(points);

        // Assert
        Assert.NotEmpty(windows);
    }

    [Fact]
    public void CalculateWindowStatistics_WithValidWindow_ShouldCalculate()
    {
        // Arrange
        var window = new WindowEvent
        {
            StartTimeMs = 1000,
            EndTimeMs = 6000,
            Data = new List<DataPoint>
            {
                new(1, 2000, 10, "S1"),
                new(2, 3000, 20, "S1"),
                new(3, 4000, 30, "S1")
            }
        };

        // Act
        var stats = _service.CalculateWindowStatistics(window);

        // Assert
        Assert.Equal(3, stats.Count);
        Assert.Equal(20, stats.Average);
        Assert.Equal(10, stats.Minimum);
        Assert.Equal(30, stats.Maximum);
    }

    [Fact]
    public void GetActiveWindows_ShouldReturnCurrent()
    {
        // Act
        var windows = _service.GetActiveWindows();

        // Assert
        Assert.NotNull(windows);
    }

    [Fact]
    public void CalculateWindowStatistics_ShouldComputeStandardDeviation()
    {
        // Arrange
        var window = new WindowEvent
        {
            StartTimeMs = 1000,
            EndTimeMs = 6000,
            Data = new List<DataPoint>
            {
                new(1, 2000, 10, "S1"),
                new(2, 3000, 20, "S1"),
                new(3, 4000, 30, "S1"),
                new(4, 5000, 40, "S1")
            }
        };

        // Act
        var stats = _service.CalculateWindowStatistics(window);

        // Assert
        Assert.True(stats.StandardDeviation > 0);
    }

    [Fact]
    public void CalculateWindowStatistics_ShouldComputePercentiles()
    {
        // Arrange
        var window = new WindowEvent
        {
            StartTimeMs = 1000,
            EndTimeMs = 6000,
            Data = Enumerable.Range(1, 100)
                .Select(i => new DataPoint(i, (1000 + i) * 10000, i, "S1"))
                .ToList()
        };

        // Act
        var stats = _service.CalculateWindowStatistics(window);

        // Assert
        Assert.True(stats.P50 >= stats.Minimum);
        Assert.True(stats.P95 >= stats.P50);
        Assert.True(stats.P99 <= stats.Maximum);
    }

    [Fact]
    public void CloseWindow_WithActiveWindow_ShouldArchive()
    {
        // Arrange
        var window = _service.CreateWindow(1000);
        window.Data.Add(new DataPoint(1, 2000, 42, "S1"));

        // Act
        _service.CloseWindow(window);
        var archived = _service.GetArchivedWindows();

        // Assert
        Assert.NotEmpty(archived);
    }
}
