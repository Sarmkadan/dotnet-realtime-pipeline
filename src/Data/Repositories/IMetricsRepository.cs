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
/// Interface for metrics persistence and aggregation.
/// </summary>
public interface IMetricsRepository
{
    /// <summary>
    /// Retrieves a metric aggregation by its unique identifier.
    /// </summary>
    /// <param name="metricId">The metric identifier.</param>
    /// <returns>The metric aggregation if found, otherwise null.</returns>
    Task<MetricAggregation?> GetByIdAsync(long metricId);

    /// <summary>
    /// Retrieves metric aggregations within a specific time range.
    /// </summary>
    /// <param name="startMs">The start timestamp in milliseconds.</param>
    /// <param name="endMs">The end timestamp in milliseconds.</param>
    /// <returns>List of metric aggregations within the specified time range.</returns>
    Task<List<MetricAggregation>> GetByTimeRangeAsync(long startMs, long endMs);

    /// <summary>
    /// Retrieves metric aggregations by their type.
    /// </summary>
    /// <param name="metricType">The type of metric aggregation (e.g., "hourly", "daily").</param>
    /// <returns>List of metric aggregations of the specified type.</returns>
    Task<List<MetricAggregation>> GetByTypeAsync(string metricType);

    /// <summary>
    /// Saves a metric aggregation to the repository.
    /// </summary>
    /// <param name="metric">The metric aggregation to save.</param>
    /// <returns>The saved metric aggregation.</returns>
    Task<MetricAggregation> SaveAsync(MetricAggregation metric);

    /// <summary>
    /// Deletes a metric aggregation by its identifier.
    /// </summary>
    /// <param name="metricId">The metric identifier.</param>
    /// <returns>True if deletion was successful, false otherwise.</returns>
    Task<bool> DeleteAsync(long metricId);

    /// <summary>
    /// Retrieves the most recent metric aggregation.
    /// </summary>
    /// <returns>The latest metric aggregation.</returns>
    Task<MetricAggregation> GetLatestAsync();

    /// <summary>
    /// Retrieves a history of metric aggregations.
    /// </summary>
    /// <param name="count">The number of historical records to retrieve.</param>
    /// <returns>List of metric aggregations in chronological order.</returns>
    Task<List<MetricAggregation>> GetHistoryAsync(int count);
}
