#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Events;

using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

/// <summary>
/// Publisher for pipeline events using observer pattern.
/// Decouples event producers from consumers through subscription-based notification.
/// </summary>
public sealed class PipelineEventPublisher : IDisposable
{
    private readonly SubscriberChannelManager _channelManager;
    private readonly ILogger<PipelineEventPublisher> _logger;
    private bool _disposed;

    public PipelineEventPublisher(ILogger<PipelineEventPublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _channelManager = new SubscriberChannelManager(logger);
    }

    /// <summary>
    /// Publishes a data ingestion event to all subscribers.
    /// </summary>
    public async Task PublishDataIngestedAsync(DataPoint dataPoint)
    {
        var eventName = nameof(DataIngestedEvent);
        var args = new DataIngestedEventArgs { DataPoint = dataPoint, Timestamp = DateTime.UtcNow };

        await PublishEventAsync(eventName, args);
    }

    /// <summary>
    /// Publishes a processing completed event.
    /// </summary>
    public async Task PublishProcessingCompletedAsync(ProcessingResult result)
    {
        var eventName = nameof(ProcessingCompletedEvent);
        var args = new ProcessingCompletedEventArgs { Result = result, Timestamp = DateTime.UtcNow };

        await PublishEventAsync(eventName, args);
    }

    /// <summary>
    /// Publishes a backpressure detected event.
    /// </summary>
    public async Task PublishBackpressureDetectedAsync(string stageName, BackpressureContext context)
    {
        var eventName = nameof(BackpressureDetectedEvent);
        var args = new BackpressureDetectedEventArgs
        {
            StageName = stageName,
            Context = context,
            Timestamp = DateTime.UtcNow
        };

        await PublishEventAsync(eventName, args);
    }

    /// <summary>
    /// Publishes a metrics collected event.
    /// </summary>
    public async Task PublishMetricsCollectedAsync(MetricAggregation metrics)
    {
        var eventName = nameof(MetricsCollectedEvent);
        var args = new MetricsCollectedEventArgs { Metrics = metrics, Timestamp = DateTime.UtcNow };

        await PublishEventAsync(eventName, args);
    }

    /// <summary>
    /// Publishes a pipeline error event.
    /// </summary>
    public async Task PublishPipelineErrorAsync(string operationName, Exception exception)
    {
        var eventName = nameof(PipelineErrorEvent);
        var args = new PipelineErrorEventArgs
        {
            OperationName = operationName,
            Exception = exception,
            Timestamp = DateTime.UtcNow
        };

        await PublishEventAsync(eventName, args);
    }

    /// <summary>
    /// Subscribes to a specific event type with default options.
    /// </summary>
    public void Subscribe<T>(string eventName, Func<T, Task> handler) where T : PipelineEventArgs
    {
        Subscribe(eventName, handler, new SubscriberOptions
        {
            Name = handler.Method.DeclaringType?.Name ?? "AnonymousSubscriber"
        });
    }

    /// <summary>
    /// Subscribes to a specific event type with customizable options.
    /// </summary>
    public void Subscribe<T>(string eventName, Func<T, Task> handler, SubscriberOptions options) where T : PipelineEventArgs
    {
        if (string.IsNullOrWhiteSpace(eventName))
        {
            throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));
        }

        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _channelManager.RegisterSubscriber(eventName, handler, options);
        _logger.LogDebug("Subscriber registered for event: {Event} with options: {Options}", eventName, options);
    }

    /// <summary>
    /// Publishes an event to all registered subscribers.
    /// Each subscriber processes events independently with its own error handling and concurrency.
    /// </summary>
    private async Task PublishEventAsync(string eventName, PipelineEventArgs args)
    {
        await _channelManager.PostEventAsync(eventName, args).ConfigureAwait(false);
    }

    /// <summary>
    /// Starts all subscriber channels.
    /// Call this after all subscriptions are registered to begin processing events.
    /// </summary>
    public void Start()
    {
        _channelManager.StartAll();
    }

    /// <summary>
    /// Stops all subscriber channels gracefully.
    /// </summary>
    public async Task StopAsync()
    {
        await _channelManager.StopAllAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the total error count across all subscribers for an event.
    /// </summary>
    public int GetSubscriberErrorCount(string eventName)
    {
        return _channelManager.GetTotalErrorCount(eventName);
    }

    /// <summary>
    /// Gets the number of registered subscriber channels for an event.
    /// </summary>
    public int GetSubscriberCount(string eventName)
    {
        return _channelManager.GetSubscriberCount(eventName);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _channelManager.Dispose();
        _disposed = true;
    }
}

/// <summary>
/// Base class for all pipeline event arguments.
/// </summary>
public abstract class PipelineEventArgs
{
    public DateTime Timestamp { get; set; }
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8);
}

/// <summary>
/// Event args for data ingestion events.
/// </summary>
public sealed class DataIngestedEventArgs : PipelineEventArgs
{
    public DataPoint DataPoint { get; set; }
}

/// <summary>
/// Event args for processing completion events.
/// </summary>
public sealed class ProcessingCompletedEventArgs : PipelineEventArgs
{
    public ProcessingResult Result { get; set; }
}

/// <summary>
/// Event args for backpressure detection events.
/// </summary>
public sealed class BackpressureDetectedEventArgs : PipelineEventArgs
{
    public string StageName { get; set; }
    public BackpressureContext Context { get; set; }
}

/// <summary>
/// Event args for metrics collection events.
/// </summary>
public sealed class MetricsCollectedEventArgs : PipelineEventArgs
{
    public MetricAggregation Metrics { get; set; }
}

/// <summary>
/// Event args for pipeline error events.
/// </summary>
public sealed class PipelineErrorEventArgs : PipelineEventArgs
{
    public string OperationName { get; set; }
    public Exception Exception { get; set; }
}

// Event definitions
public sealed class DataIngestedEvent { }
public sealed class ProcessingCompletedEvent { }
public sealed class BackpressureDetectedEvent { }
public sealed class MetricsCollectedEvent { }
public sealed class PipelineErrorEvent { }
