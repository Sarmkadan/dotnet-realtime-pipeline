using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DotNetRealtimePipeline.Data.Repositories
{
    /// <summary>
    /// Provides validation helpers for <see cref="InMemoryMetricsRepository"/> instances.
    /// </summary>
    public static class InMemoryMetricsRepositoryValidation
    {
        /// <summary>
        /// Validates the specified <see cref="InMemoryMetricsRepository"/> instance.
        /// </summary>
        /// <param name="value">The repository to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this InMemoryMetricsRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate public members
            if (value.GetByIdAsync == null)
            {
                problems.Add("GetByIdAsync cannot be null.");
            }

            if (value.GetByTimeRangeAsync == null)
            {
                problems.Add("GetByTimeRangeAsync cannot be null.");
            }

            if (value.GetByTypeAsync == null)
            {
                problems.Add("GetByTypeAsync cannot be null.");
            }

            if (value.SaveAsync == null)
            {
                problems.Add("SaveAsync cannot be null.");
            }

            if (value.DeleteAsync == null)
            {
                problems.Add("DeleteAsync cannot be null.");
            }

            if (value.GetLatestAsync == null)
            {
                problems.Add("GetLatestAsync cannot be null.");
            }

            if (value.GetHistoryAsync == null)
            {
                problems.Add("GetHistoryAsync cannot be null.");
            }

            if (value.Clear == null)
            {
                problems.Add("Clear cannot be null.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="InMemoryMetricsRepository"/> is valid.
        /// </summary>
        /// <param name="value">The repository to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this InMemoryMetricsRepository value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="InMemoryMetricsRepository"/> is valid.
        /// </summary>
        /// <param name="value">The repository to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of problems.</exception>
        public static void EnsureValid(this InMemoryMetricsRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"InMemoryMetricsRepository is invalid. Problems:\n{string.Join("\n", problems)}");
            }
        }
    }
}