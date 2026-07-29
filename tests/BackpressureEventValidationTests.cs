// tests/BackpressureEventValidationTests.cs
namespace DotNetRealtimePipeline.Tests;

using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Metrics;
using Xunit;

public class BackpressureEventValidationTests
{
    private static BackpressureEvent CreateValidEvent()
    {
        return new BackpressureEvent
        {
            StageName = "stage-1",
            BufferFillPercent = 42.5,
            Timestamp = DateTime.UtcNow,
            DroppedItems = 0,
            IsActivation = true
        };
    }

    [Fact]
    public void Validate_ValidEvent_ReturnsEmptyList()
    {
        // Arrange
        var ev = CreateValidEvent();

        // Act
        IReadOnlyList<string> errors = ev.Validate();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        BackpressureEvent? ev = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ev!.Validate());
    }

    [Fact]
    public void Validate_InvalidEvent_ReturnsAllErrors()
    {
        // Arrange
        var ev = new BackpressureEvent
        {
            StageName = null,
            BufferFillPercent = 150, // out of range
            Timestamp = default,     // default DateTime
            DroppedItems = -5,       // negative
            IsActivation = false    // must be true for activation events
        };

        // Act
        IReadOnlyList<string> errors = ev.Validate();

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains("StageName cannot be null or whitespace.", errors);
        Assert.Contains("BufferFillPercent must be between 0 and 100", errors);
        Assert.Contains("Timestamp cannot be default DateTime.", errors);
        Assert.Contains("DroppedItems must be non-negative", errors);
        Assert.Contains("IsActivation must be true for activation events.", errors);
        Assert.Equal(5, errors.Count);
    }

    [Fact]
    public void IsValid_ValidEvent_ReturnsTrue()
    {
        // Arrange
        var ev = CreateValidEvent();

        // Act
        bool result = ev.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_InvalidEvent_ReturnsFalse()
    {
        // Arrange
        var ev = new BackpressureEvent
        {
            StageName = "",
            BufferFillPercent = -1,
            Timestamp = DateTime.UtcNow,
            DroppedItems = 0,
            IsActivation = true
        };

        // Act
        bool result = ev.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EnsureValid_ValidEvent_DoesNotThrow()
    {
        // Arrange
        var ev = CreateValidEvent();

        // Act & Assert
        var exception = Record.Exception(() => ev.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_InvalidEvent_ThrowsArgumentException()
    {
        // Arrange
        var ev = new BackpressureEvent
        {
            StageName = null,
            BufferFillPercent = 200,
            Timestamp = default,
            DroppedItems = -1,
            IsActivation = false
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ev.EnsureValid());
        Assert.Contains("BackpressureEvent validation failed", ex.Message);
        Assert.Contains("StageName cannot be null or whitespace.", ex.Message);
    }

    [Fact]
    public void EnsureValid_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        BackpressureEvent? ev = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ev!.EnsureValid());
    }
}
