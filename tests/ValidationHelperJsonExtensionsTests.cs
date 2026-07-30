using Xunit;
using System;
using System.Text.Json;
using DotNetRealtimePipeline.Utilities;

namespace DotNetRealtimePipeline.Tests.ValidationHelperJsonExtensionsTests
{
    public class ValidationHelperJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var validationHelper = new ValidationHelper();
            var expectedJson = "{\"key\":\"value\"}";

            // Act
            var actualJson = validationHelper.ToJson();

            // Assert
            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationHelper().ToJson(null));
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsValidationHelper()
        {
            // Arrange
            var json = "{\"key\":\"value\"}";
            var expectedValidationHelper = new ValidationHelper();

            // Act
            var actualValidationHelper = ValidationHelperJsonExtensions.FromJson(json);

            // Assert
            Assert.Equal(expectedValidationHelper, actualValidationHelper);
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ValidationHelperJsonExtensions.FromJson(null));
        }

        [Fact]
        public void FromJson_EmptyJson_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => ValidationHelperJsonExtensions.FromJson(""));
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"key\":\"value\"}";
            var expectedValidationHelper = new ValidationHelper();

            // Act
            var result = ValidationHelperJsonExtensions.TryFromJson(json, out var actualValidationHelper);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValidationHelper, actualValidationHelper);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsFalse()
        {
            // Act
            var result = ValidationHelperJsonExtensions.TryFromJson(null, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Act
            var result = ValidationHelperJsonExtensions.TryFromJson("", out _);

            // Assert
            Assert.False(result);
        }
    }
}
