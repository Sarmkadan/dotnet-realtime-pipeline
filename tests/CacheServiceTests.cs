using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;

namespace DotNetRealtimePipeline.Tests
{
    public class CacheServiceTests
    {
        [Fact]
        public async Task TestTryGetValue_Hit()
        {
            // Arrange
            var cacheService = new CacheService<int, string>(1000, TimeSpan.FromHours(1));
            var key = 1;
            var value = "test";
            cacheService.Set(key, value);

            // Act
            var result = await cacheService.TryGetValue(key, out var cachedValue);

            // Assert
            Assert.True(result);
            Assert.Equal(value, cachedValue);
        }

        [Fact]
        public async Task TestTryGetValue_Miss()
        {
            // Arrange
            var cacheService = new CacheService<int, string>(1000, TimeSpan.FromHours(1));
            var key = 1;
            var value = "test";

            // Act
            var result = await cacheService.TryGetValue(key, out var cachedValue);

            // Assert
            Assert.False(result);
            Assert.Null(cachedValue);
        }

        [Fact]
        public async Task TestTryGetValue_Expired()
        {
            // Arrange
            var cacheService = new CacheService<int, string>(1000, TimeSpan.FromHours(1));
            var key = 1;
            var value = "test";
            cacheService.Set(key, value, TimeSpan.FromMilliseconds(1));
            await Task.Delay(TimeSpan.FromMilliseconds(2));

            // Act
            var result = await cacheService.TryGetValue(key, out var cachedValue);

            // Assert
            Assert.False(result);
            Assert.Null(cachedValue);
        }

        [Fact]
        public async Task TestSet_Get()
        {
            // Arrange
            var cacheService = new CacheService<int, string>(1000, TimeSpan.FromHours(1));
            var key = 1;
            var value = "test";
            cacheService.Set(key, value);

            // Act
            var result = await cacheService.TryGetValue(key, out var cachedValue);

            // Assert
            Assert.True(result);
            Assert.Equal(value, cachedValue);
        }

        [Fact]
        public async Task TestSet_Remove()
        {
            // Arrange
            var cacheService = new CacheService<int, string>(1000, TimeSpan.FromHours(1));
            var key = 1;
            var value = "test";
            cacheService.Set(key, value);
            cacheService.TryRemove(key);

            // Act
            var result = await cacheService.TryGetValue(key, out var cachedValue);

            // Assert
            Assert.False(result);
            Assert.Null(cachedValue);
        }

        [Fact]
        public async Task TestClear_Get()
        {
            // Arrange
            var cacheService = new CacheService<int, string>(1000, TimeSpan.FromHours(1));
            var key = 1;
            var value = "test";
            cacheService.Set(key, value);
            cacheService.Clear();

            // Act
            var result = await cacheService.TryGetValue(key, out var cachedValue);

            // Assert
            Assert.False(result);
            Assert.Null(cachedValue);
        }
    }
}