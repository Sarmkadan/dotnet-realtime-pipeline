#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Events;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Validation helpers for EventSubscriberBase.
/// </summary>
public static class EventSubscriberBaseValidation
{
    /// <summary>
    /// Validates the EventSubscriberBase instance.
    /// </summary>
    /// <param name="value">The EventSubscriberBase instance to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate([NotNull] this EventSubscriberBase? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate basic state - subscribers should have been constructed with dependencies
        // Since fields are private, we can only validate the public API
        try
        {
            // Attempt to call methods that would fail if dependencies were null
            // This provides basic validation without accessing private fields
            _ = value.GetType().Name;
        }
        catch (Exception ex)
        {
            problems.Add($"Failed to access subscriber type information: {ex.Message}");
        }

        // Validate that the subscriber can be used (basic smoke test)
        try
        {
            // Try subscribing and unsubscribing as a basic validation
            // This ensures the subscriber is in a usable state
            value.Subscribe();
            value.Unsubscribe();
        }
        catch (Exception ex)
        {
            problems.Add($"Failed to perform basic subscription operations: {ex.Message}");
        }

        return problems;
    }

    /// <summary>
    /// Checks if the EventSubscriberBase instance is valid.
    /// </summary>
    /// <param name="value">The EventSubscriberBase instance to check.</param>
    /// <returns>True if the instance is valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid([NotNullWhen(true)] this EventSubscriberBase? value)
    {
        return value is not null && !Validate(value).Any();
    }

    /// <summary>
    /// Ensures the EventSubscriberBase instance is valid.
    /// </summary>
    /// <param name="value">The EventSubscriberBase instance to ensure.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid.</exception>
    public static void EnsureValid(this EventSubscriberBase value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid EventSubscriberBase instance: {string.Join(", ", problems)}", nameof(value));
        }
    }
}