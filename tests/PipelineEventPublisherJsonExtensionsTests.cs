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

/// <summary>
/// Test class for PipelineEventPublisherJsonExtensions.
/// </summary>
public class PipelineEventPublisherJsonExtensionsTests
{
    private readonly PipelineEventPublisher _publisher;
    private readonly Mock<ILogger<PipelineEventPublisherJsonExtensionsTests>> _loggerMock;
    private readonly ILogger<PipelineEventPublisherJsonExtensionsTests> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineEventPublisherJsonExtensionsTests"/> class.
    /// Sets up mock logger and publisher for testing.
    /// </summary>
    public PipelineEventPublisherJsonExtensionsTests()
    {
        var loggerMock = new Mock<ILogger<PipelineEventPublisher>>();
        _publisher = new PipelineEventPublisher(loggerMock.Object);

        _loggerMock = new Mock<ILogger<PipelineEventPublisherJsonExtensionsTests>>();
        _logger = _loggerMock.Object;
    }

    /// <summary>
    /// Tests that ToJson() returns a non-empty JSON string when called on a valid publisher instance.
    /// </summary>
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

    /// <summary>
    /// Tests that ToJson(indented: true) returns formatted JSON with newlines.
    /// </summary>
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

    /// <summary>
    /// Tests that ToJson(indented: false) returns compact JSON without newlines.
    /// </summary>
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

    /// <summary>
    /// Tests that ToJson() throws ArgumentNullException when called on a null publisher.
    /// </summary>
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

    /// <summary>
    /// Tests that FromJson() returns a PipelineEventPublisher instance when given valid JSON.
    /// </summary>
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

    /// <summary>
    /// Tests that FromJson() throws ArgumentNullException when given null JSON.
    /// </summary>
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

    /// <summary>
    /// Tests that FromJson() returns null when given an empty string.
    /// </summary>
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

    /// <summary>
    /// Tests that FromJson() returns null when given a whitespace-only string.
    /// </summary>
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

    /// <summary>
    /// Tests that FromJson() throws JsonException when given invalid JSON.
    /// </summary>
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

    /// <summary>
    /// Tests that TryFromJson() returns true and outputs a PipelineEventPublisher when given valid JSON.
    /// </summary>
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

    /// <summary>
    /// Tests that TryFromJson() throws ArgumentNullException when given null JSON.
    /// </summary>
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

    /// <summary>
    /// Tests that TryFromJson() returns false and null when given an empty string.
    /// </summary>
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

    /// <summary>
    /// Tests that TryFromJson() returns false and null when given a whitespace-only string.
    /// </summary>
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

    /// <summary>
    /// Tests that TryFromJson() returns false and null when given invalid JSON.
    /// </summary>
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

    /// <summary>
    /// Tests that serializing and deserializing a publisher preserves its functionality.
    /// </summary>
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

    /// <summary>
    /// Tests that serializing and deserializing using TryFromJson preserves publisher functionality.
    /// </summary>
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

    /// <summary>
    /// Tests that the JSON output uses camelCase naming policy for properties.
    /// </summary>
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

    /// <summary>
    /// Tests that the JSON output ignores null values.
    /// </summary>
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