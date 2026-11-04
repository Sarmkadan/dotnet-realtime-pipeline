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

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineEventPublisher"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for diagnostic logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
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
        ArgumentNullException.ThrowIfNull(dataPoint);
        _logger.LogInformation("PublishDataIngestedAsync called with {DataPoint}", dataPoint);
        try
        {
            var eventName = nameof(DataIngestedEvent);
            var args = new DataIngestedEventArgs { DataPoint = dataPoint, Timestamp = DateTime.UtcNow };

            await PublishEventAsync(eventName, args);
            _logger.LogInformation("PublishDataIngestedAsync completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish DataIngestedEvent");
            throw;
        }
    }

    /// <summary>
    /// Publishes a processing completed event.
    /// </summary>
    public async Task PublishProcessingCompletedAsync(ProcessingResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        _logger.LogInformation("PublishProcessingCompletedAsync called with {Result}", result);
        try
        {
            var eventName = nameof(ProcessingCompletedEvent);
            var args = new ProcessingCompletedEventArgs { Result = result, Timestamp = DateTime.UtcNow };

            await PublishEventAsync(eventName, args);
            _logger.LogInformation("PublishProcessingCompletedAsync completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish ProcessingCompletedEvent");
            throw;
        }
    }

    /// <summary>
    /// Publishes a backpressure detected event.
    /// </summary>
    public async Task PublishBackpressureDetectedAsync(string stageName, BackpressureContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _logger.LogInformation("PublishBackpressureDetectedAsync called with {StageName}", stageName);
        try
        {
            var eventName = nameof(BackpressureDetectedEvent);
            var args = new BackpressureDetectedEventArgs
            {
                StageName = stageName,
                Context = context,
                Timestamp = DateTime.UtcNow
            };

            await PublishEventAsync(eventName, args);
            _logger.LogInformation("PublishBackpressureDetectedAsync completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish BackpressureDetectedEvent");
            throw;
        }
    }

    /// <summary>
    /// Publishes a metrics collected event.
    /// </summary>
    public async Task PublishMetricsCollectedAsync(MetricAggregation metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        _logger.LogInformation("PublishMetricsCollectedAsync called with {Metrics}", metrics);
        try
        {
            var eventName = nameof(MetricsCollectedEvent);
            var args = new MetricsCollectedEventArgs { Metrics = metrics, Timestamp = DateTime.UtcNow };

            await PublishEventAsync(eventName, args);
            _logger.LogInformation("PublishMetricsCollectedAsync completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish MetricsCollectedEvent");
            throw;
        }
    }

    /// <summary>
    /// Publishes a pipeline error event.
    /// </summary>
    public async Task PublishPipelineErrorAsync(string operationName, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        _logger.LogInformation("PublishPipelineErrorAsync called with {OperationName}", operationName);
        try
        {
            var eventName = nameof(PipelineErrorEvent);
            var args = new PipelineErrorEventArgs
            {
                OperationName = operationName,
                Exception = exception,
                Timestamp = DateTime.UtcNow
            };

            await PublishEventAsync(eventName, args);
            _logger.LogInformation("PublishPipelineErrorAsync completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish PipelineErrorEvent");
            throw;
        }
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

    /// <summary>
    /// Releases all resources used by the <see cref="PipelineEventPublisher"/>.
    /// </summary>
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
    private const int CorrelationIdLength = 8;
    private const string CorrelationIdFormat = "N";

    /// <summary>
    /// Gets or sets the timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the correlation ID for tracing related events.
    /// </summary>
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString(CorrelationIdFormat).Substring(0, CorrelationIdLength);

    /// <summary>
    /// Returns a summary of the event args including timestamp and correlation ID.
    /// </summary>
    public override string ToString()
    {
        return $"Timestamp={Timestamp:O}, CorrelationId={CorrelationId}";
    }
}

