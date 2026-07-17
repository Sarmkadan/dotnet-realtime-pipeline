#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Provides validation helpers for <see cref="WindowingService"/> instances.
/// </summary>
public static class WindowingServiceValidation
{
    private const BindingFlags PrivateInstanceFlags = BindingFlags.NonPublic | BindingFlags.Instance;

    /// <summary>
    /// Validates a <see cref="WindowingService"/> instance and returns any validation problems.
    /// </summary>
    /// <param name="value">The <see cref="WindowingService"/> to validate.</param>
    /// <returns>A read-only list of validation problem descriptions. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WindowingService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate internal state consistency
        // WindowingService should always have a valid config
        var configField = value.GetType().GetField("_config", PrivateInstanceFlags);
        if (configField?.GetValue(value) is null)
        {
            problems.Add("WindowingService configuration cannot be null.");
        }

        // Validate window tracking state
        var activeWindowsField = value.GetType().GetField("_activeWindows", PrivateInstanceFlags);
        if (activeWindowsField is not null)
        {
            var activeWindows = activeWindowsField.GetValue(value) as System.Collections.IDictionary;
            if (activeWindows is not null && activeWindows.Count < 0)
            {
                problems.Add("Active windows count cannot be negative.");
            }
        }

        // Validate next window ID
        var nextWindowIdField = value.GetType().GetField("_nextWindowId", PrivateInstanceFlags);
        if (nextWindowIdField is not null)
        {
            var nextWindowId = (long)nextWindowIdField.GetValue(value)!;
            if (nextWindowId < 0)
            {
                problems.Add($"Next window ID must be non-negative, but was {nextWindowId}.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="WindowingService"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="WindowingService"/> to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this WindowingService value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that a <see cref="WindowingService"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The <see cref="WindowingService"/> to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this WindowingService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"WindowingService validation failed with {problems.Count} problem(s):{Environment.NewLine}- ".Replace("- ", string.Empty) +
            string.Join(Environment.NewLine + "- ", problems),
            nameof(value));
    }
}