#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace DotNetRealtimePipeline.Middleware;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides JSON serialization extensions for <see cref="ErrorHandlingMiddleware"/>.
/// Note: ErrorHandlingMiddleware contains private state (ILogger, error mappers) and requires
/// dependency injection, so it cannot be meaningfully serialized or deserialized.
/// These extension methods are provided for API consistency but will throw <see cref="NotSupportedException"/>
/// if actually invoked.
/// </summary>
public static class ErrorHandlingMiddlewareJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the <see cref="ErrorHandlingMiddleware"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The middleware instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the middleware.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown when attempting to serialize middleware state.</exception>
    public static string ToJson(this ErrorHandlingMiddleware value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        throw new NotSupportedException(
            "ErrorHandlingMiddleware contains private state that cannot be serialized. " +
            "This method exists for API consistency only.");
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="ErrorHandlingMiddleware"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized middleware instance, or null if parsing fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown when attempting to deserialize middleware state.</exception>
    public static ErrorHandlingMiddleware? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        throw new NotSupportedException(
            "ErrorHandlingMiddleware cannot be deserialized due to dependency injection requirements. " +
            "This method exists for API consistency only.");
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="ErrorHandlingMiddleware"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized middleware instance, or null if parsing fails.</param>
    /// <returns>False; deserialization is not supported.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out ErrorHandlingMiddleware? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;
        return false;
    }
}