#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.DeadLetter;

using DotNetRealtimePipeline.Domain.Models;
using System;

/// <summary>
/// Disposition of a dead-letter entry.
/// </summary>
public enum DeadLetterStatus
{
    /// <summary>Waiting to be retried.</summary>
    Pending = 0,

    /// <summary>Currently being retried by a consumer.</summary>
    InRetry = 1,

    /// <summary>Successfully reprocessed; will be removed.</summary>
    Resolved = 2,

    /// <summary>Permanently failed — no further retries will be attempted.</summary>
    PermanentFailure = 3
}

/// <summary>
/// Represents a data point that could not be processed by the pipeline
/// and has been routed to the dead-letter queue for inspection and retry.
/// </summary>
public sealed class DeadLetterEntry
{
    /// <summary>Unique identifier of this dead-letter record.</summary>
    public Guid EntryId { get; set; } = Guid.NewGuid();

    /// <summary>The data point that failed processing.</summary>
    public DataPoint DataPoint { get; set; } = new();

    /// <summary>Name of the pipeline stage where the failure occurred.</summary>
    public string FailureStageName { get; set; } = "";

    /// <summary>Human-readable reason for the failure.</summary>
    public string FailureReason { get; set; } = "";

    /// <summary>Exception type name, if an exception caused the failure.</summary>
    public string? ExceptionType { get; set; }

    /// <summary>Exception message, if available.</summary>
    public string? ExceptionMessage { get; set; }

    /// <summary>Number of retry attempts made for this entry.</summary>
    public int RetryCount { get; set; }

    /// <summary>Maximum number of retries allowed before marking as permanent failure.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>UTC time the entry was first placed in the dead-letter queue.</summary>
    public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC time of the last retry attempt, if any.</summary>
    public DateTime? LastRetryAt { get; set; }

    /// <summary>UTC time the entry was resolved or permanently failed.</summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Current disposition of this entry.</summary>
    public DeadLetterStatus Status { get; set; } = DeadLetterStatus.Pending;

    /// <summary>Optional notes added when acknowledging failure.</summary>
    public string? ResolutionNote { get; set; }

    /// <summary>
    /// Returns <c>true</c> when the entry has not yet exhausted its retry budget.
    /// </summary>
    public bool CanRetry => RetryCount < MaxRetries && Status == DeadLetterStatus.Pending;

    /// <summary>
    /// Advances the retry counter and updates state for the next attempt.
    /// </summary>
    public void BeginRetry()
    {
        RetryCount++;
        LastRetryAt = DateTime.UtcNow;
        Status = DeadLetterStatus.InRetry;
    }

    /// <summary>
    /// Resets the entry back to <see cref="DeadLetterStatus.Pending"/> after a failed retry.
    /// </summary>
    public void RetryFailed(string reason)
    {
        FailureReason = reason ?? FailureReason;
        Status = RetryCount >= MaxRetries
            ? DeadLetterStatus.PermanentFailure
            : DeadLetterStatus.Pending;
    }

    /// <summary>
    /// Returns a concise summary of the entry.
    /// </summary>
    public string GetSummary()
    {
        return $"DLQ[{EntryId:N} | dp={DataPoint.Id} | stage={FailureStageName} | " +
               $"retries={RetryCount}/{MaxRetries} | status={Status}]";
    }
}

/// <summary>
/// Aggregated statistics about the dead-letter queue.
/// </summary>
public sealed class DeadLetterQueueStats
{
    /// <summary>Total entries currently in the queue.</summary>
    public int TotalEntries { get; set; }

    /// <summary>Entries awaiting retry.</summary>
    public int PendingEntries { get; set; }

    /// <summary>Entries currently in a retry attempt.</summary>
    public int InRetryEntries { get; set; }

    /// <summary>Entries permanently failed.</summary>
    public int PermanentFailureEntries { get; set; }

    /// <summary>Total entries that were successfully resolved since startup.</summary>
    public long TotalResolved { get; set; }

    /// <summary>UTC time these stats were generated.</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
