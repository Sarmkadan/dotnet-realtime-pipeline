# PipelineHttpClientFactoryExtensions

Provides factory methods and convenience extensions for creating and using pre-configured `HttpClient` instances within the dotnet-realtime-pipeline framework. This type centralizes HTTP client creation with base addresses and service-specific configurations, and offers simplified asynchronous methods for common HTTP operations like GET and POST with JSON payloads. It also exposes logging and diagnostic capabilities through standard logging interfaces.

## API

### CreateClientWithBaseAddress

```csharp
public static HttpClient CreateClientWithBaseAddress(string baseAddress)
```

Creates a new `HttpClient` with its `BaseAddress` property set to the specified URI string. The client is not otherwise pre-configured with default headers or handlers beyond the base address assignment.

**Parameters:**
- `baseAddress` — A string representing the base URI that will be prepended to relative request URIs.

**Returns:** A new `HttpClient` instance with `BaseAddress` initialized.

**Throws:** `UriFormatException` if `baseAddress` is not a valid absolute URI.

---

### CreateConfiguredServiceClient

```csharp
public static HttpClient CreateConfiguredServiceClient(string serviceName)
```

Creates an `HttpClient` tailored for a named service within the pipeline ecosystem. The returned client includes default request headers, timeout settings, and message handlers appropriate for inter-service communication as defined by the pipeline configuration.

**Parameters:**
- `serviceName` — The logical name of the target service, used to resolve configuration.

**Returns:** A fully configured `HttpClient` ready for communication with the named service.

**Throws:** `InvalidOperationException` if no configuration exists for the given `serviceName`. `ArgumentException` if `serviceName` is null or empty.

---

### GetStringAsync

```csharp
public static async Task<string> GetStringAsync(this HttpClient client, string requestUri)
```

Sends an asynchronous GET request to the specified URI and returns the response body as a string. This is an extension method on `HttpClient`.

**Parameters:**
- `client` — The `HttpClient` on which the extension method is invoked.
- `requestUri` — The relative or absolute URI to send the GET request to.

**Returns:** A task that completes with the response body content as a string.

**Throws:** `HttpRequestException` on network failure or non-success status codes. `TaskCanceledException` if the request times out. `ArgumentNullException` if `requestUri` is null.

---

### PostJsonAsync

```csharp
public static async Task<string> PostJsonAsync(this HttpClient client, string requestUri, object payload)
```

Sends an asynchronous POST request with a JSON-serialized body to the specified URI and returns the response body as a string. The payload object is serialized using the pipeline's default JSON serializer settings.

**Parameters:**
- `client` — The `HttpClient` on which the extension method is invoked.
- `requestUri` — The relative or absolute URI to send the POST request to.
- `payload` — An object that will be serialized to JSON as the request body.

**Returns:** A task that completes with the response body content as a string.

**Throws:** `HttpRequestException` on network failure or non-success status codes. `TaskCanceledException` if the request times out. `ArgumentNullException` if `requestUri` or `payload` is null. `JsonSerializationException` if the payload cannot be serialized.

---

### BeginScope\<TState\>

```csharp
public IDisposable? BeginScope<TState>(TState state)
```

Begins a logical operation scope for logging purposes. Returns an `IDisposable` that, when disposed, ends the scope. This follows the standard `ILogger` pattern for scoped logging contexts.

**Parameters:**
- `state` — The state object associated with the scope.

**Returns:** An `IDisposable` that ends the scope on disposal, or null if scopes are not supported by the underlying logger.

---

### IsEnabled

```csharp
public bool IsEnabled { get; }
```

Gets a value indicating whether logging is enabled for this instance at the current log level. Consumers can check this property before performing expensive log message formatting.

---

### Log\<TState\>

```csharp
public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
```

Writes a log entry with the specified log level, event identifier, state, optional exception, and a formatter function that produces the log message string.

**Parameters:**
- `logLevel` — The severity level of the log entry.
- `eventId` — The event identifier for categorizing the log entry.
- `state` — The state object to be logged.
- `exception` — An optional exception associated with the log entry.
- `formatter` — A function that converts the state and exception into a string message.

---

## Usage

### Example 1: Creating a client with a base address and performing a GET request

```csharp
// Create a client pointed at a known API base
HttpClient client = PipelineHttpClientFactoryExtensions.CreateClientWithBaseAddress("https://api.example.com/");

// Use the extension method to fetch a resource as a string
string response = await client.GetStringAsync("v1/status");

Console.WriteLine(response);
```

### Example 2: Creating a configured service client and posting JSON data

```csharp
// Obtain a client pre-configured for the "orchestrator" service
HttpClient orchestratorClient = PipelineHttpClientFactoryExtensions.CreateConfiguredServiceClient("orchestrator");

// Post a JSON payload and capture the response
var payload = new { Command = "start", Parameters = new { Timeout = 30 } };
string result = await orchestratorClient.PostJsonAsync("api/commands", payload);

Console.WriteLine($"Orchestrator response: {result}");
```

## Notes

- `CreateClientWithBaseAddress` does not set any default headers or timeout values; the caller is responsible for further configuration if needed. The base address must be an absolute URI ending with a trailing slash if relative URIs in subsequent requests are to be resolved correctly.
- `CreateConfiguredServiceClient` relies on pipeline configuration being loaded and accessible. If the configuration source is unavailable or the service name is not registered, an `InvalidOperationException` is thrown at creation time, not deferred to first use.
- `GetStringAsync` and `PostJsonAsync` do not automatically retry on transient failures. Callers should implement retry logic externally if required.
- `PostJsonAsync` uses the pipeline's internal JSON serializer settings, which may differ from `System.Text.Json` defaults. Circular references or non-serializable types will cause a `JsonSerializationException`.
- The `BeginScope`, `IsEnabled`, and `Log` members indicate that `PipelineHttpClientFactoryExtensions` implements `ILogger` or a compatible logging interface. These members are thread-safe in accordance with standard .NET logging guidelines; scopes are typically not shared across threads.
- `HttpClient` instances created by these factory methods are intended to be long-lived and reused. Creating and disposing them per request defeats connection pooling and may lead to socket exhaustion under load.
