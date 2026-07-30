using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetRealtimePipeline.Utilities;
using DotNetRealtimePipeline.Tests;

namespace DotNetRealtimePipeline.Tests
{
    [TestClass]
    public class BatchProcessorValidationTests
    {
        [TestMethod]
        public void Validate_Happy_PATH_TotalBatches_ProcessedBatches_TotalItems_StartTime_LastUpdateTime()
        {
            // Arrange
            var progress = new BatchProcessingProgress
            (
                TotalBatches: 100,
                ProcessedBatches: 50,
                TotalItems: 500,
                ProcessedItems: 250,
                StartTime: new DateTime(2022, 1, 1),
                LastUpdateTime: new DateTime(2022, 1, 1, 0, 0, 1)
            );

            // Act
            var result = BatchProcessorValidation.Validate(progress);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Validate_TotalBatches_Is_Zero_TotalItems_Is_Zero_StartTime_LastUpdateTime_Are_Default()
        {
            // Arrange
            var progress = new BatchProcessingProgress
            (
                TotalBatches: 0,
                ProcessedBatches: 0,
                TotalItems: 0,
                ProcessedItems: 0,
                StartTime: default,
                LastUpdateTime: default
            );

            // Act
            var result = BatchProcessorValidation.Validate(progress);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void Validate_TotalBatches_Is_Negative_TotalItems_Is_Negative_StartTime_LastUpdateTime_Are_Default()
        {
            // Arrange
            var progress = new BatchProcessingProgress
            (
                TotalBatches: -100,
                ProcessedBatches: 0,
                TotalItems: -500,
                ProcessedItems: 0,
                StartTime: default,
                LastUpdateTime: default
            );

            // Act
            var result = BatchProcessorValidation.Validate(progress);

            // Assert
            Assert.AreEqual(4, result.Count);
        }

        [TestMethod]
        public void Validate_TotalBatches_Exceeds_ProcessedBatches_TotalItems_Exceeds_ProcessedItems_StartTime_LastUpdateTime_Are_Default()
        {
            // Arrange
            var progress = new BatchProcessingProgress
            (
                TotalBatches: 100,
                ProcessedBatches: 50,
                TotalItems: 500,
                ProcessedItems: 250,
                StartTime: default,
                LastUpdateTime: default
            );

            // Act
            var result = BatchProcessorValidation.Validate(progress);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void Validate_Null_Progress_Throws_ArgumentNullException()
        {
            // Act and Assert
            Assert.ThrowsException<ArgumentNullException>>(() => BatchProcessorValidation.Validate(null));
        }
    }
}