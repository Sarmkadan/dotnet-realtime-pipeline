using System;
using DotNetRealtimePipeline.Services;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

public class WindowingServiceJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidInstance_ReturnsJson()
    {
        // Arrange
        var service = new WindowingService();

        // Act
        var json = service.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));

        // Ensure it can be deserialized back to a non‑null instance
        var deserialized = WindowingServiceJsonExtensions.FromJson(json);
        Assert.NotNull(deserialized);
    }

    [Fact]
    public void ToJson_NullInstance_ThrowsArgumentNullException()
    {
        // Arrange
        WindowingService? service = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service!.ToJson());
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => WindowingServiceJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_EmptyOrWhiteSpace_ReturnsNull()
    {
        // Act
        var result = WindowingServiceJsonExtensions.FromJson("   ");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndInstance()
    {
        // Arrange
        var service = new WindowingService();
        var json = service.ToJson();

        // Act
        var success = WindowingServiceJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_EmptyString_ReturnsTrueAndNull()
    {
        // Act
        var success = WindowingServiceJsonExtensions.TryFromJson(string.Empty, out var result);

        // Assert
        Assert.True(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act
        var success = WindowingServiceJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => WindowingServiceJsonExtensions.TryFromJson(null!, out var _));
    }
}
