#nullable enable
using System;
using System.Collections.Generic;

namespace DotNetRealtimePipeline.Events;

/// <summary>
/// Provides validation helpers for <see cref="PipelineEventPublisher"/> instances.
/// </summary>
/// <remarks>
/// The <see cref="PipelineEventPublisher"/> class is stateless with respect to validation criteria.
/// All validation methods validate the input parameters rather than the publisher's internal state,
/// as the publisher itself has no mutable state that requires validation.
/// </remarks>
public static class PipelineEventPublisherValidation
{
    /// <summary>
    /// Validates a <see cref="PipelineEventPublisher"/> instance.
    /// </summary>
    /// <param name="value">The publisher instance to validate.</param>
    /// <returns>
    /// A read-only list of human-readable validation problems.
    /// Returns an empty list if the instance is valid (always the case for current implementation).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method currently returns an empty list because <see cref="PipelineEventPublisher"/>
    /// has no validation criteria on its internal state. All members are methods that perform
    /// actions or return counts rather than maintaining mutable state.
    /// </remarks>
    public static IReadOnlyList<string> Validate(this PipelineEventPublisher value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PipelineEventPublisher"/> instance is valid.
    /// </summary>
    /// <param name="value">The publisher instance to check.</param>
    /// <returns>
    /// <see langword="true"/> if the instance has no validation problems; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method returns <see langword="true"/> because <see cref="PipelineEventPublisher"/>
    /// has no validation criteria on its internal state.
    /// </remarks>
    public static bool IsValid(this PipelineEventPublisher value) =>
        value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="PipelineEventPublisher"/> instance is valid.
    /// </summary>
    /// <param name="value">The publisher instance to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the instance is invalid; the message contains a list of validation problems.
    /// </exception>
    /// <remarks>
    /// This method validates the publisher instance and throws if any validation problems are found.
    /// Currently, no problems are ever found as <see cref="PipelineEventPublisher"/> has no validation criteria.
    /// </remarks>
    public static void EnsureValid(this PipelineEventPublisher value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PipelineEventPublisher is invalid. Problems:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}");
        }
    }
}