// tests/PipelineHttpClientFactoryJsonExtensionsTests.cs
namespace DotNetRealtimePipeline.Tests.Integration;

using System;
using Xunit;
using DotNetRealtimePipeline.Integration;

public class PipelineHttpClientFactoryJsonExtensionsTests
{
    [Fact]
    public void ToJson_ReturnsNonEmptyJson_ForValidFactory()
    {
        // Arrange
        var factory = new PipelineHttpClientFactory();

        // Act
        var json = factory.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.StartsWith("{", json);
        Assert.EndsWith("}", json);
    }

    [Fact]
    public void ToJson_Indents_WhenRequested()
    {
        // Arrange
        var factory = new PipelineHttpClientFactory();

        // Act
        var json = factory.ToJson(indented: true);

        // Assert
        Assert.Contains("\n", json);
    }

    [Fact]
    public void ToJson_ThrowsArgumentNullException_ForNullFactory()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => PipelineHttpClientFactoryJsonExtensions.ToJson(null!));
    }

    [Fact]
    public void FromJson_ReturnsFactory_ForValidJson()
    {
        // Arrange
        var json = new PipelineHttpClientFactory().ToJson();

        // Act
        var deserialized = PipelineHttpClientFactoryJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
    }

    [Fact]
    public void FromJson_ReturnsNull_ForWhitespaceJson()
    {
        // Act
        var result = PipelineHttpClientFactoryJsonExtensions.FromJson("   ");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_ThrowsArgumentNullException_ForNullJson()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => PipelineHttpClientFactoryJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void TryFromJson_ReturnsTrueAndOutputsFactory_ForValidJson()
    {
        // Arrange
        var json = new PipelineHttpClientFactory().ToJson();

        // Act
        var success = PipelineHttpClientFactoryJsonExtensions.TryFromJson(json, out var value);

        // Assert
        Assert.True(success);
        Assert.NotNull(value);
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_ForWhitespaceJson()
    {
        // Act
        var success = PipelineHttpClientFactoryJsonExtensions.TryFromJson("   ", out var value);

        // Assert
        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_ReturnsFalseAndError_ForMalformedJson()
    {
        // Act
        var success = PipelineHttpClientFactoryJsonExtensions.TryFromJson("{ not json", out var value, out var error);

        // Assert
        Assert.False(success);
        Assert.Null(value);
        Assert.NotNull(error);
    }
}
