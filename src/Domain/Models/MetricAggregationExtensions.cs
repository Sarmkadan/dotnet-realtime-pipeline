using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DotNetRealtimePipeline.Domain.Models
{
    /// <summary>
    /// Extension methods for <see cref="MetricAggregation"/> that provide common metric calculations and aggregations.
    /// </summary>
    public static class MetricAggregationExtensions
    {
        /// <summary>
        /// Calculates the overall success rate of processing as a value between 0.0 and 1.0.
        /// </summary>
        /// <param name="aggregation">The metric aggregation to calculate success rate for.</param>
        /// <returns>The success rate (0.0 to 1.0).</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregation"/> is null.</exception>
        public static double CalculateSuccessRate(this MetricAggregation aggregation)
        {
            ArgumentNullException.ThrowIfNull(aggregation);

            var totalProcessed = aggregation.TotalItemsProcessed;
            if (totalProcessed == 0)
            {
                return 0.0;
            }

            var failed = aggregation.TotalItemsFailed;
            return 1.0 - ((double)failed / totalProcessed);
        }

        /// <summary>
        /// Calculates the error rate across all stages combined.
        /// </summary>
        /// <param name="aggregation">The metric aggregation to calculate error rate for.</param>
        /// <returns>The combined error rate (0.0 to 1.0).</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregation"/> is null.</exception>
        public static double CalculateCombinedErrorRate(this MetricAggregation aggregation)
        {
            ArgumentNullException.ThrowIfNull(aggregation);

            if (aggregation.ErrorRateByStage == null || aggregation.ErrorRateByStage.Count == 0)
            {
                return 0.0;
            }

            return aggregation.ErrorRateByStage.Values.Average();
        }

        /// <summary>
        /// Gets the total duration of the time window in milliseconds.
        /// </summary>
        /// <param name="aggregation">The metric aggregation to get duration for.</param>
        /// <returns>The duration in milliseconds.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregation"/> is null.</exception>
        public static long GetTimeWindowDurationMs(this MetricAggregation aggregation)
        {
            ArgumentNullException.ThrowIfNull(aggregation);

            return aggregation.TimeWindowEndMs - aggregation.TimeWindowStartMs;
        }

        /// <summary>
        /// Gets the total duration of the time window as a TimeSpan.
        /// </summary>
        /// <param name="aggregation">The metric aggregation to get duration for.</param>
        /// <returns>The duration as a TimeSpan.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregation"/> is null.</exception>
        public static TimeSpan GetTimeWindowDuration(this MetricAggregation aggregation)
        {
            ArgumentNullException.ThrowIfNull(aggregation);

            var durationMs = aggregation.GetTimeWindowDurationMs();
            return TimeSpan.FromMilliseconds(durationMs);
        }

        /// <summary>
        /// Gets the list of source names that contributed to this aggregation, sorted alphabetically.
        /// </summary>
        /// <param name="aggregation">The metric aggregation to get sources for.</param>
        /// <returns>An enumerable of source names.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregation"/> is null.</exception>
        public static IEnumerable<string> GetSourceNames(this MetricAggregation aggregation)
        {
            ArgumentNullException.ThrowIfNull(aggregation);

            return aggregation.CountBySource?.Keys.OrderBy(name => name, StringComparer.Ordinal) ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets the list of stage names that had errors, sorted by error rate descending.
        /// </summary>
        /// <param name="aggregation">The metric aggregation to get error stages for.</param>
        /// <returns>An enumerable of stage names with errors, ordered by error rate.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregation"/> is null.</exception>
        public static IEnumerable<string> GetStagesWithErrors(this MetricAggregation aggregation)
        {
            ArgumentNullException.ThrowIfNull(aggregation);

            return aggregation.ErrorRateByStage
                ?.Where(kvp => kvp.Value > 0.0)
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Calculates the total number of items across all sources.
        /// </summary>
        /// <param name="aggregation">The metric aggregation to sum sources for.</param>
        /// <returns>The total count across all sources.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregation"/> is null.</exception>
        public static long GetTotalItemsFromSources(this MetricAggregation aggregation)
        {
            ArgumentNullException.ThrowIfNull(aggregation);

            return aggregation.CountBySource?.Values.Sum() ?? 0L;
        }

        /// <summary>
        /// Gets the percentage of time spent in backpressure relative to the total window duration.
        /// </summary>
        /// <param name="aggregation">The metric aggregation to calculate backpressure percentage for.</param>
        /// <returns>The percentage of time spent in backpressure (0.0 to 100.0).</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregation"/> is null.</exception>
        public static double GetBackpressurePercentage(this MetricAggregation aggregation)
        {
            ArgumentNullException.ThrowIfNull(aggregation);

            var totalDurationMs = aggregation.GetTimeWindowDurationMs();
            if (totalDurationMs <= 0)
            {
                return 0.0;
            }

            var backpressureMs = aggregation.TotalBackpressureMs;
            return (double)backpressureMs / totalDurationMs * 100.0;
        }

        /// <summary>
        /// Gets the average processing time across all percentiles (P95 and P99).
        /// </summary>
        /// <param name="aggregation">The metric aggregation to calculate average percentile for.</param>
        /// <returns>The average of P95 and P99 percentiles.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregation"/> is null.</exception>
        public static double GetAveragePercentile(this MetricAggregation aggregation)
        {
            ArgumentNullException.ThrowIfNull(aggregation);

            return (aggregation.P95ProcessingTimeMs + aggregation.P99ProcessingTimeMs) / 2.0;
        }

        /// <summary>
        /// Creates a new MetricAggregation that represents the sum of multiple aggregations.
        /// Useful for combining metrics across time periods or sources.
        /// </summary>
        /// <param name="aggregations">The aggregations to combine.</param>
        /// <returns>A new MetricAggregation representing the combined metrics.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregations"/> is null or contains null elements.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="aggregations"/> is empty.</exception>
        public static MetricAggregation Combine(this IEnumerable<MetricAggregation> aggregations)
        {
            ArgumentNullException.ThrowIfNull(aggregations);

            var list = aggregations.ToList();
            if (list.Count == 0)
            {
                throw new ArgumentException("Aggregations collection cannot be empty.", nameof(aggregations));
            }

            ArgumentNullException.ThrowIfNull(list);

            // Use first aggregation as base for metadata
            var first = list[0];

            var combined = new MetricAggregation
            {
                MetricId = first.MetricId,
                MetricType = first.MetricType,
                TimeWindowStartMs = first.TimeWindowStartMs,
                TimeWindowEndMs = list.Max(a => a.TimeWindowEndMs),
                ComputedAt = DateTime.UtcNow
            };

            // Sum all counters
            combined.TotalItemsProcessed = list.Sum(a => a.TotalItemsProcessed);
            combined.TotalItemsFailed = list.Sum(a => a.TotalItemsFailed);
            combined.TotalItemsSkipped = list.Sum(a => a.TotalItemsSkipped);
            combined.BackpressureEvents = list.Sum(a => a.BackpressureEvents);
            combined.TotalBackpressureMs = list.Sum(a => a.TotalBackpressureMs);

            // Average all timing metrics
            var count = list.Count;
            combined.AverageProcessingTimeMs = list.Average(a => a.AverageProcessingTimeMs);
            combined.MinProcessingTimeMs = list.Min(a => a.MinProcessingTimeMs);
            combined.MaxProcessingTimeMs = list.Max(a => a.MaxProcessingTimeMs);
            combined.P95ProcessingTimeMs = list.Average(a => a.P95ProcessingTimeMs);
            combined.P99ProcessingTimeMs = list.Average(a => a.P99ProcessingTimeMs);

            // Merge dictionaries by summing values for common keys
            combined.CountBySource = list
                .SelectMany(a => a.CountBySource ?? new Dictionary<string, long>())
                .GroupBy(kvp => kvp.Key, kvp => kvp.Value)
                .ToDictionary(g => g.Key, g => g.Sum());

            combined.ErrorRateByStage = list
                .SelectMany(a => a.ErrorRateByStage ?? new Dictionary<string, double>())
                .GroupBy(kvp => kvp.Key, kvp => kvp.Value)
                .ToDictionary(g => g.Key, g => g.Average());

            return combined;
        }

        /// <summary>
        /// Filters the aggregation to only include sources that match the specified predicate.
        /// </summary>
        /// <param name="aggregation">The metric aggregation to filter.</param>
        /// <param name="predicate">The predicate to filter sources by.</param>
        /// <returns>A new MetricAggregation with only matching sources.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregation"/> or <paramref name="predicate"/> is null.</exception>
        public static MetricAggregation FilterBySource(this MetricAggregation aggregation, Func<string, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(aggregation);
            ArgumentNullException.ThrowIfNull(predicate);

            var filtered = new MetricAggregation
            {
                MetricId = aggregation.MetricId,
                MetricType = aggregation.MetricType,
                TimeWindowStartMs = aggregation.TimeWindowStartMs,
                TimeWindowEndMs = aggregation.TimeWindowEndMs,
                TotalItemsProcessed = 0,
                TotalItemsFailed = 0,
                TotalItemsSkipped = 0,
                BackpressureEvents = 0,
                TotalBackpressureMs = 0,
                AverageProcessingTimeMs = 0,
                MinProcessingTimeMs = double.MaxValue,
                MaxProcessingTimeMs = double.MinValue,
                P95ProcessingTimeMs = 0,
                P99ProcessingTimeMs = 0,
                ComputedAt = DateTime.UtcNow,
                CountBySource = new Dictionary<string, long>(),
                ErrorRateByStage = new Dictionary<string, double>()
            };

            // Filter and scale counts
            if (aggregation.CountBySource != null)
            {
                foreach (var kvp in aggregation.CountBySource)
                {
                    if (predicate(kvp.Key))
                    {
                        filtered.CountBySource[kvp.Key] = kvp.Value;
                        filtered.TotalItemsProcessed += kvp.Value;
                    }
                }
            }

            // Filter error rates
            if (aggregation.ErrorRateByStage != null)
            {
                foreach (var kvp in aggregation.ErrorRateByStage)
                {
                    if (predicate(kvp.Key))
                    {
                        filtered.ErrorRateByStage[kvp.Key] = kvp.Value;
                    }
                }
            }

            // Scale other metrics proportionally
            if (aggregation.TotalItemsProcessed > 0)
            {
                var ratio = (double)filtered.TotalItemsProcessed / aggregation.TotalItemsProcessed;
                filtered.TotalItemsFailed = (long)(aggregation.TotalItemsFailed * ratio);
                filtered.TotalItemsSkipped = (long)(aggregation.TotalItemsSkipped * ratio);
                filtered.BackpressureEvents = (long)(aggregation.BackpressureEvents * ratio);
                filtered.TotalBackpressureMs = (long)(aggregation.TotalBackpressureMs * ratio);
                filtered.AverageProcessingTimeMs = aggregation.AverageProcessingTimeMs * ratio;
                filtered.MinProcessingTimeMs = aggregation.MinProcessingTimeMs * ratio;
                filtered.MaxProcessingTimeMs = aggregation.MaxProcessingTimeMs * ratio;
                filtered.P95ProcessingTimeMs = aggregation.P95ProcessingTimeMs * ratio;
                filtered.P99ProcessingTimeMs = aggregation.P99ProcessingTimeMs * ratio;
            }

            return filtered;
        }
    }
}