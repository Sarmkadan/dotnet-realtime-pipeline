#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides shared JSON serialization utilities for consistent serialization behavior
/// across the pipeline API and event subscriber layers.
/// </summary>
/// <remarks>
/// This static class centralizes JSON serialization configuration to ensure consistent
/// behavior when serializing/deserializing pipeline objects to/from JSON.
/// </remarks>
internal static class PipelineJsonUtilities
{
    /// <summary>
    /// Maximum allowed JSON depth to prevent stack overflow from deeply nested structures.
    /// </summary>
    internal const int MaxJsonDepth = 64;

    /// <summary>
    /// Maximum allowed JSON payload size in bytes to prevent memory exhaustion attacks.
    /// </summary>
    internal const int MaxJsonPayloadSizeBytes = 10_000_000; // 10 MB

    /// <summary>
    /// Maximum allowed array/list size to prevent excessive memory allocation.
    /// </summary>
    internal const int MaxArraySize = 100_000;

    /// <summary>
    /// Maximum allowed dictionary size to prevent excessive memory allocation.
    /// </summary>
    internal const int MaxDictionarySize = 10_000;

    /// <summary>
    /// Maximum allowed string length to prevent memory exhaustion.
    /// </summary>
    internal const int MaxStringLength = 100_000;

    /// <summary>
    /// Shared JsonSerializerOptions instance used throughout the pipeline for consistent
    /// serialization behavior including camelCase naming policy, enum serialization as strings,
    /// and proper null handling.
    /// </summary>
    internal static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        MaxDepth = MaxJsonDepth,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Validates JSON payload size before deserialization to prevent memory exhaustion attacks.
    /// </summary>
    /// <param name="json">The JSON string to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when JSON exceeds size limits.</exception>
    internal static void ValidateJsonPayloadSize(string json)
    {
        if (json.Length > MaxJsonPayloadSizeBytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(json),
                $"JSON payload size ({json.Length} bytes) exceeds maximum allowed size ({MaxJsonPayloadSizeBytes} bytes).");
        }
    }

    /// <summary>
    /// Validates JSON string length to prevent memory exhaustion.
    /// </summary>
    /// <param name="json">The JSON string to validate.</param>
    /// <returns>True if validation passes; false otherwise.</returns>
    internal static bool ValidateJsonStringLength(string json)
    {
        return json.Length <= MaxStringLength;
    }
}