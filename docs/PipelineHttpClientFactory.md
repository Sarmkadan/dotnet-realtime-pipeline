# PipelineHttpClientFactory

Manages the creation, configuration, and lifecycle of `HttpClient` instances for pipeline communication. It provides factory methods for default and service-specific clients, a named-client cache with removal and clearing capabilities, and a fluent `HttpClientBuilder` for constructing customized clients with timeout, retry, compression, headers, and connection limits.

## API

### PipelineHttpClientFactory

Creates a new instance of the factory. The constructor initializes internal caches and default configuration values used by all factory methods unless overridden.

### HttpClient CreateDefaultClient()

Returns a new `HttpClient` configured with the factory's default settings: `Timeout`, `MaxRetries`, `RetryDelay`, `MaxConnectionsPerHost`, `UseCompression`, `UserAgent`, and `DefaultHeaders`. The returned client is not cached; each call produces a fresh instance.

### HttpClient CreateServiceClient(string serviceName)

Returns a new `HttpClient` configured specifically for the named service. The client inherits factory defaults but may apply service-specific overrides if previously configured through the builder or cache. The returned client is not cached by this method; use `GetOrCreateClient` for cached retrieval.

**Parameters:**
- `serviceName` — The name of the service for which the client is being created. Must not be null or empty.

**Throws:**
- `ArgumentNullException` if `serviceName` is null.
- `ArgumentException` if `serviceName` is empty or whitespace.

### HttpClient GetOrCreateClient(string clientName)

Retrieves an existing `HttpClient` from the internal cache by name, or creates, caches, and returns a new one if not present. The created client uses factory defaults. Subsequent calls with the same name return the cached instance.

**Parameters:**
- `clientName` — The key used to identify the client in the cache. Must not be null or empty.

**Returns:** The cached or newly created `HttpClient`.

**Throws:**
- `ArgumentNullException` if `clientName` is null.
- `ArgumentException` if `clientName` is empty or whitespace.

### void RemoveClient(string clientName)

Removes the `HttpClient` associated with the given name from the internal cache. If no client exists for that name, the method does nothing and does not throw.

**Parameters:**
- `clientName` — The key of the client to remove. Null or empty values are tolerated and result in a no-op.

### void ClearClients()

Removes all cached `HttpClient` instances from the internal cache. After this call, subsequent `GetOrCreateClient` requests will produce new instances.

### TimeSpan Timeout

Gets or sets the default timeout applied to all `HttpClient` instances created by this factory. This value is used by `CreateDefaultClient`, `CreateServiceClient`, and `GetOrCreateClient` unless overridden during builder construction.

### int MaxRetries

Gets or sets the default maximum number of retry attempts for failed requests. Used by factory methods unless overridden.

### TimeSpan RetryDelay

Gets or sets the default delay between retry attempts. Used by factory methods unless overridden.

### int MaxConnectionsPerHost

Gets or sets the default maximum number of simultaneous connections allowed to a single host. Used by factory methods unless overridden.

### bool UseCompression

Gets or sets whether HTTP compression is enabled by default. When `true`, clients accept compressed responses. Used by factory methods unless overridden.

### string UserAgent

Gets or sets the default `User-Agent` header value applied to all clients created by the factory. Can be null or empty.

### Dictionary<string, string> DefaultHeaders

Gets the dictionary of default headers applied to every client created by the factory. Modifications to this dictionary affect all subsequently created clients. The dictionary is not thread-safe for concurrent writes; external synchronization is required if modified after factory initialization.

### HttpClientBuilder

Returns a new `HttpClientBuilder` instance for fluently constructing a customized `HttpClient`. The builder starts with factory defaults and allows per-client overrides.

### HttpClientBuilder WithTimeout(TimeSpan timeout)

Sets a custom timeout for the client being built, overriding the factory default.

**Parameters:**
- `timeout` — The timeout value. Must be greater than `TimeSpan.Zero`.

**Returns:** The builder instance for chaining.

**Throws:**
- `ArgumentOutOfRangeException` if `timeout` is less than or equal to `TimeSpan.Zero`.

### HttpClientBuilder WithRetry(int maxRetries, TimeSpan retryDelay)

Sets custom retry parameters for the client being built, overriding factory defaults.

**Parameters:**
- `maxRetries` — Maximum retry attempts. Must be non-negative.
- `retryDelay` — Delay between retries. Must be greater than `TimeSpan.Zero` if `maxRetries` is greater than zero.

**Returns:** The builder instance for chaining.

