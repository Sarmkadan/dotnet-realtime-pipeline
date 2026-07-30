// entire file content ...
// ... goes in between

using Xunit;
using DotNetRealtimePipeline.Configuration;
using DotNetRealtimePipeline.Domain.Models;

namespace DotNetRealtimePipeline.Tests.Configuration;

public class PipelineConfigurationBuilderValidationTests
{
    [Fact]
    public void Validate_ReturnsEmptyList_ForValidConfig()
    {
        // Arrange
        var builder = new PipelineConfigurationBuilder();

        // Act
        var problems = PipelineConfigurationBuilderValidation.Validate(builder);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_ReturnsList_ForInvalidConfig()
    {
        // Arrange
        var builder = new PipelineConfigurationBuilder
        {
            PipelineName = string.Empty,
            Version = string.Empty,
            MaxBufferSize = 0,
            BufferFlushIntervalMs = 0,
            MaxConcurrentConsumers = 0,
            WindowSizeMs = 0,
            WindowSlideMs = 0,
            WindowType = string.Empty,
            MaxRetries = -1,
            RetryDelayMs = -1,
            ProcessingTimeoutMs = -1,
            BackpressureTriggerThreshold = -1,
            MinDataQualityThreshold = -1,
            Stages = new List<Stage>(),
            CustomSettings = new Dictionary<string, object>()
        };

        // Act
        var problems = PipelineConfigurationBuilderValidation.Validate(builder);

        // Assert
        Assert.Single(problems);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_ForNullBuilder()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => PipelineConfigurationBuilderValidation.Validate(null));
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForValidConfig()
    {
        // Arrange
        var builder = new PipelineConfigurationBuilder();

        // Act
        var isValid = PipelineConfigurationBuilderValidation.IsValid(builder);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForInvalidConfig()
    {
        // Arrange
        var builder = new PipelineConfigurationBuilder
        {
            PipelineName = string.Empty,
            Version = string.Empty,
            MaxBufferSize = 0,
            BufferFlushIntervalMs = 0,
            MaxConcurrentConsumers = 0,
            WindowSizeMs = 0,
            WindowSlideMs = 0,
            WindowType = string.Empty,
            MaxRetries = -1,
            RetryDelayMs = -1,
            ProcessingTimeoutMs = -1,
            BackpressureTriggerThreshold = -1,
            MinDataQualityThreshold = -1,
            Stages = new List<Stage>(),
            CustomSettings = new Dictionary<string, object>()
        };

        // Act
        var isValid = PipelineConfigurationBuilderValidation.IsValid(builder);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_ForInvalidConfig()
    {
        // Arrange
        var builder = new PipelineConfigurationBuilder
        {
            PipelineName = string.Empty,
            Version = string.Empty,
            MaxBufferSize = 0,
            BufferFlushIntervalMs = 0,
            MaxConcurrentConsumers = 0,
            WindowSizeMs = 0,
            WindowSlideMs = 0,
            WindowType = string.Empty,
            MaxRetries = -1,
            RetryDelayMs = -1,
            ProcessingTimeoutMs = -1,
            BackpressureTriggerThreshold = -1,
            MinDataQualityThreshold = -1,
            Stages = new List<Stage>(),
            CustomSettings = new Dictionary<string, object>()
        };

        // Act and Assert
        Assert.Throws<ArgumentException>(() => PipelineConfigurationBuilderValidation.EnsureValid(builder));
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_ForValidConfig()
    {
        // Arrange
        var builder = new PipelineConfigurationBuilder();

        // Act and Assert
        PipelineConfigurationBuilderValidation.EnsureValid(builder);
    }
}
