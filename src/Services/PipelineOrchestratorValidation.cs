#nullable enable

namespace DotNetRealtimePipeline.Services;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="PipelineOrchestrator"/> instances.
/// </summary>
public static class PipelineOrchestratorValidation
{
    /// <summary>
    /// Validates a <see cref="PipelineOrchestrator"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The orchestrator to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PipelineOrchestrator value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Get status for validation
        var status = value.GetStatus();

        // Validate ConfigurationName from status
        if (string.IsNullOrWhiteSpace(status.ConfigurationName))
        {
            problems.Add("ConfigurationName is null or whitespace.");
        }
        else if (status.ConfigurationName.Length > 200)
        {
            problems.Add("ConfigurationName exceeds maximum length of 200 characters.");
        }

        // Validate ConfigurationVersion from status
        if (string.IsNullOrWhiteSpace(status.ConfigurationVersion))
        {
            problems.Add("ConfigurationVersion is null or whitespace.");
        }
        else if (!Version.TryParse(status.ConfigurationVersion, out _))
        {
            problems.Add("ConfigurationVersion is not a valid version string.");
        }

        // Validate Timestamp from status (should not be default/MinValue)
        if (status.Timestamp == default)
        {
            problems.Add("Timestamp is not set (default DateTime).");
        }
        else if (status.Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("Timestamp is in the future.");
        }
        else if (status.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("Timestamp is more than one year in the past.");
        }

        // Validate TotalDataPointsProcessed from status (should not be negative)
        if (status.TotalDataPointsProcessed < 0)
        {
            problems.Add("TotalDataPointsProcessed is negative.");
        }

        // Validate TotalDataPointsFailed from status (should not be negative)
        if (status.TotalDataPointsFailed < 0)
        {
            problems.Add("TotalDataPointsFailed is negative.");
        }

        // Validate PendingItemsInQueue from status (should not be negative)
        if (status.PendingItemsInQueue < 0)
        {
            problems.Add("PendingItemsInQueue is negative.");
        }

        // Validate throughput values (should be non-negative)
        if (value.GetThroughput() < 0)
        {
            problems.Add("Throughput is negative.");
        }

        // IsRunning state is already validated via status.IsRunning property

        // Validate IsRunning state from status
        if (!status.IsRunning && value.GetThroughput() > 0)
        {
            problems.Add("Pipeline is not running but has non-zero throughput.");
        }

        return problems.AsReadOnly();
    }

/// <summary>
/// Determines whether the specified <see cref="PipelineOrchestrator"/> is valid.
/// </summary>
/// <param name="value">The orchestrator to check.</param>
/// <returns>True if valid; otherwise, false.</returns>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
public static bool IsValid(this PipelineOrchestrator value)
    => value is not null && value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="PipelineOrchestrator"/> is valid.
    /// </summary>
    /// <param name="value">The orchestrator to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the orchestrator is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this PipelineOrchestrator value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"PipelineOrchestrator is not valid. Problems: {string.Join(", ", problems)}",
            nameof(value));
    }
}