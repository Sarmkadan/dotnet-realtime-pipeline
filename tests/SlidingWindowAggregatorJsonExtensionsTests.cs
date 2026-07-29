using System;
using System.Text.Json;
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Tests;
using NUnit.Framework;

namespace DotNetRealtimePipeline.Tests
{
    [TestFixture]
    public class SlidingWindowAggregatorJsonExtensionsTests
    {
        [Test]
        public void ToJson_HAPPY_PATH()
        {
            // Arrange
            var aggregator = new SlidingWindowAggregatorJsonExtensions();

            // Act
            var json = SlidingWindowAggregatorJsonExtensions.ToJson(aggregator);

            // Assert
            Assert.IsNotNull(json);
        }

        [Test]
        public void FromJson_HAPPY_PATH()
        {
            // Arrange
            var json = "{}";

            // Act
            var aggregator = SlidingWindowAggregatorJsonExtensions.FromJson(json);

            // Assert
            Assert.IsNotNull(aggregator);
        }

        [Test]
        public void TryFromJson_HAPPY_PATH()
        {
            // Arrange
            var json = "{}";
            SlidingWindowAggregator? aggregator;

            // Act
            var result = SlidingWindowAggregatorJsonExtensions.TryFromJson(json, out aggregator);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(aggregator);
        }
    }
}