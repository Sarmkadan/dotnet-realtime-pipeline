#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Configuration;

using DotNetRealtimePipeline.Events;
using DotNetRealtimePipeline.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

/// <summary>
/// Extension methods for configuring event-based services.
/// </summary>
public static class EventServiceConfiguration
{
    /// <summary>
    /// Adds event publishing and subscription infrastructure.
    /// </summary>
    public static IServiceCollection AddEventServices(this IServiceCollection services)
    {
        services.AddSingleton<PipelineEventPublisher>();

        // Register default subscribers
        services.AddSingleton<DataIngestSubscriber>();
        services.AddSingleton<ProcessingCompletionSubscriber>();
        services.AddSingleton<BackpressureAlertSubscriber>();
        services.AddSingleton<MetricsAggregationSubscriber>();
        services.AddSingleton<ErrorAlertSubscriber>();

        return services;
    }

    /// <summary>
    /// Registers event subscribers with automatic initialization.
    /// </summary>
    public static IServiceCollection AddEventSubscribers(
        this IServiceCollection services,
        Action<IServiceProvider> initializeSubscribers)
    {
        services.AddEventServices();

        // Store the initialization action to be executed later
        services.AddSingleton<Action<IServiceProvider>>(initializeSubscribers);

        return services;
    }

    /// <summary>
    /// Subscribes an event handler to the pipeline event publisher.
    /// </summary>
    public static void SubscribeToDataIngestedEvents(
        this IServiceProvider serviceProvider,
        Func<DataIngestedEventArgs, System.Threading.Tasks.Task> handler)
    {
        var publisher = serviceProvider.GetRequiredService<PipelineEventPublisher>();
        publisher.Subscribe<DataIngestedEventArgs>(nameof(DataIngestedEvent), handler);
    }

    /// <summary>
    /// Subscribes an event handler to processing completion events.
    /// </summary>
    public static void SubscribeToProcessingCompletedEvents(
        this IServiceProvider serviceProvider,
        Func<ProcessingCompletedEventArgs, System.Threading.Tasks.Task> handler)
    {
        var publisher = serviceProvider.GetRequiredService<PipelineEventPublisher>();
        publisher.Subscribe<ProcessingCompletedEventArgs>(nameof(ProcessingCompletedEvent), handler);
    }

    /// <summary>
    /// Subscribes an event handler to backpressure detection events.
    /// </summary>
    public static void SubscribeToBackpressureEvents(
        this IServiceProvider serviceProvider,
        Func<BackpressureDetectedEventArgs, System.Threading.Tasks.Task> handler)
    {
        var publisher = serviceProvider.GetRequiredService<PipelineEventPublisher>();
        publisher.Subscribe<BackpressureDetectedEventArgs>(nameof(BackpressureDetectedEvent), handler);
    }

    /// <summary>
    /// Subscribes an event handler to metrics collection events.
    /// </summary>
    public static void SubscribeToMetricsEvents(
        this IServiceProvider serviceProvider,
        Func<MetricsCollectedEventArgs, System.Threading.Tasks.Task> handler)
    {
        var publisher = serviceProvider.GetRequiredService<PipelineEventPublisher>();
        publisher.Subscribe<MetricsCollectedEventArgs>(nameof(MetricsCollectedEvent), handler);
    }

    /// <summary>
    /// Subscribes an event handler to pipeline error events.
    /// </summary>
    public static void SubscribeToPipelineErrorEvents(
        this IServiceProvider serviceProvider,
        Func<PipelineErrorEventArgs, System.Threading.Tasks.Task> handler)
    {
        var publisher = serviceProvider.GetRequiredService<PipelineEventPublisher>();
        publisher.Subscribe<PipelineErrorEventArgs>(nameof(PipelineErrorEvent), handler);
    }
}

