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

    /// <summary>
    /// Gets the default subscriber options for this subscriber.
    /// Override this property to customize per-subscriber behavior.
    /// </summary>
    protected virtual SubscriberOptions GetSubscriberOptions()
    {
        return new SubscriberOptions
        {
            Name = GetType().Name,
            MaxQueueSize = 1000,
            MaxQueueSizeBehavior = MaxQueueSizeBehavior.DropNew,
            DispatchMode = SubscriberDispatchMode.Sequential,
            ErrorPolicy = SubscriberErrorPolicy.SwallowAndCount,
            MaxDegreeOfParallelism = 1
        };
    }

    /// <summary>
    /// Starts the subscriber's event processing.
    /// </summary>
    public virtual void Start()
    {
        // Base implementation does nothing
        // Derived classes can override to start background processing
    }

    /// <summary>
    /// Stops the subscriber's event processing gracefully.
    /// </summary>
    public virtual async Task StopAsync()
    {
        // Base implementation does nothing
        await Task.CompletedTask;
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
        var options = GetSubscriberOptions();
        _publisher.Subscribe<DataIngestedEventArgs>(
            nameof(DataIngestedEvent), OnDataIngestedAsync, options);
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
        var options = GetSubscriberOptions();
        _publisher.Subscribe<ProcessingCompletedEventArgs>(
            nameof(ProcessingCompletedEvent), OnProcessingCompletedAsync, options);
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
        var options = GetSubscriberOptions();
        _publisher.Subscribe<BackpressureDetectedEventArgs>(
            nameof(BackpressureDetectedEvent), OnBackpressureDetectedAsync, options);
    }

    /// <summary>
    /// Gets the subscriber options for backpressure alerting.
    /// Alerts are the signal that tells operators the pipeline is saturated, so their
    /// channel must never apply the same bounded, drop-on-overflow policy used for
    /// high-volume data events - doing so would silently discard the alert exactly
    /// when it matters most. This overrides the default with an effectively unbounded,
    /// non-dropping queue, giving alert delivery a dedicated out-of-band lane.
    /// </summary>
    protected override SubscriberOptions GetSubscriberOptions()
    {
        return new SubscriberOptions
        {
            Name = GetType().Name,
            MaxQueueSize = int.MaxValue,
            MaxQueueSizeBehavior = MaxQueueSizeBehavior.Block,
            DispatchMode = SubscriberDispatchMode.Sequential,
            ErrorPolicy = SubscriberErrorPolicy.SwallowAndCount,
            MaxDegreeOfParallelism = 1
        };
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
/// Uses striped counters with Interlocked operations to minimize allocations on the hot path.
/// </summary>
public sealed class MetricsAggregationSubscriber : EventSubscriberBase
{
    // Striped counters for thread-safe aggregation across multiple threads
    // Using 16 stripes to reduce contention while maintaining cache locality
    private const int StripeCount = 16;
    private readonly StripedMetrics[] _stripedMetrics = new StripedMetrics[StripeCount];

    // Simple lock for snapshot operations (infrequent, not on hot path)
    private readonly object _snapshotLock = new object();
    private MetricsSnapshot _currentSnapshot;

    public MetricsAggregationSubscriber(PipelineEventPublisher publisher, ILogger<MetricsAggregationSubscriber> logger)
        : base(publisher, logger)
    {
        // Initialize striped counters
        for (int i = 0; i < StripeCount; i++)
        {
            _stripedMetrics[i] = new StripedMetrics();
        }
        _currentSnapshot = new MetricsSnapshot(0, 0);
    }

    public override void Subscribe()
    {
        base.Subscribe();
        var options = GetSubscriberOptions();
        _publisher.Subscribe<MetricsCollectedEventArgs>(
            nameof(MetricsCollectedEvent), OnMetricsCollectedAsync, options);
    }

    /// <summary>
    /// Handles metrics collection events on the hot path.
    /// Uses Interlocked operations for thread-safe accumulation with minimal overhead.
    /// </summary>
    private async Task OnMetricsCollectedAsync(MetricsCollectedEventArgs args)
    {
        try
        {
            // Fast path: accumulate metrics using striped counters
            // Use thread ID for minimal contention
            var stripeIndex = Environment.CurrentManagedThreadId % StripeCount;
            var metrics = _stripedMetrics[stripeIndex];

            // Accumulate using Interlocked operations - zero allocation on hot path
            Interlocked.Add(ref metrics.TotalLatencyNs, (long)(args.Metrics.AverageProcessingTimeMs * 1_000_000));
            Interlocked.Increment(ref metrics.MetricsCount);

            // Optional: Log at debug level only when needed
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Metrics collected - Processing Time: {ProcessingTime:F2}ms, Total Items: {Total}",
                    args.Metrics.AverageProcessingTimeMs, args.Metrics.TotalItemsProcessed);
            }

            OnMetricsReceived(args.Metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling metrics event");
        }
    }

    /// <summary>
    /// Gets the average processing time in milliseconds.
    /// Takes a consistent snapshot and computes the average from striped counters.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the subscriber is null.</exception>
    public double GetAverageProcessingTime()
    {
        var snapshot = TakeSnapshot();
        return snapshot.MetricsCount > 0
            ? snapshot.TotalLatencyMs / snapshot.MetricsCount
            : 0.0;
    }

    /// <summary>
    /// Gets the total number of metrics collected.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the subscriber is null.</exception>
    public int GetMetricsCount()
    {
        return TakeSnapshot().MetricsCount;
    }

    /// <summary>
    /// Takes a consistent snapshot of all accumulated metrics.
    /// Uses double-checked locking for efficiency (snapshot is infrequent).
    /// </summary>
    private MetricsSnapshot TakeSnapshot()
    {
        // Fast path: return cached snapshot if valid
        var snapshot = _currentSnapshot;
        if (snapshot.IsValid)
        {
            return snapshot;
        }

        // Slow path: merge all striped counters under lock
        lock (_snapshotLock)
        {
            // Re-check after acquiring lock
            snapshot = _currentSnapshot;
            if (snapshot.IsValid)
            {
                return snapshot;
            }

            // Merge all striped counters
            long totalLatencyNs = 0;
            int metricsCount = 0;

            foreach (var metrics in _stripedMetrics)
            {
                totalLatencyNs += metrics.TotalLatencyNs;
                metricsCount += metrics.MetricsCount;
            }

            // Create new snapshot
            snapshot = new MetricsSnapshot(totalLatencyNs / 1_000_000.0, metricsCount);
            _currentSnapshot = snapshot;

            return snapshot;
        }
    }

    /// <summary>
    /// Custom logic when metrics are received.
    /// Made non-virtual for maximum performance on hot path.
    /// </summary>
    private void OnMetricsReceived(MetricAggregation metrics)
    {
        // Extension point for derived classes - empty by default
    }

    /// <summary>
    /// Resets all accumulated metrics. Useful for testing or after snapshot operations.
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < StripeCount; i++)
        {
            _stripedMetrics[i] = new StripedMetrics();
        }

        lock (_snapshotLock)
        {
            _currentSnapshot = new MetricsSnapshot(0, 0);
        }
    }

    /// <summary>
    /// Struct representing per-stripe metrics using Interlocked-compatible fields.
    /// </summary>
    private struct StripedMetrics
    {
        public long TotalLatencyNs;
        public int MetricsCount;
    }

    /// <summary>
    /// Immutable snapshot of accumulated metrics.
    /// </summary>
    private readonly struct MetricsSnapshot
    {
        public readonly double TotalLatencyMs;
        public readonly int MetricsCount;
        public readonly DateTime Timestamp;

        public MetricsSnapshot(double totalLatencyMs, int metricsCount)
        {
            TotalLatencyMs = totalLatencyMs;
            MetricsCount = metricsCount;
            Timestamp = DateTime.UtcNow;
        }

        public readonly bool IsValid => MetricsCount >= 0;
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
        var options = GetSubscriberOptions();
        _publisher.Subscribe<PipelineErrorEventArgs>(
            nameof(PipelineErrorEvent), OnPipelineErrorAsync, options);
    }

    /// <summary>
    /// Gets the subscriber options for error alerting.
    /// Uses an effectively unbounded, non-dropping queue so error alerts are never
    /// discarded due to the same bounded capacity applied to high-volume data events.
    /// </summary>
    protected override SubscriberOptions GetSubscriberOptions()
    {
        return new SubscriberOptions
        {
            Name = GetType().Name,
            MaxQueueSize = int.MaxValue,
            MaxQueueSizeBehavior = MaxQueueSizeBehavior.Block,
            DispatchMode = SubscriberDispatchMode.Sequential,
            ErrorPolicy = SubscriberErrorPolicy.SwallowAndCount,
            MaxDegreeOfParallelism = 1
        };
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
