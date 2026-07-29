// tests/PipelineInitializerJsonExtensionsTests.cs
namespace DotNetRealtimePipeline.Tests;

using System;
using System.Text.Json;
using Xunit;

public class PipelineInitializerJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var pipelineInitializer = new PipelineInitializer(new ServiceProvider(), new Logger(), new StateManager());
        var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Act
        var json = PipelineInitializerJsonExtensions.ToJson(pipelineInitializer);

        // Assert
        Assert.NotNull(json);
        Assert.IsType<string>(json);
    }

    [Fact]
    public void ToJson_NullPipelineInitializer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineInitializerJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsPipelineInitializer()
    {
        // Arrange
        var pipelineInitializer = new PipelineInitializer(new ServiceProvider(), new Logger(), new StateManager());
        var json = JsonSerializer.Serialize(pipelineInitializer, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        // Act
        var result = PipelineInitializerJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<PipelineInitializer>(result);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineInitializerJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyJson_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => PipelineInitializerJsonExtensions.FromJson(""));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrue()
    {
        // Arrange
        var pipelineInitializer = new PipelineInitializer(new ServiceProvider(), new Logger(), new StateManager());
        var json = JsonSerializer.Serialize(pipelineInitializer, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        // Act
        var result = PipelineInitializerJsonExtensions.TryFromJson(json, out var value);

        // Assert
        Assert.True(result);
        Assert.NotNull(value);
        Assert.IsType<PipelineInitializer>(value);
    }

    [Fact]
    public void TryFromJson_NullJson_ReturnsFalse()
    {
        // Act
        var result = PipelineInitializerJsonExtensions.TryFromJson(null, out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsFalse()
    {
        // Act
        var result = PipelineInitializerJsonExtensions.TryFromJson("", out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }
}
