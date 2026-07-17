#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
/// Provides extension methods for <see cref="DataPoint"/> to enhance data processing capabilities.
/// </summary>
public static class DataPointExtensions
{
    /// <summary>
    /// Creates a new <see cref="DataPoint"/> with updated value and preserves all other properties.
    /// </summary>
    /// <param name="dataPoint">The source data point to update.</param>
    /// <param name="newValue">The new value to set.</param>
    /// <returns>A new <see cref="DataPoint"/> instance with the updated value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataPoint"/> is null.</exception>
    public static DataPoint WithValue(this DataPoint dataPoint, double newValue)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        var updated = dataPoint.Clone(dataPoint.Id);
        updated.Value = newValue;
        return updated;
    }

    /// <summary>
    /// Creates a new <see cref="DataPoint"/> with updated timestamp and preserves all other properties.
    /// </summary>
    /// <param name="dataPoint">The source data point to update.</param>
    /// <param name="newTimestamp">The new timestamp in milliseconds since Unix epoch.</param>
    /// <returns>A new <see cref="DataPoint"/> instance with the updated timestamp.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataPoint"/> is null.</exception>
    public static DataPoint WithTimestamp(this DataPoint dataPoint, long newTimestamp)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        var updated = dataPoint.Clone(dataPoint.Id);
        updated.Timestamp = newTimestamp;
        updated.CreatedAt = DateTime.UtcNow;
        return updated;
    }

    /// <summary>
    /// Creates a new <see cref="DataPoint"/> with updated quality score and preserves all other properties.
    /// </summary>
    /// <param name="dataPoint">The source data point to update.</param>
    /// <param name="newQuality">The new quality score (0-100).</param>
    /// <returns>A new <see cref="DataPoint"/> instance with the updated quality score.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataPoint"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="newQuality"/> is not between 0 and 100.</exception>
    public static DataPoint WithQuality(this DataPoint dataPoint, int newQuality)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        if (newQuality < 0 || newQuality > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(newQuality), "Quality must be between 0 and 100");
        }

        var updated = dataPoint.Clone(dataPoint.Id);
        updated.Quality = newQuality;
        return updated;
    }

    /// <summary>
    /// Creates a new <see cref="DataPoint"/> with additional tags appended to existing tags.
    /// </summary>
    /// <param name="dataPoint">The source data point to update.</param>
    /// <param name="newTags">Tags to append, comma-separated.</param>
    /// <returns>A new <see cref="DataPoint"/> instance with updated tags.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataPoint"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="newTags"/> is null or whitespace.</exception>
    public static DataPoint WithTags(this DataPoint dataPoint, string newTags)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(newTags);

        var updated = dataPoint.Clone(dataPoint.Id);
        updated.Tags = string.IsNullOrEmpty(dataPoint.Tags)
            ? newTags
            : $"{dataPoint.Tags},{newTags}";
        return updated;
    }

    /// <summary>
    /// Gets all metadata values of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of metadata values to retrieve.</typeparam>
    /// <param name="dataPoint">The data point containing metadata.</param>
    /// <returns>Read-only list of metadata values of the specified type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataPoint"/> is null.</exception>
    public static IReadOnlyList<T> GetMetadataValues<T>(this DataPoint dataPoint)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        return dataPoint.Metadata.Values
            .OfType<T>()
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Attempts to get a metadata value by key with type safety.
    /// </summary>
    /// <typeparam name="T">The expected type of the metadata value.</typeparam>
    /// <param name="dataPoint">The data point containing metadata.</param>
    /// <param name="key">The metadata key to retrieve.</param>
    /// <param name="value">When this method returns, contains the metadata value if found and of correct type; otherwise, the default value for type T.</param>
    /// <returns>True if the key exists and the value is of type T; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataPoint"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is null or whitespace.</exception>
    public static bool TryGetMetadataValue<T>(this DataPoint dataPoint, string key, out T? value)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        value = default;
        if (dataPoint.Metadata.TryGetValue(key, out var objValue) && objValue is T typedValue)
        {
            value = typedValue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Formats the data point for logging purposes with key metadata.
    /// </summary>
    /// <param name="dataPoint">The data point to format.</param>
    /// <param name="includeMetadata">Whether to include metadata in the output.</param>
    /// <returns>A formatted string representation of the data point.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataPoint"/> is null.</exception>
    public static string ToLogString(this DataPoint dataPoint, bool includeMetadata = false)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        var metadataPart = includeMetadata && dataPoint.Metadata.Count > 0
            ? $" | Metadata[{dataPoint.Metadata.Count}]"
            : string.Empty;

        return $"DataPoint[{dataPoint.Id}] - Source: {dataPoint.Source}, " +
               $"Timestamp: {DateTimeOffset.FromUnixTimeMilliseconds(dataPoint.Timestamp):O}, " +
               $"Value: {dataPoint.Value:G}, " +
               $"Quality: {dataPoint.Quality}%{metadataPart}";
    }

    /// <summary>
    /// Determines if this data point is stale based on age threshold.
    /// </summary>
    /// <param name="dataPoint">The data point to check.</param>
    /// <param name="maxAgeMs">Maximum allowed age in milliseconds before considered stale.</param>
    /// <returns>True if the data point is stale; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataPoint"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxAgeMs"/> is negative.</exception>
    public static bool IsStale(this DataPoint dataPoint, long maxAgeMs)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        if (maxAgeMs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAgeMs), "Max age cannot be negative");
        }

        return dataPoint.GetAgeMs() > maxAgeMs;
    }

    /// <summary>
    /// Creates a shallow copy of this data point with a new ID.
    /// </summary>
    /// <param name="dataPoint">The data point to copy.</param>
    /// <param name="newId">The new ID for the copied data point.</param>
    /// <returns>A new <see cref="DataPoint"/> instance with the same properties except ID.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataPoint"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="newId"/> is not positive.</exception>
    public static DataPoint WithId(this DataPoint dataPoint, long newId)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        if (newId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newId), "ID must be positive");
        }

        return dataPoint.Clone(newId);
    }
}