# DataSourceManager

`DataSourceManager` coordinates several `IExternalDataSource` implementations and returns data from the highest-priority source that is currently usable. It fails over to lower-priority sources when a source is unavailable or throws, and it tracks a per-source healthy flag that is never restored automatically.

The class is defined in `src/Integration/ExternalDataSource.cs`, in the `DotNetRealtimePipeline.Integration` namespace, together with the types it works with:

- `IExternalDataSource` - the contract every source implements (`FetchDataAsync`, `IsAvailableAsync`).
- `HttpDataSource` - an HTTP connector that reads from a remote `/data` endpoint and probes `/health`.
- `CachedDataSource` - a decorator that caches fetch results in memory for one hour.

For a general overview of these components, see [ExternalDataSource.md](ExternalDataSource.md).

## Constructor

```csharp
public DataSourceManager(ILogger<DataSourceManager> logger)
```

Throws `ArgumentNullException` if `logger` is `null`. The manager starts with no registered sources.

## Register

```csharp
public void Register(string name, IExternalDataSource source, int priority = 0)
```

Adds a source to the manager.

| Parameter | Description |
|-----------|-------------|
| `name` | Identifier used in logs and as the key in `GetSourceHealth()`. Must not be `null`, empty, or whitespace (`ArgumentException`). |
| `source` | The data source. Must not be `null` (`ArgumentNullException`). |
| `priority` | Integer ordering value. Defaults to `0`. |

### Priority semantics

