using Xunit;

namespace DotNetRealtimePipeline.Tests.Middleware;

public class RateLimitingMiddlewareTests
{
    [Fact]
    public void Constructor_WithDefaultValues_CreatesMiddleware()
    {
        // Arrange
        var middleware = new RateLimitingMiddleware();

        // Act & Assert
        Assert.NotNull(middleware);
    }

    [Fact]
    public void Constructor_WithCustomValues_CreatesMiddleware()
    {
        // Arrange
        var middleware = new RateLimitingMiddleware(100, 500);

        // Act & Assert
        Assert.NotNull(middleware);
    }

    [Fact]
    public void TryAcquire_WithValidTokens_ReturnsTrue()
    {
        // Arrange
        var middleware = new RateLimitingMiddleware(100, 500);
        var identifier = "test-identifier";

        // Act
        var result = middleware.TryAcquire(identifier);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryAcquire_WithInvalidTokens_ReturnsFalse()
    {
        // Arrange
        var middleware = new RateLimitingMiddleware(100, 500);
        var identifier = "test-identifier";

        // Act
        var result = middleware.TryAcquire(identifier, 1001);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetStatus_WithValidTokens_ReturnsStatus()
    {
        // Arrange
        var middleware = new RateLimitingMiddleware(100, 500);
        var identifier = "test-identifier";

        // Act
        var result = middleware.GetStatus(identifier);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Reset_WithValidIdentifier_ResetsStatus()
    {
        // Arrange
        var middleware = new RateLimitingMiddleware(100, 500);
        var identifier = "test-identifier";

        // Act
        middleware.Reset(identifier);

        // Assert
        Assert.Null(middleware._buckets.TryGetValue(identifier, out var bucket));
    }

    [Fact]
    public void GetAllStatuses_WithMultipleIdentifiers_ReturnsStatuses()
    {
        // Arrange
        var middleware = new RateLimitingMiddleware(100, 500);
        var identifier1 = "test-identifier-1";
        var identifier2 = "test-identifier-2";

        // Act
        var result = middleware.GetAllStatuses();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void AvailableTokens_WithValidTokens_ReturnsTokens()
    {
        // Arrange
        var middleware = new RateLimitingMiddleware(100, 500);

        // Act
        var result = middleware.AvailableTokens;

        // Assert
        Assert.Equal(500, result);
    }

    [Fact]
    public void Capacity_WithValidTokens_ReturnsCapacity()
    {
        // Arrange
        var middleware = new RateLimitingMiddleware(100, 500);

        // Act
        var result = middleware.Capacity;

        // Assert
        Assert.Equal(500, result);
    }

    [Fact]
    public void ResetTime_WithValidTokens_ReturnsResetTime()
    {
        // Arrange
        var middleware = new RateLimitingMiddleware(100, 500);

        // Act
        var result = middleware.ResetTime;

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void TryConsume_WithValidTokens_ReturnsTrue()
    {
        // Arrange
        var middleware = new RateLimitingMiddleware(100, 500);
        var identifier = "test-identifier";

        // Act
        var result = middleware.TryConsume(identifier);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryConsume_WithInvalidTokens_ReturnsFalse()
    {
        // Arrange
        var middleware = new RateLimitingMiddleware(100, 500);
        var identifier = "test-identifier";

        // Act
        var result = middleware.TryConsume(identifier, 1001);

        // Assert
        Assert.False(result);
    }
}
