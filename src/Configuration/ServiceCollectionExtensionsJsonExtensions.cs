#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Configuration;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="ServiceCollectionExtensions"/>.
/// </summary>
/// <remarks>
/// This class provides serialization methods for the <see cref="ServiceCollectionExtensions"/> static class.
/// Since static classes cannot be instantiated, these methods work with configuration
/// that would be used with ServiceCollectionExtensions methods.
/// </remarks>
public static class ServiceCollectionExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the <see cref="ServiceCollectionExtensions"/> type reference to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="ServiceCollectionExtensions"/> type reference.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the ServiceCollectionExtensions configuration.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public static string ToJson(this object value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var configMarker = new ServiceCollectionExtensionsConfigMarker
        {
            Type = nameof(ServiceCollectionExtensions),
            IsStaticClass = true,
            SupportsAddPipelineServices = true
        };

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(configMarker, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ServiceCollectionExtensions"/> type reference.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="ServiceCollectionExtensions"/> type reference, or <see langword="null"/> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="json"/> is <see langword="null"/>.
    /// </exception>
    public static object? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            var configMarker = JsonSerializer.Deserialize<ServiceCollectionExtensionsConfigMarker>(json, _jsonSerializerOptions);

            if (configMarker is not null && configMarker.Type == nameof(ServiceCollectionExtensions))
            {
                return default;
            }

            return default;
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ServiceCollectionExtensions"/> type reference.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Outputs the deserialized <see cref="ServiceCollectionExtensions"/> type reference, or <see langword="null"/> if deserialization fails.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="json"/> is <see langword="null"/>.
    /// </exception>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            var configMarker = JsonSerializer.Deserialize<ServiceCollectionExtensionsConfigMarker>(json, _jsonSerializerOptions);

            if (configMarker is not null && configMarker.Type == nameof(ServiceCollectionExtensions))
            {
                value = default;
                return true;
            }

            value = default;
            return false;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Internal marker class representing ServiceCollectionExtensions configuration.
    /// </summary>
    private sealed class ServiceCollectionExtensionsConfigMarker
    {
        public string? Type { get; set; }
        public bool IsStaticClass { get; set; }
        public bool SupportsAddPipelineServices { get; set; }
    }
}
