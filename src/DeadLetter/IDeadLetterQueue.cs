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
    Task EnqueueAsync(DataPoint dataPoint, string stageName, string failureReason, Exception? exception = null);

    /// <summary>
    /// Returns up to <paramref name="maxCount"/> entries from the queue without removing them.
    /// </summary>
    Task<IReadOnlyList<DeadLetterEntry>> PeekAsync(int maxCount = 100);

    /// <summary>
    /// Returns and removes up to <paramref name="maxCount"/> entries ready for re-processing.
    /// </summary>
    Task<IReadOnlyList<DeadLetterEntry>> DequeueForRetryAsync(int maxCount = 10);

    /// <summary>
    /// Marks an entry as permanently failed (exhausted retries or non-retryable error).
    /// </summary>
    Task AcknowledgeFailureAsync(Guid entryId, string finalReason);

    /// <summary>
    /// Marks an entry as successfully reprocessed and removes it from the queue.
    /// </summary>
    Task AcknowledgeSuccessAsync(Guid entryId);

    /// <summary>
    /// Returns queue statistics.
    /// </summary>
    Task<DeadLetterQueueStats> GetStatsAsync();

    /// <summary>
    /// Total number of entries currently waiting in the queue (pending + failed-permanently).
    /// </summary>
    int Count { get; }
}
