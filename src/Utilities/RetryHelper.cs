#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Helper class for implementing retry logic with various strategies.
/// Supports exponential backoff, jitter, and custom retry conditions.
/// </summary>
public sealed class RetryHelper
{
    /// <summary>
    /// Default maximum number of attempts.
    /// </summary>
    private const int DefaultMaxAttempts = 3;

    /// <summary>
    /// Default initial delay between retries in milliseconds.
    /// </summary>
    private const int DefaultInitialDelayMs = 100;

    /// <summary>
    /// Backoff multiplier for exponential delay.
    /// </summary>
    private const int BackoffMultiplier = 2;

    /// <summary>
    /// Default maximum delay between retries in milliseconds.
    /// </summary>
    private const int DefaultMaxDelayMs = 30000;

    /// <summary>
    /// Gets or sets the maximum number of attempts.
    /// </summary>
    public int MaxAttempts { get; set; } = DefaultMaxAttempts;

    /// <summary>
    /// Gets or sets the initial delay between retries in milliseconds.
    /// </summary>
    public int InitialDelayMs { get; set; } = DefaultInitialDelayMs;

    /// <summary>
    /// Gets or sets the maximum delay between retries in milliseconds.
    /// </summary>
    public int MaxDelayMs { get; set; } = DefaultMaxDelayMs;

    /// <summary>
    /// Gets or sets whether to use jitter in backoff.
    /// </summary>
    public bool UseJitter { get; set; } = false;

    /// <summary>
    /// Gets or sets the list of exception types that are retryable.
    /// </summary>
    public List<Type> RetryableExceptions { get; set; } = new()
    {
        typeof(TimeoutException),
        typeof(HttpRequestException),
        typeof(InvalidOperationException)
    };

    /// <summary>
    /// Gets or sets the total number of attempts made.
    /// </summary>
    public int TotalAttempts { get; set; } = 0;

    /// <summary>
    /// Returns a string representation of the retry helper configuration.
    /// </summary>
    public override string ToString() => $"RetryHelper {{ MaxAttempts = {MaxAttempts}, InitialDelayMs = {InitialDelayMs}, MaxDelayMs = {MaxDelayMs}, UseJitter = {UseJitter}, RetryableExceptions = {RetryableExceptions}, TotalAttempts = {TotalAttempts} }}";

