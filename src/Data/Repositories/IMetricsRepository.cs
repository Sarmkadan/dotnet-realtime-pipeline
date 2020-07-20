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
    Task<MetricAggregation?> GetByIdAsync(long metricId);
    Task<List<MetricAggregation>> GetByTimeRangeAsync(long startMs, long endMs);
    Task<List<MetricAggregation>> GetByTypeAsync(string metricType);
    Task<MetricAggregation> SaveAsync(MetricAggregation metric);
    Task<bool> DeleteAsync(long metricId);
    Task<MetricAggregation> GetLatestAsync();
    Task<List<MetricAggregation>> GetHistoryAsync(int count);
}
