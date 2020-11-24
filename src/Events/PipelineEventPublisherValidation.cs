using System;
using System.Collections.Generic;

namespace DotNetRealtimePipeline.Events
{
    using DotNetRealtimePipeline.Domain.Models;

    /// <summary>
    /// Provides validation helpers for <see cref="PipelineEventPublisher"/> and related pipeline event types.
    /// </summary>
    public static class PipelineEventPublisherValidation
    {
        /// <summary>
        /// Validates a <see cref="DataPoint"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The data point to validate.</param>
        /// <returns>A read-only list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this DataPoint value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            if (value.Id <= 0)
            {
                problems.Add("DataPoint.Id must be a positive value.");
            }

            if (value.Timestamp <= 0)
            {
                problems.Add("DataPoint.Timestamp must be a positive Unix timestamp.");
            }

            if (value.Value < 0)
            {
                problems.Add("DataPoint.Value must be a non-negative number.");
            }

            if (string.IsNullOrWhiteSpace(value.Source))
            {
                problems.Add("DataPoint.Source must be a non-empty, non-whitespace string.");
            }

            if (value.Quality < 0 || value.Quality > 100)
            {
                problems.Add("DataPoint.Quality must be between 0 and 100 inclusive.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="DataPoint"/> instance is valid.
        /// </summary>
        /// <param name="value">The data point to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this DataPoint value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="DataPoint"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="value">The data point to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of problems.</exception>
        public static void EnsureValid(this DataPoint value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();

            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"DataPoint is invalid. Problems:\n  - {
                    string.Join("\n  - ", problems)
                    }");
            }
        }

        /// <summary>
        /// Validates a <see cref="ProcessingResult"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The processing result to validate.</param>
        /// <returns>A read-only list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ProcessingResult value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            if (value.ResultId <= 0)
            {
                problems.Add("ProcessingResult.ResultId must be a positive value.");
            }

            if (string.IsNullOrWhiteSpace(value.StageName))
            {
                problems.Add("ProcessingResult.StageName must be a non-empty, non-whitespace string.");
            }

            if (value.ProcessingTimeMs < 0)
            {
                problems.Add("ProcessingResult.ProcessingTimeMs must be a non-negative value.");
            }

            if (value.RetryCount < 0)
            {
                problems.Add("ProcessingResult.RetryCount must be a non-negative value.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="ProcessingResult"/> instance is valid.
        /// </summary>
        /// <param name="value">The processing result to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this ProcessingResult value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="ProcessingResult"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="value">The processing result to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of problems.</exception>
        public static void EnsureValid(this ProcessingResult value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();

            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"ProcessingResult is invalid. Problems:\n  - {
                    string.Join("\n  - ", problems)
                    }");
            }
        }

        /// <summary>
        /// Validates a <see cref="BackpressureContext"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The backpressure context to validate.</param>
        /// <returns>A read-only list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this BackpressureContext value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            if (value.ContextId <= 0)
            {
                problems.Add("BackpressureContext.ContextId must be a positive value.");
            }

            if (string.IsNullOrWhiteSpace(value.PipelineStageName))
            {
                problems.Add("BackpressureContext.PipelineStageName must be a non-empty, non-whitespace string.");
            }

            if (value.MaxBufferCapacity <= 0)
            {
                problems.Add("BackpressureContext.MaxBufferCapacity must be a positive value.");
            }

            if (value.BufferSize < 0)
            {
                problems.Add("BackpressureContext.BufferSize must be a non-negative value.");
            }

            if (value.BufferSize > value.MaxBufferCapacity)
            {
                problems.Add("BackpressureContext.BufferSize cannot exceed MaxBufferCapacity.");
            }

            if (value.ActiveConsumers < 0)
            {
                problems.Add("BackpressureContext.ActiveConsumers must be a non-negative value.");
            }

            if (value.MaxConcurrentConsumers <= 0)
            {
                problems.Add("BackpressureContext.MaxConcurrentConsumers must be a positive value.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="BackpressureContext"/> instance is valid.
        /// </summary>
        /// <param name="value">The backpressure context to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this BackpressureContext value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="BackpressureContext"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="value">The backpressure context to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of problems.</exception>
        public static void EnsureValid(this BackpressureContext value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();

            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"BackpressureContext is invalid. Problems:\n  - {
                    string.Join("\n  - ", problems)
                    }");
            }
        }

        /// <summary>
        /// Validates a <see cref="MetricAggregation"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The metric aggregation to validate.</param>
        /// <returns>A read-only list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this MetricAggregation value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            if (value.MetricId <= 0)
            {
                problems.Add("MetricAggregation.MetricId must be a positive value.");
            }

            if (value.TimeWindowStartMs <= 0)
            {
                problems.Add("MetricAggregation.TimeWindowStartMs must be a positive Unix timestamp.");
            }

            if (value.TimeWindowEndMs <= 0)
            {
                problems.Add("MetricAggregation.TimeWindowEndMs must be a positive Unix timestamp.");
            }

            if (value.TimeWindowEndMs < value.TimeWindowStartMs)
            {
                problems.Add("MetricAggregation.TimeWindowEndMs must be greater than or equal to TimeWindowStartMs.");
            }

            if (string.IsNullOrWhiteSpace(value.MetricType))
            {
                problems.Add("MetricAggregation.MetricType must be a non-empty, non-whitespace string.");
            }

            if (value.MetricType.Length > 50)
            {
                problems.Add("MetricAggregation.MetricType must be 50 characters or less.");
            }

            if (value.TotalItemsProcessed < 0)
            {
                problems.Add("MetricAggregation.TotalItemsProcessed must be a non-negative value.");
            }

            if (value.TotalItemsFailed < 0)
            {
                problems.Add("MetricAggregation.TotalItemsFailed must be a non-negative value.");
            }

            if (value.TotalItemsSkipped < 0)
            {
                problems.Add("MetricAggregation.TotalItemsSkipped must be a non-negative value.");
            }

            if (value.AverageProcessingTimeMs < 0)
            {
                problems.Add("MetricAggregation.AverageProcessingTimeMs must be a non-negative value.");
            }

            if (value.MinProcessingTimeMs < 0)
            {
                problems.Add("MetricAggregation.MinProcessingTimeMs must be a non-negative value.");
            }

            if (value.MaxProcessingTimeMs < 0)
            {
                problems.Add("MetricAggregation.MaxProcessingTimeMs must be a non-negative value.");
            }

            if (value.P95ProcessingTimeMs < 0)
            {
                problems.Add("MetricAggregation.P95ProcessingTimeMs must be a non-negative value.");
            }

            if (value.P99ProcessingTimeMs < 0)
            {
                problems.Add("MetricAggregation.P99ProcessingTimeMs must be a non-negative value.");
            }

            if (value.BackpressureEvents < 0)
            {
                problems.Add("MetricAggregation.BackpressureEvents must be a non-negative value.");
            }

            if (value.TotalBackpressureMs < 0)
            {
                problems.Add("MetricAggregation.TotalBackpressureMs must be a non-negative value.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="MetricAggregation"/> instance is valid.
        /// </summary>
        /// <param name="value">The metric aggregation to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this MetricAggregation value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="MetricAggregation"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="value">The metric aggregation to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of problems.</exception>
        public static void EnsureValid(this MetricAggregation value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();

            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"MetricAggregation is invalid. Problems:\n  - {
                    string.Join("\n  - ", problems)
                    }");
            }
        }
    }
}
