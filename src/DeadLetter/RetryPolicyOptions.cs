#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.DeadLetter;

using DotNetRealtimePipeline.Domain.Exceptions;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;

/// <summary>
/// Configures how <see cref="ExponentialBackoffRetryPolicy"/> retries a failing
/// operation before the item is handed to the dead-letter queue.
/// </summary>
public sealed class RetryPolicyOptions
{
    private int _maxAttempts = 3;
    private TimeSpan _baseDelay = TimeSpan.FromMilliseconds(200);
    private TimeSpan _maxDelay = TimeSpan.FromSeconds(30);
    private double _jitterFactor = 0.25;

    /// <summary>
    /// Gets or sets the maximum number of attempts (including the first one).
    /// Defaults to 3.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is less than 1.</exception>
    public int MaxAttempts
    {
        get => _maxAttempts;
        set => _maxAttempts = value >= 1
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "MaxAttempts must be at least 1.");
    }

    /// <summary>
    /// Gets or sets the delay before the second attempt; each subsequent attempt doubles it.
    /// Defaults to 200 ms.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    public TimeSpan BaseDelay
    {
        get => _baseDelay;
        set => _baseDelay = value >= TimeSpan.Zero
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "BaseDelay cannot be negative.");
    }

    /// <summary>
    /// Gets or sets the upper bound applied to the computed backoff delay.
    /// Defaults to 30 seconds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    public TimeSpan MaxDelay
    {
        get => _maxDelay;
        set => _maxDelay = value >= TimeSpan.Zero
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "MaxDelay cannot be negative.");
    }

    /// <summary>
    /// Gets or sets the proportion of random jitter applied to each delay
    /// (0 = none, 0.25 = up to ±25% of the computed delay). Defaults to 0.25.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is outside [0, 1].</exception>
    public double JitterFactor
    {
        get => _jitterFactor;
        set => _jitterFactor = value is >= 0.0 and <= 1.0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "JitterFactor must be between 0 and 1.");
    }

    /// <summary>
    /// Gets or sets the predicate that decides whether an exception is transient
    /// and therefore worth retrying. Defaults to <see cref="DefaultTransientPredicate"/>.
    /// </summary>
    public Func<Exception, bool> TransientExceptionPredicate { get; set; } = DefaultTransientPredicate;

    /// <summary>
    /// The default transient-exception classification: timeouts, I/O, socket and
    /// HTTP transport failures are treated as transient; everything else is permanent.
    /// </summary>
    /// <param name="exception">The exception to classify.</param>
    /// <returns><see langword="true"/> when the exception is considered transient.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static bool DefaultTransientPredicate(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            TimeoutException or IOException or SocketException or HttpRequestException => true,
            OperationCanceledException => false,
            AggregateException aggregate => aggregate.InnerException is { } inner && DefaultTransientPredicate(inner),
            // A stage-processing failure is transient only when the underlying cause is;
            // business-rule rejections (e.g. failed validation) carry no inner exception
            // and are therefore treated as permanent.
            PipelineProcessingException processing => processing.Result.Exception is { } cause && DefaultTransientPredicate(cause),
            _ => false
        };
    }
}
