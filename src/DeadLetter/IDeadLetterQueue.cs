#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.DeadLetter;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Abstracts the dead-letter queue so implementations can be swapped
/// (in-memory, database-backed, external broker, etc.) without changing
/// the calling code.
/// </summary>
public interface IDeadLetterQueue
{
    /// <summary>
    /// Enqueues a failed data point with contextual failure information.
    /// </summary>
    /// <param name="dataPoint">The data point that failed processing.</param>
    /// <param name="stageName">The name of the pipeline stage where the failure occurred.</param>
    /// <param name="failureReason">The reason for the failure.</param>
    /// <param name="exception">The exception that caused the failure, if any.</param>
    Task EnqueueAsync(DataPoint dataPoint, string stageName, string failureReason, Exception? exception = null);

    /// <summary>
    /// Returns up to <paramref name="maxCount"/> entries from the queue without removing them.
    /// </summary>
    /// <param name="maxCount">The maximum number of entries to peek.</param>
    /// <returns>List of dead letter entries.</returns>
    Task<IReadOnlyList<DeadLetterEntry>> PeekAsync(int maxCount = 100);

    /// <summary>
    /// Returns and removes up to <paramref name="maxCount"/> entries ready for re-processing.
    /// </summary>
    /// <param name="maxCount">The maximum number of entries to dequeue for retry.</param>
    /// <returns>List of dead letter entries ready for retry.</returns>
    Task<IReadOnlyList<DeadLetterEntry>> DequeueForRetryAsync(int maxCount = 10);

    /// <summary>
    /// Marks an entry as permanently failed (exhausted retries or non-retryable error).
    /// </summary>
    /// <param name="entryId">The identifier of the entry to acknowledge.</param>
    /// <param name="finalReason">The final reason for the permanent failure.</param>
    Task AcknowledgeFailureAsync(Guid entryId, string finalReason);

    /// <summary>
    /// Marks an entry as successfully reprocessed and removes it from the queue.
    /// </summary>
    /// <param name="entryId">The identifier of the entry to acknowledge.</param>
    Task AcknowledgeSuccessAsync(Guid entryId);

    /// <summary>
    /// Returns queue statistics.
    /// </summary>
    /// <returns>Queue statistics including counts and totals.</returns>
    Task<DeadLetterQueueStats> GetStatsAsync();

    /// <summary>
    /// Gets the total number of entries currently waiting in the queue (pending + failed-permanently).
    /// </summary>
    int Count { get; }
}
