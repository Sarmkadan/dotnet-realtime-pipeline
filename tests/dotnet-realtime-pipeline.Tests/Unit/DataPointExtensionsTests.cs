#nullable enable
using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Domain.Models;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Tests for <see cref="DataPointExtensions"/>.
/// </summary>
public class DataPointExtensionsTests
{
    /// <summary>
    /// Creates a valid data point for testing.
    /// </summary>
    private DataPoint CreateValidDataPoint(long id = 1)
    {
        return new DataPoint(id, 1_000_000L, 42.5, "sensor-01") { Quality = 85 };
    }

    [Fact]
    public void TryGetMetadataValue_WithExistingKeyAndCorrectType_ReturnsTrue()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Metadata["unit"] = "celsius";
        dataPoint.Metadata["value"] = 42;

        // Act
        var result = dataPoint.TryGetMetadataValue<string>("unit", out var value);

        // Assert
        Assert.True(result);
        Assert.Equal("celsius", value);
    }

    [Fact]
    public void TryGetMetadataValue_WithExistingKeyButIncorrectType_ReturnsFalse()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Metadata["value"] = 42; // int

        // Act
        var result = dataPoint.TryGetMetadataValue<string>("value", out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetMetadataValue_WithNonExistingKey_ReturnsFalse()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();

        // Act
        var result = dataPoint.TryGetMetadataValue<string>("nonexistent", out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetMetadataValue_WithNullDataPoint_ThrowsArgumentNullException()
    {
        // Arrange
        DataPoint? dataPoint = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dataPoint!.TryGetMetadataValue<string>("key", out _));
    }

    [Fact]
    public void TryGetMetadataValue_WithNullOrWhiteSpaceKey_ThrowsArgumentException()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => dataPoint.TryGetMetadataValue<string>(null!, out _));
        Assert.Throws<ArgumentException>(() => dataPoint.TryGetMetadataValue<string>("", out _));
        Assert.Throws<ArgumentException>(() => dataPoint.TryGetMetadataValue<string>("   ", out _));
    }

    [Fact]
    public void ToLogString_WithoutMetadata_ReturnsFormattedString()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();

        // Act
        var result = dataPoint.ToLogString();

        // Assert
        Assert.Contains($"DataPoint[{dataPoint.Id}]", result);
        Assert.Contains($"Source: {dataPoint.Source}", result);
        Assert.Contains($"Timestamp: {DateTimeOffset.FromUnixTimeMilliseconds(dataPoint.Timestamp):O}", result);
        Assert.Contains($"Value: {dataPoint.Value:G}", result);
        Assert.Contains($"Quality: {dataPoint.Quality}%", result);
        Assert.DoesNotContain("| Metadata", result);
    }

    [Fact]
    public void ToLogString_WithMetadata_ReturnsFormattedStringWithMetadata()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Metadata.Add("unit", "celsius");
        dataPoint.Metadata.Add("count", 42);

        // Act
        var result = dataPoint.ToLogString(includeMetadata: true);

        // Assert
        Assert.Contains($"DataPoint[{dataPoint.Id}]", result);
        Assert.Contains($"Source: {dataPoint.Source}", result);
        Assert.Contains($"Timestamp: {DateTimeOffset.FromUnixTimeMilliseconds(dataPoint.Timestamp):O}", result);
        Assert.Contains($"Value: {dataPoint.Value:G}", result);
        Assert.Contains($"Quality: {dataPoint.Quality}%", result);
        Assert.Contains($"| Metadata[{dataPoint.Metadata.Count}]", result);
    }

    [Fact]
    public void ToLogString_WithNullDataPoint_ThrowsArgumentNullException()
    {
        // Arrange
        DataPoint? dataPoint = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dataPoint!.ToLogString());
    }

    [Fact]
    public void IsStale_WithOldDataPoint_ReturnsTrue()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        // Set timestamp to 10 seconds ago (in milliseconds)
        var tenSecondsAgo = DateTimeOffset.UtcNow.AddSeconds(-10).ToUnixTimeMilliseconds();
        dataPoint.Timestamp = tenSecondsAgo;

        // Act
        var result = dataPoint.IsStale(maxAgeMs: 5000); // 5 seconds

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsStale_WithRecentDataPoint_ReturnsFalse()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        // Set timestamp to 2 seconds ago
        var twoSecondsAgo = DateTimeOffset.UtcNow.AddSeconds(-2).ToUnixTimeMilliseconds();
        dataPoint.Timestamp = twoSecondsAgo;

        // Act
        var result = dataPoint.IsStale(maxAgeMs: 5000); // 5 seconds

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsStale_WithNullDataPoint_ThrowsArgumentNullException()
    {
        // Arrange
        DataPoint? dataPoint = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dataPoint!.IsStale(1000));
    }

    [Fact]
    public void IsStale_WithNegativeMaxAge_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => dataPoint.IsStale(-1));
    }

    [Fact]
    public void WithId_WithValidId_ReturnsNewDataPointWithNewId()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint(id: 5);
        var newId = 42L;

        // Act
        var result = dataPoint.WithId(newId);

        // Assert
        Assert.NotSame(dataPoint, result);
        Assert.Equal(newId, result.Id);
        Assert.Equal(dataPoint.Timestamp, result.Timestamp);
        Assert.Equal(dataPoint.Value, result.Value);
        Assert.Equal(dataPoint.Source, result.Source);
        Assert.Equal(dataPoint.Quality, result.Quality);
        Assert.Equal(dataPoint.CreatedAt, result.CreatedAt);
        Assert.Equal(dataPoint.Metadata.Count, result.Metadata.Count);
        foreach (var kvp in dataPoint.Metadata)
        {
            Assert.Equal(kvp.Value, result.Metadata[kvp.Key]);
        }
    }

    [Fact]
    public void WithId_WithZeroOrNegativeId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => dataPoint.WithId(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => dataPoint.WithId(-1));
    }

    [Fact]
    public void WithId_WithNullDataPoint_ThrowsArgumentNullException()
    {
        // Arrange
        DataPoint? dataPoint = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dataPoint!.WithId(1));
    }
}