/// <summary>
/// Event args for data ingestion events.
/// </summary>
public sealed class DataIngestedEventArgs : PipelineEventArgs
{
    /// <summary>
    /// Gets or sets the data point that was ingested.
    /// </summary>
    public DataPoint DataPoint { get; set; }

    /// <summary>
    /// Returns a summary of the event args including the ingested data point.
    /// </summary>
    public override string ToString()
    {
        return $"{base.ToString()}, DataPoint={DataPoint}";
    }
}

/// <summary>
/// Event args for processing completion events.
/// </summary>
public sealed class ProcessingCompletedEventArgs : PipelineEventArgs
{
    /// <summary>
    /// Gets or sets the processing result.
    /// </summary>
    public ProcessingResult Result { get; set; }

    /// <summary>
    /// Returns a summary of the event args including the processing result.
    /// </summary>
    public override string ToString()
    {
        return $"{base.ToString()}, Result={Result}";
    }
}

/// <summary>
/// Event args for backpressure detection events.
/// </summary>
public sealed class BackpressureDetectedEventArgs : PipelineEventArgs
{
    /// <summary>
    /// Gets or sets the name of the pipeline stage where backpressure was detected.
    /// </summary>
    public string StageName { get; set; }

    /// <summary>
    /// Gets or sets the backpressure context containing details about the backpressure condition.
    /// </summary>
    public BackpressureContext Context { get; set; }

    /// <summary>
    /// Returns a summary of the event args including the stage name and backpressure context.
    /// </summary>
    public override string ToString()
    {
        return $"{base.ToString()}, StageName={StageName}, Context={Context}";
    }
}

/// <summary>
/// Event args for metrics collection events.
/// </summary>
public sealed class MetricsCollectedEventArgs : PipelineEventArgs
{
    /// <summary>
    /// Gets or sets the collected metrics aggregation.
    /// </summary>
    public MetricAggregation Metrics { get; set; }

    /// <summary>
    /// Returns a summary of the event args including the collected metrics.
    /// </summary>
    public override string ToString()
    {
        return $"{base.ToString()}, Metrics={Metrics}";
    }
}

/// <summary>
/// Event args for pipeline error events.
/// </summary>
public sealed class PipelineErrorEventArgs : PipelineEventArgs
{
    /// <summary>
    /// Gets or sets the name of the operation that caused the error.
    /// </summary>
    public string OperationName { get; set; }

    /// <summary>
    /// Gets or sets the exception that occurred.
    /// </summary>
    public Exception Exception { get; set; }

    /// <summary>
    /// Returns a summary of the event args including the operation name and exception type and message.
    /// </summary>
    public override string ToString()
    {
        return $"{base.ToString()}, OperationName={OperationName}, Exception={Exception?.GetType().FullName}: {Exception?.Message}";
    }
}

// Event definitions
/// <summary>
/// Event-type marker for data ingested events.
/// Used to identify the <see cref="DataIngestedEventArgs"/> event type in subscriptions.
/// </summary>
public sealed class DataIngestedEvent { }

/// <summary>
/// Event-type marker for processing completed events.
/// Used to identify the <see cref="ProcessingCompletedEventArgs"/> event type in subscriptions.
/// </summary>
public sealed class ProcessingCompletedEvent { }

/// <summary>
/// Event-type marker for backpressure detected events.
/// Used to identify the <see cref="BackpressureDetectedEventArgs"/> event type in subscriptions.
/// </summary>
public sealed class BackpressureDetectedEvent { }

/// <summary>
/// Event-type marker for metrics collected events.
/// Used to identify the <see cref="MetricsCollectedEventArgs"/> event type in subscriptions.
/// </summary>
public sealed class MetricsCollectedEvent { }

/// <summary>
/// Event-type marker for pipeline error events.
/// Used to identify the <see cref="PipelineErrorEventArgs"/> event type in subscriptions.
/// </summary>
public sealed class PipelineErrorEvent { }
