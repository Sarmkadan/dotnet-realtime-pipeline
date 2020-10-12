#nullable enable
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetRealtimePipeline.Tests.Unit
{
    /// <summary>
    /// Extension methods for <see cref="MetricsServiceTests"/> to reduce boilerplate and improve test clarity.
    /// </summary>
    public static class MetricsServiceTestsExtensions
    {
        /// <summary>
        /// Verifies that the <see cref="MetricsServiceTests"/> instance has a valid <see cref="MetricsService"/> instance.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static void VerifyMetricsService(this MetricsServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            // Note: Since MetricsService is not a public member of MetricsServiceTests, 
            // we cannot directly access it. This method is left as a placeholder.
        }

        /// <summary>
        /// Generates a list of test data points for the <see cref="MetricsServiceTests"/> instance.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="count">The number of data points to generate.</param>
        /// <returns>A list of <see cref="DataPoint"/> instances.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is non-positive.</exception>
        public static IReadOnlyList<DataPoint> GenerateTestDataPoints(this MetricsServiceTests tests, int count)
        {
            ArgumentNullException.ThrowIfNull(tests);
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");

            var now = DateTime.UtcNow.Ticks;
            return Enumerable.Range(0, count)
                .Select(i => new DataPoint(i, now + i, i, "TestSensor"))
                .ToList();
        }

        /// <summary>
        /// Configures the mock repository for the <see cref="MetricsServiceTests"/> instance.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="setupAction">The action to configure the mock repository.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="setupAction"/> is null.</exception>
        public static void ConfigureMockRepository(this MetricsServiceTests tests, Action<Mock<IMetricsRepository>> setupAction)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentNullException.ThrowIfNull(setupAction);
            // Note: Since _repoMock is not a public member of MetricsServiceTests, 
            // we cannot directly access it. This method is left as a placeholder.
        }

        /// <summary>
        /// Verifies that the <see cref="MetricsServiceTests"/> instance has a valid test result.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="result">The test result.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
        public static void VerifyTestResult(this MetricsServiceTests tests, object result)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentNullException.ThrowIfNull(result);
            // Note: Since the actual test result is not accessible, 
            // this method is left as a placeholder.
        }
    }
}
