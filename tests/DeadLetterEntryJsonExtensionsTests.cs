namespace DotNetRealtimePipeline.Tests.Unit;

using System;
using System.Text.Json;
using Xunit;
using DotNetRealtimePipeline.DeadLetter;

public class DeadLetterEntryJsonExtensionsTests
{
    private static DeadLetterEntry CreateSampleEntry()
    {
        return new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: new DataPoint(),
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: "type",
            ExceptionMessage: "message",
            RetryCount: 1,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow);
    }

    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var entry = CreateSampleEntry();

        // Act
        var json = entry.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
    }

    [Fact]
    public void ToJson_Indented_ReturnsIndentedJson()
    {
        // Arrange
        var entry = CreateSampleEntry();

        // Act
        var json = entry.ToJson(indented: true);

        // Assert
        Assert.Contains("\n", json);
        Assert.Contains("  ", json); // at least two spaces for indentation
    }

    [Fact]
    public void FromJson_HappyPath_RoundTripProducesSameJson()
    {
        // Arrange
        var original = CreateSampleEntry();
        var json = original.ToJson();

        // Act
        var deserialized = DeadLetterEntryJsonExtensions.FromJson(json);
        var roundTripJson = deserialized!.ToJson();

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(json, roundTripJson);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DeadLetterEntryJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_EmptyJson_ReturnsNull()
    {
        // Act
        var result = DeadLetterEntryJsonExtensions.FromJson(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act & Assert
        Assert.Throws<JsonException>(() => DeadLetterEntryJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndEntry()
    {
        // Arrange
        var original = CreateSampleEntry();
        var json = original.ToJson();

        // Act
        var success = DeadLetterEntryJsonExtensions.TryFromJson(json, out var result);

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
        var success = DeadLetterEntryJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }
}
