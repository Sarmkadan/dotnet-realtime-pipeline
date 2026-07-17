using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetRealtimePipeline.Benchmarks;

/// <summary>
/// Provides validation helpers for <see cref="PipelineBenchmarks"/> instances.
/// Validates that all benchmark methods can be invoked without throwing exceptions.
/// </summary>
public static class PipelineBenchmarksValidation
{
    /// <summary>
    /// Validates that all benchmark methods on the specified <see cref="PipelineBenchmarks"/> instance
    /// can be invoked without throwing exceptions.
    /// </summary>
    /// <param name="value">The PipelineBenchmarks instance to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PipelineBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Setup method
        try
        {
            value.Setup();
        }
        catch (Exception ex)
        {
            errors.Add($"Setup() failed: {ex.Message}");
        }

        // Validate Cleanup method
        try
        {
            value.Cleanup();
        }
        catch (Exception ex)
        {
            errors.Add($"Cleanup() failed: {ex.Message}");
        }

        // Validate IngestSingleDataPoint method
        try
        {
            _ = value.IngestSingleDataPoint().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            errors.Add($"IngestSingleDataPoint() failed: {ex.Message}");
        }

        // Validate ProcessBatch method
        try
        {
            _ = value.ProcessBatch(100).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            errors.Add($"ProcessBatch(100) failed: {ex.Message}");
        }

        // Validate ProcessDataPointsThroughWindowing method
        try
        {
            value.ProcessDataPointsThroughWindowing(100);
        }
        catch (Exception ex)
        {
            errors.Add($"ProcessDataPointsThroughWindowing(100) failed: {ex.Message}");
        }

        // Validate GenerateHealthReport method
        try
        {
            _ = value.GenerateHealthReport().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            errors.Add($"GenerateHealthReport() failed: {ex.Message}");
        }

        // Validate BackpressureBufferOperations method
        try
        {
            value.BackpressureBufferOperations();
        }
        catch (Exception ex)
        {
            errors.Add($"BackpressureBufferOperations() failed: {ex.Message}");
        }

        // Validate EndToEndThroughput method
        try
        {
            _ = value.EndToEndThroughput().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            errors.Add($"EndToEndThroughput() failed: {ex.Message}");
        }

        // Validate MemoryAllocationBenchmark method
        try
        {
            _ = value.MemoryAllocationBenchmark().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            errors.Add($"MemoryAllocationBenchmark() failed: {ex.Message}");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PipelineBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The PipelineBenchmarks instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PipelineBenchmarks value) =>
        ArgumentNullException.ThrowIfNull(value), !value.Validate().Any();

    /// <summary>
    /// Ensures that the specified <see cref="PipelineBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The PipelineBenchmarks instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing all validation errors.</exception>
    public static void EnsureValid(this PipelineBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"PipelineBenchmarks instance is not valid. Validation errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}
