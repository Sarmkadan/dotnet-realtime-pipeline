// tests/BackpressureMetricsCollectorJsonExtensionsTests.cs
namespace DotNetRealtimePipeline.Tests.Metrics;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetRealtimePipeline.Metrics;
using Xunit;

public class BackpressureMetricsCollectorJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJson()
    {
        // Arrange
        var collector = new BackpressureMetricsCollector(default!, 1);

        // Act
        var json = BackpressureMetricsCollectorJsonExtensions.ToJson(collector);

        // Assert
        Assert.NotNull(json);
    }

    [Fact]
    public void ToJson_NullCollector_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BackpressureMetricsCollectorJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsCollector()
    {
        // Arrange
        var json = "{\"StageMetrics\":[]}";
        var collector = new BackpressureMetricsCollector(default!, 1);

        // Act
        var result = BackpressureMetricsCollectorJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => BackpressureMetricsCollectorJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyJson_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => BackpressureMetricsCollectorJsonExtensions.FromJson(""));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsCollector()
    {
        // Arrange
        var json = "{\"StageMetrics\":[]}";
        var collector = new BackpressureMetricsCollector(default!, 1);

        // Act
        var result = BackpressureMetricsCollectorJsonExtensions.TryFromJson(json, out var value);

        // Assert
        Assert.True(result);
        Assert.NotNull(value);
    }

    [Fact]
    public void TryFromJson_NullJson_ReturnsFalse()
    {
        // Act
        var result = BackpressureMetricsCollectorJsonExtensions.TryFromJson(null, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsFalse()
    {
        // Act
        var result = BackpressureMetricsCollectorJsonExtensions.TryFromJson("", out _);

        // Assert
        Assert.False(result);
    }
}
