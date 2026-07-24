namespace DotNetRealtimePipeline.Tests.Unit;

using System;
using Xunit;
using DotNetRealtimePipeline.Domain.Models;
using static DotNetRealtimePipeline.Domain.Models.DataPointValidation;

public class DataPointValidationTests
{
    private static DataPoint CreateValidDataPoint()
    {
        return new DataPoint
        {
            Id = 1,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Value = 42.5,
            Source = "test-source",
            Quality = 95,
            CreatedAt = DateTime.UtcNow,
            Metadata = new System.Collections.Generic.Dictionary<string, object>(),
            Tags = "test-tag"
        };
    }

    [Fact]
    public void Validate_HappyPath_WithValidDataPoint_ReturnsEmptyList()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NullDataPoint_ThrowsArgumentNullException()
    {
        // Arrange
        DataPoint? dataPoint = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DataPointValidation.Validate(dataPoint));
    }

    [Fact]
    public void Validate_WithZeroId_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Id = 0;

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Id must be positive.", errors[0]);
    }

    [Fact]
    public void Validate_WithNegativeId_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Id = -1;

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Id must be positive.", errors[0]);
    }

    [Fact]
    public void Validate_WithZeroTimestamp_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Timestamp = 0;

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Timestamp must be positive.", errors[0]);
    }

    [Fact]
    public void Validate_WithNegativeTimestamp_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Timestamp = -1000;

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Timestamp must be positive.", errors[0]);
    }

    [Fact]
    public void Validate_WithNaNValue_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Value = double.NaN;

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Value cannot be NaN.", errors[0]);
    }

    [Fact]
    public void Validate_WithPositiveInfinityValue_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Value = double.PositiveInfinity;

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Value cannot be infinite.", errors[0]);
    }

    [Fact]
    public void Validate_WithNegativeInfinityValue_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Value = double.NegativeInfinity;

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Value cannot be infinite.", errors[0]);
    }

    [Fact]
    public void Validate_WithNullSource_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Source = null!;

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Source cannot be null or whitespace.", errors[0]);
    }

    [Fact]
    public void Validate_WithEmptySource_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Source = "";

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Source cannot be null or whitespace.", errors[0]);
    }

    [Fact]
    public void Validate_WithWhitespaceSource_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Source = "   ";

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Source cannot be null or whitespace.", errors[0]);
    }

    [Fact]
    public void Validate_WithQualityBelowZero_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Quality = -1;

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Quality must be between 0 and 100.", errors[0]);
    }

    [Fact]
    public void Validate_WithQualityAboveHundred_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Quality = 101;

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Quality must be between 0 and 100.", errors[0]);
    }

    [Fact]
    public void Validate_WithDefaultCreatedAt_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.CreatedAt = default;

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.CreatedAt cannot be default(DateTime).", errors[0]);
    }

    [Fact]
    public void Validate_WithNullMetadata_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Metadata = null!;

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Metadata cannot be null.", errors[0]);
    }

    [Fact]
    public void Validate_WithWhitespaceTags_ReturnsError()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Tags = "   ";

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Single(errors);
        Assert.Contains("DataPoint.Tags cannot be whitespace when set.", errors[0]);
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Id = 0;
        dataPoint.Timestamp = 0;
        dataPoint.Value = double.NaN;
        dataPoint.Source = "";
        dataPoint.Quality = 150;
        dataPoint.CreatedAt = default;
        dataPoint.Metadata = null!;

        // Act
        var errors = DataPointValidation.Validate(dataPoint);

        // Assert
        Assert.Equal(7, errors.Count);
        Assert.Contains("DataPoint.Id must be positive.", errors[0]);
        Assert.Contains("DataPoint.Timestamp must be positive.", errors[1]);
        Assert.Contains("DataPoint.Value cannot be NaN.", errors[2]);
        Assert.Contains("DataPoint.Source cannot be null or whitespace.", errors[3]);
        Assert.Contains("DataPoint.Quality must be between 0 and 100.", errors[4]);
        Assert.Contains("DataPoint.CreatedAt cannot be default(DateTime).", errors[5]);
        Assert.Contains("DataPoint.Metadata cannot be null.", errors[6]);
    }

    [Fact]
    public void IsValid_HappyPath_WithValidDataPoint_ReturnsTrue()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();

        // Act
        var isValid = DataPointValidation.IsValid(dataPoint);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_NullDataPoint_ReturnsFalse()
    {
        // Arrange
        DataPoint? dataPoint = null;

        // Act
        var isValid = DataPointValidation.IsValid(dataPoint);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_InvalidDataPoint_ReturnsFalse()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Id = 0; // Invalid

        // Act
        var isValid = DataPointValidation.IsValid(dataPoint);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_HappyPath_WithValidDataPoint_DoesNotThrow()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();

        // Act
        var exception = Record.Exception(() => DataPointValidation.EnsureValid(dataPoint));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_NullDataPoint_ThrowsArgumentNullException()
    {
        // Arrange
        DataPoint? dataPoint = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DataPointValidation.EnsureValid(dataPoint));
    }

    [Fact]
    public void EnsureValid_InvalidDataPoint_ThrowsArgumentException_WithErrorDetails()
    {
        // Arrange
        var dataPoint = CreateValidDataPoint();
        dataPoint.Id = 0;
        dataPoint.Source = "";

        // Act
        var ex = Assert.Throws<ArgumentException>(() => dataPoint.EnsureValid());

        // Assert
        Assert.Contains("DataPoint is invalid.", ex.Message);
        Assert.Contains("DataPoint.Id must be positive.", ex.Message);
        Assert.Contains("DataPoint.Source cannot be null or whitespace.", ex.Message);
    }
}