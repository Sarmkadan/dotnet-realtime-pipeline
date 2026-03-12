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
    /// Executes an operation with exponential backoff retry strategy.
    /// </summary>
    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxAttempts = 3,
        int initialDelayMs = 100,
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
            catch (Exception ex) when (attempt < maxAttempts - 1 && (shouldRetry?.Invoke(ex) ?? IsRetryableException(ex)))
            {
                attempt++;
                await Task.Delay(delay);
                delay = (int)Math.Min(delay * 2, 30000); // Cap at 30 seconds
            }
        }
    }

    /// <summary>
    /// Executes a synchronous operation with retry logic.
    /// </summary>
    public static T Retry<T>(
        Func<T> operation,
        int maxAttempts = 3,
        int initialDelayMs = 100,
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
            catch (Exception ex) when (attempt < maxAttempts - 1 && (shouldRetry?.Invoke(ex) ?? IsRetryableException(ex)))
            {
                attempt++;
                System.Threading.Thread.Sleep(delay);
                delay = (int)Math.Min(delay * 2, 30000);
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
            or OperationCanceledException
            or InvalidOperationException;
    }
}

/// <summary>
/// Builder for fluent retry configuration.
/// </summary>
public sealed class RetryPolicyBuilder
{
    private int _maxAttempts = 3;
    private int _initialDelayMs = 100;
    private int _maxDelayMs = 30000;
    private bool _useJitter = true;
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
        var random = new Random();

        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < MaxAttempts - 1 && IsRetryableException(ex))
            {
                attempt++;

                if (UseJitter)
                {
                    delay = (int)(delay * (0.5 + random.NextDouble() * 0.5));
                }

                delay = Math.Min(delay * 2, MaxDelayMs);
                await Task.Delay(delay);
            }
        }
    }

    private bool IsRetryableException(Exception ex)
    {
        if (RetryableExceptions.Count == 0)
            return true;

        return RetryableExceptions.Any(t => t.IsInstanceOfType(ex));
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
