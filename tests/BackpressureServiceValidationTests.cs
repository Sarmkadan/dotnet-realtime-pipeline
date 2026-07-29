using System;
using DotNetRealtimePipeline.Services;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

public class BackpressureServiceValidationTests
{
    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var service = new BackpressureService();

        // Act
        var errors = service.Validate();

        // Assert
        Assert.NotNull(errors);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        BackpressureService? service = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service!.Validate());
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var service = new BackpressureService();

        // Act
        var result = service.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        BackpressureService? service = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service!.IsValid());
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var service = new BackpressureService();

        // Act & Assert
        var exception = Record.Exception(() => service.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        BackpressureService? service = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service!.EnsureValid());
    }
}
