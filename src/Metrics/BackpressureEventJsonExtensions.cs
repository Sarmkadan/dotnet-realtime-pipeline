#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

namespace DotNetRealtimePipeline.Metrics;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="BackpressureEvent"/>.
/// </summary>
public static class BackpressureEventJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the <see cref="BackpressureEvent"/> to a JSON string.
    /// </summary>
    /// <param name="value">The backpressure event to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the backpressure event.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this BackpressureEvent value, bool indented = false) =>
        ToJson(value, indented, _jsonOptions);

    /// <summary>
    /// Serializes the <see cref="BackpressureEvent"/> to a JSON string with custom formatting.
    /// </summary>
    /// <param name="value">The backpressure event to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <param name="options">The JSON serialization options to use.</param>
    /// <returns>A JSON string representation of the backpressure event.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    private static string ToJson(BackpressureEvent value, bool indented, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);

        var localOptions = indented
            ? new JsonSerializerOptions(options) { WriteIndented = true }
            : options;

        return JsonSerializer.Serialize(value, localOptions);
    }

    /// <summary>
    /// Deserializes a <see cref="BackpressureEvent"/> from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized backpressure event, or <see langword="null"/> if the JSON is <see langword="null"/>, empty, or whitespace-only.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or whitespace-only.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized into a <see cref="BackpressureEvent"/>.</exception>
    public static BackpressureEvent? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<BackpressureEvent>(json.Trim(), _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="BackpressureEvent"/> from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized backpressure event if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or whitespace-only.</exception>
    public static bool TryFromJson(string json, out BackpressureEvent? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<BackpressureEvent>(json.Trim(), _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
