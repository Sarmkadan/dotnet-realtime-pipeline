#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;

/// <summary>
/// Extension methods for DateTime and DateTimeOffset operations.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts a DateTime to Unix milliseconds.
    /// </summary>
    /// <param name="dateTime">The DateTime to convert. Must be a valid DateTime value.</param>
    /// <returns>The Unix timestamp in milliseconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the DateTime is outside the valid range for Unix timestamps.</exception>
    public static long ToUnixMilliseconds(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dateTime, DateTime.UnixEpoch, nameof(dateTime));
        return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Converts Unix milliseconds to DateTime.
    /// </summary>
    /// <param name="milliseconds">The Unix timestamp in milliseconds.</param>
    /// <returns>The corresponding DateTime.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the milliseconds value is outside the valid range for DateTime.</exception>
    public static DateTime FromUnixMilliseconds(long milliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(milliseconds, 0, nameof(milliseconds));
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
    }

    /// <summary>
    /// Gets the current Unix timestamp in milliseconds.
    /// </summary>
    /// <returns>The current Unix timestamp in milliseconds.</returns>
    public static long GetCurrentUnixMilliseconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Gets the start of a time window for a given timestamp.
    /// </summary>
    /// <param name="timestampMs">The timestamp in milliseconds.</param>
    /// <param name="windowSizeMs">The size of the time window in milliseconds.</param>
    /// <returns>The start of the time window.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when windowSizeMs is zero or negative.</exception>
    public static long GetWindowStart(long timestampMs, long windowSizeMs)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(windowSizeMs, 0, nameof(windowSizeMs));
        return (timestampMs / windowSizeMs) * windowSizeMs;
    }

    /// <summary>
    /// Gets the end of a time window for a given timestamp.
    /// </summary>
    /// <param name="timestampMs">The timestamp in milliseconds.</param>
    /// <param name="windowSizeMs">The size of the time window in milliseconds.</param>
    /// <returns>The end of the time window.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when windowSizeMs is zero or negative.</exception>
    public static long GetWindowEnd(long timestampMs, long windowSizeMs)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(windowSizeMs, 0, nameof(windowSizeMs));
        return GetWindowStart(timestampMs, windowSizeMs) + windowSizeMs;
    }

    /// <summary>
    /// Calculates the age of a timestamp in milliseconds.
    /// </summary>
    /// <param name="timestampMs">The timestamp in milliseconds to calculate age for.</param>
    /// <returns>The age in milliseconds.</returns>
    public static long GetAgeMs(long timestampMs)
    {
        return GetCurrentUnixMilliseconds() - timestampMs;
    }

    /// <summary>
    /// Rounds a timestamp to the nearest window boundary.
    /// </summary>
    /// <param name="timestampMs">The timestamp in milliseconds to round.</param>
    /// <param name="windowSizeMs">The size of the time window in milliseconds.</param>
    /// <returns>The rounded timestamp.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when windowSizeMs is zero or negative.</exception>
    public static long RoundToWindowBoundary(long timestampMs, long windowSizeMs)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(windowSizeMs, 0, nameof(windowSizeMs));
        long remainder = timestampMs % windowSizeMs;
        return remainder < windowSizeMs / 2
            ? timestampMs - remainder
            : timestampMs + (windowSizeMs - remainder);
    }
}