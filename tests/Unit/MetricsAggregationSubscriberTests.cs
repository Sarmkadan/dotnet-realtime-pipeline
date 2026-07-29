using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.UnitTesting;

namespace dotnet_realtime_pipeline.tests.Unit
{
    [TestClass]
    public class MetricsAggregationSubscriberTests
    {
        [TestMethod]
        public async Task TestAggregationCorrectness()
        {
            // Arrange
            var subscriber = new MetricsAggregationSubscriber();
            var events = new List<MetricsEvent>();
            for (int i = 0; i < 3; i++)
            {
                events.Add(new MetricsEvent
                {
                    Metric = "ProcessingTime",
                    Value = 10
                });
            }
            // Act
            await subscriber.Aggregate(events);
            // Assert
            Assert.AreEqual(30, subscriber.GetAggregatedMetrics()["ProcessingTime"]);
        }

        [TestMethod]
        public async Task TestThreadSafety()
        {
            // Arrange
            var subscriber = new MetricsAggregationSubscriber();
            var events = new List<MetricsEvent>();
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    events.Add(new MetricsEvent
                    {
                        Metric = "ProcessingTime",
                        Value = 10
                    });
                }));
            }
            // Act
            await Task.WhenAll(tasks);
            await subscriber.Aggregate(events);
            // Assert
            Assert.AreEqual(100, subscriber.GetAggregatedMetrics()["ProcessingTime"]);
        }

        [TestMethod]
        public async Task TestFirstEvent()
        {
            // Arrange
            var subscriber = new MetricsAggregationSubscriber();
            var events = new List<MetricsEvent>();
            events.Add(new MetricsEvent
            {
                Metric = "ProcessingTime",
                Value = 10
            });
            // Act
            await subscriber.Aggregate(events);
            // Assert
            Assert.AreEqual(10, subscriber.GetAggregatedMetrics()["ProcessingTime"]);
        }

        [TestMethod]
        public async Task TestReset()
        {
            // Arrange
            var subscriber = new MetricsAggregationSubscriber();
            var events = new List<MetricsEvent>();
            events.Add(new MetricsEvent
            {
                Metric = "ProcessingTime",
                Value = 10
            });
            await subscriber.Aggregate(events);
            // Act
            subscriber.Reset();
            // Assert
            Assert.AreEqual(0, subscriber.GetAggregatedMetrics()["ProcessingTime"]);
        }
    }
}