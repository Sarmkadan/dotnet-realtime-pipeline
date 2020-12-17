#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Data.Repositories;

using DotNetRealtimePipeline.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Interface for data point persistence operations.
/// </summary>
public interface IDataPointRepository
{
    /// <summary>
    /// Retrieves a data point by its unique identifier.
    /// </summary>
    /// <param name="id">The data point identifier.</param>
    /// <returns>The data point if found, otherwise null.</returns>
    Task<DataPoint?> GetByIdAsync(long id);

    /// <summary>
    /// Retrieves all data points from a specific source.
    /// </summary>
    /// <param name="source">The source identifier.</param>
    /// <returns>List of data points from the specified source.</returns>
    Task<List<DataPoint>> GetBySourceAsync(string source);

    /// <summary>
    /// Retrieves data points within a specific time range.
    /// </summary>
    /// <param name="startMs">The start timestamp in milliseconds.</param>
    /// <param name="endMs">The end timestamp in milliseconds.</param>
    /// <returns>List of data points within the specified time range.</returns>
    Task<List<DataPoint>> GetByTimeRangeAsync(long startMs, long endMs);

    /// <summary>
    /// Retrieves data points that meet or exceed a minimum quality threshold.
    /// </summary>
    /// <param name="minQuality">The minimum quality score (0-100).</param>
    /// <returns>List of data points with quality at or above the threshold.</returns>
    Task<List<DataPoint>> GetByQualityThresholdAsync(int minQuality);

    /// <summary>
    /// Creates a new data point in the repository.
    /// </summary>
    /// <param name="dataPoint">The data point to create.</param>
    /// <returns>The created data point with updated metadata.</returns>
    Task<DataPoint> CreateAsync(DataPoint dataPoint);

    /// <summary>
    /// Updates an existing data point.
    /// </summary>
    /// <param name="dataPoint">The data point with updated values.</param>
    /// <returns>The updated data point.</returns>
    Task<DataPoint> UpdateAsync(DataPoint dataPoint);

    /// <summary>
    /// Deletes a data point by its identifier.
    /// </summary>
    /// <param name="id">The data point identifier.</param>
    /// <returns>True if deletion was successful, false otherwise.</returns>
    Task<bool> DeleteAsync(long id);

    /// <summary>
    /// Gets the total count of data points in the repository.
    /// </summary>
    /// <returns>The total count of data points.</returns>
    Task<int> CountAsync();

    /// <summary>
    /// Retrieves a paged subset of data points.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>List of data points for the specified page.</returns>
    Task<List<DataPoint>> GetPagedAsync(int pageNumber, int pageSize);
}
