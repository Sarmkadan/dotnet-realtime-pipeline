using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetRealtimePipeline.Services
{
    /// <summary>
    /// Provides validation helpers for <see cref="MetricsService"/> instances.
    /// </summary>
    public static class MetricsServiceValidation
    {
        /// <summary>
        /// Validates the specified <see cref="MetricsService"/> instance.
        /// </summary>
        /// <param name="value">The metrics service instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this MetricsService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            return problems;
        }

        /// <summary>
        /// Determines whether the specified <see cref="MetricsService"/> instance is valid.
        /// </summary>
        /// <param name="value">The metrics service instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this MetricsService value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="MetricsService"/> instance is valid.
        /// </summary>
        /// <param name="value">The metrics service instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of problems.</exception>
        public static void EnsureValid(this MetricsService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"MetricsService is not valid. Problems:\n{string.Join("\n", problems)}");
            }
        }
    }
}