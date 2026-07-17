#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Text.Json;

/// <summary>
/// Provides JSON serialization extensions for the <see cref="ValidationHelper"/> type.
/// </summary>
public static class ValidationHelperJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="ValidationHelper"/> to JSON.
    /// </summary>
    /// <param name="value">The validation helper.</param>
    /// <param name="indented">Whether to indent the JSON.</param>
    /// <returns>JSON string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this ValidationHelper value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="ValidationHelper"/> from JSON.
    /// </summary>
    /// <param name="json">JSON string.</param>
    /// <returns>Deserialized validation helper or null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or whitespace.</exception>
    public static ValidationHelper? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<ValidationHelper>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="ValidationHelper"/> from JSON.
    /// </summary>
    /// <param name="json">JSON string.</param>
    /// <param name="value">Receives the result.</param>
    /// <returns>True if successful.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out ValidationHelper? value)
    {
        value = null;

        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            value = JsonSerializer.Deserialize<ValidationHelper>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}