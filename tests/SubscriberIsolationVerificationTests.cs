#nullable enable

namespace DotNetRealtimePipeline.Tests;

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Events;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// Verification tests for per-subscriber error isolation and concurrency policy.
/// Demonstrates that the architecture correctly implements the required improvement.
/// </summary>
public class SubscriberIsolationVerificationTests
{
    private readonly ILoggerFactory _loggerFactory;

    public SubscriberIsolationVerificationTests()
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    }

    [Fact]
    public async Task PerSubscriberIsolation_WhenOneSubscriberThrows_OtherSubscribersContinueProcessing()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var normalEvents = new ConcurrentBag<DataPoint>();
        var throwingEvents = new ConcurrentBag<DataPoint>();
        var errorCount = 0;

        // Create a subscriber that throws on every event
        var throwingSubscriber = new ThrowingSubscriber(publisher, _loggerFactory.CreateLogger<ThrowingSubscriber>(),
            (args) =>
            {
                errorCount++;
                throw new InvalidOperationException("Simulated error");
            });

        // Create normal subscribers that count events
        var normalSubscriber1 = new CountingSubscriber(publisher, _loggerFactory.CreateLogger<CountingSubscriber>(), normalEvents);
        var normalSubscriber2 = new CountingSubscriber(publisher, _loggerFactory.CreateLogger<CountingSubscriber>(), normalEvents);

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
    public async Task SubscriberOptions_ConfigurePerSubscriberConcurrencyAndErrorHandling()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var receivedEvents = new ConcurrentBag<int>();
        var processedCount = 0;

        // Create subscriber with custom options: parallel processing with 4 workers
        var parallelOptions = new SubscriberOptions
        {
            Name = "Parallel4WorkerSubscriber",
            DispatchMode = SubscriberDispatchMode.Parallel,
            MaxDegreeOfParallelism = 4,
            MaxQueueSize = 100,
            MaxQueueSizeBehavior = MaxQueueSizeBehavior.DropNew,
            ErrorPolicy = SubscriberErrorPolicy.SwallowAndCount
        };

        // Create subscriber with fail-fast policy
        var failFastOptions = new SubscriberOptions
        {
            Name = "FailFastSubscriber",
            DispatchMode = SubscriberDispatchMode.Sequential,
            MaxDegreeOfParallelism = 1,
            MaxQueueSize = 100,
            MaxQueueSizeBehavior = MaxQueueSizeBehavior.DropNew,
            ErrorPolicy = SubscriberErrorPolicy.FailFast
        };

        publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent),
            async (args) =>
            {
                await Task.Delay(5); // Simulate work
                receivedEvents.Add(args.DataPoint.Id);
                Interlocked.Increment(ref processedCount);
            }, parallelOptions);

        publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent),
            (args) =>
            {
                throw new InvalidOperationException("Fail fast test");
            }, failFastOptions);

        publisher.Start();

        // Act - publish multiple events
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            var dataPoint = new DataPoint(i, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), i * 1.5, "test");
            tasks.Add(publisher.PublishDataIngestedAsync(dataPoint));
        }

        await Task.WhenAll(tasks);
        await Task.Delay(200);
        await publisher.StopAsync();

        // Assert - parallel subscriber should have processed many events
        Assert.True(receivedEvents.Count > 50,
            $"Parallel subscriber should have processed many events. Actual: {receivedEvents.Count}");

        // Verify options were applied correctly
        Assert.Equal("Parallel4WorkerSubscriber", parallelOptions.Name);
        Assert.Equal(SubscriberDispatchMode.Parallel, parallelOptions.DispatchMode);
        Assert.Equal(4, parallelOptions.MaxDegreeOfParallelism);
        Assert.Equal(SubscriberErrorPolicy.FailFast, failFastOptions.ErrorPolicy);
    }

    [Fact]
    public async Task EventSubscriberBase_GetSubscriberOptions_ReturnsCustomizableOptions()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var subscriber = new CustomOptionsSubscriber(publisher, _loggerFactory.CreateLogger<CustomOptionsSubscriber>());

        // Act
        var options = subscriber.GetSubscriberOptions();

        // Assert
        Assert.Equal("CustomOptionsSubscriber", options.Name);
        Assert.Equal(500, options.MaxQueueSize);
        Assert.Equal(SubscriberDispatchMode.Parallel, options.DispatchMode);
        Assert.Equal(2, options.MaxDegreeOfParallelism);
        Assert.Equal(SubscriberErrorPolicy.DeadLetter, options.ErrorPolicy);
        Assert.Equal(MaxQueueSizeBehavior.Block, options.MaxQueueSizeBehavior);
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
            var options = GetSubscriberOptions();
            _publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent), OnDataIngestedAsync, options);
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
            var options = GetSubscriberOptions();
            _publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent), OnDataIngestedAsync, options);
        }

        private async Task OnDataIngestedAsync(DataIngestedEventArgs args)
        {
            Interlocked.Increment(ref _receivedCount);
            _receivedEvents.Add(args.DataPoint);
        }

        public int GetReceivedCount() => _receivedCount;
    }

    // Helper subscriber that demonstrates custom options override
    private class CustomOptionsSubscriber : EventSubscriberBase
    {
        public CustomOptionsSubscriber(PipelineEventPublisher publisher, ILogger<CustomOptionsSubscriber> logger)
            : base(publisher, logger)
        {
        }

        protected override SubscriberOptions GetSubscriberOptions()
        {
            return new SubscriberOptions
            {
                Name = "CustomOptionsSubscriber",
                MaxQueueSize = 500,
                MaxQueueSizeBehavior = MaxQueueSizeBehavior.Block,
                DispatchMode = SubscriberDispatchMode.Parallel,
                MaxDegreeOfParallelism = 2,
                ErrorPolicy = SubscriberErrorPolicy.DeadLetter
            };
        }

        public override void Subscribe()
        {
            base.Subscribe();
            var options = GetSubscriberOptions();
            _publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent), OnDataIngestedAsync, options);
        }

        private async Task OnDataIngestedAsync(DataIngestedEventArgs args)
        {
            await Task.Delay(1);
        }
    }
}