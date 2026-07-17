#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Initialization;

using System;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="PipelineInitializer"/> that provide additional pipeline lifecycle management functionality.
/// </summary>
public static class PipelineInitializerExtensions
{
    /// <summary>
    /// Initializes the pipeline and automatically starts it if initialization succeeds.
    /// </summary>
    /// <param name="initializer">The pipeline initializer instance.</param>
    /// <returns>An initialization result containing success status and timing information.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="initializer"/> is <see langword="null"/>.</exception>
    public static async Task<InitializationResult> InitializeAndStartAsync(this PipelineInitializer initializer)
    {
        ArgumentNullException.ThrowIfNull(initializer);

        var result = await initializer.InitializeAsync().ConfigureAwait(false);

        if (result.Success)
        {
            var startSuccess = await initializer.StartAsync().ConfigureAwait(false);
            if (!startSuccess)
            {
                result.Success = false;
                result.ErrorMessage = "Initialization succeeded but automatic start failed";
            }
        }

        return result;
    }

    /// <summary>
    /// Attempts to initialize the pipeline with automatic retry logic for transient failures.
    /// </summary>
    /// <param name="initializer">The pipeline initializer instance.</param>
    /// <param name="maxAttempts">Maximum number of initialization attempts (must be positive).</param>
    /// <param name="delayBetweenAttempts">Delay between retry attempts in milliseconds.</param>
    /// <returns>An initialization result containing success status and timing information.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="initializer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxAttempts"/> is less than 1 or <paramref name="delayBetweenAttempts"/> is negative.</exception>
    public static async Task<InitializationResult> InitializeWithRetryAsync(
        this PipelineInitializer initializer,
        int maxAttempts = 3,
        int delayBetweenAttempts = 1000)
    {
        ArgumentNullException.ThrowIfNull(initializer);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(delayBetweenAttempts);

        InitializationResult? lastResult = null;
        var attempt = 0;

        while (attempt < maxAttempts)
        {
            attempt++;
            lastResult = await initializer.InitializeAsync().ConfigureAwait(false);

            if (lastResult.Success)
            {
                return lastResult;
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(delayBetweenAttempts).ConfigureAwait(false);
            }
        }

        return lastResult ?? new InitializationResult { Success = false, ErrorMessage = "All initialization attempts failed" };
    }

    /// <summary>
    /// Safely stops the pipeline if it's running, swallowing any exceptions.
    /// <para>Note: This method intentionally swallows exceptions to ensure cleanup always succeeds.</para>
    /// </summary>
    /// <param name="initializer">The pipeline initializer instance.</param>
    /// <returns>True if the pipeline was stopped successfully or wasn't running; false if an error occurred.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="initializer"/> is <see langword="null"/>.</exception>
    public static async Task<bool> SafeStopAsync(this PipelineInitializer initializer)
    {
        ArgumentNullException.ThrowIfNull(initializer);

        try
        {
            return await initializer.StopAsync().ConfigureAwait(false);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the pipeline has been initialized.
    /// </summary>
    /// <param name="initializer">The pipeline initializer instance.</param>
    /// <returns>True if the pipeline has been initialized; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="initializer"/> is <see langword="null"/>.</exception>
    public static bool IsInitialized(this PipelineInitializer initializer)
    {
        ArgumentNullException.ThrowIfNull(initializer);

        return initializer.IsInitialized;
    }

    /// <summary>
    /// Gets the current state of the pipeline as a string.
    /// </summary>
    /// <param name="initializer">The pipeline initializer instance.</param>
    /// <returns>A string representation of the pipeline state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="initializer"/> is <see langword="null"/>.</exception>
    public static string GetPipelineState(this PipelineInitializer initializer)
    {
        ArgumentNullException.ThrowIfNull(initializer);

        return initializer.IsInitialized ? "Initialized" : "Not Initialized";
    }
}
