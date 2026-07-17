#nullable enable

using System;
using System.Text.Json;

namespace DotNetRealtimePipeline.Domain.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="MetricAggregation"/> instances.
/// Uses camelCase property naming policy for JSON serialization to match JavaScript/TypeScript conventions.
/// </summary>
public static class MetricAggregationJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the <see cref="MetricAggregation"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">If <c>true</c>, the output JSON will be formatted with indentation.</param>
    /// <returns>A JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <remarks>
    /// Uses invariant culture for formatting numeric values to ensure consistent behavior across different system locales.
    /// </remarks>
    public static string ToJson(this MetricAggregation value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        // Clone the shared options only when indentation is requested to avoid mutating the static instance.
        var options = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="MetricAggregation"/> instance.
    /// </summary>
    /// <param name="json">The JSON string representing a <see cref="MetricAggregation"/>.</param>
    /// <returns>The deserialized <see cref="MetricAggregation"/> object, or <c>null</c> if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized into <see cref="MetricAggregation"/>.</exception>
    /// <remarks>
    /// Uses invariant culture for parsing numeric values to ensure consistent behavior across different system locales.
    /// </remarks>
    public static MetricAggregation? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<MetricAggregation>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="MetricAggregation"/> instance.
    /// </summary>
    /// <param name="json">The JSON string representing a <see cref="MetricAggregation"/>.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="MetricAggregation"/> if the operation succeeded; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or empty.</exception>
    public static bool TryFromJson(string json, out MetricAggregation? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            value = JsonSerializer.Deserialize<MetricAggregation>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}