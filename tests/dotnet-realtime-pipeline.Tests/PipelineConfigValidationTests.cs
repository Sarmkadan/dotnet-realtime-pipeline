#nullable enable

using System;
using DotNetRealtimePipeline.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Unit tests for <see cref="PipelineConfigValidation"/> covering <see cref="PipelineConfig"/> validation rules.
/// </summary>
public class PipelineConfigValidationTests
{
    /// <summary>
    /// Builds a configuration that satisfies every validation rule.
    /// </summary>
    private static PipelineConfig CreateValidConfig()
    {
        var config = new PipelineConfig(1, "orders", "1.0.0");
        config.Stages.Add(new PipelineStageDef("parse", "Transform"));
        return config;
    }

    [Fact]
    public void ValidateConfig_WithValidConfig_ReturnsNoErrors()
    {
        // Arrange
        var config = CreateValidConfig();

        // Act
        var errors = config.ValidateConfig();

        // Assert
        errors.Should().BeEmpty();
        config.IsValid().Should().BeTrue();
        Action ensure = () => config.EnsureValid();
        ensure.Should().NotThrow();
    }

    [Fact]
    public void ValidateConfig_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange
        PipelineConfig? config = null;

        // Act & Assert
        Action validate = () => config.ValidateConfig();
        validate.Should().Throw<ArgumentNullException>();

        Action isValid = () => config.IsValid();
        isValid.Should().Throw<ArgumentNullException>();

        Action ensure = () => config.EnsureValid();
        ensure.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateConfig_WithDefaultConstructedConfig_ReportsMissingRequiredFields()
    {
        // Arrange
        var config = new PipelineConfig();

        // Act
        var errors = config.ValidateConfig();

        // Assert
        errors.Should().Contain(e => e.StartsWith("PipelineName must be", StringComparison.Ordinal));
        errors.Should().Contain(e => e.StartsWith("Version must be", StringComparison.Ordinal));
        errors.Should().Contain(e => e.StartsWith("ConfigId must be", StringComparison.Ordinal));
        errors.Should().Contain("Stages collection must contain at least one stage.");
        errors.Should().Contain("CreatedAt must be set to a valid DateTime.");
        errors.Should().Contain("LastModifiedAt must be set to a valid DateTime.");
        config.IsValid().Should().BeFalse();
    }

    [Theory]
    [InlineData("sliding", true)]
    [InlineData("  Session ", true)]
    [InlineData("HOPPING", false)]
    [InlineData("", false)]
    public void ValidateConfig_WindowType_IsCaseInsensitiveAndRejectsUnknownValues(string windowType, bool expectedValid)
    {
        // Arrange
        var config = CreateValidConfig();
        config.WindowType = windowType;

        // Act & Assert
        config.IsValid().Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(100, true)]
    [InlineData(-0.01, false)]
    [InlineData(100.01, false)]
    public void ValidateConfig_BackpressureThreshold_AcceptsInclusiveZeroToHundred(double threshold, bool expectedValid)
    {
        // Arrange
        var config = CreateValidConfig();
        config.BackpressureTriggerThreshold = threshold;

        // Act & Assert
        config.IsValid().Should().Be(expectedValid);
    }

    [Fact]
    public void ValidateConfig_WithNullOrMalformedStagesAndSettings_ReportsEachProblem()
    {
        // Arrange: null collections
        var nullStages = CreateValidConfig();
        nullStages.Stages = null!;

        var nullSettings = CreateValidConfig();
        nullSettings.CustomSettings = null!;

        // Arrange: malformed stage entries; the null element is added last because validation stops at it
        var badStages = CreateValidConfig();
        badStages.Stages.Add(new PipelineStageDef("", "Transform"));
        badStages.Stages.Add(new PipelineStageDef("filter", "") { ExecutionOrder = -1 });
        badStages.Stages.Add(null!);

        var nullValue = CreateValidConfig();
        nullValue.CustomSettings["retry"] = null!;

        // Act & Assert
        nullStages.ValidateConfig().Should().Contain("Stages collection must not be null.");
        nullSettings.ValidateConfig().Should().Contain("CustomSettings collection must not be null.");
        nullValue.ValidateConfig().Should().Contain("CustomSettings collection contains a null value for key 'retry'.");

        var stageErrors = badStages.ValidateConfig();
        stageErrors.Should().Contain("StageName in Stages collection must be a non-empty string.");
        stageErrors.Should().Contain("StageType in Stages collection must be a non-empty string.");
        stageErrors.Should().Contain("ExecutionOrder in Stages collection must be a non-negative integer (got -1).");
        stageErrors.Should().Contain("Stages collection contains a null element.");
    }

    [Fact]
    public void ValidateConfig_WithOutOfRangeNumericsAndWindowing_ReportsErrors()
    {
        // Arrange
        var config = CreateValidConfig();
        config.ConfigId = 0;
        config.MaxBufferSize = 0;
        config.MaxRetries = -1;
        config.RetryDelayMs = 0;
        config.WindowSizeMs = 1000;
        config.WindowSlideMs = 2000;

        // Act
        var errors = config.ValidateConfig();

        // Assert: zero is allowed for non-negative fields only, slide must not exceed size
        errors.Should().ContainSingle(e => e.StartsWith("ConfigId must be a positive", StringComparison.Ordinal));
        errors.Should().ContainSingle(e => e.StartsWith("MaxBufferSize must be a positive", StringComparison.Ordinal));
        errors.Should().ContainSingle(e => e.StartsWith("MaxRetries must be a non-negative", StringComparison.Ordinal));
        errors.Should().NotContain(e => e.StartsWith("RetryDelayMs", StringComparison.Ordinal));
        errors.Should().Contain("WindowSlideMs must be less than or equal to WindowSizeMs for proper windowing behavior.");
    }

    [Fact]
    public void ValidateConfig_WithInconsistentTimestamps_ReportsErrors()
    {
        // Arrange
        var lastModifiedBeforeCreated = CreateValidConfig();
        lastModifiedBeforeCreated.LastModifiedAt = lastModifiedBeforeCreated.CreatedAt.AddMinutes(-1);

        var createdInFuture = CreateValidConfig();
        createdInFuture.CreatedAt = DateTime.UtcNow.AddHours(1);
        createdInFuture.LastModifiedAt = createdInFuture.CreatedAt;

        // Act & Assert
        lastModifiedBeforeCreated.ValidateConfig().Should().Contain("LastModifiedAt cannot be earlier than CreatedAt.");
        createdInFuture.ValidateConfig().Should().Contain("CreatedAt cannot be in the future.");
    }

    [Fact]
    public void EnsureValid_WithInvalidConfig_ThrowsArgumentExceptionListingErrors()
    {
        // Arrange
        var config = CreateValidConfig();
        config.PipelineName = "";

        // Act
        Action act = () => config.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Pipeline configuration is invalid:*PipelineName must be a non-empty string.*");
        config.IsValid().Should().BeFalse();
    }
}
