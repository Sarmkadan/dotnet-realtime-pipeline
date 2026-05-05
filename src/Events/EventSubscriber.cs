#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Events;

using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

/// <summary>
/// Base class for event subscribers with common lifecycle management.
/// Provides subscription pattern and event handling infrastructure.
/// </summary>
public abstract class EventSubscriberBase
{
    protected readonly PipelineEventPublisher _publisher;
    protected readonly ILogger _logger;
    protected bool _isSubscribed;

    protected EventSubscriberBase(PipelineEventPublisher publisher, ILogger logger)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Subscribes to all relevant events.
    /// </summary>
    public virtual void Subscribe()
    {
        _isSubscribed = true;
        _logger.LogInformation("{SubscriberName} subscribed to events", GetType().Name);
    }

    /// <summary>
    /// Unsubscribes from all events.
    /// </summary>
    public virtual void Unsubscribe()
    {
        _isSubscribed = false;
        _logger.LogInformation("{SubscriberName} unsubscribed from events", GetType().Name);
    }
}

/// <summary>
/// Subscriber for data ingestion events with processing logic.
/// </summary>
public class DataIngestSubscriber : EventSubscriberBase
{
    public DataIngestSubscriber(PipelineEventPublisher publisher, ILogger<DataIngestSubscriber> logger)
        : base(publisher, logger)
    {
    }

    public override void Subscribe()
    {
        base.Subscribe();
        _publisher.Subscribe<DataIngestedEventArgs>(
            nameof(DataIngestedEvent), OnDataIngestedAsync);
    }

    /// <summary>
    /// Handles data ingestion events.
    /// </summary>
    private async Task OnDataIngestedAsync(DataIngestedEventArgs args)
    {
        try
        {
            _logger.LogDebug("Data point ingested - ID: {Id}, Source: {Source}, Quality: {Quality}",
                args.DataPoint.Id, args.DataPoint.Source, args.DataPoint.Quality);

            await ProcessIngestedDataAsync(args.DataPoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ingested data");
        }
    }

    /// <summary>
    /// Custom processing logic for ingested data.
    /// </summary>
    protected virtual async Task ProcessIngestedDataAsync(DataPoint dataPoint)
    {
        // Can be overridden by derived classes
        await Task.CompletedTask;
    }
}

/// <summary>
/// Subscriber for processing completion events with metrics tracking.
/// </summary>
public class ProcessingCompletionSubscriber : EventSubscriberBase
{
    private long _successCount;
    private long _failureCount;

    public ProcessingCompletionSubscriber(PipelineEventPublisher publisher, ILogger<ProcessingCompletionSubscriber> logger)
        : base(publisher, logger)
    {
    }

    public override void Subscribe()
    {
        base.Subscribe();
        _publisher.Subscribe<ProcessingCompletedEventArgs>(
            nameof(ProcessingCompletedEvent), OnProcessingCompletedAsync);
    }

    /// <summary>
    /// Handles processing completion events.
    /// </summary>
    private async Task OnProcessingCompletedAsync(ProcessingCompletedEventArgs args)
    {
        try
        {
            if (args.Result.Success)
            {
                _successCount++;
                _logger.LogDebug("Processing completed successfully - ResultId: {Id}", args.Result.ResultId);
            }
            else
            {
                _failureCount++;
                _logger.LogWarning("Processing failed - ResultId: {Id}, Message: {Message}",
                    args.Result.ResultId, args.Result.ErrorMessage);
            }

            await OnProcessingCompletionAsync(args.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling processing completion");
        }
    }

    /// <summary>
    /// Gets the success rate percentage.
    /// </summary>
    public double GetSuccessRatePercent()
    {
        var total = _successCount + _failureCount;
        return total > 0 ? (_successCount * 100.0) / total : 100.0;
    }

    /// <summary>
    /// Custom processing logic for completion events.
    /// </summary>
    protected virtual async Task OnProcessingCompletionAsync(ProcessingResult result)
    {
        await Task.CompletedTask;
    }
}

/// <summary>
/// Subscriber for backpressure events with alerting logic.
/// </summary>
public class BackpressureAlertSubscriber : EventSubscriberBase
{
    private int _backpressureCount;
    private DateTime _firstBackpressureTime = DateTime.MinValue;

    public BackpressureAlertSubscriber(PipelineEventPublisher publisher, ILogger<BackpressureAlertSubscriber> logger)
        : base(publisher, logger)
    {
    }

    public override void Subscribe()
    {
        base.Subscribe();
        _publisher.Subscribe<BackpressureDetectedEventArgs>(
            nameof(BackpressureDetectedEvent), OnBackpressureDetectedAsync);
    }

