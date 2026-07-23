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
/// An <see cref="IRetryPolicy"/> that retries transient failures with exponential
/// backoff and random jitter: delay = min(baseDelay * 2^(attempt-1), maxDelay) ± jitter.
/// </summary>
public sealed class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private readonly RetryPolicyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExponentialBackoffRetryPolicy"/> class.
    /// </summary>
    /// <param name="options">The retry configuration; when omitted, defaults are used.</param>
    public ExponentialBackoffRetryPolicy(RetryPolicyOptions? options = null)
        => _options = options ?? new RetryPolicyOptions();

    /// <inheritdoc/>
    public int MaxAttempts => _options.MaxAttempts;

    /// <inheritdoc/>
    public bool IsTransient(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return _options.TransientExceptionPredicate(exception);
    }

    /// <inheritdoc/>
    public TimeSpan GetBackoffDelay(int attemptNumber)
    {
        if (attemptNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(attemptNumber), attemptNumber, "Attempt number must be at least 1.");

        var exponent = Math.Min(attemptNumber - 1, 20); // avoid overflow for large attempt numbers
        var baseMs = _options.BaseDelay.TotalMilliseconds * Math.Pow(2, exponent);
        var cappedMs = Math.Min(baseMs, _options.MaxDelay.TotalMilliseconds);

        if (_options.JitterFactor > 0 && cappedMs > 0)
        {
            var jitter = cappedMs * _options.JitterFactor * ((Random.Shared.NextDouble() * 2.0) - 1.0);
            cappedMs = Math.Max(0, cappedMs + jitter);
        }

        return TimeSpan.FromMilliseconds(cappedMs);
    }

    /// <inheritdoc/>
    public async Task<RetryResult<T>> ExecuteAsync<T>(Func<int, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Exception lastException = null!;
        var attempt = 0;

        while (attempt < _options.MaxAttempts)
        {
            attempt++;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var value = await operation(attempt).ConfigureAwait(false);
                return RetryResult<T>.Success(value, attempt);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (!IsTransient(ex) || attempt >= _options.MaxAttempts)
                    return RetryResult<T>.Failure(attempt, ex);

                var delay = GetBackoffDelay(attempt);
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }

        return RetryResult<T>.Failure(attempt, lastException);
    }
}
