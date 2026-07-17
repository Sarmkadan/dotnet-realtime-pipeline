#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Validation helpers for <see cref="DateTimeExtensions"/> static class methods.
/// </summary>
public static class DateTimeExtensionsValidation
{
    /// <summary>
    /// Validates parameters for <see cref="DateTimeExtensions.ToUnixMilliseconds"/> method.
    /// </summary>
    /// <param name="dateTime">The DateTime value to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this DateTime dateTime)
    {
        var problems = new List<string>();

        if (dateTime < DateTime.UnixEpoch)
        {
            problems.Add($"DateTime must be on or after Unix epoch ({DateTime.UnixEpoch}), but was {dateTime}.");
        }

        return problems;
    }

    /// <summary>
    /// Validates parameters for <see cref="DateTimeExtensions.FromUnixMilliseconds"/> method.
    /// </summary>
    /// <param name="milliseconds">The Unix timestamp in milliseconds to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this long milliseconds)
    {
        var problems = new List<string>();

        if (milliseconds < 0)
        {
            problems.Add($"Milliseconds value must be non-negative, but was {milliseconds}.");
        }

        return problems;
    }

    /// <summary>
    /// Validates parameters for <see cref="DateTimeExtensions.GetWindowStart"/> and <see cref="DateTimeExtensions.GetWindowEnd"/> methods.
    /// </summary>
    /// <param name="timestampMs">The timestamp in milliseconds to validate.</param>
    /// <param name="windowSizeMs">The size of the time window in milliseconds to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this long timestampMs, long windowSizeMs)
    {
        var problems = new List<string>();

        if (windowSizeMs <= 0)
        {
            problems.Add($"Window size must be positive, but was {windowSizeMs}.");
        }

        return problems;
    }

    /// <summary>
    /// Validates parameters for <see cref="DateTimeExtensions.RoundToWindowBoundary"/> method.
    /// </summary>
    /// <param name="timestampMs">The timestamp in milliseconds to validate.</param>
    /// <param name="windowSizeMs">The size of the time window in milliseconds to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this long timestampMs, long windowSizeMs, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName);

        var problems = new List<string>();

        if (windowSizeMs <= 0)
        {
            problems.Add($"Window size must be positive, but was {windowSizeMs}.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether a DateTime value is valid for <see cref="DateTimeExtensions.ToUnixMilliseconds"/> method.
    /// </summary>
    /// <param name="dateTime">The DateTime value to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this DateTime dateTime) => Validate(dateTime).Count == 0;

    /// <summary>
    /// Determines whether a long milliseconds value is valid for time window operations.
    /// </summary>
    /// <param name="timestampMs">The timestamp in milliseconds to check.</param>
    /// <param name="windowSizeMs">The window size in milliseconds to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this long timestampMs, long windowSizeMs) => Validate(timestampMs, windowSizeMs).Count == 0;

    /// <summary>
    /// Determines whether a long milliseconds value is valid for window boundary rounding.
    /// </summary>
    /// <param name="timestampMs">The timestamp in milliseconds to check.</param>
    /// <param name="windowSizeMs">The window size in milliseconds to check.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this long timestampMs, long windowSizeMs, string paramName) => Validate(timestampMs, windowSizeMs, paramName).Count == 0;

    /// <summary>
    /// Ensures that a DateTime value is valid for <see cref="DateTimeExtensions.ToUnixMilliseconds"/> method.
    /// </summary>
    /// <param name="dateTime">The DateTime value to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the value is not valid.</exception>
    public static void EnsureValid(this DateTime dateTime)
    {
        var problems = Validate(dateTime);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DateTime is not valid for ToUnixMilliseconds:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that a long milliseconds value is valid for time window operations.
    /// </summary>
    /// <param name="timestampMs">The timestamp in milliseconds to validate.</param>
    /// <param name="windowSizeMs">The window size in milliseconds to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the value is not valid.</exception>
    public static void EnsureValid(this long timestampMs, long windowSizeMs)
    {
        var problems = Validate(timestampMs, windowSizeMs);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Timestamp and window size are not valid:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that a long milliseconds value is valid for window boundary rounding.
    /// </summary>
    /// <param name="timestampMs">The timestamp in milliseconds to validate.</param>
    /// <param name="windowSizeMs">The window size in milliseconds to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown if the value is not valid.</exception>
    public static void EnsureValid(this long timestampMs, long windowSizeMs, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName);

        var problems = Validate(timestampMs, windowSizeMs, paramName);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Timestamp and window size are not valid for {paramName}:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}");
        }
    }
}