// tests/PipelineExceptionValidationTests.cs
using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Domain.Exceptions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

public class PipelineExceptionValidationTests
{
    // Helper to create a minimal valid PipelineException
    private static PipelineException CreateValidBaseException()
        => new PipelineException("Something went wrong", "ERR001");

    [Fact]
    public void Validate_WithValidBaseException_ReturnsEmptyList()
    {
        // Arrange
        var ex = CreateValidBaseException();

        // Act
        IReadOnlyList<string> result = ex.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        PipelineException? ex = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ex!.Validate());
    }

    [Fact]
    public void IsValid_WithValidBaseException_ReturnsTrue()
    {
        // Arrange
        var ex = CreateValidBaseException();

        // Act
        bool isValid = ex.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WithInvalidBaseException_ReturnsFalse()
    {
        // Arrange: empty message triggers a validation error
        var ex = new PipelineException(string.Empty, "ERR001");

        // Act
        bool isValid = ex.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_WithValidBaseException_DoesNotThrow()
    {
        // Arrange
        var ex = CreateValidBaseException();

        // Act & Assert
        var exception = Record.Exception(() => ex.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_WithInvalidBaseException_ThrowsArgumentException()
    {
        // Arrange: missing error code triggers a validation error
        var ex = new PipelineException("Message present", string.Empty);

        // Act & Assert
        var argEx = Assert.Throws<ArgumentException>(() => ex.EnsureValid());
        Assert.Contains("PipelineException is not valid", argEx.Message);
    }

    [Fact]
    public void Validate_BackpressureException_WithInvalidValues_ReturnsErrors()
    {
        // Arrange: create a BackpressureException with several invalid fields
        var ex = new BackpressureException("Backpressure detected", "BP001")
        {
            BufferSize = 0,          // invalid: must be > 0
            MaxCapacity = -5        // invalid: must be > 0 and >= BufferSize
        };

        // Act
        IReadOnlyList<string> errors = ex.Validate();

        // Assert
        Assert.Contains("BufferSize must be a positive number for BackpressureException.", errors);
        Assert.Contains("MaxCapacity must be a positive number for BackpressureException.", errors);
        // The third rule (MaxCapacity >= BufferSize) is also violated because MaxCapacity is negative
        Assert.Contains("MaxCapacity must be greater than or equal to BufferSize for BackpressureException.", errors);
    }

    [Fact]
    public void Validate_BackpressureException_WithValidValues_ReturnsEmptyList()
    {
        // Arrange
        var ex = new BackpressureException("Backpressure detected", "BP001")
        {
            BufferSize = 10,
            MaxCapacity = 20
        };

        // Act
        IReadOnlyList<string> errors = ex.Validate();

        // Assert
        Assert.Empty(errors);
    }
}
