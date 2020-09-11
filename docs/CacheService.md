# CacheService

The `CacheService` class provides a high-performance, in-memory caching mechanism designed for the `dotnet-realtime-pipeline` project. It supports time-based expiration, capacity constraints, and detailed statistical tracking to monitor cache efficiency. The service allows for thread-safe storage and retrieval of generic values, offering both individual entry management and bulk operations for maintenance and cleanup.

## API

### Constructor
*   **`public CacheService()`**
    *   Initializes a new instance of the `CacheService` class with default configuration settings.

### Data Operations
*   **`public bool TryGetValue(TKey key, out TValue value)`**
    *   Attempts to retrieve the value associated with the specified key.
    *   **Parameters**: `key` (The unique identifier for the cached item), `value` (The output parameter containing the retrieved value if successful).
    *   **Returns**: `true` if the key exists and has not expired; otherwise, `false`.
    *   **Throws**: Does not throw exceptions for missing keys; returns `false` instead.

*   **`public void Set(TKey key, TValue value)`**
    *   Adds a new entry to the cache or updates an existing entry with the specified key and value using default expiration policies.
    *   **Parameters**: `key` (The unique identifier), `value` (The data to store).
    *   **Returns**: None.
    *   **Throws**: May throw if the key is null or if internal capacity limits are strictly violated depending on implementation specifics.

*   **`public void Set(TKey key, TValue value, TimeSpan expiration)`**
    *   Adds or updates an entry with a specific time-to-live duration.
    *   **Parameters**: `key` (The unique identifier), `value` (The data to store), `expiration` (The duration before the entry becomes invalid).
    *   **Returns**: None.
    *   **Throws**: Throws `ArgumentOutOfRangeException` if `expiration` is negative.

*   **`public bool TryRemove(TKey key)`**
    *   Removes the entry associated with the specified key from the cache.
    *   **Parameters**: `key` (The unique identifier of the item to remove).
    *   **Returns**: `true` if the item was successfully found and removed; `false` if the key did not exist.
    *   **Throws**: None.

*   **`public void Clear()`**
    *   Removes all entries from the cache and resets statistical counters.
    *   **Parameters**: None.
    *   **Returns**: None.
    *   **Throws**: None.

*   **`public int RemoveExpiredEntries()`**
    *   Scans the cache and removes all entries that have passed their `ExpirationTime`.
    *   **Parameters**: None.
    *   **Returns**: The number of entries removed during the operation.
    *   **Throws**: None.

### Statistics and Metadata
*   **`public CacheStatistics GetStatistics()`**
    *   Retrieves a snapshot of current cache performance metrics.
    *   **Parameters**: None.
    *   **Returns**: A `CacheStatistics` object containing aggregated data such as hit rates and total operations.
    *   **Throws**: None.

*   **`public TValue Value`**
    *   Represents the payload data stored in a specific cache entry context. Accessible primarily when inspecting individual entry structures or within iterator contexts.

*   **`public DateTime CreatedTime`**
    *   Gets the timestamp indicating when the specific cache entry was originally added.

*   **`public DateTime LastAccessTime`**
    *   Gets the timestamp of the most recent successful read operation for the entry.

*   **`public DateTime ExpirationTime`**
    *   Gets the absolute time at which the entry will be considered invalid and eligible for removal.

*   **`public long AccessCount`**
    *   Gets the total number of times this specific entry has been accessed.

*   **`public long TotalHits`**
    *   Gets the cumulative count of successful `TryGetValue` operations across the entire cache service instance.

*   **`public long TotalMisses`**
    *   Gets the cumulative count of failed `TryGetValue` operations (where the key was missing or expired).

*   **`public double HitRate`**
    *   Gets the calculated ratio of hits to total requests (`TotalHits / (TotalHits + TotalMisses)`), expressed as a value between 0.0 and 1.0.

*   **`public int CurrentSize`**
    *   Gets the current number of valid entries stored in the cache.

*   **`public int MaxCapacity`**
    *   Gets the maximum number of entries the cache is configured to hold before eviction policies trigger.

*   **`public double UtilizationPercent`**
    *   Gets the percentage of the `MaxCapacity` currently in use.

### System Overrides
*   **`public override string ToString()`**
    *   Returns a string representation of the cache service, typically including current size, capacity, and hit rate summary.
    *   **Returns**: A formatted string describing the cache state.

## Usage

### Example 1: Basic Caching with Expiration
This example demonstrates storing a user profile with a 10-minute expiration and retrieving it.

```csharp
// Initialize the cache service
var cache = new CacheService();

string userId = "user_123";
var userProfile = new UserProfile { Id = userId, Name = "Alice" };

// Set the value with a 10-minute expiration
cache.Set(userId, userProfile, TimeSpan.FromMinutes(10));

// Attempt to retrieve the value
if (cache.TryGetValue(userId, out UserProfile cachedProfile))
{
    Console.WriteLine($"Retrieved profile for {cachedProfile.Name}");
}
else
{
    Console.WriteLine("Cache miss: Profile not found or expired.");
}
```

### Example 2: Monitoring and Maintenance
This example illustrates checking cache health statistics and manually cleaning up expired entries.

```csharp
var cache = new CacheService();

// ... perform various cache operations ...

// Retrieve statistics
var stats = cache.GetStatistics();
Console.WriteLine($"Hit Rate: {stats.HitRate:P2}");
Console.WriteLine($"Current Size: {stats.CurrentSize} / {stats.MaxCapacity}");

// If utilization is high or specific cleanup is needed
if (cache.UtilizationPercent > 90.0)
{
    int removedCount = cache.RemoveExpiredEntries();
    Console.WriteLine($"Maintenance complete: {removedCount} expired entries removed.");
}

// Output full state summary
Console.WriteLine(cache.ToString());
```

## Notes

*   **Thread Safety**: The `CacheService` is designed for concurrent access. Methods such as `TryGetValue`, `Set`, and `TryRemove` are thread-safe and can be called simultaneously from multiple threads without external locking. However, enumeration over internal collections (if exposed via extension) is not inherently safe.
*   **Expiration Logic**: Entries are not removed immediately upon reaching their `ExpirationTime`. They are marked as invalid and removed either during a subsequent access attempt (`TryGetValue`), during an explicit call to `RemoveExpiredEntries`, or when capacity limits require space.
*   **Capacity Management**: When `CurrentSize` reaches `MaxCapacity`, the behavior of `Set` depends on the internal eviction policy (typically Least Recently Used based on `LastAccessTime`).
*   **Statistics Accuracy**: Properties like `HitRate` and `UtilizationPercent` are calculated based on the current state of `TotalHits`, `TotalMisses`, and `CurrentSize`. In highly concurrent environments, these values represent a near-real-time snapshot and may fluctuate between reads.
*   **Null Handling**: Passing a `null` key to `Set`, `TryGetValue`, or `TryRemove` will typically result in an `ArgumentNullException` or a `false` return, depending on the specific method implementation strictness.
