// tests/PipelineExceptionTests.cs

using Xunit;

namespace DotNetRealtimePipeline.Tests.Domain.Exceptions;

public class PipelineExceptionTests
{
    [Fact]
    public void ErrorCode_HappyPath_ReturnsErrorCode()
    {
        // Arrange
        var exception = new PipelineException("Test message", "Test error code");

        // Act
        var errorCode = exception.ErrorCode;

        // Assert
        Assert.Equal("Test error code", errorCode);
    }

    [Fact]
    public void ErrorDetails_HappyPath_ReturnsErrorDetails()
    {
        // Arrange
        var exception = new PipelineException("Test message", "Test error code", new object());

        // Act
        var errorDetails = exception.ErrorDetails;

        // Assert
        Assert.NotNull(errorDetails);
    }

    [Fact]
    public void Constructor_HappyPath_ReturnsPipelineException()
    {
        // Arrange
        var message = "Test message";

        // Act
        var exception = new PipelineException(message);

        // Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public void Constructor_WithErrorCode_HappyPath_ReturnsPipelineException()
    {
        // Arrange
        var message = "Test message";
        var errorCode = "Test error code";

        // Act
        var exception = new PipelineException(message, errorCode);

        // Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public void Constructor_WithErrorCodeAndErrorDetails_HappyPath_ReturnsPipelineException()
    {
        // Arrange
        var message = "Test message";
        var errorCode = "Test error code";
        var errorDetails = new object();

        // Act
        var exception = new PipelineException(message, errorCode, errorDetails);

        // Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public void InvalidDataPointException_HappyPath_ReturnsInvalidDataPointException()
    {
        // Arrange
        var message = "Test message";

        // Act
        var exception = new InvalidDataPointException(message);

        // Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public void InvalidDataPointException_WithDetails_HappyPath_ReturnsInvalidDataPointException()
    {
        // Arrange
        var message = "Test message";
        var details = new object();

        // Act
        var exception = new InvalidDataPointException(message, details);

        // Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public void BackpressureException_HappyPath_ReturnsBackpressureException()
    {
        // Arrange
        var message = "Test message";
        var bufferSize = 10;
        var maxCapacity = 100;

        // Act
        var exception = new BackpressureException(message, bufferSize, maxCapacity);

        // Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public void BackpressureException_WithDetails_HappyPath_ReturnsBackpressureException()
    {
        // Arrange
        var message = "Test message";
        var bufferSize = 10;
        var maxCapacity = 100;
        var details = new object();

        // Act
        var exception = new BackpressureException(message, bufferSize, maxCapacity, details);

        // Assert
        Assert.NotNull(exception);
    }
}
