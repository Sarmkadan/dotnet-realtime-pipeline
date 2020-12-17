using System;
using System.Collections.Generic;
using System.Globalization;

namespace RealTimePipeline.Tests.Unit
{
    /// <summary>
    /// Validation helpers for <see cref="DataProcessingServiceTests"/> unit tests.
    /// </summary>
    public static class DataProcessingServiceTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="DataProcessingServiceTests"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        public static IReadOnlyList<string> Validate(this DataProcessingServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate public methods are not null (they're delegates)
            if (value.ProcessDataPointAsync_ValidPoint_ShouldSucceed == null)
            {
                errors.Add("ProcessDataPointAsync_ValidPoint_ShouldSucceed delegate cannot be null.");
            }

            if (value.ProcessDataPointAsync_InvalidPoint_ShouldFail == null)
            {
                errors.Add("ProcessDataPointAsync_InvalidPoint_ShouldFail delegate cannot be null.");
            }

            if (value.ProcessDataPointAsync_LowQuality_ShouldFail == null)
            {
                errors.Add("ProcessDataPointAsync_LowQuality_ShouldFail delegate cannot be null.");
            }

            if (value.AnalyzeDataQuality_ValidPoints_ShouldReturnCorrectStats == null)
            {
                errors.Add("AnalyzeDataQuality_ValidPoints_ShouldReturnCorrectStats delegate cannot be null.");
            }

            if (value.AnalyzeDataQuality_NullPoints_ShouldReturnDefault == null)
            {
                errors.Add("AnalyzeDataQuality_NullPoints_ShouldReturnDefault delegate cannot be null.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="DataProcessingServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this DataProcessingServiceTests value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="DataProcessingServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid.</exception>
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