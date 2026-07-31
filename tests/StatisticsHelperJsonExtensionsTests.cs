using System;
using DotNetRealtimePipeline.Utilities;
using Xunit;

namespace DotNetRealtimePipeline.Tests
{
    public class StatisticsHelperJsonExtensionsTests
    {
        private static StatisticsHelper CreateSampleHelper()
        {
            // The StatisticsHelper class is assumed to have a parameterless constructor.
            // If it exposes properties, they can be set here to make the JSON more interesting.
            return new StatisticsHelper();
        }

        [Fact]
        public void ToJson_HappyPath_ReturnsNonEmptyString()
        {
            var helper = CreateSampleHelper();

            string json = helper.ToJson();

            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
        }

        [Fact]
        public void ToJson_WithIndentation_ReturnsIndentedJson()
        {
            var helper = CreateSampleHelper();

            string json = helper.ToJson(indented: true);

            Assert.Contains(Environment.NewLine, json);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            StatisticsHelper? nullHelper = null;

            Assert.Throws<ArgumentNullException>(() => nullHelper!.ToJson());
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsObject()
        {
            var original = CreateSampleHelper();
            string json = original.ToJson();

            StatisticsHelper? deserialized = StatisticsHelperJsonExtensions.FromJson(json);

            Assert.NotNull(deserialized);
            // Basic sanity check – the deserialized instance should be of the correct type.
            Assert.IsType<StatisticsHelper>(deserialized);
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => StatisticsHelperJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void FromJson_EmptyString_ReturnsNull()
        {
            StatisticsHelper? result = StatisticsHelperJsonExtensions.FromJson(string.Empty);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndValue()
        {
            var original = CreateSampleHelper();
            string json = original.ToJson();

            bool success = StatisticsHelperJsonExtensions.TryFromJson(json, out StatisticsHelper? value);

            Assert.True(success);
            Assert.NotNull(value);
            Assert.IsType<StatisticsHelper>(value);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            const string invalidJson = "{ this is not valid json }";

            bool success = StatisticsHelperJsonExtensions.TryFromJson(invalidJson, out StatisticsHelper? value);

            Assert.False(success);
            Assert.Null(value);
        }

        [Fact]
        public void TryFromJson_NullOrEmpty_ReturnsFalse()
        {
            bool successNull = StatisticsHelperJsonExtensions.TryFromJson(null!, out StatisticsHelper? valueNull);
            bool successEmpty = StatisticsHelperJsonExtensions.TryFromJson(string.Empty, out StatisticsHelper? valueEmpty);

            Assert.False(successNull);
            Assert.False(successEmpty);
            Assert.Null(valueNull);
            Assert.Null(valueEmpty);
        }
    }
}
