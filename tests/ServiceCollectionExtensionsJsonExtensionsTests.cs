#nullable enable

using System;
using System.Text.Json;
using DotNetRealtimePipeline.Configuration;
using Xunit;

namespace DotNetRealtimePipeline.Configuration.Tests;

public class ServiceCollectionExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_ReturnsValidJson_WithDefaultSettings()
    {
        // Arrange
        object dummyInput = new();

        // Act
        var json = dummyInput.ToJson();

        // Assert
        Assert.NotNull(json);
        // Verify camelCase naming policy is applied and expected fields exist
        Assert.Contains("\"type\":\"ServiceCollectionExtensions\"", json);
        Assert.Contains("\"isStaticClass\":true", json);
        Assert.Contains("\"supportsAddPipelineServices\":true", json);
    }

    [Fact]
    public void ToJson_ReturnsIndentedJson_WhenIndentedTrue()
    {
        // Arrange
        object dummyInput = new();

        // Act
        var json = dummyInput.ToJson(indented: true);

        // Assert
        Assert.Contains('\n', json);
        Assert.Contains("  ", json); // Check for indentation
    }

    [Fact]
    public void ToJson_ThrowsArgumentNullException_WhenInputIsNull()
    {
        // Arrange
        object? input = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => input!.ToJson());
    }

    [Fact]
    public void FromJson_ReturnsMarker_WhenJsonIsValid()
    {
        // Arrange
        const string json = "{\"type\":\"ServiceCollectionExtensions\",\"isStaticClass\":true,\"supportsAddPipelineServices\":true}";

        // Act
        var result = ServiceCollectionExtensionsJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ServiceCollectionExtensions", result!.Type);
        Assert.True(result.IsStaticClass);
        Assert.True(result.SupportsAddPipelineServices);
    }

    [Fact]
    public void FromJson_ReturnsNull_WhenJsonIsInvalid()
    {
        // Arrange
        const string json = "not valid json";

        // Act
        var result = ServiceCollectionExtensionsJsonExtensions.FromJson(json);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_ThrowsArgumentNullException_WhenJsonIsNull()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensionsJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void TryFromJson_ReturnsTrueAndMarker_WhenJsonIsValid()
    {
        // Arrange
        const string json = "{\"type\":\"ServiceCollectionExtensions\",\"isStaticClass\":true,\"supportsAddPipelineServices\":true}";

        // Act
        var success = ServiceCollectionExtensionsJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal("ServiceCollectionExtensions", result!.Type);
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_WhenJsonIsInvalid()
    {
        // Arrange
        const string json = "invalid";

        // Act
        var success = ServiceCollectionExtensionsJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }
}
