// tests/ProcessingResultJsonExtensionsTests.cs

using Xunit;

namespace DotNetRealtimePipeline.Tests.Domain.Models;

public class ProcessingResultJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var processingResult = new ProcessingResult();
        var expectedJson = "{\"property\":\"value\"}";

        // Act
        var actualJson = processingResult.ToJson();

        // Assert
        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new ProcessingResult().ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsDeserializedProcessingResult()
    {
        // Arrange
        var json = "{\"property\":\"value\"}";
        var expectedProcessingResult = new ProcessingResult();

        // Act
        var actualProcessingResult = ProcessingResultJsonExtensions.FromJson(json);

        // Assert
        Assert.Equal(expectedProcessingResult, actualProcessingResult);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ProcessingResultJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyJson_ReturnsNull()
    {
        // Act
        var actualProcessingResult = ProcessingResultJsonExtensions.FromJson("");

        // Assert
        Assert.Null(actualProcessingResult);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrue()
    {
        // Arrange
        var json = "{\"property\":\"value\"}";

        // Act
        var result = ProcessingResultJsonExtensions.TryFromJson(json, out var _);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var result = ProcessingResultJsonExtensions.TryFromJson(null, out var _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsFalse()
    {
        // Act
        var result = ProcessingResultJsonExtensions.TryFromJson("", out var _);

        // Assert
        Assert.False(result);
    }
}
