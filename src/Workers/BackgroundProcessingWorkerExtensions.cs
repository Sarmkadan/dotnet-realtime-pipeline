namespace DotNetRealtimePipeline.Workers;

using Microsoft.Extensions.Logging;

/// <summary>
/// Extension methods for <see cref="BackgroundProcessingWorker"/> and related worker types.
/// Provides fluent-style operations for starting, stopping, and coordinating background workers.
/// </summary>
public static class BackgroundProcessingWorkerExtensions
{
    /// <summary>
    /// Stops the background processing worker if it's running.
    /// </summary>
    /// <param name="worker">The background processing worker to stop.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="worker"/> is <see langword="null"/>.</exception>
    public static async Task TryStopAsync(this BackgroundProcessingWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        if (worker.IsRunning)
        {
            await worker.StopAsync();
        }
    }

    /// <summary>
    /// Starts the background processing worker if it's not already running.
    /// </summary>
    /// <param name="worker">The background processing worker to start.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="worker"/> is <see langword="null"/>.</exception>
    public static void TryStart(this BackgroundProcessingWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        if (!worker.IsRunning)
        {
            worker.Start();
        }
    }

    /// <summary>
    /// Creates a new worker coordinator with the specified workers.
    /// </summary>
    /// <param name="processingWorker">The background processing worker.</param>
    /// <param name="metricsWorker">The metrics aggregation worker.</param>
    /// <param name="healthCheckWorker">The health check worker.</param>
    /// <param name="logger">The logger for the worker coordinator.</param>
    /// <returns>A new <see cref="WorkerCoordinator"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="processingWorker"/>, <paramref name="metricsWorker"/>,
    /// <paramref name="healthCheckWorker"/>, or <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    public static WorkerCoordinator ToCoordinator(
        this BackgroundProcessingWorker processingWorker,
        MetricsAggregationWorker metricsWorker,
        HealthCheckWorker healthCheckWorker,
        ILogger<WorkerCoordinator> logger)
    {
        ArgumentNullException.ThrowIfNull(processingWorker);
        ArgumentNullException.ThrowIfNull(metricsWorker);
        ArgumentNullException.ThrowIfNull(healthCheckWorker);
        ArgumentNullException.ThrowIfNull(logger);

        return new WorkerCoordinator(processingWorker, metricsWorker, healthCheckWorker, logger);
    }
}
