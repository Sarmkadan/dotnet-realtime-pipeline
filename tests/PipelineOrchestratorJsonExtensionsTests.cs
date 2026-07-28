// tests/PipelineOrchestratorJsonExtensionsTests.cs
namespace DotNetRealtimePipeline.Tests;

public class PipelineOrchestratorJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var pipelineOrchestrator = new PipelineOrchestrator();
        var expectedJson = "{\"property1\":\"value1\",\"property2\":\"value2\"}";

        // Act
        var actualJson = pipelineOrchestrator.ToJson();

        // Assert
        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new PipelineOrchestrator().ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsPipelineOrchestratorInstance()
    {
        // Arrange
        var json = "{\"property1\":\"value1\",\"property2\":\"value2\"}";
        var expectedPipelineOrchestrator = new PipelineOrchestrator();

        // Act
        var actualPipelineOrchestrator = PipelineOrchestratorJsonExtensions.FromJson(json);

        // Assert
        Assert.Equal(expectedPipelineOrchestrator, actualPipelineOrchestrator);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => PipelineOrchestratorJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyJson_ThrowsJsonException()
    {
        // Act and Assert
        Assert.Throws<JsonException>(() => PipelineOrchestratorJsonExtensions.FromJson(""));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrue()
    {
        // Arrange
        var json = "{\"property1\":\"value1\",\"property2\":\"value2\"}";

        // Act
        var result = PipelineOrchestratorJsonExtensions.TryFromJson(json, out var _);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var result = PipelineOrchestratorJsonExtensions.TryFromJson(null, out var _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsFalse()
    {
        // Act
        var result = PipelineOrchestratorJsonExtensions.TryFromJson("", out var _);

        // Assert
        Assert.False(result);
    }
}
