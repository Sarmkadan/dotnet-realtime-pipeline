using System;
using System.Text.Json;
using DotNetRealtimePipeline.Utilities;
using Xunit;

namespace BatchProcessorJsonExtensionsTests
{
    public class BatchProcessorJsonExtensionsTests
    {
        [Fact]
        public void ToJson_NullValue_ThrowsArgumentNullException()
        {
            DataPointBatchProcessor? nullProcessor = null;
            Assert.Throws<ArgumentNullException>(() => nullProcessor!.ToJson());
        }

        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            var processor = new DataPointBatchProcessor();
            var json = processor.ToJson();
            Assert.False(string.IsNullOrWhiteSpace(json));
            // Basic sanity: the JSON should start with '{' and end with '}'
            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
        }

        [Fact]
        public void ToJson_IndentedTrue_ReturnsFormattedJson()
        {
            var processor = new DataPointBatchProcessor();
            var json = processor.ToJson(indented: true);
            // Indented JSON should contain line breaks
            Assert.Contains(Environment.NewLine, json);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            string? nullJson = null;
            Assert.Throws<ArgumentNullException>(() => BatchProcessorJsonExtensions.FromJson(nullJson!));
        }

        [Fact]
        public void FromJson_EmptyOrWhitespace_ReturnsNull()
        {
            var empty = BatchProcessorJsonExtensions.FromJson("");
            var whitespace = BatchProcessorJsonExtensions.FromJson("   ");
            Assert.Null(empty);
            Assert.Null(whitespace);
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsInstance()
        {
            var processor = new DataPointBatchProcessor();
            var json = JsonSerializer.Serialize(processor);
            var result = BatchProcessorJsonExtensions.FromJson(json);
            Assert.NotNull(result);
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            string? nullJson = null;
            Assert.Throws<ArgumentNullException>(() => BatchProcessorJsonExtensions.TryFromJson(nullJson!, out _));
        }

        [Fact]
        public void TryFromJson_EmptyOrWhitespace_ReturnsTrueAndNull()
        {
            bool successEmpty = BatchProcessorJsonExtensions.TryFromJson("", out var emptyResult);
            bool successWhitespace = BatchProcessorJsonExtensions.TryFromJson("   ", out var whitespaceResult);
            Assert.True(successEmpty);
            Assert.True(successWhitespace);
            Assert.Null(emptyResult);
            Assert.Null(whitespaceResult);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            var invalidJson = "{ this is not valid json }";
            bool success = BatchProcessorJsonExtensions.TryFromJson(invalidJson, out var result);
            Assert.False(success);
            Assert.Null(result);
        }
    }
}
