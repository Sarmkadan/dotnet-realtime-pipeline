// tests/PipelineVisualizationNodeJsonExtensionsTests.cs

using System;
using Xunit;
using DotNetRealtimePipeline.Visualization;

namespace DotNetRealtimePipeline.Tests.Visualization;

public class PipelineVisualizationNodeJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsValidJson()
    {
        // Arrange
        var node = new PipelineVisualizationNode();

        // Act
        var json = node.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.StartsWith("{", json);
        Assert.EndsWith("}", json);
    }

    [Fact]
    public void ToJson_WithIndent_ReturnsIndentedJson()
    {
        // Arrange
        var node = new PipelineVisualizationNode();

        // Act
        var jsonIndented = node.ToJson(indented: true);
        var jsonNonIndented = node.ToJson(indented: false);

        // Assert
        Assert.NotEqual(jsonNonIndented, jsonIndented);
        // Indented JSON should contain a newline character
        Assert.Contains("\n", jsonIndented);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((PipelineVisualizationNode)null!).ToJson());
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineVisualizationNodeJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsNull()
    {
        // Act
        var result = PipelineVisualizationNodeJsonExtensions.FromJson("invalid json");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_ValidJson_RoundTrip_ReturnsDeserializedNode()
    {
        // Arrange
        var original = new PipelineVisualizationNode();
        var json = original.ToJson();

        // Act
        var deserialized = PipelineVisualizationNodeJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<PipelineVisualizationNode>(deserialized);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndValue()
    {
        // Arrange
        var original = new PipelineVisualizationNode();
        var json = original.ToJson();

        // Act
        var success = PipelineVisualizationNodeJsonExtensions.TryFromJson(json, out var value);

        // Assert
        Assert.True(success);
        Assert.NotNull(value);
        Assert.IsType<PipelineVisualizationNode>(value);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var success = PipelineVisualizationNodeJsonExtensions.TryFromJson(null!, out var value);

        // Assert
        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Act
        var success = PipelineVisualizationNodeJsonExtensions.TryFromJson("invalid json", out var value);

        // Assert
        Assert.False(success);
        Assert.Null(value);
    }
}
