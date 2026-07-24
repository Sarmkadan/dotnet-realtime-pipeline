using Xunit;
using DotNetRealtimePipeline.Domain.Models;

namespace DotNetRealtimePipeline.Tests.Domain.Models
{
    public class ProcessingResultValidationTests
    {
        private ProcessingResult CreateValidProcessingResult()
        {
            return new ProcessingResult(1, true, "Test Stage")
            {
                ProcessingTimeMs = 100,
                ProcessedAt = DateTime.UtcNow,
                RetryCount = 0,
                OutputData = new Dictionary<string, object> { { "Key1", "Value1" } }
            };
        }

        private ProcessingResult CreateValidFailedProcessingResult()
        {
            return new ProcessingResult(1, false, "Failed Stage")
            {
                ProcessingTimeMs = 100,
                ProcessedAt = DateTime.UtcNow,
                ErrorMessage = "Test error message",
                Exception = new InvalidOperationException("Test exception"),
                RetryCount = 2,
                OutputData = new Dictionary<string, object> { { "Key1", "Value1" } }
            };
        }

        [Fact]
        public void Validate_HappyPath_WithValidResult_ReturnsEmptyList()
        {
            // Arrange
            var result = CreateValidProcessingResult();

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_HappyPath_WithValidFailedResult_ReturnsEmptyList()
        {
            // Arrange
            var result = CreateValidFailedProcessingResult();

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            ProcessingResult? result = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => result!.Validate());
        }

        [Fact]
        public void Validate_InvalidResultId_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.ResultId = 0;

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("ResultId must be a positive integer", errors[0]);
        }

        [Fact]
        public void Validate_NegativeResultId_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.ResultId = -1;

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("ResultId must be a positive integer", errors[0]);
        }

        [Fact]
        public void Validate_NullStageName_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.StageName = null!;

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("StageName cannot be null, empty, or whitespace", errors[0]);
        }

        [Fact]
        public void Validate_EmptyStageName_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.StageName = "";

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("StageName cannot be null, empty, or whitespace", errors[0]);
        }

        [Fact]
        public void Validate_WhitespaceStageName_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.StageName = "   ";

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("StageName cannot be null, empty, or whitespace", errors[0]);
        }

        [Fact]
        public void Validate_LongStageName_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.StageName = new string('A', 257);

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("StageName cannot exceed 256 characters", errors[0]);
        }

        [Fact]
        public void Validate_NegativeProcessingTimeMs_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.ProcessingTimeMs = -1;

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("ProcessingTimeMs cannot be negative", errors[0]);
        }

        [Fact]
        public void Validate_DefaultProcessedAt_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.ProcessedAt = default;

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("ProcessedAt cannot be the default DateTime value", errors[0]);
        }

        [Fact]
        public void Validate_FutureProcessedAt_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.ProcessedAt = DateTime.UtcNow.AddMinutes(10);

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("ProcessedAt cannot be in the future", errors[0]);
        }

        [Fact]
        public void Validate_TooOldProcessedAt_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.ProcessedAt = DateTime.UtcNow.AddMinutes(-10);

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("ProcessedAt cannot be more than 5 minutes in the past", errors[0]);
        }

        [Fact]
        public void Validate_NegativeRetryCount_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.RetryCount = -1;

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("RetryCount cannot be negative", errors[0]);
        }

        [Fact]
        public void Validate_SuccessFalseWithNullErrorMessage_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.Success = false;
            result.ErrorMessage = null;

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("ErrorMessage must be provided when Success is false", errors[0]);
        }

        [Fact]
        public void Validate_SuccessFalseWithEmptyErrorMessage_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.Success = false;
            result.ErrorMessage = "";

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("ErrorMessage must be provided when Success is false", errors[0]);
        }

        [Fact]
        public void Validate_LongErrorMessage_ReturnsError()
        {
            // Arrange
            var result = CreateValidFailedProcessingResult();
            result.ErrorMessage = new string('E', 4097);

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("ErrorMessage cannot exceed 4096 characters", errors[0]);
        }

        [Fact]
        public void Validate_NullOutputData_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.OutputData = null!;

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("OutputData dictionary cannot be null", errors[0]);
        }

        [Fact]
        public void Validate_MissingErrorMessageWithException_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.Exception = new InvalidOperationException("Test exception");

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("ErrorMessage should be provided when Exception is set", errors[0]);
        }

        [Fact]
        public void Validate_MissingExceptionWithErrorMessage_ReturnsError()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.Success = false;
            result.ErrorMessage = "Test error";
            result.Exception = null;

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Single(errors);
            Assert.Contains("Exception should be provided when ErrorMessage is set", errors[0]);
        }

        [Fact]
        public void IsValid_HappyPath_WithValidResult_ReturnsTrue()
        {
            // Arrange
            var result = CreateValidProcessingResult();

            // Act
            var isValid = result.IsValid();

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_WithInvalidResult_ReturnsFalse()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.ResultId = 0;

            // Act
            var isValid = result.IsValid();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            ProcessingResult? result = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => result!.Validate());
            Assert.NotNull(ex.ParamName);
        }

        [Fact]
        public void EnsureValid_HappyPath_WithValidResult_DoesNotThrow()
        {
            // Arrange
            var result = CreateValidProcessingResult();

            // Act
            var ex = Record.Exception(() => result.EnsureValid());

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void EnsureValid_WithInvalidResult_ThrowsArgumentException()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.ResultId = 0;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => result.EnsureValid());
            Assert.Contains("ProcessingResult is invalid", ex.Message);
            Assert.Contains("ResultId must be a positive integer", ex.Message);
        }

        [Fact]
        public void EnsureValid_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            ProcessingResult? result = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => result!.EnsureValid());
        }

        [Fact]
        public void Validate_MultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var result = CreateValidProcessingResult();
            result.ResultId = 0;
            result.StageName = "";
            result.ProcessingTimeMs = -100;
            result.ProcessedAt = default;
            result.RetryCount = -5;
            result.Success = false;
            result.ErrorMessage = null;
            result.OutputData = null!;

            // Act
            var errors = result.Validate();

            // Assert
            Assert.Equal(7, errors.Count);
        }
    }
}