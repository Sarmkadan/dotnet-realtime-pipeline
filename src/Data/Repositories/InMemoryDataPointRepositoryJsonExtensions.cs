#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Data.Repositories;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetRealtimePipeline.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization extensions for InMemoryDataPointRepository.
/// </summary>
public static class InMemoryDataPointRepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the InMemoryDataPointRepository to a JSON string.
    /// </summary>
    /// <param name="value">The repository to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the repository.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static string ToJson(this InMemoryDataPointRepository value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value.GetInternalStore(), options);
    }

    /// <summary>
    /// Deserializes a JSON string to an InMemoryDataPointRepository.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized repository, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static InMemoryDataPointRepository? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var store = JsonSerializer.Deserialize<Dictionary<long, DataPoint>>(json, _jsonSerializerOptions);
        return store is null
            ? null
            : DeserializeStore(store);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an InMemoryDataPointRepository.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized repository, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out InMemoryDataPointRepository? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = FromJson(json);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static InMemoryDataPointRepository DeserializeStore(Dictionary<long, DataPoint> store)
    {
        var repository = new InMemoryDataPointRepository();
        var internalStore = repository.GetInternalStore();
        foreach (var kvp in store)
        {
            internalStore[kvp.Key] = kvp.Value;
        }

        return repository;
    }
}
