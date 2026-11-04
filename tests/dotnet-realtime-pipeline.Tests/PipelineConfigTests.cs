#nullable enable

using System;
using DotNetRealtimePipeline.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Unit tests for <see cref="PipelineConfig"/> covering defaults, construction, validation, stages, custom settings and cloning.
/// </summary>
public class PipelineConfigTests
{
    [Fact]
    public void Constructor_Parameterless_AppliesDocumentedDefaults()
    {
        // Arrange & Act
        var config = new PipelineConfig();

        // Assert
        config.ConfigId.Should().Be(0);
        config.PipelineName.Should().BeEmpty();
        config.Version.Should().BeEmpty();
        config.MaxBufferSize.Should().Be(10000);
        config.BufferFlushIntervalMs.Should().Be(1000);
        config.MaxConcurrentConsumers.Should().Be(4);
        config.WindowSizeMs.Should().Be(5000);
        config.WindowSlideMs.Should().Be(1000);
        config.WindowType.Should().Be("TUMBLING");
        config.MaxRetries.Should().Be(3);
        config.IsActive.Should().BeTrue();
        config.Stages.Should().BeEmpty();
        config.CustomSettings.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithValues_SetsIdentityAndTimestamps()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var config = new PipelineConfig(42, "orders", "2.1.0");

        // Assert
        config.ConfigId.Should().Be(42);
        config.PipelineName.Should().Be("orders");
        config.Version.Should().Be("2.1.0");
        config.CreatedAt.Should().BeOnOrAfter(before);
        config.LastModifiedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Constructor_WithNullNameOrVersion_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Action nullName = () => new PipelineConfig(1, null!, "1.0.0");
        nullName.Should().Throw<ArgumentNullException>().WithParameterName("pipelineName");

        Action nullVersion = () => new PipelineConfig(1, "orders", null!);
        nullVersion.Should().Throw<ArgumentNullException>().WithParameterName("version");
    }

    [Fact]
    public void Validate_BoundaryAndInvalidValues_ReturnExpectedResult()
    {
        // Arrange
        var config = new PipelineConfig(1, "orders", "1.0.0");

        // Act & Assert: a default config is valid and thresholds are inclusive 0..100
        config.Validate().Should().BeTrue();
        config.MinDataQualityThreshold = 0;
        config.BackpressureTriggerThreshold = 100;
        config.Validate().Should().BeTrue();

        config.MinDataQualityThreshold = 101;
        config.Validate().Should().BeFalse();
        config.MinDataQualityThreshold = 70;

        config.WindowSlideMs = 0;
        config.Validate().Should().BeFalse();
        config.WindowSlideMs = 1000;

        config.PipelineName = "   ";
        config.Validate().Should().BeFalse();
        config.PipelineName = "orders";

        config.ConfigId = 0;
        config.Validate().Should().BeFalse();
    }

    [Fact]
    public void AddStage_ThenGetStageByName_FindsStageOrReturnsNull()
    {
        // Arrange
        var config = new PipelineConfig(1, "orders", "1.0.0");
        var before = DateTime.UtcNow;
        var stage = new PipelineStageDef("parse", "transform");

        // Act
        config.AddStage(stage);

        // Assert
        config.Stages.Should().ContainSingle().Which.Should().BeSameAs(stage);
        config.GetStageByName("parse").Should().BeSameAs(stage);
        config.GetStageByName("missing").Should().BeNull();
        config.LastModifiedAt.Should().BeOnOrAfter(before);

        Action nullStage = () => config.AddStage(null!);
        nullStage.Should().Throw<ArgumentNullException>();

        Action blankName = () => config.GetStageByName(" ");
        blankName.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetCustomSetting_RoundTripsValueAndRejectsBadArguments()
    {
        // Arrange
        var config = new PipelineConfig();

        // Act
        config.SetCustomSetting("batchSize", 250);

        // Assert
        config.GetCustomSetting("batchSize").Should().Be(250);
        config.GetCustomSetting("unknown").Should().BeNull();

        Action nullValue = () => config.SetCustomSetting("key", null!);
        nullValue.Should().Throw<ArgumentNullException>();

        Action emptyKey = () => config.SetCustomSetting("", "value");
        emptyKey.Should().Throw<ArgumentException>();

        Action nullKey = () => config.GetCustomSetting(null!);
        nullKey.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(1000, 1000, 1)]
    [InlineData(1000, 300, 4)]
    [InlineData(1000, 1, 1000)]
    [InlineData(1000, 0, 1)]
    public void CalculateWindowsPerFlushInterval_RoundsUpOrReturnsOneForNonPositiveSlide(long flushMs, long slideMs, int expected)
    {
        // Arrange
        var config = new PipelineConfig { BufferFlushIntervalMs = flushMs, WindowSlideMs = slideMs };

        // Act
        var result = config.CalculateWindowsPerFlushInterval();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Clone_CopiesSettingsAndDetachesCollections()
    {
        // Arrange
        var original = new PipelineConfig(1, "orders", "1.0.0")
        {
            MaxBufferSize = 500,
            WindowType = "SLIDING",
            MaxRetries = 7
        };
        original.AddStage(new PipelineStageDef("parse", "transform"));
        original.SetCustomSetting("region", "eu");

        // Act
        var clone = original.Clone(99);

        // Assert
        clone.ConfigId.Should().Be(99);
        clone.PipelineName.Should().Be("orders");
        clone.MaxBufferSize.Should().Be(500);
        clone.WindowType.Should().Be("SLIDING");
        clone.MaxRetries.Should().Be(7);
        clone.Stages.Should().HaveCount(1);
        clone.GetCustomSetting("region").Should().Be("eu");

        // Changing the clone must not leak back into the original
        clone.AddStage(new PipelineStageDef("filter", "filter"));
        clone.SetCustomSetting("region", "us");
        original.Stages.Should().HaveCount(1);
        original.GetCustomSetting("region").Should().Be("eu");
    }
}
