// tests/RawPipelineAccessorTests.cs
namespace DotNetRealtimePipeline.Tests.Integration;

using Xunit;
using System;
using System.IO.Pipelines;
using DotNetRealtimePipeline.Integration;

public class RawPipelineAccessorTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_ForNullPipeOptions()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new RawPipelineAccessor(null));
    }

    [Fact]
    public void Constructor_CreatesRawPipelineAccessor_WithDefaultPipeOptions()
    {
        // Act
        var accessor = new RawPipelineAccessor();

        // Assert
        Assert.NotNull(accessor.AsPipeReader());
        Assert.NotNull(accessor.AsPipeWriter());
    }

    [Fact]
    public void AsPipeReader_ReturnsPipeReader()
    {
        // Act
        var accessor = new RawPipelineAccessor();
        var pipeReader = accessor.AsPipeReader();

        // Assert
        Assert.NotNull(pipeReader);
    }

    [Fact]
    public void AsPipeWriter_ReturnsPipeWriter()
    {
        // Act
        var accessor = new RawPipelineAccessor();
        var pipeWriter = accessor.AsPipeWriter();

        // Assert
        Assert.NotNull(pipeWriter);
    }

    [Fact]
    public void Reset_ResetsPipe()
    {
        // Act
        var accessor = new RawPipelineAccessor();
        accessor.Reset();

        // Assert
        Assert.True(accessor.AsPipeReader().IsCompleted);
        Assert.True(accessor.AsPipeWriter().IsCompleted);
    }

    [Fact]
    public void Dispose_DisposesPipe()
    {
        // Act
        var accessor = new RawPipelineAccessor();
        accessor.Dispose();

        // Assert
        Assert.True(accessor.AsPipeReader().IsCompleted);
        Assert.True(accessor.AsPipeWriter().IsCompleted);
    }
}
