using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetRealtimePipeline.Domain.Models
{
    /// <summary>
    /// Provides extension methods for the <see cref="PipelineConfig"/> class to simplify common operations
    /// on pipeline configurations and their stages.
    /// </summary>
    public static class PipelineConfigExtensions
    {
        /// <summary>
        /// Gets the total number of stages in the pipeline configuration.
        /// </summary>
        /// <param name="config">The pipeline configuration.</param>
        /// <returns>The total number of stages.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
        public static int GetTotalStages(this PipelineConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);
            return config.Stages.Count;
        }

        /// <summary>
        /// Determines whether the pipeline configuration has any stages.
        /// </summary>
        /// <param name="config">The pipeline configuration.</param>
        /// <returns>True if the pipeline configuration has any stages; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
        public static bool HasStages(this PipelineConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);
            return config.Stages.Any();
        }

        /// <summary>
        /// Gets the names of all stages in the pipeline configuration.
        /// </summary>
        /// <param name="config">The pipeline configuration.</param>
        /// <returns>An enumerable collection of stage names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
        public static IEnumerable<string> GetStageNames(this PipelineConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);
            return config.Stages.Select(s => s.StageName);
        }

        /// <summary>
        /// Determines whether the pipeline configuration has a stage with the specified name.
        /// </summary>
        /// <param name="config">The pipeline configuration.</param>
        /// <param name="stageName">The name of the stage to search for.</param>
        /// <returns>True if the pipeline configuration has a stage with the specified name; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="stageName"/> is null or empty.</exception>
        public static bool HasStage(this PipelineConfig config, string stageName)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentException.ThrowIfNullOrEmpty(stageName);
            return config.Stages.Any(s => s.StageName == stageName);
        }

        /// <summary>
        /// Gets the stage definition with the specified name, or null if not found.
        /// </summary>
        /// <param name="config">The pipeline configuration.</param>
        /// <param name="stageName">The name of the stage to find.</param>
        /// <returns>The stage definition if found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="stageName"/> is null or empty.</exception>
        public static PipelineStageDef? GetStageByName(this PipelineConfig config, string stageName)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentException.ThrowIfNullOrEmpty(stageName);
            return config.Stages.FirstOrDefault(s => s.StageName == stageName);
        }

        /// <summary>
        /// Gets the first stage that matches the specified predicate.
        /// </summary>
        /// <param name="config">The pipeline configuration.</param>
        /// <param name="predicate">A function to test each stage for a condition.</param>
        /// <returns>The first stage that matches the predicate, or null if no match.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="config"/> is null.
        /// Thrown if <paramref name="predicate"/> is null.
        /// </exception>
        public static PipelineStageDef? FindStage(this PipelineConfig config, Func<PipelineStageDef, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(predicate);
            return config.Stages.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Gets all stages that match the specified predicate.
        /// </summary>
        /// <param name="config">The pipeline configuration.</param>
        /// <param name="predicate">A function to test each stage for a condition.</param>
        /// <returns>An enumerable collection of matching stages.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="config"/> is null.
        /// Thrown if <paramref name="predicate"/> is null.
        /// </exception>
        public static IEnumerable<PipelineStageDef> FindStages(this PipelineConfig config, Func<PipelineStageDef, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(predicate);
            return config.Stages.Where(predicate);
        }

        /// <summary>
        /// Gets all enabled stages in the pipeline configuration.
        /// </summary>
        /// <param name="config">The pipeline configuration.</param>
        /// <returns>An enumerable collection of enabled stages.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
        public static IEnumerable<PipelineStageDef> GetEnabledStages(this PipelineConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);
            return config.Stages.Where(s => s.Enabled);
        }

        /// <summary>
        /// Gets all stages of a specific type in the pipeline configuration.
        /// </summary>
        /// <param name="config">The pipeline configuration.</param>
        /// <param name="stageType">The type of stage to filter by (e.g., "filter", "transform", "aggregate").</param>
        /// <returns>An enumerable collection of stages matching the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="stageType"/> is null or empty.</exception>
        public static IEnumerable<PipelineStageDef> GetStagesByType(this PipelineConfig config, string stageType)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentException.ThrowIfNullOrEmpty(stageType);
            return config.Stages.Where(s => s.StageType == stageType);
        }
    }
}
