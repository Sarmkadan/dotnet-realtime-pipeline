#nullable enable

using System;
using DotNetRealtimePipeline.Metrics;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Unit tests for <see cref="BackpressureMetricsSnapshotExtensions"/>.
/// </summary>
public class BackpressureMetricsSnapshotExtensionsTests
{
    private static BackpressureMetricsSnapshot CreateSnapshot(params StageBackpressureMetrics[] stages)
    {
        var snapshot = new BackpressureMetricsSnapshot
        {
            SnapshotAt = DateTime.UtcNow,
            TotalActivations = 10,
            TotalDroppedItems = 5,
            ActiveBackpressureStages = 2,
            StageMetrics = new System.Collections.Generic.List<StageBackpressureMetrics>(stages)
        };
        return snapshot;
    }

    private static StageBackpressureMetrics CreateStage(string name, double currentFill, double peakFill = 100)
    {
        return new StageBackpressureMetrics
        {
            StageName = name,
            CurrentBufferFillPercent = currentFill,
            PeakBufferFillPercent = peakFill,
            ActivationCount = 1,
            TotalActiveDurationMs = 100,
            TotalDroppedItems = 0,
            LastActivationAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void IsCritical_WithStageAboveThreshold_ReturnsTrue()
    {
        // Arrange
        var snapshot = CreateSnapshot(CreateStage("stage1", 90), CreateStage("stage2", 50));

        // Act
        var result = snapshot.IsCritical(80);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCritical_WithAllStagesBelowThreshold_ReturnsFalse()
    {
        // Arrange
        var snapshot = CreateSnapshot(CreateStage("stage1", 70), CreateStage("stage2", 50));

        // Act
        var result = snapshot.IsCritical(80);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCritical_WithStageExactlyAtThreshold_ReturnsFalse()
    {
        // Arrange
        var snapshot = CreateSnapshot(CreateStage("stage1", 80), CreateStage("stage2", 50));

        // Act
        var result = snapshot.IsCritical(80);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCritical_WithEmptyStages_ReturnsFalse()
    {
        // Arrange
        var snapshot = CreateSnapshot();

        // Act
        var result = snapshot.IsCritical(80);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCritical_WithNullSnapshot_ThrowsArgumentNullException()
    {
        // Arrange
        BackpressureMetricsSnapshot? snapshot = null;

        // Act & Assert
        Action act = () => snapshot!.IsCritical(80);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void IsCritical_WithInvalidThreshold_ThrowsArgumentOutOfRangeException(double threshold)
    {
        // Arrange
        var snapshot = CreateSnapshot(CreateStage("stage1", 50));

        // Act & Assert
        Action act = () => snapshot.IsCritical(threshold);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UtilizationPercent_WithMultipleStages_ReturnsAverage()
    {
        // Arrange
        var snapshot = CreateSnapshot(CreateStage("stage1", 50), CreateStage("stage2", 100), CreateStage("stage3", 0));

        // Act
        var result = snapshot.UtilizationPercent();

        // Assert
        result.Should().BeApproximately(50.0, 0.001);
    }

    [Fact]
    public void UtilizationPercent_WithSingleStage_ReturnsThatStageFill()
    {
        // Arrange
        var snapshot = CreateSnapshot(CreateStage("stage1", 75.5));

        // Act
        var result = snapshot.UtilizationPercent();

        // Assert
        result.Should().BeApproximately(75.5, 0.001);
    }

    [Fact]
    public void UtilizationPercent_WithEmptyStages_ReturnsZero()
    {
        // Arrange
        var snapshot = CreateSnapshot();

        // Act
        var result = snapshot.UtilizationPercent();

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void UtilizationPercent_WithNullSnapshot_ThrowsArgumentNullException()
    {
        // Arrange
        BackpressureMetricsSnapshot? snapshot = null;

        // Act & Assert
        Action act = () => snapshot!.UtilizationPercent();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToDisplayString_ContainsKeyMetrics()
    {
        // Arrange
        var snapshot = CreateSnapshot(CreateStage("stage1", 60), CreateStage("stage2", 40));
        snapshot.SnapshotAt = new DateTime(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);
        snapshot.TotalActivations = 42;
        snapshot.TotalDroppedItems = 7;
        snapshot.ActiveBackpressureStages = 1;

        // Act
        var result = snapshot.ToDisplayString();

        // Assert
        result.Should().Contain("2026-11-04T12:00:00Z");
        result.Should().Contain("TotalActivations = 42");
        result.Should().Contain("TotalDroppedItems = 7");
        result.Should().Contain("ActiveStages = 1");
        result.Should().Contain("Utilization = 50.0%");
        result.Should().Contain("Critical = False");
    }

    [Fact]
    public void ToDisplayString_WithCriticalState_ShowsCriticalTrue()
    {
        // Arrange
        var snapshot = CreateSnapshot(CreateStage("stage1", 90));
        snapshot.SnapshotAt = new DateTime(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var result = snapshot.ToDisplayString();

        // Assert
        result.Should().Contain("Critical = True");
    }

    [Fact]
    public void ToDisplayString_WithNullSnapshot_ThrowsArgumentNullException()
    {
        // Arrange
        BackpressureMetricsSnapshot? snapshot = null;

        // Act & Assert
        Action act = () => snapshot!.ToDisplayString();
        act.Should().Throw<ArgumentNullException>();
    }
}