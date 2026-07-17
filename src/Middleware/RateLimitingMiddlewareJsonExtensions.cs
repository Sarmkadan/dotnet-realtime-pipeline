#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Middleware;

using System;
using System.Text.Json;

/// <summary>
/// Provides JSON serialization helpers for <see cref="RateLimitingMiddleware"/>.
/// </summary>
public static class RateLimitingMiddlewareJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="RateLimitingMiddleware"/> instance to JSON.
    /// </summary>
    /// <param name="value">The <see cref="RateLimitingMiddleware"/> instance to serialize.</param>
    /// <param name="indented">If true, formats the JSON with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="RateLimitingMiddleware"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this RateLimitingMiddleware value, bool indented = false) =>
        JsonSerializer.Serialize(value, new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = indented });

    /// <summary>
    /// Deserializes a <see cref="RateLimitingMiddleware"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="RateLimitingMiddleware"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON parsing fails.</exception>
    public static RateLimitingMiddleware? FromJson(string json) =>
        JsonSerializer.Deserialize<RateLimitingMiddleware>(json, _jsonSerializerOptions);

    /// <summary>
    /// Tries to deserialize a <see cref="RateLimitingMiddleware"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="RateLimitingMiddleware"/> instance, or null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out RateLimitingMiddleware? value)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(json);
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