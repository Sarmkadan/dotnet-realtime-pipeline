// tests/PipelineOrchestratorValidationTests.cs
namespace DotNetRealtimePipeline.Tests;

public class PipelineOrchestratorValidationTests
{
    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var pipelineOrchestrator = new PipelineOrchestrator(new PipelineConfig(), new PipelineMetrics());

        // Act
        var problems = PipelineOrchestratorValidation.Validate(pipelineOrchestrator);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineOrchestratorValidation.Validate(null));
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var pipelineOrchestrator = new PipelineOrchestrator(new PipelineConfig(), new PipelineMetrics());

        // Act
        var isValid = PipelineOrchestratorValidation.IsValid(pipelineOrchestrator);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineOrchestratorValidation.IsValid(null));
    }

    [Fact]
    public void IsValid_InvalidInput_ReturnsFalse()
    {
        // Arrange
        var pipelineOrchestrator = new PipelineOrchestrator(new PipelineConfig(), new PipelineMetrics());
        pipelineOrchestrator.ConfigurationName = null;

        // Act
        var isValid = PipelineOrchestratorValidation.IsValid(pipelineOrchestrator);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var pipelineOrchestrator = new PipelineOrchestrator(new PipelineConfig(), new PipelineMetrics());

        // Act & Assert
        Assert.DoesNotThrow(() => PipelineOrchestratorValidation.EnsureValid(pipelineOrchestrator));
    }

    [Fact]
    public void EnsureValid_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineOrchestratorValidation.EnsureValid(null));
    }

    [Fact]
    public void EnsureValid_InvalidInput_ThrowsArgumentException()
    {
        // Arrange
        var pipelineOrchestrator = new PipelineOrchestrator(new PipelineConfig(), new PipelineMetrics());
        pipelineOrchestrator.ConfigurationName = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PipelineOrchestratorValidation.EnsureValid(pipelineOrchestrator));
    }
}
