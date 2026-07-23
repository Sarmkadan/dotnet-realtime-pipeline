// tests/RawPipelineAccessorValidationTests.cs
namespace DotNetRealtimePipeline.Integration;

using Xunit;
using System;
using DotNetRealtimePipeline.Integration;

public class RawPipelineAccessorValidationTests
{
    [Fact]
    public void Validate_ReturnsEmptyList_ForValidRawPipelineAccessor()
    {
        // Arrange
        var rawPipelineAccessor = new RawPipelineAccessor();

        // Act
        var errors = RawPipelineAccessorValidation.Validate(rawPipelineAccessor);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ReturnsEmptyList_ForNullRawPipelineAccessor()
    {
        // Act and Assert
        Assert.Empty(RawPipelineAccessorValidation.Validate(null));
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForValidRawPipelineAccessor()
    {
        // Arrange
        var rawPipelineAccessor = new RawPipelineAccessor();

        // Act
        var isValid = RawPipelineAccessorValidation.IsValid(rawPipelineAccessor);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNullRawPipelineAccessor()
    {
        // Act and Assert
        Assert.False(RawPipelineAccessorValidation.IsValid(null));
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_ForValidRawPipelineAccessor()
    {
        // Arrange
        var rawPipelineAccessor = new RawPipelineAccessor();

        // Act and Assert
        RawPipelineAccessorValidation.EnsureValid(rawPipelineAccessor);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_ForNullRawPipelineAccessor()
    {
        // Act and Assert
        Assert.Throws<ArgumentException>(() => RawPipelineAccessorValidation.EnsureValid(null));
    }
}
