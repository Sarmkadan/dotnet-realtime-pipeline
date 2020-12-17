#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents an event flowing through the stream at a specific point in time.
/// Wraps data points with metadata about their processing context.
/// </summary>
public sealed class StreamEvent
{
    /// <summary>
    /// Gets or sets the unique identifier for this stream event.
    /// </summary>
    public long EventId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the data point associated with this event.
    /// </summary>
    public long DataPointId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the event occurred (Unix timestamp in milliseconds).
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the type of event (e.g., "data", "error", "control").
    /// </summary>
    public string EventType { get; set; } = "";

    /// <summary>
    /// Gets or sets the priority level (1-10) for processing order.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Gets or sets the source system that generated this event.
    /// </summary>
    public string? SourceSystem { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier for tracing this event through the pipeline.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the causation identifier (the event that caused this one).
    /// </summary>
    public string? CausationId { get; set; }

    /// <summary>
    /// Gets or sets the payload data as key-value pairs.
    /// </summary>
    public Dictionary<string, object> Payload { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of stages that have processed this event.
    /// </summary>
    public List<string> ProcessedByStages { get; set; } = new();

    /// <summary>
    /// Gets or sets the creation timestamp of this event.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the completion timestamp if processing is complete.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this event is a retry.
    /// </summary>
    public bool IsRetry { get; set; }

    /// <summary>
    /// Gets or sets the retry attempt number.
    /// </summary>
    public int RetryAttempt { get; set; }

    /// <summary>
    /// Gets or sets the last error message if processing failed.
    /// </summary>
    public string? LastErrorMessage { get; set; }

    public StreamEvent()
    {
    }

    public StreamEvent(long eventId, long dataPointId, long timestamp, string eventType)
    {
        EventId = eventId;
        DataPointId = dataPointId;
        Timestamp = timestamp;
        EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        Priority = 5; // Default priority
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks this event as processed by a stage.
    /// </summary>
    public void MarkProcessedByStage(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName))
            throw new ArgumentException("Stage name cannot be null", nameof(stageName));

        if (!ProcessedByStages.Contains(stageName))
        {
            ProcessedByStages.Add(stageName);
        }
    }

    /// <summary>
    /// Checks if the event has been processed by a specific stage.
    /// </summary>
    public bool HasBeenProcessedByStage(string stageName)
    {
        return ProcessedByStages.Contains(stageName);
    }

    /// <summary>
    /// Gets the processing path (stages the event has been through).
    /// </summary>
    public string GetProcessingPath()
    {
        return string.Join(" -> ", ProcessedByStages);
    }

    /// <summary>
    /// Completes the event processing.
    /// </summary>
    public void CompleteProcessing()
    {
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the event as a retry with error information.
    /// </summary>
    public void MarkAsRetry(string errorMessage)
    {
        IsRetry = true;
        RetryAttempt++;
        LastErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
    }

    /// <summary>
    /// Adds a payload item to the event.
    /// </summary>
    public void AddPayload(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null", nameof(key));
        Payload[key] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets a payload item.
    /// </summary>
    public object? GetPayload(string key)
    {
        return Payload.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Calculates the total processing time in milliseconds.
    /// </summary>
    public long GetTotalProcessingTimeMs()
    {
        if (!CompletedAt.HasValue) return -1;
        return (long)(CompletedAt.Value - CreatedAt).TotalMilliseconds;
    }

    /// <summary>
    /// Gets the age of this event in milliseconds.
    /// </summary>
    public long GetAgeMs()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - Timestamp;
    }

    /// <summary>
    /// Gets summary information about this event.
    /// </summary>
    public string GetSummary()
    {
        return $"StreamEvent[Id={EventId}, Type={EventType}, Priority={Priority}, Stages={ProcessedByStages.Count}, Completed={CompletedAt.HasValue}]";
    }

    /// <summary>
    /// Creates a child event derived from this one.
    /// </summary>
    public StreamEvent CreateChildEvent(long newEventId, string newEventType)
    {
        var child = new StreamEvent(newEventId, DataPointId, Timestamp, newEventType)
        {
            SourceSystem = SourceSystem,
            CorrelationId = CorrelationId ?? Guid.NewGuid().ToString(),
            CausationId = EventId.ToString(),
            Priority = Priority,
            Payload = new Dictionary<string, object>(Payload)
        };

        return child;
    }

    /// <summary>
    /// Validates the stream event.
    /// </summary>
    public bool Validate()
    {
        if (EventId <= 0) return false;
        if (DataPointId <= 0) return false;
        if (Timestamp <= 0) return false;
        if (string.IsNullOrWhiteSpace(EventType)) return false;
        if (Priority < 1 || Priority > 10) return false;
        return true;
    }
}
