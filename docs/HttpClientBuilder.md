# HttpClientBuilder and HttpClientConfiguration

[`src/Integration/HttpClientFactory.cs`](../src/Integration/HttpClientFactory.cs) defines `HttpClientBuilder`, a fluent builder that produces a configured `HttpClient`, and `HttpClientConfiguration`, the settings object the builder keeps internally. This page documents both types. For the cached-client factory in the same file, see [HttpClientFactory.md](HttpClientFactory.md).

Both types are in the `DotNetRealtimePipeline.Integration` namespace.

## `HttpClientConfiguration`

`HttpClientConfiguration` is a public, sealed class of mutable properties plus a set of default values. Each `HttpClientBuilder` creates its own instance in a private field, and the builder writes into it through its `With*` methods. No public API accepts an `HttpClientConfiguration` instance, and the builder does not expose its configuration, so changing properties on a standalone instance has no effect on any client.

### Defaults

| Property | Type | Default | Source constant |
| --- | --- | --- | --- |
| `Timeout` | `TimeSpan` | 30 seconds | `DefaultTimeout` |
| `MaxRetries` | `int` | `3` | `DefaultMaxRetries` |
| `RetryDelay` | `TimeSpan` | 1 second | `DefaultRetryDelay` |
| `MaxConnectionsPerHost` | `int` | `10` | `DefaultMaxConnectionsPerHost` |
| `UseCompression` | `bool` | `true` | (literal) |
| `UserAgent` | `string` | `"DotNetRealtimePipeline/1.0"` | `DefaultUserAgent` |
| `DefaultHeaders` | `Dictionary<string, string>` | empty dictionary | (literal) |

The default values are available as public static members: `DefaultTimeout`, `DefaultRetryDelay`, `DefaultUserAgent`, and the constants `DefaultMaxRetries` and `DefaultMaxConnectionsPerHost`. The `UseCompression` default is written as a literal `true` in the source, not as a named constant.

### `ToString()`

Returns a single-line summary of the timeout, retry settings, connection limit, compression flag, user agent, and the number of default headers (`DefaultHeadersCount`). The header values themselves are not printed.

## `HttpClientBuilder`

`HttpClientBuilder` is a sealed class. Construct it with an `ILogger<HttpClientBuilder>`; a null logger throws `ArgumentNullException`.

Every `With*` method stores its value in the builder's internal configuration and returns the same builder instance, so calls chain. The builder holds no reference to any client it has built. Calling `Build()` repeatedly produces separate clients from the same settings, and later `With*` calls affect only clients built after they run.

### `WithTimeout(TimeSpan timeout)`

Sets the timeout that `Build()` assigns to `HttpClient.Timeout`. The method does not validate the value. A zero, negative, or otherwise out-of-range value is rejected by the `HttpClient.Timeout` setter when `Build()` runs, so the exception surfaces from `Build()`, not from `WithTimeout`.

### `WithRetry(int maxRetries, TimeSpan delay)`

Validates both arguments and stores them as `MaxRetries` and `RetryDelay`:

- `maxRetries` must be zero or greater; a negative value throws `ArgumentOutOfRangeException`.
- `delay` must be zero or greater; a negative value throws `ArgumentOutOfRangeException`.

**The values are recorded but not enforced.** `Build()` does not add a retry handler or delegating handler, and the builder does not retry requests. The values appear only in the log message emitted by `Build()`.

### `WithCompression(bool enabled)`

Stores `UseCompression`. When `Build()` runs, `true` sets `AutomaticDecompression` on the `HttpClientHandler` to GZip and Deflate, and `false` sets it to `DecompressionMethods.None`. This controls only response decompression; the client does not send an `Accept-Encoding` header.

### `WithHeader(string name, string value)`

Validates the arguments: `name` must not be null, empty, or whitespace (`ArgumentException`), and `value` must not be null (`ArgumentNullException`). It stores the pair in `DefaultHeaders`, so a second call with the same name replaces the earlier value in the builder's dictionary.

`Build()` adds each stored pair to `DefaultRequestHeaders` with `Add`. The method does not check whether the header name or value is valid HTTP syntax; an invalid value causes `Build()` to throw. Passing `User-Agent` is not a replacement for the user agent. `Build()` always adds `UserAgent` first, so a `User-Agent` supplied here is added as an additional value.

### `WithMaxConnectionsPerHost(int connections)`

Validates that `connections` is zero or greater (`ArgumentOutOfRangeException` otherwise) and stores it as `MaxConnectionsPerHost`.

**The value is recorded but not applied.** `Build()` does not set `HttpClientHandler.MaxConnectionsPerHost`, so the built client uses the .NET runtime's default connection limit regardless of this setting.

### `Build()`

Creates a new `HttpClientHandler` and `HttpClient` each time it is called. In order, it:

1. Sets `AutomaticDecompression` from `UseCompression` (GZip and Deflate, or none).
2. Creates the `HttpClient` with the handler and sets `Timeout` from the configured value.
3. Adds the `UserAgent` value to `DefaultRequestHeaders` as `User-Agent`.
4. Adds every entry from the header dictionary to `DefaultRequestHeaders`.
5. Logs an information message with the timeout, retry count, and compression flag.
6. Returns the client.

The builder does not add an `Accept` header. Only the headers supplied through `WithHeader` are sent besides `User-Agent`.

The returned client is not cached. The caller owns it and should dispose it. `Build()` can throw if the timeout is out of range or if a header value is invalid, as described above.

### Defaults when nothing is configured

`new HttpClientBuilder(logger).Build()` produces a client with a 30-second timeout, GZip and Deflate decompression, `User-Agent: DotNetRealtimePipeline/1.0`, and no other default headers.

## Example

```csharp
using DotNetRealtimePipeline.Integration;
using Microsoft.Extensions.Logging.Abstractions;

var builder = new HttpClientBuilder(NullLogger<HttpClientBuilder>.Instance);

// Each With* call returns the same builder, so calls chain.
using HttpClient client = builder
    .WithTimeout(TimeSpan.FromSeconds(10))
    .WithRetry(5, TimeSpan.FromMilliseconds(500))   // recorded only; not applied by Build()
    .WithCompression(true)
    .WithHeader("Accept", "application/json")
    .WithHeader("X-Correlation-Source", "pipeline")
    .WithMaxConnectionsPerHost(20)                  // recorded only; not applied by Build()
    .Build();

client.BaseAddress = new Uri("https://api.example/");
string body = await client.GetStringAsync("status");

// Build() creates a new client from the same settings; the first client is unaffected.
using HttpClient second = builder.WithTimeout(TimeSpan.FromSeconds(5)).Build();

// Default settings: 30 s timeout, compression on, User-Agent DotNetRealtimePipeline/1.0.
using HttpClient defaults = new HttpClientBuilder(NullLogger<HttpClientBuilder>.Instance).Build();
```

The `WithRetry` and `WithMaxConnectionsPerHost` calls in this example are stored in the builder's configuration but have no effect on the client that `Build()` returns. Retries must be implemented by the caller, and the connection limit must be set on the handler directly if it is needed.
