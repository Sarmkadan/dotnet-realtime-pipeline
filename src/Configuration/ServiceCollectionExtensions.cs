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
/// Extension methods for configuring pipeline services via dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all pipeline services and repositories.
    /// </summary>
    public static IServiceCollection AddPipelineServices(
        this IServiceCollection services,
        PipelineConfig pipelineConfig)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (pipelineConfig == null) throw new ArgumentNullException(nameof(pipelineConfig));

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
        services.AddSingleton<PipelineOrchestrator>();

        return services;
    }

    /// <summary>
    /// Registers pipeline services with a default configuration.
    /// </summary>
    public static IServiceCollection AddPipelineServices(
        this IServiceCollection services)
    {
        var defaultConfig = CreateDefaultConfiguration();
        return services.AddPipelineServices(defaultConfig);
    }

    /// <summary>
    /// Registers pipeline services with configuration builder.
    /// </summary>
    public static IServiceCollection AddPipelineServices(
        this IServiceCollection services,
        Action<PipelineConfig> configureOptions)
    {
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        var config = CreateDefaultConfiguration();
        configureOptions(config);

        return services.AddPipelineServices(config);
    }

    /// <summary>
    /// Creates a default pipeline configuration.
    /// </summary>
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
