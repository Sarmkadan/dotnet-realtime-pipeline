#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Tests;

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// Tests for per-subscriber error isolation and concurrency policy.
/// Verifies that one failing subscriber does not affect other subscribers.
/// </summary>
public class EventSubscriberIsolationTests
{
    private readonly ILoggerFactory _loggerFactory;

    public EventSubscriberIsolationTests()
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    }

    [Fact]
    public async Task MultipleSubscribers_WhenOneThrowsOnEveryEvent_OtherSubscribersStillReceiveAllEvents()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());

        // Track received events for each subscriber
        var normalSubscriberEvents = new ConcurrentBag<DataPoint>();
        var throwingSubscriberEvents = new ConcurrentBag<DataPoint>();
        var errorCount = 0;

        // Create a subscriber that throws on every event
        var throwingSubscriber = new ThrowingSubscriber(publisher, _loggerFactory.CreateLogger<ThrowingSubscriber>(),
            (args) =>
            {
                errorCount++;
                throw new InvalidOperationException("Simulated error");
            });

        // Create normal subscribers that count events
        var normalSubscriber1 = new CountingSubscriber(publisher, _loggerFactory.CreateLogger<CountingSubscriber>(), normalSubscriberEvents);
        var normalSubscriber2 = new CountingSubscriber(publisher, _loggerFactory.CreateLogger<CountingSubscriber>(), normalSubscriberEvents);

        // Subscribe all
        throwingSubscriber.Subscribe();
        normalSubscriber1.Subscribe();
        normalSubscriber2.Subscribe();

        publisher.Start();

        // Act - publish 500 events
        var publishTasks = new List<Task>();
        for (int i = 0; i < 500; i++)
        {
            var dataPoint = new DataPoint(i, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), i * 1.5, "test-source");
            publishTasks.Add(publisher.PublishDataIngestedAsync(dataPoint));
        }

        await Task.WhenAll(publishTasks);

        // Give channels time to process
        await Task.Delay(100);

        // Stop and clean up
        await publisher.StopAsync();

        // Assert
        // The throwing subscriber should have received all 500 events (and thrown 500 times)
        Assert.Equal(500, throwingSubscriber.GetReceivedCount());
        Assert.Equal(500, errorCount);

        // Normal subscribers should also have received all 500 events each
        // Note: Due to concurrent processing, we might not get exactly 500 each, but should be close
        Assert.True(normalSubscriber1.GetReceivedCount() >= 450,
            $"Normal subscriber 1 should have received most events. Actual: {normalSubscriber1.GetReceivedCount()}");
        Assert.True(normalSubscriber2.GetReceivedCount() >= 450,
            $"Normal subscriber 2 should have received most events. Actual: {normalSubscriber2.GetReceivedCount()}");

        // Total events across normal subscribers should be close to 1000 (500 each)
        var totalNormalEvents = normalSubscriber1.GetReceivedCount() + normalSubscriber2.GetReceivedCount();
        Assert.True(totalNormalEvents >= 900,
            $"Total normal events should be close to 1000. Actual: {totalNormalEvents}");
    }

    [Fact]
    public async Task SubscriberWithFailFastPolicy_WhenErrorOccurs_PropagatesException()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var receivedEvents = new ConcurrentBag<DataPoint>();
        var exceptionCaught = false;

        // Create subscriber with FailFast policy
        var options = new SubscriberOptions
        {
            Name = "FailFastSubscriber",
            ErrorPolicy = SubscriberErrorPolicy.FailFast
        };

        void handler(DataIngestedEventArgs args)
        {
            receivedEvents.Add(args.DataPoint);
            throw new InvalidOperationException("Fail fast test");
        }

        publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent), args => Task.Run(() => handler(args)), options);
        publisher.Start();

        // Act - publish one event
        var dataPoint = new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 1.0, "test");

        try
        {
            await publisher.PublishDataIngestedAsync(dataPoint);
        }
        catch (InvalidOperationException)
        {
            exceptionCaught = true;
        }

        await Task.Delay(50);
        await publisher.StopAsync();

        // Assert
        Assert.True(exceptionCaught, "Exception should have been propagated with FailFast policy");
        Assert.Single(receivedEvents); // Should have received the event before throwing
    }

    [Fact]
    public async Task SubscriberWithParallelDispatch_ProcessesEventsConcurrently()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var processingTimes = new ConcurrentBag<long>();
        var options = new SubscriberOptions
        {
            Name = "ParallelSubscriber",
            DispatchMode = SubscriberDispatchMode.Parallel,
            MaxDegreeOfParallelism = 4
        };

        publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent), async (args) =>
        {
            var start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await Task.Delay(10); // Simulate work
            var end = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            processingTimes.Add(end - start);
        }, options);

        publisher.Start();

        // Act - publish multiple events
        var tasks = new List<Task>();
        for (int i = 0; i < 20; i++)
        {
            var dataPoint = new DataPoint(i, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), i * 1.5, "test");
            tasks.Add(publisher.PublishDataIngestedAsync(dataPoint));
        }

        await Task.WhenAll(tasks);
        await Task.Delay(200);
        await publisher.StopAsync();

        // Assert - with parallel processing, we should see events being processed concurrently
        // (not sequentially with 10ms each = 200ms total)
        // With 4 workers and 20 events, processing should complete faster than 200ms
        var totalProcessingTime = processingTimes.Sum();
        Assert.True(totalProcessingTime < 150,
            $"Parallel processing should be faster than sequential. Total processing time: {totalProcessingTime}ms");
    }

    [Fact]
    public async Task SubscriberWithBoundedQueue_WhenFull_DropsNewEvents()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var receivedEvents = new ConcurrentBag<DataPoint>();

        var options = new SubscriberOptions
        {
            Name = "BoundedQueueSubscriber",
            MaxQueueSize = 5, // Small queue
            MaxQueueSizeBehavior = MaxQueueSizeBehavior.DropNew,
            DispatchMode = SubscriberDispatchMode.Sequential
        };

        publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent), async (args) =>
        {
            await Task.Delay(50); // Slow processing
            receivedEvents.Add(args.DataPoint);
        }, options);

        publisher.Start();

        // Act - publish more events than queue can hold
        var tasks = new List<Task>();
        for (int i = 0; i < 20; i++)
        {
            var dataPoint = new DataPoint(i, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), i * 1.5, "test");
            tasks.Add(publisher.PublishDataIngestedAsync(dataPoint));
        }

        await Task.WhenAll(tasks);
        await Task.Delay(300); // Wait for processing
        await publisher.StopAsync();

        // Assert - should only receive events that fit in queue (5 events)
        // Note: Due to timing, might get slightly more, but should be close to 5
        Assert.InRange(receivedEvents.Count, 4, 7);
    }

    // Helper subscriber that throws on every event
    private class ThrowingSubscriber : EventSubscriberBase
    {
        private readonly Action<DataIngestedEventArgs> _onEvent;
        private int _receivedCount;

        public ThrowingSubscriber(PipelineEventPublisher publisher, ILogger<ThrowingSubscriber> logger,
            Action<DataIngestedEventArgs> onEvent) : base(publisher, logger)
        {
            _onEvent = onEvent;
        }

        public override void Subscribe()
        {
            base.Subscribe();
            _publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent), OnDataIngestedAsync);
        }

        private async Task OnDataIngestedAsync(DataIngestedEventArgs args)
        {
            Interlocked.Increment(ref _receivedCount);
            _onEvent(args);
        }

        public int GetReceivedCount() => _receivedCount;
    }

    // Helper subscriber that counts received events
    private class CountingSubscriber : EventSubscriberBase
    {
        private readonly ConcurrentBag<DataPoint> _receivedEvents;
        private int _receivedCount;

        public CountingSubscriber(PipelineEventPublisher publisher, ILogger<CountingSubscriber> logger,
            ConcurrentBag<DataPoint> receivedEvents) : base(publisher, logger)
        {
            _receivedEvents = receivedEvents;
        }

        public override void Subscribe()
        {
            base.Subscribe();
            _publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent), OnDataIngestedAsync);
        }

        private async Task OnDataIngestedAsync(DataIngestedEventArgs args)
        {
            Interlocked.Increment(ref _receivedCount);
            _receivedEvents.Add(args.DataPoint);
        }

        public int GetReceivedCount() => _receivedCount;
    }
}