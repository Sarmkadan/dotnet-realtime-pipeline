using System;
using System.Collections.Generic;
using Xunit;

namespace DotNetRealtimePipeline.Domain.Models;

public class DataPointValidationTests
{
    [Fact]
    public void Validate_WithValidDataPoint_ReturnsEmptyList()
    {
        var dataPoint = new DataPoint(1, 1234567890, 42.5, "sensor-01")
        {
            Quality = 85,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "unit", "celsius" } }
        };

        var errors = dataPoint.Validate();

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WithNullDataPoint_ThrowsArgumentNullException()
    {
        DataPoint? dataPoint = null;
        Assert.Throws<ArgumentNullException>(() => dataPoint!.Validate());
    }

    [Fact]
    public void Validate_WithInvalidId_ReturnsError()
    {
        var dataPoint = new DataPoint(0, 1234567890, 42.5, "sensor-01")
        {
            Quality = 85,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "unit", "celsius" } }
        };

        var errors = dataPoint.Validate();

        Assert.Single(errors);
        Assert.Contains("DataPoint.Id must be positive.", errors);
    }

    [Fact]
    public void Validate_WithZeroTimestamp_ReturnsError()
    {
        var dataPoint = new DataPoint(1, 0, 42.5, "sensor-01")
        {
            Quality = 85,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "unit", "celsius" } }
        };

        var errors = dataPoint.Validate();

        Assert.Single(errors);
        Assert.Contains("DataPoint.Timestamp must be positive.", errors);
    }

    [Fact]
    public void Validate_WithNaNValue_ReturnsError()
    {
        var dataPoint = new DataPoint(1, 1234567890, double.NaN, "sensor-01")
        {
            Quality = 85,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "unit", "celsius" } }
        };

        var errors = dataPoint.Validate();

        Assert.Single(errors);
        Assert.Contains("DataPoint.Value cannot be NaN.", errors);
    }

    [Fact]
    public void Validate_WithInfiniteValue_ReturnsError()
    {
        var dataPoint = new DataPoint(1, 1234567890, double.PositiveInfinity, "sensor-01")
        {
            Quality = 85,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "unit", "celsius" } }
        };

        var errors = dataPoint.Validate();

        Assert.Single(errors);
        Assert.Contains("DataPoint.Value cannot be infinite.", errors);
    }

    [Fact]
    public void Validate_WithInvalidSource_ReturnsError()
    {
        var dataPoint = new DataPoint(1, 1234567890, 42.5, null!)
        {
            Quality = 85,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "unit", "celsius" } }
        };

        var errors = dataPoint.Validate();

        Assert.Single(errors);
        Assert.Contains("DataPoint.Source cannot be null or whitespace.", errors);
    }

    [Fact]
    public void Validate_WithInvalidQuality_ReturnsError()
    {
        var dataPoint = new DataPoint(1, 1234567890, 42.5, "sensor-01")
        {
            Quality = 150,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "unit", "celsius" } }
        };

        var errors = dataPoint.Validate();

        Assert.Single(errors);
        Assert.Contains("DataPoint.Quality must be between 0 and 100.", errors);
    }

    [Fact]
    public void Validate_WithDefaultCreatedAt_ReturnsError()
    {
        var dataPoint = new DataPoint(1, 1234567890, 42.5, "sensor-01")
        {
            Quality = 85,
            CreatedAt = default,
            Metadata = new Dictionary<string, object> { { "unit", "celsius" } }
        };

        var errors = dataPoint.Validate();

        Assert.Single(errors);
        Assert.Contains("DataPoint.CreatedAt cannot be default(DateTime).", errors);
    }

    [Fact]
    public void Validate_WithNullMetadata_ReturnsError()
    {
        var dataPoint = new DataPoint(1, 1234567890, 42.5, "sensor-01")
        {
            Quality = 85,
            CreatedAt = DateTime.UtcNow,
            Metadata = null!
        };

        var errors = dataPoint.Validate();

        Assert.Single(errors);
        Assert.Contains("DataPoint.Metadata cannot be null.", errors);
    }

    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        var dataPoint = new DataPoint(0, 0, double.NaN, string.Empty)
        {
            Quality = 150,
            CreatedAt = default,
            Metadata = null!
        };

        var errors = dataPoint.Validate();

        Assert.Equal(6, errors.Count);
        Assert.Contains("DataPoint.Id must be positive.", errors);
        Assert.Contains("DataPoint.Timestamp must be positive.", errors);
        Assert.Contains("DataPoint.Value cannot be NaN.", errors);
        Assert.Contains("DataPoint.Source cannot be null or whitespace.", errors);
        Assert.Contains("DataPoint.Quality must be between 0 and 100.", errors);
        Assert.Contains("DataPoint.CreatedAt cannot be default(DateTime).", errors);
    }

    [Fact]
    public void IsValid_WithValidDataPoint_ReturnsTrue()
    {
        var dataPoint = new DataPoint(1, 1234567890, 42.5, "sensor-01")
        {
            Quality = 85,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "unit", "celsius" } }
        };

        Assert.True(dataPoint.IsValid());
    }

    [Fact]
    public void IsValid_WithInvalidDataPoint_ReturnsFalse()
    {
        var dataPoint = new DataPoint(0, 1234567890, 42.5, "sensor-01")
        {
            Quality = 85,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "unit", "celsius" } }
        };

        Assert.False(dataPoint.IsValid());
    }

    [Fact]
    public void EnsureValid_WithValidDataPoint_DoesNotThrow()
    {
        var dataPoint = new DataPoint(1, 1234567890, 42.5, "sensor-01")
        {
            Quality = 85,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "unit", "celsius" } }
        };

        var exception = Record.Exception(() => dataPoint.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_WithInvalidDataPoint_ThrowsArgumentException()
    {
        var dataPoint = new DataPoint(0, 1234567890, 42.5, "sensor-01")
        {
            Quality = 85,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "unit", "celsius" } }
        };

        var exception = Assert.Throws<ArgumentException>(() => dataPoint.EnsureValid());
        Assert.Contains("DataPoint is invalid", exception.Message);
    }
}