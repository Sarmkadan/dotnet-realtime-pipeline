#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.DeadLetter;

using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Extension methods for <see cref="IRetryPolicy"/>.
/// </summary>
public static class RetryPolicyExtensions
{
    /// <summary>
    /// Returns the backoff delay for each attempt number from 1 to <see cref="IRetryPolicy.MaxAttempts"/>.
    /// </summary>
    /// <param name="policy">The retry policy.</param>
    /// <returns>An enumerable of backoff delays, where the first element corresponds to the delay after the first attempt, and so on.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is <see langword="null"/>.</exception>
    public static IEnumerable<TimeSpan> GetBackoffSchedule(this IRetryPolicy policy)
    {
        if (policy == null)
            throw new ArgumentNullException(nameof(policy));

        for (int attempt = 1; attempt <= policy.MaxAttempts; attempt++)
        {
            yield return policy.GetBackoffDelay(attempt);
        }
    }

    /// <summary>
    /// Returns the sum of all backoff delays for attempts 1 through <see cref="IRetryPolicy.MaxAttempts"/>.
    /// </summary>
    /// <param name="policy">The retry policy.</param>
    /// <returns>The total backoff time if all attempts were to fail and wait the full delay.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is <see langword="null"/>.</exception>
    public static TimeSpan GetTotalBackoff(this IRetryPolicy policy)
    {
        if (policy == null)
            throw new ArgumentNullException(nameof(policy));

        var total = TimeSpan.Zero;
        for (int attempt = 1; attempt <= policy.MaxAttempts; attempt++)
        {
            total += policy.GetBackoffDelay(attempt);
        }

        return total;
    }

    /// <summary>
    /// Determines whether a retry should be attempted based on the exception being transient and the attempt number being within the allowed retry count.
    /// </summary>
    /// <param name="policy">The retry policy.</param>
    /// <param name="exception">The exception to check for transience.</param>
    /// <param name="attemptNumber">The 1-based number of the attempt that just failed.</param>
    /// <returns><see langword="true"/> if the exception is transient and another attempt is allowed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> or <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static bool ShouldRetry(this IRetryPolicy policy, Exception exception, int attemptNumber)
    {
        if (policy == null)
            throw new ArgumentNullException(nameof(policy));
        if (exception == null)
            throw new ArgumentNullException(nameof(exception));

        return policy.IsTransient(exception) && attemptNumber < policy.MaxAttempts;
    }
}