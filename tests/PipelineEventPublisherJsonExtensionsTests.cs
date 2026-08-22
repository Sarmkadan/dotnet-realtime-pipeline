#nullable enable

using System;
using System.Diagnostics;
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
    private readonly Mock<ILogger<PipelineEventPublisherJsonExtensionsTests>> _loggerMock;
    private readonly ILogger<PipelineEventPublisherJsonExtensionsTests> _logger;

    public PipelineEventPublisherJsonExtensionsTests()
    {
        var loggerMock = new Mock<ILogger<PipelineEventPublisher>>();
        _publisher = new PipelineEventPublisher(loggerMock.Object);

        _loggerMock = new Mock<ILogger<PipelineEventPublisherJsonExtensionsTests>>();
        _logger = _loggerMock.Object;
    }

    [Fact]
    public void ToJson_WithValidPublisher_ReturnsJsonString()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Act
            var json = _publisher.ToJson();

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("PipelineEventPublisher");
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Act
            var json = _publisher.ToJson(indented: true);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("\n"); // Should have newlines for formatting
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Act
            var json = _publisher.ToJson(indented: false);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().NotContain("\n"); // Should not have newlines
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void ToJson_WithNullPublisher_ThrowsArgumentNullException()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Arrange
            PipelineEventPublisher? nullPublisher = null;

            // Act
            Action act = () => nullPublisher!.ToJson();

            // Assert
            act.Should().Throw<ArgumentNullException>();
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsPublisherInstance()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Arrange
            var json = _publisher.ToJson();

            // Act
            var result = PipelineEventPublisherJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<PipelineEventPublisher>();
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Arrange
            string? nullJson = null;

            // Act
            Action act = () => PipelineEventPublisherJsonExtensions.FromJson(nullJson!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void FromJson_WithEmptyString_ReturnsNull()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Arrange
            var emptyJson = string.Empty;

            // Act
            var result = PipelineEventPublisherJsonExtensions.FromJson(emptyJson);

            // Assert
            result.Should().BeNull();
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void FromJson_WithWhitespaceString_ReturnsNull()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Arrange
            var whitespaceJson = "   \n\t  ";

            // Act
            var result = PipelineEventPublisherJsonExtensions.FromJson(whitespaceJson);

            // Assert
            result.Should().BeNull();
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Arrange
            var invalidJson = "{ invalid json";

            // Act
            Action act = () => PipelineEventPublisherJsonExtensions.FromJson(invalidJson);

            // Assert
            act.Should().Throw<System.Text.Json.JsonException>();
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndPublisher()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
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
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Arrange
            string? nullJson = null;
            PipelineEventPublisher? value = null;

            // Act
            Action act = () => PipelineEventPublisherJsonExtensions.TryFromJson(nullJson!, out value);

            // Assert
            act.Should().Throw<ArgumentNullException>();
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void TryFromJson_WithEmptyString_ReturnsFalseAndNull()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Arrange
            var emptyJson = string.Empty;
            PipelineEventPublisher? value = null;

            // Act
            var result = PipelineEventPublisherJsonExtensions.TryFromJson(emptyJson, out value);

            // Assert
            result.Should().BeFalse();
            value.Should().BeNull();
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void TryFromJson_WithWhitespaceString_ReturnsFalseAndNull()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Arrange
            var whitespaceJson = "   \n\t  ";
            PipelineEventPublisher? value = null;

            // Act
            var result = PipelineEventPublisherJsonExtensions.TryFromJson(whitespaceJson, out value);

            // Assert
            result.Should().BeFalse();
            value.Should().BeNull();
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Arrange
            var invalidJson = "{ invalid json";
            PipelineEventPublisher? value = null;

            // Act
            var result = PipelineEventPublisherJsonExtensions.TryFromJson(invalidJson, out value);

            // Assert
            result.Should().BeFalse();
            value.Should().BeNull();
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void Roundtrip_SerializationDeserialization_PreservesPublisher()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
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
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void Roundtrip_SerializationTryFromJson_PreservesPublisher()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
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
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void JsonFormat_UsesCamelCaseNamingPolicy()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PipelineEventPublisher>>();
            var publisher = new PipelineEventPublisher(loggerMock.Object);

            // Act
            var json = publisher.ToJson();

            // Assert
            json.Should().Contain("logger"); // Should use camelCase for property names
            json.Should().NotContain("Logger"); // Should not use PascalCase
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }

    [Fact]
    public void JsonFormat_IgnoresNullValues()
    {
        var method = new StackTrace().GetFrame(0).GetMethod();
        string methodName = method.Name;
        _logger.LogInformation("Starting test {MethodName}", methodName);
        try
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PipelineEventPublisher>>();
            var publisher = new PipelineEventPublisher(loggerMock.Object);

            // Act
            var json = publisher.ToJson();

            // Assert
            // The publisher has a logger field that should not appear in JSON when null
            json.Should().NotContain("null");
            _logger.LogInformation("Finished test {MethodName}", methodName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test {MethodName} failed", methodName);
            throw;
        }
    }
}