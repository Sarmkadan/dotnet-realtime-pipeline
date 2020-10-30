#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Middleware;

using System;
using System.Text.Json;

/// <summary>
/// Provides JSON serialization helpers for <see cref="LoggingMiddleware"/>.
/// </summary>
public static class LoggingMiddlewareJsonExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="LoggingMiddleware"/> instance to JSON.
    /// </summary>
    /// <param name="value">The <see cref="LoggingMiddleware"/> instance to serialize.</param>
    /// <param name="indented">If true, formats the JSON with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="LoggingMiddleware"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this LoggingMiddleware value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(JsonSerializerOptions);
        options.WriteIndented = indented;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="LoggingMiddleware"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="LoggingMiddleware"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON parsing fails.</exception>
    public static LoggingMiddleware? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<LoggingMiddleware>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Tries to deserialize a <see cref="LoggingMiddleware"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="LoggingMiddleware"/> instance, or null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out LoggingMiddleware? value)
    {
        try
        {
            value = FromJson(json);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
