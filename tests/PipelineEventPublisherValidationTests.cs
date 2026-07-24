using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

public class PipelineEventPublisherValidationTests
{
    private readonly Mock<ILogger<PipelineEventPublisher>> _mockLogger;
    private readonly PipelineEventPublisher _publisher;

    public PipelineEventPublisherValidationTests()
    {
        _mockLogger = new Mock<ILogger<PipelineEventPublisher>>();
        _publisher = new PipelineEventPublisher(_mockLogger.Object);
    }

    [Fact]
    public void Validate_WithValidPublisher_ReturnsEmptyList()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(Mock.Of<ILogger<PipelineEventPublisher>>());

        // Act
        var result = publisher.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithNullPublisher_ThrowsArgumentNullException()
    {
        // Arrange
        PipelineEventPublisher? publisher = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => publisher!.Validate());
    }

    [Fact]
    public void IsValid_WithValidPublisher_ReturnsTrue()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(Mock.Of<ILogger<PipelineEventPublisher>>());

        // Act
        var isValid = publisher.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WithNullPublisher_ThrowsArgumentNullException()
    {
        // Arrange
        PipelineEventPublisher? publisher = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => publisher!.IsValid());
    }

    [Fact]
    public void EnsureValid_WithValidPublisher_DoesNotThrow()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(Mock.Of<ILogger<PipelineEventPublisher>>());

        // Act
        var exception = Record.Exception(() => publisher.EnsureValid());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_WithNullPublisher_ThrowsArgumentNullException()
    {
        // Arrange
        PipelineEventPublisher? publisher = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => publisher!.EnsureValid());
    }
}