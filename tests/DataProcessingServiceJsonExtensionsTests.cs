// tests/DataProcessingServiceJsonExtensionsTests.cs
namespace DotNetRealtimePipeline.Tests;

public class DataProcessingServiceJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJson()
    {
        // Arrange
        var dataProcessingService = new DataProcessingService(new MetricsRepository(), new PipelineMetrics());

        // Act
        var json = DataProcessingServiceJsonExtensions.ToJson(dataProcessingService);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    [Fact]
    public void ToJson_NullInstance_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DataProcessingServiceJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsInstance()
    {
        // Arrange
        var dataProcessingService = new DataProcessingService(new MetricsRepository(), new PipelineMetrics());
        var json = DataProcessingServiceJsonExtensions.ToJson(dataProcessingService);

        // Act
        var instance = DataProcessingServiceJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(instance);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DataProcessingServiceJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyOrWhiteSpace_ReturnsNull()
    {
        // Act
        var instance = DataProcessingServiceJsonExtensions.FromJson("   ");

        // Assert
        Assert.Null(instance);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndInstance()
    {
        // Arrange
        var dataProcessingService = new DataProcessingService(new MetricsRepository(), new PipelineMetrics());
        var json = DataProcessingServiceJsonExtensions.ToJson(dataProcessingService);

        // Act
        var success = DataProcessingServiceJsonExtensions.TryFromJson(json, out var instance);

        // Assert
        Assert.True(success);
        Assert.NotNull(instance);
    }

    [Fact]
    public void TryFromJson_EmptyString_ReturnsTrueAndNull()
    {
        // Act
        var success = DataProcessingServiceJsonExtensions.TryFromJson(string.Empty, out var instance);

        // Assert
        Assert.True(success);
        Assert.Null(instance);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act
        var success = DataProcessingServiceJsonExtensions.TryFromJson(invalidJson, out var instance);

        // Assert
        Assert.False(success);
        Assert.Null(instance);
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DataProcessingServiceJsonExtensions.TryFromJson(null, out var _));
    }
}
