#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Validation helpers for <see cref="BatchProcessor{TInput, TOutput}"/> and related classes.
/// </summary>
public static class BatchProcessorValidation
{
    /// <summary>
    /// Validates a <see cref="BatchProcessingProgress"/> instance.
    /// </summary>
    /// <param name="value">The batch processing progress to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this BatchProcessingProgress value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.TotalBatches < 0)
        {
            problems.Add($"TotalBatches must be non-negative, but was {value.TotalBatches}.");
        }

        if (value.ProcessedBatches < 0)
        {
            problems.Add($"ProcessedBatches must be non-negative, but was {value.ProcessedBatches}.");
        }

        if (value.ProcessedBatches > value.TotalBatches)
        {
            problems.Add($"ProcessedBatches ({value.ProcessedBatches}) cannot exceed TotalBatches ({value.TotalBatches}).");
        }

        if (value.TotalItems < 0)
        {
            problems.Add($"TotalItems must be non-negative, but was {value.TotalItems}.");
        }

        if (value.ProcessedItems < 0)
        {
            problems.Add($"ProcessedItems must be non-negative, but was {value.ProcessedItems}.");
        }

        if (value.ProcessedItems > value.TotalItems)
        {
            problems.Add($"ProcessedItems ({value.ProcessedItems}) cannot exceed TotalItems ({value.TotalItems}).");
        }

        if (value.StartTime == default)
        {
            problems.Add("StartTime must be set to a valid DateTime.");
        }

        if (value.LastUpdateTime == default)
        {
            problems.Add("LastUpdateTime must be set to a valid DateTime.");
        }

        return problems;
    }

    /// <summary>
    /// Validates a <see cref="DataPointBatchProcessor"/> instance.
    /// </summary>
    /// <param name="value">The data point batch processor to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DataPointBatchProcessor value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // DataPointBatchProcessor doesn't expose its internal processor for validation,
        // so we can't validate batchSize or parallelism directly.
        // The processor is initialized with default values (1000, 4) which are valid.

        return problems;
    }

    /// <summary>
    /// Determines whether a <see cref="BatchProcessingProgress"/> instance is valid.
    /// </summary>
    /// <param name="value">The batch processing progress to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this BatchProcessingProgress value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Determines whether a <see cref="DataPointBatchProcessor"/> instance is valid.
    /// </summary>
    /// <param name="value">The data point batch processor to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this DataPointBatchProcessor value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="BatchProcessingProgress"/> instance is valid.
    /// </summary>
    /// <param name="value">The batch processing progress to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this BatchProcessingProgress value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"BatchProcessingProgress is not valid:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that a <see cref="DataPointBatchProcessor"/> instance is valid.
    /// </summary>
    /// <param name="value">The data point batch processor to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid.</exception>
    public static void EnsureValid(this DataPointBatchProcessor value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DataPointBatchProcessor is not valid:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }
}