using System;
using System.Collections.Generic;
using System.Globalization;

namespace Unit.Tests
{
    public static class PipelineVisualizerTestsValidation
    {
        /// <summary>
        /// Validates a <see cref="PipelineVisualizerTests"/> instance for common issues.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of human-readable problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this PipelineVisualizerTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate public methods are non-null and non-empty where applicable
            // These are method names, so we just ensure they're not null/empty strings
            if (string.IsNullOrEmpty(value.BuildNodes_WithValidConfig_ReturnsOneNodePerStage))
            {
                errors.Add("BuildNodes_WithValidConfig_ReturnsOneNodePerStage cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(value.BuildNodes_EdgesAreLinkedSequentially))
            {
                errors.Add("BuildNodes_EdgesAreLinkedSequentially cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(value.Render_ContainsPipelineName))
            {
                errors.Add("Render_ContainsPipelineName cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(value.Render_ContainsAllStageNames))
            {
                errors.Add("Render_ContainsAllStageNames cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(value.RenderCompact_ContainsSeparators))
            {
                errors.Add("RenderCompact_ContainsSeparators cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(value.PipelineVisualizationNode_ComputeHealthLabel_BackpressuredIsCritical))
            {
                errors.Add("PipelineVisualizationNode_ComputeHealthLabel_BackpressuredIsCritical cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(value.PipelineVisualizationNode_ComputeHealthLabel_HighBufferIsWarning))
            {
                errors.Add("PipelineVisualizationNode_ComputeHealthLabel_HighBufferIsWarning cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(value.PipelineVisualizationNode_ComputeHealthLabel_NormalIsHealthy))
            {
                errors.Add("PipelineVisualizationNode_ComputeHealthLabel_NormalIsHealthy cannot be null or empty.");
            }

            return errors;
        }

        /// <summary>
        /// Determines whether the specified <see cref="PipelineVisualizerTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this PipelineVisualizerTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="PipelineVisualizerTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of problems.</exception>
        public static void EnsureValid(this PipelineVisualizerTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"PipelineVisualizerTests instance is not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }
    }
}