#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.DeadLetter;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="DeadLetterQueue"/> instances.
/// </summary>
public static class DeadLetterQueueValidation
{
    /// <summary>
    /// Validates the specified <see cref="DeadLetterQueue"/> instance.
    /// </summary>
    /// <param name="value">The dead letter queue to validate.</param>
    /// <returns>
    /// An empty list if the queue is valid; otherwise, a list of human-readable
    /// problem descriptions.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public static IReadOnlyList<string> Validate(this DeadLetterQueue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate internal state consistency
        lock (value)
        {
            // Check if _maxCapacity is valid (should be > 0 based on constructor)
            if (value.GetMaxCapacity() <= 0)
            {
                problems.Add("DeadLetterQueue._maxCapacity must be greater than 0.");
            }

            // Check if _defaultMaxRetries is valid (should be >= 0 based on constructor)
            if (value.GetDefaultMaxRetries() < 0)
            {
                problems.Add("DeadLetterQueue._defaultMaxRetries must be greater than or equal to 0.");
            }

            // Validate that _entries dictionary is not null
            var entries = value.GetEntries();
            if (entries is null)
            {
                problems.Add("DeadLetterQueue._entries dictionary must not be null.");
            }
            else
            {
                // Validate each entry in the queue
                foreach (var entry in entries.Values)
                {
                    if (entry is null)
                    {
                        problems.Add("DeadLetterQueue contains a null entry.");
                        continue;
                    }

                    var entryProblems = ValidateDeadLetterEntry(entry);
                    problems.AddRange(entryProblems);
                }
            }

            // Validate _totalResolved is non-negative
            if (value.GetTotalResolved() < 0)
            {
                problems.Add("DeadLetterQueue._totalResolved must be greater than or equal to 0.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DeadLetterQueue"/> instance is valid.
    /// </summary>
    /// <param name="value">The dead letter queue to check.</param>
    /// <returns>
    /// <see langword="true"/> if the queue is valid; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public static bool IsValid(this DeadLetterQueue value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="DeadLetterQueue"/> instance is valid.
    /// </summary>
    /// <param name="value">The dead letter queue to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="value"/> is not valid. The exception message contains
    /// a list of all validation problems.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public static void EnsureValid(this DeadLetterQueue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DeadLetterQueue validation failed:{Environment.NewLine}" +
                string.Join(Environment.NewLine, problems),
                nameof(value));
        }
    }

