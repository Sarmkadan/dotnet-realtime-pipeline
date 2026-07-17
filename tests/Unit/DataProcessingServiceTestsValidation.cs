using System;
using System.Collections.Generic;

namespace DotNetRealtimePipeline.Tests.Unit
{
    /// <summary>
    /// Provides validation methods for <see cref="DataProcessingServiceTests"/> instances.
    /// Validates that test fixture delegates are properly initialized before test execution.
    /// </summary>
    public static class DataProcessingServiceTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="DataProcessingServiceTests"/> instance.
        /// Checks that all test delegate fields are initialized and not null.
        /// </summary>
        /// <param name="value">The test fixture instance to validate.</param>
        /// <returns>A list of validation errors; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this DataProcessingServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate public test delegates are not null
            if (value.ProcessDataPointAsync_ValidPoint_ShouldSucceed is null)
            {
                errors.Add("ProcessDataPointAsync_ValidPoint_ShouldSucceed delegate cannot be null.");
            }

            if (value.ProcessDataPointAsync_InvalidPoint_ShouldFail is null)
            {
                errors.Add("ProcessDataPointAsync_InvalidPoint_ShouldFail delegate cannot be null.");
            }

            if (value.ProcessDataPointAsync_LowQuality_ShouldFail is null)
            {
                errors.Add("ProcessDataPointAsync_LowQuality_ShouldFail delegate cannot be null.");
            }

            if (value.AnalyzeDataQuality_ValidPoints_ShouldReturnCorrectStats is null)
            {
                errors.Add("AnalyzeDataQuality_ValidPoints_ShouldReturnCorrectStats delegate cannot be null.");
            }

            if (value.AnalyzeDataQuality_NullPoints_ShouldReturnDefault is null)
            {
                errors.Add("AnalyzeDataQuality_NullPoints_ShouldReturnDefault delegate cannot be null.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="DataProcessingServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid (all delegates are initialized); otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this DataProcessingServiceTests value) => value.Validate().Count == 0;

        /// <summary>
        /// Ensures that the specified <see cref="DataProcessingServiceTests"/> instance is valid.
        /// Throws an exception if any test delegates are null.
        /// </summary>
        /// <param name="value">The test fixture instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance contains null delegates.</exception>
        public static void EnsureValid(this DataProcessingServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();

            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"DataProcessingServiceTests instance is invalid. Errors: {string.Join(" ", errors)}");
            }
        }
    }
}