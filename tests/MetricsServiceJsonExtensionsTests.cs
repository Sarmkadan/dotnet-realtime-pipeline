using Xunit;

namespace DotNetRealtimePipeline.Services.Tests;

public class MetricsServiceJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath()
    {
        // Arrange
        var metricsService = new MetricsService();
        var expectedJson = "{\"key\":\"value\"}";

        // Act
        var actualJson = MetricsServiceJsonExtensions.ToJson(metricsService);

        // Assert
        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public void ToJson_NullMetricsService_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => MetricsServiceJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath()
    {
        // Arrange
        var json = "{\"key\":\"value\"}";
        var expectedMetricsService = new MetricsService();

        // Act
        var actualMetricsService = MetricsServiceJsonExtensions.FromJson(json);

        // Assert
        Assert.Equal(expectedMetricsService, actualMetricsService);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => MetricsServiceJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyJson_ReturnsNull()
    {
        // Act
        var actualMetricsService = MetricsServiceJsonExtensions.FromJson("");

        // Assert
        Assert.Null(actualMetricsService);
    }

    [Fact]
    public void TryFromJson_HappyPath()
    {
        // Arrange
        var json = "{\"key\":\"value\"}";
        var expectedMetricsService = new MetricsService();

        // Act
        var result = MetricsServiceJsonExtensions.TryFromJson(json, out var actualMetricsService);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedMetricsService, actualMetricsService);
    }

    [Fact]
    public void TryFromJson_NullJson_ReturnsFalse()
    {
        // Act
        var result = MetricsServiceJsonExtensions.TryFromJson(null, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsFalse()
    {
        // Act
        var result = MetricsServiceJsonExtensions.TryFromJson("", out _);

        // Assert
        Assert.False(result);
    }
}
