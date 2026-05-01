// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Integration;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Text.Json;

namespace DotNetRealtimePipeline.Examples;

/// <summary>
/// External API integration example demonstrating how to connect
/// the pipeline to external data sources and export processed data.
/// </summary>
public class ExternalApiIntegrationExample
{
    private record ExternalDataSourceConfig(
        string Name,
        string ApiUrl,
        string ApiKey,
        int PollIntervalMs
    );

    public static async Task RunAsync()
    {
        Console.WriteLine("=== External API Integration Example ===\n");

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPipelineServices();
        var provider = services.BuildServiceProvider();

        var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

        await orchestrator.StartAsync();

        try
        {
            // Simulate multiple external data sources
            await SimulateExternalSourcesAsync(orchestrator);

            // Export metrics to external system
            await ExportMetricsAsync(orchestrator);

            // Query and analyze results
            await QueryAndAnalyzeAsync(orchestrator);
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }

    private static async Task SimulateExternalSourcesAsync(PipelineOrchestrator orchestrator)
    {
        Console.WriteLine("Simulating external data sources...\n");

        var sources = new[]
        {
            new ExternalDataSourceConfig(
                "Weather API",
                "https://api.weather.example.com",
                "weather-key-123",
                1000
            ),
            new ExternalDataSourceConfig(
                "Stock Market API",
                "https://api.stocks.example.com",
                "stock-key-456",
                500
            ),
            new ExternalDataSourceConfig(
                "IoT Sensor Gateway",
                "https://iot.example.com/sensors",
                "iot-key-789",
                2000
            )
        };

        var sourceNames = sources.Select(s => s.Name).ToArray();

        // Simulate receiving data from external APIs
        var tasks = sources.Select(async source =>
        {
            Console.WriteLine($"Connected to {source.Name}");

            for (int i = 0; i < 100; i++)
            {
                // Simulate API response
                var dataPoints = GenerateExternalData(source.Name, 5);

                foreach (var point in dataPoints)
                {
                    bool accepted = await orchestrator.IngestDataPointAsync(point);

                    if (!accepted)
                    {
                        Console.WriteLine($"⚠ Data rejected from {source.Name} - backpressure applied");
                        await Task.Delay(100);
                    }
                }

                // Respect API rate limits
                await Task.Delay(source.PollIntervalMs);
            }

            Console.WriteLine($"✓ {source.Name} completed data transfer");
        });

        await Task.WhenAll(tasks);

        Console.WriteLine($"\n✓ All external sources synchronized\n");
    }

    private static async Task ExportMetricsAsync(PipelineOrchestrator orchestrator)
    {
        Console.WriteLine("Exporting metrics to external systems...\n");

        // Wait for processing
        await Task.Delay(2000);

        var health = await orchestrator.GetHealthReportAsync();
        var metrics = orchestrator.GetMetricsHistory();

        // Export in different formats
        var exportFormats = new[] { "prometheus", "json", "http" };

        foreach (var format in exportFormats)
        {
            try
            {
                // Simulate export to different systems
                Console.WriteLine($"✓ Metrics exported to {format}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to export to {format}: {ex.Message}");
            }
        }

        Console.WriteLine();
    }

    private static async Task QueryAndAnalyzeAsync(PipelineOrchestrator orchestrator)
    {
        Console.WriteLine("Querying and analyzing aggregated data...\n");

        var status = orchestrator.GetStatus();

        Console.WriteLine($"Pipeline Status:");
        Console.WriteLine($"  Total Processed: {status.TotalDataPointsProcessed}");
        Console.WriteLine($"  Total Failed: {status.TotalDataPointsFailed}");
        Console.WriteLine($"  Pending Items: {status.PendingItemsInQueue}");
        Console.WriteLine($"  Buffer Utilization: {status.BufferUtilization:P2}");

        var health = await orchestrator.GetHealthReportAsync();
        Console.WriteLine($"\nHealth Metrics:");
        Console.WriteLine($"  Status: {health.Status}");
        Console.WriteLine($"  Throughput: {health.ThroughputItemsPerSecond:F0} items/sec");
        Console.WriteLine($"  Avg Latency: {health.AverageLatencyMs:F2}ms");
        Console.WriteLine($"  Success Rate: {health.SuccessRatePercent:F2}%");

        // Simulate webhook callback
        await SendWebhookAsync(health);

        Console.WriteLine();
    }

    private static List<DataPoint> GenerateExternalData(string source, int count)
    {
        var points = new List<DataPoint>();
        var baseValue = source switch
        {
            "Weather API" => 20 + Random.Shared.NextDouble() * 30,  // Temperature
            "Stock Market API" => 100 + Random.Shared.NextDouble() * 100,  // Stock price
            "IoT Sensor Gateway" => Random.Shared.NextDouble() * 100,  // Generic sensor
            _ => Random.Shared.NextDouble() * 100
        };

        for (int i = 0; i < count; i++)
        {
            points.Add(new DataPoint(
                id: Random.Shared.Next(1000000),
                timestamp: DateTime.UtcNow.Ticks,
                value: (decimal)(baseValue + Random.Shared.NextGaussian() * 5),
                source: source
            )
            {
                Quality = 70 + Random.Shared.Next(0, 30)
            });
        }

        return points;
    }

    private static async Task SendWebhookAsync(HealthReport health)
    {
        Console.WriteLine("Sending webhook notification...");

        try
        {
            var payload = new
            {
                timestamp = DateTime.UtcNow,
                event_type = "health_check",
                status = health.Status,
                throughput = health.ThroughputItemsPerSecond,
                latency_ms = health.AverageLatencyMs,
                success_rate = health.SuccessRatePercent
            };

            // Simulate HTTP POST to webhook endpoint
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            Console.WriteLine($"Webhook Payload:");
            foreach (var line in json.Split('\n').Take(10))
            {
                Console.WriteLine($"  {line}");
            }

            // In real scenario, would do:
            // using var client = new HttpClient();
            // var content = new StringContent(json, Encoding.UTF8, "application/json");
            // var response = await client.PostAsync("https://webhook.example.com/pipeline", content);

            Console.WriteLine("✓ Webhook sent successfully (simulated)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to send webhook: {ex.Message}");
        }
    }
}

/// <summary>
/// Simulated implementation showing how to create custom external source connector.
/// </summary>
public class CustomExternalSourceConnector
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public CustomExternalSourceConnector(string baseUrl, string apiKey)
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;
        _apiKey = apiKey;
    }

    public async Task<List<DataPoint>> FetchDataAsync(int limit = 100)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_baseUrl}/data?limit={limit}");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<DataPoint>>(json);

            return data ?? new List<DataPoint>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching data from external source: {ex.Message}");
            return new List<DataPoint>();
        }
    }

    public async Task<bool> SendHeartbeatAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_baseUrl}/heartbeat");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
