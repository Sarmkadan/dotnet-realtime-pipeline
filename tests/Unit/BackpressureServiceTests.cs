#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Contains unit tests for <see cref="BackpressureService"/>.
/// </summary>
public sealed class BackpressureServiceTests
{
    private readonly BackpressureService _service = new();

    /// <summary>
    /// Verifies that <see cref="BackpressureService.CreateContext(string, int)"/> creates a
    /// context with the specified stage name and maximum capacity.
    /// </summary>
    [Fact]
    public void CreateContext_WithValidParameters_ShouldSucceed()
    {
        // Act
        var context = _service.CreateContext("TestStage", 1000);

        // Assert
        Assert.NotNull(context);
        Assert.Equal("TestStage", context.StageName);
        Assert.Equal(1000, context.MaxCapacity);
    }

    /// <summary>
    /// Ensures that adding items to a buffer that is below its capacity returns <c>true</c>.
    /// </summary>
    [Fact]
    public void TryAddToBuffer_WhenBelowCapacity_ShouldReturnTrue()
    {
        // Arrange
        _service.CreateContext("TestStage", 1000);

        // Act
        var result = _service.TryAddToBuffer("TestStage", 500);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Ensures that adding items to a buffer that would exceed its capacity returns <c>false</c>.
    /// </summary>
    [Fact]
    public void TryAddToBuffer_WhenExceedsCapacity_ShouldReturnFalse()
    {
        // Arrange
        _service.CreateContext("TestStage", 100);
        _service.TryAddToBuffer("TestStage", 100);

        // Act
        var result = _service.TryAddToBuffer("TestStage", 50);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that <see cref="BackpressureService.GetBufferStatus"/> returns the current
    /// item counts for each stage that has a context.
    /// </summary>
    [Fact]
    public void GetBufferStatus_ShouldReturnCurrentLevels()
    {
        // Arrange
        _service.CreateContext("Stage1", 1000);
        _service.CreateContext("Stage2", 500);
        _service.TryAddToBuffer("Stage1", 250);
        _service.TryAddToBuffer("Stage2", 100);

        // Act
        var status = _service.GetBufferStatus();

        // Assert
        Assert.Equal(2, status.Count);
        Assert.Equal(250, status["Stage1"]);
        Assert.Equal(100, status["Stage2"]);
    }

    /// <summary>
    /// Tests that <see cref="BackpressureService.ApplyBackpressureAsync(string, BackpressureStrategy, int)"/>
    /// with the <see cref="BackpressureStrategy.Block"/> strategy waits (does not throw) when the buffer
    /// is full and a timeout is provided.
    /// </summary>
    /// <returns>A task that completes when the back‑pressure operation finishes.</returns>
    [Fact]
    public async Task ApplyBackpressureAsync_WithBlockStrategy_ShouldWait()
    {
        // Arrange
        _service.CreateContext("TestStage", 100);
        _service.TryAddToBuffer("TestStage", 100);

        // Act
        var response = await _service.ApplyBackpressureAsync(
            "TestStage",
            BackpressureStrategy.Block,
            timeoutMs: 1000
        );

        // Assert
        Assert.NotNull(response);
    }

    /// <summary>
    /// Tests that <see cref="BackpressureService.ApplyBackpressureAsync(string, BackpressureStrategy, int)"/>
    /// with the <see cref="BackpressureStrategy.Throttle"/> strategy succeeds and returns a response
    /// when the buffer is not full.
    /// </summary>
    /// <returns>A task that completes when the back‑pressure operation finishes.</returns>
    [Fact]
    public async Task ApplyBackpressureAsync_WithThrottleStrategy_ShouldSucceed()
    {
        // Arrange
        _service.CreateContext("TestStage", 1000);

        // Act
        var response = await _service.ApplyBackpressureAsync(
            "TestStage",
            BackpressureStrategy.Throttle,
            timeoutMs: 500
        );

        // Assert
        Assert.NotNull(response);
    }

    /// <summary>
    /// Verifies that <see cref="BackpressureService.RemoveFromBuffer(string, int)"/> decreases the
    /// buffered item count for the specified stage.
    /// </summary>
    [Fact]
    public void RemoveFromBuffer_ShouldDecreaseCount()
    {
        // Arrange
        _service.CreateContext("TestStage", 1000);
        _service.TryAddToBuffer("TestStage", 500);

        // Act
        _service.RemoveFromBuffer("TestStage", 200);

        // Assert
        var status = _service.GetBufferStatus();
        Assert.Equal(300, status["TestStage"]);
    }
}
