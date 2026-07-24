using Xunit;

namespace DotNetRealtimePipeline.Tests.Domain.Models;

public class ProcessingResultTests
{
    [Fact]
    public void Constructor_HappyPath_ReturnsProcessingResult()
    {
        // Arrange
        var resultId = 1;
        var success = true;
        var stageName = "Test Stage";

        // Act
        var result = new ProcessingResult(resultId, success, stageName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resultId, result.ResultId);
        Assert.Equal(success, result.Success);
        Assert.Equal(stageName, result.StageName);
    }

    [Fact]
    public void Constructor_WithNullStageName_ThrowsArgumentNullException()
    {
        // Arrange
        var resultId = 1;
        var success = true;
        var stageName = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new ProcessingResult(resultId, success, stageName));
    }

    [Fact]
    public void MarkFailure_HappyPath_MarksResultAsFailure()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "Test Stage");

        // Act
        result.MarkFailure("Test error message");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Test error message", result.ErrorMessage);
    }

    [Fact]
    public void MarkSuccess_HappyPath_MarksResultAsSuccess()
    {
        // Arrange
        var result = new ProcessingResult(1, false, "Test Stage");

        // Act
        result.MarkSuccess();

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void AddOutput_HappyPath_AddsOutputData()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "Test Stage");

        // Act
        result.AddOutput("Test Key", "Test Value");

        // Assert
        Assert.Single(result.OutputData);
        Assert.Equal("Test Value", result.OutputData["Test Key"]);
    }

    [Fact]
    public void GetOutput_HappyPath_ReturnsOutputData()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "Test Stage");
        result.AddOutput("Test Key", "Test Value");

        // Act
        var output = result.GetOutput("Test Key");

        // Assert
        Assert.Equal("Test Value", output);
    }

    [Fact]
    public void IncrementRetryCount_HappyPath_IncrementsRetryCount()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "Test Stage");

        // Act
        result.IncrementRetryCount();

        // Assert
        Assert.Equal(1, result.RetryCount);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "Test Stage");

        // Act
        var isValid = result.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WithInvalidResultId_ReturnsFalse()
    {
        // Arrange
        var result = new ProcessingResult(0, true, "Test Stage");

        // Act
        var isValid = result.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_WithNullStageName_ReturnsFalse()
    {
        // Arrange
        var result = new ProcessingResult(1, true, null);

        // Act
        var isValid = result.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void GetSummary_HappyPath_ReturnsSummary()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "Test Stage");

        // Act
        var summary = result.GetSummary();

        // Assert
        Assert.Equal("Result[Id=1, Stage=Test Stage, Success=True, ProcessingTime=0ms, Retries=0]", summary);
    }
}
