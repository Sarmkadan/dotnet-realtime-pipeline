# InMemoryDataPointRepository

A thread-safe, in-memory implementation of `IDataPointRepository` that stores `DataPoint` objects in memory for testing or lightweight scenarios. It provides basic CRUD operations and query capabilities for data points, with all data residing in a concurrent dictionary to ensure safe concurrent access.

## API

### `Task<DataPoint?> GetByIdAsync(Guid id)`

Retrieves a single data point by its unique identifier. Returns `null` if no data point with the given ID exists. This method is thread-safe and does not throw under normal operation.

- **Parameters**: `id` – The unique identifier of the data point to retrieve.
- **Returns**: A `Task<DataPoint?>` resolving to the found data point or `null`.
- **Exceptions**: Does not throw under normal operation.

---

### `Task<List<DataPoint>> GetBySourceAsync(string source)`

Retrieves all data points that match the specified source string. The comparison is case-sensitive. Returns an empty list if no matches are found. This method is thread-safe.

- **Parameters**: `source` – The source identifier to filter by.
- **Returns**: A `Task<List<DataPoint>>` containing all matching data points.
- **Exceptions**: Does not throw under normal operation.

---

### `Task<List<DataPoint>> GetByTimeRangeAsync(DateTimeOffset start, DateTimeOffset end)`

Retrieves all data points whose timestamp falls within the specified inclusive range. The comparison uses `DateTimeOffset` equality. Returns an empty list if no data points fall within the range. This method is thread-safe.

- **Parameters**:
  - `start` – The inclusive start of the time range.
  - `end` – The inclusive end of the time range.
- **Returns**: A `Task<List<DataPoint>>` containing all matching data points.
- **Exceptions**: Does not throw under normal operation.

---
### `Task<List<DataPoint>> GetByQualityThresholdAsync(double threshold)`

Retrieves all data points whose quality value is greater than or equal to the specified threshold. Returns an empty list if no data points meet the condition. This method is thread-safe.

- **Parameters**: `threshold` – The minimum quality value to include.
- **Returns**: A `Task<List<DataPoint>>` containing all matching data points.
- **Exceptions**: Does not throw under normal operation.

---
### `Task<DataPoint> CreateAsync(DataPoint dataPoint)`

Adds a new data point to the in-memory store. The data point must have a unique, non-default `Id`; otherwise, the operation throws an `ArgumentException`. This method is thread-safe and returns the created data point with its ID preserved.

- **Parameters**: `dataPoint` – The data point to create.
- **Returns**: A `Task<DataPoint>` resolving to the created data point.
- **Exceptions**:
  - `ArgumentException` – If `dataPoint.Id` is `Guid.Empty`.
  - `ArgumentNullException` – If `dataPoint` is `null`.

---
### `Task<DataPoint> UpdateAsync(DataPoint dataPoint)`

Updates an existing data point in the store. The data point must exist and have a valid `Id`; otherwise, the operation throws an `ArgumentException`. This method is thread-safe and returns the updated data point.

- **Parameters**: `dataPoint` – The data point containing updated values and a valid ID.
- **Returns**: A `Task<DataPoint>` resolving to the updated data point.
- **Exceptions**:
  - `ArgumentException` – If `dataPoint.Id` is `Guid.Empty` or the ID does not exist.
  - `ArgumentNullException` – If `dataPoint` is `null`.

---
### `Task<bool> DeleteAsync(Guid id)`

Removes a data point from the store by its unique identifier. Returns `true` if the data point existed and was removed; otherwise, returns `false`. This method is thread-safe.

- **Parameters**: `id` – The unique identifier of the data point to delete.
- **Returns**: A `Task<bool>` indicating whether a data point was deleted.
- **Exceptions**: Does not throw under normal operation.

---
### `Task<int> CountAsync()`

Returns the total number of data points currently stored in memory. This method is thread-safe.

- **Returns**: A `Task<int>` resolving to the count of data points.
- **Exceptions**: Does not throw under normal operation.

---
### `Task<List<DataPoint>> GetPagedAsync(int pageNumber, int pageSize)`

Retrieves a paged subset of data points. The results are ordered by insertion time (FIFO). Returns an empty list if no data points exist or if the page is out of bounds. This method is thread-safe.

- **Parameters**:
  - `pageNumber` – The 1-based page number to retrieve.
  - `pageSize` – The maximum number of items per page.
- **Returns**: A `Task<List<DataPoint>>` containing the requested page of data points.
- **Exceptions**:
  - `ArgumentOutOfRangeException` – If `pageNumber` is less than 1 or `pageSize` is less than 1.

---
### `void Clear()`

Removes all data points from the in-memory store. This method is thread-safe and does not throw under normal operation.

- **Parameters**: None.
- **Returns**: None.
- **Exceptions**: Does not throw under normal operation.

## Usage

### Example 1: Basic CRUD Operations
