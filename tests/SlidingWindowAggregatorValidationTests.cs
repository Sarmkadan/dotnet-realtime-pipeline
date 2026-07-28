using Xunit;

namespace DotNetRealtimePipeline.Services.Tests;

public class SlidingWindowAggregatorValidationTests
{
    [Fact]
    public void Validate_HappyPath()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(1000, 500);

        // Act
        var problems = SlidingWindowAggregatorValidation.Validate(aggregator);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_WindowSizeMs_LessThanZero_ThrowsArgumentException()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(-1000, 500);

        // Act and Assert
        Assert.Throws<ArgumentException>(() => SlidingWindowAggregatorValidation.Validate(aggregator));
    }

    [Fact]
    public void Validate_StepIntervalMs_LessThanZero_ThrowsArgumentException()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(1000, -500);

        // Act and Assert
        Assert.Throws<ArgumentException>(() => SlidingWindowAggregatorValidation.Validate(aggregator));
    }

    [Fact]
    public void Validate_StepIntervalMs_GreaterThanWindowSizeMs_ThrowsArgumentException()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(1000, 1500);

        // Act and Assert
        Assert.Throws<ArgumentException>(() => SlidingWindowAggregatorValidation.Validate(aggregator));
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(1000, 500);

        // Act
        var isValid = SlidingWindowAggregatorValidation.IsValid(aggregator);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WindowSizeMs_LessThanZero_ReturnsFalse()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(-1000, 500);

        // Act
        var isValid = SlidingWindowAggregatorValidation.IsValid(aggregator);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_StepIntervalMs_LessThanZero_ReturnsFalse()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(1000, -500);

        // Act
        var isValid = SlidingWindowAggregatorValidation.IsValid(aggregator);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_StepIntervalMs_GreaterThanWindowSizeMs_ReturnsFalse()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(1000, 1500);

        // Act
        var isValid = SlidingWindowAggregatorValidation.IsValid(aggregator);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_HappyPath_NoExceptionThrown()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(1000, 500);

        // Act and Assert
        SlidingWindowAggregatorValidation.EnsureValid(aggregator);
    }

    [Fact]
    public void EnsureValid_WindowSizeMs_LessThanZero_ThrowsArgumentException()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(-1000, 500);

        // Act and Assert
        Assert.Throws<ArgumentException>(() => SlidingWindowAggregatorValidation.EnsureValid(aggregator));
    }

    [Fact]
    public void EnsureValid_StepIntervalMs_LessThanZero_ThrowsArgumentException()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(1000, -500);

        // Act and Assert
        Assert.Throws<ArgumentException>(() => SlidingWindowAggregatorValidation.EnsureValid(aggregator));
    }

    [Fact]
    public void EnsureValid_StepIntervalMs_GreaterThanWindowSizeMs_ThrowsArgumentException()
    {
        // Arrange
        var aggregator = new SlidingWindowAggregator(1000, 1500);

        // Act and Assert
        Assert.Throws<ArgumentException>(() => SlidingWindowAggregatorValidation.EnsureValid(aggregator));
    }
}
