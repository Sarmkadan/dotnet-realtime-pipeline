namespace DotNetRealtimePipeline.Tests.Integration;

using Xunit;
using System;
using System.Text.Json;
using DotNetRealtimePipeline.Integration;

public class WebhookHandlerJsonExtensionsTests
{
    [Fact]
    public void ToJson_ReturnsJsonString_ForValidHandler()
    {
        // Arrange
        var handler = new WebhookHandler();

        // Act
        var json = handler.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
    }

    [Fact]
    public void ToJson_ThrowsArgumentNullException_ForNullHandler()
    {
        // Arrange
        WebhookHandler handler = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => handler.ToJson());
    }

    [Fact]
    public void ToJson_ReturnsIndentedJson_WhenFlagIsTrue()
    {
        // Arrange
        var handler = new WebhookHandler();

        // Act
        var json = handler.ToJson(indented: true);

        // Assert
        Assert.Contains("\n", json);
        Assert.Contains("  ", json);
    }

    [Fact]
    public void FromJson_ReturnsHandler_ForValidJson()
    {
        // Arrange
        var handler = new WebhookHandler();
        var json = handler.ToJson();

        // Act
        var result = json.FromJson();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_ThrowsArgumentException_ForNullJson()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ((string)null).FromJson());
    }

    [Fact]
    public void FromJson_ThrowsArgumentException_ForEmptyJson()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => string.Empty.FromJson());
    }

    [Fact]
    public void FromJson_ThrowsJsonException_ForInvalidJson()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act & Assert
        Assert.Throws<JsonException>(() => invalidJson.FromJson());
    }

    [Fact]
    public void TryFromJson_ReturnsTrue_ForValidJson()
    {
        // Arrange
        var handler = new WebhookHandler();
        var json = handler.ToJson();

        // Act
        var success = json.TryFromJson(out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_ForInvalidJson()
    {
        // Arrange
        var invalidJson = "{ invalid }";

        // Act
        var success = invalidJson.TryFromJson(out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_ThrowsArgumentException_ForNullJson()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ((string)null).TryFromJson(out _));
    }
}
