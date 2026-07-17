#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Services;

using System;
using System.Text.Json;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="SlidingWindowAggregator"/>.
/// </summary>
/// <remarks>
/// This static class contains extension methods for serializing <see cref="SlidingWindowAggregator"/> instances
/// to JSON strings and deserializing them back, using camelCase property naming policy by default.
/// </remarks>
public static class SlidingWindowAggregatorJsonExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="SlidingWindowAggregator"/> instance to JSON.
    /// </summary>
    /// <param name="value">The <see cref="SlidingWindowAggregator"/> instance to serialize.</param>
    /// <param name="indented"><see langword="true"/> to format the JSON with indentation; otherwise, produces compact JSON.</param>
    /// <returns>A JSON string representation of the <see cref="SlidingWindowAggregator"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this SlidingWindowAggregator value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(JsonSerializerOptions);
        options.WriteIndented = indented;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="SlidingWindowAggregator"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="SlidingWindowAggregator"/> instance or <see langword="null"/> if deserialization yields <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON parsing fails.</exception>
    public static SlidingWindowAggregator? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<SlidingWindowAggregator>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="SlidingWindowAggregator"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="SlidingWindowAggregator"/> instance,
    /// or <see langword="null"/> if deserialization failed.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out SlidingWindowAggregator? value)
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