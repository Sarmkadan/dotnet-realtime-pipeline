using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using DotNetRealtimePipeline.Data.Repositories;

namespace DotNetRealtimePipeline.Tests.Data.Repositories
{
    [TestFixture]
    public class InMemoryMetricsRepositoryValidationTests
    {
        [Test]
        public void Validate_Happy_PATH_Passing_Valid_Repository_Returns_Empty_List()
        {
            // Arrange
            var repository = new InMemoryMetricsRepositoryValidation(new InMemoryMetricsRepository());
            var metrics = new List<Metric>();
            for (int i = 0; i < 10; i++)
            {
                metrics.Add(new Metric
                {
                    MetricId = i,
                    TimeWindowStartMs = i * 1000,
                    TimeWindowEndMs = (i + 1) * 1000
                });
            }
            repository.GetInternalMetrics = () => metrics;

            // Act
            var problems = repository.Validate();

            // Assert
            Assert.IsEmpty(problems);
        }

        [Test]
        public void Validate_HAPPY_PATH_Passing_Valid_Repository_With_Null_Metric_Returns_Problem_List_Containing_Null_Metric()
        {
            // Arrange
            var repository = new InMemoryMetricsRepositoryValidation(new InMemoryMetricsRepository());
            var metrics = new List<Metric>();
            metrics.Add(null);
            repository.GetInternalMetrics = () => metrics;

            // Act
            var problems = repository.Validate();

            // Assert
            Assert.AreEqual(1, problems.Count);
            Assert.IsTrue(problems.Contains("Internal metrics list contains 1 null entries."));
        }

        [Test]
        public void Validate_HAPPY_PATH_Passing_Valid_Repository_With_Duplicate_MetricIds_Returns_Problem_List_Containing_Duplicate_MetricIds()
        {
            // Arrange
            var repository = new InMemoryMetricsRepositoryValidation(new InMemoryMetricsRepository());
            var metrics = new List<Metric>();
            for (int i = 0; i < 10; i++)
            {
                metrics.Add(new Metric
                {
                    MetricId = i,
                    TimeWindowStartMs = i * 1000,
                    TimeWindowEndMs = (i + 1) * 1000
                });
            }
            metrics.Add(new Metric { MetricId = 5, TimeWindowStartMs = 5000, TimeWindowEndMs = 6000 });
            repository.GetInternalMetrics = () => metrics;

            // Act
            var problems = repository.Validate();

            // Assert
            Assert.AreEqual(1, problems.Count);
            Assert.IsTrue(problems.Contains("Internal metrics list contains duplicate MetricIds: 5"));
        }

        [Test]
        public void Validate_HAPPY_PATH_Passsing_Valid_Repository_With_Invalid_Time_Windows_Returns_Problem_List_Containing_Invalid_Time_Windows()
        {
            // Arrange
            var repository = new InMemoryMetricsRepositoryValidation(new InMemoryMetricsRepository());
            var metrics = new List<Metric>();
            metrics.Add(new Metric { MetricId = 1, TimeWindowStartMs = 1000, TimeWindowEndMs = 500 });
            repository.GetInternalMetrics = () => metrics;

            // Act
            var problems = repository.Validate();

            // Assert
            Assert.AreEqual(1, problems.Count);
            Assert.IsTrue(problems.Contains("Internal metrics list contains 1 metrics with invalid time windows."));
        }

        [Test]
        public void IsValid_HAPPY_PATH_Passing_Valid_Repository_Returns_True()
        {
            // Arrange
            var repository = new InMemoryMetricsRepositoryValidation(new InMemoryMetricsRepository());
            var metrics = new List<Metric>();
            for (int i = 0; i < 10; i++)
            {
                metrics.Add(new Metric
                {
                    MetricId = i,
                    TimeWindowStartMs = i * 1000,
                    TimeWindowEndMs = (i + 1) * 1000
                });
            }
            repository.GetInternalMetrics = () => metrics;

            // Act
            var isValid = repository.IsValid();

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsValid_HAPPY_PATH_Passing_Invalid_Repository_Returns_False()
        {
            // Arrange
            var repository = new InMemoryMetricsRepositoryValidation(new InMemoryMetricsRepository());
            var metrics = new List<Metric>();
            metrics.Add(null);
            repository.GetInternalMetrics = () => metrics;

            // Act
            var isValid = repository.IsValid();

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void EnsureValid_HAPPY_PATH_Passing_Valid_Repository_Does_Nothing()
        {
            // Arrange
            var repository = new InMemoryMetricsRepositoryValidation(new InMemoryMetricsRepository());
            var metrics = new List<Metric>();
            for (int i = 0; i < 10; i++)
            {
                metrics.Add(new Metric
                {
                    MetricId = i,
                    TimeWindowStartMs = i * 1000,
                    TimeWindowEndMs = (i + 1) * 1000
                });
            }
            repository.GetInternalMetrics = () => metrics;

            // Act
            repository.EnsureValid();
        }

        [Test]
        public void EnsureValid_HAPPY_PATH_Passing_Invalid_Repository_Throws_Exception()
        {
            // Arrange
            var repository = new InMemoryMetricsRepositoryValidation(new InMemoryMetricsRepository());
            var metrics = new List<Metric>();
            metrics.Add(null);
            repository.GetInternalMetrics = () => metrics;

            // Act and Assert
            Assert.Throws<ArgumentException>(() => repository.EnsureValid());
        }
    }
}