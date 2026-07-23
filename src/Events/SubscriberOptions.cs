#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Events;

using System;
using System.Threading.Tasks;

/// <summary>
/// Defines error handling policies for subscriber error recovery.
/// </summary>
public enum SubscriberErrorPolicy
{
    /// <summary>
    /// Swallow the exception, log it, and continue processing subsequent events.
    /// </summary>
    SwallowAndCount,

    /// <summary>
    /// Swallow the exception, log it, and send the failed event to a dead-letter queue.
    /// </summary>
    DeadLetter,

    /// <summary>
    /// Propagate the exception to the caller, failing fast.
    /// </summary>
    FailFast
}

/// <summary>
/// Configuration options for subscriber behavior including concurrency and error handling.
/// </summary>
public sealed class SubscriberOptions
{
    /// <summary>
    /// Gets or sets the maximum number of events that can be queued per subscriber.
    /// When the queue is full, new events will be dropped or rejected based on <see cref="MaxQueueSizeBehavior"/>.
    /// Default: 1000
    /// </summary>
    public int MaxQueueSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the behavior when the queue is full.
    /// Default: DropNew
    /// </summary>
    public MaxQueueSizeBehavior MaxQueueSizeBehavior { get; set; } = MaxQueueSizeBehavior.DropNew;

    /// <summary>
    /// Gets or sets the dispatch mode for event handling.
    /// Default: Sequential
    /// </summary>
    public SubscriberDispatchMode DispatchMode { get; set; } = SubscriberDispatchMode.Sequential;

    /// <summary>
    /// Gets or sets the error handling policy when an event handler throws.
    /// Default: SwallowAndCount
    /// </summary>
    public SubscriberErrorPolicy ErrorPolicy { get; set; } = SubscriberErrorPolicy.SwallowAndCount;

    /// <summary>
    /// Gets or sets the maximum degree of parallelism when <see cref="DispatchMode"/> is Parallel.
    /// Default: 1 (sequential)
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 1;

    /// <summary>
    /// Gets or sets a custom name for this subscriber configuration.
    /// Useful for logging and diagnostics.
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Defines behaviors when a subscriber's queue reaches maximum capacity.
/// </summary>
public enum MaxQueueSizeBehavior
{
    /// <summary>
    /// Drop new events when the queue is full.
    /// </summary>
    DropNew,

    /// <summary>
    /// Block and wait when the queue is full (may cause backpressure).
    /// </summary>
    Block,

    /// <summary>
    /// Reject new events with an exception when the queue is full.
    /// </summary>
    Reject
}

/// <summary>
/// Defines dispatch modes for subscriber event handling.
/// </summary>
public enum SubscriberDispatchMode
{
    /// <summary>
    /// Process events sequentially in a single background task.
    /// </summary>
    Sequential,

    /// <summary>
    /// Process events in parallel using multiple worker tasks.
    /// </summary>
    Parallel
}

/// <summary>
/// Represents a dead-letter entry for an event that failed to be processed by a subscriber.
/// </summary>
public sealed class SubscriberDeadLetterEntry
{
    /// <summary>
    /// Gets the event name.
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// Gets the serialized event arguments.
    /// </summary>
    public string SerializedEventArgs { get; }

    /// <summary>
    /// Gets the subscriber name.
    /// </summary>
    public string SubscriberName { get; }

    /// <summary>
    /// Gets the timestamp when the event was sent to dead-letter.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the exception that caused the failure.
    /// </summary>
    public Exception? Exception { get; }

    public SubscriberDeadLetterEntry(string eventName, string serializedEventArgs, string subscriberName, Exception? exception)
    {
        EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
        SerializedEventArgs = serializedEventArgs ?? throw new ArgumentNullException(nameof(serializedEventArgs));
        SubscriberName = subscriberName ?? throw new ArgumentNullException(nameof(subscriberName));
        Exception = exception;
    }
}