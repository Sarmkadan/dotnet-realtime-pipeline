#nullable enable
// =============================================================================
// Extension helpers for QueryService
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Domain.Models;

namespace DotNetRealtimePipeline.Services;

/// <summary>
/// Provides additional convenience methods for <see cref="QueryService"/>.
/// </summary>
public static class QueryServiceExtensions
{
    /// <summary>
    /// Retrieves aggregated statistics for a time range expressed as <see cref="DateTime"/> values.
    /// </summary>
    /// <param name="service">The <see cref="QueryService"/> instance.</param>
    /// <param name="start">Inclusive start of the range (UTC).</param>
    /// <param name="end">Inclusive end of the range (UTC).</param>
    /// <returns>A <see cref="Task{DataAggregateStatistics}"/> containing the statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="end"/> is earlier than <paramref name="start"/>.</exception>
    public static async Task<DataAggregateStatistics> GetAggregateStatisticsAsync(
        this QueryService service,
        DateTime start,
        DateTime end)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (end < start)
        {
            throw new ArgumentException(
                "End time must be greater than or equal to start time.",
                nameof(end));
        }

        long startMs = new DateTimeOffset(start, TimeSpan.Zero).ToUnixTimeMilliseconds();
        long endMs = new DateTimeOffset(end, TimeSpan.Zero).ToUnixTimeMilliseconds();

        return await service.GetAggregateStatisticsAsync(startMs, endMs).ConfigureAwait(false);
    }

    /// <summary>
    /// Searches all data points and returns those that satisfy a predicate.
    /// </summary>
    /// <param name="service">The <see cref="QueryService"/> instance.</param>
    /// <param name="predicate">A function that determines whether a <see cref="DataPoint"/> should be included.</param>
    /// <returns>A read-only list of matching <see cref="DataPoint"/> objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static async Task<IReadOnlyList<DataPoint>> SearchDataPointsAsync(
        this QueryService service,
        Func<DataPoint, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(predicate);

        // Retrieve all data points (no filters) and apply the predicate locally.
        var all = await service
            .SearchDataPointsAsync(
                startTime: null,
                endTime: null,
                source: null,
                minQuality: null)
            .ConfigureAwait(false);

        return all
            .Where(predicate)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Retrieves the most recent metric aggregations as a read-only collection.
    /// </summary>
    /// <param name="service">The <see cref="QueryService"/> instance.</param>
    /// <param name="count">The maximum number of metric records to return.</param>
    /// <returns>A read-only list of <see cref="MetricAggregation"/> objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than 1.</exception>
    public static async Task<IReadOnlyList<MetricAggregation>> GetRecentMetricsAsync(
        this QueryService service,
        int count = 10)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "Count must be at least 1.");
        }

        var list = await service.GetRecentMetricsAsync(count).ConfigureAwait(false);
        return list.AsReadOnly();
    }
}