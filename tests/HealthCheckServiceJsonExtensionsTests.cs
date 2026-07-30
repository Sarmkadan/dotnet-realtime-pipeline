// tests/HealthCheckServiceJsonExtensionsTests.cs
using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using DotNetRealtimePipeline.Monitoring;

namespace DotNetRealtimePipeline.Tests.Monitoring;

public class HealthCheckServiceJsonExtensionsTests
{
    private static HealthCheckService CreateService()
    {
        var orchestratorMock = new Mock<PipelineOrchestrator>();
        var loggerMock = new Mock<ILogger<HealthCheckService>>();
        return new HealthCheckService(orchestratorMock.Object, loggerMock.Object);
    }

    [Fact]
    public void ToJson_HappyPath_ReturnsNonEmptyString()
    {
        var service = CreateService();
        var json = service.ToJson();

        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    [Fact]
    public void ToJson_WithIndentation_ReturnsIndentedJson()
    {
        var service = CreateService();
        var json = service.ToJson(indented: true);

        Assert.Contains(Environment.NewLine, json);
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsInstance()
    {
        var service = CreateService();
        var json = service.ToJson();

        var deserialized = HealthCheckServiceJsonExtensions.FromJson(json);

        Assert.NotNull(deserialized);
    }

    [Fact]
    public void FromJson_EmptyOrWhitespace_ReturnsNull()
    {
        Assert.Null(HealthCheckServiceJsonExtensions.FromJson(string.Empty));
        Assert.Null(HealthCheckServiceJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void FromJson_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => HealthCheckServiceJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndInstance()
    {
        var service = CreateService();
        var json = service.ToJson();

        var success = HealthCheckServiceJsonExtensions.TryFromJson(json, out var deserialized);

        Assert.True(success);
        Assert.NotNull(deserialized);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        var invalidJson = "{ not valid json }";

        var success = HealthCheckServiceJsonExtensions.TryFromJson(invalidJson, out var deserialized);

        Assert.False(success);
        Assert.Null(deserialized);
    }

    [Fact]
    public void TryFromJson_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => HealthCheckServiceJsonExtensions.TryFromJson(null!, out _));
    }
}
