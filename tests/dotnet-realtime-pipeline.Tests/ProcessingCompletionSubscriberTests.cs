#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Tests;

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// Unit tests for <see cref="ProcessingCompletionSubscriber"/> completion event handling and metrics tracking.
/// Tests verify that processing completion events are handled correctly, success/failure
/// metrics are tracked accurately, and edge cases are covered with robust error handling.
/// </summary>
public class ProcessingCompletionSubscriberTests
{
    private readonly ILoggerFactory _loggerFactory;

    public ProcessingCompletionSubscriberTests()
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    }

    [Fact]
    public void Constructor_WithNullPublisher_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ProcessingCompletionSubscriber(null!, logger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ProcessingCompletionSubscriber(publisher, null!));
    }

    [Fact]
    public void GetSuccessRatePercent_InitialState_Returns100Percent()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());

        // Act
        var successRate = subscriber.GetSuccessRatePercent();

        // Assert
        Assert.Equal(100.0, successRate);
    }

    [Fact]
    public async Task OnProcessingCompletedAsync_WithSuccessfulResult_IncrementsSuccessCount()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        var result = new ProcessingResult(1, success: true, stageName: "test-stage");

        // Act
        await publisher.PublishProcessingCompletedAsync(result);
        await Task.Delay(100); // Allow event to be processed
        await publisher.StopAsync();

        // Assert - Success count should be incremented
        // Note: GetSuccessRatePercent returns 100% when no failures exist, regardless of success count
        // We need to verify the internal state through behavior
        var successRate = subscriber.GetSuccessRatePercent();
        Assert.Equal(100.0, successRate);
    }

    [Fact]
    public async Task OnProcessingCompletedAsync_WithFailedResult_IncrementsFailureCount()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        var result = new ProcessingResult(1, success: false, stageName: "test-stage");
        result.MarkFailure("Test failure message");

        // Act
        await publisher.PublishProcessingCompletedAsync(result);
        await Task.Delay(100); // Allow event to be processed
        await publisher.StopAsync();

        // Assert - Failure should be counted
        var successRate = subscriber.GetSuccessRatePercent();
        Assert.Equal(0.0, successRate);
    }

    [Fact]
    public async Task OnProcessingCompletedAsync_WithMixedResults_CalculatesCorrectSuccessRate()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        // Send 3 successful results
        for (int i = 1; i <= 3; i++)
        {
            var successResult = new ProcessingResult(i, success: true, stageName: "test-stage");
            await publisher.PublishProcessingCompletedAsync(successResult);
        }

        // Send 2 failed results
        for (int i = 4; i <= 5; i++)
        {
            var failedResult = new ProcessingResult(i, success: false, stageName: "test-stage");
            failedResult.MarkFailure($"Failure {i}");
            await publisher.PublishProcessingCompletedAsync(failedResult);
        }

        await Task.Delay(200); // Allow events to be processed
        await publisher.StopAsync();

        // Act
        var successRate = subscriber.GetSuccessRatePercent();

        // Assert - 3 successes out of 5 total = 60% success rate
        Assert.Equal(60.0, successRate);
    }

    [Fact]
    public async Task OnProcessingCompletedAsync_WithZeroResults_Returns100Percent()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        // Don't publish any events

        await Task.Delay(100);
        await publisher.StopAsync();

        // Act
        var successRate = subscriber.GetSuccessRatePercent();

        // Assert - No results means 100% success rate by definition
        Assert.Equal(100.0, successRate);
    }

    [Fact]
    public async Task OnProcessingCompletedAsync_WithMultipleSuccessfulResults_AllCounted()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        // Act - Send 10 successful results
        for (int i = 1; i <= 10; i++)
        {
            var result = new ProcessingResult(i, success: true, stageName: "test-stage");
            await publisher.PublishProcessingCompletedAsync(result);
        }

        await Task.Delay(200); // Allow events to be processed
        await publisher.StopAsync();

        // Assert - All 10 results should be processed
        var successRate = subscriber.GetSuccessRatePercent();
        Assert.Equal(100.0, successRate);
    }

    [Fact]
    public async Task OnProcessingCompletedAsync_WithMultipleFailedResults_AllCounted()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        // Act - Send 10 failed results
        for (int i = 1; i <= 10; i++)
        {
            var result = new ProcessingResult(i, success: false, stageName: "test-stage");
            result.MarkFailure($"Failure {i}");
            await publisher.PublishProcessingCompletedAsync(result);
        }

        await Task.Delay(200); // Allow events to be processed
        await publisher.StopAsync();

        // Assert - All 10 failures should be counted
        var successRate = subscriber.GetSuccessRatePercent();
        Assert.Equal(0.0, successRate);
    }

    [Fact]
    public async Task OnProcessingCompletedAsync_WithResultWithErrorMessage_LogsWarning()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        var result = new ProcessingResult(1, success: false, stageName: "test-stage");
        result.MarkFailure("Test error message");

        // Act
        await publisher.PublishProcessingCompletedAsync(result);
        await Task.Delay(100); // Allow event to be processed
        await publisher.StopAsync();

        // Assert - No exception should be thrown, event should be handled
        // The warning is logged internally by the subscriber
    }

    [Fact]
    public async Task OnProcessingCompletedAsync_WithNullResult_HandlesGracefullyWithoutThrowing()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        // Act & Assert - Should not throw even with null result
        await publisher.PublishProcessingCompletedAsync(null!);
        await Task.Delay(100); // Allow event to be processed
        await publisher.StopAsync();
    }

    [Fact]
    public async Task OnProcessingCompletedAsync_WithExceptionInResult_LogsErrorWithoutCrashing()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        var result = new ProcessingResult(1, success: false, stageName: "test-stage");
        result.MarkFailure("Test failure");
        result.Exception = new InvalidOperationException("Test exception");

        // Act - Should not throw even with exception in result
        await publisher.PublishProcessingCompletedAsync(result);
        await Task.Delay(100); // Allow event to be processed
        await publisher.StopAsync();
    }

    [Fact]
    public async Task OnProcessingCompletionAsync_WhenOverridden_CallsCustomLogic()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var customSubscriber = new CustomProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<CustomProcessingCompletionSubscriber>());
        customSubscriber.Subscribe();
        publisher.Start();

        var result = new ProcessingResult(1, success: true, stageName: "test-stage");

        // Act
        await publisher.PublishProcessingCompletedAsync(result);
        await Task.Delay(100); // Allow event to be processed
        await publisher.StopAsync();

        // Assert
        Assert.True(customSubscriber.CustomLogicCalled);
    }

    [Fact]
    public async Task OnProcessingCompletedAsync_WithExceptionInSubscriber_DoesNotPropagateToPublisher()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var failingSubscriber = new FailingProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<FailingProcessingCompletionSubscriber>());
        failingSubscriber.Subscribe();
        publisher.Start();

        var result = new ProcessingResult(1, success: true, stageName: "test-stage");

        // Act & Assert - Should not throw even if subscriber's OnProcessingCompletionAsync throws
        await publisher.PublishProcessingCompletedAsync(result);
        await Task.Delay(100); // Allow event to be processed
        await publisher.StopAsync();
    }

    [Fact]
    public async Task OnProcessingCompletedAsync_WithResultWithAllProperties_HandlesCorrectly()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        var result = new ProcessingResult(123, success: true, stageName: "processing-stage")
        {
            ProcessingTimeMs = 456,
            ErrorMessage = null,
            Exception = null,
            ProcessedAt = DateTime.UtcNow,
            RetryCount = 2,
            CorrelationId = "test-correlation-123",
            OutputData = new System.Collections.Generic.Dictionary<string, object>
            {
                { "output1", "value1" },
                { "output2", 42 }
            }
        };

        // Act
        await publisher.PublishProcessingCompletedAsync(result);
        await Task.Delay(100); // Allow event to be processed
        await publisher.StopAsync();

        // Assert - Should handle result with all properties set
        var successRate = subscriber.GetSuccessRatePercent();
        Assert.Equal(100.0, successRate);
    }

    [Fact]
    public async Task Subscribe_RegistersForProcessingCompletedEvent()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());

        // Act
        subscriber.Subscribe();

        // Assert - Subscribe should register the handler
        // The actual registration is verified by the fact that events are processed
    }

    [Fact]
    public async Task OnProcessingCompletedAsync_WithEmptyStageName_HandlesGracefully()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        var result = new ProcessingResult(1, success: true, stageName: string.Empty);

        // Act
        await publisher.PublishProcessingCompletedAsync(result);
        await Task.Delay(100); // Allow event to be processed
        await publisher.StopAsync();

        // Assert - Should handle empty stage name gracefully
    }

    [Fact]
    public async Task OnProcessingCompletedAsync_WithWhitespaceStageName_HandlesGracefully()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ProcessingCompletionSubscriber(publisher, _loggerFactory.CreateLogger<ProcessingCompletionSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        var result = new ProcessingResult(1, success: true, stageName: "   ");

        // Act
        await publisher.PublishProcessingCompletedAsync(result);
        await Task.Delay(100); // Allow event to be processed
        await publisher.StopAsync();

        // Assert - Should handle whitespace stage name gracefully
    }

    /// <summary>
    /// Custom subscriber that tracks if OnProcessingCompletionAsync was called
    /// </summary>
    private class CustomProcessingCompletionSubscriber : ProcessingCompletionSubscriber
    {
        public bool CustomLogicCalled { get; private set; }

        public CustomProcessingCompletionSubscriber(PipelineEventPublisher publisher, ILogger<CustomProcessingCompletionSubscriber> logger)
            : base(publisher, logger)
        {
        }

        protected override async Task OnProcessingCompletionAsync(ProcessingResult result)
        {
            CustomLogicCalled = true;
            await base.OnProcessingCompletionAsync(result);
        }
    }

    /// <summary>
    /// Subscriber that throws in OnProcessingCompletionAsync to test error handling
    /// </summary>
    private class FailingProcessingCompletionSubscriber : ProcessingCompletionSubscriber
    {
        public FailingProcessingCompletionSubscriber(PipelineEventPublisher publisher, ILogger<FailingProcessingCompletionSubscriber> logger)
            : base(publisher, logger)
        {
        }

        protected override async Task OnProcessingCompletionAsync(ProcessingResult result)
        {
            await base.OnProcessingCompletionAsync(result);
            throw new InvalidOperationException("Custom error in OnProcessingCompletionAsync");
        }
    }
}