#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Events;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
    public static IReadOnlyList<string> Validate(this EventSubscriberBase value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.GetType().GetMethods().Any(m => m.Name == "Subscribe" && m.GetParameters().Any(p => p.Name == "this")))
        {
            problems.Add("Subscribe method is not overridden");
        }

        if (value.GetType().GetMethods().Any(m => m.Name == "Unsubscribe" && m.GetParameters().Any(p => p.Name == "this")))
        {
            problems.Add("Unsubscribe method is not overridden");
        }

        if (value.GetType().GetMethods().Any(m => m.Name == "GetSuccessRatePercent" && m.GetParameters().Any(p => p.Name == "this")))
        {
            problems.Add("GetSuccessRatePercent method is not overridden");
        }

        if (value.GetType().GetMethods().Any(m => m.Name == "GetBackpressureEventCount" && m.GetParameters().Any(p => p.Name == "this")))
        {
            problems.Add("GetBackpressureEventCount method is not overridden");
        }

        if (value.GetType().GetMethods().Any(m => m.Name == "GetAverageProcessingTime" && m.GetParameters().Any(p => p.Name == "this")))
        {
            problems.Add("GetAverageProcessingTime method is not overridden");
        }

        if (value.GetType().GetMethods().Any(m => m.Name == "GetMetricsCount" && m.GetParameters().Any(p => p.Name == "this")))
        {
            problems.Add("GetMetricsCount method is not overridden");
        }

        if (value.GetType().GetMethods().Any(m => m.Name == "GetErrorCount" && m.GetParameters().Any(p => p.Name == "this")))
        {
            problems.Add("GetErrorCount method is not overridden");
        }

        return problems;
    }

    /// <summary>
    /// Checks if the EventSubscriberBase instance is valid.
    /// </summary>
    /// <param name="value">The EventSubscriberBase instance to check.</param>
    /// <returns>True if the instance is valid, false otherwise.</returns>
    public static bool IsValid(this EventSubscriberBase value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return !Validate(value).Any();
    }

    /// <summary>
    /// Ensures the EventSubscriberBase instance is valid.
    /// </summary>
    /// <param name="value">The EventSubscriberBase instance to ensure.</param>
    /// <exception cref="ArgumentException">If the instance is not valid.</exception>
    public static void EnsureValid(this EventSubscriberBase value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Any())
        {
            throw new ArgumentException($"Invalid EventSubscriberBase instance: {string.Join(", ", problems)}", nameof(value));
        }
    }
}
