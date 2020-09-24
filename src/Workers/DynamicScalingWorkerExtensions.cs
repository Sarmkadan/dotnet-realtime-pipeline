namespace DotNetRealtimePipeline.Workers;

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="DynamicScalingWorker"/>.
/// </summary>
public static class DynamicScalingWorkerExtensions
{
    /// <summary>
    /// Starts the dynamic scaling worker and waits for it to begin running.
    /// </summary>
    /// <param name="worker">The dynamic scaling worker to start.</param>
    /// <exception cref="InvalidOperationException">Thrown if the worker is already running.</exception>
    public static void StartAndWait(this DynamicScalingWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        if (worker.IsRunning)
        {
            throw new InvalidOperationException("Worker is already running");
        }

        worker.Start();

        // Verify that the worker has started
        while (!worker.IsRunning)
        {
            // Busy-waiting is generally discouraged, but in this case,
            // we want to ensure the worker has started before returning.
        }
    }

    /// <summary>
    /// Stops the dynamic scaling worker and waits for it to shut down.
    /// </summary>
    /// <param name="worker">The dynamic scaling worker to stop.</param>
    /// <exception cref="InvalidOperationException">Thrown if the worker is not running.</exception>
    public static async Task StopAndWaitAsync(this DynamicScalingWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        if (!worker.IsRunning)
        {
            throw new InvalidOperationException("Worker is not running");
        }

        await worker.StopAsync();
    }

    /// <summary>
    /// Restarts the dynamic scaling worker.
    /// </summary>
    /// <param name="worker">The dynamic scaling worker to restart.</param>
    /// <exception cref="InvalidOperationException">Thrown if the worker is not running.</exception>
    public static async Task RestartAsync(this DynamicScalingWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        if (!worker.IsRunning)
        {
            throw new InvalidOperationException("Worker is not running");
        }

        await worker.StopAsync();
        worker.Start();
    }
}
