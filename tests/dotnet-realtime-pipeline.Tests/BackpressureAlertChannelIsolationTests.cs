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
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// Tests verifying that backpressure alert delivery is not compromised when the
/// data-ingestion channel is saturated.
/// </summary>
public class BackpressureAlertChannelIsolationTests
{
    private readonly ILoggerFactory _loggerFactory;

    public BackpressureAlertChannelIsolationTests()
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    }

    [Fact]
    public async Task BackpressureAlert_WhenDataChannelIsFullToCapacity_IsStillDelivered()
    {
        // Arrange
        var publisher = new PipelineEventPublisher(_loggerFactory.CreateLogger<PipelineEventPublisher>());
        var receivedAlerts = new ConcurrentBag<string>();

        // A slow, tightly bounded data-ingest subscriber to force its channel into saturation.
        var dataOptions = new SubscriberOptions
        {
            Name = "SaturatedDataSubscriber",
            MaxQueueSize = 5,
            MaxQueueSizeBehavior = MaxQueueSizeBehavior.DropNew,
            DispatchMode = SubscriberDispatchMode.Sequential
        };

        publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent), async args =>
        {
            await Task.Delay(200); // Slow enough to guarantee the small queue fills up.
        }, dataOptions);

        var backpressureSubscriber = new BackpressureAlertSubscriber(
            publisher, _loggerFactory.CreateLogger<BackpressureAlertSubscriber>());
        backpressureSubscriber.Subscribe();

        publisher.Start();

        // Act - flood the data channel well past its bounded capacity.
        var floodTasks = new List<Task>();
        for (int i = 0; i < 500; i++)
        {
            var dataPoint = new DataPoint(i, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), i * 1.0, "flood-source");
            floodTasks.Add(publisher.PublishDataIngestedAsync(dataPoint));
        }
        await Task.WhenAll(floodTasks);

        // Publish a burst of backpressure alerts while the data channel remains saturated.
        for (int i = 0; i < 50; i++)
        {
            var context = new BackpressureContext
            {
                BufferSize = 990,
                MaxBufferCapacity = 1000,
                IsBackpressured = true
            };
            await publisher.PublishBackpressureDetectedAsync("ingest-stage", context);
            receivedAlerts.Add($"published-{i}");
        }

        await Task.Delay(300);
        await publisher.StopAsync();

        // Assert - every backpressure alert must have been counted, none dropped,
        // even though the data channel it shares a publisher with was saturated.
        Assert.Equal(50, backpressureSubscriber.GetBackpressureEventCount());
        Assert.Equal(50, receivedAlerts.Count);
    }
}
