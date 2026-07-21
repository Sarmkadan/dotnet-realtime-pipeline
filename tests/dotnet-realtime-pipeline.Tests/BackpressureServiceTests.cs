using System;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Exceptions;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

public class BackpressureServiceTests
{
    private readonly BackpressureService _service;

    public BackpressureServiceTests()
    {
        _service = new BackpressureService();
    }

    [Fact]
    public void CreateContext_WithValidParameters_CreatesContextSuccessfully()
    {
        // Arrange
        var stageName = "test-stage";
        var maxCapacity = 1000;

        // Act
        var context = _service.CreateContext(stageName, maxCapacity);

        // Assert
        context.Should().NotBeNull();
        context.PipelineStageName.Should().Be(stageName);
        context.MaxBufferCapacity.Should().Be(maxCapacity);
        context.BufferSize.Should().Be(0);
        context.IsBackpressured.Should().BeFalse();
        context.ContextId.Should().BeGreaterThan(0);
        context.MaxConcurrentConsumers.Should().Be(4);
    }

    [Fact]
    public void CreateContext_WithNullStageName_ThrowsArgumentException()
    {
        // Arrange
        string stageName = null!;
        var maxCapacity = 1000;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.CreateContext(stageName, maxCapacity));
    }

    [Fact]
    public void CreateContext_WithEmptyStageName_ThrowsArgumentException()
    {
        // Arrange
        var stageName = "   ";
        var maxCapacity = 1000;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.CreateContext(stageName, maxCapacity));
    }

    [Fact]
    public void CreateContext_WithNonPositiveCapacity_ThrowsArgumentException()
    {
        // Arrange
        var stageName = "test-stage";
        var maxCapacity = 0;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.CreateContext(stageName, maxCapacity));
    }

    [Fact]
    public void CreateContext_WithDuplicateStageName_ThrowsInvalidOperationException()
    {
        // Arrange
        var stageName = "duplicate-stage";
        var maxCapacity = 1000;

        // Act - create first context
        _service.CreateContext(stageName, maxCapacity);

        // Assert - second creation should throw
        Assert.Throws<InvalidOperationException>(() => _service.CreateContext(stageName, maxCapacity));
    }

    [Fact]
    public void GetContext_WithExistingStage_ReturnsContext()
    {
        // Arrange
        var stageName = "existing-stage";
        var maxCapacity = 1000;
        var createdContext = _service.CreateContext(stageName, maxCapacity);

        // Act
        var retrievedContext = _service.GetContext(stageName);

        // Assert
        retrievedContext.Should().NotBeNull();
        retrievedContext.Should().BeSameAs(createdContext);
    }

    [Fact]
    public void GetContext_WithNonExistingStage_ReturnsNull()
    {
        // Arrange
        var stageName = "non-existing-stage";

        // Act
        var context = _service.GetContext(stageName);

        // Assert
        context.Should().BeNull();
    }

    [Fact]
    public void GetContext_WithNullStageName_ThrowsArgumentNullException()
    {
        // Arrange
        string stageName = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.GetContext(stageName));
    }

    [Fact]
    public void TryAddToBuffer_WithValidParameters_AddsItemsSuccessfully()
    {
        // Arrange
        var stageName = "buffer-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Act - add items within capacity
        var result = _service.TryAddToBuffer(stageName, 50);

        // Assert
        result.Should().BeTrue();
        context.BufferSize.Should().Be(50);
        context.IsBackpressured.Should().BeFalse();
    }

    [Fact]
    public void TryAddToBuffer_WhenBufferFull_ReturnsFalseAndActivatesBackpressure()
    {
        // Arrange
        var stageName = "full-buffer-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Fill the buffer
        var fillResult = _service.TryAddToBuffer(stageName, 100);
        fillResult.Should().BeTrue();

        // Verify context is backpressured after trying to exceed capacity
        var retrievedContext = _service.GetContext(stageName);
        retrievedContext.Should().NotBeNull();
        retrievedContext.IsBackpressured.Should().BeFalse();

        // Act - try to add more items (should fail and activate backpressure)
        var result = _service.TryAddToBuffer(stageName, 1);

        // Assert
        result.Should().BeFalse();
        retrievedContext = _service.GetContext(stageName);
        retrievedContext.Should().NotBeNull();
        retrievedContext.BufferSize.Should().Be(100);
        retrievedContext.IsBackpressured.Should().BeTrue();
        retrievedContext.DroppedItemCount.Should().Be(1);
    }

    [Fact]
    public void TryAddToBuffer_WithNullStageName_ThrowsArgumentException()
    {
        // Arrange
        string stageName = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.TryAddToBuffer(stageName, 10));
    }

    [Fact]
    public void TryAddToBuffer_WithNonExistingStage_ThrowsResourceNotFoundException()
    {
        // Arrange
        var stageName = "non-existing";

        // Act & Assert
        Assert.Throws<ResourceNotFoundException>(() => _service.TryAddToBuffer(stageName, 10));
    }

    [Fact]
    public void RemoveFromBuffer_WithValidParameters_RemovesItemsSuccessfully()
    {
        // Arrange
        var stageName = "remove-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Fill buffer
        _service.TryAddToBuffer(stageName, 80);

        // Act
        _service.RemoveFromBuffer(stageName, 30);

        // Assert
        context.BufferSize.Should().Be(50);
    }

    [Fact]
    public void RemoveFromBuffer_WhenBufferDropsBelowThreshold_DeactivatesBackpressure()
    {
        // Arrange
        var stageName = "recovery-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Fill buffer to 85% to trigger backpressure
        _service.TryAddToBuffer(stageName, 85);
        var retrievedContext = _service.GetContext(stageName);
        retrievedContext.IsBackpressured.Should().BeTrue();
        retrievedContext.BufferSize.Should().Be(85);

        // Remove items to drop below 60% threshold
        _service.RemoveFromBuffer(stageName, 30);
        retrievedContext = _service.GetContext(stageName);

        // Assert - backpressure should still be active (above 60%)
        retrievedContext.IsBackpressured.Should().BeTrue();
        retrievedContext.BufferSize.Should().Be(55);

        // Remove more items to drop below 60%
        _service.RemoveFromBuffer(stageName, 5);
        retrievedContext = _service.GetContext(stageName);

        // Assert - backpressure should now be deactivated (hysteresis)
        retrievedContext.IsBackpressured.Should().BeFalse();
        retrievedContext.BufferSize.Should().Be(50);
    }

    [Fact]
    public void RemoveFromBuffer_WithNullStageName_ThrowsArgumentException()
    {
        // Arrange
        string stageName = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.RemoveFromBuffer(stageName, 10));
    }

    [Fact]
    public void RemoveFromBuffer_WithNonExistingStage_ThrowsResourceNotFoundException()
    {
        // Arrange
        var stageName = "non-existing";

        // Act & Assert
        Assert.Throws<ResourceNotFoundException>(() => _service.RemoveFromBuffer(stageName, 10));
    }

    [Fact]
    public async Task ApplyBackpressureAsync_WithThrottleStrategy_AppliesDelay()
    {
        // Arrange
        var stageName = "throttle-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Fill buffer to trigger backpressure
        _service.TryAddToBuffer(stageName, 100);

        // Act
        var startTime = DateTime.UtcNow;
        var response = await _service.ApplyBackpressureAsync(stageName, BackpressureStrategy.Throttle, 500);
        var elapsedTime = DateTime.UtcNow - startTime;

        // Assert
        response.Should().NotBeNull();
        response.Applied.Should().BeTrue();
        response.StrategyUsed.Should().Be(BackpressureStrategy.Throttle.ToString());
        response.Reason.Should().NotBeNullOrEmpty();
        elapsedTime.TotalMilliseconds.Should().BeGreaterThan(100); // Should have some delay
        context.IsBackpressured.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyBackpressureAsync_WithBlockStrategy_AppliesDelay()
    {
        // Arrange
        var stageName = "block-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Fill buffer to trigger backpressure
        _service.TryAddToBuffer(stageName, 100);

        // Act
        var startTime = DateTime.UtcNow;
        var response = await _service.ApplyBackpressureAsync(stageName, BackpressureStrategy.Block, 1000);
        var elapsedTime = DateTime.UtcNow - startTime;

        // Assert
        response.Should().NotBeNull();
        response.Applied.Should().BeTrue();
        response.StrategyUsed.Should().Be(BackpressureStrategy.Block.ToString());
        elapsedTime.TotalMilliseconds.Should().BeGreaterThan(100); // Should have some delay
    }

    [Fact]
    public async Task ApplyBackpressureAsync_WhenContextRemovedDuringDelay_ReturnsAppropriateResponse()
    {
        // Arrange
        var stageName = "removed-during-delay-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Fill buffer to trigger backpressure
        _service.TryAddToBuffer(stageName, 100);

        // Act - remove context during delay
        var task = _service.ApplyBackpressureAsync(stageName, BackpressureStrategy.Throttle, 500);
        _service.Clear(); // This will remove the context during the delay
        var response = await task;

        // Assert
        response.Should().NotBeNull();
        response.Applied.Should().BeFalse();
        response.Reason.Should().Contain("removed during processing");
    }

    [Fact]
    public void IsBackpressured_WithBackpressuredStage_ReturnsTrue()
    {
        // Arrange
        var stageName = "backpressured-stage";
        var maxCapacity = 100;
        _service.CreateContext(stageName, maxCapacity);

        // Fill buffer to trigger backpressure
        _service.TryAddToBuffer(stageName, 100);

        // Try to exceed capacity to trigger backpressure activation
        _service.TryAddToBuffer(stageName, 1);

        // Act
        var isBackpressured = _service.IsBackpressured(stageName);

        // Assert
        isBackpressured.Should().BeTrue();
    }

    [Fact]
    public void IsBackpressured_WithNonBackpressuredStage_ReturnsFalse()
    {
        // Arrange
        var stageName = "normal-stage";
        var maxCapacity = 100;
        _service.CreateContext(stageName, maxCapacity);

        // Act
        var isBackpressured = _service.IsBackpressured(stageName);

        // Assert
        isBackpressured.Should().BeFalse();
    }

    [Fact]
    public void IsBackpressured_WithNonExistingStage_ReturnsFalse()
    {
        // Arrange
        var stageName = "non-existing";

        // Act
        var isBackpressured = _service.IsBackpressured(stageName);

        // Assert
        isBackpressured.Should().BeFalse();
    }

    [Fact]
    public void TryRegisterConsumer_WithAvailableSlots_ReturnsTrue()
    {
        // Arrange
        var stageName = "consumer-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Act
        var result = _service.TryRegisterConsumer(stageName);

        // Assert
        result.Should().BeTrue();
        context.ActiveConsumers.Should().Be(1);
    }

    [Fact]
    public void TryRegisterConsumer_WhenMaxConsumersReached_ReturnsFalse()
    {
        // Arrange
        var stageName = "max-consumers-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Fill all consumer slots
        for (int i = 0; i < context.MaxConcurrentConsumers; i++)
        {
            _service.TryRegisterConsumer(stageName);
        }

        // Act - try to register one more
        var result = _service.TryRegisterConsumer(stageName);

        // Assert
        result.Should().BeFalse();
        context.ActiveConsumers.Should().Be(context.MaxConcurrentConsumers);
    }

    [Fact]
    public void TryRegisterConsumer_WithNullStageName_ThrowsArgumentException()
    {
        // Arrange
        string stageName = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.TryRegisterConsumer(stageName));
    }

    [Fact]
    public void TryRegisterConsumer_WithNonExistingStage_ThrowsResourceNotFoundException()
    {
        // Arrange
        var stageName = "non-existing";

        // Act & Assert
        Assert.Throws<ResourceNotFoundException>(() => _service.TryRegisterConsumer(stageName));
    }

    [Fact]
    public void UnregisterConsumer_WithValidParameters_DecrementsConsumerCount()
    {
        // Arrange
        var stageName = "unregister-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Register a consumer
        _service.TryRegisterConsumer(stageName);
        context.ActiveConsumers.Should().Be(1);

        // Act
        _service.UnregisterConsumer(stageName);

        // Assert
        context.ActiveConsumers.Should().Be(0);
    }

    [Fact]
    public void UnregisterConsumer_WithNullStageName_ThrowsArgumentException()
    {
        // Arrange
        string stageName = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.UnregisterConsumer(stageName));
    }

    [Fact]
    public void UnregisterConsumer_WithNonExistingStage_DoesNotThrow()
    {
        // Arrange
        var stageName = "non-existing";

        // Act - should not throw
        _service.UnregisterConsumer(stageName);

        // Assert - no exception thrown
    }

    [Fact]
    public void GetSystemStatus_WithMultipleStages_ReturnsAggregatedStatus()
    {
        // Arrange
        var stage1 = "stage-1";
        var stage2 = "stage-2";
        var stage3 = "stage-3";

        var context1 = _service.CreateContext(stage1, 100);
        var context2 = _service.CreateContext(stage2, 200);
        var context3 = _service.CreateContext(stage3, 300);

        // Fill buffers - stage2 to exceed capacity to trigger backpressure
        _service.TryAddToBuffer(stage1, 50);
        _service.TryAddToBuffer(stage2, 200);
        _service.TryAddToBuffer(stage2, 1); // This will exceed capacity and trigger backpressure
        _service.TryAddToBuffer(stage3, 100);

        // Act
        var status = _service.GetSystemStatus();

        // Assert
        status.Should().NotBeNull();
        status.TotalStages.Should().Be(3);
        status.BackpressuredStages.Should().Be(1); // Only stage2 is backpressured
        status.AverageBufferFillPercent.Should().BeApproximately(50.1, 0.1); // (50 + 100 + 33.33) / 3
        status.TotalStages.Should().Be(3);
        status.IsSystemBackpressured.Should().BeFalse(); // Average is 50.1%, not above high water mark
    }

    [Fact]
    public void GetSystemStatus_WithBackpressuredStages_ReturnsCorrectStatus()
    {
        // Arrange
        var stage1 = "backpressured-1";
        var stage2 = "backpressured-2";

        var context1 = _service.CreateContext(stage1, 100);
        var context2 = _service.CreateContext(stage2, 100);

        // Fill buffers to exceed capacity to trigger backpressure
        _service.TryAddToBuffer(stage1, 100);
        _service.TryAddToBuffer(stage1, 1); // Exceeds capacity
        _service.TryAddToBuffer(stage2, 100);
        _service.TryAddToBuffer(stage2, 1); // Exceeds capacity

        // Act
        var status = _service.GetSystemStatus();

        // Assert
        status.Should().NotBeNull();
        status.TotalStages.Should().Be(2);
        status.BackpressuredStages.Should().Be(2);
        status.IsSystemBackpressured.Should().BeTrue();
    }

    [Fact]
    public void ResetBackpressure_WithValidStage_DeactivatesBackpressureAndClearsBuffer()
    {
        // Arrange
        var stageName = "reset-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Fill buffer to trigger backpressure (85% threshold)
        _service.TryAddToBuffer(stageName, 85);
        var retrievedContext = _service.GetContext(stageName);
        retrievedContext.IsBackpressured.Should().BeTrue();
        retrievedContext.BufferSize.Should().Be(85);

        // Act
        _service.ResetBackpressure(stageName);

        // Assert
        retrievedContext = _service.GetContext(stageName);
        retrievedContext.IsBackpressured.Should().BeFalse();
        retrievedContext.BufferSize.Should().Be(0);
        retrievedContext.TotalBackpressureTimeMs.Should().BeGreaterThan(0); // Should have accumulated time
    }

    [Fact]
    public void ResetBackpressure_WithNullStageName_ThrowsArgumentNullException()
    {
        // Arrange
        string stageName = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.ResetBackpressure(stageName));
    }

    [Fact]
    public void GetDroppedItemCount_WithStageHavingDroppedItems_ReturnsCount()
    {
        // Arrange
        var stageName = "dropped-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Fill buffer and trigger dropped items
        _service.TryAddToBuffer(stageName, 100);
        _service.TryAddToBuffer(stageName, 10); // This should be dropped

        // Act
        var droppedCount = _service.GetDroppedItemCount(stageName);

        // Assert
        droppedCount.Should().Be(10);
    }

    [Fact]
    public void GetDroppedItemCount_WithStageHavingNoDroppedItems_ReturnsZero()
    {
        // Arrange
        var stageName = "no-drop-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Add items within capacity
        _service.TryAddToBuffer(stageName, 50);

        // Act
        var droppedCount = _service.GetDroppedItemCount(stageName);

        // Assert
        droppedCount.Should().Be(0);
    }

    [Fact]
    public void GetDroppedItemCount_WithNonExistingStage_ReturnsZero()
    {
        // Arrange
        var stageName = "non-existing";

        // Act
        var droppedCount = _service.GetDroppedItemCount(stageName);

        // Assert
        droppedCount.Should().Be(0);
    }

    [Fact]
    public void GetBufferStatus_ReturnsBufferLevelsForAllStages()
    {
        // Arrange
        var stage1 = "buffer-stage-1";
        var stage2 = "buffer-stage-2";

        var context1 = _service.CreateContext(stage1, 100);
        var context2 = _service.CreateContext(stage2, 200);

        _service.TryAddToBuffer(stage1, 30);
        _service.TryAddToBuffer(stage2, 150);

        // Act
        var bufferStatus = _service.GetBufferStatus();

        // Assert
        bufferStatus.Should().NotBeNull();
        bufferStatus.Should().ContainKey(stage1);
        bufferStatus.Should().ContainKey(stage2);
        bufferStatus[stage1].Should().Be(30);
        bufferStatus[stage2].Should().Be(150);
    }

    [Fact]
    public void Clear_RemovesAllContexts()
    {
        // Arrange
        var stage1 = "clear-stage-1";
        var stage2 = "clear-stage-2";

        _service.CreateContext(stage1, 100);
        _service.CreateContext(stage2, 200);

        // Verify contexts exist
        _service.GetContext(stage1).Should().NotBeNull();
        _service.GetContext(stage2).Should().NotBeNull();

        // Act
        _service.Clear();

        // Assert
        _service.GetContext(stage1).Should().BeNull();
        _service.GetContext(stage2).Should().BeNull();
    }

    [Fact]
    public void BufferFillPercentage_CalculatesCorrectly()
    {
        // Arrange
        var stageName = "percentage-stage";
        var maxCapacity = 200;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Add 50 items (25%)
        _service.TryAddToBuffer(stageName, 50);

        // Act
        var fillPercent = context.GetBufferFillPercentage();

        // Assert
        fillPercent.Should().Be(25.0);
    }

    [Fact]
    public void ShouldApplyBackpressure_WithThresholdMet_ReturnsTrue()
    {
        // Arrange
        var stageName = "threshold-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Fill to 80% (default threshold)
        _service.TryAddToBuffer(stageName, 80);

        // Act
        var shouldApply = context.ShouldApplyBackpressure();

        // Assert
        shouldApply.Should().BeTrue();
    }

    [Fact]
    public void ShouldApplyBackpressure_WithThresholdNotMet_ReturnsFalse()
    {
        // Arrange
        var stageName = "below-threshold-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Fill to 70% (below 80% threshold)
        _service.TryAddToBuffer(stageName, 70);

        // Act
        var shouldApply = context.ShouldApplyBackpressure();

        // Assert
        shouldApply.Should().BeFalse();
    }

    [Fact]
    public void Hysteresis_WhenDroppingBelow60Percent_DeactivatesBackpressure()
    {
        // Arrange
        var stageName = "hysteresis-stage";
        var maxCapacity = 100;
        var context = _service.CreateContext(stageName, maxCapacity);

        // Fill to 85% to trigger backpressure
        _service.TryAddToBuffer(stageName, 85);
        var retrievedContext = _service.GetContext(stageName);
        retrievedContext.IsBackpressured.Should().BeTrue();

        // Remove items to 65% (still above 60% threshold)
        _service.RemoveFromBuffer(stageName, 20);
        retrievedContext = _service.GetContext(stageName);
        retrievedContext.IsBackpressured.Should().BeTrue(); // Still backpressured

        // Remove more items to drop below 60%
        _service.RemoveFromBuffer(stageName, 5);
        retrievedContext = _service.GetContext(stageName);
        retrievedContext.IsBackpressured.Should().BeFalse(); // Hysteresis kicks in
    }
}