namespace DotNetRealtimePipeline.Tests;

using System;
using Xunit;
using DotNetRealtimePipeline.Middleware;

public sealed class LoggingMiddlewareJsonExtensionsTests
{
    [Fact]
    public void ToJson_ThrowsArgumentNullException_OnNullInput()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => LoggingMiddlewareJsonExtensions.ToJson(null!));
    }

    [Fact]
    public void FromJson_ThrowsArgumentNullException_OnNullInput()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => LoggingMiddlewareJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_ThrowsArgumentException_OnEmptyInput()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingMiddlewareJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_ThrowsNotSupportedException_OnValidInput()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => LoggingMiddlewareJsonExtensions.FromJson("{}"));
    }

    [Fact]
    public void TryFromJson_ThrowsArgumentNullException_OnNullInput()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => LoggingMiddlewareJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_OnValidInput()
    {
        // Act
        var result = LoggingMiddlewareJsonExtensions.TryFromJson("{}", out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }
}
