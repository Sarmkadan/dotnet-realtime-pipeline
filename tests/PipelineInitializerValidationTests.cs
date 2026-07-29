// tests/PipelineInitializerValidationTests.cs
namespace DotNetRealtimePipeline.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class PipelineInitializerValidationTests
{
    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var pipelineInitializer = new PipelineInitializer(new ServiceProvider(), new Logger(), new StateManager());

        // Act
        var errors = PipelineInitializerValidation.Validate(pipelineInitializer);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NullPipelineInitializer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineInitializerValidation.Validate(null));
    }

    [Fact]
    public void Validate_ServiceProviderNull_ThrowsNoException()
    {
        // Arrange
        var pipelineInitializer = new PipelineInitializer(null, new Logger(), new StateManager());

        // Act
        var errors = PipelineInitializerValidation.Validate(pipelineInitializer);

        // Assert
        Assert.Single(errors);
    }

    [Fact]
    public void Validate_LoggerNull_ThrowsNoException()
    {
        // Arrange
        var pipelineInitializer = new PipelineInitializer(new ServiceProvider(), null, new StateManager());

        // Act
        var errors = PipelineInitializerValidation.Validate(pipelineInitializer);

        // Assert
        Assert.Single(errors);
    }

    [Fact]
    public void Validate_StateManagerNull_ThrowsNoException()
    {
        // Arrange
        var pipelineInitializer = new PipelineInitializer(new ServiceProvider(), new Logger(), null);

        // Act
        var errors = PipelineInitializerValidation.Validate(pipelineInitializer);

        // Assert
        Assert.Single(errors);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var pipelineInitializer = new PipelineInitializer(new ServiceProvider(), new Logger(), new StateManager());

        // Act
        var isValid = PipelineInitializerValidation.IsValid(pipelineInitializer);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_NullPipelineInitializer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineInitializerValidation.IsValid(null));
    }

    [Fact]
    public void IsValid_ServiceProviderNull_ReturnsFalse()
    {
        // Arrange
        var pipelineInitializer = new PipelineInitializer(null, new Logger(), new StateManager());

        // Act
        var isValid = PipelineInitializerValidation.IsValid(pipelineInitializer);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var pipelineInitializer = new PipelineInitializer(new ServiceProvider(), new Logger(), new StateManager());

        // Act & Assert
        PipelineInitializerValidation.EnsureValid(pipelineInitializer);
    }

    [Fact]
    public void EnsureValid_NullPipelineInitializer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineInitializerValidation.EnsureValid(null));
    }

    [Fact]
    public void EnsureValid_ServiceProviderNull_ThrowsArgumentException()
    {
        // Arrange
        var pipelineInitializer = new PipelineInitializer(null, new Logger(), new StateManager());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PipelineInitializerValidation.EnsureValid(pipelineInitializer));
    }
}
