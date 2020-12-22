using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetRealtimePipeline.Domain.Models
{
    /// <summary>
    /// Provides extension methods for the <see cref="PipelineConfig"/> class.
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
    }
}
