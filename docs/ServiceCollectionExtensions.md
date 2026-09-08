# ServiceCollectionExtensions

This document describes the `ServiceCollectionExtensions` class located in `src/Configuration/ServiceCollectionExtensions.cs`. The class provides extension methods for registering pipeline services with the dependency injection container.

## Overview

The `ServiceCollectionExtensions` class contains three overloads of the `AddPipelineServices` method, each providing a different way to configure and register the pipeline services. All methods register the same set of services with singleton lifetimes, differing only in how the `PipelineConfig` is provided.

## Service Registrations

All overloads of `AddPipelineServices` register the following services with **singleton** lifetime:

| Service Interface / Type | Implementation Type | Lifetime |
|--------------------------|---------------------|----------|
| `IDataPointRepository` | `InMemoryDataPointRepository` | Singleton |
| `IMetricsRepository` | `InMemoryMetricsRepository` | Singleton |
| `PipelineConfig` (provided instance) | `PipelineConfig` | Singleton |
| `DataProcessingService` | `DataProcessingService` | Singleton |
| `WindowingService` | `WindowingService` | Singleton |
| `MetricsService` | `MetricsService` | Singleton |
| `BackpressureService` | `BackpressureService` | Singleton |
| `QueryService` | `QueryService` | Singleton |
| `PipelineOrchestrator` | `PipelineOrchestrator` | Singleton |
| `DotNetRealtimePipeline.Visualization.PipelineVisualizer` | `DotNetRealtimePipeline.Visualization.PipelineVisualizer` | Singleton |
| `DotNetRealtimePipeline.Metrics.BackpressureMetricsCollector` | `DotNetRealtimePipeline.Metrics.BackpressureMetricsCollector` | Singleton |
| `DotNetRealtimePipeline.DeadLetter.IDeadLetterQueue` | `DotNetRealtimePipeline.DeadLetter.DeadLetterQueue` | Singleton |
| `DotNetRealtimePipeline.DeadLetter.RetryPolicyOptions` | `DotNetRealtimePipeline.DeadLetter.RetryPolicyOptions` | Singleton |
| `DotNetRealtimePipeline.DeadLetter.IRetryPolicy` | `DotNetRealtimePipeline.DeadLetter.ExponentialBackoffRetryPolicy` (factory) | Singleton |
| `DotNetRealtimePipeline.Integration.PipelineHttpClientFactory` | `DotNetRealtimePipeline.Integration.PipelineHttpClientFactory` | Singleton |

## Method Overloads

### `AddPipelineServices(IServiceCollection services, PipelineConfig pipelineConfig, Action<RetryPolicyOptions>? configureRetryPolicy = null)`

Registers all pipeline services using the provided `PipelineConfig` instance.

**Parameters:**
- `services`: The service collection to add services to.
- `pipelineConfig`: The pipeline configuration instance to register.
- `configureRetryPolicy`: Optional action to customize the retry policy options before dead-lettering.

**Returns:** The same `IServiceCollection` instance for chaining.

**Exceptions:** Throws `ArgumentNullException` if `services` or `pipelineConfig` is null.

### `AddPipelineServices(IServiceCollection services, Action<RetryPolicyOptions>? configureRetryPolicy = null)`

Registers all pipeline services with a default pipeline configuration.

**Parameters:**
- `services`: The service collection to add services to.
- `configureRetryPolicy`: Optional action to customize the retry policy options before dead-lettering.

**Returns:** The same `IServiceCollection` instance for chaining.

**Exceptions:** Throws `ArgumentNullException` if `services` is null.

**Note:** This overload creates a default `PipelineConfig` instance with:
- `configId`: 1
- `pipelineName`: "DefaultPipeline"
- `version`: "1.0.0"
- Six stages: Ingestion (SOURCE), Validation (FILTER), Transformation (TRANSFORM), Windowing (WINDOW), Aggregation (AGGREGATE), Output (SINK)

### `AddPipelineServices(IServiceCollection services, Action<PipelineConfig> configureOptions, Action<RetryPolicyOptions>? configureRetryPolicy = null)`

Registers all pipeline services with a default pipeline configuration that can be customized via `configureOptions`.

**Parameters:**
- `services`: The service collection to add services to.
- `configureOptions`: Action to apply to the default pipeline configuration.
- `configureRetryPolicy`: Optional action to customize the retry policy options before dead-lettering.

**Returns:** The same `IServiceCollection` instance for chaining.

**Exceptions:** Throws `ArgumentNullException` if `services` or `configureOptions` is null.

**Note:** This overload creates a default `PipelineConfig` instance (same as above) and applies the `configureOptions` action to it before registration.

## Example Usage

The following example demonstrates how to use the extension methods in a `Program.cs` file with `Microsoft.Extensions.DependencyInjection`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using DotNetRealtimePipeline.Configuration;
using DotNetRealtimePipeline.DeadLetter;

// Example 1: Using the overload with explicit PipelineConfig
var services = new ServiceCollection();
var pipelineConfig = new PipelineConfig(
    configId: 1,
    pipelineName: "MyCustomPipeline",
    version: "2.0.0");

// Configure stages as needed
pipelineConfig.AddStage(new PipelineStageDef("CustomIngestion", "SOURCE"));
pipelineConfig.AddStage(new PipelineStageDef("CustomProcessing", "TRANSFORM"));

services.AddPipelineServices(
    pipelineConfig,
    retryOptions => 
    {
        retryOptions.MaxAttempts = 3;
        retryOptions.BackoffExponent = 2;
        retryOptions.MaxDelaySeconds = 30;
    });

// Example 2: Using the overload with default configuration
var services2 = new ServiceCollection();
services2.AddPipelineServices(
    retryOptions => 
    {
        retryOptions.MaxAttempts = 5;
    });

// Example 3: Using the overload with configuration customization
var services3 = new ServiceCollection();
services3.AddPipelineServices(
    config => 
    {
        config.PipelineName = "ConfiguredPipeline";
        config.Version = "3.0.0";
        // Add or modify stages
        config.AddStage(new PipelineStageDef("Enrichment", "ENRICH"));
    },
    retryOptions => 
    {
        retryOptions.JitterMaxSeconds = 5;
    });

// Build the service provider
var serviceProvider = services.BuildServiceProvider();
// Resolve services as needed
var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();
```