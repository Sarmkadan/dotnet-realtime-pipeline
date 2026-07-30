// tests/PathHelperValidationTests.cs
using System;
using System.Collections.Generic;
using Xunit;
using DotNetRealtimePipeline.Utilities;

namespace DotNetRealtimePipeline.Tests.Utilities;

public class PathHelperValidationTests
{
    [Fact]
    public void Validate_NullPathHelper_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PathHelperValidation.Validate(null));
    }

    [Fact]
    public void Validate_EmptyOriginalPath_ReturnsSingleProblem()
    {
        // Arrange
        var pathHelper = new PathHelper { OriginalPath = string.Empty };

        // Act
        var problems = PathHelperValidation.Validate(pathHelper);

        // Assert
        Assert.Single(problems);
        Assert.Equal("OriginalPath cannot be null or whitespace.", problems[0]);
    }

    [Fact]
    public void Validate_NullOriginalPath_ReturnsSingleProblem()
    {
        // Arrange
        var pathHelper = new PathHelper { OriginalPath = null };

        // Act
        var problems = PathHelperValidation.Validate(pathHelper);

        // Assert
        Assert.Single(problems);
        Assert.Equal("OriginalPath cannot be null or whitespace.", problems[0]);
    }

    [Fact]
    public void Validate_ValidPath_ReturnsEmptyList()
    {
        // Arrange
        var pathHelper = new PathHelper { OriginalPath = "path/to/file.txt" };

        // Act
        var problems = PathHelperValidation.Validate(pathHelper);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_NullPathHelper_ReturnsFalse()
    {
        // Act
        var isValid = PathHelperValidation.IsValid(null);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_EmptyOriginalPath_ReturnsFalse()
    {
        // Arrange
        var pathHelper = new PathHelper { OriginalPath = string.Empty };

        // Act
        var isValid = PathHelperValidation.IsValid(pathHelper);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_NullOriginalPath_ReturnsFalse()
    {
        // Arrange
        var pathHelper = new PathHelper { OriginalPath = null };

        // Act
        var isValid = PathHelperValidation.IsValid(pathHelper);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_ValidPath_ReturnsTrue()
    {
        // Arrange
        var pathHelper = new PathHelper { OriginalPath = "path/to/file.txt" };

        // Act
        var isValid = PathHelperValidation.IsValid(pathHelper);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_NullPathHelper_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PathHelperValidation.EnsureValid(null));
    }

    [Fact]
    public void EnsureValid_EmptyOriginalPath_ThrowsArgumentException()
    {
        // Arrange
        var pathHelper = new PathHelper { OriginalPath = string.Empty };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PathHelperValidation.EnsureValid(pathHelper));
    }

    [Fact]
    public void EnsureValid_NullOriginalPath_ThrowsArgumentException()
    {
        // Arrange
        var pathHelper = new PathHelper { OriginalPath = null };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PathHelperValidation.EnsureValid(pathHelper));
    }

    [Fact]
    public void EnsureValid_ValidPath_DoesNotThrow()
    {
        // Arrange
        var pathHelper = new PathHelper { OriginalPath = "path/to/file.txt" };

        // Act
        PathHelperValidation.EnsureValid(pathHelper);
    }
}
