using Xunit;
using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Utilities;

namespace DotNetRealtimePipeline.Tests.Utilities;

public class StatisticsHelperValidationTests
{
    [Fact]
    public void Validate_WithValidStatisticsHelper_ReturnsEmptyList()
    {
        // Arrange
        var statisticsHelper = new StatisticsHelper();

        // Act
        var problems = StatisticsHelperValidation.Validate(statisticsHelper);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_WithNaNMean_ReturnsProblem()
    {
        // Arrange
        var statisticsHelper = new StatisticsHelper { Mean = double.NaN };

        // Act
        var problems = StatisticsHelperValidation.Validate(statisticsHelper);

        // Assert
        Assert.Single(problems);
        Assert.Contains("Mean is NaN or infinity.", problems);
    }

    [Fact]
    public void Validate_WithInfinityMean_ReturnsProblem()
    {
        // Arrange
        var statisticsHelper = new StatisticsHelper { Mean = double.PositiveInfinity };

        // Act
        var problems = StatisticsHelperValidation.Validate(statisticsHelper);

        // Assert
        Assert.Single(problems);
        Assert.Contains("Mean is NaN or infinity.", problems);
    }

    [Fact]
    public void IsValid_WithValidStatisticsHelper_ReturnsTrue()
    {
        // Arrange
        var statisticsHelper = new StatisticsHelper();

        // Act
        var isValid = StatisticsHelperValidation.IsValid(statisticsHelper);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WithInvalidStatisticsHelper_ReturnsFalse()
    {
        // Arrange
        var statisticsHelper = new StatisticsHelper { Mean = double.NaN };

        // Act
        var isValid = StatisticsHelperValidation.IsValid(statisticsHelper);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_WithValidStatisticsHelper_DoesNotThrow()
    {
        // Arrange
        var statisticsHelper = new StatisticsHelper();

        // Act & Assert
        StatisticsHelperValidation.EnsureValid(statisticsHelper);
    }

    [Fact]
    public void EnsureValid_WithInvalidStatisticsHelper_ThrowsArgumentException()
    {
        // Arrange
        var statisticsHelper = new StatisticsHelper { Mean = double.NaN };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => StatisticsHelperValidation.EnsureValid(statisticsHelper));
    }

    [Fact]
    public void Validate_NullStatisticsHelper_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => StatisticsHelperValidation.Validate(null));
    }

    [Fact]
    public void IsValid_NullStatisticsHelper_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => StatisticsHelperValidation.IsValid(null));
    }

    [Fact]
    public void EnsureValid_NullStatisticsHelper_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => StatisticsHelperValidation.EnsureValid(null));
    }
}
