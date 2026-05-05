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
public class StreamEvent
{
    public long EventId { get; set; }
    public long DataPointId { get; set; }
    public long Timestamp { get; set; }
    public string EventType { get; set; } = "";
    public int Priority { get; set; }

    public string? SourceSystem { get; set; }
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }

    public Dictionary<string, object> Payload { get; set; } = new();
    public List<string> ProcessedByStages { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public bool IsRetry { get; set; }
    public int RetryAttempt { get; set; }
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
