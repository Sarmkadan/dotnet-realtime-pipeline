using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetRealtimePipeline.Data.Repositories
{
    /// <summary>
    /// Provides validation helpers for <see cref="InMemoryMetricsRepository"/> instances.
    /// </summary>
    public static class InMemoryMetricsRepositoryValidation
    {
        /// <summary>
        /// Validates the internal state of the specified <see cref="InMemoryMetricsRepository"/> instance.
        /// </summary>
        /// <param name="value">The repository to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this InMemoryMetricsRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate internal state consistency
            try
            {
                var metrics = value.GetInternalMetrics();

                if (metrics == null)
                {
                    problems.Add("GetInternalMetrics() returned null.");
                }
                else
                {
                    // Check for null metrics in the list
                    var nullMetrics = metrics.Where(m => m is null).ToList();
                    if (nullMetrics.Count > 0)
                    {
                        problems.Add($"Internal metrics list contains {nullMetrics.Count} null entries.");
                    }

                    // Check for duplicate MetricIds
                    var duplicateMetricIds = metrics
                        .GroupBy(m => m?.MetricId ?? -1)
                        .Where(g => g.Count() > 1 && g.Key != -1)
                        .Select(g => g.Key)
                        .ToList();

                    if (duplicateMetricIds.Count > 0)
                    {
                        problems.Add($"Internal metrics list contains duplicate MetricIds: {string.Join(", ", duplicateMetricIds)}.");
                    }

                    // Check for metrics with invalid time windows
                    var invalidTimeMetrics = metrics.Where(m => m != null &&
                        (m.TimeWindowStartMs > m.TimeWindowEndMs ||
                         m.TimeWindowStartMs < 0 ||
                         m.TimeWindowEndMs < 0)).ToList();

                    if (invalidTimeMetrics.Count > 0)
                    {
                        problems.Add($"Internal metrics list contains {invalidTimeMetrics.Count} metrics with invalid time windows.");
                    }
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Exception thrown during internal state validation: {ex.GetType().Name}: {ex.Message}");
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
                    $"InMemoryMetricsRepository internal state is invalid. Problems:\n{string.Join("\n", problems)}");
            }
        }
    }
}