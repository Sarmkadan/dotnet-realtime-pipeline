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
public class PipelineEventPublisher
{
    private readonly ConcurrentDictionary<string, List<Delegate>> _subscribers = new();
    private readonly ILogger<PipelineEventPublisher> _logger;

    public PipelineEventPublisher(ILogger<PipelineEventPublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    /// Subscribes to a specific event type.
    /// </summary>
    public void Subscribe<T>(string eventName, Func<T, Task> handler) where T : PipelineEventArgs
    {
        var handlers = _subscribers.GetOrAdd(eventName, _ => new List<Delegate>());
        lock (handlers)
        {
            handlers.Add(handler);
        }

        _logger.LogDebug("Subscriber registered for event: {Event}", eventName);
    }

    /// <summary>
    /// Unsubscribes from an event.
    /// </summary>
    public void Unsubscribe(string eventName, Delegate handler)
    {
        if (_subscribers.TryGetValue(eventName, out var handlers))
        {
            lock (handlers)
            {
                handlers.Remove(handler);
            }
        }

        _logger.LogDebug("Subscriber unregistered from event: {Event}", eventName);
    }

    /// <summary>
    /// Publishes an event to all registered subscribers.
    /// </summary>
    private async Task PublishEventAsync(string eventName, PipelineEventArgs args)
    {
        if (!_subscribers.TryGetValue(eventName, out var handlers))
        {
            return;
        }

        List<Delegate> handlersCopy;
        lock (handlers)
        {
            handlersCopy = new List<Delegate>(handlers);
        }

        foreach (var handler in handlersCopy)
        {
            try
            {
                if (handler is Delegate del)
                {
                    var result = del.DynamicInvoke(args);
                    if (result is Task task)
                    {
                        await task;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking subscriber for event: {Event}", eventName);
            }
        }
    }

    /// <summary>
    /// Gets subscriber count for an event.
    /// </summary>
    public int GetSubscriberCount(string eventName)
    {
        if (_subscribers.TryGetValue(eventName, out var handlers))
        {
            lock (handlers)
            {
                return handlers.Count;
            }
        }

        return 0;
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
public class DataIngestedEventArgs : PipelineEventArgs
{
    public DataPoint DataPoint { get; set; }
}

/// <summary>
/// Event args for processing completion events.
/// </summary>
public class ProcessingCompletedEventArgs : PipelineEventArgs
{
    public ProcessingResult Result { get; set; }
}

/// <summary>
/// Event args for backpressure detection events.
/// </summary>
public class BackpressureDetectedEventArgs : PipelineEventArgs
{
    public string StageName { get; set; }
    public BackpressureContext Context { get; set; }
}

/// <summary>
/// Event args for metrics collection events.
/// </summary>
public class MetricsCollectedEventArgs : PipelineEventArgs
{
    public MetricAggregation Metrics { get; set; }
}

/// <summary>
/// Event args for pipeline error events.
/// </summary>
public class PipelineErrorEventArgs : PipelineEventArgs
{
    public string OperationName { get; set; }
    public Exception Exception { get; set; }
}

// Event definitions
public class DataIngestedEvent { }
public class ProcessingCompletedEvent { }
public class BackpressureDetectedEvent { }
public class MetricsCollectedEvent { }
public class PipelineErrorEvent { }
