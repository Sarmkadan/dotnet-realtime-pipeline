using System;
using Xunit;
using DotNetRealtimePipeline.Domain.Exceptions;

namespace DotNetRealtimePipeline.Tests;

public class PipelineExceptionJsonExtensionsTests
{
    [Fact]
    public void ToJson_ValidInput_ReturnsJsonString()
    {
        // Arrange
        var exception = new PipelineException("Test error message");

        // Act
        var json = exception.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.Contains("test error message", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((PipelineException)null!).ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsPipelineException()
    {
        // Arrange
        var originalException = new PipelineException("Deserialization test");
        var json = originalException.ToJson();

        // Act
        var result = PipelineExceptionJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Deserialization test", result.Message);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineExceptionJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_WhitespaceInput_ReturnsNull()
    {
        // Act
        var result = PipelineExceptionJsonExtensions.FromJson("   ");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndObject()
    {
        // Arrange
        var originalException = new PipelineException("Try parse test");
        var json = originalException.ToJson();

        // Act
        var success = PipelineExceptionJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal("Try parse test", result.Message);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Act
        var success = PipelineExceptionJsonExtensions.TryFromJson("{invalid json}", out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineExceptionJsonExtensions.TryFromJson(null!, out _));
    }
}
