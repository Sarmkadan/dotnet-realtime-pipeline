#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Unit tests for the <see cref="DataPoint"/> class.
/// </summary>
public sealed class DataPointTests
{
    /// <summary>
    /// Creates a valid test data point with default values.
    /// </summary>
    /// <param name="id">The identifier for the data point. Defaults to 1.</param>
    /// <returns>A new <see cref="DataPoint"/> instance with valid properties.</returns>
    private static DataPoint ValidPoint(long id = 1) =>
    new DataPoint(id, 1_000_000L, 42.5, "sensor-01") { Quality = 85 };

    /// <summary>
    /// Tests that validation returns true when all data point properties are valid.
    /// </summary>
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

    /// <summary>
    /// Tests that validation returns false when the ID is zero.
    /// </summary>
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

    /// <summary>
    /// Tests that validation returns false when the source is empty.
    /// </summary>
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

    /// <summary>
    /// Tests that validation returns false when quality is above the upper bound (100).
    /// </summary>
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

    /// <summary>
    /// Tests that the quality threshold check returns true when quality equals the threshold.
    /// </summary>
    [Fact]
    public void MeetsQualityThreshold_WhenQualityEqualsThreshold_ReturnsTrue()
    {
        // Arrange
        var point = new DataPoint(1, 1_000_000L, 10.0, "sensor-01") { Quality = 70 };

        // Act & Assert
        point.MeetsQualityThreshold(70).Should().BeTrue();
    }

    /// <summary>
    /// Tests that the quality threshold check returns false when quality is below the threshold.
    /// </summary>
    [Fact]
    public void MeetsQualityThreshold_WhenQualityBelowThreshold_ReturnsFalse()
    {
        // Arrange
        var point = new DataPoint(1, 1_000_000L, 10.0, "sensor-01") { Quality = 50 };

        // Act & Assert
        point.MeetsQualityThreshold(70).Should().BeFalse();
    }

    /// <summary>
    /// Tests that cloning a data point with a new ID preserves all other properties.
    /// </summary>
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

    /// <summary>
    /// Tests that adding metadata with a valid key and value stores the entry correctly.
    /// </summary>
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

    /// <summary>
    /// Tests that adding metadata with an existing key overwrites the previous value.
    /// </summary>
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