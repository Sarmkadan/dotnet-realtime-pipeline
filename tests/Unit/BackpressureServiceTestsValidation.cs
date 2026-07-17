using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Exceptions;

namespace tests.Unit
{
    public static class BackpressureServiceTestsValidation
    {
        /// <summary>
        /// Validates the BackpressureService instance for common testability issues.
        /// </summary>
        /// <param name="value">The BackpressureServiceTests instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        public static IReadOnlyList<string> Validate(this BackpressureServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate that the service instance is properly initialized
            // and can be used in unit tests without throwing exceptions

            try
            {
                // Test basic service operations to ensure they work
                var testStage = "ValidationTestStage";
                var context = value.Service.CreateContext(testStage, 1000);

                if (context == null)
                {
                    problems.Add("CreateContext returned null context");
                }
                else if (context.StageName != testStage)
                {
                    problems.Add("CreateContext returned context with incorrect stage name");
                }
                else if (context.MaxCapacity != 1000)
                {
                    problems.Add("CreateContext returned context with incorrect max capacity");
                }

                // Test TryAddToBuffer
                bool addResult = value.Service.TryAddToBuffer(testStage, 500);
                if (!addResult)
                {
                    problems.Add("TryAddToBuffer failed for valid operation");
                }

                // Test GetBufferStatus
                var status = value.Service.GetBufferStatus();
                if (status == null)
                {
                    problems.Add("GetBufferStatus returned null");
                }
                else if (!status.ContainsKey(testStage))
                {
                    problems.Add("GetBufferStatus does not contain test stage");
                }
                else if (status[testStage] != 500)
                {
                    problems.Add("GetBufferStatus returned incorrect buffer size");
                }

                // Test RemoveFromBuffer
                value.Service.RemoveFromBuffer(testStage, 200);
                status = value.Service.GetBufferStatus();
                if (status[testStage] != 300)
                {
                    problems.Add("RemoveFromBuffer did not decrease buffer size correctly");
                }

                // Test IsBackpressured (should be false initially)
                bool isBackpressured = value.Service.IsBackpressured(testStage);
                if (isBackpressured)
                {
                    problems.Add("IsBackpressured returned true for non-backpressured stage");
                }

                // Test GetContext
                var retrievedContext = value.Service.GetContext(testStage);
                if (retrievedContext == null)
                {
                    problems.Add("GetContext returned null for existing stage");
                }
                else if (retrievedContext != context)
                {
                    problems.Add("GetContext returned different context instance");
                }

                // Test ApplyBackpressureAsync
                var backpressureTask = value.Service.ApplyBackpressureAsync(testStage, BackpressureStrategy.Throttle, 100);
                if (backpressureTask == null)
                {
                    problems.Add("ApplyBackpressureAsync returned null task");
                }

                // Test SystemStatus
                var systemStatus = value.Service.GetSystemStatus();
                if (systemStatus == null)
                {
                    problems.Add("GetSystemStatus returned null");
                }
                else
                {
                    if (systemStatus.TotalStages != 1)
                    {
                        problems.Add("GetSystemStatus reported incorrect total stages count");
                    }
                    if (systemStatus.BackpressuredStages != 0)
                    {
                        problems.Add("GetSystemStatus reported backpressured stages when none should be");
                    }
                }

                // Clean up test context
                value.Service.ResetBackpressure(testStage);
                value.Service.Clear();
            }
            catch (Exception ex) when (ex is not ArgumentNullException and not ArgumentException)
            {
                problems.Add($"Service operations threw exception: {ex.GetType().Name}: {ex.Message}");
            }

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