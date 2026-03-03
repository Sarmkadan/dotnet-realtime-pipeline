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
/// In-memory implementation of the data point repository.
/// Suitable for development and testing scenarios.
/// </summary>
public class InMemoryDataPointRepository : IDataPointRepository
{
    private readonly Dictionary<long, DataPoint> _dataPoints = new();
    private readonly object _lockObject = new();

    /// <summary>
    /// Retrieves a data point by ID.
    /// </summary>
    public Task<DataPoint?> GetByIdAsync(long id)
    {
        lock (_lockObject)
        {
            _dataPoints.TryGetValue(id, out var dataPoint);
            return Task.FromResult(dataPoint);
        }
    }

    /// <summary>
    /// Retrieves all data points from a specific source.
    /// </summary>
    public Task<List<DataPoint>> GetBySourceAsync(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source cannot be null", nameof(source));

        lock (_lockObject)
        {
            var results = _dataPoints.Values
                .Where(dp => dp.Source == source)
                .ToList();

            return Task.FromResult(results);
        }
    }

    /// <summary>
    /// Retrieves data points within a time range.
    /// </summary>
    public Task<List<DataPoint>> GetByTimeRangeAsync(long startMs, long endMs)
    {
        if (startMs > endMs)
            throw new ArgumentException("Start time must be <= end time");

        lock (_lockObject)
        {
            var results = _dataPoints.Values
                .Where(dp => dp.Timestamp >= startMs && dp.Timestamp <= endMs)
                .OrderBy(dp => dp.Timestamp)
                .ToList();

            return Task.FromResult(results);
        }
    }

    /// <summary>
    /// Retrieves data points meeting or exceeding a quality threshold.
    /// </summary>
    public Task<List<DataPoint>> GetByQualityThresholdAsync(int minQuality)
    {
        if (minQuality < 0 || minQuality > 100)
            throw new ArgumentException("Quality must be between 0 and 100", nameof(minQuality));

        lock (_lockObject)
        {
            var results = _dataPoints.Values
                .Where(dp => dp.Quality >= minQuality)
                .OrderByDescending(dp => dp.Quality)
                .ToList();

            return Task.FromResult(results);
        }
    }

    /// <summary>
    /// Creates a new data point.
    /// </summary>
    public Task<DataPoint> CreateAsync(DataPoint dataPoint)
    {
        if (dataPoint == null) throw new ArgumentNullException(nameof(dataPoint));
        if (!dataPoint.Validate()) throw new ArgumentException("Data point validation failed");

        lock (_lockObject)
        {
            if (_dataPoints.ContainsKey(dataPoint.Id))
                throw new InvalidOperationException($"Data point with ID {dataPoint.Id} already exists");

            _dataPoints[dataPoint.Id] = dataPoint;
            return Task.FromResult(dataPoint);
        }
    }

    /// <summary>
    /// Updates an existing data point.
    /// </summary>
    public Task<DataPoint> UpdateAsync(DataPoint dataPoint)
    {
        if (dataPoint == null) throw new ArgumentNullException(nameof(dataPoint));
        if (!dataPoint.Validate()) throw new ArgumentException("Data point validation failed");

        lock (_lockObject)
        {
            if (!_dataPoints.ContainsKey(dataPoint.Id))
                throw new KeyNotFoundException($"Data point with ID {dataPoint.Id} not found");

            _dataPoints[dataPoint.Id] = dataPoint;
            return Task.FromResult(dataPoint);
        }
    }

    /// <summary>
    /// Deletes a data point by ID.
    /// </summary>
    public Task<bool> DeleteAsync(long id)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_dataPoints.Remove(id));
        }
    }

    /// <summary>
    /// Gets the total count of data points.
    /// </summary>
    public Task<int> CountAsync()
    {
        lock (_lockObject)
        {
            return Task.FromResult(_dataPoints.Count);
        }
    }

    /// <summary>
    /// Gets a paginated list of data points.
    /// </summary>
    public Task<List<DataPoint>> GetPagedAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) throw new ArgumentException("Page number must be >= 1", nameof(pageNumber));
        if (pageSize < 1) throw new ArgumentException("Page size must be >= 1", nameof(pageSize));

        lock (_lockObject)
        {
            var results = _dataPoints.Values
                .OrderBy(dp => dp.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(results);
        }
    }

    /// <summary>
    /// Clears all data points (useful for testing).
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            _dataPoints.Clear();
        }
    }

    /// <summary>
    /// Gets the internal store for testing.
    /// </summary>
    internal Dictionary<long, DataPoint> GetInternalStore() => _dataPoints;
}
