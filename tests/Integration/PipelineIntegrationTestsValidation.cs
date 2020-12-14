#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetRealtimePipeline.Tests.Integration;

/// <summary>
/// Provides validation helpers for <see cref="PipelineIntegrationTests"/> instances.
/// </summary>
public static class PipelineIntegrationTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="PipelineIntegrationTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PipelineIntegrationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate methods are non-null and non-empty
        if (value.StartStop_ShouldInitializeAndCleanup is null)
        {
            problems.Add($"Method '{nameof(PipelineIntegrationTests.StartStop_ShouldInitializeAndCleanup)}' is null.");
        }

        if (value.IngestDataPoint_ShouldAcceptAndProcess is null)
        {
            problems.Add($"Method '{nameof(PipelineIntegrationTests.IngestDataPoint_ShouldAcceptAndProcess)}' is null.");
        }

        if (value.IngestBatch_ShouldProcessMultiplePoints is null)
        {
            problems.Add($"Method '{nameof(PipelineIntegrationTests.IngestBatch_ShouldProcessMultiplePoints)}' is null.");
        }

        if (value.GetHealthReport_ShouldReturnMetrics is null)
        {
            problems.Add($"Method '{nameof(PipelineIntegrationTests.GetHealthReport_ShouldReturnMetrics)}' is null.");
        }

        if (value.QueryDataPoints_ShouldReturnFilteredResults is null)
        {
            problems.Add($"Method '{nameof(PipelineIntegrationTests.QueryDataPoints_ShouldReturnFilteredResults)}' is null.");
        }

        if (value.MultipleSourceIngestion_ShouldHandleConcurrentData is null)
        {
            problems.Add($"Method '{nameof(PipelineIntegrationTests.MultipleSourceIngestion_ShouldHandleConcurrentData)}' is null.");
        }

        if (value.GetMetricsHistory_ShouldReturnCollectedMetrics is null)
        {
            problems.Add($"Method '{nameof(PipelineIntegrationTests.GetMetricsHistory_ShouldReturnCollectedMetrics)}' is null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PipelineIntegrationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this PipelineIntegrationTests value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="PipelineIntegrationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing the validation problems.</exception>
    public static void EnsureValid(this PipelineIntegrationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PipelineIntegrationTests instance is invalid. Problems: {string.Join(", ", problems)}",
                nameof(value));
        }
    }
}