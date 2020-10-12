#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Configuration;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetRealtimePipeline.Tests.Integration;

/// <summary>
/// Extension methods for <see cref="PipelineIntegrationTests"/> to reduce boilerplate and improve test clarity.
/// </summary>
public static class PipelineIntegrationTestsExtensions
{
    /// <summary>
    /// Creates a service provider, resolves the pipeline orchestrator, and starts it asynchronously.
    /// </summary>
    /// <param name="tests">The test instance providing access to <see cref="PipelineIntegrationTests.SetupServices"/>.</param>
    /// <returns>A tuple containing the service provider and started orchestrator.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static (ServiceProvider ServiceProvider, PipelineOrchestrator Orchestrator) WithStartedOrchestrator(this PipelineIntegrationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var serviceProvider = tests.SetupServices();
        var orchestrator = serviceProvider.GetRequiredService<PipelineOrchestrator>();
        _ = orchestrator.StartAsync().AsTask().GetAwaiter().GetResult(); // Synchronously start to avoid async void

        return (serviceProvider, orchestrator);
    }

    /// <summary>
    /// Generates a list of synthetic <see cref="DataPoint"/> instances with sequential IDs and customizable properties.
    /// </summary>
    /// <param name="count">Number of data points to generate.</param>
    /// <param name="sensorName">Sensor identifier for all generated points.</param>
    /// <param name="valueGenerator">Function to generate values based on index.</param>
    /// <returns>A list of <see cref="DataPoint"/> instances.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is non-positive.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sensorName"/> is null or empty.</exception>
    public static List<DataPoint> GenerateDataPoints(this PipelineIntegrationTests tests, int count, string sensorName, Func<int, decimal> valueGenerator)
    {
        ArgumentException.ThrowIfNullOrEmpty(sensorName);
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");

        var now = DateTime.UtcNow.Ticks;
        return Enumerable.Range(0, count)
            .Select(i => new DataPoint(i, now + i, valueGenerator(i), sensorName))
            .ToList();
    }

    /// <summary>
    /// Waits for a specified duration to allow asynchronous pipeline processing to complete.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="milliseconds">Number of milliseconds to wait.</param>
    /// <returns>A completed task after the delay.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="milliseconds"/> is negative.</exception>
    public static async Task WaitForProcessingAsync(this PipelineIntegrationTests tests, int milliseconds)
    {
        if (milliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(milliseconds), "Delay must be non-negative.");

        await Task.Delay(milliseconds);
    }
}
