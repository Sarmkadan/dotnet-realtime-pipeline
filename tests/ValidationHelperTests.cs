using System;
using System.Collections.Generic;
using System.Linq;
using DotNetRealtimePipeline.Utilities;
using Xunit;

namespace DotNetRealtimePipeline.Tests.ValidationHelperTests
{
    public class ValidationHelperTests
    {
        [Fact]
        public void ValidateDataPoints_HappyPath_ReturnsValidResult()
        {
            var dataPoints = new List<DataPoint>
            {
                new DataPoint { Id = 1, Timestamp = 100 },
                new DataPoint { Id = 2, Timestamp = 200 },
                new DataPoint { Id = 3, Timestamp = 300 }
            };

            var result = ValidationHelper.ValidateDataPoints(dataPoints);
            Assert.True(result.IsValid);
            Assert.Empty(result.InvalidIndices);
        }

        [Fact]
        public void ValidateDataPoints_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ValidationHelper.ValidateDataPoints(null));
        }

        [Fact]
        public void ValidateDataPoints_EmptyCollection_ReturnsInvalidResult()
        {
            var dataPoints = new List<DataPoint>();
            var result = ValidationHelper.ValidateDataPoints(dataPoints);
            Assert.False(result.IsValid);
            Assert.Empty(result.InvalidIndices);
        }

        [Fact]
        public void ValidatePipelineConfig_HappyPath_ReturnsValidResult()
        {
            var config = new PipelineConfig { Stages = new List<Stage> { new Stage { Name = "Stage1" } } };
            var result = ValidationHelper.ValidatePipelineConfig(config);
            Assert.True(result.IsValid);
            Assert.Empty(result.ErrorMessage);
        }

        [Fact]
        public void ValidatePipelineConfig_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ValidationHelper.ValidatePipelineConfig(null));
        }

        [Fact]
        public void ValidateProcessingResults_HappyPath_ReturnsValidResult()
        {
            var results = new List<ProcessingResult>
            {
                new ProcessingResult { ResultId = 1, IsValid = true },
                new ProcessingResult { ResultId = 2, IsValid = true },
                new ProcessingResult { ResultId = 3, IsValid = true }
            };

            var result = ValidationHelper.ValidateProcessingResults(results);
            Assert.True(result.IsValid);
            Assert.Empty(result.InvalidIndices);
        }

        [Fact]
        public void ValidateProcessingResults_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ValidationHelper.ValidateProcessingResults(null));
        }

        [Fact]
        public void ValidateWindowEvent_HappyPath_ReturnsValidResult()
        {
            var window = new WindowEvent { WindowStartMs = 100, WindowEndMs = 200, DataPoints = new List<DataPoint> { new DataPoint { Id = 1, Timestamp = 150 } } };
            var result = ValidationHelper.ValidateWindowEvent(window);
            Assert.True(result.IsValid);
            Assert.Empty(result.ErrorMessage);
        }

        [Fact]
        public void ValidateWindowEvent_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ValidationHelper.ValidateWindowEvent(null));
        }

        [Fact]
        public void IsInTimeRange_HappyPath_ReturnsTrue()
        {
            var dataPoint = new DataPoint { Timestamp = 150 };
            var result = ValidationHelper.IsInTimeRange(dataPoint, 100, 200);
            Assert.True(result);
        }

        [Fact]
        public void IsInTimeRange_OutOfRange_ReturnsFalse()
        {
            var dataPoint = new DataPoint { Timestamp = 250 };
            var result = ValidationHelper.IsInTimeRange(dataPoint, 100, 200);
            Assert.False(result);
        }

        [Fact]
        public void IsWithinBounds_HappyPath_ReturnsTrue()
        {
            var value = 150;
            var result = ValidationHelper.IsWithinBounds(value, 100, 200);
            Assert.True(result);
        }

        [Fact]
        public void IsWithinBounds_OutOfRange_ReturnsFalse()
        {
            var value = 250;
            var result = ValidationHelper.IsWithinBounds(value, 100, 200);
            Assert.False(result);
        }
    }
}
