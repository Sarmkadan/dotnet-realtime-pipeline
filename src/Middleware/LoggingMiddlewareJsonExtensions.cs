#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Middleware;

using System;
using System.Text.Json;

/// <summary>
/// Provides JSON serialization helpers for logging-related models and data structures.
/// <para><see cref="LoggingMiddleware"/> instances cannot be serialized due to their dependency on <see cref="Microsoft.Extensions.Logging.ILogger"/>.</para>
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
    /// <exception cref="NotSupportedException">Thrown because <see cref="LoggingMiddleware"/> instances cannot be serialized.
    /// Middleware instances contain non-serializable dependencies such as <see cref="Microsoft.Extensions.Logging.ILogger"/>.</exception>
    public static string ToJson(this LoggingMiddleware value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        throw new NotSupportedException(
            "LoggingMiddleware instances cannot be serialized due to their dependency on ILogger. " +
            "Consider serializing logging-related data models instead (e.g., ProcessingResult, BackpressureContext).");
    }

    /// <summary>
    /// Deserializes a <see cref="LoggingMiddleware"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="LoggingMiddleware"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="NotSupportedException">Thrown because <see cref="LoggingMiddleware"/> cannot be deserialized.
    /// Middleware instances require runtime dependencies that cannot be reconstructed from serialized data.</exception>
    public static LoggingMiddleware? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(json);

        throw new NotSupportedException(
            "LoggingMiddleware instances cannot be deserialized. " +
            "Middleware requires runtime dependencies like ILogger that cannot be reconstructed from serialized data.");
    }

    /// <summary>
    /// Tries to deserialize a <see cref="LoggingMiddleware"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="LoggingMiddleware"/> instance, or null.</param>
    /// <returns>False, since deserialization always fails for <see cref="LoggingMiddleware"/>.</returns>
    public static bool TryFromJson(string json, out LoggingMiddleware? value)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(json);

        value = null;
        return false;
    }
}