**Throws:**
- `ArgumentOutOfRangeException` if `maxRetries` is negative.
- `ArgumentOutOfRangeException` if `maxRetries` > 0 and `retryDelay` is less than or equal to `TimeSpan.Zero`.

### HttpClientBuilder WithCompression(bool enable)

Enables or disables HTTP compression for the client being built, overriding the factory default.

**Parameters:**
- `enable` — `true` to accept compressed responses; `false` otherwise.

**Returns:** The builder instance for chaining.

### HttpClientBuilder WithHeader(string name, string value)

Adds a custom header to the client being built. Headers added via this method are merged with factory `DefaultHeaders`; builder headers take precedence in case of key conflicts.

**Parameters:**
- `name` — The header name. Must not be null or empty.
- `value` — The header value. Can be null or empty.

**Returns:** The builder instance for chaining.

**Throws:**
- `ArgumentNullException` if `name` is null.
- `ArgumentException` if `name` is empty or whitespace.

### HttpClientBuilder WithMaxConnectionsPerHost(int maxConnections)

Sets a custom maximum connections-per-host limit for the client being built, overriding the factory default.

**Parameters:**
- `maxConnections` — The maximum number of connections. Must be greater than zero.

**Returns:** The builder instance for chaining.

**Throws:**
- `ArgumentOutOfRangeException` if `maxConnections` is less than or equal to zero.

### HttpClient Build()

Constructs and returns the `HttpClient` with all configured options. The resulting client is not cached by the factory; it is a standalone instance. Repeated calls to `Build()` on the same builder produce independent clients.

**Returns:** A new `HttpClient` configured according to the builder's accumulated settings.

## Usage

### Example 1: Basic factory setup and cached client retrieval

```csharp
var factory = new PipelineHttpClientFactory
{
    Timeout = TimeSpan.FromSeconds(30),
    MaxRetries = 3,
    RetryDelay = TimeSpan.FromMilliseconds(500),
    MaxConnectionsPerHost = 10,
    UseCompression = true,
    UserAgent = "MyPipeline/1.0"
};

factory.DefaultHeaders["X-Custom-Header"] = "CustomValue";

// Retrieve a cached client for a named endpoint
HttpClient ordersClient = factory.GetOrCreateClient("OrdersService");

// Later, remove it when no longer needed
factory.RemoveClient("OrdersService");

// Clear all cached clients during shutdown
factory.ClearClients();
```

### Example 2: Building a specialized client with overrides

```csharp
var factory = new PipelineHttpClientFactory
{
    Timeout = TimeSpan.FromSeconds(15),
    MaxRetries = 2,
    RetryDelay = TimeSpan.FromSeconds(1)
};

HttpClient secureClient = factory.HttpClientBuilder
    .WithTimeout(TimeSpan.FromSeconds(60))
    .WithRetry(maxRetries: 5, retryDelay: TimeSpan.FromSeconds(2))
    .WithCompression(true)
    .WithHeader("Authorization", "Bearer token-value")
    .WithHeader("X-Priority", "High")
    .WithMaxConnectionsPerHost(20)
    .Build();

// Use the client directly — it is not cached
var response = await secureClient.GetAsync("https://api.example.com/secure-data");
```

## Notes

- **Thread safety:** `GetOrCreateClient`, `RemoveClient`, and `ClearClients` are safe to call concurrently. The `DefaultHeaders` dictionary is not synchronized; concurrent reads are safe if the dictionary is fully populated before any client creation, but concurrent writes or mixed read-write access require external locking.
- **Cached client disposal:** The factory does not dispose cached clients when they are removed or cleared. Callers are responsible for managing the lifecycle of any clients they retrieve, including disposal when appropriate. Removing a client from the cache does not invalidate or dispose references held externally.
- **Builder reuse:** `HttpClientBuilder` instances are not reusable after `Build()` is called. Each builder produces exactly one client. Obtain a new builder from the factory for each additional customized client.
- **Header merging:** Builder headers supplement, not replace, factory `DefaultHeaders`. If the same header key is defined in both, the builder's value wins for that specific client. The factory's `DefaultHeaders` dictionary remains unchanged.
- **Timeout and retry interaction:** The timeout applies to each individual HTTP request attempt. Retries multiply the total possible elapsed time. For example, a 30-second timeout with 3 retries and a 1-second delay can take up to approximately 93 seconds before final failure.
- **Service-specific clients:** `CreateServiceClient` does not cache its result. To combine service-specific configuration with caching, configure a client via the builder, then manually insert it into the cache or use a separate caching strategy.
