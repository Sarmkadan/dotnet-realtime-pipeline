using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetRealtimePipeline.Utilities;

namespace DotNetRealtimePipeline.Tests
{
    [TestClass]
    public class ValidationHelperValidationTests
    {
        [TestMethod]
        public void Validate_HappyPath_DataPoints()
        {
            // Arrange
            var validationHelper = new ValidationHelperValidation();
            var dataPoints = new List<DataPoint> { new DataPoint() };

            // Act
            var result = validationHelper.Validate(dataPoints);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_HappyPath_PipelineConfig()
        {
            // Arrange
            var validationHelper = new ValidationHelperValidation();
            var pipelineConfig = new PipelineConfig();

            // Act
            var result = validationHelper.Validate(pipelineConfig);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_HappyPath_ProcessingResults()
        {
            // Arrange
            var validationHelper = new ValidationHelperValidation();
            var processingResults = new List<ProcessingResult> { new ProcessingResult() };

            // Act
            var result = validationHelper.Validate(processingResults);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_HappyPath_WindowEvent()
        {
            // Arrange
            var validationHelper = new ValidationHelperValidation();
            var windowEvent = new WindowEvent();

            // Act
            var result = validationHelper.Validate(windowEvent);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_Null_DataPoints_ThrowsArgumentNullException()
        {
            // Arrange
            var validationHelper = new ValidationHelperValidation();
            var dataPoints = null as List<DataPoint>;

            // Act and Assert
            Assert.ThrowsException<ArgumentNullException>(() => validationHelper.Validate(dataPoints));
        }

        [TestMethod]
        public void Validate_Empty_DataPoints_ReturnsInvalidResult()
        {
            // Arrange
            var validationHelper = new ValidationHelperValidation();
            var dataPoints = new List<DataPoint>();

            // Act
            var result = validationHelper.Validate(dataPoints);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void IsInTimeRange_HappyPath_ReturnsTrue()
        {
            // Arrange
            var validationHelper = new ValidationHelperValidation();
            var dataPoint = new DataPoint();
            var startMs = 100;
            var endMs = 200;

            // Act
            var result = validationHelper.IsInTimeRange(dataPoint, startMs, endMs);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsInTimeRange_Null_DataPoint_ThrowsArgumentNullException()
        {
            // Arrange
            var validationHelper = new ValidationHelperValidation();
            var dataPoint = null as DataPoint;
            var startMs = 100;
            var endMs = 200;

            // Act and Assert
            Assert.ThrowsException<ArgumentNullException>(() => validationHelper.IsInTimeRange(dataPoint, startMs, endMs));
        }

        [TestMethod]
        public void IsWithinBounds_HappyPath_ReturnsTrue()
        {
            // Arrange
            var validationHelper = new ValidationHelperValidation();
            var inputValue = 100;
            var minValue = 0;
            var maxValue = 200;

            // Act
            var result = validationHelper.IsWithinBounds(inputValue, minValue, maxValue);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsWithinBounds_Null_InputValue_ThrowsArgumentNullException()
        {
            // Arrange
            var validationHelper = new ValidationHelperValidation();
            var inputValue = null as double?;
            var minValue = 0;
            var maxValue = 200;

            // Act and Assert
            Assert.ThrowsException<ArgumentNullException>(() => validationHelper.IsWithinBounds(inputValue, minValue, maxValue));
        }

        [TestMethod]
        public void Validate_ReturnsEmptyList()
        {
            // Arrange
            var validationHelper = new ValidationHelperValidation();

            // Act
            var result = validationHelper.Validate(validationHelper);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void IsValid_ReturnsTrue()
        {
            // Arrange
            var validationHelper = new ValidationHelperValidation();

            // Act
            var result = validationHelper.IsValid();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EnsureValid_DoesNothing()
        {
            // Arrange
            var validationHelper = new ValidationHelperValidation();

            // Act
            validationHelper.EnsureValid();

            // Assert
            // No assertions needed, as this method does nothing
        }
    }
}
