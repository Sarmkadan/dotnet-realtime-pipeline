# External Data Sources

The external data source components in `src/Integration/ExternalDataSource.cs` provide a common asynchronous contract, an HTTP implementation, priority-based fallback across registered sources, and an optional in-memory caching wrapper.

All types are in the `DotNetRealtimePipeline.Integration` namespace.

## IExternalDataSource Contract

`IExternalDataSource` defines two methods:

| Method | Returns | Description |
|--------|---------|-------------|
| `FetchDataAsync(DateTime startTime, DateTime endTime)` | `Task<List<DataPoint>>` | Fetches data points for the requested time range. |
| `IsAvailableAsync()` | `Task<bool>` | Checks whether the source is currently available. |

The contract does not define cancellation, time-range validation, exception handling, or thread-safety guarantees. Implementations determine those behaviors.

## Concrete Sources

### HttpDataSource

`HttpDataSource` retrieves data through an injected `HttpClient`.

- `FetchDataAsync` converts the start and end times to Unix epoch milliseconds and sends a GET request to `{baseUrl}/data?start={startMs}&end={endMs}`.
- A successful response body is deserialized as `List<DataPoint>` using `System.Text.Json`.
- A non-success HTTP status, a null deserialization result, or an exception produces an empty list. Failures are logged rather than rethrown.
- `IsAvailableAsync` sends a GET request to `{baseUrl}/health`. It returns `true` only for a successful HTTP status and returns `false` if the request throws.

### CachedDataSource

`CachedDataSource` decorates any `IExternalDataSource`. Its in-memory cache has a capacity of 1,000 entries and a default expiration of one hour.

- Cache keys combine the round-trip (`O`) representations of `startTime` and `endTime`; only requests with the same represented values share an entry.
- On a cache miss, `FetchDataAsync` delegates to the inner source and caches the returned list for one hour, including an empty list.
- `IsAvailableAsync` always delegates to the inner source and does not use cached data to determine availability.
- `ClearCache()` removes all entries from this wrapper's cache.

## DataSourceManager

`DataSourceManager` coordinates multiple sources and returns data from the first eligible source it can use.

### Register(string name, IExternalDataSource source, int priority = 0)

Registers a source as initially healthy, records its name and priority, and sorts the registered sources by descending priority. Larger priority values are attempted before smaller values. The default priority is `0`.

### FetchDataAsync(DateTime startTime, DateTime endTime)

The manager takes the currently healthy sources in priority order and processes them as follows:

1. Calls `IsAvailableAsync` on the source.
2. If availability is `false`, marks the source unhealthy and moves to the next source.
3. If available, calls `FetchDataAsync` and immediately returns its result.
4. If either source call throws, marks the source unhealthy and moves to the next source.

If no healthy source succeeds, the manager returns an empty `List<DataPoint>`.

### GetSourceHealth()

Returns a `Dictionary<string, bool>` containing each registered name and its manager-maintained `IsHealthy` flag. This reports the manager's stored state; it does not perform live availability checks.

## Priority and Health Semantics

- Priorities are integers sorted from highest to lowest. Ordering between sources with equal priorities is not guaranteed by the implementation.
- Every source starts with `IsHealthy = true` when registered.
- A source becomes unhealthy only when its availability check returns `false` or an exception escapes from its availability or fetch call.
- An unhealthy source is excluded before subsequent fetch attempts. The manager has no method to probe it again or restore it to healthy; re-registration or a new manager is required to make a failed source eligible again.
- A successful call is defined as a fetch that returns normally. An empty result is returned immediately and does not trigger fallback.
- Health is not changed when `HttpDataSource` handles an HTTP or deserialization failure internally and returns an empty list.
- Registered names should be unique. `GetSourceHealth` builds a dictionary by name, so duplicate names cause that call to throw `ArgumentException`.

## Usage Example

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

var cachedFallback = new CachedDataSource(
    new HttpDataSource(
        "https://fallback.example.com",
        fallbackClient,
        loggerFactory.CreateLogger<HttpDataSource>()),
    loggerFactory.CreateLogger<CachedDataSource>());

var manager = new DataSourceManager(
    loggerFactory.CreateLogger<DataSourceManager>());

manager.Register("primary", primary, priority: 100);
manager.Register("fallback", cachedFallback, priority: 10);

var endTime = DateTime.UtcNow;
var startTime = endTime.AddMinutes(-15);
var dataPoints = await manager.FetchDataAsync(startTime, endTime);

foreach (var source in manager.GetSourceHealth())
{
    Console.WriteLine($"{source.Key}: {(source.Value ? "healthy" : "unhealthy")}");
}

// Remove all entries held by the fallback wrapper when cached ranges are stale.
cachedFallback.ClearCache();
```

The example gives the primary source precedence. The fallback is attempted only if the primary is already unhealthy, reports itself unavailable, or throws an exception that reaches the manager.
