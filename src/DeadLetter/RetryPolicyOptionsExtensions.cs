#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.DeadLetter;

using DotNetRealtimePipeline.Domain.Exceptions;
using System;

/// <summary>
/// Extension methods for <see cref="RetryPolicyOptions"/> to facilitate cloning and configuration.
/// </summary>
public static class RetryPolicyOptionsExtensions
{
    /// <summary>
    /// Creates a deep copy of the <see cref="RetryPolicyOptions"/> instance.
    /// </summary>
    /// <param name="o">The options to clone.</param>
    /// <returns>A new <see cref="RetryPolicyOptions"/> with the same property values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="o"/> is <see langword="null"/>.</exception>
    public static RetryPolicyOptions Clone(this RetryPolicyOptions o)
    {
        ArgumentNullException.ThrowIfNull(o);

        return new RetryPolicyOptions
        {
            MaxAttempts = o.MaxAttempts,
            BaseDelay = o.BaseDelay,
            MaxDelay = o.MaxDelay,
            JitterFactor = o.JitterFactor,
            TransientExceptionPredicate = o.TransientExceptionPredicate
        };
    }

    /// <summary>
    /// Creates a clone of the specified <see cref="RetryPolicyOptions"/> and sets the <see cref="RetryPolicyOptions.MaxAttempts"/> property.
    /// </summary>
    /// <param name="o">The options to clone and modify.</param>
    /// <param name="n">The maximum number of attempts (including the first one). Must be at least 1.</param>
    /// <returns>A new <see cref="RetryPolicyOptions"/> with the specified <see cref="RetryPolicyOptions.MaxAttempts"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="o"/> is <see langword="null"/>.</exception>
    public static RetryPolicyOptions WithMaxAttempts(this RetryPolicyOptions o, int n)
    {
        ArgumentNullException.ThrowIfNull(o);

        var clone = o.Clone();
        clone.MaxAttempts = n;
        return clone;
    }

    /// <summary>
    /// Creates a clone of the specified <see cref="RetryPolicyOptions"/> and sets the <see cref="RetryPolicyOptions.BaseDelay"/> property.
    /// </summary>
    /// <param name="o">The options to clone and modify.</param>
    /// <param name="delay">The delay before the second attempt; each subsequent attempt doubles it. Cannot be negative.</param>
    /// <returns>A new <see cref="RetryPolicyOptions"/> with the specified <see cref="RetryPolicyOptions.BaseDelay"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="o"/> is <see langword="null"/>.</exception>
    public static RetryPolicyOptions WithBaseDelay(this RetryPolicyOptions o, TimeSpan delay)
    {
        ArgumentNullException.ThrowIfNull(o);

        var clone = o.Clone();
        clone.BaseDelay = delay;
        return clone;
    }

    /// <summary>
    /// Creates a clone of the specified <see cref="RetryPolicyOptions"/> and sets the <see cref="RetryPolicyOptions.MaxDelay"/> property.
    /// </summary>
    /// <param name="o">The options to clone and modify.</param>
    /// <param name="delay">The upper bound applied to the computed backoff delay. Cannot be negative.</param>
    /// <returns>A new <see cref="RetryPolicyOptions"/> with the specified <see cref="RetryPolicyOptions.MaxDelay"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="o"/> is <see langword="null"/>.</exception>
    public static RetryPolicyOptions WithMaxDelay(this RetryPolicyOptions o, TimeSpan delay)
    {
        ArgumentNullException.ThrowIfNull(o);

        var clone = o.Clone();
        clone.MaxDelay = delay;
        return clone;
    }

    /// <summary>
    /// Creates a clone of the specified <see cref="RetryPolicyOptions"/> and sets the <see cref="RetryPolicyOptions.JitterFactor"/> property.
    /// </summary>
    /// <param name="o">The options to clone and modify.</param>
    /// <param name="factor">The proportion of random jitter applied to each delay (0 = none, 0.25 = up to ±25% of the computed delay). Must be between 0 and 1.</param>
    /// <returns>A new <see cref="RetryPolicyOptions"/> with the specified <see cref="RetryPolicyOptions.JitterFactor"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="o"/> is <see langword="null"/>.</exception>
    public static RetryPolicyOptions WithJitter(this RetryPolicyOptions o, double factor)
    {
        ArgumentNullException.ThrowIfNull(o);

        var clone = o.Clone();
        clone.JitterFactor = factor;
        return clone;
    }

    /// <summary>
    /// Creates a clone of the specified <see cref="RetryPolicyOptions"/> and disables jitter by setting <see cref="RetryPolicyOptions.JitterFactor"/> to 0.
    /// </summary>
    /// <param name="o">The options to clone and modify.</param>
    /// <returns>A new <see cref="RetryPolicyOptions"/> with jitter disabled.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="o"/> is <see langword="null"/>.</exception>
    public static RetryPolicyOptions WithoutJitter(this RetryPolicyOptions o)
    {
        ArgumentNullException.ThrowIfNull(o);

        var clone = o.Clone();
        clone.JitterFactor = 0.0;
        return clone;
    }

    /// <summary>
    /// Builds an <see cref="ExponentialBackoffRetryPolicy"/> from the specified <see cref="RetryPolicyOptions"/>.
    /// </summary>
    /// <param name="o">The options to use for building the policy.</param>
    /// <returns>A new <see cref="ExponentialBackoffRetryPolicy"/> instance configured with the specified options.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="o"/> is <see langword="null"/>.</exception>
    public static ExponentialBackoffRetryPolicy BuildPolicy(this RetryPolicyOptions o)
    {
        ArgumentNullException.ThrowIfNull(o);

        return new ExponentialBackoffRetryPolicy(o);
    }
}