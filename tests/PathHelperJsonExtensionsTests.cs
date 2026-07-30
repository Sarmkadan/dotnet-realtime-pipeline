// tests/PathHelperJsonExtensionsTests.cs
using System;
using System.Text.Json;
using Xunit;
using DotNetRealtimePipeline.Utilities;

namespace DotNetRealtimePipeline.Tests.Utilities;

public class PathHelperJsonExtensionsTests
{
    private static readonly PathHelper SamplePathHelper = new();

    [Fact]
    public void ToJson_WithValidInstance_ReturnsNonEmptyString()
    {
        // Act
        var json = SamplePathHelper.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        // The default serialization of an empty object should be "{}"
        Assert.Equal("{}", json);
    }

    [Fact]
    public void ToJson_WithIndentation_ProducesIndentedJson()
    {
        // Act
        var json = SamplePathHelper.ToJson(indented: true);

        // Assert
        Assert.Contains(Environment.NewLine, json);
        // Indented empty object should be "{\n  \n}"
        // We only assert that a newline exists; exact formatting may vary with options.
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    [Fact]
    public void ToJson_NullInstance_ThrowsArgumentNullException()
    {
        // Arrange
        PathHelper? nullHelper = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullHelper!.ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsPathHelperInstance()
    {
        // Arrange
        var json = SamplePathHelper.ToJson();

        // Act
        var result = PathHelperJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_EmptyOrWhitespaceJson_ReturnsNull()
    {
        // Act
        var result1 = PathHelperJsonExtensions.FromJson(string.Empty);
        var result2 = PathHelperJsonExtensions.FromJson("   ");

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PathHelperJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndInstance()
    {
        // Arrange
        var json = SamplePathHelper.ToJson();

        // Act
        var success = PathHelperJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act
        var success = PathHelperJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PathHelperJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsFalse()
    {
        // Act
        var success = PathHelperJsonExtensions.TryFromJson(string.Empty, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }
}
