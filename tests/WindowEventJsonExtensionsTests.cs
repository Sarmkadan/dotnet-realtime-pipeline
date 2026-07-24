using Xunit;

namespace DotNetRealtimePipeline.Tests.Domain.Models;

public class WindowEventJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var windowEvent = new WindowEvent { Id = 1, Name = "Test Window" };

        // Act
        var json = WindowEventJsonExtensions.ToJson(windowEvent);

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
    }

    [Fact]
    public void ToJson_NullWindowEvent_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => WindowEventJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsWindowEvent()
    {
        // Arrange
        var json = "{\"Id\":1,\"Name\":\"Test Window\"}";

        // Act
        var windowEvent = WindowEventJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(windowEvent);
        Assert.Equal(1, windowEvent.Id);
        Assert.Equal("Test Window", windowEvent.Name);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => WindowEventJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyJson_ThrowsArgumentException()
    {
        // Act and Assert
        Assert.Throws<ArgumentException>(() => WindowEventJsonExtensions.FromJson(""));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrue()
    {
        // Arrange
        var json = "{\"Id\":1,\"Name\":\"Test Window\"}";

        // Act
        var success = WindowEventJsonExtensions.TryFromJson(json, out var windowEvent);

        // Assert
        Assert.True(success);
        Assert.NotNull(windowEvent);
        Assert.Equal(1, windowEvent.Id);
        Assert.Equal("Test Window", windowEvent.Name);
    }

    [Fact]
    public void TryFromJson_NullJson_ReturnsFalse()
    {
        // Act
        var success = WindowEventJsonExtensions.TryFromJson(null, out var windowEvent);

        // Assert
        Assert.False(success);
        Assert.Null(windowEvent);
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsFalse()
    {
        // Act
        var success = WindowEventJsonExtensions.TryFromJson("", out var windowEvent);

        // Assert
        Assert.False(success);
        Assert.Null(windowEvent);
    }
}
