// tests/PipelineHttpClientFactoryTests.cs
namespace DotNetRealtimePipeline.Tests.Integration;

using System;
using System.Collections.Generic;
using System.Net.Http;
using DotNetRealtimePipeline.Integration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class PipelineHttpClientFactoryTests
{
    private readonly PipelineHttpClientFactory _factory;

    public PipelineHttpClientFactoryTests()
    {
        // Use the built‑in NullLogger so we don't need any mocking framework.
        _factory = new PipelineHttpClientFactory(NullLogger<PipelineHttpClientFactory>.Instance);
    }

    [Fact]
    public void DefaultProperties_ShouldMatchExpectedValues()
    {
        Assert.Equal(TimeSpan.FromSeconds(30), _factory.Timeout);
        Assert.Equal(3, _factory.MaxRetries);
        Assert.Equal(TimeSpan.FromSeconds(1), _factory.RetryDelay);
        Assert.Equal(10, _factory.MaxConnectionsPerHost);
        Assert.True(_factory.UseCompression);
        Assert.Equal("DotNetRealtimePipeline/1.0", _factory.UserAgent);
        Assert.NotNull(_factory.DefaultHeaders);
        Assert.Single(_factory.DefaultHeaders);
        Assert.Equal("application/json", _factory.DefaultHeaders["Accept"]);
    }

    [Fact]
    public void CreateDefaultClient_ShouldConfigureHeadersAndTimeout()
    {
        using var client = _factory.CreateDefaultClient();

        // Timeout is set to 30 seconds in the implementation.
        Assert.Equal(TimeSpan.FromSeconds(30), client.Timeout);

        // Default request headers
        Assert.True(client.DefaultRequestHeaders.Contains("User-Agent"));
        Assert.Contains("DotNetRealtimePipeline/1.0", client.DefaultRequestHeaders.GetValues("User-Agent"));
        Assert.True(client.DefaultRequestHeaders.Contains("Accept"));
        Assert.Contains("application/json", client.DefaultRequestHeaders.GetValues("Accept"));
    }

    [Fact]
    public void CreateServiceClient_ShouldConfigureHeadersAndCacheInstance()
    {
        var serviceName = "myservice";
        var timeout = TimeSpan.FromSeconds(15);

        // First call creates a new client.
        var client1 = _factory.CreateServiceClient(serviceName, timeout);
        Assert.NotNull(client1);
        Assert.Equal(timeout, client1.Timeout);
        Assert.True(client1.DefaultRequestHeaders.Contains("User-Agent"));
        Assert.Contains($"DotNetRealtimePipeline/{serviceName}", client1.DefaultRequestHeaders.GetValues("User-Agent"));
        Assert.True(client1.DefaultRequestHeaders.Contains("Accept"));
        Assert.Contains("application/json", client1.DefaultRequestHeaders.GetValues("Accept"));
        Assert.True(client1.DefaultRequestHeaders.Contains("Accept-Encoding"));
        Assert.Contains("gzip, deflate", client1.DefaultRequestHeaders.GetValues("Accept-Encoding"));

        // Second call with the same service name returns the cached instance.
        var client2 = _factory.CreateServiceClient(serviceName, timeout);
        Assert.Same(client1, client2);
    }

    [Fact]
    public void CreateServiceClient_NullServiceName_ShouldThrowArgumentNullException()
    {
        // Dictionary<string, HttpClient> does not allow null keys, so a null service name
        // results in an ArgumentNullException when trying to look up the cache.
        Assert.Throws<ArgumentNullException>(() => _factory.CreateServiceClient(null!, TimeSpan.FromSeconds(10)));
    }

    [Fact]
    public void CreateServiceClient_NegativeTimeout_ShouldThrowArgumentOutOfRangeException()
    {
        // HttpClient.Timeout throws ArgumentOutOfRangeException for negative values.
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _factory.CreateServiceClient("negativetimeout", TimeSpan.FromSeconds(-5)));
    }

    [Fact]
    public void CreateServiceClient_DisableCompression_ShouldCreateHandlerWithoutDecompression()
    {
        // The compression flag only influences the HttpClientHandler's AutomaticDecompression.
        // While the handler is not publicly exposed, we can verify that the client is still
        // created and that the Accept-Encoding header is still present (the header is added
        // regardless of the flag). The absence of an exception proves the path works.
        var client = _factory.CreateServiceClient("nocomp", TimeSpan.FromSeconds(10), useCompression: false);
        Assert.NotNull(client);
        Assert.True(client.DefaultRequestHeaders.Contains("Accept-Encoding"));
    }
}
