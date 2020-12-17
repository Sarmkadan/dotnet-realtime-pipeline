using System;
using System.Collections.Generic;
using System.Globalization;

namespace tests.Unit
{
    public static class BackpressureServiceTestsValidation
    {
        /// <summary>
        /// Validates the BackpressureServiceTests instance for common issues.
        /// </summary>
        /// <param name="value">The BackpressureServiceTests instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        public static IReadOnlyList<string> Validate(this BackpressureServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate public methods exist (compiler will catch missing members)
            // No instance fields to validate beyond null check

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the BackpressureServiceTests instance is valid.
        /// </summary>
        /// <param name="value">The BackpressureServiceTests instance to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        public static bool IsValid(this BackpressureServiceTests value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures the BackpressureServiceTests instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The BackpressureServiceTests instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        /// <exception cref="ArgumentException">Thrown if validation fails, containing a list of problems.</exception>
        public static void EnsureValid(this BackpressureServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"BackpressureServiceTests is invalid:{Environment.NewLine}" +
                    string.Join(Environment.NewLine, problems));
            }
        }
    }
}