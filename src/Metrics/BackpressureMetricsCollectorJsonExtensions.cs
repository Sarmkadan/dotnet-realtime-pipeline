#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Metrics;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="BackpressureMetricsCollector"/>.
/// </summary>
public static class BackpressureMetricsCollectorJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the <see cref="BackpressureMetricsCollector"/> to a JSON string.
    /// </summary>
    /// <param name="value">The metrics collector instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the metrics collector.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this BackpressureMetricsCollector value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var snapshot = value.GetSnapshot();
        return JsonSerializer.Serialize(snapshot, indented ? GetIndentedOptions() : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="BackpressureMetricsCollector"/> instance.
    /// </summary>
    /// <remarks>
    /// This method cannot fully reconstruct a <see cref="BackpressureMetricsCollector"/> because it requires a <see cref="BackpressureService"/> instance.
    /// It only deserializes the metrics snapshot. Use <see cref="BackpressureMetricsCollector.GetSnapshot"/> on an existing collector
    /// to obtain a serializable snapshot, then serialize that snapshot instead.
    /// </remarks>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized <see cref="BackpressureMetricsCollector"/> instance with a null service.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static BackpressureMetricsCollector? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        var snapshot = JsonSerializer.Deserialize<BackpressureMetricsSnapshot>(json, _jsonOptions);
        if (snapshot is null)
            return null;

        return new BackpressureMetricsCollector(default!, snapshot.StageMetrics.Count);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="BackpressureMetricsCollector"/> instance.
    /// </summary>
    /// <remarks>
    /// This method cannot fully reconstruct a <see cref="BackpressureMetricsCollector"/> because it requires a <see cref="BackpressureService"/> instance.
    /// It only deserializes the metrics snapshot. Use <see cref="BackpressureMetricsCollector.GetSnapshot"/> on an existing collector
    /// to obtain a serializable snapshot, then serialize that snapshot instead.
    /// </remarks>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or empty.</exception>
    public static bool TryFromJson(string json, out BackpressureMetricsCollector? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        value = null;
        try
        {
            var snapshot = JsonSerializer.Deserialize<BackpressureMetricsSnapshot>(json, _jsonOptions);
            if (snapshot is null)
                return false;

            value = new BackpressureMetricsCollector(default!, snapshot.StageMetrics.Count);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static JsonSerializerOptions GetIndentedOptions() =>
        new JsonSerializerOptions(_jsonOptions) { WriteIndented = true };
}