    /// <summary>
    /// Executes an operation with exponential backoff retry strategy.
    /// </summary>
    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxAttempts = DefaultMaxAttempts,
        int initialDelayMs = DefaultInitialDelayMs,
        Func<Exception, bool> shouldRetry = null)
    {
        var attempt = 0;
        var delay = initialDelayMs;

        while (true)
        {
            try
            {
                return await operation();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (attempt < maxAttempts - 1 && (shouldRetry?.Invoke(ex) ?? IsRetryableException(ex)))
            {
                attempt++;
                await Task.Delay(delay);
                delay = (int)Math.Min(delay * BackoffMultiplier, DefaultMaxDelayMs);
            }
        }
    }

    /// <summary>
    /// Executes a synchronous operation with retry logic.
    /// </summary>
    public static T Retry<T>(
        Func<T> operation,
        int maxAttempts = DefaultMaxAttempts,
        int initialDelayMs = DefaultInitialDelayMs,
        Func<Exception, bool> shouldRetry = null)
    {
        var attempt = 0;
        var delay = initialDelayMs;

        while (true)
        {
            try
            {
                return operation();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (attempt < maxAttempts - 1 && (shouldRetry?.Invoke(ex) ?? IsRetryableException(ex)))
            {
                attempt++;
                System.Threading.Thread.Sleep(delay);
                delay = (int)Math.Min(delay * BackoffMultiplier, DefaultMaxDelayMs);
            }
        }
    }

    /// <summary>
    /// Determines if an exception is retryable.
    /// </summary>
    private static bool IsRetryableException(Exception ex)
    {
        return ex is TimeoutException
            or HttpRequestException
            or InvalidOperationException;
    }
}

/// <summary>
/// Builder for fluent retry configuration.
/// </summary>
public sealed class RetryPolicyBuilder
{
    /// <summary>
    /// Default maximum number of attempts.
    /// </summary>
    private const int DefaultMaxAttempts = 3;

    /// <summary>
    /// Default initial delay between retries in milliseconds.
    /// </summary>
    private const int DefaultInitialDelayMs = 100;

    /// <summary>
    /// Default maximum delay between retries in milliseconds.
    /// </summary>
    private const int DefaultMaxDelayMs = 30000;

    /// <summary>
    /// Default jitter enabled state.
    /// </summary>
    private const bool DefaultUseJitter = true;

    /// <summary>
    /// Backoff multiplier for exponential delay.
    /// </summary>
    private const int BackoffMultiplier = 2;

    /// <summary>
    /// Jitter range multiplier (0.5 to 1.0 of base delay).
    /// </summary>
    private const double JitterMinMultiplier = 0.5;
    private const double JitterMaxMultiplier = 1.0;

    private int _maxAttempts = DefaultMaxAttempts;
    private int _initialDelayMs = DefaultInitialDelayMs;
    private int _maxDelayMs = DefaultMaxDelayMs;
    private bool _useJitter = DefaultUseJitter;
    private List<Type> _retryableExceptions = new();

    /// <summary>
    /// Sets maximum number of attempts.
    /// </summary>
    public RetryPolicyBuilder WithMaxAttempts(int attempts)
    {
        _maxAttempts = attempts;
        return this;
    }

    /// <summary>
    /// Sets initial delay between retries.
    /// </summary>
    public RetryPolicyBuilder WithInitialDelay(int delayMs)
    {
        _initialDelayMs = delayMs;
        return this;
    }

    /// <summary>
    /// Sets maximum delay between retries.
    /// </summary>
    public RetryPolicyBuilder WithMaxDelay(int delayMs)
    {
        _maxDelayMs = delayMs;
        return this;
    }

    /// <summary>
    /// Enables or disables jitter in backoff.
    /// </summary>
    public RetryPolicyBuilder WithJitter(bool enabled)
    {
        _useJitter = enabled;
        return this;
    }

    /// <summary>
    /// Adds an exception type to retry on.
    /// </summary>
    public RetryPolicyBuilder RetryOn<TException>() where TException : Exception
    {
        _retryableExceptions.Add(typeof(TException));
        return this;
    }

    /// <summary>
    /// Builds the retry policy.
    /// </summary>
    public RetryPolicy Build()
    {
        return new RetryPolicy
        {
            MaxAttempts = _maxAttempts,
            InitialDelayMs = _initialDelayMs,
            MaxDelayMs = _maxDelayMs,
            UseJitter = _useJitter,
            RetryableExceptions = _retryableExceptions
        };
    }
}

/// <summary>
/// Represents a retry policy.
/// </summary>
public sealed class RetryPolicy
{
    /// <summary>
    /// Backoff multiplier for exponential delay.
    /// </summary>
    private const int BackoffMultiplier = 2;

    /// <summary>
    /// Jitter minimum multiplier (0.5 of base delay).
    /// </summary>
    private const double JitterMinMultiplier = 0.5;

    /// <summary>
    /// Jitter maximum multiplier (1.0 of base delay).
    /// </summary>
    private const double JitterMaxMultiplier = 1.0;

    public int MaxAttempts { get; set; }
    public int InitialDelayMs { get; set; }
    public int MaxDelayMs { get; set; }
    public bool UseJitter { get; set; }
    public List<Type> RetryableExceptions { get; set; } = new();

    /// <summary>
    /// Executes an operation with this retry policy.
    /// </summary>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        var attempt = 0;
        var delay = InitialDelayMs;

        while (true)
        {
            try
            {
                return await operation();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (attempt < MaxAttempts - 1 && IsRetryableException(ex))
            {
                attempt++;
                delay = (int)Math.Min((long)delay * BackoffMultiplier, MaxDelayMs);

                if (UseJitter)
                {
                    delay = (int)(delay * (JitterMinMultiplier + Random.Shared.NextDouble() * (JitterMaxMultiplier - JitterMinMultiplier)));
                }

                await Task.Delay(delay);
            }
        }
    }

    private bool IsRetryableException(Exception ex)
    {
        if (RetryableExceptions.Count == 0)
            return true;

        return RetryableExceptions.Any(t => t.IsAssignableFrom(ex.GetType()));
    }
}

/// <summary>
/// Retry statistics and monitoring.
/// </summary>
public sealed class RetryStatistics
{
    public int TotalAttempts { get; set; }
    public int SuccessfulAttempts { get; set; }
    public int FailedAttempts { get; set; }
    public double SuccessRate => TotalAttempts > 0 ? (SuccessfulAttempts * 100.0) / TotalAttempts : 0;
    public List<RetryEvent> Events { get; set; } = new();

    public void RecordAttempt(bool success, int delayMs)
    {
        TotalAttempts++;
        if (success)
            SuccessfulAttempts++;
        else
            FailedAttempts++;

        Events.Add(new RetryEvent
        {
            Timestamp = DateTime.UtcNow,
            Success = success,
            DelayMs = delayMs
        });
    }
}

public sealed class RetryEvent
{
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public int DelayMs { get; set; }
}
