#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

public sealed class DataPointTests
{
    private static DataPoint ValidPoint(long id = 1) =>
        new DataPoint(id, 1_000_000L, 42.5, "sensor-01") { Quality = 85 };

    [Fact]
    public void Validate_WithAllValidProperties_ReturnsTrue()
    {
        // Arrange
        var point = ValidPoint();

        // Act
        bool result = point.Validate();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithZeroId_ReturnsFalse()
    {
        // Arrange
        var point = ValidPoint(id: 0);

        // Act
        bool result = point.Validate();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithEmptySource_ReturnsFalse()
    {
        // Arrange
        var point = new DataPoint(1, 1_000_000L, 42.5, "sensor-01");
        point.Source = "";

        // Act
        bool result = point.Validate();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithQualityAboveUpperBound_ReturnsFalse()
    {
        // Arrange
        var point = ValidPoint();
        point.Quality = 101;

        // Act
        bool result = point.Validate();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MeetsQualityThreshold_WhenQualityEqualsThreshold_ReturnsTrue()
    {
        // Arrange
        var point = new DataPoint(1, 1_000_000L, 10.0, "sensor-01") { Quality = 70 };

        // Act & Assert
        point.MeetsQualityThreshold(70).Should().BeTrue();
    }

    [Fact]
    public void MeetsQualityThreshold_WhenQualityBelowThreshold_ReturnsFalse()
    {
        // Arrange
        var point = new DataPoint(1, 1_000_000L, 10.0, "sensor-01") { Quality = 50 };

        // Act & Assert
        point.MeetsQualityThreshold(70).Should().BeFalse();
    }

    [Fact]
    public void Clone_WithNewId_PreservesValueSourceAndQuality()
    {
        // Arrange
        var original = new DataPoint(1, 1_000_000L, 99.9, "sensor-A")
        {
            Quality = 88,
            Tags = "env:prod"
        };

        // Act
        var clone = original.Clone(newId: 999);

        // Assert
        clone.Id.Should().Be(999);
        clone.Value.Should().Be(original.Value);
        clone.Source.Should().Be(original.Source);
        clone.Quality.Should().Be(original.Quality);
        clone.Tags.Should().Be(original.Tags);
        clone.Timestamp.Should().Be(original.Timestamp);
    }

    [Fact]
    public void AddMetadata_WithValidKeyAndValue_StoresEntry()
    {
        // Arrange
        var point = ValidPoint();

        // Act
        point.AddMetadata("region", "us-east-1");

        // Assert
        point.Metadata.Should().ContainKey("region");
        point.Metadata["region"].Should().Be("us-east-1");
    }

    [Fact]
    public void AddMetadata_OverwritesExistingKeyWithNewValue()
    {
        // Arrange
        var point = ValidPoint();
        point.AddMetadata("env", "staging");

        // Act
        point.AddMetadata("env", "production");

        // Assert
        point.Metadata["env"].Should().Be("production");
    }
}
