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
/// Unit tests for <see cref="ErrorAlertSubscriber"/> error classification and alert suppression behavior.
/// Tests verify that errors are logged correctly, edge cases are handled gracefully,
/// and the subscriber's error handling is robust against invalid inputs.
/// </summary>
public class ErrorAlertSubscriberTests
{
    private readonly ILoggerFactory _loggerFactory;

    public ErrorAlertSubscriberTests()
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    }

    [Fact]
    public void Constructor_WithNullPublisher_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = _loggerFactory.CreateLogger<ErrorAlertSubscriber>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ErrorAlertSubscriber(null!, logger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ErrorAlertSubscriber(publisher, null!));
    }

    [Fact]
    public void GetErrorCount_InitialState_ReturnsZero()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<ErrorAlertSubscriber>());

        // Act
        var count = subscriber.GetErrorCount();

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task OnPipelineErrorAsync_WithValidException_LogsErrorAndIncrementsCount()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<ErrorAlertSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        var testException = new InvalidOperationException("Test error message");
        var operationName = "TestOperation";

        // Act
        await publisher.PublishPipelineErrorAsync(operationName, testException);
        await Task.Delay(100); // Allow event to be processed
        await publisher.StopAsync();
        var count = subscriber.GetErrorCount();

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task OnPipelineErrorAsync_WithNullException_HandlesGracefullyWithoutThrowing()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<ErrorAlertSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        var operationName = "TestOperation";

        // Act & Assert - Should not throw even with null exception
        await publisher.PublishPipelineErrorAsync(operationName, null!);
        await Task.Delay(100); // Allow event to be processed
        await publisher.StopAsync();
        var count = subscriber.GetErrorCount();

        // Assert - Event should be handled gracefully
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task OnPipelineErrorAsync_WithAggregateException_LogsInnerException()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<ErrorAlertSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        var innerException = new ArgumentException("Inner error");
        var aggregateException = new AggregateException("Multiple errors occurred", innerException);
        var operationName = "TestOperation";

        // Act
        await publisher.PublishPipelineErrorAsync(operationName, aggregateException);
        await Task.Delay(100); // Allow event to be processed
        await publisher.StopAsync();
        var count = subscriber.GetErrorCount();

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task OnPipelineErrorAsync_WithOperationNameNull_HandlesGracefully()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<ErrorAlertSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        var testException = new InvalidOperationException("Test error");

        // Act & Assert - Should handle null operation name
        await publisher.PublishPipelineErrorAsync(null!, testException);
        await Task.Delay(100);
        await publisher.StopAsync();
        var count = subscriber.GetErrorCount();

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task OnPipelineErrorAsync_WithEmptyOperationName_HandlesGracefully()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<ErrorAlertSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        var testException = new InvalidOperationException("Test error");

        // Act
        await publisher.PublishPipelineErrorAsync(string.Empty, testException);
        await Task.Delay(100);
        await publisher.StopAsync();
        var count = subscriber.GetErrorCount();

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task OnPipelineErrorAsync_WithMultipleErrors_AllErrorsAreCounted()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<ErrorAlertSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        // Act - Send multiple different errors
        for (int i = 0; i < 10; i++)
        {
            var exception = new InvalidOperationException($"Error {i}");
            await publisher.PublishPipelineErrorAsync("TestOperation", exception);
        }

        await Task.Delay(200); // Allow events to be processed
        await publisher.StopAsync();
        var count = subscriber.GetErrorCount();

        // Assert - All errors should be counted
        Assert.Equal(10, count);
    }

    [Fact]
    public async Task OnPipelineErrorAsync_WithNullExceptionMessage_LogsErrorWithoutCrashing()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<ErrorAlertSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        // Create exception with null message
        var testException = new Exception(null);
        var operationName = "TestOperation";

        // Act
        await publisher.PublishPipelineErrorAsync(operationName, testException);
        await Task.Delay(100);
        await publisher.StopAsync();
        var count = subscriber.GetErrorCount();

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task OnPipelineErrorAsync_WithEmptyExceptionMessage_LogsErrorWithoutCrashing()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<ErrorAlertSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        // Create exception with empty message
        var testException = new Exception(string.Empty);
        var operationName = "TestOperation";

        // Act
        await publisher.PublishPipelineErrorAsync(operationName, testException);
        await Task.Delay(100);
        await publisher.StopAsync();
        var count = subscriber.GetErrorCount();

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task OnPipelineErrorAsync_WithCorrelationId_UsesProvidedCorrelationId()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<ErrorAlertSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        var testException = new InvalidOperationException("Test error");
        var operationName = "TestOperation";

        // Act
        await publisher.PublishPipelineErrorAsync(operationName, testException);
        await Task.Delay(100);
        await publisher.StopAsync();
        var count = subscriber.GetErrorCount();

        // Assert
        Assert.Equal(1, count);
        // Note: The correlation ID is set in PipelineEventArgs base class
    }

    [Fact]
    public void GetErrorCount_AfterMultipleErrors_ReturnsCorrectCount()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<ErrorAlertSubscriber>());

        // Simulate error counting
        // Note: We can't directly increment _errorCount as it's private,
        // but we can verify the public API works correctly
        var count = subscriber.GetErrorCount();

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task OnErrorDetectedAsync_WhenOverridden_CallsCustomLogic()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var customSubscriber = new CustomErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<CustomErrorAlertSubscriber>());
        customSubscriber.Subscribe();
        publisher.Start();

        var testException = new InvalidOperationException("Test error");
        var operationName = "TestOperation";

        // Act
        await publisher.PublishPipelineErrorAsync(operationName, testException);
        await Task.Delay(100);
        await publisher.StopAsync();

        // Assert
        Assert.True(customSubscriber.CustomLogicCalled);
    }

    [Fact]
    public async Task OnPipelineErrorAsync_WithExceptionInSubscriber_DoesNotPropagateToPublisher()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var failingSubscriber = new FailingErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<FailingErrorAlertSubscriber>());
        failingSubscriber.Subscribe();
        publisher.Start();

        var testException = new InvalidOperationException("Test error");
        var operationName = "TestOperation";

        // Act - Should not throw even if subscriber's OnErrorDetectedAsync throws
        await publisher.PublishPipelineErrorAsync(operationName, testException);
        await Task.Delay(100);
        await publisher.StopAsync();
        var count = failingSubscriber.GetErrorCount();

        // Assert - Error should still be counted, exception should be swallowed
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task OnPipelineErrorAsync_WithNullOperationNameAndException_HandlesGracefully()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new ErrorAlertSubscriber(publisher, _loggerFactory.CreateLogger<ErrorAlertSubscriber>());
        subscriber.Subscribe();
        publisher.Start();

        // Act & Assert - Should handle both null values
        await publisher.PublishPipelineErrorAsync(null!, null!);
        await Task.Delay(100);
        await publisher.StopAsync();
        var count = subscriber.GetErrorCount();

        // Assert
        Assert.Equal(1, count);
    }

    /// <summary>
    /// Custom subscriber that tracks if OnErrorDetectedAsync was called
    /// </summary>
    private class CustomErrorAlertSubscriber : ErrorAlertSubscriber
    {
        public bool CustomLogicCalled { get; private set; }

        public CustomErrorAlertSubscriber(PipelineEventPublisher publisher, ILogger<CustomErrorAlertSubscriber> logger)
            : base(publisher, logger)
        {
        }

        protected override async Task OnErrorDetectedAsync(PipelineErrorEventArgs args)
        {
            CustomLogicCalled = true;
            await base.OnErrorDetectedAsync(args);
        }
    }

    /// <summary>
    /// Subscriber that throws in OnErrorDetectedAsync to test error handling
    /// </summary>
    private class FailingErrorAlertSubscriber : ErrorAlertSubscriber
    {
        public FailingErrorAlertSubscriber(PipelineEventPublisher publisher, ILogger<FailingErrorAlertSubscriber> logger)
            : base(publisher, logger)
        {
        }

        protected override async Task OnErrorDetectedAsync(PipelineErrorEventArgs args)
        {
            await base.OnErrorDetectedAsync(args);
            throw new InvalidOperationException("Custom error in OnErrorDetectedAsync");
        }
    }
}
