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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="worker"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the worker is already running.</exception>
    public static async Task StartAndWaitAsync(this DynamicScalingWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        if (worker.IsRunning)
        {
            throw new InvalidOperationException("Worker is already running");
        }

        worker.Start();

        // Verify that the worker has started with a reasonable timeout
        var timeout = TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        while (!worker.IsRunning)
        {
            if (DateTime.UtcNow - startTime > timeout)
            {
                throw new InvalidOperationException("Worker failed to start within the expected timeout period");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }

    /// <summary>
    /// Stops the dynamic scaling worker and waits for it to shut down.
    /// </summary>
    /// <param name="worker">The dynamic scaling worker to stop.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="worker"/> is <see langword="null"/>.</exception>
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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="worker"/> is <see langword="null"/>.</exception>
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