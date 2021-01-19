// existing content ...

// ## InMemoryMetricsRepository
// 
// `InMemoryMetricsRepository` is an in-memory implementation of the metrics repository. 
// Maintains a rolling history of metrics for analysis. 

// ```csharp
// using DotNetRealtimePipeline.Data.Repositories;
// using DotNetRealtimePipeline.Domain.Models;

// var metricsRepository = new InMemoryMetricsRepository();

// // Save a metric aggregation
// var metric = new MetricAggregation(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeMilliseconds(), "test")
// {
//     TotalItemsProcessed = 1500,
//     TotalItemsFailed = 5,
//     AverageProcessingTimeMs = 45.2
// };
// await metricsRepository.SaveAsync(metric);

// // Get a metric by ID
// var savedMetric = await metricsRepository.GetByIdAsync(1);
// Console.WriteLine($"Saved metric ID: {savedMetric?.MetricId}");

// // Get metrics within a time window
// var metrics = await metricsRepository.GetByTimeRangeAsync(
//     DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds(),
//     DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
// );
// Console.WriteLine($"Metrics in time range: {metrics.Count}");

// // Clear all metrics
// metricsRepository.Clear();
// ```
