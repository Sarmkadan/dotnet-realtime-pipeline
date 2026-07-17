#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Text.Json;

/// <summary>
/// Provides System.Text.Json serialization extensions for DateTime-related types.
/// </summary>
public static class DateTimeExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes a DateTime to a JSON string using Unix milliseconds representation.
    /// </summary>
    /// <param name="dateTime">The DateTime to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the DateTime as Unix milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dateTime"/> is null.</exception>
    public static string ToJson(this DateTime dateTime, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(dateTime);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(dateTime.ToUnixMilliseconds(), options);
    }

    /// <summary>
    /// Serializes a DateTimeOffset to a JSON string using Unix milliseconds representation.
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the DateTimeOffset as Unix milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dateTimeOffset"/> is null.</exception>
    public static string ToJson(this DateTimeOffset dateTimeOffset, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(dateTimeOffset);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(dateTimeOffset.ToUnixTimeMilliseconds(), options);
    }

    /// <summary>
    /// Deserializes a JSON string to a DateTime.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A DateTime instance parsed from Unix milliseconds, or null if the JSON is null, empty, or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static DateTime? FromJsonToDateTime(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        long milliseconds = JsonSerializer.Deserialize<long>(json, _jsonOptions);
        return DateTimeExtensions.FromUnixMilliseconds(milliseconds);
    }

    /// <summary>
    /// Deserializes a JSON string to a DateTimeOffset.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A DateTimeOffset instance parsed from Unix milliseconds, or null if the JSON is null, empty, or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static DateTimeOffset? FromJsonToDateTimeOffset(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        long milliseconds = JsonSerializer.Deserialize<long>(json, _jsonOptions);
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a DateTime.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized DateTime instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJsonToDateTime(string json, out DateTime? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        try
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                long milliseconds = JsonSerializer.Deserialize<long>(json, _jsonOptions);
                value = DateTimeExtensions.FromUnixMilliseconds(milliseconds);
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a DateTimeOffset.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized DateTimeOffset instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJsonToDateTimeOffset(string json, out DateTimeOffset? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        try
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                long milliseconds = JsonSerializer.Deserialize<long>(json, _jsonOptions);
                value = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a long Unix timestamp to a JSON string.
    /// </summary>
    /// <param name="milliseconds">The Unix timestamp in milliseconds to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the Unix timestamp.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="milliseconds"/> is negative.</exception>
    public static string ToJson(this long milliseconds, bool indented = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(milliseconds);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(milliseconds, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a long Unix timestamp.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A long Unix timestamp, or null if the JSON is null, empty, or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static long? FromJsonToUnixMilliseconds(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<long>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a long Unix timestamp.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized Unix timestamp, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJsonToUnixMilliseconds(string json, out long? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        try
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                value = JsonSerializer.Deserialize<long>(json, _jsonOptions);
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}