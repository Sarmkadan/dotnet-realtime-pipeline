#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Visualization;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="PipelineVisualizationNode"/> instances.
/// </summary>
public static class PipelineVisualizationNodeValidation
{
    private static readonly HashSet<string> ValidHealthLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        "HEALTHY",
        "WARNING",
        "CRITICAL"
    };

    /// <summary>
    /// Validates the specified <see cref="PipelineVisualizationNode"/> instance.
    /// </summary>
    /// <param name="value">The node to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PipelineVisualizationNode? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate StageName
        if (string.IsNullOrEmpty(value.StageName))
        {
            problems.Add("StageName must be a non-empty string.");
        }

        // Validate StageType
        if (string.IsNullOrEmpty(value.StageType))
        {
            problems.Add("StageType must be a non-empty string.");
        }

        // Validate BufferFillPercent (0-100 inclusive)
        if (value.BufferFillPercent < 0 || value.BufferFillPercent > 100)
        {
            problems.Add("BufferFillPercent must be between 0 and 100 inclusive.");
        }

        // Validate ThroughputEps (non-negative)
        if (value.ThroughputEps < 0)
        {
            problems.Add("ThroughputEps must be non-negative.");
        }

        // Validate DroppedItems (non-negative)
        if (value.DroppedItems < 0)
        {
            problems.Add("DroppedItems must be non-negative.");
        }

        // Validate HealthLabel
        if (string.IsNullOrEmpty(value.HealthLabel) || !ValidHealthLabels.Contains(value.HealthLabel))
        {
            problems.Add("HealthLabel must be one of: HEALTHY, WARNING, CRITICAL.");
        }

        // Validate DownstreamStages list
        if (value.DownstreamStages is null)
        {
            problems.Add("DownstreamStages must not be null.");
        }
        else
        {
            for (var i = 0; i < value.DownstreamStages.Count; i++)
            {
                var stage = value.DownstreamStages[i];
                if (string.IsNullOrEmpty(stage))
                {
                    problems.Add($"DownstreamStages[{i}] must be a non-empty string.");
                }
            }
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified <see cref="PipelineVisualizationNode"/> is valid.
    /// </summary>
    /// <param name="value">The node to check.</param>
    /// <returns>True if the node is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PipelineVisualizationNode? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="PipelineVisualizationNode"/> is valid.
    /// </summary>
    /// <param name="value">The node to check.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the node is invalid. The exception message lists all validation problems.</exception>
    public static void EnsureValid(this PipelineVisualizationNode? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"PipelineVisualizationNode is invalid:{Environment.NewLine}- {
                string.Join(
                    $"{Environment.NewLine}- ",
                    problems
                )
            }");
    }
}