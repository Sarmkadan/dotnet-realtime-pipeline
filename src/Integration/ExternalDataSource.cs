// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Interface for external data sources.
/// </summary>
public interface IExternalDataSource
{
    Task<List<DataPoint>> FetchDataAsync(DateTime startTime, DateTime endTime);
    Task<bool> IsAvailableAsync();
}

/// <summary>
/// HTTP-based external data source connector.
/// </summary>
public class HttpDataSource : IExternalDataSource
{
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpDataSource> _logger;

    public HttpDataSource(string baseUrl, HttpClient httpClient, ILogger<HttpDataSource> logger)
    {
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Fetches data from an HTTP endpoint.
    /// </summary>
    public async Task<List<DataPoint>> FetchDataAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            var startMs = new DateTimeOffset(startTime).ToUnixTimeMilliseconds();
            var endMs = new DateTimeOffset(endTime).ToUnixTimeMilliseconds();

            var url = $"{_baseUrl}/data?start={startMs}&end={endMs}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("HTTP data source returned {StatusCode}", response.StatusCode);
                return new List<DataPoint>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var dataPoints = JsonSerializer.Deserialize<List<DataPoint>>(content);

            _logger.LogInformation("Fetched {Count} data points from {Url}", dataPoints?.Count ?? 0, _baseUrl);
            return dataPoints ?? new List<DataPoint>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from HTTP source");
            return new List<DataPoint>();
        }
    }

    /// <summary>
    /// Checks if the HTTP source is available.
    /// </summary>
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Manager for multiple external data sources with fallback support.
/// </summary>
public class DataSourceManager
{
    private readonly List<DataSourceConnection> _sources = new();
    private readonly ILogger<DataSourceManager> _logger;

    public DataSourceManager(ILogger<DataSourceManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers an external data source.
    /// </summary>
    public void Register(string name, IExternalDataSource source, int priority = 0)
    {
        _sources.Add(new DataSourceConnection
        {
            Name = name,
            Source = source,
            Priority = priority,
            IsHealthy = true
        });

        _sources.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        _logger.LogInformation("Registered data source: {Name} (priority: {Priority})", name, priority);
    }

    /// <summary>
    /// Fetches data from available sources with fallback.
    /// </summary>
    public async Task<List<DataPoint>> FetchDataAsync(DateTime startTime, DateTime endTime)
    {
        var healthySources = _sources.Where(s => s.IsHealthy).ToList();

        foreach (var connection in healthySources)
        {
            try
            {
                var available = await connection.Source.IsAvailableAsync();
                if (!available)
                {
                    connection.IsHealthy = false;
                    _logger.LogWarning("Data source {Name} is unavailable", connection.Name);
                    continue;
                }

                var data = await connection.Source.FetchDataAsync(startTime, endTime);
                _logger.LogInformation("Successfully fetched {Count} points from {Name}", data.Count, connection.Name);
                return data;
            }
            catch (Exception ex)
            {
                connection.IsHealthy = false;
                _logger.LogError(ex, "Error fetching from data source {Name}", connection.Name);
            }
        }

        _logger.LogWarning("All data sources are unavailable");
        return new List<DataPoint>();
    }

    /// <summary>
    /// Gets the health status of all registered sources.
    /// </summary>
    public Dictionary<string, bool> GetSourceHealth()
    {
        return _sources.ToDictionary(s => s.Name, s => s.IsHealthy);
    }

    private class DataSourceConnection
    {
        public string Name { get; set; }
        public IExternalDataSource Source { get; set; }
        public int Priority { get; set; }
        public bool IsHealthy { get; set; }
    }
}

/// <summary>
/// Cache layer for external data sources to reduce network calls.
/// </summary>
public class CachedDataSource : IExternalDataSource
{
    private readonly IExternalDataSource _innerSource;
    private readonly Caching.CacheService<string, List<DataPoint>> _cache;
    private readonly ILogger<CachedDataSource> _logger;

    public CachedDataSource(IExternalDataSource innerSource, ILogger<CachedDataSource> logger)
    {
        _innerSource = innerSource ?? throw new ArgumentNullException(nameof(innerSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = new Caching.CacheService<string, List<DataPoint>>(1000, TimeSpan.FromHours(1));
    }

    /// <summary>
    /// Fetches data with caching.
    /// </summary>
    public async Task<List<DataPoint>> FetchDataAsync(DateTime startTime, DateTime endTime)
    {
        var cacheKey = $"{startTime:O}_{endTime:O}";

        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            _logger.LogDebug("Cache hit for {Key}", cacheKey);
            return cached;
        }

        var data = await _innerSource.FetchDataAsync(startTime, endTime);
        _cache.Set(cacheKey, data, TimeSpan.FromHours(1));

        _logger.LogDebug("Cache miss, fetched {Count} items", data.Count);
        return data;
    }

    /// <summary>
    /// Checks availability of the inner source.
    /// </summary>
    public async Task<bool> IsAvailableAsync()
    {
        return await _innerSource.IsAvailableAsync();
    }

    /// <summary>
    /// Clears the cache.
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
        _logger.LogInformation("Data source cache cleared");
    }
}
