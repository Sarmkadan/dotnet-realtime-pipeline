#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

/// <summary>
/// Factory for creating and configuring HTTP clients with retry and timeout policies.
/// </summary>
public class PipelineHttpClientFactory
{
    private readonly ILogger<PipelineHttpClientFactory> _logger;
    private readonly Dictionary<string, HttpClient> _clients = new();

    public PipelineHttpClientFactory(ILogger<PipelineHttpClientFactory> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates an HTTP client with default configuration.
    /// </summary>
    public HttpClient CreateDefaultClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 3
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        client.DefaultRequestHeaders.Add("User-Agent", "DotNetRealtimePipeline/1.0");
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        return client;
    }

    /// <summary>
    /// Creates an HTTP client for a specific service with cached instance.
    /// </summary>
    public HttpClient CreateServiceClient(string serviceName, TimeSpan timeout, bool useCompression = true)
    {
        if (_clients.TryGetValue(serviceName, out var cached))
        {
            return cached;
        }

        var handler = new HttpClientHandler
        {
            AutomaticDecompression = useCompression ? (DecompressionMethods.GZip | DecompressionMethods.Deflate) : DecompressionMethods.None,
            AllowAutoRedirect = true,
            Credentials = CredentialCache.DefaultCredentials
        };

        var client = new HttpClient(handler)
        {
            Timeout = timeout
        };

        ConfigureHeaders(client, serviceName);

        _clients[serviceName] = client;
        _logger.LogInformation("HTTP client created for service: {Service} (timeout: {Timeout}ms)",
            serviceName, timeout.TotalMilliseconds);

        return client;
    }

    /// <summary>
    /// Configures default request headers for a client.
    /// </summary>
    private static void ConfigureHeaders(HttpClient client, string serviceName)
    {
        client.DefaultRequestHeaders.Add("User-Agent", $"DotNetRealtimePipeline/{serviceName}");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
    }

    /// <summary>
    /// Gets or creates a client for a specific endpoint.
    /// </summary>
    public HttpClient GetOrCreateClient(string key, Action<HttpClient> configure)
    {
        if (_clients.TryGetValue(key, out var client))
        {
            return client;
        }

        var newClient = CreateDefaultClient();
        configure?.Invoke(newClient);

        _clients[key] = newClient;
        return newClient;
    }

    /// <summary>
    /// Removes a cached client.
    /// </summary>
    public void RemoveClient(string serviceName)
    {
        if (_clients.ContainsKey(serviceName))
        {
            var client = _clients[serviceName];
            _clients.Remove(serviceName);
            client?.Dispose();
            _logger.LogInformation("HTTP client removed for service: {Service}", serviceName);
        }
    }

    /// <summary>
    /// Clears all cached clients.
    /// </summary>
    public void ClearClients()
    {
        foreach (var client in _clients.Values)
        {
            client?.Dispose();
        }

        _clients.Clear();
        _logger.LogInformation("All HTTP clients cleared");
    }
}

/// <summary>
/// Configuration for HTTP client behavior.
/// </summary>
public class HttpClientConfiguration
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    public int MaxConnectionsPerHost { get; set; } = 10;
    public bool UseCompression { get; set; } = true;
    public string UserAgent { get; set; } = "DotNetRealtimePipeline/1.0";
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
}

/// <summary>
/// Builder pattern for fluent HTTP client configuration.
/// </summary>
public class HttpClientBuilder
{
    private readonly HttpClientConfiguration _config = new();
    private readonly ILogger<HttpClientBuilder> _logger;

    public HttpClientBuilder(ILogger<HttpClientBuilder> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sets the request timeout.
    /// </summary>
    public HttpClientBuilder WithTimeout(TimeSpan timeout)
    {
        _config.Timeout = timeout;
        return this;
    }

    /// <summary>
    /// Sets retry configuration.
    /// </summary>
    public HttpClientBuilder WithRetry(int maxRetries, TimeSpan delay)
    {
        _config.MaxRetries = maxRetries;
        _config.RetryDelay = delay;
        return this;
    }

    /// <summary>
    /// Enables or disables compression.
    /// </summary>
    public HttpClientBuilder WithCompression(bool enabled)
    {
        _config.UseCompression = enabled;
        return this;
    }

    /// <summary>
    /// Adds a default header.
    /// </summary>
    public HttpClientBuilder WithHeader(string name, string value)
    {
        _config.DefaultHeaders[name] = value;
        return this;
    }

    /// <summary>
    /// Sets max connections per host.
    /// </summary>
    public HttpClientBuilder WithMaxConnectionsPerHost(int connections)
    {
        _config.MaxConnectionsPerHost = connections;
        return this;
    }

    /// <summary>
    /// Builds the configured HTTP client.
    /// </summary>
    public HttpClient Build()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = _config.UseCompression ? (DecompressionMethods.GZip | DecompressionMethods.Deflate) : DecompressionMethods.None
        };

        var client = new HttpClient(handler)
        {
            Timeout = _config.Timeout
        };

        client.DefaultRequestHeaders.Add("User-Agent", _config.UserAgent);

        foreach (var header in _config.DefaultHeaders)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        _logger.LogInformation("HTTP client built with timeout={Timeout}, retries={Retries}, compression={Compression}",
            _config.Timeout, _config.MaxRetries, _config.UseCompression);

        return client;
    }
}
