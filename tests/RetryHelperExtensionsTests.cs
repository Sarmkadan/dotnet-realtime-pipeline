using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.UnitTesting;

namespace dotnet_realtime_pipeline
{
    [TestClass]
    public class RetryHelperExtensionsTests
    {
        [TestMethod]
        public async Task TestRetryWithStatisticsAsync_Happy_PATH()
        {
            // Arrange
            var retryHelperExtensions = new RetryHelperExtensions();
            // Act
            var result = await retryHelperExtensions.RetryWithStatisticsAsync<string>("test");
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.statistics);
        }

        [TestMethod]
        public async Task TestRetryWithStatisticsAsync_Null_Input()
        {
            // Arrange
            var retryHelperExtensions = new RetryHelperExtensions();
            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await retryHelperExtensions.RetryWithStatisticsAsync<string>(null);
            });
        }

        [TestMethod]
        public async Task TestGetRetryEvents_HAPPY_PATH()
        {
            // Arrange
            var retryHelperExtensions = new RetryHelperExtensions();
            // Act
            var retryEvents = retryHelperExtensions.GetRetryEvents();
            // Assert
            Assert.IsNotNull(retryEvents);
        }

        [TestMethod]
        public async Task TestGetRetryEvents_Empty_Collection()
        {
            // Arrange
            var retryHelperExtensions = new RetryHelperExtensions();
            // Act and Assert
            var retryEvents = retryHelperExtensions.GetRetryEvents();
            Assert.IsNotNull(retryEvents);
            Assert.IsFalse(retryEvents.Any());
        }
    }
}