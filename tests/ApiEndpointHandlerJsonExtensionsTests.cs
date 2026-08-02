using System;
using System.Text.Json;
using DotNetRealtimePipeline.API;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetRealtimePipeline.Tests
{
    /// <summary>
    /// Unit tests for <see cref="ApiEndpointHandlerJsonExtensions"/>.
    /// Covers serialization, deserialization and the Try pattern.
    /// </summary>
    public class ApiEndpointHandlerJsonExtensionsTests
    {
        private static DataIngestionHandler CreateHandler()
        {
            // The orchestrator constructor expects a number of service dependencies.
            // We provide mocks for all of them – the handler itself does not invoke any
            // orchestrator methods during serialization, so the behavior of the mocks is irrelevant.
            var orchestratorMock = new Mock<PipelineOrchestrator>(
                Mock.Of<DataProcessingService>(),
                Mock.Of<WindowingService>(),
                Mock.Of<MetricsService>(),
                Mock.Of<BackpressureService>(),
                Mock.Of<QueryService>(),
                new PipelineConfig()
            );

            var loggerMock = new Mock<ILogger<DataIngestionHandler>>();
            return new DataIngestionHandler(orchestratorMock.Object, loggerMock.Object);
        }

        [Fact]
        public void ToJson_HappyPath_ReturnsNonEmptyString()
        {
            // Arrange
            var handler = CreateHandler();

            // Act
            var json = handler.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            // The concrete type name should appear somewhere in the JSON payload.
            Assert.Contains(nameof(DataIngestionHandler), json);
        }

        [Fact]
        public void ToJson_IndentedTrue_ProducesFormattedJson()
        {
            // Arrange
            var handler = CreateHandler();

            // Act
            var json = handler.ToJson(indented: true);

            // Assert
            Assert.Contains("\n", json); // at least one newline
            Assert.Contains("  ", json); // indentation (two spaces) should be present
        }

        [Fact]
        public void FromJson_HappyPath_DeserializesHandler()
        {
            // Arrange
            var handler = CreateHandler();
            var json = handler.ToJson();

            // Act
            var deserialized = ApiEndpointHandlerJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsType<DataIngestionHandler>(deserialized);
        }

        [Fact]
        public void FromJson_NullOrEmpty_ThrowsArgumentException()
        {
            // Arrange
            string empty = string.Empty;
            string? nullStr = null;

            // Act / Assert
            Assert.Throws<ArgumentException>(() => ApiEndpointHandlerJsonExtensions.FromJson(empty));
            Assert.Throws<ArgumentException>(() => ApiEndpointHandlerJsonExtensions.FromJson(nullStr!));
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidJson = "this is not json";

            // Act / Assert
            Assert.Throws<JsonException>(() => ApiEndpointHandlerJsonExtensions.FromJson(invalidJson));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndHandler()
        {
            // Arrange
            var handler = CreateHandler();
            var json = handler.ToJson();

            // Act
            var result = ApiEndpointHandlerJsonExtensions.TryFromJson(json, out var value);

            // Assert
            Assert.True(result);
            Assert.NotNull(value);
            Assert.IsType<DataIngestionHandler>(value);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var invalidJson = "invalid { json";

            // Act
            var result = ApiEndpointHandlerJsonExtensions.TryFromJson(invalidJson, out var value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        [Fact]
        public void TryFromJson_NullOrEmpty_ThrowsArgumentException()
        {
            // Arrange
            string empty = string.Empty;
            string? nullStr = null;

            // Act / Assert
            Assert.Throws<ArgumentException>(() => ApiEndpointHandlerJsonExtensions.TryFromJson(empty, out _));
            Assert.Throws<ArgumentException>(() => ApiEndpointHandlerJsonExtensions.TryFromJson(nullStr!, out _));
        }
    }
}
