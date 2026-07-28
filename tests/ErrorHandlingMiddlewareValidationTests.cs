// tests/ErrorHandlingMiddlewareValidationTests.cs
namespace DotNetRealtimePipeline.Tests.Middleware;

using Xunit;

public class ErrorHandlingMiddlewareValidationTests
{
    [Fact]
    public void Validate_ReturnsEmptyList_WhenMiddlewareIsValid()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware();

        // Act
        var result = ErrorHandlingMiddlewareValidation.Validate(middleware);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ReturnsListWithErrors_WhenMiddlewareIsInvalid()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware();
        middleware.ErrorMappers = new Dictionary<Type, Func<Exception, ErrorResponse>>
        {
            { typeof(Exception), _ => new ErrorResponse() }
        };

        // Act
        var result = ErrorHandlingMiddlewareValidation.Validate(middleware);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenMiddlewareIsNull()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ErrorHandlingMiddlewareValidation.Validate(null));
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenMiddlewareIsValid()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware();

        // Act
        var result = ErrorHandlingMiddlewareValidation.IsValid(middleware);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenMiddlewareIsInvalid()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware();
        middleware.ErrorMappers = new Dictionary<Type, Func<Exception, ErrorResponse>>
        {
            { typeof(Exception), _ => new ErrorResponse() }
        };

        // Act
        var result = ErrorHandlingMiddlewareValidation.IsValid(middleware);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ThrowsArgumentNullException_WhenMiddlewareIsNull()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ErrorHandlingMiddlewareValidation.IsValid(null));
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_WhenMiddlewareIsValid()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware();

        // Act and Assert
        ErrorHandlingMiddlewareValidation.EnsureValid(middleware);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_WhenMiddlewareIsInvalid()
    {
        // Arrange
        var middleware = new ErrorHandlingMiddleware();
        middleware.ErrorMappers = new Dictionary<Type, Func<Exception, ErrorResponse>>
        {
            { typeof(Exception), _ => new ErrorResponse() }
        };

        // Act and Assert
        Assert.Throws<ArgumentException>(() => ErrorHandlingMiddlewareValidation.EnsureValid(middleware));
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentNullException_WhenMiddlewareIsNull()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ErrorHandlingMiddlewareValidation.EnsureValid(null));
    }
}
