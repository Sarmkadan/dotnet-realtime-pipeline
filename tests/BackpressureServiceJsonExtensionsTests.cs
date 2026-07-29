using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Xunit;
using System;

namespace DotNetRealtimePipeline.Tests.Services;
{
    public class BackpressureServiceJsonExtensionsTests
    {
        [Fact]
        public void ToJson_Happy_PATH_Test()
        {
            // Given
            var service = new BackpressureServiceJsonExtensions();
            // When
            var json = service.ToJson();
            // Then
            Assert.NotNull(json);
        }

        [Fact]
        public void FromJson_HAPPY_PATH_Test()
        {
            // Given
            var json = "{}";
            // When
            var service = BackpressureServiceJsonExtensions.FromJson(json);
            // Then
            Assert.NotNull(service);
        }

        [Fact]
        public void TryFromJson_HAPPY_PATH_Test()
        {
            // Given
            var json = "{}";
            BackpressureService? service;
            // When
            var result = BackpressureServiceJsonExtensions.TryFromJson(json, out service);
            // Then
            Assert.True(result);
            Assert.NotNull(service);
        }
    }
}