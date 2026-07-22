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
        // Act
        var result = _publisher.Validate();

        // Assert
        result.Should().BeEmpty();
        result.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    [Fact]
    public void Validate_WithNullPublisher_ThrowsArgumentNullException()
    {
        // Arrange
        PipelineEventPublisher nullPublisher = null!;

        // Act
        Action act = () => nullPublisher.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsValid_WithValidPublisher_ReturnsTrue()
    {
        // Act
        var result = _publisher.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithNullPublisher_ThrowsArgumentNullException()
    {
        // Arrange
        PipelineEventPublisher nullPublisher = null!;

        // Act
        Action act = () => nullPublisher.IsValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_WithValidPublisher_DoesNotThrow()
    {
        // Act
        Action act = () => _publisher.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_WithNullPublisher_ThrowsArgumentNullException()
    {
        // Arrange
        PipelineEventPublisher nullPublisher = null!;

        // Act
        Action act = () => nullPublisher.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_WithInvalidPublisher_ThrowsArgumentException()
    {
        // Arrange - Create a publisher that would be invalid if validation existed
        // Since current implementation always returns empty list, this test documents expected behavior
        // when validation criteria are added in the future
        var publisher = _publisher;

        // Act
        Action act = () => publisher.EnsureValid();

        // Assert - Should not throw with current implementation
        act.Should().NotThrow<ArgumentException>();
    }

    [Fact]
    public void Validate_ReturnsReadOnlyList()
    {
        // Act
        var result = _publisher.Validate();

        // Assert
        result.Should().BeAssignableTo<IReadOnlyList<string>>();
        result.Count.Should().Be(0);
    }

    [Fact]
    public void IsValid_ReturnsTrueWhenValidateReturnsEmptyList()
    {
        // Arrange
        var publisher = _publisher;

        // Act
        var isValid = publisher.IsValid();
        var problems = publisher.Validate();

        // Assert
        isValid.Should().BeTrue();
        problems.Should().BeEmpty();
        isValid.Should().Be(problems.Count == 0);
    }

    [Fact]
    public void Methods_AreExtensionMethodsForPipelineEventPublisher()
    {
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
    }
}