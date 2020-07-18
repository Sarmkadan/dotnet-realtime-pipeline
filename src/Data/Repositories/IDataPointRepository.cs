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
    Task<DataPoint?> GetByIdAsync(long id);
    Task<List<DataPoint>> GetBySourceAsync(string source);
    Task<List<DataPoint>> GetByTimeRangeAsync(long startMs, long endMs);
    Task<List<DataPoint>> GetByQualityThresholdAsync(int minQuality);
    Task<DataPoint> CreateAsync(DataPoint dataPoint);
    Task<DataPoint> UpdateAsync(DataPoint dataPoint);
    Task<bool> DeleteAsync(long id);
    Task<int> CountAsync();
    Task<List<DataPoint>> GetPagedAsync(int pageNumber, int pageSize);
}
