# DotNetRealtimePipeline

<!-- existing content ... -->

## DynamicScalingWorker
The `DynamicScalingWorker` class is a background worker that periodically evaluates and adapts the concurrency of pipeline stages in response to observed backpressure. It uses the `DynamicScalingService` to drive scaling decisions. 

## BackgroundProcessingWorker
`BackgroundProcessingWorker` is a long‑running background worker that continuously drives pipeline processing by periodically querying the `PipelineOrchestrator` for status.  
It works together with `MetricsAggregationWorker`, `HealthCheckWorker`, and `WorkerCoordinator` to provide a complete set of background services for a running pipeline.

### Usage example
The following example demonstrates how to create, start, stop, and dispose the workers using their real public members. It assumes that the required services (`PipelineOrchestrator`, `MetricsService`) have parameterless constructors or are otherwise available.

## HealthCheckService

The `HealthCheckService` monitors the health of pipeline components and the overall system. It tracks component statuses, resource usage, and performance metrics, providing both detailed reports and quick status checks.

### Key Features
- Register custom health check components
- Perform complete system health assessments
- Get quick status without detailed checks
- Retrieve component-specific status
- Determine overall system health status

### Usage Examples

#### Basic Setup and Registration
```csharp
// Create required services (simplified for example)
var orchestrator = new PipelineOrchestrator();
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<HealthCheckService>();

// Initialize HealthCheckService
var healthCheckService = new HealthCheckService(orchestrator, logger);

// Register components with health checks
healthCheckService.RegisterComponent("Database", async () => 
{
    try
    {
        // Check database connection
        var isConnected = await CheckDatabaseConnectionAsync();
        return new ComponentHealth
        {
            IsHealthy = isConnected,
            Message = isConnected ? "Database connection OK" : "Cannot connect to database",
            Details = new Dictionary<string, object> { { "ConnectionString", "***" } }
        };
    }
    catch (Exception ex)
    {
        return new ComponentHealth
        {
            IsHealthy = false,
            Message = $"Database check failed: {ex.Message}"
        };
    }
});

healthCheckService.RegisterComponent("Cache", async () => 
{
    var cacheStats = await GetCacheStatisticsAsync();
    return new ComponentHealth
    {
        IsHealthy = cacheStats.HitRate > 0.7,
        Message = $"Cache hit rate: {cacheStats.HitRate:P}",
        Details = new Dictionary<string, object>
        {
            { "HitRate", cacheStats.HitRate },
            { "Misses", cacheStats.Misses },
            { "TotalRequests", cacheStats.TotalRequests }
        }
    };
});
```

#### Complete Health Check
```csharp
var report = await healthCheckService.PerformCompleteHealthCheckAsync();

Console.WriteLine($"Health Report Generated: {report.CheckedAt}");
Console.WriteLine($"Overall Status: {report.OverallStatus}");
Console.WriteLine($"Pipeline Status: {report.PipelineStatus}");
Console.WriteLine($"Throughput: {report.Throughput} items/sec");
Console.WriteLine($"Success Rate: {report.SuccessRate}%");

foreach (var component in report.Components)
{
    Console.WriteLine($"\nComponent: {component.Key}");
    Console.WriteLine($"  Status: {component.Value.IsHealthy}");
    Console.WriteLine($"  Message: {component.Value.Message}");
    Console.WriteLine($"  Checked At: {component.Value.CheckedAt}");
    
    if (component.Value.Details != null)
    {
        foreach (var detail in component.Value.Details)
        {
            Console.WriteLine($"  {detail.Key}: {detail.Value}");
        }
    }
}
```

#### Quick Status Check
```csharp
var quickStatus = await healthCheckService.GetQuickStatusAsync();

if (quickStatus.IsRunning)
{
    Console.WriteLine("Pipeline is running");
    Console.WriteLine($"Health: {quickStatus.HealthStatus}");
    Console.WriteLine($"Pending Items: {quickStatus.PendingItems}");
    Console.WriteLine($"Throughput OK: {quickStatus.ThroughputOk}");
    Console.WriteLine($"Error Rate Acceptable: {quickStatus.ErrorRateAcceptable}");
}
else
{
    Console.WriteLine("Pipeline is not running");
}
```

#### Get Component Status
```csharp
var dbStatus = healthCheckService.GetComponentStatus("Database");
var cacheStatus = healthCheckService.GetComponentStatus("Cache");

Console.WriteLine($"Database Status: {dbStatus}");
Console.WriteLine($"Cache Status: {cacheStatus}");
```

### Return Types

- **`SystemHealthReport`**: Complete health assessment including all components, pipeline metrics, and overall system health
- **`QuickHealthStatus`**: Lightweight status with running state, health status, and key metrics
- **`ComponentHealth`**: Individual component health with status, message, and custom details
- **`ComponentStatus`**: Enum representing component health state (Unknown, Healthy, Degraded, Unhealthy)
- **`SystemHealth`**: Enum representing overall system health state (Unknown, Healthy, Degraded, Unhealthy)

## CacheService
The `CacheService` class provides an in-memory caching mechanism with support for time-to-live (TTL) and eviction policies. It allows for storing and retrieving values based on a given key, as well as tracking cache statistics. Here's an example of how to use the `CacheService`:
