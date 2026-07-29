namespace DotNetRealtimePipeline.Tests;

using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Services;
using Xunit;

public class DataProcessingServiceValidationTests
{
    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var service = new DataProcessingService(new MetricsRepository(), new PipelineMetrics());

        // Act
        var errors = DataProcessingServiceValidation.Validate(service);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DataProcessingServiceValidation.Validate(null));
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var service = new DataProcessingService(new MetricsRepository(), new PipelineMetrics());

        // Act
        var isValid = DataProcessingServiceValidation.IsValid(service);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_NullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DataProcessingServiceValidation.IsValid(null));
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var service = new DataProcessingService(new MetricsRepository(), new PipelineMetrics());

        // Act & Assert
        DataProcessingServiceValidation.EnsureValid(service);
    }

    [Fact]
    public void EnsureValid_NullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DataProcessingServiceValidation.EnsureValid(null));
    }

    [Fact]
    public void EnsureValid_InvalidService_ThrowsArgumentException()
    {
        // Arrange
        var service = new DataProcessingService(null, new PipelineMetrics());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => DataProcessingServiceValidation.EnsureValid(service));
    }
}
