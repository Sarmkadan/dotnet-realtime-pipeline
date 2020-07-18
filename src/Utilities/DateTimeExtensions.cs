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
    public static long ToUnixMilliseconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Converts Unix milliseconds to DateTime.
    /// </summary>
    public static DateTime FromUnixMilliseconds(long milliseconds)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
    }

    /// <summary>
    /// Gets the current Unix timestamp in milliseconds.
    /// </summary>
    public static long GetCurrentUnixMilliseconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Gets the start of a time window for a given timestamp.
    /// </summary>
    public static long GetWindowStart(long timestampMs, long windowSizeMs)
    {
        return (timestampMs / windowSizeMs) * windowSizeMs;
    }

    /// <summary>
    /// Gets the end of a time window for a given timestamp.
    /// </summary>
    public static long GetWindowEnd(long timestampMs, long windowSizeMs)
    {
        return GetWindowStart(timestampMs, windowSizeMs) + windowSizeMs;
    }

    /// <summary>
    /// Calculates the age of a timestamp in milliseconds.
    /// </summary>
    public static long GetAgeMs(long timestampMs)
    {
        return GetCurrentUnixMilliseconds() - timestampMs;
    }

    /// <summary>
    /// Rounds a timestamp to the nearest window boundary.
    /// </summary>
    public static long RoundToWindowBoundary(long timestampMs, long windowSizeMs)
    {
        long remainder = timestampMs % windowSizeMs;
        return remainder < windowSizeMs / 2
            ? timestampMs - remainder
            : timestampMs + (windowSizeMs - remainder);
    }
}
