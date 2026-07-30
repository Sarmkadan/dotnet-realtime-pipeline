using System;
using DotNetRealtimePipeline.Utilities;
using Xunit;

namespace DotNetRealtimePipeline.Tests
{
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void ToUnixMilliseconds_HappyPath_ReturnsExpectedValue()
        {
            // 2023-01-01T00:00:00Z => Unix ms = 1672531200000
            var date = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long unixMs = date.ToUnixMilliseconds();

            Assert.Equal(1672531200000L, unixMs);
        }

        [Fact]
        public void ToUnixMilliseconds_PreUnixDate_ThrowsArgumentOutOfRangeException()
        {
            var preUnix = new DateTime(1960, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.Throws<ArgumentOutOfRangeException>(() => preUnix.ToUnixMilliseconds());
        }

        [Fact]
        public void FromUnixMilliseconds_HappyPath_ReturnsExpectedDate()
        {
            long unixMs = 1672531200000L; // 2023-01-01T00:00:00Z
            DateTime dt = DateTimeExtensions.FromUnixMilliseconds(unixMs);
            Assert.Equal(DateTimeKind.Utc, dt.Kind);
            Assert.Equal(2023, dt.Year);
            Assert.Equal(1, dt.Month);
            Assert.Equal(1, dt.Day);
            Assert.Equal(0, dt.Hour);
            Assert.Equal(0, dt.Minute);
            Assert.Equal(0, dt.Second);
        }

        [Fact]
        public void FromUnixMilliseconds_NegativeValue_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeExtensions.FromUnixMilliseconds(-1));
        }

        [Fact]
        public void GetCurrentUnixMilliseconds_ReturnsValueCloseToNow()
        {
            long before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long value = DateTimeExtensions.GetCurrentUnixMilliseconds();
            long after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // The returned value should be between the two captured timestamps
            Assert.InRange(value, before, after);
        }

        [Fact]
        public void GetWindowStart_HappyPath_ReturnsCorrectStart()
        {
            long timestamp = 12345; // ms
            long windowSize = 1000; // 1 second windows
            long expectedStart = (timestamp / windowSize) * windowSize;
            long actualStart = DateTimeExtensions.GetWindowStart(timestamp, windowSize);
            Assert.Equal(expectedStart, actualStart);
        }

        [Fact]
        public void GetWindowStart_NonPositiveWindowSize_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeExtensions.GetWindowStart(1000, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeExtensions.GetWindowStart(1000, -5));
        }

        [Fact]
        public void GetWindowEnd_HappyPath_ReturnsCorrectEnd()
        {
            long timestamp = 12345;
            long windowSize = 1000;
            long expectedEnd = ((timestamp / windowSize) * windowSize) + windowSize;
            long actualEnd = DateTimeExtensions.GetWindowEnd(timestamp, windowSize);
            Assert.Equal(expectedEnd, actualEnd);
        }

        [Fact]
        public void GetWindowEnd_NonPositiveWindowSize_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeExtensions.GetWindowEnd(1000, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeExtensions.GetWindowEnd(1000, -10));
        }

        [Fact]
        public void GetAgeMs_HappyPath_ReturnsPositiveAge()
        {
            long now = DateTimeExtensions.GetCurrentUnixMilliseconds();
            // Simulate a timestamp 5 seconds ago
            long past = now - 5_000;
            long age = DateTimeExtensions.GetAgeMs(past);
            Assert.InRange(age, 4_900, 5_100); // allow small timing variance
        }

        [Fact]
        public void RoundToWindowBoundary_HappyPath_RoundsDown()
        {
            long timestamp = 12345;
            long windowSize = 1000;
            // remainder = 345, which is less than 500 => round down
            long expected = 12000;
            long actual = DateTimeExtensions.RoundToWindowBoundary(timestamp, windowSize);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RoundToWindowBoundary_HappyPath_RoundsUp()
        {
            long timestamp = 12700;
            long windowSize = 1000;
            // remainder = 700, which is >= 500 => round up
            long expected = 13000;
            long actual = DateTimeExtensions.RoundToWindowBoundary(timestamp, windowSize);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RoundToWindowBoundary_NonPositiveWindowSize_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeExtensions.RoundToWindowBoundary(1000, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeExtensions.RoundToWindowBoundary(1000, -1));
        }
    }
}
