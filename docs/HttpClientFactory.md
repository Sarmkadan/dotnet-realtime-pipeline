# HTTP client factory and builder

[`src/Integration/HttpClientFactory.cs`](../src/Integration/HttpClientFactory.cs) defines `PipelineHttpClientFactory`, its named-client cache, and the standalone fluent `HttpClientBuilder`. This page documents the implementation in that source file. For the different pipeline helper, see [PipelineHttpClientFactory](PipelineHttpClientFactory.md).

All types described here are in the `DotNetRealtimePipeline.Integration` namespace.

## `PipelineHttpClientFactory`

Construct the factory with an `ILogger<PipelineHttpClientFactory>`. The factory owns every client placed in its internal cache: removing or clearing a cached client disposes it. The cache uses a regular `Dictionary<string, HttpClient>` and is not safe for concurrent access.

### `CreateDefaultClient()`

Creates a new, uncached `HttpClient` on every call. Its handler:

- decompresses GZip and Deflate responses automatically;
- follows redirects; and
- allows at most three automatic redirects.

The client has a 30-second timeout and sends `User-Agent: DotNetRealtimePipeline/1.0` and `Accept: application/json`. The factory's public configuration properties describe the same defaults, but this method uses the literal values in the implementation.

The caller owns and should dispose the returned client.

### `CreateServiceClient(serviceName, timeout, useCompression)`

Returns the client cached under `serviceName`, or creates and caches one when the key is new. `useCompression` defaults to `true`.

A newly created service client:

- uses the supplied `timeout`;
- enables automatic GZip and Deflate decompression when `useCompression` is `true`, otherwise disables automatic decompression;
- follows redirects;
- uses `CredentialCache.DefaultCredentials`; and
- sends `User-Agent: DotNetRealtimePipeline/{serviceName}`, `Accept: application/json`, and `Accept-Encoding: gzip, deflate`.

Configuration arguments only affect initial creation. If the name is already cached, the existing client is returned and the new `timeout` and `useCompression` values are ignored. The method logs creation, but not cache hits.

### `GetOrCreateClient(key, configure)`

Returns the client cached under `key`. For a new key, it calls `CreateDefaultClient()`, invokes `configure` on that client, caches it, and returns it. Although the signature declares a non-null `Action<HttpClient>`, the implementation uses a null-conditional invocation, so a runtime null value results in no additional configuration.

The callback runs only during initial creation. Later calls with the same key return the cached client without invoking their callback.

### `RemoveClient(serviceName)`

If the cache contains `serviceName`, removes and disposes that client and logs the removal. It does nothing when the key is absent. External references to a removed client refer to the now-disposed instance.

### `ClearClients()`

Disposes every cached client, empties the cache, and logs the operation. Clients created by `CreateDefaultClient()` or `HttpClientBuilder.Build()` are not in this cache and are unaffected.

## `HttpClientBuilder`

`HttpClientBuilder` is constructed separately with an `ILogger<HttpClientBuilder>`. It accumulates settings in an internal `HttpClientConfiguration`; each fluent method returns the same builder instance.

### Fluent methods

| Method | Effect |
| --- | --- |
| `WithTimeout(TimeSpan timeout)` | Sets the timeout assigned to the built client. |
| `WithRetry(int maxRetries, TimeSpan delay)` | Records retry count and delay in the configuration. These values appear in logging, but `Build()` does not create a retry policy or handler, so requests are not retried by this class. |
| `WithCompression(bool enabled)` | Enables or disables automatic GZip and Deflate response decompression. |
| `WithHeader(string name, string value)` | Adds or replaces an entry in the builder's default-header dictionary. |
| `WithMaxConnectionsPerHost(int connections)` | Records the connection limit. The current `Build()` implementation does not apply it to the handler. |

These methods do not perform their own argument validation. Invalid values may instead fail when assigned to `HttpClient` or added to `DefaultRequestHeaders` during `Build()`.

### `Build()`

Creates a new, uncached client using a new `HttpClientHandler`. It applies the configured compression setting and timeout, adds `User-Agent: DotNetRealtimePipeline/1.0`, then adds all headers supplied through `WithHeader`. It logs timeout, retry count, and compression state.

Each call returns a distinct client that the caller owns. The builder retains its configuration, so subsequent calls reuse the accumulated settings. As noted above, retry settings and the maximum-connections setting are not applied to request execution by the current implementation.

## Example

```csharp
using DotNetRealtimePipeline.Integration;
using Microsoft.Extensions.Logging.Abstractions;

var factory = new PipelineHttpClientFactory(
    NullLogger<PipelineHttpClientFactory>.Instance);

// The first call creates and configures the named client.
HttpClient catalog = factory.GetOrCreateClient("catalog", client =>
{
    client.BaseAddress = new Uri("https://catalog.example/");
    client.DefaultRequestHeaders.Add("X-Application", "orders");
});

// This returns the same cached instance; the callback is not run.
HttpClient sameCatalog = factory.GetOrCreateClient(
    "catalog",
    client => client.Timeout = TimeSpan.FromSeconds(5));

var builder = new HttpClientBuilder(NullLogger<HttpClientBuilder>.Instance);
using HttpClient reporting = builder
    .WithTimeout(TimeSpan.FromSeconds(20))
    .WithRetry(3, TimeSpan.FromMilliseconds(250))
    .WithCompression(true)
    .WithHeader("Accept", "application/json")
    .WithMaxConnectionsPerHost(20)
    .Build();

// RemoveClient disposes the cached catalog client.
factory.RemoveClient("catalog");

// Dispose any remaining cached clients when the factory is no longer needed.
factory.ClearClients();
```

In this example, `WithRetry` and `WithMaxConnectionsPerHost` preserve the requested values in builder configuration, but the source does not currently wire either setting into the constructed handler.
