# EventServiceConfiguration
The `EventServiceConfiguration` type provides a set of methods and properties for configuring event services in the `dotnet-realtime-pipeline` project. It allows for the addition of various services, such as event subscribers, background workers, caching services, and middleware services, as well as configuration of metrics aggregation and health check intervals. This type is essential for setting up and customizing the event processing pipeline.

## API
* `public static IServiceCollection AddEventServices`: Adds event services to the specified `IServiceCollection`. Parameters: `IServiceCollection` services. Return value: `IServiceCollection`. Throws: None.
* `public static IServiceCollection AddEventSubscribers`: Adds event subscribers to the specified `IServiceCollection`. Parameters: `IServiceCollection` services. Return value: `IServiceCollection`. Throws: None.
* `public static void SubscribeToDataIngestedEvents`: Subscribes to data ingested events. Parameters: None. Return value: None. Throws: None.
* `public static void SubscribeToProcessingCompletedEvents`: Subscribes to processing completed events. Parameters: None. Return value: None. Throws: None.
* `public static void SubscribeToBackpressureEvents`: Subscribes to backpressure events. Parameters: None. Return value: None. Throws: None.
* `public static void SubscribeToMetricsEvents`: Subscribes to metrics events. Parameters: None. Return value: None. Throws: None.
* `public static void SubscribeToPipelineErrorEvents`: Subscribes to pipeline error events. Parameters: None. Return value: None. Throws: None.
* `public static IServiceCollection AddBackgroundWorkers`: Adds background workers to the specified `IServiceCollection`. Parameters: `IServiceCollection` services. Return value: `IServiceCollection`. Throws: None.
* `public static WorkerCoordinator GetWorkerCoordinator`: Gets the worker coordinator. Parameters: None. Return value: `WorkerCoordinator`. Throws: None.
* `public int MetricsAggregationIntervalMs`: Gets or sets the metrics aggregation interval in milliseconds. Parameters: None. Return value: `int`. Throws: None.
* `public int HealthCheckIntervalMs`: Gets or sets the health check interval in milliseconds. Parameters: None. Return value: `int`. Throws: None.
* `public bool EnableProcessingWorker`: Gets or sets a value indicating whether the processing worker is enabled. Parameters: None. Return value: `bool`. Throws: None.
* `public bool EnableMetricsWorker`: Gets or sets a value indicating whether the metrics worker is enabled. Parameters: None. Return value: `bool`. Throws: None.
* `public bool EnableHealthCheckWorker`: Gets or sets a value indicating whether the health check worker is enabled. Parameters: None. Return value: `bool`. Throws: None.
* `public static IServiceCollection AddCachingServices`: Adds caching services to the specified `IServiceCollection`. Parameters: `IServiceCollection` services. Return value: `IServiceCollection`. Throws: None.
* `public static IServiceCollection AddMiddlewareServices`: Adds middleware services to the specified `IServiceCollection`. Parameters: `IServiceCollection` services. Return value: `IServiceCollection`. Throws: None.
* `public static Middleware.ErrorHandlingMiddleware GetErrorHandlingMiddleware`: Gets the error handling middleware. Parameters: None. Return value: `Middleware.ErrorHandlingMiddleware`. Throws: None.
* `public static Middleware.RateLimitingMiddleware GetRateLimitingMiddleware`: Gets the rate limiting middleware. Parameters: None. Return value: `Middleware.RateLimitingMiddleware`. Throws: None.
* `public CompleteConfigurationBuilder`: Gets the complete configuration builder. Parameters: None. Return value: `CompleteConfigurationBuilder`. Throws: None.

## Usage
```csharp
// Example 1: Adding event services and subscribers
var services = new ServiceCollection();
services.AddEventServices();
services.AddEventSubscribers();

// Example 2: Configuring metrics aggregation and health check intervals
var config = new EventServiceConfiguration();
config.MetricsAggregationIntervalMs = 1000;
config.HealthCheckIntervalMs = 5000;
config.EnableProcessingWorker = true;
config.EnableMetricsWorker = true;
config.EnableHealthCheckWorker = true;
```

## Notes
The `EventServiceConfiguration` type is not thread-safe, and its methods and properties should not be accessed concurrently from multiple threads. The `AddEventServices`, `AddEventSubscribers`, `AddBackgroundWorkers`, `AddCachingServices`, and `AddMiddlewareServices` methods will throw an exception if the specified `IServiceCollection` is null. The `GetWorkerCoordinator`, `GetErrorHandlingMiddleware`, and `GetRateLimitingMiddleware` methods will throw an exception if the corresponding service is not registered. The `MetricsAggregationIntervalMs` and `HealthCheckIntervalMs` properties will throw an exception if the specified value is less than or equal to zero. The `EnableProcessingWorker`, `EnableMetricsWorker`, and `EnableHealthCheckWorker` properties will throw an exception if the specified value is not a boolean.
