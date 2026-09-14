#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="IExternalDataSource"/>.
/// </summary>
public static class ExternalDataSourceExtensions
{
    /// <summary>
    /// Fetches data for the last specified lookback period.
    /// </summary>
    /// <param name="source">The external data source.</param>
    /// <param name="lookback">The lookback time span from now.</param>
    /// <returns>A list of data points for the specified period.</returns>
    public static Task<List<DataPoint>> FetchLastAsync(this IExternalDataSource source, TimeSpan lookback)
    {
        ArgumentNullException.ThrowIfNull(source);
        var endTime = DateTime.UtcNow;
        var startTime = endTime - lookback;
        return source.FetchDataAsync(startTime, endTime);
    }

    /// <summary>
    /// Fetches data for today (from start of day to now in UTC).
    /// </summary>
    /// <param name="source">The external data source.</param>
    /// <returns>A list of data points for today.</returns>
    public static Task<List<DataPoint>> FetchTodayAsync(this IExternalDataSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var startTime = DateTime.UtcNow.Date;
        var endTime = DateTime.UtcNow;
        return source.FetchDataAsync(startTime, endTime);
    }

    /// <summary>
    /// Fetches data if the source is available, otherwise returns an empty list.
    /// </summary>
    /// <param name="source">The external data source.</param>
    /// <param name="start">The start time of the data to fetch.</param>
    /// <param name="end">The end time of the data to fetch.</param>
    /// <returns>A list of data points if available, otherwise an empty list.</returns>
    public static async Task<List<DataPoint>> FetchIfAvailableAsync(this IExternalDataSource source, DateTime start, DateTime end)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!await source.IsAvailableAsync())
        {
            return new List<DataPoint>();
        }

        return await source.FetchDataAsync(start, end);
    }
}