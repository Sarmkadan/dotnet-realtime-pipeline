using System;
using System.Text.Json;

namespace DotNetRealtimePipeline.Benchmarks;

/// <summary>
/// Provides JSON (de)serialization helpers for <see cref="PipelineBenchmarks"/> using <see cref="System.Text.Json"/>.
/// </summary>
public static class PipelineBenchmarksJsonExtensions
{
    // Cached options with camel‑case naming. WriteIndented is set per call.
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // Preserve defaults for other settings (e.g., ignore null values) – they are suitable for benchmarks.
    };

    /// <summary>
    /// Serializes the <paramref name="value"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="PipelineBenchmarks"/> instance to serialize.</param>
    /// <param name="indented">If <c>true</c>, the output JSON will be formatted with indentation.</param>
    /// <returns>A JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this PipelineBenchmarks value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        // If indentation is requested, clone the cached options and enable WriteIndented.
        var options = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="PipelineBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string representing a <see cref="PipelineBenchmarks"/>.</param>
    /// <returns>The deserialized <see cref="PipelineBenchmarks"/> object, or <c>null</c> if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized into <see cref="PipelineBenchmarks"/>.</exception>
    public static PipelineBenchmarks? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<PipelineBenchmarks>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="PipelineBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized <see cref="PipelineBenchmarks"/> if the operation succeeded;
    /// otherwise, <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out PipelineBenchmarks? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
