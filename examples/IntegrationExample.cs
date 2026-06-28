// =============================================================================
// ASP.NET Core Integration Example
// Demonstrates how to register the pipeline in an ASP.NET Core application
// using built-in dependency injection.
// =============================================================================

using DotNetRealtimePipeline.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Register pipeline services in the DI container
builder.Services.AddPipelineServices(config =>
{
    config.PipelineName = "WebAppPipeline";
    config.MaxBufferSize = 20000;
});

// The PipelineOrchestrator can now be injected into controllers or background services
// builder.Services.AddHostedService<MyPipelineBackgroundWorker>();

using IHost host = builder.Build();
// ... run application
