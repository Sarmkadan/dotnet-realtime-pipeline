#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Configuration;

using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

/// <summary>
/// Provides extension methods for configuring pipeline services via dependency injection.
/// </summary>
/// <remarks>
/// This class is static and sealed to prevent inheritance.
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all pipeline services and repositories with the specified configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="pipelineConfig">The pipeline configuration to use.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="pipelineConfig"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddPipelineServices(
        this IServiceCollection services,
        PipelineConfig pipelineConfig)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(pipelineConfig);

        // Register repositories
        services.AddSingleton<IDataPointRepository, InMemoryDataPointRepository>();
        services.AddSingleton<IMetricsRepository, InMemoryMetricsRepository>();

        // Register configuration
        services.AddSingleton(pipelineConfig);

        // Register services
        services.AddSingleton<DataProcessingService>();
        services.AddSingleton<WindowingService>();
        services.AddSingleton<MetricsService>();
        services.AddSingleton<BackpressureService>();
        services.AddSingleton<QueryService>();
        services.AddSingleton<PipelineOrchestrator>();

        // Register visualization
        services.AddSingleton<DotNetRealtimePipeline.Visualization.PipelineVisualizer>();

        // Register backpressure metrics collector
        services.AddSingleton<DotNetRealtimePipeline.Metrics.BackpressureMetricsCollector>();

        // Register dead-letter queue
        services.AddSingleton<DotNetRealtimePipeline.DeadLetter.IDeadLetterQueue, DotNetRealtimePipeline.DeadLetter.DeadLetterQueue>();

// Register HTTP client factory
services.AddSingleton<DotNetRealtimePipeline.Integration.PipelineHttpClientFactory>();

        return services;
    }

    /// <summary>
    /// Registers pipeline services with a default configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddPipelineServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var defaultConfig = CreateDefaultConfiguration();
        return services.AddPipelineServices(defaultConfig);
    }

    /// <summary>
    /// Registers pipeline services with configuration builder.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">The configuration action to apply to the default configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configureOptions"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddPipelineServices(
        this IServiceCollection services,
        Action<PipelineConfig> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var config = CreateDefaultConfiguration();
        configureOptions(config);

        return services.AddPipelineServices(config);
    }

    /// <summary>
    /// Creates a default pipeline configuration.
    /// </summary>
    /// <returns>A new <see cref="PipelineConfig"/> instance with default settings.</returns>
    private static PipelineConfig CreateDefaultConfiguration()
    {
        var config = new PipelineConfig(
            configId: 1,
            pipelineName: "DefaultPipeline",
            version: "1.0.0"
        );

        // Add default stages
        config.AddStage(new PipelineStageDef("Ingestion", "SOURCE"));
        config.AddStage(new PipelineStageDef("Validation", "FILTER"));
        config.AddStage(new PipelineStageDef("Transformation", "TRANSFORM"));
        config.AddStage(new PipelineStageDef("Windowing", "WINDOW"));
        config.AddStage(new PipelineStageDef("Aggregation", "AGGREGATE"));
        config.AddStage(new PipelineStageDef("Output", "SINK"));

        return config;
    }
}