// tests/WindowingServiceExtensionsTests.cs
namespace DotNetRealtimePipeline.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class WindowingServiceExtensionsTests
{
    [Fact]
    public void CreateCustomDurationWindow_HappyPath_ReturnsWindowEvent()
    {
        // Arrange
        var service = new WindowingService();

        // Act
        var windowEvent = WindowingServiceExtensions.CreateCustomDurationWindow(service, 1000, 1000);

        // Assert
        Assert.NotNull(windowEvent);
    }

    [Fact]
    public void ProcessDataPointsWithState_HappyPath_ReturnsEmittedAndActive()
    {
        // Arrange
        var service = new WindowingService();
        var dataPoints = new List<DataPoint> { new DataPoint(1, 1) };

        // Act
        var (emitted, active) = WindowingServiceExtensions.ProcessDataPointsWithState(service, dataPoints);

        // Assert
        Assert.NotNull(emitted);
        Assert.NotNull(active);
    }

    [Fact]
    public void CalculateCombinedWindowStatistics_HappyPath_ReturnsWindowStatistics()
    {
        // Arrange
        var service = new WindowingService();
        var windows = new List<WindowEvent> { new WindowEvent(1, 1000, 2000, "TEST") };

        // Act
        var statistics = WindowingServiceExtensions.CalculateCombinedWindowStatistics(service, windows);

        // Assert
        Assert.NotNull(statistics);
    }

    [Fact]
    public void GetCompleteWindows_HappyPath_ReturnsCompleteWindows()
    {
        // Arrange
        var service = new WindowingService();

        // Act
        var completeWindows = WindowingServiceExtensions.GetCompleteWindows(service);

        // Assert
        Assert.NotNull(completeWindows);
    }

    [Fact]
    public void GetActiveWindows_HappyPath_ReturnsActiveWindows()
    {
        // Arrange
        var service = new WindowingService();

        // Act
        var activeWindows = WindowingServiceExtensions.GetActiveWindows(service);

        // Assert
        Assert.NotNull(activeWindows);
    }

    [Fact]
    public void GetNextWindowId_HappyPath_ReturnsNextWindowId()
    {
        // Arrange
        var service = new WindowingService();

        // Act
        var nextWindowId = WindowingServiceExtensions.GetNextWindowId(service);

        // Assert
        Assert.True(nextWindowId > 0);
    }

    [Fact]
    public void CreateCustomDurationWindow_NullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => WindowingServiceExtensions.CreateCustomDurationWindow(null, 1000, 1000));
    }

    [Fact]
    public void ProcessDataPointsWithState_NullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => WindowingServiceExtensions.ProcessDataPointsWithState(null, new List<DataPoint>()));
    }
}