- Sources are kept sorted by **descending** priority. A source with priority `100` is attempted before one with priority `10`, and `10` is attempted before `-5`.
- Negative values are valid and sort below `0`.
- The ordering is applied on every `Register` call, so registration order does not matter for sources with different priorities.
- Sources with **equal** priority have no guaranteed relative order. The implementation uses `List<T>.Sort`, which is not stable, so do not rely on registration order as a tie-breaker. Give distinct priorities when the order matters.
- Each registration gets a new entry. The manager does not check for duplicate names or duplicate source instances; see [GetSourceHealth](#getsourcehealth) for the consequence of duplicate names.
- The new source starts with `IsHealthy = true`, regardless of whether it is reachable.
- Registration writes to an unsynchronized `List<T>`. Register all sources before the manager is used concurrently; `Register` and `FetchDataAsync` are not safe to call from multiple threads at the same time.

## FetchDataAsync

```csharp
public async Task<List<DataPoint>> FetchDataAsync(DateTime startTime, DateTime endTime)
```

Returns the data from the first source that completes a fetch, walking the sources in priority order.

### Failover algorithm

1. Take a snapshot of the sources whose `IsHealthy` flag is `true`, in priority order. Sources already marked unhealthy are skipped and never retried.
2. For each source in the snapshot:
   1. Call `IsAvailableAsync()`.
   2. If it returns `false`, set `IsHealthy = false`, log a warning, and continue with the next source.
   3. If it returns `true`, call `FetchDataAsync(startTime, endTime)`, log the number of points, and **return that result immediately**. This includes an empty list.
   4. If either call throws, set `IsHealthy = false`, log the error, and continue with the next source.
3. If no source returns, log `All data sources are unavailable` and return an empty `List<DataPoint>`.

### Behavior to be aware of

- **Empty results do not trigger failover.** A source that answers successfully with zero points ends the search. Only unavailability or an exception moves on to the next source.
- **Failure inside a source is not always visible to the manager.** `HttpDataSource.FetchDataAsync` and `IsAvailableAsync` catch their own exceptions. A non-success HTTP status, a `null` deserialization result, or a network error makes `HttpDataSource.FetchDataAsync` return an empty list, and `IsAvailableAsync` returns `false` on exceptions. The first case looks like a successful empty result to the manager and does not mark the source unhealthy. The second case marks it unhealthy through the availability check.
- **Unhealthy is permanent for the lifetime of the manager.** The manager has no probe, no cooldown, and no reset method. Once a source is marked unhealthy it is excluded from every later `FetchDataAsync` call until the manager is discarded or the source is registered again under a new `DataSourceManager`.
- **Only one source is queried per call.** The manager does not merge results from several sources.
- **There is no cancellation token.** Each `IsAvailableAsync` and `FetchDataAsync` call runs to completion (or until the underlying `HttpClient` times out).
- **Availability is checked before every fetch.** Each attempted source gets an `IsAvailableAsync` call first, so a healthy source costs two calls per `FetchDataAsync`. With `HttpDataSource`, that is a `GET {baseUrl}/health` followed by a `GET {baseUrl}/data?...`.

## GetSourceHealth

```csharp
public Dictionary<string, bool> GetSourceHealth()
```

Returns a dictionary that maps each registered name to its stored `IsHealthy` flag.

- The method reports the manager's stored state. It does **not** call `IsAvailableAsync` on any source, so a source may show `true` while it is actually down, until the next `FetchDataAsync` finds out.
- A source only changes from `true` to `false` during `FetchDataAsync`. Nothing changes it back.
- The dictionary is built with `ToDictionary` on the source name. **Duplicate names throw `ArgumentException`** from this method. Because `Register` does not check for duplicates, make sure names are unique before calling `GetSourceHealth`.
- The method returns a new dictionary each time, so changing it does not affect the manager.
- With no registered sources, it returns an empty dictionary.

```csharp
foreach (var (name, healthy) in manager.GetSourceHealth())
{
    Console.WriteLine($"{name}: {(healthy ? "healthy" : "unhealthy")}");
}
```

## Caching with CachedDataSource

`CachedDataSource` wraps any `IExternalDataSource` and keeps the results of `FetchDataAsync` in memory. Register it with the manager in place of the inner source when repeated requests for the same range are common.

```csharp
public CachedDataSource(IExternalDataSource innerSource, ILogger<CachedDataSource> logger)
```

Throws `ArgumentNullException` if either argument is `null`.

### Cache configuration

The cache is a `Caching.CacheService<string, List<DataPoint>>` created with fixed settings:

| Setting | Value | Source |
|---------|-------|--------|
| Capacity | 1,000 entries | `DefaultCacheCapacity` |
| Time-to-live | 1 hour | `DefaultCacheTtl` (`TimeSpan.FromHours(1)`) |
| Eviction when full | LRU (`EvictionPolicy.LRU`, the `CacheService` default) | `CacheService` constructor default |

These values cannot be changed through the public API. The constructor does not accept a capacity or TTL.

### Cache key and TTL

- The key is `$"{startTime:O}_{endTime:O}"`. Two requests share an entry only when both boundaries produce the same round-trip string. `DateTime` values with different `Kind` (for example, UTC and local) produce different keys even when they describe the same instant.
- On a miss, `FetchDataAsync` calls the inner source and stores the result with `TimeSpan.FromHours(1)`. The stored expiration is absolute: it is set at insertion time and does not slide on reads.
- An expired entry is removed when it is next read, and the read counts as a miss.
- Empty lists are cached like any other result. **If the inner source returns an empty list because of a failure that it handled internally (for example, `HttpDataSource` receiving a 500 response), that empty list is cached for one hour** and is returned for that range until it expires or `ClearCache` is called.
- If the inner source throws, nothing is cached and the exception propagates to the caller (the manager treats it as a failure of the source).
- The cached list is returned by reference. Callers that modify the returned list also modify the cached value.

### IsAvailableAsync

`CachedDataSource.IsAvailableAsync` always delegates to the inner source. A cached result does not make the source appear available, so the manager's availability check still reaches the network on every fetch attempt.

### ClearCache

```csharp
public void ClearCache()
```

Removes all entries from this wrapper's cache and logs `Data source cache cleared`. It does not change the manager's health flags, and it does not affect other `CachedDataSource` instances.

Typical uses:

- After a known data correction on the upstream service, so the next fetch goes to the source.
- In tests, to make each case start from an empty cache.

The method is a synchronous call with no return value and no exceptions. It can be called at any time, including while fetches are running; a fetch that is already in progress may still store its result after the clear.

## HttpDataSource

`HttpDataSource` is the HTTP connector used most often with the manager.

```csharp
public HttpDataSource(string baseUrl, HttpClient httpClient, ILogger<HttpDataSource> logger)
```

Throws `ArgumentNullException` if any argument is `null`.

- **`FetchDataAsync`** converts `startTime` and `endTime` to Unix milliseconds with `new DateTimeOffset(dt).ToUnixTimeMilliseconds()` and sends `GET {baseUrl}/data?start={startMs}&end={endMs}`. A success response is deserialized as `List<DataPoint>` with default `System.Text.Json` options. A non-success status, a `null` result, or an exception produces an empty list and a log entry; the method does not throw.
- **`IsAvailableAsync`** sends `GET {baseUrl}/health` and returns `true` only for a success status. Any exception returns `false`.

Notes that follow from the code:

- `baseUrl` is concatenated as-is. A trailing slash produces `//data` and `//health`. Pass the base URL without a trailing slash.
- The URL is not escaped, so `baseUrl` must already be a valid absolute URL prefix.
- Timestamps use `new DateTimeOffset(DateTime)`. For `DateTimeKind.Unspecified` the value is interpreted as local time, so pass `DateTimeKind.Utc` values when the server expects UTC epoch milliseconds.
- Because `HttpDataSource` handles its own errors, the manager does not see HTTP failures in `FetchDataAsync` (see [Behavior to be aware of](#behavior-to-be-aware-of)). Failover for a server that returns errors on `/data` but succeeds on `/health` will not happen automatically.

## Usage example

```csharp
using DotNetRealtimePipeline.Integration;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
using var primaryClient = new HttpClient();
using var fallbackClient = new HttpClient();

var primary = new HttpDataSource(
    "https://primary.example.com",
    primaryClient,
    loggerFactory.CreateLogger<HttpDataSource>());

var fallback = new CachedDataSource(
    new HttpDataSource(
        "https://fallback.example.com",
        fallbackClient,
        loggerFactory.CreateLogger<HttpDataSource>()),
    loggerFactory.CreateLogger<CachedDataSource>());

var manager = new DataSourceManager(
    loggerFactory.CreateLogger<DataSourceManager>());

manager.Register("primary", primary, priority: 100);
manager.Register("fallback", fallback, priority: 10);

var endTime = DateTime.UtcNow;
var startTime = endTime.AddMinutes(-15);
var dataPoints = await manager.FetchDataAsync(startTime, endTime);

foreach (var (name, healthy) in manager.GetSourceHealth())
{
    Console.WriteLine($"{name}: {(healthy ? "healthy" : "unhealthy")}");
}

// Drop cached fallback results, for example after upstream corrections.
fallback.ClearCache();
```

In this example, `fallback` is only queried when `primary` is already unhealthy, reports itself unavailable, or throws. Once `primary` is marked unhealthy it stays excluded for the rest of the process's lifetime, so a long-running process needs a new manager (or a re-registration plan) to switch back to the primary after an outage.

## Related

- [ExternalDataSource.md](ExternalDataSource.md) - overview of the external data source components.
- [CacheService.md](CacheService.md) - the cache implementation used by `CachedDataSource`.
