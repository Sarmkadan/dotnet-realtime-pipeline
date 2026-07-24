using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Domain.Models
{
    public class WindowEventValidationTests
    {
        private WindowEvent CreateValidWindowEvent()
        {
            return new WindowEvent
            {
                WindowId = 1,
                WindowStartMs = 1000,
                WindowEndMs = 2000,
                AggregationType = "Sum",
                CreatedAt = DateTime.UtcNow,
                CreatedAtTicks = DateTime.UtcNow.Ticks,
                DataPoints = new List<DataPoint>
                {
                    new DataPoint { Id = 1, Timestamp = 1500, Quality = 90 },
                    new DataPoint { Id = 2, Timestamp = 1600, Quality = 95 }
                },
                Description = "Test window",
                IsComplete = true
            };
        }

        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            var window = CreateValidWindowEvent();
            var errors = window.Validate();
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_NullWindow_ThrowsArgumentNullException()
        {
            WindowEvent? window = null;
            Assert.Throws<ArgumentNullException>(() => window.Validate());
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            var window = CreateValidWindowEvent();
            Assert.True(window.IsValid());
        }

        [Fact]
        public void IsValid_WithInvalidWindow_ReturnsFalse()
        {
            var window = CreateValidWindowEvent();
            window.WindowId = 0; // invalid
            Assert.False(window.IsValid());
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            var window = CreateValidWindowEvent();
            var ex = Record.Exception(() => window.EnsureValid());
            Assert.Null(ex);
        }

        [Fact]
        public void EnsureValid_WithInvalidWindow_ThrowsArgumentException()
        {
            var window = CreateValidWindowEvent();
            window.WindowStartMs = -10;
            var ex = Assert.Throws<ArgumentException>(() => window.EnsureValid());
            Assert.Contains("WindowStartMs must be positive", ex.Message);
        }

        [Fact]
        public void IsDurationValid_HappyPath_ReturnsTrue()
        {
            var window = CreateValidWindowEvent();
            Assert.True(window.IsDurationValid());
        }

        [Fact]
        public void IsDurationValid_BoundaryValues_ReturnsTrue()
        {
            var window = CreateValidWindowEvent();
            window.WindowStartMs = 0;
            window.WindowEndMs = 86400000; // 24h
            Assert.True(window.IsDurationValid());
        }

        [Fact]
        public void IsDurationValid_InvalidRange_ThrowsArgumentException()
        {
            var window = CreateValidWindowEvent();
            Assert.Throws<ArgumentException>(() => window.IsDurationValid(maxDurationMs: 1000, minDurationMs: 2000));
        }

        [Fact]
        public void HasSufficientDataPoints_HappyPath_ReturnsTrue()
        {
            var window = CreateValidWindowEvent();
            Assert.True(window.HasSufficientDataPoints(minDataPoints: 2));
        }

        [Fact]
        public void HasSufficientDataPoints_Insufficient_ReturnsFalse()
        {
            var window = CreateValidWindowEvent();
            Assert.False(window.HasSufficientDataPoints(minDataPoints: 5));
        }

        [Fact]
        public void HasSufficientDataPoints_NegativeMin_ThrowsArgumentOutOfRangeException()
        {
            var window = CreateValidWindowEvent();
            Assert.Throws<ArgumentOutOfRangeException>(() => window.HasSufficientDataPoints(minDataPoints: -1));
        }

        [Fact]
        public void HasSupportedAggregationType_HappyPath_ReturnsTrue()
        {
            var window = CreateValidWindowEvent();
            var supported = new List<string> { "Sum", "Avg" };
            Assert.True(window.HasSupportedAggregationType(supported));
        }

        [Fact]
        public void HasSupportedAggregationType_Unsupported_ReturnsFalse()
        {
            var window = CreateValidWindowEvent();
            var supported = new List<string> { "Avg" };
            Assert.False(window.HasSupportedAggregationType(supported));
        }

        [Fact]
        public void HasSupportedAggregationType_NullSupported_ThrowsArgumentNullException()
        {
            var window = CreateValidWindowEvent();
            Assert.Throws<ArgumentNullException>(() => window.HasSupportedAggregationType(null!));
        }

        [Fact]
        public void HasQualityDataPoints_HappyPath_ReturnsTrue()
        {
            var window = CreateValidWindowEvent();
            Assert.True(window.HasQualityDataPoints(qualityThreshold: 80));
        }

        [Fact]
        public void HasQualityDataPoints_InsufficientQuality_ReturnsFalse()
        {
            var window = CreateValidWindowEvent();
            window.DataPoints[0].Quality = 50;
            Assert.False(window.HasQualityDataPoints(qualityThreshold: 80));
        }

        [Fact]
        public void HasQualityDataPoints_EmptyCollection_ReturnsFalse()
        {
            var window = CreateValidWindowEvent();
            window.DataPoints = new List<DataPoint>();
            Assert.False(window.HasQualityDataPoints());
        }

        [Fact]
        public void HasQualityDataPoints_ThresholdOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            var window = CreateValidWindowEvent();
            Assert.Throws<ArgumentOutOfRangeException>(() => window.HasQualityDataPoints(qualityThreshold: 150));
        }

        [Fact]
        public void IsCompleteAndValid_HappyPath_ReturnsTrue()
        {
            var window = CreateValidWindowEvent();
            Assert.True(window.IsCompleteAndValid());
        }

        [Fact]
        public void IsCompleteAndValid_Incomplete_ReturnsFalse()
        {
            var window = CreateValidWindowEvent();
            window.IsComplete = false;
            Assert.False(window.IsCompleteAndValid());
        }

        [Fact]
        public void HasReasonableTimestamps_HappyPath_ReturnsTrue()
        {
            var window = CreateValidWindowEvent();
            Assert.True(window.HasReasonableTimestamps());
        }

        [Fact]
        public void HasReasonableTimestamps_FutureTooFar_ReturnsFalse()
        {
            var window = CreateValidWindowEvent();
            window.WindowEndMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 2_000_000_000; // far future
            Assert.False(window.HasReasonableTimestamps());
        }

        [Fact]
        public void HasReasonableTimestamps_NegativeMaxFuture_ThrowsArgumentOutOfRangeException()
        {
            var window = CreateValidWindowEvent();
            Assert.Throws<ArgumentOutOfRangeException>(() => window.HasReasonableTimestamps(maxFutureMs: -100));
        }
    }
}
