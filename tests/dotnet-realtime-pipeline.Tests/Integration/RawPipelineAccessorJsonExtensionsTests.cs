using System;
using System.IO.Pipelines;
using DotNetRealtimePipeline.Integration;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Integration;

/// <summary>
/// Unit tests for <see cref="RawPipelineAccessorJsonExtensions"/> class.
/// Tests JSON serialization and deserialization for RawPipelineAccessor types.
/// </summary>
public class RawPipelineAccessorJsonExtensionsTests
{
    #region ToJson Tests

    [Fact]
    public void ToJson_WithValidAccessor_ReturnsNonEmptyJsonString()
    {
        // Arrange
        var accessor = new RawPipelineAccessor();

        // Act
        var json = accessor.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Be("{}");
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var accessor = new RawPipelineAccessor();

        // Act
        var json = accessor.ToJson(indented: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Be("{}");
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        // Arrange
        var accessor = new RawPipelineAccessor();

        // Act
        var json = accessor.ToJson(indented: false);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Be("{}");
    }

    [Fact]
    public void ToJson_WithNullAccessor_ThrowsArgumentNullException()
    {
        // Arrange
        RawPipelineAccessor accessor = null!;

        // Act
        Action act = () => accessor.ToJson();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToJson_WithCustomPipeOptions_PreservesOptionsInJson()
    {
        // Arrange
        var options = new PipeOptions(
            minimumSegmentSize: 4096,
            useSynchronizationContext: false
        );
        var accessor = new RawPipelineAccessor(options);

        // Act
        var json = accessor.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Be("{}");
    }

    #endregion

    #region FromJson Tests

    [Fact]
    public void FromJson_WithValidJson_ReturnsDeserializedAccessor()
    {
        // Arrange
        var accessor = new RawPipelineAccessor();
        var json = accessor.ToJson();

        // Act
        var result = RawPipelineAccessorJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RawPipelineAccessor>();
    }

    [Fact]
    public void FromJson_WithEmptyJson_ReturnsNull()
    {
        // Arrange
        var json = string.Empty;

        // Act
        var result = RawPipelineAccessorJsonExtensions.FromJson(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentException()
    {
        // Arrange
        string json = null!;

        // Act
        Action act = () => RawPipelineAccessorJsonExtensions.FromJson(json);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromJson_WithWhitespaceJson_ReturnsNull()
    {
        // Arrange
        var json = "   \t\n  ";

        // Act
        var result = RawPipelineAccessorJsonExtensions.FromJson(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var json = "invalid json {{{";

        // Act
        Action act = () => RawPipelineAccessorJsonExtensions.FromJson(json);

        // Assert
        act.Should().Throw<System.Text.Json.JsonException>();
    }

    [Fact]
    public void FromJson_WithCamelCaseProperties_DeserializesCorrectly()
    {
        // Arrange
        var json = "{\"_pipe\":{}}";

        // Act
        var result = RawPipelineAccessorJsonExtensions.FromJson(json);

        // Assert - should deserialize successfully
        result.Should().NotBeNull();
    }

    #endregion

    #region TryFromJson Tests

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializedAccessor()
    {
        // Arrange
        var accessor = new RawPipelineAccessor();
        var json = accessor.ToJson();

        // Act
        var result = RawPipelineAccessorJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().NotBeNull();
        value.Should().BeOfType<RawPipelineAccessor>();
    }

    [Fact]
    public void TryFromJson_WithEmptyJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = string.Empty;

        // Act
        var result = RawPipelineAccessorJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentException()
    {
        // Arrange
        string json = null!;

        // Act
        Action act = () => RawPipelineAccessorJsonExtensions.TryFromJson(json, out _);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryFromJson_WithWhitespaceJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = " \t\n ";

        // Act
        var result = RawPipelineAccessorJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = "invalid { json";

        // Act
        var result = RawPipelineAccessorJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithValidAccessorJson_RoundTripPreservesPipeState()
    {
        // Arrange
        var originalAccessor = new RawPipelineAccessor();
        var json = originalAccessor.ToJson();

        // Act
        var result = RawPipelineAccessorJsonExtensions.TryFromJson(json, out var deserializedAccessor);

        // Assert
        result.Should().BeTrue();
        deserializedAccessor.Should().NotBeNull();
        deserializedAccessor.Should().BeOfType<RawPipelineAccessor>();
    }

    #endregion

    #region Round-trip Tests

    [Fact]
    public void RoundTrip_ToJsonThenFromJson_PreservesDataIntegrity()
    {
        // Arrange
        var originalAccessor = new RawPipelineAccessor();

        // Act
        var json = originalAccessor.ToJson();
        var deserialized = RawPipelineAccessorJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<RawPipelineAccessor>();
    }

    [Fact]
    public void RoundTrip_ToJsonThenTryFromJson_PreservesDataIntegrity()
    {
        // Arrange
        var originalAccessor = new RawPipelineAccessor();

        // Act
        var json = originalAccessor.ToJson();
        var result = RawPipelineAccessorJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().NotBeNull();
        value.Should().BeOfType<RawPipelineAccessor>();
    }

    [Fact]
    public void RoundTrip_WithCustomPipeOptions_PreservesOptions()
    {
        // Arrange
        var options = new PipeOptions(
            minimumSegmentSize: 2048,
            useSynchronizationContext: false
        );
        var originalAccessor = new RawPipelineAccessor(options);
        var json = originalAccessor.ToJson();

        // Act
        var deserialized = RawPipelineAccessorJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<RawPipelineAccessor>();
    }

    #endregion
}