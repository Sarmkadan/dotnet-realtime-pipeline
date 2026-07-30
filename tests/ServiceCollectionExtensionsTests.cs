using Xunit;
using Microsoft.Extensions.DependencyInjection;
using DotNetRealtimePipeline.Configuration;
using DotNetRealtimePipeline.Domain.Models;

namespace DotNetRealtimePipeline.Tests.Configuration;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPipelineServices_WithValidConfig_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var pipelineConfig = new PipelineConfig(1, "TestPipeline", "1.0.0");

        // Act and Assert
        Assert.DoesNotThrow(() => services.AddPipelineServices(pipelineConfig));
    }

    [Fact]
    public void AddPipelineServices_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        PipelineConfig pipelineConfig = new PipelineConfig(1, "TestPipeline", "1.0.0");

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null).AddPipelineServices(pipelineConfig));
    }

    [Fact]
    public void AddPipelineServices_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => services.AddPipelineServices(null));
    }

    [Fact]
    public void AddPipelineServices_WithDefaultConfig_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act and Assert
        Assert.DoesNotThrow(() => services.AddPipelineServices());
    }

    [Fact]
    public void AddPipelineServices_WithConfigAction_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<PipelineConfig> configureOptions = config => { };

        // Act and Assert
        Assert.DoesNotThrow(() => services.AddPipelineServices(configureOptions));
    }

    [Fact]
    public void AddPipelineServices_WithNullConfigAction_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => services.AddPipelineServices(null));
    }
}
