namespace DotNetRealtimePipeline.Tests.Unit;

using System;
using System.Text.Json;
using Xunit;
using DotNetRealtimePipeline.DeadLetter;

public class DeadLetterQueueJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var queue = new DeadLetterQueue();

        // Act
        var json = queue.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
    }

    [Fact]
    public void ToJson_Indented_ReturnsIndentedJson()
    {
        // Arrange
        var queue = new DeadLetterQueue();

        // Act
        var json = queue.ToJson(indented: true);

        // Assert
        Assert.Contains("\n", json);
        Assert.Contains("  ", json);
    }

    [Fact]
    public void ToJson_NullValue_ThrowsArgumentNullException()
    {
        // Arrange
        DeadLetterQueue? queue = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => queue!.ToJson());
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsQueue()
    {
        // Arrange
        var original = new DeadLetterQueue();
        var json = original.ToJson();

        // Act
        var result = DeadLetterQueueJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        // Ensure round‑trip produces the same JSON representation
        var roundTrip = result!.ToJson();
        Assert.Equal(json, roundTrip);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DeadLetterQueueJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_EmptyJson_ReturnsNull()
    {
        // Act
        var result = DeadLetterQueueJsonExtensions.FromJson(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act & Assert
        Assert.Throws<JsonException>(() => DeadLetterQueueJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndQueue()
    {
        // Arrange
        var original = new DeadLetterQueue();
        var json = original.ToJson();

        // Act
        var success = DeadLetterQueueJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(json, result!.ToJson());
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ invalid }";

        // Act
        var success = DeadLetterQueueJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DeadLetterQueueJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsFalseAndNull()
    {
        // Act
        var success = DeadLetterQueueJsonExtensions.TryFromJson(string.Empty, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }
}
