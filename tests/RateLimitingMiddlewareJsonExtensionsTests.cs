// tests/RateLimitingMiddlewareJsonExtensionsTests.cs
namespace DotNetRealtimePipeline.Tests;

public sealed class RateLimitingMiddlewareJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsExpectedJson()
    {
        // Arrange
        var rateLimitingMiddleware = new RateLimitingMiddleware();

        // Act
        var json = RateLimitingMiddlewareJsonExtensions.ToJson(rateLimitingMiddleware);

        // Assert
        Assert.NotNull(json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        RateLimitingMiddleware? rateLimitingMiddleware = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => RateLimitingMiddlewareJsonExtensions.ToJson(rateLimitingMiddleware));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsExpectedRateLimitingMiddleware()
    {
        // Arrange
        var json = "{\"rateLimitingMiddleware\": {}}";

        // Act
        var rateLimitingMiddleware = RateLimitingMiddlewareJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(rateLimitingMiddleware);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => RateLimitingMiddlewareJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var json = "{\"rateLimitingMiddleware\": {\"invalid\": \"json\"}}";

        // Act & Assert
        Assert.Throws<JsonException>(() => RateLimitingMiddlewareJsonExtensions.FromJson(json));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrue()
    {
        // Arrange
        var json = "{\"rateLimitingMiddleware\": {}}";

        // Act
        var result = RateLimitingMiddlewareJsonExtensions.TryFromJson(json, out var rateLimitingMiddleware);

        // Assert
        Assert.True(result);
        Assert.NotNull(rateLimitingMiddleware);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Arrange
        string? json = null;

        // Act
        var result = RateLimitingMiddlewareJsonExtensions.TryFromJson(json, out var rateLimitingMiddleware);

        // Assert
        Assert.False(result);
        Assert.Null(rateLimitingMiddleware);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Arrange
        var json = "{\"rateLimitingMiddleware\": {\"invalid\": \"json\"}}";

        // Act
        var result = RateLimitingMiddlewareJsonExtensions.TryFromJson(json, out var rateLimitingMiddleware);

        // Assert
        Assert.False(result);
        Assert.Null(rateLimitingMiddleware);
    }
}
