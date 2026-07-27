using System;
using System.Collections.Generic;
using Xunit;
using DotNetRealtimePipeline.Domain.Models;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Unit tests for <see cref="BackpressureContextValidation"/> extension methods.
/// </summary>
public sealed class BackpressureContextValidationTests
{
    private static BackpressureContext CreateValidContext()
    {
        return new BackpressureContext
        {
            ContextId = 1,
            PipelineStageName = "StageA",
            BufferSize = 10,
            MaxBufferCapacity = 100,
            IsBackpressured = false,
            BackpressureStartTimeMs = 0,
            TotalBackpressureTimeMs = 0,
            DroppedItemCount = 0,
            ActiveConsumers = 1,
            MaxConcurrentConsumers = 5,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            BackpressureEventTimestamps = new List<long> { 1000, 2000 },
            BufferMetrics = new Dictionary<string, long> { ["metric1"] = 42 }
        };
    }

    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateValidContext();

        // Act
        var errors = context.Validate();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var context = CreateValidContext();

        // Act
        var result = context.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var context = CreateValidContext();

        // Act / Assert
        var exception = Record.Exception(() => context.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        BackpressureContext? context = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context!.Validate());
    }

    [Fact]
    public void IsValid_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        BackpressureContext? context = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context!.IsValid());
    }

    [Fact]
    public void EnsureValid_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        BackpressureContext? context = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context!.EnsureValid());
    }

    [Fact]
    public void Validate_InvalidValues_ReturnsExpectedErrors()
    {
        // Arrange: create a context that violates several rules
        var context = new BackpressureContext
        {
            ContextId = 0, // invalid: must be positive
            PipelineStageName = new string('x', 101), // invalid: exceeds 100 chars
            BufferSize = 150,
            MaxBufferCapacity = 100, // BufferSize > MaxBufferCapacity
            IsBackpressured = true,
            BackpressureStartTimeMs = -5, // invalid: negative
            TotalBackpressureTimeMs = -10, // invalid: negative
            DroppedItemCount = -1, // invalid: negative
            ActiveConsumers = -2, // invalid: negative
            MaxConcurrentConsumers = 0, // invalid: must be positive
            CreatedAt = default, // invalid: default DateTime
            LastUpdatedAt = DateTime.UtcNow.AddHours(1), // invalid: future date
            BackpressureEventTimestamps = new List<long> { 100, -200 }, // contains negative timestamp
            BufferMetrics = new Dictionary<string, long> { ["metric"] = -5 } // negative metric value
        };

        // Act
        var errors = context.Validate();

        // Assert: ensure each expected error message appears
        Assert.Contains("ContextId must be positive", errors);
        Assert.Contains("PipelineStageName exceeds maximum length of 100 characters", errors);
        Assert.Contains("BufferSize (150) cannot exceed MaxBufferCapacity (100)", errors);
        Assert.Contains("BackpressureStartTimeMs cannot be negative", errors);
        Assert.Contains("TotalBackpressureTimeMs cannot be negative", errors);
        Assert.Contains("DroppedItemCount cannot be negative", errors);
        Assert.Contains("ActiveConsumers cannot be negative", errors);
        Assert.Contains("MaxConcurrentConsumers must be positive", errors);
        Assert.Contains("CreatedAt cannot be the default DateTime value.", errors);
        Assert.Contains("LastUpdatedAt cannot be in the future", errors);
        Assert.Contains("BackpressureEventTimestamps contains negative timestamp", errors);
        Assert.Contains("BufferMetrics['metric'] cannot be negative", errors);
        Assert.Equal(12, errors.Count); // ensure no extra or missing errors
    }

    [Fact]
    public void EnsureValid_Invalid_ThrowsArgumentException_WithAllMessages()
    {
        // Arrange: reuse the invalid context from the previous test
        var context = new BackpressureContext
        {
            ContextId = 0,
            PipelineStageName = string.Empty,
            BufferSize = 10,
            MaxBufferCapacity = 5,
            IsBackpressured = false,
            BackpressureStartTimeMs = -1,
            TotalBackpressureTimeMs = -1,
            DroppedItemCount = -1,
            ActiveConsumers = -1,
            MaxConcurrentConsumers = -1,
            CreatedAt = default,
            LastUpdatedAt = DateTime.UtcNow.AddMinutes(10),
            BackpressureEventTimestamps = null,
            BufferMetrics = null
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() => context.EnsureValid());

        // Assert: the exception message should contain each validation problem
        var message = ex.Message;
        Assert.Contains("ContextId must be positive", message);
        Assert.Contains("PipelineStageName cannot be null or whitespace", message);
        Assert.Contains("BufferSize (10) cannot exceed MaxBufferCapacity (5)", message);
        Assert.Contains("BackpressureStartTimeMs cannot be negative", message);
        Assert.Contains("TotalBackpressureTimeMs cannot be negative", message);
        Assert.Contains("DroppedItemCount cannot be negative", message);
        Assert.Contains("ActiveConsumers cannot be negative", message);
        Assert.Contains("MaxConcurrentConsumers must be positive", message);
        Assert.Contains("CreatedAt cannot be the default DateTime value.", message);
        Assert.Contains("LastUpdatedAt cannot be in the future", message);
        Assert.Contains("BackpressureEventTimestamps collection cannot be null", message);
        Assert.Contains("BufferMetrics dictionary cannot be null", message);
    }
}
