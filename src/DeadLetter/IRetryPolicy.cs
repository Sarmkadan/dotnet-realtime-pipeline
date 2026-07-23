#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.DeadLetter;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Decides whether and how a failed processing operation is retried
/// before the item is routed to the dead-letter queue.
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Gets the maximum number of attempts (including the first one).
    /// </summary>
    int MaxAttempts { get; }

    /// <summary>
    /// Classifies an exception as transient (retryable) or permanent.
    /// </summary>
    /// <param name="exception">The exception to classify.</param>
    /// <returns><see langword="true"/> when the exception is worth retrying.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    bool IsTransient(Exception exception);

    /// <summary>
    /// Computes the backoff delay to wait before the given attempt number.
    /// </summary>
    /// <param name="attemptNumber">The 1-based number of the attempt that just failed.</param>
    /// <returns>The delay to wait before the next attempt.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="attemptNumber"/> is less than 1.</exception>
    TimeSpan GetBackoffDelay(int attemptNumber);

    /// <summary>
    /// Executes <paramref name="operation"/>, retrying transient failures with backoff
    /// until it succeeds or the attempt budget is exhausted.
    /// </summary>
    /// <typeparam name="T">The operation result type.</typeparam>
    /// <param name="operation">The operation to execute; receives the 1-based attempt number.</param>
    /// <param name="cancellationToken">A token to cancel waiting between attempts.</param>
    /// <returns>A <see cref="RetryResult{T}"/> describing the outcome.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operation"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is cancelled.</exception>
    Task<RetryResult<T>> ExecuteAsync<T>(Func<int, Task<T>> operation, CancellationToken cancellationToken = default);
}

/// <summary>
/// The outcome of executing an operation under an <see cref="IRetryPolicy"/>.
/// </summary>
/// <typeparam name="T">The operation result type.</typeparam>
public sealed class RetryResult<T>
{
    private RetryResult(bool succeeded, T? value, int attempts, Exception? lastException)
    {
        Succeeded = succeeded;
        Value = value;
        Attempts = attempts;
        LastException = lastException;
    }

    /// <summary>
    /// Gets a value indicating whether the operation eventually succeeded.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Gets the operation result when <see cref="Succeeded"/> is <see langword="true"/>.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the number of attempts that were made.
    /// </summary>
    public int Attempts { get; }

    /// <summary>
    /// Gets the exception thrown by the final failed attempt, if any.
    /// </summary>
    public Exception? LastException { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="value">The operation result.</param>
    /// <param name="attempts">The number of attempts made.</param>
    /// <returns>A successful <see cref="RetryResult{T}"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="attempts"/> is less than 1.</exception>
    public static RetryResult<T> Success(T value, int attempts) => attempts >= 1
        ? new(true, value, attempts, null)
        : throw new ArgumentOutOfRangeException(nameof(attempts), attempts, "Attempts must be at least 1.");

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="attempts">The number of attempts made.</param>
    /// <param name="lastException">The exception thrown by the final attempt.</param>
    /// <returns>A failed <see cref="RetryResult{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="lastException"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="attempts"/> is less than 1.</exception>
    public static RetryResult<T> Failure(int attempts, Exception lastException)
    {
        ArgumentNullException.ThrowIfNull(lastException);
        return attempts >= 1
            ? new(false, default, attempts, lastException)
            : throw new ArgumentOutOfRangeException(nameof(attempts), attempts, "Attempts must be at least 1.");
    }
}
