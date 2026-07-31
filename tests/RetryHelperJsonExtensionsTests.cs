using System;
using System.Text.Json;
using DotNetRealtimePipeline.Utilities;
using Xunit;

namespace DotNetRealtimePipeline.Tests
{
    public class RetryHelperJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            var retryHelper = new RetryHelper();
            var json = retryHelper.ToJson();
            Assert.False(string.IsNullOrWhiteSpace(json));
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsRetryHelper()
        {
            var retryHelper = new RetryHelper();
            var json = retryHelper.ToJson();
            var result = RetryHelperJsonExtensions.FromJson(json);
            Assert.NotNull(result);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndRetryHelper()
        {
            var retryHelper = new RetryHelper();
            var json = retryHelper.ToJson();
            var success = RetryHelperJsonExtensions.TryFromJson(json, out var result);
            Assert.True(success);
            Assert.NotNull(result);
        }

        [Fact]
        public void ToJsonPolicy_HappyPath_ReturnsJsonString()
        {
            var retryPolicy = new RetryPolicy();
            var json = retryPolicy.ToJson();
            Assert.False(string.IsNullOrWhiteSpace(json));
        }

        [Fact]
        public void FromJsonPolicy_HappyPath_ReturnsRetryPolicy()
        {
            var retryPolicy = new RetryPolicy();
            var json = retryPolicy.ToJson();
            var result = RetryHelperJsonExtensions.FromJsonPolicy(json);
            Assert.NotNull(result);
        }

        [Fact]
        public void TryFromJsonPolicy_HappyPath_ReturnsTrueAndRetryPolicy()
        {
            var retryPolicy = new RetryPolicy();
            var json = retryPolicy.ToJson();
            var success = RetryHelperJsonExtensions.TryFromJsonPolicy(json, out var result);
            Assert.True(success);
            Assert.NotNull(result);
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentNullException()
        {
            string? nullJson = null;
            Assert.Throws<ArgumentNullException>(() => RetryHelperJsonExtensions.FromJson(nullJson!));
        }

        [Fact]
        public void FromJsonPolicy_NullInput_ThrowsArgumentNullException()
        {
            string? nullJson = null;
            Assert.Throws<ArgumentNullException>(() => RetryHelperJsonExtensions.FromJsonPolicy(nullJson!));
        }
    }
}