    /// <summary>
    /// Handles backpressure detection events with alerting.
    /// </summary>
    private async Task OnBackpressureDetectedAsync(BackpressureDetectedEventArgs args)
    {
        try
        {
            _backpressureCount++;
            if (_firstBackpressureTime == DateTime.MinValue)
                _firstBackpressureTime = args.Timestamp;

            var utilizationPercent = (args.Context.BufferSize * 100.0) / args.Context.MaxBufferCapacity;

            _logger.LogWarning(
                "Backpressure detected - Stage: {Stage}, Utilization: {Util:F1}%, IsBackpressured: {Backpressured}, Count: {Count}",
                args.StageName, utilizationPercent, args.Context.IsBackpressured, _backpressureCount);

            if (utilizationPercent > 95)
            {
                await OnCriticalBackpressureAsync(args);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling backpressure event");
        }
    }

    /// <summary>
    /// Gets backpressure event count.
    /// </summary>
    public int GetBackpressureEventCount() => _backpressureCount;

    /// <summary>
    /// Custom logic for critical backpressure situations.
    /// </summary>
    protected virtual async Task OnCriticalBackpressureAsync(BackpressureDetectedEventArgs args)
    {
        _logger.LogError("CRITICAL: Backpressure exceeds 95% threshold for stage {Stage}", args.StageName);
        await Task.CompletedTask;
    }
}

/// <summary>
/// Subscriber for metrics collection events with aggregation.
/// </summary>
public class MetricsAggregationSubscriber : EventSubscriberBase
{
    private double _totalThroughput;
    private double _totalLatency;
    private int _metricsCount;

    public MetricsAggregationSubscriber(PipelineEventPublisher publisher, ILogger<MetricsAggregationSubscriber> logger)
        : base(publisher, logger)
    {
    }

    public override void Subscribe()
    {
        base.Subscribe();
        _publisher.Subscribe<MetricsCollectedEventArgs>(
            nameof(MetricsCollectedEvent), OnMetricsCollectedAsync);
    }

    /// <summary>
    /// Handles metrics collection events.
    /// </summary>
    private async Task OnMetricsCollectedAsync(MetricsCollectedEventArgs args)
    {
        try
        {
            _totalLatency += args.Metrics.AverageProcessingTimeMs;
            _metricsCount++;

            _logger.LogDebug("Metrics collected - Processing Time: {ProcessingTime:F2}ms, Total Items: {Total}",
                args.Metrics.AverageProcessingTimeMs, args.Metrics.TotalItemsProcessed);

            await OnMetricsReceivedAsync(args.Metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling metrics event");
        }
    }

    /// <summary>
    /// Gets average processing time.
    /// </summary>
    public double GetAverageProcessingTime()
    {
        return _metricsCount > 0 ? _totalLatency / _metricsCount : 0;
    }

    /// <summary>
    /// Gets total metrics collected.
    /// </summary>
    public int GetMetricsCount()
    {
        return _metricsCount;
    }

    /// <summary>
    /// Custom logic when metrics are received.
    /// </summary>
    protected virtual async Task OnMetricsReceivedAsync(MetricAggregation metrics)
    {
        await Task.CompletedTask;
    }
}

/// <summary>
/// Subscriber for error events with logging and alerting.
/// </summary>
public class ErrorAlertSubscriber : EventSubscriberBase
{
    private int _errorCount;

    public ErrorAlertSubscriber(PipelineEventPublisher publisher, ILogger<ErrorAlertSubscriber> logger)
        : base(publisher, logger)
    {
    }

    public override void Subscribe()
    {
        base.Subscribe();
        _publisher.Subscribe<PipelineErrorEventArgs>(
            nameof(PipelineErrorEvent), OnPipelineErrorAsync);
    }

    /// <summary>
    /// Handles pipeline error events.
    /// </summary>
    private async Task OnPipelineErrorAsync(PipelineErrorEventArgs args)
    {
        try
        {
            _errorCount++;
            _logger.LogError(args.Exception,
                "Pipeline error occurred - Operation: {Operation}, Count: {Count}, Message: {Message}",
                args.OperationName, _errorCount, args.Exception.Message);

            await OnErrorDetectedAsync(args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling error event");
        }
    }

    /// <summary>
    /// Gets total error count.
    /// </summary>
    public int GetErrorCount() => _errorCount;

    /// <summary>
    /// Custom logic when errors are detected.
    /// </summary>
    protected virtual async Task OnErrorDetectedAsync(PipelineErrorEventArgs args)
    {
        await Task.CompletedTask;
    }
}
