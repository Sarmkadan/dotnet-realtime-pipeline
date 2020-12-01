namespace DotNetRealtimePipeline.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Validation helpers for <see cref="SlidingWindowAggregator"/>.
/// </summary>
public static class SlidingWindowAggregatorValidation
{
    /// <summary>
    /// Validates the given <paramref name="value"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The <see cref="SlidingWindowAggregator"/> to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    public static IReadOnlyList<string> Validate(this SlidingWindowAggregator value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.WindowSizeMs <= 0)
            problems.Add("WindowSizeMs must be greater than zero.");
        if (value.StepIntervalMs <= 0)
            problems.Add("StepIntervalMs must be greater than zero.");
        if (value.StepIntervalMs > value.WindowSizeMs)
            problems.Add("StepIntervalMs cannot exceed WindowSizeMs.");

        return problems;
    }

    /// <summary>
    /// Checks if the given <paramref name="value"/> is valid.
    /// </summary>
    /// <param name="value">The <see cref="SlidingWindowAggregator"/> to check.</param>
    /// <returns>True if the <paramref name="value"/> is valid, false otherwise.</returns>
    public static bool IsValid(this SlidingWindowAggregator value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return !Validate(value).Any();
    }

    /// <summary>
    /// Ensures that the given <paramref name="value"/> is valid.
    /// </summary>
    /// <param name="value">The <see cref="SlidingWindowAggregator"/> to ensure is valid.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="value"/> is invalid.</exception>
    public static void EnsureValid(this SlidingWindowAggregator value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Any())
            throw new ArgumentException($"The following problems were found: {string.Join(", ", problems)}");
    }
}
