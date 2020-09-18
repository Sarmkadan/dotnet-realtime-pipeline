namespace DotNetRealtimePipeline.DeadLetter;

using System;

/// <summary>
/// Provides extension methods for <see cref="DeadLetterEntry"/>.
/// </summary>
public static class DeadLetterEntryExtensions
{
    /// <summary>
    /// Checks if the entry has been successfully resolved.
    /// </summary>
    /// <param name="entry">The <see cref="DeadLetterEntry"/> to check.</param>
    /// <returns><c>true</c> if the entry status is <see cref="DeadLetterStatus.Resolved"/>; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is null.</exception>
    public static bool IsResolved(this DeadLetterEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.Status == DeadLetterStatus.Resolved;
    }

    /// <summary>
    /// Checks if the entry has reached a permanent failure state.
    /// </summary>
    /// <param name="entry">The <see cref="DeadLetterEntry"/> to check.</param>
    /// <returns><c>true</c> if the entry status is <see cref="DeadLetterStatus.PermanentFailure"/>; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is null.</exception>
    public static bool IsPermanentFailure(this DeadLetterEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.Status == DeadLetterStatus.PermanentFailure;
    }

    /// <summary>
    /// Calculates the retry attempt percentage as a ratio of <see cref="DeadLetterEntry.RetryCount"/> to <see cref="DeadLetterEntry.MaxRetries"/>.
    /// </summary>
    /// <param name="entry">The <see cref="DeadLetterEntry"/> to analyze.</param>
    /// <returns>A double value representing the retry progress (0.0 to 1.0).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is null.</exception>
    public static double GetRetryProgress(this DeadLetterEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.MaxRetries > 0 ? (double)entry.RetryCount / entry.MaxRetries : 0.0;
    }

    /// <summary>
    /// Returns the most recent activity timestamp for the entry.
    /// </summary>
    /// <param name="entry">The <see cref="DeadLetterEntry"/> to inspect.</param>
    /// <returns>The <see cref="DeadLetterEntry.LastRetryAt"/> if available, otherwise the <see cref="DeadLetterEntry.EnqueuedAt"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is null.</exception>
    public static DateTime GetLastActivity(this DeadLetterEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.LastRetryAt ?? entry.EnqueuedAt;
    }
}
