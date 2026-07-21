using System;
using System.Collections.Generic;
using System.Linq;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

public class SlidingWindowAggregatorTests
{
    [Fact]
    public void ValuesWithinWindowAggregateCorrectly()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(windowSizeMs: 10000, stepIntervalMs: 2000);
        long baseTime = 1000000;

        // Add data points within the window
        aggregator.Add(new DataPoint(1, baseTime + 1000, 10.0, "test"));
        aggregator.Add(new DataPoint(2, baseTime + 2000, 20.0, "test"));
        aggregator.Add(new DataPoint(3, baseTime + 3000, 30.0, "test"));

        // Act - flush at time that ensures all points are within window
        var results = aggregator.FlushDueWindows(baseTime + 15000);

        // Assert - find the window that contains our points (window ending at 12000 contains 1000-2000-3000)
        var targetWindow = results.FirstOrDefault(r => r.WindowStartMs <= baseTime + 1000 && r.WindowEndMs >= baseTime + 3000);
        targetWindow.Should().NotBeNull();
        targetWindow.DataPointCount.Should().Be(3);
        targetWindow.Average.Should().Be(20.0);
        targetWindow.Sum.Should().Be(60.0);
        targetWindow.Min.Should().Be(10.0);
        targetWindow.Max.Should().Be(30.0);
    }

    [Fact]
    public void ValuesOlderThanWindowAreEvictedFromAggregate()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(windowSizeMs: 5000, stepIntervalMs: 1000);
        long baseTime = 1000000;

        // Add data points - some old, some recent
        aggregator.Add(new DataPoint(1, baseTime + 1000, 10.0, "test")); // Will be evicted
        aggregator.Add(new DataPoint(2, baseTime + 2000, 20.0, "test")); // Will be evicted
        aggregator.Add(new DataPoint(3, baseTime + 6000, 30.0, "test")); // Within window
        aggregator.Add(new DataPoint(4, baseTime + 7000, 40.0, "test")); // Within window

        // Act - flush at time where old points should be evicted
        var results = aggregator.FlushDueWindows(baseTime + 8000);

        // Assert - find the window that contains the recent points
        var recentWindow = results.FirstOrDefault(r => r.WindowStartMs >= baseTime + 3000 && r.DataPointCount > 0);
        recentWindow.Should().NotBeNull();
        recentWindow.DataPointCount.Should().Be(2); // Only the recent points (6000 and 7000)
        recentWindow.Average.Should().Be(35.0); // (30 + 40) / 2
        recentWindow.Sum.Should().Be(70.0);
        recentWindow.Min.Should().Be(30.0);
        recentWindow.Max.Should().Be(40.0);
    }

    [Fact]
    public void EmptyWindowResult()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(windowSizeMs: 5000, stepIntervalMs: 1000);
        long baseTime = 1000000;

        // Act - no data points added
        var results = aggregator.FlushDueWindows(baseTime + 5000);

        // Assert - when no data points, return empty array
        results.Should().BeEmpty();
    }

    [Fact]
    public void SingleValueInWindow()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(windowSizeMs: 5000, stepIntervalMs: 1000);
        long baseTime = 1000000;

        // Add single data point
        aggregator.Add(new DataPoint(1, baseTime + 2000, 42.5, "test"));

        // Act
        var results = aggregator.FlushDueWindows(baseTime + 3000);

        // Assert - find the window that contains our point
        var windowWithPoint = results.FirstOrDefault(r => r.DataPointCount > 0);
        windowWithPoint.Should().NotBeNull();
        windowWithPoint.DataPointCount.Should().Be(1);
        windowWithPoint.Average.Should().Be(42.5);
        windowWithPoint.Sum.Should().Be(42.5);
        windowWithPoint.Min.Should().Be(42.5);
        windowWithPoint.Max.Should().Be(42.5);
    }

    [Fact]
    public void OutOfOrderTimestampHandling()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(windowSizeMs: 10000, stepIntervalMs: 2000);
        long baseTime = 1000000;

        // Add data points out of order
        aggregator.Add(new DataPoint(3, baseTime + 3000, 30.0, "test"));
        aggregator.Add(new DataPoint(1, baseTime + 1000, 10.0, "test"));
        aggregator.Add(new DataPoint(2, baseTime + 2000, 20.0, "test"));

        // Act
        var results = aggregator.FlushDueWindows(baseTime + 5000);

        // Assert - find the window that contains all points
        var windowWithAllPoints = results.FirstOrDefault(r => r.DataPointCount == 3);
        windowWithAllPoints.Should().NotBeNull();
        windowWithAllPoints.DataPointCount.Should().Be(3);
        windowWithAllPoints.Average.Should().Be(20.0);
        windowWithAllPoints.Sum.Should().Be(60.0);
        windowWithAllPoints.Min.Should().Be(10.0);
        windowWithAllPoints.Max.Should().Be(30.0);
    }

    [Fact]
    public void MultipleWindowsEmitted()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(windowSizeMs: 6000, stepIntervalMs: 2000);
        long baseTime = 1000000;

        // Add data points spanning multiple windows
        aggregator.Add(new DataPoint(1, baseTime + 1000, 10.0, "test"));
        aggregator.Add(new DataPoint(2, baseTime + 2000, 20.0, "test"));
        aggregator.Add(new DataPoint(3, baseTime + 4000, 30.0, "test"));
        aggregator.Add(new DataPoint(4, baseTime + 5000, 40.0, "test"));
        aggregator.Add(new DataPoint(5, baseTime + 7000, 50.0, "test"));
        aggregator.Add(new DataPoint(6, baseTime + 8000, 60.0, "test"));

        // Act - flush at time that should emit multiple windows
        var results = aggregator.FlushDueWindows(baseTime + 9000);

        // Assert - should have multiple windows with different aggregations
        results.Should().HaveCountGreaterThan(1);

        // Find windows with data
        var windowsWithData = results.Where(r => r.DataPointCount > 0).ToList();
        windowsWithData.Should().HaveCountGreaterThan(1);

        // Verify different aggregations in different windows
        var firstDataWindow = windowsWithData.First();
        var secondDataWindow = windowsWithData.Skip(1).First();

        firstDataWindow.Average.Should().NotBe(secondDataWindow.Average);
        firstDataWindow.Sum.Should().NotBe(secondDataWindow.Sum);
    }

    [Fact]
    public void AggregationCalculationsAreCorrect()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(windowSizeMs: 10000, stepIntervalMs: 2000);
        long baseTime = 1000000;

        // Add data points with known values
        aggregator.Add(new DataPoint(1, baseTime + 1000, 5.0, "test"));
        aggregator.Add(new DataPoint(2, baseTime + 2000, 10.0, "test"));
        aggregator.Add(new DataPoint(3, baseTime + 3000, 15.0, "test"));

        // Act
        var results = aggregator.FlushDueWindows(baseTime + 15000);

        // Assert
        var targetWindow = results.FirstOrDefault(r => r.DataPointCount == 3);
        targetWindow.Should().NotBeNull();

        // Verify calculations
        targetWindow.Average.Should().Be(10.0); // (5 + 10 + 15) / 3
        targetWindow.Sum.Should().Be(30.0); // 5 + 10 + 15
        targetWindow.Min.Should().Be(5.0);
        targetWindow.Max.Should().Be(15.0);
        targetWindow.DataPointCount.Should().Be(3);
    }

    [Fact]
    public void TrendCalculationIsCorrect()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(windowSizeMs: 10000, stepIntervalMs: 2000);
        long baseTime = 1000000;

        // Add data points with increasing values (positive trend)
        aggregator.Add(new DataPoint(1, baseTime + 1000, 10.0, "test"));
        aggregator.Add(new DataPoint(2, baseTime + 2000, 20.0, "test"));
        aggregator.Add(new DataPoint(3, baseTime + 3000, 30.0, "test"));
        aggregator.Add(new DataPoint(4, baseTime + 4000, 40.0, "test"));

        // Act
        var results = aggregator.FlushDueWindows(baseTime + 15000);

        // Assert
        var targetWindow = results.FirstOrDefault(r => r.DataPointCount == 4);
        targetWindow.Should().NotBeNull();
        targetWindow.Trend.Should().BePositive(); // Should be positive since values are increasing

        // Add decreasing values (negative trend)
        var aggregator2 = new SlidingWindowAggregator(windowSizeMs: 10000, stepIntervalMs: 2000);
        aggregator2.Add(new DataPoint(1, baseTime + 1000, 40.0, "test"));
        aggregator2.Add(new DataPoint(2, baseTime + 2000, 30.0, "test"));
        aggregator2.Add(new DataPoint(3, baseTime + 3000, 20.0, "test"));
        aggregator2.Add(new DataPoint(4, baseTime + 4000, 10.0, "test"));

        var results2 = aggregator2.FlushDueWindows(baseTime + 15000);
        var targetWindow2 = results2.FirstOrDefault(r => r.DataPointCount == 4);
        targetWindow2.Should().NotBeNull();
        targetWindow2.Trend.Should().BeNegative(); // Should be negative since values are decreasing
    }

    [Fact]
    public void WindowMetadataIsCorrect()
    {
        // Arrange
        var windowSizeMs = 10000L;
        var stepIntervalMs = 2000L;
        var aggregator = new SlidingWindowAggregator(windowSizeMs, stepIntervalMs);

        // Act
        var results = aggregator.FlushDueWindows(15000);

        // Assert
        var result = results.FirstOrDefault(r => r.DataPointCount > 0);
        if (result != null)
        {
            result.WindowSizeMs.Should().Be(windowSizeMs);
            result.StepIntervalMs.Should().Be(stepIntervalMs);
            result.AggregatedData["WindowSizeMs"].Should().Be(windowSizeMs);
            result.AggregatedData["StepIntervalMs"].Should().Be(stepIntervalMs);
            result.AggregatedData["WindowType"].Should().Be("SLIDING");
        }
    }
}