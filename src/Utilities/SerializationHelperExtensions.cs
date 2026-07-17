#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Utilities;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for SerializationHelper for file-based serialization operations.
/// </summary>
public static class SerializationHelperExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a list of ProcessingResult to a file asynchronously.
    /// </summary>
    /// <param name="results">List of ProcessingResult to serialize.</param>
    /// <param name="filePath">Path to the file to write to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> or <paramref name="filePath"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is empty or consists only of whitespace.</exception>
    /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="System.IO.PathTooLongException">The specified path exceeds the system-defined maximum length.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid.</exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    /// <exception cref="System.Security.SecurityException">The caller lacks the required permission.</exception>
    public static async Task SerializeResultsToFileAsync(this List<ProcessingResult> results, string filePath)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var content = SerializationHelper.SerializeResults(results);
        await System.IO.File.WriteAllTextAsync(filePath, content, System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// Deserializes a file to a list of ProcessingResult asynchronously.
    /// </summary>
    /// <param name="filePath">Path to the file to read from.</param>
    /// <returns>A list of ProcessingResult.</returns>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">The file specified by <paramref name="filePath"/> does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="System.IO.PathTooLongException">The specified path exceeds the system-defined maximum length.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid.</exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    /// <exception cref="System.Security.SecurityException">The caller lacks the required permission.</exception>
    /// <exception cref="JsonException">The JSON is invalid.</exception>
    public static async Task<List<ProcessingResult>> DeserializeResultsFromFileAsync(this string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var content = await System.IO.File.ReadAllTextAsync(filePath, System.Text.Encoding.UTF8);
        return JsonSerializer.Deserialize<List<ProcessingResult>>(content, JsonOptions)
            ?? new List<ProcessingResult>();
    }

    /// <summary>
    /// Serializes a MetricAggregation to a file asynchronously.
    /// </summary>
    /// <param name="metrics">MetricAggregation to serialize.</param>
    /// <param name="filePath">Path to the file to write to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="metrics"/> or <paramref name="filePath"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is empty or consists only of whitespace.</exception>
    /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="System.IO.PathTooLongException">The specified path exceeds the system-defined maximum length.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid.</exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    /// <exception cref="System.Security.SecurityException">The caller lacks the required permission.</exception>
    public static async Task SerializeMetricsToFileAsync(this MetricAggregation metrics, string filePath)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var content = SerializationHelper.SerializeMetrics(metrics);
        await System.IO.File.WriteAllTextAsync(filePath, content, System.Text.Encoding.UTF8);
    }
}