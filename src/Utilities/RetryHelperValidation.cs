#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="RetryHelper"/> and related classes.
/// </summary>
public static class RetryHelperValidation
{
    /// <summary>
    /// Validates a <see cref="RetryHelper"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The retry helper instance to validate.</param>
    /// <returns>A list of validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RetryHelper value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates a <see cref="RetryPolicyBuilder"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="builder">The retry policy builder to validate.</param>
    /// <returns>A list of validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RetryPolicyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var problems = new List<string>();

        // Use reflection to access private fields since RetryPolicyBuilder doesn't expose them as properties
        var maxAttemptsField = typeof(RetryPolicyBuilder).GetField("_maxAttempts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var initialDelayMsField = typeof(RetryPolicyBuilder).GetField("_initialDelayMs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var maxDelayMsField = typeof(RetryPolicyBuilder).GetField("_maxDelayMs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (maxAttemptsField != null)
        {
            var maxAttempts = (int)maxAttemptsField.GetValue(builder)!;
            if (maxAttempts <= 0)
            {
                problems.Add($"MaxAttempts must be greater than 0, but was {maxAttempts}.");
            }
        }

        if (initialDelayMsField != null)
        {
            var initialDelayMs = (int)initialDelayMsField.GetValue(builder)!;
            if (initialDelayMs <= 0)
            {
                problems.Add($"InitialDelayMs must be greater than 0, but was {initialDelayMs}.");
            }
        }

        if (maxDelayMsField != null)
        {
            var maxDelayMs = (int)maxDelayMsField.GetValue(builder)!;
            if (maxDelayMs <= 0)
            {
                problems.Add($"MaxDelayMs must be greater than 0, but was {maxDelayMs}.");
            }

            if (initialDelayMsField != null)
            {
                var initialDelayMs = (int)initialDelayMsField.GetValue(builder)!;
                if (maxDelayMs < initialDelayMs)
                {
                    problems.Add($"MaxDelayMs ({maxDelayMs}) cannot be less than InitialDelayMs ({initialDelayMs}).");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="RetryPolicy"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="policy">The retry policy to validate.</param>
    /// <returns>A list of validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RetryPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var problems = new List<string>();

        if (policy.MaxAttempts <= 0)
        {
            problems.Add($"MaxAttempts must be greater than 0, but was {policy.MaxAttempts}.");
        }

        if (policy.InitialDelayMs <= 0)
        {
            problems.Add($"InitialDelayMs must be greater than 0, but was {policy.InitialDelayMs}.");
        }

        if (policy.MaxDelayMs <= 0)
        {
            problems.Add($"MaxDelayMs must be greater than 0, but was {policy.MaxDelayMs}.");
        }

        if (policy.MaxDelayMs < policy.InitialDelayMs)
        {
            problems.Add($"MaxDelayMs ({policy.MaxDelayMs}) cannot be less than InitialDelayMs ({policy.InitialDelayMs}).");
        }

        if (policy.RetryableExceptions == null)
        {
            problems.Add("RetryableExceptions list cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="RetryStatistics"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="stats">The retry statistics to validate.</param>
    /// <returns>A list of validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stats"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RetryStatistics stats)
    {
        ArgumentNullException.ThrowIfNull(stats);

        var problems = new List<string>();

        if (stats.TotalAttempts < 0)
        {
            problems.Add($"TotalAttempts cannot be negative, but was {stats.TotalAttempts}.");
        }

        if (stats.SuccessfulAttempts < 0)
        {
            problems.Add($"SuccessfulAttempts cannot be negative, but was {stats.SuccessfulAttempts}.");
        }

        if (stats.FailedAttempts < 0)
        {
            problems.Add($"FailedAttempts cannot be negative, but was {stats.FailedAttempts}.");
        }

        if (stats.TotalAttempts < stats.SuccessfulAttempts + stats.FailedAttempts)
        {
            problems.Add("TotalAttempts cannot be less than the sum of SuccessfulAttempts and FailedAttempts.");
        }

        if (stats.SuccessRate < 0 || stats.SuccessRate > 100)
        {
            problems.Add($"SuccessRate must be between 0 and 100, but was {stats.SuccessRate:F2}.");
        }

        if (stats.Events == null)
        {
            problems.Add("Events list cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="RetryEvent"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="e">The retry event to validate.</param>
    /// <returns>A list of validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="e"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RetryEvent e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var problems = new List<string>();

        if (e.Timestamp == default)
        {
            problems.Add("Timestamp cannot be default(DateTime).");
        }

        if (e.DelayMs < 0)
        {
            problems.Add($"DelayMs cannot be negative, but was {e.DelayMs}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="RetryPolicyBuilder"/> instance is valid.
    /// </summary>
    /// <param name="builder">The retry policy builder to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static bool IsValid(this RetryPolicyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return Validate(builder).Count == 0;
    }

    /// <summary>
    /// Determines whether a <see cref="RetryPolicy"/> instance is valid.
    /// </summary>
    /// <param name="policy">The retry policy to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this RetryPolicy policy)
    {
        return Validate(policy).Count == 0;
    }

    /// <summary>
    /// Determines whether a <see cref="RetryStatistics"/> instance is valid.
    /// </summary>
    /// <param name="stats">The retry statistics to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this RetryStatistics stats)
    {
        return Validate(stats).Count == 0;
    }

    /// <summary>
    /// Determines whether a <see cref="RetryEvent"/> instance is valid.
    /// </summary>
    /// <param name="e">The retry event to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this RetryEvent e)
    {
        return Validate(e).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="RetryPolicyBuilder"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="builder">The retry policy builder to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the builder is invalid.</exception>
    public static void EnsureValid(this RetryPolicyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var problems = Validate(builder);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "RetryPolicyBuilder is invalid. " + string.Join(" ", problems),
                nameof(builder));
        }
    }

    /// <summary>
    /// Ensures that a <see cref="RetryPolicy"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="policy">The retry policy to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the policy is invalid.</exception>
    public static void EnsureValid(this RetryPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var problems = Validate(policy);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "RetryPolicy is invalid. " + string.Join(" ", problems),
                nameof(policy));
        }
    }

    /// <summary>
    /// Ensures that a <see cref="RetryStatistics"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="stats">The retry statistics to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stats"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the statistics are invalid.</exception>
    public static void EnsureValid(this RetryStatistics stats)
    {
        ArgumentNullException.ThrowIfNull(stats);

        var problems = Validate(stats);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "RetryStatistics is invalid. " + string.Join(" ", problems),
                nameof(stats));
        }
    }

    /// <summary>
    /// Ensures that a <see cref="RetryEvent"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="e">The retry event to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="e"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the event is invalid.</exception>
    public static void EnsureValid(this RetryEvent e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var problems = Validate(e);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "RetryEvent is invalid. " + string.Join(" ", problems),
                nameof(e));
        }
    }
}