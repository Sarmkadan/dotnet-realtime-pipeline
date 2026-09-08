# IDataPointRepository

`IDataPointRepository` defines the asynchronous persistence contract for
`DataPoint` instances. Implementations provide lookup, filtering, creation,
updates, deletion, counting, and paging while keeping storage details behind a
consistent API.

See [InMemoryDataPointRepository](InMemoryDataPointRepository.md) for the
repository's in-memory implementation.

## Contract

### `Task<DataPoint?> GetByIdAsync(long id)`

Retrieves the data point with the specified unique identifier. The result is
`null` when no matching data point is found.

### `Task<List<DataPoint>> GetBySourceAsync(string source)`

Retrieves all data points from the specified source. The returned list contains
the data points associated with `source`.

### `Task<List<DataPoint>> GetByTimeRangeAsync(long startMs, long endMs)`

Retrieves data points within the specified time range. `startMs` is the start
timestamp and `endMs` is the end timestamp, both expressed in milliseconds.

### `Task<List<DataPoint>> GetByQualityThresholdAsync(int minQuality)`

Retrieves data points whose quality meets or exceeds `minQuality`. The minimum
quality score uses the `0` to `100` scale.

### `Task<DataPoint> CreateAsync(DataPoint dataPoint)`

Creates a new data point and returns the created instance with any metadata
updated by the repository.

### `Task<DataPoint> UpdateAsync(DataPoint dataPoint)`

Updates an existing data point with the supplied values and returns the updated
instance.

### `Task<bool> DeleteAsync(long id)`

Deletes the data point with the specified identifier. The result is `true` when
deletion succeeds and `false` otherwise.

### `Task<int> CountAsync()`

Returns the total number of data points in the repository.

### `Task<List<DataPoint>> GetPagedAsync(int pageNumber, int pageSize)`

Retrieves one page of data points. `pageNumber` is 1-based, and `pageSize`
specifies the number of items in a page.

## Implementation example

The following implementation uses a list to demonstrate the complete contract.
A production implementation can replace the collection operations with database
or service calls while preserving the same method signatures and semantics.

```csharp
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class ListDataPointRepository : IDataPointRepository
{
    private readonly List<DataPoint> _dataPoints = new();

    public Task<DataPoint?> GetByIdAsync(long id) =>
        Task.FromResult(_dataPoints.SingleOrDefault(point => point.Id == id));

    public Task<List<DataPoint>> GetBySourceAsync(string source) =>
        Task.FromResult(_dataPoints
            .Where(point => point.Source == source)
            .ToList());

    public Task<List<DataPoint>> GetByTimeRangeAsync(long startMs, long endMs) =>
        Task.FromResult(_dataPoints
            .Where(point => point.Timestamp >= startMs && point.Timestamp <= endMs)
            .ToList());

    public Task<List<DataPoint>> GetByQualityThresholdAsync(int minQuality) =>
        Task.FromResult(_dataPoints
            .Where(point => point.Quality >= minQuality)
            .ToList());

    public Task<DataPoint> CreateAsync(DataPoint dataPoint)
    {
        _dataPoints.Add(dataPoint);
        return Task.FromResult(dataPoint);
    }

    public Task<DataPoint> UpdateAsync(DataPoint dataPoint)
    {
        var index = _dataPoints.FindIndex(point => point.Id == dataPoint.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException(
                $"Data point with ID {dataPoint.Id} was not found.");
        }

        _dataPoints[index] = dataPoint;
        return Task.FromResult(dataPoint);
    }

    public Task<bool> DeleteAsync(long id)
    {
        var removed = _dataPoints.RemoveAll(point => point.Id == id) > 0;
        return Task.FromResult(removed);
    }

    public Task<int> CountAsync() => Task.FromResult(_dataPoints.Count);

    public Task<List<DataPoint>> GetPagedAsync(int pageNumber, int pageSize) =>
        Task.FromResult(_dataPoints
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList());
}
```

Callers should program against `IDataPointRepository` so implementations can be
substituted without changing application code:

```csharp
using System;

IDataPointRepository repository = new ListDataPointRepository();

var point = new DataPoint(
    id: 1,
    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    value: 21.5,
    source: "sensor-a");

await repository.CreateAsync(point);
var storedPoint = await repository.GetByIdAsync(point.Id);
```
