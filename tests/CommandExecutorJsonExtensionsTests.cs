using System;
using Xunit;
using DotNetRealtimePipeline.CLI;

namespace DotNetRealtimePipeline.CLI.Tests;

public class CommandExecutorJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var executor = new CommandExecutor();

        // Act
        var json = executor.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        // The JSON should be a valid object representation
        Assert.StartsWith("{", json);
        Assert.EndsWith("}", json);
    }

    [Fact]
    public void ToJson_NullValue_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((CommandExecutor?)null)!.ToJson());
    }

    [Fact]
    public void ToJson_IndentedTrue_ReturnsPrettyJson()
    {
        // Arrange
        var executor = new CommandExecutor();

        // Act
        var json = executor.ToJson(indented: true);

        // Assert
        Assert.Contains(Environment.NewLine, json);
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsCommandExecutor()
    {
        // Arrange
        var executor = new CommandExecutor();
        var json = executor.ToJson();

        // Act
        var result = CommandExecutorJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        // Round‑trip should produce the same JSON representation
        Assert.Equal(json, result!.ToJson());
    }

    [Fact]
    public void FromJson_NullOrWhiteSpace_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CommandExecutorJsonExtensions.FromJson(null!));
        Assert.Throws<ArgumentException>(() => CommandExecutorJsonExtensions.FromJson(""));
        Assert.Throws<ArgumentException>(() => CommandExecutorJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsNull()
    {
        // Arrange
        var invalidJson = "not a valid json";

        // Act
        var result = CommandExecutorJsonExtensions.FromJson(invalidJson);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndValue()
    {
        // Arrange
        var executor = new CommandExecutor();
        var json = executor.ToJson();

        // Act
        var success = CommandExecutorJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(json, result!.ToJson());
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "invalid json";

        // Act
        var success = CommandExecutorJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullOrWhiteSpace_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CommandExecutorJsonExtensions.TryFromJson(null!, out _));
        Assert.Throws<ArgumentException>(() => CommandExecutorJsonExtensions.TryFromJson("", out _));
        Assert.Throws<ArgumentException>(() => CommandExecutorJsonExtensions.TryFromJson("   ", out _));
    }
}
