using Xunit;

namespace DotNetRealtimePipeline.Tests.Integration;

public class PipelineHttpClientFactoryValidationTests
{
    [Fact]
    public void Validate_ReturnsEmptyList_WhenFactoryIsValid()
    {
        // Arrange
        var factory = new PipelineHttpClientFactory(
            TimeSpan.FromSeconds(1),
            2,
            TimeSpan.FromSeconds(3),
            4,
            "User-Agent",
            new Dictionary<string, string> { { "Header1", "Value1" } }
        );

        // Act
        var errors = PipelineHttpClientFactoryValidation.Validate(factory);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ReturnsListWithTimeoutError_WhenTimeoutIsZero()
    {
        // Arrange
        var factory = new PipelineHttpClientFactory(
            TimeSpan.Zero,
            2,
            TimeSpan.FromSeconds(3),
            4,
            "User-Agent",
            new Dictionary<string, string> { { "Header1", "Value1" } }
        );

        // Act
        var errors = PipelineHttpClientFactoryValidation.Validate(factory);

        // Assert
        Assert.Single(errors);
        Assert.Equal("PipelineHttpClientFactory.Timeout must be greater than zero.", errors[0]);
    }

    [Fact]
    public void Validate_ReturnsListWithMaxRetriesError_WhenMaxRetriesIsNegative()
    {
        // Arrange
        var factory = new PipelineHttpClientFactory(
            TimeSpan.FromSeconds(1),
            -1,
            TimeSpan.FromSeconds(3),
            4,
            "User-Agent",
            new Dictionary<string, string> { { "Header1", "Value1" } }
        );

        // Act
        var errors = PipelineHttpClientFactoryValidation.Validate(factory);

        // Assert
        Assert.Single(errors);
        Assert.Equal("PipelineHttpClientFactory.MaxRetries must be a non-negative integer.", errors[0]);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenFactoryIsValid()
    {
        // Arrange
        var factory = new PipelineHttpClientFactory(
            TimeSpan.FromSeconds(1),
            2,
            TimeSpan.FromSeconds(3),
            4,
            "User-Agent",
            new Dictionary<string, string> { { "Header1", "Value1" } }
        );

        // Act
        var isValid = PipelineHttpClientFactoryValidation.IsValid(factory);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenFactoryIsInvalid()
    {
        // Arrange
        var factory = new PipelineHttpClientFactory(
            TimeSpan.Zero,
            2,
            TimeSpan.FromSeconds(3),
            4,
            "User-Agent",
            new Dictionary<string, string> { { "Header1", "Value1" } }
        );

        // Act
        var isValid = PipelineHttpClientFactoryValidation.IsValid(factory);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_WhenFactoryIsInvalid()
    {
        // Arrange
        var factory = new PipelineHttpClientFactory(
            TimeSpan.Zero,
            2,
            TimeSpan.FromSeconds(3),
            4,
            "User-Agent",
            new Dictionary<string, string> { { "Header1", "Value1" } }
        );

        // Act and Assert
        Assert.Throws<ArgumentException>(() => PipelineHttpClientFactoryValidation.EnsureValid(factory));
    }
}
