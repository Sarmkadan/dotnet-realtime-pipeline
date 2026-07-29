using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetRealtimePipeline.Metrics;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Metrics
{
    public class BackpressureEventJsonExtensionsTests
    {
        [Fact]
        public void ToJson_Happy_PATH_SerialIZES_EVENT()
        {
            // Arrange
            var backpressureEvent = new BackpressureEvent();
            var expectedJson = "{}";

            // Act
            var json = BackpressureEventJsonExtensions.ToJson(backpressureEvent);

            // Assert
            Assert.Equal(expectedJson, json);
        }

        [Fact]
        public void FromJson_HAPPY_PATH_DESERIALIZES_EVENT()
        {
            // Arrange
            var json = "{}";
            var expectedEvent = new BackpressureEvent();

            // Act
            var @event = BackpressureEventJsonExtensions.FromJson(json);

            // Assert
            Assert.Equal(expectedEvent, @event);
        }

        [Fact]
        public void TryFromJson_HAPPY_PATH_DESERIALIZES_EVENT()
        {
            // Arrange
            var json = "{}";
            var expectedEvent = new BackpressureEvent();
            BackpressureEvent? @event = null;

            // Act
            var result = BackpressureEventJsonExtensions.TryFromJson(json, out @event);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedEvent, @event);
        }
    }
}