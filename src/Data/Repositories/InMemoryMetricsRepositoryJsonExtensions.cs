#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Data.Repositories;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetRealtimePipeline.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization extensions for InMemoryMetricsRepository.
/// </summary>
public static class InMemoryMetricsRepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the InMemoryMetricsRepository to a JSON string.
    /// </summary>
    /// <param name="value">The repository to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the repository.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static string ToJson(this InMemoryMetricsRepository value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        var metrics = value.GetInternalMetrics();
        return JsonSerializer.Serialize(metrics, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an InMemoryMetricsRepository.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized repository, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static InMemoryMetricsRepository? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var metrics = JsonSerializer.Deserialize<List<MetricAggregation>>(json, _jsonSerializerOptions);
        if (metrics is null)
        {
            return null;
        }

        var repository = new InMemoryMetricsRepository();
        var internalMetrics = repository.GetInternalMetrics();
        internalMetrics.AddRange(metrics);

        return repository;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an InMemoryMetricsRepository.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized repository, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null.</exception>
    public static bool TryFromJson(string json, out InMemoryMetricsRepository? value)
    {
        ArgumentNullException.ThrowIfNull(json);

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
}