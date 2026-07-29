// tests/BackpressureMetricsCollectorExtensionsTests.cs
namespace DotNetRealtimePipeline.Tests;

using System;
using DotNetRealtimePipeline.Metrics;
using Xunit;

public class BackpressureMetricsCollectorExtensionsTests
{
    [Fact]
    public void GetTotalActivations_NullCollector_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BackpressureMetricsCollectorExtensions.GetTotalActivations(null!));
    }

    [Fact]
    public void GetTotalActivations_HappyPath_ReturnsZeroWhenNoActivations()
    {
        // Arrange
        var collector = new BackpressureMetricsCollector();

        // Act
        var total = collector.GetTotalActivations();

        // Assert
        Assert.Equal(0L, total);
    }

    [Fact]
    public void GetTotalDroppedItems_NullCollector_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BackpressureMetricsCollectorExtensions.GetTotalDroppedItems(null!));
    }

    [Fact]
    public void GetTotalDroppedItems_HappyPath_ReturnsZeroWhenNoDrops()
    {
        // Arrange
        var collector = new BackpressureMetricsCollector();

        // Act
        var total = collector.GetTotalDroppedItems();

        // Assert
        Assert.Equal(0L, total);
    }

    [Fact]
    public void GetOverallPeakBufferFillPercent_EmptyStageMetrics_ThrowsInvalidOperationException()
    {
        // Arrange
        var collector = new BackpressureMetricsCollector();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => collector.GetOverallPeakBufferFillPercent());
    }

    [Fact]
    public void GetOverallPeakBufferFillPercent_NullCollector_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BackpressureMetricsCollectorExtensions.GetOverallPeakBufferFillPercent(null!));
    }

    [Fact]
    public void GetRecentActivationEvents_NullCollector_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BackpressureMetricsCollectorExtensions.GetRecentActivationEvents(null!));
    }

    [Fact]
    public void GetRecentActivationEvents_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var collector = new BackpressureMetricsCollector();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => collector.GetRecentActivationEvents(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => collector.GetRecentActivationEvents(-5));
    }

    [Fact]
    public void GetRecentActivationEvents_HappyPath_ReturnsEmptyWhenNoActivations()
    {
        // Arrange
        var collector = new BackpressureMetricsCollector();

        // Act
        var events = collector.GetRecentActivationEvents();

        // Assert
        Assert.NotNull(events);
        Assert.Empty(events);
    }
}
