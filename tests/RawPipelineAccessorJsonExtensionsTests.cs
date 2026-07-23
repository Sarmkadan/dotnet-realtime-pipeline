// tests/RawPipelineAccessorJsonExtensionsTests.cs
namespace DotNetRealtimePipeline.Tests.Integration;

using System;
using Xunit;
using DotNetRealtimePipeline.Integration;

public class RawPipelineAccessorJsonExtensionsTests
{
    [Fact]
    public void ToJson_ReturnsNonEmptyJson_ForValidAccessor()
    {
        // Arrange
        var accessor = new RawPipelineAccessor();

        // Act
        var json = accessor.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.StartsWith("{", json);
        Assert.EndsWith("}", json);
    }

    [Fact]
    public void ToJson_Indents_WhenRequested()
    {
        // Arrange
        var accessor = new RawPipelineAccessor();

        // Act
        var json = accessor.ToJson(indented: true);

        // Assert
        // Indented JSON contains line breaks; check for at least one newline character.
        Assert.Contains("\n", json);
    }

    [Fact]
    public void FromJson_ReturnsAccessor_ForValidJson()
    {
        // Arrange
        var accessor = new RawPipelineAccessor();
        var json = accessor.ToJson();

        // Act
        var deserialized = RawPipelineAccessorJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
    }

    [Fact]
    public void FromJson_ReturnsNull_ForWhitespaceJson()
    {
        // Arrange
        var whitespaceJson = "   ";

        // Act
        var result = RawPipelineAccessorJsonExtensions.FromJson(whitespaceJson);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_ThrowsArgumentException_ForNullOrEmpty()
    {
        // Null input
        Assert.Throws<ArgumentException>(() => RawPipelineAccessorJsonExtensions.FromJson(null!));

        // Empty string input
        Assert.Throws<ArgumentException>(() => RawPipelineAccessorJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void TryFromJson_ReturnsTrueAndOutputsAccessor_ForValidJson()
    {
        // Arrange
        var accessor = new RawPipelineAccessor();
        var json = accessor.ToJson();

        // Act
        var success = RawPipelineAccessorJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_ForInvalidJson()
    {
        // Arrange
        var invalidJson = "this is not json";

        // Act
        var success = RawPipelineAccessorJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_ForWhitespaceJson()
    {
        // Arrange
        var whitespaceJson = "   ";

        // Act
        var success = RawPipelineAccessorJsonExtensions.TryFromJson(whitespaceJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }
}
