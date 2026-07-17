#nullable enable

using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRealtimePipeline.Tests.Unit
{
    /// <summary>
    /// Extension methods for <see cref="MetricsServiceTests"/> to reduce boilerplate and improve test clarity.
    /// </summary>
    public static class MetricsServiceTestsExtensions
    {
        /// <summary>
        /// Gets the <see cref="MetricsService"/> instance from the test class.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>The <see cref="MetricsService"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static MetricsService GetService(this MetricsServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var serviceField = typeof(MetricsServiceTests).GetField("_service", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (MetricsService)serviceField!.GetValue(tests)!;
        }

        /// <summary>
        /// Gets the mock repository from the test class.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>The <see cref="Mock{IMetricsRepository}"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static Mock<IMetricsRepository> GetMockRepository(this MetricsServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var repoField = typeof(MetricsServiceTests).GetField("_repoMock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Mock<IMetricsRepository>)repoField!.GetValue(tests)!;
        }

        /// <summary>
        /// Verifies that the <see cref="MetricsServiceTests"/> instance has a valid <see cref="MetricsService"/> instance.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static void VerifyMetricsService(this MetricsServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            tests.GetService().Should().NotBeNull("MetricsService should be initialized in test constructor");
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

            var mockRepo = tests.GetMockRepository();
            setupAction(mockRepo);
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

            result.Should().NotBeNull("Test result should not be null");
        }

        /// <summary>
        /// Verifies that the mock repository was called with the specified parameters.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="predicate">Predicate to match the expected call.</param>
        /// <param name="times">The expected number of times the method should have been called.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="predicate"/> is null.</exception>
        public static void VerifyRepositoryCall(this MetricsServiceTests tests, Func<MetricAggregation, bool> predicate, Times times)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentNullException.ThrowIfNull(predicate);

            var mockRepo = tests.GetMockRepository();
            mockRepo.Verify(r => r.SaveAsync(It.Is<MetricAggregation>(x => predicate(x))), times);
        }

        /// <summary>
        /// Sets up the mock repository to return a specific metric aggregation.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="metric">The metric aggregation to return.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="metric"/> is null.</exception>
        public static void SetupRepositoryToReturn(this MetricsServiceTests tests, MetricAggregation metric)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentNullException.ThrowIfNull(metric);

            var mockRepo = tests.GetMockRepository();
            mockRepo.Setup(r => r.SaveAsync(metric)).ReturnsAsync(metric);
            mockRepo.Setup(r => r.GetLatestAsync()).ReturnsAsync(metric);
        }
    }
}