// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetRealtimePipeline.Examples;

/// <summary>
/// Windowing and aggregation example demonstrating time-based window creation and statistical calculations.
/// </summary>
public class WindowingAggregationExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Windowing & Aggregation Example ===\n");

        var services = new ServiceCollection();
        services.AddPipelineServices(config =>
        {
            config.WindowSizeMs = 10000;    // 10-second windows
            config.WindowSlideMs = 5000;    // 5-second slide
            config.WindowType = WindowType.SLIDING;
        });
        var serviceProvider = services.BuildServiceProvider();

        var windowingService = serviceProvider.GetRequiredService<WindowingService>();
        var processingService = serviceProvider.GetRequiredService<DataProcessingService>();

        try
        {
            // Generate time-series data
            Console.WriteLine("Generating time-series data...");
            var dataPoints = GenerateTimeSeriesData(500);
            Console.WriteLine($"✓ Generated {dataPoints.Count} data points\n");

            // Assign to windows
            Console.WriteLine("Assigning data points to windows...");
            var windows = windowingService.AssignDataPointsToWindows(dataPoints);
            Console.WriteLine($"✓ Created {windows.Count} windows\n");

            // Calculate statistics for each window
            Console.WriteLine("=== Window Statistics ===\n");
            foreach (var window in windows.Take(5)) // Show first 5 windows
            {
                var stats = windowingService.CalculateWindowStatistics(window);

                var startTime = new DateTime(window.StartTimeMs * 10000, DateTimeKind.Utc);
                var endTime = new DateTime(window.EndTimeMs * 10000, DateTimeKind.Utc);

                Console.WriteLine($"Window [{startTime:HH:mm:ss}-{endTime:HH:mm:ss}]");
                Console.WriteLine($"  Count: {stats.Count}");
                Console.WriteLine($"  Sum: {stats.Sum:F2}");
                Console.WriteLine($"  Average: {stats.Average:F2}");
                Console.WriteLine($"  Min: {stats.Minimum:F2}");
                Console.WriteLine($"  Max: {stats.Maximum:F2}");
                Console.WriteLine($"  StdDev: {stats.StandardDeviation:F2}");
                Console.WriteLine($"  Percentiles: P50={stats.Percentile50:F2}, P95={stats.Percentile95:F2}, P99={stats.Percentile99:F2}\n");
            }

            // Quality analysis
            Console.WriteLine("=== Data Quality Analysis ===\n");
            var qualityAnalysis = processingService.AnalyzeDataQuality(dataPoints);
            Console.WriteLine($"Average Quality Score: {qualityAnalysis.AverageQualityScore:P2}");
            Console.WriteLine($"Valid Points: {qualityAnalysis.ValidPointsCount}/{dataPoints.Count}");
            Console.WriteLine($"Invalid Points: {dataPoints.Count - qualityAnalysis.ValidPointsCount}");

            Console.WriteLine("\n✓ Windowing aggregation example completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    private static List<DataPoint> GenerateTimeSeriesData(int pointCount)
    {
        var dataPoints = new List<DataPoint>();
        var baseTime = DateTime.UtcNow.AddHours(-1).Ticks;

        for (int i = 0; i < pointCount; i++)
        {
            // Generate realistic time-series with trend and noise
            double trend = i * 0.1; // Slowly increasing trend
            double noise = (Random.Shared.NextDouble() - 0.5) * 10;
            double value = 50 + trend + noise;

            var dataPoint = new DataPoint(
                id: i,
                timestamp: baseTime + (i * 10000000), // Each point 1 second apart
                value: (decimal)Math.Max(0, value),
                source: "TimeSeries-1"
            );

            dataPoints.Add(dataPoint);
        }

        return dataPoints;
    }
}
