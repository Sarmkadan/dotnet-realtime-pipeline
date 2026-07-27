using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

public class WindowEventTests
{
    private const long WindowId = 42;
    private const long StartMs = 1_000;
    private const long EndMs = 2_000;
    private const string AggType = "tumbling";

    [Fact]
    public void ParameterizedConstructor_SetsAllPropertiesCorrectly()
    {
        // Act
        var window = new WindowEvent(WindowId, StartMs, EndMs, AggType);

        // Assert
        window.WindowId.Should().Be(WindowId);
        window.WindowStartMs.Should().Be(StartMs);
        window.WindowEndMs.Should().Be(EndMs);
        window.AggregationType.Should().Be(AggType);
        window.DataPoints.Should().BeEmpty();
        window.IsComplete.Should().BeFalse();
        window.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        window.CreatedAtTicks.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetDurationMs_ReturnsCorrectDifference()
    {
        var window = new WindowEvent(WindowId, StartMs, EndMs, AggType);
        window.GetDurationMs().Should().Be(EndMs - StartMs);
    }

    [Fact]
    public void TryAddDataPoint_ValidPoint_ReturnsTrueAndAdds()
    {
        var window = new WindowEvent(WindowId, StartMs, EndMs, AggType);
        var dp = new DataPoint { Timestamp = 1_500, Value = 10.0 };

        var result = window.TryAddDataPoint(dp);

        result.Should().BeTrue();
        window.DataPoints.Should().ContainSingle().Which.Should().Be(dp);
    }

    [Fact]
    public void TryAddDataPoint_OutOfBounds_ReturnsFalseAndDoesNotAdd()
    {
        var window = new WindowEvent(WindowId, StartMs, EndMs, AggType);
        var before = new DataPoint { Timestamp = 500, Value = 5.0 };
        var after = new DataPoint { Timestamp = 2_500, Value = 15.0 };

        window.TryAddDataPoint(before).Should().BeFalse();
        window.TryAddDataPoint(after).Should().BeFalse();

        window.DataPoints.Should().BeEmpty();
    }

    [Fact]
    public void TryAddDataPoint_Null_ThrowsArgumentNullException()
    {
        var window = new WindowEvent(WindowId, StartMs, EndMs, AggType);
        Action act = () => window.TryAddDataPoint(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CalculationMethods_WithDataPoints_ReturnCorrectValues()
    {
        var window = new WindowEvent(WindowId, StartMs, EndMs, AggType);
        var points = new[]
        {
            new DataPoint { Timestamp = 1_100, Value = 10.0 },
            new DataPoint { Timestamp = 1_500, Value = 20.0 },
            new DataPoint { Timestamp = 1_900, Value = 30.0 }
        };

        foreach (var p in points) window.TryAddDataPoint(p);

        window.CalculateAverage().Should().BeApproximately(20.0, 0.0001);
        window.CalculateSum().Should().Be(60.0);
        window.CalculateMin().Should().Be(10.0);
        window.CalculateMax().Should().Be(30.0);
        window.CalculateStandardDeviation().Should().BeApproximately(8.1649658, 0.0001);
    }

    [Fact]
    public void CalculationMethods_EmptyCollection_ReturnZero()
    {
        var window = new WindowEvent(WindowId, StartMs, EndMs, AggType);

        window.CalculateAverage().Should().Be(0.0);
        window.CalculateSum().Should().Be(0.0);
        window.CalculateMin().Should().Be(0.0);
        window.CalculateMax().Should().Be(0.0);
        window.CalculateStandardDeviation().Should().Be(0.0);
    }

    [Fact]
    public void MarkComplete_SetsIsCompleteTrue()
    {
        var window = new WindowEvent(WindowId, StartMs, EndMs, AggType);
        window.IsComplete.Should().BeFalse();

        window.MarkComplete();

        window.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void GetMetadata_ReturnsAllExpectedEntries()
    {
        var window = new WindowEvent(WindowId, StartMs, EndMs, AggType);
        var dp = new DataPoint { Timestamp = 1_500, Value = 42.0 };
        window.TryAddDataPoint(dp);
        window.MarkComplete();

        var meta = window.GetMetadata();

        meta.Should().ContainKey("WindowId").WhichValue.Should().Be(WindowId);
        meta.Should().ContainKey("StartMs").WhichValue.Should().Be(StartMs);
        meta.Should().ContainKey("EndMs").WhichValue.Should().Be(EndMs);
        meta.Should().ContainKey("DurationMs").WhichValue.Should().Be(EndMs - StartMs);
        meta.Should().ContainKey("DataPointCount").WhichValue.Should().Be(1);
        meta.Should().ContainKey("AggregationType").WhichValue.Should().Be(AggType);
        meta.Should().ContainKey("IsComplete").WhichValue.Should().BeTrue();
        meta.Should().ContainKey("Average").WhichValue.Should().Be(42.0);
        meta.Should().ContainKey("Sum").WhichValue.Should().Be(42.0);
        meta.Should().ContainKey("Min").WhichValue.Should().Be(42.0);
        meta.Should().ContainKey("Max").WhichValue.Should().Be(42.0);
    }
}
