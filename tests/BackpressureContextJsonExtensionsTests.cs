using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace DotNetRealtimePipeline.Domain.Tests
{
    public class BackpressureContextJsonExtensionsTests
    {
        [Test]
        public void ToJson_Happy_PATH_PASSES()
        {
            // Arrange
            var backpressureContext = new BackpressureContext();
            backpressureContext.Id = 1;
            backpressureContext.Name = "Test Backpressure Context";
            backpressureContext.Description = "This is a test backpressure context";
            backpressureContext.StartTime = DateTime.Now;
            backpressureContext.EndTime = DateTime.Now.AddHours(1);

            // Act
            var json = BackpressureContextJsonExtensions.ToJson(backpressureContext);

            // Assert
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Length > 0);
        }

        [Test]
        public void FromJson_HAPPY_PATH_PASSES()
        {
            // Arrange
            var json = "{\"id\": 1, \"name\": \"Test Backpressure Context\", \"description\": \"This is a test backpressure context\", \"startTime\": \"2022-01-01T12:00:00.000Z\", \"endTime\": \"2022-01-01T13:00:00.000Z\"}";

            // Act
            var backpressureContext = BackpressureContextJsonExtensions.FromJson(json);

            // Assert
            Assert.IsNotNull(backpressureContext);
            Assert.AreEqual(1, backpressureContext.Id);
            Assert.AreEqual("Test Backpressure Context", backpressureContext.Name);
            Assert.AreEqual("This is a test backpressure context", backpressureContext.Description);
            Assert.AreEqual(new DateTime(2022, 1, 1, 12, 0, 0), backpressureContext.StartTime);
            Assert.AreEqual(new DateTime(2022, 1, 1, 13, 0, 0), backpressureContext.EndTime);
        }

        [Test]
        public void TryFromJson_HAPPY_PATH_PASSES()
        {
            // Arrange
            var json = "{\"id\": 1, \"name\": \"Test Backpressure Context\", \"description\": \"This is a test backpressure context\", \"startTime\": \"2022-01-01T12:00:00.000Z\", \"endTime\": \"2022-01-01T13:00:00.000Z\"}";

            // Act
            var backpressureContext = BackpressureContextJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.IsTrue(backpressureContext);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("Test Backpressure Context", result.Name);
            Assert.AreEqual("This is a test backpressure context", result.Description);
            Assert.AreEqual(new DateTime(2022, 1, 1, 12, 0, 0), result.StartTime);
            Assert.AreEqual(new DateTime(2022, 1, 1, 13, 0, 0), result.EndTime);
        }
    }
}