namespace DotNetRealtimePipeline.Tests;

using System;
using System.Text;
using System.Text.Json;
using DotNetRealtimePipeline.Metrics;
using Xunit;

public class ThroughputCounterJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var counter = new ThroughputCounter(60);
        counter.RecordEvents(10);
        counter.RecordEvents("stage1", 5);

        // Act
        var json = counter.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"windowSeconds\":60", json);
        Assert.Contains("\"totalCount\":15", json);
        Assert.Contains("\"stageCount\":5", json);
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsIndentedJson()
    {
        // Arrange
        var counter = new ThroughputCounter(60);
        counter.RecordEvents(10);

        // Act
        var json = counter.ToJson(indented: true);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("{\n  ", json); // Check for indentation
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ThroughputCounterJsonExtensions.ToJson(null!));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsThroughputCounter()
    {
        // Arrange
        var original = new ThroughputCounter(30);
        original.RecordEvents(7);
        original.RecordEvents("stageA", 3);
        var json = original.ToJson();

        // Act
        var result = ThroughputCounterJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(30, typeof(ThroughputCounter).GetProperty("WindowSeconds")?.GetValue(result));
        Assert.Equal(10, result.GetThroughput()); // 7 + 3
        Assert.Equal(3, result.GetThroughput("stageA"));
    }

    [Fact]
    public void FromJson_NullOrEmptyInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ThroughputCounterJsonExtensions.FromJson(null!));
        Assert.Throws<ArgumentException>(() => ThroughputCounterJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Act & Assert
        Assert.Throws<JsonException>(() => ThroughputCounterJsonExtensions.FromJson("{ invalid json }"));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndValue()
    {
        // Arrange
        var original = new ThroughputCounter(10);
        original.RecordEvents(4);
        var json = original.ToJson();

        // Act
        var success = ThroughputCounterJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(4, result.GetThroughput());
    }

    [Fact]
    public void TryFromJson_NullOrEmptyInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ThroughputCounterJsonExtensions.TryFromJson(null!, out _));
        Assert.Throws<ArgumentException>(() => ThroughputCounterJsonExtensions.TryFromJson(string.Empty, out _));
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Act
        var success = ThroughputCounterJsonExtensions.TryFromJson("{ invalid json }", out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_ValidJsonButWrongType_ReturnsFalseAndNull()
    {
        // Arrange: JSON that is valid but not a ThroughputCounter
        var json = "\"just a string\"";

        // Act
        var success = ThroughputCounterJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }
}