#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetRealtimePipeline.Examples;

/// <summary>
/// Data querying example demonstrating various search and analysis operations.
/// </summary>
public sealed class QueryingDataExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Data Querying Example ===\n");

        var services = new ServiceCollection();
        services.AddPipelineServices();
        var serviceProvider = services.BuildServiceProvider();

        var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();
        var queryService = serviceProvider.GetRequiredService<QueryService>();

        try
        {
            await orchestrator.StartAsync();
            Console.WriteLine("Pipeline started\n");

            // Ingest test data from multiple sources
            Console.WriteLine("Ingesting test data from multiple sources...");
            await IngestTestDataAsync(orchestrator);
            Console.WriteLine("✓ Test data ingested\n");

            // Wait for processing
            await Task.Delay(2000);

            // Query 1: Search in time range
            Console.WriteLine("=== Query 1: Time Range Search ===");
            var now = DateTime.UtcNow.Ticks;
            var oneHourAgo = DateTime.UtcNow.AddHours(-1).Ticks;

            var timeRangeResults = await queryService.SearchDataPointsAsync(
                startTime: oneHourAgo,
                endTime: now,
                source: null,
                minQualityScore: 0);

            Console.WriteLine($"Found {timeRangeResults.Count()} points in the last hour\n");

            // Query 2: Search by source
            Console.WriteLine("=== Query 2: Search by Source ===");
            var sensorResults = await queryService.SearchDataPointsAsync(
                startTime: oneHourAgo,
                endTime: now,
                source: "Temperature-1",
                minQualityScore: 0);

            Console.WriteLine($"Found {sensorResults.Count()} points from Temperature-1");
            if (sensorResults.Any())
            {
                var sample = sensorResults.First();
                Console.WriteLine($"  Sample: ID={sample.Id}, Value={sample.Value}, Quality={sample.Quality:P2}\n");
            }

            // Query 3: Quality-based filtering
            Console.WriteLine("=== Query 3: Quality-Based Filtering ===");
            var highQualityResults = await queryService.SearchDataPointsAsync(
                startTime: oneHourAgo,
                endTime: now,
                source: null,
                minQualityScore: 0.8m);

            Console.WriteLine($"Found {highQualityResults.Count()} high-quality points (>0.8)");
            var avgQuality = highQualityResults.Any() ? highQualityResults.Average(p => (double)p.Quality) : 0;
            Console.WriteLine($"Average quality: {avgQuality:P2}\n");

            // Query 4: Aggregate statistics
            Console.WriteLine("=== Query 4: Aggregate Statistics ===");
            var startMs = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds();
            var endMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var statistics = await queryService.GetAggregateStatisticsAsync(startMs, endMs);
            Console.WriteLine($"Total Points: {statistics.Count}");
            Console.WriteLine($"Average: {statistics.Average:F2}");
            Console.WriteLine($"Min: {statistics.Minimum:F2}");
            Console.WriteLine($"Max: {statistics.Maximum:F2}");
            Console.WriteLine($"StdDev: {statistics.StandardDeviation:F2}\n");

            // Query 5: Trend analysis
            Console.WriteLine("=== Query 5: Trend Analysis ===");
            var trends = await queryService.AnalyzeTrendsAsync(
                startMs: startMs,
                endMs: endMs,
                intervalMs: 60000); // 1-minute intervals

            Console.WriteLine($"Found {trends.Count} trend points");
            if (trends.Any())
            {
                Console.WriteLine("Trend samples:");
                foreach (var trend in trends.Take(3))
                {
                    var time = DateTimeOffset.FromUnixTimeMilliseconds(trend.TimeMs).DateTime;
                    Console.WriteLine($"  [{time:HH:mm:ss}] Value={trend.Value:F2}");
                }
            }

            Console.WriteLine("\n✓ Querying example completed successfully");
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }

    private static async Task IngestTestDataAsync(PipelineOrchestrator orchestrator)
    {
        var baseTime = DateTime.UtcNow.AddHours(-1).Ticks;
        string[] sensors = { "Temperature-1", "Temperature-2", "Humidity-1" };

        for (int hour = 0; hour < 2; hour++)
        {
            foreach (var sensor in sensors)
            {
                for (int i = 0; i < 100; i++)
                {
                    var dataPoint = new DataPoint(
                        id: Random.Shared.Next(100000),
                        timestamp: baseTime + ((hour * 3600 + i * 36) * 10000000),
                        value: (decimal)(20 + Random.Shared.NextDouble() * 10),
                        source: sensor
                    );

                    await orchestrator.IngestDataPointAsync(dataPoint);
                }
            }
        }
    }
}
