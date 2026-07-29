// tests/MetricsServiceValidationTests.cs
namespace DotNetRealtimePipeline.Tests;

public class MetricsServiceValidationTests
{
    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var metricsService = new MetricsService(new MetricsRepository(), new PipelineMetrics());

        // Act
        var problems = MetricsServiceValidation.Validate(metricsService);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_NullMetricsService_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => MetricsServiceValidation.Validate(null));
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var metricsService = new MetricsService(new MetricsRepository(), new PipelineMetrics());

        // Act
        var isValid = MetricsServiceValidation.IsValid(metricsService);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_NullMetricsService_ReturnsFalse()
    {
        // Act
        var isValid = MetricsServiceValidation.IsValid(null);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_InvalidMetricsService_ReturnsFalse()
    {
        // Arrange
        var metricsService = new MetricsService(null, new PipelineMetrics());

        // Act
        var isValid = MetricsServiceValidation.IsValid(metricsService);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var metricsService = new MetricsService(new MetricsRepository(), new PipelineMetrics());

        // Act and Assert
        Assert.DoesNotThrow(() => MetricsServiceValidation.EnsureValid(metricsService));
    }

    [Fact]
    public void EnsureValid_NullMetricsService_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => MetricsServiceValidation.EnsureValid(null));
    }

    [Fact]
    public void EnsureValid_InvalidMetricsService_ThrowsArgumentException()
    {
        // Arrange
        var metricsService = new MetricsService(null, new PipelineMetrics());

        // Act and Assert
        Assert.Throws<ArgumentException>(() => MetricsServiceValidation.EnsureValid(metricsService));
    }
}
