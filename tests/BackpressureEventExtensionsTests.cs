using System;
using DotNetRealtimePipeline.Metrics;
using Xunit;

namespace DotNetRealtimePipeline.Tests
{
    public class BackpressureEventExtensionsTests
    {
        [Fact]
        public void IsCritical_HappyPath_ReturnsTrue()
        {
            // Arrange
            var @event = new BackpressureEvent { BufferFillPercent = 80.0 };

            // Act
            var result = @event.IsCritical();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsCritical_NullEvent_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((BackpressureEvent?)null).IsCritical());
        }

        [Fact]
        public void GetSeverityLevel_HappyPath_ReturnsSeverity()
        {
            // Arrange
            var @event = new BackpressureEvent { BufferFillPercent = 50.0 };

            // Act
            var result = @event.GetSeverityLevel();

            // Assert
            Assert.Equal(BackpressureSeverity.Medium, result);
        }

        [Fact]
        public void ToFormattedString_HappyPath_ReturnsString()
        {
            // Arrange
            var @event = new BackpressureEvent { Timestamp = DateTime.Now, StageName = "Test", BufferFillPercent = 50.0, IsActivation = true, DroppedItems = 10 };

            // Act
            var result = @event.ToFormattedString();

            // Assert
            Assert.NotNull(result);
            Assert.Contains("BackpressureEvent", result);
        }

        [Fact]
        public void IsNewActivation_HappyPath_ReturnsTrue()
        {
            // Arrange
            var @event = new BackpressureEvent { IsActivation = true };

            // Act
            var result = @event.IsNewActivation();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsRelease_HappyPath_ReturnsTrue()
        {
            // Arrange
            var @event = new BackpressureEvent { IsActivation = false };

            // Act
            var result = @event.IsRelease();

            // Assert
            Assert.True(result);
        }
    }
}