    /// <summary>
    /// Validates the specified <see cref="DeadLetterEntry"/> instance.
    /// </summary>
    /// <param name="entry">The dead letter entry to validate.</param>
    /// <returns>
    /// An empty list if the entry is valid; otherwise, a list of human-readable
    /// problem descriptions.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="entry"/> is <see langword="null"/>.
    /// </exception>
    private static IReadOnlyList<string> ValidateDeadLetterEntry(this DeadLetterEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var problems = new List<string>();

        // Validate EntryId (should be a valid Guid, not empty)
        if (entry.EntryId == Guid.Empty)
        {
            problems.Add("DeadLetterEntry.EntryId must be a non-empty Guid.");
        }

        // Validate DataPoint (should not be null)
        if (entry.DataPoint is null)
        {
            problems.Add("DeadLetterEntry.DataPoint must not be null.");
        }
        else
        {
            // Validate the DataPoint itself
            if (!entry.DataPoint.Validate())
            {
                problems.Add("DeadLetterEntry.DataPoint is not valid.");
            }
        }

        // Validate FailureStageName (should not be null or whitespace)
        if (string.IsNullOrWhiteSpace(entry.FailureStageName))
        {
            problems.Add("DeadLetterEntry.FailureStageName must not be null or empty.");
        }

        // Validate FailureReason (should not be null or whitespace)
        if (string.IsNullOrWhiteSpace(entry.FailureReason))
        {
            problems.Add("DeadLetterEntry.FailureReason must not be null or empty.");
        }

        // Validate ExceptionType (if not null, should not be empty or whitespace)
        if (!string.IsNullOrWhiteSpace(entry.ExceptionType) &&
            string.IsNullOrWhiteSpace(entry.ExceptionType))
        {
            problems.Add("DeadLetterEntry.ExceptionType must not be empty if set.");
        }

        // Validate ExceptionMessage (if not null, should not be empty or whitespace when ExceptionType is set)
        if (!string.IsNullOrWhiteSpace(entry.ExceptionType) &&
            string.IsNullOrWhiteSpace(entry.ExceptionMessage))
        {
            problems.Add("DeadLetterEntry.ExceptionMessage should be set when ExceptionType is set.");
        }

        // Validate RetryCount (should be >= 0)
        if (entry.RetryCount < 0)
        {
            problems.Add("DeadLetterEntry.RetryCount must be greater than or equal to 0.");
        }

        // Validate MaxRetries (should be >= 0)
        if (entry.MaxRetries < 0)
        {
            problems.Add("DeadLetterEntry.MaxRetries must be greater than or equal to 0.");
        }
        else if (entry.RetryCount > entry.MaxRetries)
        {
            problems.Add("DeadLetterEntry.RetryCount must not exceed MaxRetries.");
        }

        // Validate EnqueuedAt (should be a valid date, not default(DateTime))
        if (entry.EnqueuedAt == default)
        {
            problems.Add("DeadLetterEntry.EnqueuedAt must be a valid UTC date.");
        }
        else if (entry.EnqueuedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("DeadLetterEntry.EnqueuedAt cannot be in the future.");
        }

        // Validate LastRetryAt (if set, should be a valid date and not in the future)
        if (entry.LastRetryAt.HasValue)
        {
            if (entry.LastRetryAt.Value == default)
            {
                problems.Add("DeadLetterEntry.LastRetryAt must be a valid UTC date if set.");
            }
            else if (entry.LastRetryAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("DeadLetterEntry.LastRetryAt cannot be in the future.");
            }

            // If LastRetryAt is set, Status should be InRetry
            if (entry.Status != DeadLetterStatus.InRetry)
            {
                problems.Add("DeadLetterEntry.Status must be InRetry when LastRetryAt is set.");
            }
        }
        else
        {
            // If LastRetryAt is not set, Status should not be InRetry
            if (entry.Status == DeadLetterStatus.InRetry)
            {
                problems.Add("DeadLetterEntry.Status must not be InRetry when LastRetryAt is not set.");
            }
        }

        // Validate ResolvedAt (if set, should be a valid date and not in the future)
        if (entry.ResolvedAt.HasValue)
        {
            if (entry.ResolvedAt.Value == default)
            {
                problems.Add("DeadLetterEntry.ResolvedAt must be a valid UTC date if set.");
            }
            else if (entry.ResolvedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("DeadLetterEntry.ResolvedAt cannot be in the future.");
            }
        }

        // Validate Status (should be a valid DeadLetterStatus enum value)
        if (!Enum.IsDefined(typeof(DeadLetterStatus), entry.Status))
        {
            problems.Add("DeadLetterEntry.Status must be a valid DeadLetterStatus value.");
        }

        // Validate ResolutionNote (if set, should not be empty or whitespace)
        if (!string.IsNullOrWhiteSpace(entry.ResolutionNote) &&
            string.IsNullOrWhiteSpace(entry.ResolutionNote))
        {
            problems.Add("DeadLetterEntry.ResolutionNote must not be empty if set.");
        }

        // Validate business rules based on Status
        switch (entry.Status)
        {
            case DeadLetterStatus.Resolved:
                if (!entry.ResolvedAt.HasValue)
                {
                    problems.Add("DeadLetterEntry.ResolvedAt must be set when Status is Resolved.");
                }
                break;

            case DeadLetterStatus.PermanentFailure:
                if (!entry.ResolvedAt.HasValue)
                {
                    problems.Add("DeadLetterEntry.ResolvedAt must be set when Status is PermanentFailure.");
                }
                if (string.IsNullOrWhiteSpace(entry.ResolutionNote))
                {
                    problems.Add("DeadLetterEntry.ResolutionNote must be set when Status is PermanentFailure.");
                }
                break;

            case DeadLetterStatus.InRetry:
                if (!entry.LastRetryAt.HasValue)
                {
                    problems.Add("DeadLetterEntry.LastRetryAt must be set when Status is InRetry.");
                }
                break;

            case DeadLetterStatus.Pending:
                // No additional validation needed for Pending status
                break;
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the specified <see cref="DeadLetterQueueStats"/> instance.
    /// </summary>
    /// <param name="stats">The dead letter queue stats to validate.</param>
    /// <returns>
    /// An empty list if the stats are valid; otherwise, a list of human-readable
    /// problem descriptions.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="stats"/> is <see langword="null"/>.
    /// </exception>
    private static IReadOnlyList<string> ValidateDeadLetterQueueStats(this DeadLetterQueueStats stats)
    {
        ArgumentNullException.ThrowIfNull(stats);

        var problems = new List<string>();

        // Validate TotalEntries (should be >= 0)
        if (stats.TotalEntries < 0)
        {
            problems.Add("DeadLetterQueueStats.TotalEntries must be greater than or equal to 0.");
        }

        // Validate PendingEntries (should be >= 0 and <= TotalEntries)
        if (stats.PendingEntries < 0)
        {
            problems.Add("DeadLetterQueueStats.PendingEntries must be greater than or equal to 0.");
        }
        else if (stats.PendingEntries > stats.TotalEntries)
        {
            problems.Add("DeadLetterQueueStats.PendingEntries must not exceed TotalEntries.");
        }

        // Validate InRetryEntries (should be >= 0 and <= TotalEntries)
        if (stats.InRetryEntries < 0)
        {
            problems.Add("DeadLetterQueueStats.InRetryEntries must be greater than or equal to 0.");
        }
        else if (stats.InRetryEntries > stats.TotalEntries)
        {
            problems.Add("DeadLetterQueueStats.InRetryEntries must not exceed TotalEntries.");
        }

        // Validate PermanentFailureEntries (should be >= 0 and <= TotalEntries)
        if (stats.PermanentFailureEntries < 0)
        {
            problems.Add("DeadLetterQueueStats.PermanentFailureEntries must be greater than or equal to 0.");
        }
        else if (stats.PermanentFailureEntries > stats.TotalEntries)
        {
            problems.Add("DeadLetterQueueStats.PermanentFailureEntries must not exceed TotalEntries.");
        }

        // Validate TotalResolved (should be >= 0)
        if (stats.TotalResolved < 0)
        {
            problems.Add("DeadLetterQueueStats.TotalResolved must be greater than or equal to 0.");
        }

        // Validate GeneratedAt (should be a valid date, not default(DateTime))
        if (stats.GeneratedAt == default)
        {
            problems.Add("DeadLetterQueueStats.GeneratedAt must be a valid UTC date.");
        }
        else if (stats.GeneratedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("DeadLetterQueueStats.GeneratedAt cannot be in the future.");
        }

        // Validate that the sum of status counts equals TotalEntries
        var calculatedTotal = stats.PendingEntries + stats.InRetryEntries + stats.PermanentFailureEntries;
        if (calculatedTotal != stats.TotalEntries)
        {
            problems.Add(
                $"DeadLetterQueueStats status counts do not match TotalEntries. " +
                $"Expected: {stats.TotalEntries}, Actual: {calculatedTotal} (Pending: {stats.PendingEntries}, " +
                $"InRetry: {stats.InRetryEntries}, PermanentFailure: {stats.PermanentFailureEntries}).");
        }

        return problems.AsReadOnly();
    }

    // Reflection-based accessors for private fields to enable validation
    private static int GetMaxCapacity(this DeadLetterQueue queue)
    {
        var field = typeof(DeadLetterQueue).GetField("_maxCapacity",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (int)field!.GetValue(queue)!;
    }

    private static int GetDefaultMaxRetries(this DeadLetterQueue queue)
    {
        var field = typeof(DeadLetterQueue).GetField("_defaultMaxRetries",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (int)field!.GetValue(queue)!;
    }

    private static System.Collections.Generic.Dictionary<Guid, DeadLetterEntry> GetEntries(this DeadLetterQueue queue)
    {
        var field = typeof(DeadLetterQueue).GetField("_entries",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (System.Collections.Generic.Dictionary<Guid, DeadLetterEntry>)field!.GetValue(queue)!;
    }

    private static long GetTotalResolved(this DeadLetterQueue queue)
    {
        var field = typeof(DeadLetterQueue).GetField("_totalResolved",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (long)field!.GetValue(queue)!;
    }
}