/// <summary>
/// Extension methods for configuring background workers.
/// </summary>
public static class WorkerServiceConfiguration
{
    /// <summary>
    /// Adds background worker services.
    /// </summary>
    public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services)
    {
        services.AddSingleton<BackgroundProcessingWorker>();
        services.AddSingleton<MetricsAggregationWorker>();
        services.AddSingleton<HealthCheckWorker>();
        services.AddSingleton<WorkerCoordinator>();

        return services;
    }

    /// <summary>
    /// Configures worker intervals and options.
    /// </summary>
    public static IServiceCollection AddBackgroundWorkers(
        this IServiceCollection services,
        Action<WorkerOptions> configureOptions)
    {
        var options = new WorkerOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(options);
        services.AddBackgroundWorkers();

        return services;
    }

    /// <summary>
    /// Gets the worker coordinator for managing all background workers.
    /// </summary>
    public static WorkerCoordinator GetWorkerCoordinator(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<WorkerCoordinator>();
    }
}

/// <summary>
/// Configuration options for background workers.
/// </summary>
public class WorkerOptions
{
    public int MetricsAggregationIntervalMs { get; set; } = 5000;
    public int HealthCheckIntervalMs { get; set; } = 10000;
    public bool EnableProcessingWorker { get; set; } = true;
    public bool EnableMetricsWorker { get; set; } = true;
    public bool EnableHealthCheckWorker { get; set; } = true;
}

/// <summary>
/// Extension methods for configuring caching services.
/// </summary>
public static class CachingServiceConfiguration
{
    /// <summary>
    /// Adds caching services.
    /// </summary>
    public static IServiceCollection AddCachingServices(
        this IServiceCollection services,
        int maxCapacity = 5000)
    {
        services.AddSingleton<Caching.IDistributedCache>(
            new Caching.InMemoryDistributedCache(maxCapacity));

        return services;
    }
}

/// <summary>
/// Extension methods for configuring middleware components.
/// </summary>
public static class MiddlewareConfiguration
{
    /// <summary>
    /// Adds middleware services.
    /// </summary>
    public static IServiceCollection AddMiddlewareServices(this IServiceCollection services)
    {
        services.AddSingleton<Middleware.LoggingMiddleware>();
        services.AddSingleton<Middleware.PerformanceLoggingMiddleware>();
        services.AddSingleton<Middleware.ErrorHandlingMiddleware>();
        services.AddSingleton<Middleware.RetryMiddleware>();
        services.AddSingleton<Middleware.CircuitBreakerMiddleware>();
        services.AddSingleton<Middleware.RateLimitingMiddleware>();
        services.AddSingleton<Middleware.StageRateLimitingMiddleware>();
        services.AddSingleton<Middleware.CorrelationMiddleware>();

        return services;
    }

    /// <summary>
    /// Gets the error handling middleware.
    /// </summary>
    public static Middleware.ErrorHandlingMiddleware GetErrorHandlingMiddleware(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<Middleware.ErrorHandlingMiddleware>();
    }

    /// <summary>
    /// Gets the rate limiting middleware.
    /// </summary>
    public static Middleware.RateLimitingMiddleware GetRateLimitingMiddleware(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<Middleware.RateLimitingMiddleware>();
    }
}

/// <summary>
/// Configuration builder for complete pipeline setup with all components.
/// </summary>
public class CompleteConfigurationBuilder
{
    private readonly IServiceCollection _services;
    private readonly ILogger<CompleteConfigurationBuilder> _logger;

    public CompleteConfigurationBuilder(IServiceCollection services, ILogger<CompleteConfigurationBuilder> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Adds all pipeline services.
    /// </summary>
    public CompleteConfigurationBuilder WithAllServices()
    {
        _services.AddEventServices();
        _services.AddBackgroundWorkers();
        _services.AddCachingServices();
        _services.AddMiddlewareServices();

        _logger.LogInformation("All pipeline services registered");
        return this;
    }

    /// <summary>
    /// Adds HTTP client factory.
    /// </summary>
    public CompleteConfigurationBuilder WithHttpClientFactory()
    {
        _services.AddSingleton<Integration.PipelineHttpClientFactory>();
        _logger.LogInformation("HTTP client factory registered");
        return this;
    }

    /// <summary>
    /// Configures event subscriber initialization.
    /// </summary>
    public CompleteConfigurationBuilder WithEventSubscribers()
    {
        _services.AddEventServices();
        _logger.LogInformation("Event subscribers registered");
        return this;
    }

    public IServiceCollection Build()
    {
        return _services;
    }
}
