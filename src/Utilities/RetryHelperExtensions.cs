#nullable enable
// =============================================================================
// Author: 
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;

/// <summary>
/// Extension methods for RetryHelper.
/// </summary>
public static class RetryHelperExtensions
{
    /// <summary>
    /// Executes an operation with retry logic and returns the retry statistics.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="maxAttempts">The maximum number of attempts.</param>
    /// <param name="initialDelayMs">The initial delay in milliseconds.</param>
    /// <returns>The result of the operation and the retry statistics.</returns>
    public static (T result, RetryStatistics statistics) RetryWithStatistics<T>(
        Func<Task<T>> operation,
        int maxAttempts = 3,
        int initialDelayMs = 100)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var statistics = new RetryStatistics();
        var policy = new RetryPolicy
        {
            MaxAttempts = maxAttempts,
            InitialDelayMs = initialDelayMs
        };

        return (policy.ExecuteAsync(operation).Result, statistics);
    }

    /// <summary>
    /// Executes an operation with retry logic and returns the retry statistics.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="maxAttempts">The maximum number of attempts.</param>
    /// <param name="initialDelayMs">The initial delay in milliseconds.</param>
    /// <returns>The result of the operation and the retry statistics.</returns>
    public static (T result, RetryStatistics statistics) RetryWithStatistics<T>(
        Func<T> operation,
        int maxAttempts = 3,
        int initialDelayMs = 100)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var statistics = new RetryStatistics();
        var policy = new RetryPolicy
        {
            MaxAttempts = maxAttempts,
            InitialDelayMs = initialDelayMs
        };

        return (policy.ExecuteAsync(() => Task.FromResult(operation())).Result, statistics);
    }

    /// <summary>
    /// Maps retry events to a list of tuples containing timestamp and delay.
    /// </summary>
    /// <param name="statistics">The retry statistics.</param>
    /// <returns>A list of tuples containing timestamp and delay.</returns>
    public static IReadOnlyList<(DateTime timestamp, int delayMs)> GetRetryEvents(
        this RetryStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);

        return statistics.Events.Select(e => (e.Timestamp, e.DelayMs)).ToList();
    }
}
