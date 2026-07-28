using Xunit;

namespace DotNetRealtimePipeline.Tests.Middleware;

public class ErrorHandlingMiddlewareJsonExtensionsTests
{
    [Fact]
    public void ToJson_ThrowsNotSupportedException()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware();

        // Act and Assert
        Assert.Throws<NotSupportedException>(() => ErrorHandlingMiddlewareJsonExtensions.ToJson(middleware));
    }

    [Fact]
    public void FromJson_ThrowsNotSupportedException()
    {
        // Arrange
        var json = "{}";

        // Act and Assert
        Assert.Throws<NotSupportedException>(() => ErrorHandlingMiddlewareJsonExtensions.FromJson(json));
    }

    [Fact]
    public void TryFromJson_ReturnsFalse()
    {
        // Arrange
        var json = "{}";

        // Act
        var result = ErrorHandlingMiddlewareJsonExtensions.TryFromJson(json, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ErrorHandlingMiddlewareJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ErrorHandlingMiddlewareJsonExtensions.FromJson(null));
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var result = ErrorHandlingMiddlewareJsonExtensions.TryFromJson(null, out _);

        // Assert
        Assert.False(result);
    }
}
