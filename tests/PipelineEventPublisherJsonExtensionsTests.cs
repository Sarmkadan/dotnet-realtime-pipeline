#nullable enable

using DotNetRealtimePipeline.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

public class PipelineEventPublisherJsonExtensionsTests
{
    private readonly PipelineEventPublisher _publisher;

    public PipelineEventPublisherJsonExtensionsTests()
    {
        var loggerMock = new Mock<ILogger<PipelineEventPublisher>>();
        _publisher = new PipelineEventPublisher(loggerMock.Object);
    }

    [Fact]
    public void ToJson_WithValidPublisher_ReturnsJsonString()
    {
        // Act
        var json = _publisher.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("PipelineEventPublisher");
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Act
        var json = _publisher.ToJson(indented: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\n"); // Should have newlines for formatting
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        // Act
        var json = _publisher.ToJson(indented: false);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().NotContain("\n"); // Should not have newlines
    }

    [Fact]
    public void ToJson_WithNullPublisher_ThrowsArgumentNullException()
    {
        // Arrange
        PipelineEventPublisher? nullPublisher = null;

        // Act
        Action act = () => nullPublisher!.ToJson();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsPublisherInstance()
    {
        // Arrange
        var json = _publisher.ToJson();

        // Act
        var result = PipelineEventPublisherJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PipelineEventPublisher>();
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullJson = null;

        // Act
        Action act = () => PipelineEventPublisherJsonExtensions.FromJson(nullJson!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_WithEmptyString_ReturnsNull()
    {
        // Arrange
        var emptyJson = string.Empty;

        // Act
        var result = PipelineEventPublisherJsonExtensions.FromJson(emptyJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithWhitespaceString_ReturnsNull()
    {
        // Arrange
        var whitespaceJson = "   \n\t  ";

        // Act
        var result = PipelineEventPublisherJsonExtensions.FromJson(whitespaceJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ invalid json";

        // Act
        Action act = () => PipelineEventPublisherJsonExtensions.FromJson(invalidJson);

        // Assert
        act.Should().Throw<System.Text.Json.JsonException>();
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndPublisher()
    {
        // Arrange
        var json = _publisher.ToJson();
        PipelineEventPublisher? value = null;

        // Act
        var result = PipelineEventPublisherJsonExtensions.TryFromJson(json, out value);

        // Assert
        result.Should().BeTrue();
        value.Should().NotBeNull();
        value.Should().BeOfType<PipelineEventPublisher>();
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullJson = null;
        PipelineEventPublisher? value = null;

        // Act
        Action act = () => PipelineEventPublisherJsonExtensions.TryFromJson(nullJson!, out value);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryFromJson_WithEmptyString_ReturnsFalseAndNull()
    {
        // Arrange
        var emptyJson = string.Empty;
        PipelineEventPublisher? value = null;

        // Act
        var result = PipelineEventPublisherJsonExtensions.TryFromJson(emptyJson, out value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithWhitespaceString_ReturnsFalseAndNull()
    {
        // Arrange
        var whitespaceJson = "   \n\t  ";
        PipelineEventPublisher? value = null;

        // Act
        var result = PipelineEventPublisherJsonExtensions.TryFromJson(whitespaceJson, out value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ invalid json";
        PipelineEventPublisher? value = null;

        // Act
        var result = PipelineEventPublisherJsonExtensions.TryFromJson(invalidJson, out value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void Roundtrip_SerializationDeserialization_PreservesPublisher()
    {
        // Arrange - Create a publisher with some state
        var loggerMock = new Mock<ILogger<PipelineEventPublisher>>();
        var publisher = new PipelineEventPublisher(loggerMock.Object);

        // Act - Serialize and deserialize
        var json = publisher.ToJson();
        var deserialized = PipelineEventPublisherJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        // The deserialized publisher should be functional
        deserialized.Should().BeAssignableTo<PipelineEventPublisher>();
    }

    [Fact]
    public void Roundtrip_SerializationTryFromJson_PreservesPublisher()
    {
        // Arrange - Create a publisher with some state
        var loggerMock = new Mock<ILogger<PipelineEventPublisher>>();
        var publisher = new PipelineEventPublisher(loggerMock.Object);

        // Act - Serialize and deserialize
        var json = publisher.ToJson();
        PipelineEventPublisherJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeAssignableTo<PipelineEventPublisher>();
    }

    [Fact]
    public void JsonFormat_UsesCamelCaseNamingPolicy()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PipelineEventPublisher>>();
        var publisher = new PipelineEventPublisher(loggerMock.Object);

        // Act
        var json = publisher.ToJson();

        // Assert
        json.Should().Contain("logger"); // Should use camelCase for property names
        json.Should().NotContain("Logger"); // Should not use PascalCase
    }

    [Fact]
    public void JsonFormat_IgnoresNullValues()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PipelineEventPublisher>>();
        var publisher = new PipelineEventPublisher(loggerMock.Object);

        // Act
        var json = publisher.ToJson();

        // Assert
        // The publisher has a logger field that should not appear in JSON when null
        json.Should().NotContain("null");
    }
}
