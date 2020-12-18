#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="PipelineHttpClientFactory"/> that provide additional HTTP client functionality.
/// </summary>
public static class PipelineHttpClientFactoryExtensions
{
    /// <summary>
    /// Creates an HTTP client with a specific base address.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    /// <param name="baseAddress">The base address for the HTTP client.</param>
    /// <returns>A configured HTTP client with the specified base address.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseAddress"/> is null.</exception>
    public static HttpClient CreateClientWithBaseAddress(this PipelineHttpClientFactory factory, string baseAddress)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentException.ThrowIfNullOrEmpty(baseAddress);

        var client = factory.CreateDefaultClient();
        client.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
        return client;
    }

    /// <summary>
    /// Creates an HTTP client for a specific service with custom timeout and retry configuration.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="timeout">The request timeout.</param>
    /// <param name="maxRetries">The maximum number of retry attempts.</param>
    /// <param name="retryDelay">The delay between retry attempts.</param>
    /// <param name="useCompression">Whether to enable compression.</param>
    /// <returns>A configured HTTP client for the specified service.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceName"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeout"/> or <paramref name="retryDelay"/> is negative.</exception>
    public static HttpClient CreateConfiguredServiceClient(
        this PipelineHttpClientFactory factory,
        string serviceName,
        TimeSpan timeout,
        int maxRetries = 3,
        TimeSpan? retryDelay = null,
        bool useCompression = true)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);

        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive.");
        }

        if (maxRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "Max retries cannot be negative.");
        }

        var client = factory.CreateServiceClient(serviceName, timeout, useCompression);

        // Apply custom retry configuration via builder pattern
        var builder = new HttpClientBuilder(new NullLogger<HttpClientBuilder>());
        builder.WithTimeout(timeout);
        builder.WithRetry(maxRetries, retryDelay ?? factory.RetryDelay);
        builder.WithCompression(useCompression);

        // Copy configuration to the client
        var config = new HttpClientConfiguration
        {
            Timeout = timeout,
            MaxRetries = maxRetries,
            RetryDelay = retryDelay ?? factory.RetryDelay,
            UseCompression = useCompression,
            UserAgent = $"{factory.UserAgent} ({serviceName})"
        };

        client.Timeout = config.Timeout;
        client.DefaultRequestHeaders.Add("User-Agent", config.UserAgent);

        return client;
    }

    /// <summary>
    /// Executes an HTTP GET request and returns the response as a string.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="clientKey">Optional key to identify the client in the factory cache.</param>
    /// <param name="configureClient">Optional action to configure the HTTP client before use.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The response body as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestUri"/> is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    public static async Task<string> GetStringAsync(
        this PipelineHttpClientFactory factory,
        string requestUri,
        string? clientKey = null,
        Action<HttpClient>? configureClient = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentException.ThrowIfNullOrEmpty(requestUri);

        HttpClient client;
        if (clientKey is not null)
        {
            client = factory.GetOrCreateClient(clientKey, configureClient);
        }
        else
        {
            client = factory.CreateDefaultClient();
            configureClient?.Invoke(client);
        }

        var response = await client.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an HTTP POST request with JSON content and returns the response as a string.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="content">The request content.</param>
    /// <param name="clientKey">Optional key to identify the client in the factory cache.</param>
    /// <param name="configureClient">Optional action to configure the HTTP client before use.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The response body as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestUri"/> or <paramref name="content"/> is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    public static async Task<string> PostJsonAsync(
        this PipelineHttpClientFactory factory,
        string requestUri,
        HttpContent content,
        string? clientKey = null,
        Action<HttpClient>? configureClient = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrEmpty(requestUri);

        HttpClient client;
        if (clientKey is not null)
        {
            client = factory.GetOrCreateClient(clientKey, configureClient);
        }
        else
        {
            client = factory.CreateDefaultClient();
            configureClient?.Invoke(client);
        }

        var response = await client.PostAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
}

// Null logger implementation for use in extension methods where logger is required but not available
internal sealed class NullLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => false;

    public void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        Microsoft.Extensions.Logging.EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) => _ = exception;
}