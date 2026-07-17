# CacheServiceExtensions

The `CacheServiceExtensions` static class provides a set of extension methods designed to simplify common operations on cache services that implement a key‑value store with generic `TKey` and `TValue` types. These methods cover atomic get‑or‑add semantics, batch retrieval and removal, bulk insertion, and performance monitoring. They are intended to be used with an underlying cache implementation that supports the required operations (e.g., `ICacheService<TKey, TValue>`).

## API

### `GetOrAdd<TKey, TValue>` (two overloads)

**Purpose**  
Retrieves the value associated with the specified key from the cache. If the key does not exist, the value is created and added to the cache atomically.

**Parameters**  
- Overload 1: `TKey key`, `Func<TKey, TValue> valueFactory` – a factory delegate that produces the value when the key is missing.  
- Overload 2: `TKey key`, `TValue value` – a pre‑computed value to insert if the key is absent.

**Return Value**  
The cached value for the given key, either the existing one or the newly added one.

**Exceptions**  
- `ArgumentNullException` – thrown when `key` is `null` (if `TKey` is a reference type) or when `valueFactory` is `null` in the first overload.

---

### `RemoveRange<TKey, TValue>`

**Purpose**  
Removes all entries whose keys are contained in the provided collection.

**Parameters**  
- `IEnumerable<TKey> keys` – the keys to remove.

**Return Value**  
The number of entries that were actually removed from the cache.

**Exceptions**  
- `ArgumentNullException` – thrown when `keys` is `null`.  
- `ArgumentException` – thrown if `keys` contains a `null` key (when `TKey` is a reference type).

---

### `GetRange<TKey, TValue>`

**Purpose**  
Retrieves the values for a set of keys and returns them as a read‑only dictionary.

**Parameters**  
- `IEnumerable<TKey> keys` – the keys to look up.

**Return Value**  
An `IReadOnlyDictionary<TKey, TValue>` containing only the keys that were found in the cache. Keys not present are omitted.

**Exceptions**  
- `ArgumentNullException` – thrown when `keys` is `null`.  
- `ArgumentException` – thrown if `keys` contains a `null` key (when `TKey` is a reference type).

---

### `SetRange<TKey, TValue>` (two overloads)

**Purpose**  
Inserts or updates multiple entries in the cache in a single operation.

**Parameters**  
- Overload 1: `IEnumerable<KeyValuePair<TKey, TValue>> items` – a collection of key‑value pairs to set.  
- Overload 2: `IEnumerable<TKey> keys`, `IEnumerable<TValue> values` – two parallel sequences of keys and values; they must have the same length.

**Return Value**  
None (`void`).

**Exceptions**  
- `ArgumentNullException` – thrown when any argument is `null`.  
- `ArgumentException` – thrown in overload 2 when `keys` and `values` have different lengths, or when any key is `null` (if `TKey` is a reference type).  
- `InvalidOperationException` – thrown if the underlying cache cannot accept the new entries (e.g., capacity exceeded).

---

### `ToPerformanceCounterString<TKey, TValue>`

**Purpose**  
Returns a human‑readable string summarizing the current performance counters of the cache (e.g., hit rate, miss count, eviction count).

**Parameters**  
None.

**Return Value**  
A `string` containing formatted performance metrics. The exact format is implementation‑defined but typically includes labels and numeric values.

**Exceptions**  
None.

---

### `GetValueOrDefault<TKey, TValue>`

**Purpose**  
Attempts to retrieve the value for the specified key. If the key is not found, returns the default value for `TValue` (or a caller‑supplied default, depending on the overload).

**Parameters**  
- `TKey key` – the key to look up.  
- (Optional) `TValue defaultValue` – the value to return if the key is absent.

**Return Value**  
The cached value if found; otherwise `default(TValue)` or the provided `defaultValue`.

**Exceptions**  
- `ArgumentNullException` – thrown when `key` is `null` (if `TKey` is a reference type).

---

### `GetUtilizationRatio<TKey, TValue>`

**Purpose**  
Returns the ratio of currently used cache capacity to the total capacity, expressed as a `double` between 0.0 and 1.0.

**Parameters**  
None.

**Return Value**  
A `double` representing the utilization ratio. A value of 1.0 indicates the cache is full.

**Exceptions**  
None.

---

## Usage

### Example 1: Atomic Get‑or‑Add with a Factory

```csharp
using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Caching;

// Assume 'cache' is an instance of a cache service (e.g., ICacheService<string, Order>)
ICacheService<string, Order> cache = GetCacheService();

string orderId = "ORD-12345";

// Retrieve the order from cache, or load it from the database if missing
Order order = cache.GetOrAdd(orderId, id => LoadOrderFromDatabase(id));

Console.WriteLine($"Order {order.Id} retrieved.");
```

### Example 2: Batch Operations – Set and Retrieve Multiple Entries

```csharp
using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Caching;

ICacheService<int, string> cache = GetCacheService();

// Prepare a batch of entries
var entries = new Dictionary<int, string>
{
    { 1, "Alpha" },
    { 2, "Beta" },
    { 3, "Gamma" }
};

// Insert all entries at once
cache.SetRange(entries);

// Retrieve a subset of keys
var keysToFetch = new[] { 1, 3, 5 };
IReadOnlyDictionary<int, string> fetched = cache.GetRange(keysToFetch);

foreach (var kvp in fetched)
{
    Console.WriteLine($"Key {kvp.Key}: {kvp.Value}");
}
// Output: Key 1: Alpha, Key 3: Gamma  (key 5 is not present)
```

---

## Notes

- **Thread Safety** – All extension methods are thread‑safe provided the underlying cache implementation is thread‑safe. The `GetOrAdd` overloads guarantee atomicity: concurrent calls for the same key will not result in duplicate creation of the value.  
- **Null Keys** – When `TKey` is a reference type, passing `null` as a key to any method that accepts a key or a collection of keys will throw `ArgumentNullException`.  
- **Duplicate Keys in Range Operations** – If the same key appears multiple times in a `SetRange` call, the last occurrence in the enumeration determines the final value. For `RemoveRange`, duplicate keys are harmless and do not affect the removal count.  
- **Capacity Limits** – `SetRange` may throw `InvalidOperationException` if the cache cannot accommodate all new entries (e.g., a fixed‑size cache that is already full). Use `GetUtilizationRatio` to monitor capacity.  
- **Performance Counters** – The string returned by `ToPerformanceCounterString` is intended for diagnostic logging or monitoring dashboards. Its format may vary between cache implementations and is not guaranteed to be stable across versions.  
- **Default Values** – `GetValueOrDefault` returns `default(TValue)` when no explicit default is provided. For reference types this is `null`; for value types it is the zero‑initialized value. Be aware of potential null‑reference issues when using the result without checking.
