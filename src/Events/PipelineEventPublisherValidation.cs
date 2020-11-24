#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetRealtimePipeline.Events;

/// <summary>
/// Provides validation helpers for <see cref="PipelineEventPublisher"/>.
/// </summary>
public static class PipelineEventPublisherValidation
{
    /// <summary>
    /// Validates a <see cref="PipelineEventPublisher"/> instance and returns a read‑only list of human‑readable problems.
    /// </summary>
    /// <param name="value">The publisher to validate.</param>
    /// <returns>A read‑only list of validation problems; empty if the instance is considered valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this PipelineEventPublisher value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // The current public API of <see cref="PipelineEventPublisher"/> does not expose any state
        // that can be validated. All members are methods that either perform actions or return
        // counts. Therefore, there are no validation problems to report.
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether a <see cref="PipelineEventPublisher"/> instance is valid.
    /// </summary>
    /// <param name="value">The publisher to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this PipelineEventPublisher value) =>
        value.Validate().Count == 0;

    /// <summary>
    /// Ensures that a <see cref="PipelineEventPublisher"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// if any validation problems are found.
    /// </summary>
    /// <param name="value">The publisher to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid; the message contains a list of problems.</exception>
    public static void EnsureValid(this PipelineEventPublisher value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                string.Create(CultureInfo.InvariantCulture,
                    $"PipelineEventPublisher is invalid. Problems:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}"));
        }
    }
}
