using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetRealtimePipeline.Tests.Unit
{
    /// <summary>
    /// Provides validation helpers for <see cref="MetricAggregationTests"/> instances.
    /// </summary>
    public static class MetricAggregationTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="MetricAggregationTests"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this MetricAggregationTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate test methods exist using reflection
            var methods = typeof(MetricAggregationTests).GetMethods();
            var methodNames = methods.Select(m => m.Name).ToHashSet();

            if (!methodNames.Contains(nameof(MetricAggregationTests.CalculateThroughput_WithValidWindow_ReturnsItemsPerSecond)))
            {
                problems.Add($"Test method '{nameof(MetricAggregationTests.CalculateThroughput_WithValidWindow_ReturnsItemsPerSecond)}' is missing.");
            }

            if (!methodNames.Contains(nameof(MetricAggregationTests.CalculateThroughput_WithZeroDurationWindow_ReturnsZero)))
            {
                problems.Add($"Test method '{nameof(MetricAggregationTests.CalculateThroughput_WithZeroDurationWindow_ReturnsZero)}' is missing.");
            }

            if (!methodNames.Contains(nameof(MetricAggregationTests.CalculateErrorRate_WithFailedItems_ReturnsCorrectPercentage)))
            {
                problems.Add($"Test method '{nameof(MetricAggregationTests.CalculateErrorRate_WithFailedItems_ReturnsCorrectPercentage)}' is missing.");
            }

            if (!methodNames.Contains(nameof(MetricAggregationTests.CalculateSuccessRate_WhenNoItemsRecorded_ReturnsHundredPercent)))
            {
                problems.Add($"Test method '{nameof(MetricAggregationTests.CalculateSuccessRate_WhenNoItemsRecorded_ReturnsHundredPercent)}' is missing.");
            }

            if (!methodNames.Contains(nameof(MetricAggregationTests.IsUnhealthy_WhenErrorRateExceedsFivePercent_ReturnsTrue)))
            {
                problems.Add($"Test method '{nameof(MetricAggregationTests.IsUnhealthy_WhenErrorRateExceedsFivePercent_ReturnsTrue)}' is missing.");
            }

            if (!methodNames.Contains(nameof(MetricAggregationTests.ComputeAverageProcessingTime_WithValidSamples_SetsCorrectAverage)))
            {
                problems.Add($"Test method '{nameof(MetricAggregationTests.ComputeAverageProcessingTime_WithValidSamples_SetsCorrectAverage)}' is missing.");
            }

            if (!methodNames.Contains(nameof(MetricAggregationTests.ComputeAverageProcessingTime_WithEmptyList_SetsAverageToZero)))
            {
                problems.Add($"Test method '{nameof(MetricAggregationTests.ComputeAverageProcessingTime_WithEmptyList_SetsAverageToZero)}' is missing.");
            }

            if (!methodNames.Contains(nameof(MetricAggregationTests.CalculateBackpressureRatio_WithKnownBackpressureDuration_ReturnsCorrectRatio)))
            {
                problems.Add($"Test method '{nameof(MetricAggregationTests.CalculateBackpressureRatio_WithKnownBackpressureDuration_ReturnsCorrectRatio)}' is missing.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="MetricAggregationTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this MetricAggregationTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="MetricAggregationTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing the validation problems.</exception>
        public static void EnsureValid(this MetricAggregationTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    message: $"MetricAggregationTests instance is invalid. Problems: {string.Join(", ", problems)}",
                    paramName: nameof(value));
            }
        }
    }
}