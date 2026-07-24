// tests/PipelineVisualizerJsonExtensionsTests.cs

using Xunit;

namespace DotNetRealtimePipeline.Tests.Visualization;

public class PipelineVisualizerJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var visualizer = new PipelineVisualizer();
        var expectedJson = "{\"property\":\"value\"}";

        // Act
        var actualJson = visualizer.ToJson();

        // Assert
        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new PipelineVisualizer().ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsDeserializedVisualizer()
    {
        // Arrange
        var json = "{\"property\":\"value\"}";
        var expectedVisualizer = new PipelineVisualizer();

        // Act
        var actualVisualizer = PipelineVisualizerJsonExtensions.FromJson(json);

        // Assert
        Assert.Equal(expectedVisualizer, actualVisualizer);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => PipelineVisualizerJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsNull()
    {
        // Act
        var actualVisualizer = PipelineVisualizerJsonExtensions.FromJson("invalid json");

        // Assert
        Assert.Null(actualVisualizer);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrue()
    {
        // Arrange
        var json = "{\"property\":\"value\"}";

        // Act
        var result = PipelineVisualizerJsonExtensions.TryFromJson(json, out var _);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var result = PipelineVisualizerJsonExtensions.TryFromJson(null, out var _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Act
        var result = PipelineVisualizerJsonExtensions.TryFromJson("invalid json", out var _);

        // Assert
        Assert.False(result);
    }
}
