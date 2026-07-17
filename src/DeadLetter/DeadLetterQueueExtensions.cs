#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace DotNetRealtimePipeline.DeadLetter;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="DeadLetterQueue"/> that provide common operations
/// for working with dead-letter entries.
/// </summary>
public static class DeadLetterQueueExtensions
{
    private const string ExceptionReconstructionFailedMessage = "Failed to reconstruct exception from stored type information";

    /// <summary>
    /// Attempts to dequeue and process entries for retry until the queue is empty or
    /// the maximum number of entries have been processed.
    /// </summary>
    /// <param name="queue">The dead letter queue instance.</param>
    /// <param name="maxCount">Maximum number of entries to process.</param>
    /// <param name="processEntry">Async function that processes a single entry.</param>
    /// <returns>Statistics about the processing operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="queue"/> or <paramref name="processEntry"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxCount"/> is not positive.</exception>
    public static async Task<DeadLetterProcessingResult> ProcessForRetryAsync(
        this DeadLetterQueue queue,
        int maxCount,
        Func<DeadLetterEntry, Task<bool>> processEntry)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentNullException.ThrowIfNull(processEntry);

        if (maxCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Max count must be positive.");
        }

        var processedCount = 0;
        var successfulCount = 0;
        var failedCount = 0;
        var entriesProcessed = new List<DeadLetterEntry>();

        while (processedCount < maxCount)
        {
            var entries = await queue.DequeueForRetryAsync(Math.Min(10, maxCount - processedCount));
            if (entries.Count == 0)
            {
                break; // Queue is empty
            }

            foreach (var entry in entries)
            {
                entriesProcessed.Add(entry);
                processedCount++;

                try
                {
                    var success = await processEntry(entry);
                    if (success)
                    {
                        await queue.AcknowledgeSuccessAsync(entry.EntryId);
                        successfulCount++;
                    }
                    else
                    {
                        // Requeue the entry for another attempt
                        Exception? exceptionToRequeue = null;
                        if (entry.ExceptionType is not null)
                        {
                            try
                            {
                                var exceptionType = Type.GetType(entry.ExceptionType);
                                if (exceptionType is not null)
                                {
                                    var constructor = exceptionType.GetConstructor(Type.EmptyTypes);
                                    if (constructor is not null)
                                    {
                                        exceptionToRequeue = constructor.Invoke(null) as Exception;
                                    }
                                }
                            }
                            catch
                            {
                                // If we can't reconstruct the exception, proceed without it
                            }
                        }

                        await queue.EnqueueAsync(
                            entry.DataPoint,
                            entry.FailureStageName,
                            entry.FailureReason,
                            exceptionToRequeue);
                        failedCount++;
                    }
                }
                catch (Exception ex)
                {
                    // Mark as permanent failure on processing exception
                    await queue.AcknowledgeFailureAsync(entry.EntryId, $"Processing failed: {ex.Message}");
                    failedCount++;
                }
            }
        }

        return new DeadLetterProcessingResult
        {
            TotalProcessed = processedCount,
            SuccessfullyProcessed = successfulCount,
            FailedProcessing = failedCount,
            EntriesProcessed = entriesProcessed.AsReadOnly()
        };
    }

    /// <summary>
    /// Finds all entries matching the specified predicate.
    /// </summary>
    /// <param name="queue">The dead letter queue instance.</param>
    /// <param name="predicate">Filter predicate to match entries.</param>
    /// <param name="maxCount">Maximum number of entries to return.</param>
    /// <returns>Matching entries, ordered by enqueue time.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="queue"/> or <paramref name="predicate"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxCount"/> is not positive.</exception>
    public static async Task<IReadOnlyList<DeadLetterEntry>> FindAsync(
        this DeadLetterQueue queue,
        Func<DeadLetterEntry, bool> predicate,
        int maxCount = 100)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentNullException.ThrowIfNull(predicate);

        if (maxCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Max count must be positive.");
        }

        var allEntries = await queue.PeekAsync(maxCount);
        var matches = allEntries.Where(predicate).ToList();
        return matches.AsReadOnly();
    }

    /// <summary>
    /// Finds entries by failure stage name.
    /// </summary>
    /// <param name="queue">The dead letter queue instance.</param>
    /// <param name="stageName">Stage name to filter by (case-insensitive).</param>
    /// <param name="maxCount">Maximum number of entries to return.</param>
    /// <returns>Matching entries, ordered by enqueue time.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="stageName"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxCount"/> is not positive.</exception>
    public static async Task<IReadOnlyList<DeadLetterEntry>> FindByStageAsync(
        this DeadLetterQueue queue,
        string stageName,
        int maxCount = 100)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        if (maxCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Max count must be positive.");
        }

        return await queue.FindAsync(
            entry => string.Equals(entry.FailureStageName, stageName, StringComparison.OrdinalIgnoreCase),
            maxCount);
    }

    /// <summary>
    /// Gets a summary report of the dead letter queue state.
    /// </summary>
    /// <param name="queue">The dead letter queue instance.</param>
    /// <param name="includeDetails">Whether to include detailed entry information.</param>
    /// <returns>A formatted report string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
    public static async Task<string> GetReportAsync(
        this DeadLetterQueue queue,
        bool includeDetails = false)
    {
        ArgumentNullException.ThrowIfNull(queue);

        var stats = await queue.GetStatsAsync();
        var report = new List<string>();

        report.Add("=== Dead Letter Queue Report ===");
        report.Add($"Generated: {stats.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
        report.Add(string.Empty);
        report.Add($"Total Entries: {stats.TotalEntries}");
        report.Add($"Pending: {stats.PendingEntries}");
        report.Add($"In Retry: {stats.InRetryEntries}");
        report.Add($"Permanent Failures: {stats.PermanentFailureEntries}");
        report.Add($"Total Resolved: {stats.TotalResolved}");
        report.Add(string.Empty);

        if (includeDetails && stats.TotalEntries > 0)
        {
            report.Add("=== Entry Details ===");
            var entries = await queue.PeekAsync(stats.TotalEntries);

            foreach (var entry in entries.OrderBy(e => e.EnqueuedAt))
            {
                report.Add(entry.GetSummary());

                if (entry.ExceptionType is not null)
                {
                    report.Add($" Exception: {entry.ExceptionType}: {entry.ExceptionMessage ?? "No message"}");
                }

                if (entry.Status == DeadLetterStatus.PermanentFailure && entry.ResolutionNote is not null)
                {
                    report.Add($" Resolution: {entry.ResolutionNote}");
                }

                report.Add(string.Empty);
            }
        }

        return string.Join(Environment.NewLine, report);
    }
}

/// <summary>
/// Result of processing dead letter entries.
/// </summary>
public sealed class DeadLetterProcessingResult
{
    /// <summary>Total number of entries processed.</summary>
    public int TotalProcessed { get; init; }

    /// <summary>Number of entries successfully processed.</summary>
    public int SuccessfullyProcessed { get; init; }

    /// <summary>Number of entries that failed processing.</summary>
    public int FailedProcessing { get; init; }

    /// <summary>List of entries that were processed.</summary>
    public IReadOnlyList<DeadLetterEntry> EntriesProcessed { get; init; } = Array.Empty<DeadLetterEntry>();
}