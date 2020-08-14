// =============================================================================
// Basic Usage Example
// This example demonstrates the minimal setup required to start the pipeline
// and ingest a single data point.
// =============================================================================

using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Configuration; // Ensure this namespace is available
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

// 1. Setup dependency injection
var services = new ServiceCollection();
services.AddPipelineServices(); // Uses default configuration
var serviceProvider = services.BuildServiceProvider();

// 2. Get the orchestrator
var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();

// 3. Start the pipeline
await orchestrator.StartAsync();
Console.WriteLine("Pipeline started.");

// 4. Ingest a data point
var dataPoint = new DataPoint(
    id: 1,
    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    value: 42.5,
    source: "Sensor-1"
);

bool accepted = await orchestrator.IngestDataPointAsync(dataPoint);
Console.WriteLine($"Data point accepted: {accepted}");

// 5. Get current status
var status = orchestrator.GetStatus();
Console.WriteLine($"Processed: {status.TotalDataPointsProcessed}");

// 6. Stop the pipeline
await orchestrator.StopAsync();
Console.WriteLine("Pipeline stopped.");
