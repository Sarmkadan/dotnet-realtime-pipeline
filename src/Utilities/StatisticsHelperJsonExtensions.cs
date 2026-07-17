using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace DotNetRealtimePipeline.Utilities;

/// <summary>
/// Provides JSON serialization extensions for <see cref="StatisticsHelper"/> instances.
/// </summary>
public static class StatisticsHelperJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="StatisticsHelper"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The statistics helper instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the statistics helper.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this StatisticsHelper value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="StatisticsHelper"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized statistics helper instance, or <see langword="null"/> if <paramref name="json"/> is <see langword="null"/> or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static StatisticsHelper? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrEmpty(json)
            ? null
            : JsonSerializer.Deserialize<StatisticsHelper>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="StatisticsHelper"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized statistics helper instance if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out StatisticsHelper? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<StatisticsHelper>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}