#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Data.Repositories;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="InMemoryDataPointRepository"/> that provide
/// additional query capabilities and convenience methods.
/// </summary>
public static class InMemoryDataPointRepositoryExtensions
{
    /// <summary>
    /// Retrieves data points by source with optional tag filtering.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="source">The source identifier.</param>
    /// <param name="tagFilter">Optional tag filter (comma-separated tags).</param>
    /// <returns>Filtered list of data points.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> is null or whitespace.</exception>
    public static async Task<IReadOnlyList<DataPoint>> GetBySourceAsync(
        this InMemoryDataPointRepository repository,
        string source,
        string? tagFilter = null)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        var dataPoints = await repository.GetBySourceAsync(source);

        if (string.IsNullOrWhiteSpace(tagFilter))
        {
            return dataPoints.AsReadOnly();
        }

        var tags = tagFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return dataPoints
            .Where(dp => !string.IsNullOrWhiteSpace(dp.Tags) &&
                tags.Any(tag => dp.Tags.Contains(tag, StringComparison.OrdinalIgnoreCase)))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Retrieves data points within a time range with optional quality threshold.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="startMs">Start timestamp in milliseconds.</param>
    /// <param name="endMs">End timestamp in milliseconds.</param>
    /// <param name="minQuality">Optional minimum quality threshold (0-100).</param>
    /// <returns>Filtered list of data points ordered by timestamp.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="startMs"/> > <paramref name="endMs"/>, or <paramref name="minQuality"/> is invalid.</exception>
    public static async Task<IReadOnlyList<DataPoint>> GetByTimeRangeAsync(
        this InMemoryDataPointRepository repository,
        long startMs,
        long endMs,
        int? minQuality = null)
    {
        ArgumentNullException.ThrowIfNull(repository);

        if (startMs > endMs)
        {
            throw new ArgumentException("Start time must be <= end time", nameof(startMs));
        }

        if (minQuality.HasValue && (minQuality.Value < 0 || minQuality.Value > 100))
        {
            throw new ArgumentException("Quality must be between 0 and 100", nameof(minQuality));
        }

        var dataPoints = await repository.GetByTimeRangeAsync(startMs, endMs);

        if (minQuality.HasValue)
        {
            dataPoints = dataPoints.Where(dp => dp.Quality >= minQuality.Value).ToList();
        }

        return dataPoints.AsReadOnly();
    }

    /// <summary>
    /// Retrieves the most recent data points across all sources, optionally filtered by quality.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="count">Number of most recent data points to retrieve.</param>
    /// <param name="minQuality">Optional minimum quality threshold (0-100).</param>
    /// <returns>List of most recent data points ordered by timestamp descending.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="count"/> is less than 1, or <paramref name="minQuality"/> is invalid.</exception>
    public static async Task<IReadOnlyList<DataPoint>> GetMostRecentAsync(
        this InMemoryDataPointRepository repository,
        int count,
        int? minQuality = null)
    {
        ArgumentNullException.ThrowIfNull(repository);

        if (count < 1)
        {
            throw new ArgumentException("Count must be >= 1", nameof(count));
        }

        if (minQuality.HasValue && (minQuality.Value < 0 || minQuality.Value > 100))
        {
            throw new ArgumentException("Quality must be between 0 and 100", nameof(minQuality));
        }

        var allDataPoints = await repository.GetPagedAsync(1, int.MaxValue);

        var filtered = minQuality.HasValue
            ? allDataPoints.Where(dp => dp.Quality >= minQuality.Value).ToList()
            : allDataPoints;

        return filtered
            .OrderByDescending(dp => dp.Timestamp)
            .Take(count)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Retrieves data points by source and value range.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="source">The source identifier.</param>
    /// <param name="minValue">Minimum value (inclusive).</param>
    /// <param name="maxValue">Maximum value (inclusive).</param>
    /// <param name="minQuality">Optional minimum quality threshold (0-100).</param>
    /// <returns>Filtered list of data points ordered by timestamp.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> is null or whitespace, <paramref name="minValue"/> > <paramref name="maxValue"/>, or <paramref name="minQuality"/> is invalid.</exception>
    public static async Task<IReadOnlyList<DataPoint>> GetByValueRangeAsync(
        this InMemoryDataPointRepository repository,
        string source,
        double minValue,
        double maxValue,
        int? minQuality = null)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        if (minValue > maxValue)
        {
            throw new ArgumentException("Minimum value must be <= maximum value", nameof(minValue));
        }

        if (minQuality.HasValue && (minQuality.Value < 0 || minQuality.Value > 100))
        {
            throw new ArgumentException("Quality must be between 0 and 100", nameof(minQuality));
        }

        var dataPoints = await repository.GetBySourceAsync(source);

        var results = dataPoints
            .Where(dp => dp.Value >= minValue && dp.Value <= maxValue)
            .ToList();

        if (minQuality.HasValue)
        {
            results = results.Where(dp => dp.Quality >= minQuality.Value).ToList();
        }

        return results
            .OrderBy(dp => dp.Timestamp)
            .ToList()
            .AsReadOnly();
    }
}