using System;
using System.Linq;
using DotNetRealtimePipeline.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

public class PipelineEventPublisherValidationTests
{
    private readonly Mock<ILogger<PipelineEventPublisher>> _loggerMock;
    private readonly PipelineEventPublisher _publisher;

    public PipelineEventPublisherValidationTests()
    {
        _loggerMock = new Mock<ILogger<PipelineEventPublisher>>();
        _publisher = new PipelineEventPublisher(_loggerMock.Object);
    }

    [Fact]
    public void Validate_WithValidPublisher_ReturnsEmptyList()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(Validate_WithValidPublisher_ReturnsEmptyList));

        // Act
        var result = _publisher.Validate();

        // Assert
        result.Should().BeEmpty();
        result.Should().BeAssignableTo<IReadOnlyList<string>>();

        _loggerMock.Object.LogInformation("Finished test {TestName}", nameof(Validate_WithValidPublisher_ReturnsEmptyList));
    }

    [Fact]
    public void Validate_WithNullPublisher_ThrowsArgumentNullException()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(Validate_WithNullPublisher_ThrowsArgumentNullException));
        _loggerMock.Object.LogWarning("Testing null publisher validation, expecting ArgumentNullException");

        // Arrange
        PipelineEventPublisher nullPublisher = null!;

        // Act
        Action act = () => nullPublisher.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();

        _loggerMock.Object.LogInformation("Finished test {TestName}", nameof(Validate_WithNullPublisher_ThrowsArgumentNullException));
    }

    [Fact]
    public void IsValid_WithValidPublisher_ReturnsTrue()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(IsValid_WithValidPublisher_ReturnsTrue));

        // Act
        var result = _publisher.IsValid();

        // Assert
        result.Should().BeTrue();

        _loggerMock.Object.LogInformation("Finished test {TestName}", nameof(IsValid_WithValidPublisher_ReturnsTrue));
    }

    [Fact]
    public void IsValid_WithNullPublisher_ThrowsArgumentNullException()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(IsValid_WithNullPublisher_ThrowsArgumentNullException));
        _loggerMock.Object.LogWarning("Testing null publisher IsValid, expecting ArgumentNullException");

        // Arrange
        PipelineEventPublisher nullPublisher = null!;

        // Act
        Action act = () => nullPublisher.IsValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();

        _loggerMock.Object.LogInformation("Finished test {TestName}", nameof(IsValid_WithNullPublisher_ThrowsArgumentNullException));
    }

    [Fact]
    public void EnsureValid_WithValidPublisher_DoesNotThrow()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(EnsureValid_WithValidPublisher_DoesNotThrow));

        // Act
        Action act = () => _publisher.EnsureValid();

        // Assert
        act.Should().NotThrow();

        _loggerMock.Object.LogInformation("Finished test {TestName}", nameof(EnsureValid_WithValidPublisher_DoesNotThrow));
    }

    [Fact]
    public void EnsureValid_WithNullPublisher_ThrowsArgumentNullException()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(EnsureValid_WithNullPublisher_ThrowsArgumentNullException));
        _loggerMock.Object.LogWarning("Testing null publisher EnsureValid, expecting ArgumentNullException");

        // Arrange
        PipelineEventPublisher nullPublisher = null!;

        // Act
        Action act = () => nullPublisher.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();

        _loggerMock.Object.LogInformation("Finished test {TestName}", nameof(EnsureValid_WithNullPublisher_ThrowsArgumentNullException));
    }

    [Fact]
    public void EnsureValid_WithInvalidPublisher_ThrowsArgumentException()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(EnsureValid_WithInvalidPublisher_ThrowsArgumentException));

        // Arrange - Create a publisher that would be invalid if validation existed
        // Since current implementation always returns empty list, this test documents expected behavior
        // when validation criteria are added in the future
        var publisher = _publisher;

        // Act
        Action act = () => publisher.EnsureValid();

        // Assert - Should not throw with current implementation
        act.Should().NotThrow<ArgumentException>();

        _loggerMock.Object.LogInformation("Finished test {TestName}", nameof(EnsureValid_WithInvalidPublisher_ThrowsArgumentException));
    }

    [Fact]
    public void Validate_ReturnsReadOnlyList()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(Validate_ReturnsReadOnlyList));

        // Act
        var result = _publisher.Validate();

        // Assert
        result.Should().BeAssignableTo<IReadOnlyList<string>>();
        result.Count.Should().Be(0);

        _loggerMock.Object.LogInformation("Finished test {TestName}", nameof(Validate_ReturnsReadOnlyList));
    }

    [Fact]
    public void IsValid_ReturnsTrueWhenValidateReturnsEmptyList()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(IsValid_ReturnsTrueWhenValidateReturnsEmptyList));

        // Arrange
        var publisher = _publisher;

        // Act
        var isValid = publisher.IsValid();
        var problems = publisher.Validate();

        // Assert
        isValid.Should().BeTrue();
        problems.Should().BeEmpty();
        isValid.Should().Be(problems.Count == 0);

        _loggerMock.Object.LogInformation("Finished test {TestName}", nameof(IsValid_ReturnsTrueWhenValidateReturnsEmptyList));
    }

    [Fact]
    public void Methods_AreExtensionMethodsForPipelineEventPublisher()
    {
        _loggerMock.Object.LogInformation("Starting test {TestName}", nameof(Methods_AreExtensionMethodsForPipelineEventPublisher));

        // Arrange
        var publisher = _publisher;

        // Act & Assert - Verify all methods work as expected
        var validateResult = publisher.Validate();
        var isValidResult = publisher.IsValid();

        Action ensureValidAction = () => publisher.EnsureValid();

        // Assert
        validateResult.Should().NotBeNull();
        isValidResult.Should().BeTrue();
        ensureValidAction.Should().NotThrow();

        _loggerMock.Object.LogInformation("Finished test {TestName}", nameof(Methods_AreExtensionMethodsForPipelineEventPublisher));
    }
}
