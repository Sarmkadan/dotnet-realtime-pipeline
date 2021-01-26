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

## SerializationHelper

The `SerializationHelper` class provides utility methods for serializing and deserializing pipeline objects to/from JSON, CSV, and other formats. It supports both single objects and collections, with proper escaping and formatting for each format.

### Key Features
- Convert `DataPoint` objects to JSON strings
- Parse JSON strings back to `DataPoint` objects
- Serialize collections of data points to JSON arrays
- Convert data points to CSV format (single or batch)
- Serialize processing results and metrics to JSON
- Convert timestamps between Unix milliseconds and ISO 8601 strings
- Convert objects to dictionaries for flexible serialization

### Usage Examples

#### Serialize a DataPoint to JSON
```csharp
var dataPoint = new DataPoint(
    id: 12345,
    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    value: 42.75,
    source: "sensor-001"
);

// Add optional properties
dataPoint.Quality = 95;
dataPoint.Tags = "temperature,environment";

dataPoint.Metadata = new Dictionary<string, object>
{
    ["unit"] = "celsius",
    ["location"] = "room-101"
};

// Serialize to JSON
string json = SerializationHelper.ToJson(dataPoint);
Console.WriteLine(json);
```

#### Deserialize JSON to DataPoint
```csharp
string jsonData = @"{
    \"Id\": 12345,
    \"Timestamp\": 1719580800000,
    \"Value\": 42.75,
    \"Source\": \"sensor-001\",
    \"Quality\": 95,
    \"Tags\": \"temperature,environment\",
    \"Metadata\": {
        \"unit\": \"celsius\",
        \"location\": \"room-101\"
    }
}";

DataPoint deserialized = SerializationHelper.FromJson(jsonData);
Console.WriteLine($"Deserialized: {deserialized.Id} - {deserialized.Value}");
```

#### Serialize a collection to JSON array
```csharp
var dataPoints = new List<DataPoint>
{
    new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 25.5, "sensor-001"),
    new DataPoint(2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 30.2, "sensor-002"),
    new DataPoint(3, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 22.8, "sensor-003")
};

string jsonArray = SerializationHelper.ToJsonArray(dataPoints);
Console.WriteLine(jsonArray);
```

#### Convert DataPoint to CSV
```csharp
var dataPoint = new DataPoint(
    id: 67890,
    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    value: 19.3,
    source: "sensor-004"
);

dataPoint.Quality = 88;
dataPoint.Tags = "humidity,environment";

// Convert to CSV (with header)
string csv = SerializationHelper.ToCsv(dataPoint);
Console.WriteLine(csv);

// Convert to CSV without header
string csvNoHeader = SerializationHelper.ToCsv(dataPoint, includeHeader: false);
```

#### Serialize a batch of DataPoints to CSV
```csharp
var batch = new List<DataPoint>
{
    new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 25.5, "sensor-001"),
    new DataPoint(2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 30.2, "sensor-002"),
    new DataPoint(3, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 22.8, "sensor-003")
};

string csvBatch = SerializationHelper.ToCsvBatch(batch);
Console.WriteLine(csvBatch);
```

#### Serialize ProcessingResults
```csharp
var results = new List<ProcessingResult>
{
    new ProcessingResult
    {
        ResultId = "result-001",
        Success = true,
        ProcessingTimeMs = 150,
        ProcessedAt = DateTimeOffset.UtcNow,
        StageName = "validation"
    },
    new ProcessingResult
    {
        ResultId = "result-002",
        Success = false,
        ErrorMessage = "Validation failed",
        ProcessingTimeMs = 85,
        ProcessedAt = DateTimeOffset.UtcNow,
        StageName = "validation"
    }
};

string resultsJson = SerializationHelper.SerializeResults(results);
Console.WriteLine(resultsJson);
```

#### Serialize Metrics
```csharp
var metrics = new MetricAggregation
{
    ComputedAt = DateTimeOffset.UtcNow,
    TotalItemsProcessed = 1000,
    TotalItemsFailed = 25,
    TotalItemsSkipped = 15,
    AverageProcessingTimeMs = 125.5,
    BackpressureEvents = 3
};

string metricsJson = SerializationHelper.SerializeMetrics(metrics);
Console.WriteLine(metricsJson);
```

#### Convert timestamp formats
```csharp
// Unix timestamp to ISO 8601
long unixTime = 1719580800000;
string iso8601 = SerializationHelper.UnixToIso8601(unixTime);
Console.WriteLine($"ISO 8601: {iso8601}");

// ISO 8601 to Unix timestamp
string isoString = "2024-06-28T12:00:00.000Z";
long unixTimestamp = SerializationHelper.Iso8601ToUnix(isoString);
Console.WriteLine($"Unix timestamp: {unixTimestamp}");
```

#### Convert objects to dictionaries
```csharp
var dataPoint = new DataPoint(
    id: 99999,
    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    value: 45.6,
    source: "sensor-005"
);

var dict = SerializationHelper.ToDictionary(dataPoint);
Console.WriteLine($"Dictionary has {dict.Count} keys:");
foreach (var kvp in dict)
{
    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
}
```

## BatchSerializationHelper
The `BatchSerializationHelper` class extends the functionality of `SerializationHelper` by providing asynchronous file I/O operations for batch processing of data points in JSON or CSV format.
