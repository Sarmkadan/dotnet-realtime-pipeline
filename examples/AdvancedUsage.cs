// =============================================================================
// Advanced Usage Example
// This example shows custom pipeline configuration, error handling,
// and advanced features like custom stage definitions.
// =============================================================================

using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

// 1. Setup dependency injection with custom configuration
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

services.AddPipelineServices(config =>
{
    config.PipelineName = "AdvancedPipeline";
    config.MaxBufferSize = 100000;
    config.WindowSizeMs = 10000;
    config.MaxConcurrentConsumers = 16;
    
    // Add custom stage
    config.AddStage(new PipelineStageDef("CustomFilter", "FILTER"));
});

var serviceProvider = services.BuildServiceProvider();
var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>(); // Assuming Program context

// 2. Start the pipeline with error handling
try
{
    await orchestrator.StartAsync();
    
    // ... Ingestion logic ...
}
catch (Exception ex)
{
    // Handle initialization errors
    Console.WriteLine($"Error starting pipeline: {ex.Message}");
}
finally
{
    // 3. Graceful shutdown
    await orchestrator.StopAsync();
}
