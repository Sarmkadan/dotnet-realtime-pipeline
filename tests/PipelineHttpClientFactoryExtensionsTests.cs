// tests/PipelineHttpClientFactoryExtensionsTests.cs
namespace DotNetRealtimePipeline.Tests.Integration;

using Xunit;
using System;
using System.Net.Http;
using DotNetRealtimePipeline.Integration;

public class PipelineHttpClientFactoryExtensionsTests
{
    [Fact]
    public void CreateClientWithBaseAddress_ReturnsHttpClient()
    {
        // Arrange
        var factory = new PipelineHttpClientFactory();

        // Act
        var client = factory.CreateClientWithBaseAddress("https://example.com");

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void CreateClientWithBaseAddress_ThrowsArgumentNullException_ForNullFactory()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => PipelineHttpClientFactoryExtensions.CreateClientWithBaseAddress(null, "https://example.com"));
    }

    [Fact]
    public void CreateClientWithBaseAddress_ThrowsArgumentException_ForEmptyBaseAddress()
    {
        // Act and Assert
        Assert.Throws<ArgumentException>(() => PipelineHttpClientFactoryExtensions.CreateClientWithBaseAddress(new PipelineHttpClientFactory(), string.Empty));
    }

    [Fact]
    public void CreateConfiguredServiceClient_ReturnsHttpClient()
    {
        // Arrange
        var factory = new PipelineHttpClientFactory();

        // Act
        var client = factory.CreateConfiguredServiceClient("service", TimeSpan.FromSeconds(10), 3, TimeSpan.FromSeconds(1), true);

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void CreateConfiguredServiceClient_ThrowsArgumentNullException_ForNullFactory()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => PipelineHttpClientFactoryExtensions.CreateConfiguredServiceClient(null, "service", TimeSpan.FromSeconds(10), 3, TimeSpan.FromSeconds(1), true));
    }

    [Fact]
    public void CreateConfiguredServiceClient_ThrowsArgumentException_ForEmptyServiceName()
    {
        // Act and Assert
        Assert.Throws<ArgumentException>(() => PipelineHttpClientFactoryExtensions.CreateConfiguredServiceClient(new PipelineHttpClientFactory(), string.Empty, TimeSpan.FromSeconds(10), 3, TimeSpan.FromSeconds(1), true));
    }

    [Fact]
    public void CreateConfiguredServiceClient_ThrowsArgumentOutOfRangeException_ForNegativeTimeout()
    {
        // Act and Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => PipelineHttpClientFactoryExtensions.CreateConfiguredServiceClient(new PipelineHttpClientFactory(), "service", TimeSpan.FromSeconds(-10), 3, TimeSpan.FromSeconds(1), true));
    }

    [Fact]
    public void CreateConfiguredServiceClient_ThrowsArgumentOutOfRangeException_ForNegativeMaxRetries()
    {
        // Act and Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => PipelineHttpClientFactoryExtensions.CreateConfiguredServiceClient(new PipelineHttpClientFactory(), "service", TimeSpan.FromSeconds(10), -3, TimeSpan.FromSeconds(1), true));
    }

    [Fact]
    public async Task GetStringAsync_ReturnsResponseContent()
    {
        // Arrange
        var factory = new PipelineHttpClientFactory();
        var client = factory.CreateDefaultClient();

        // Act
        var response = await client.GetAsync("https://example.com");

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetStringAsync_ThrowsArgumentNullException_ForNullFactory()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => PipelineHttpClientFactoryExtensions.GetStringAsync(null, "https://example.com"));
    }

    [Fact]
    public async Task GetStringAsync_ThrowsArgumentException_ForEmptyUri()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(() => PipelineHttpClientFactoryExtensions.GetStringAsync(new PipelineHttpClientFactory(), string.Empty));
    }

    [Fact]
    public async Task PostJsonAsync_ReturnsResponseContent()
    {
        // Arrange
        var factory = new PipelineHttpClientFactory();
        var client = factory.CreateDefaultClient();
        var content = new StringContent("Hello, World!", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("https://example.com", content);

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public async Task PostJsonAsync_ThrowsArgumentNullException_ForNullFactory()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => PipelineHttpClientFactoryExtensions.PostJsonAsync(null, "https://example.com", new StringContent("Hello, World!", System.Text.Encoding.UTF8, "application/json")));
    }

    [Fact]
    public async Task PostJsonAsync_ThrowsArgumentNullException_ForNullContent()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => PipelineHttpClientFactoryExtensions.PostJsonAsync(new PipelineHttpClientFactory(), "https://example.com", null));
    }

    [Fact]
    public async Task PostJsonAsync_ThrowsArgumentException_ForEmptyUri()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(() => PipelineHttpClientFactoryExtensions.PostJsonAsync(new PipelineHttpClientFactory(), string.Empty, new StringContent("Hello, World!", System.Text.Encoding.UTF8, "application/json")));
    }
}
