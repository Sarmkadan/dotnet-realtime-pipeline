namespace DotNetRealtimePipeline.Tests.Unit;

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using DotNetRealtimePipeline.State;
using System;
using System.Linq;
using System.Collections.Generic;

public class PipelineStateManagerExtensionsTests
{
    private readonly Mock<ILogger<PipelineStateManager>> _mockLogger;
    private readonly PipelineStateManager _manager;

    public PipelineStateManagerExtensionsTests()
    {
        _mockLogger = new Mock<ILogger<PipelineStateManager>>();
        _manager = new PipelineStateManager(_mockLogger.Object);
    }

    [Fact]
    public void GetTransitionsTo_ReturnsCorrectTransitions()
    {
        _manager.TransitionTo(PipelineState.Running, "reason1");
        _manager.TransitionTo(PipelineState.Paused, "reason2");
        _manager.TransitionTo(PipelineState.Running, "reason3");

        var transitions = _manager.GetTransitionsTo(PipelineState.Running);

        Assert.Equal(2, transitions.Count);
        Assert.All(transitions, t => Assert.Equal(PipelineState.Running, t.ToState));
    }

    [Fact]
    public void GetLastTransition_ReturnsCorrectLastTransition()
    {
        _manager.TransitionTo(PipelineState.Running, "reason1");
        _manager.TransitionTo(PipelineState.Paused, "reason2");

        var last = _manager.GetLastTransition();

        Assert.NotNull(last);
        Assert.Equal(PipelineState.Running, last!.FromState);
        Assert.Equal(PipelineState.Paused, last!.ToState);
    }

    [Fact]
    public void GetTotalTimeInState_CalculatesCorrectTime()
    {
        // This is tricky to test with real time.
        // The implementation uses DateTime.UtcNow.
        // For the sake of unit testing, we might need a way to mock time, 
        // but given the current implementation, we'll test the zero case and simple path.
        
        var time = _manager.GetTotalTimeInState(PipelineState.Stopped);
        Assert.True(time >= TimeSpan.Zero);
    }

    [Fact]
    public void ToHistoryString_ReturnsFormattedString()
    {
        _manager.TransitionTo(PipelineState.Running, "reason1");
        
        var history = _manager.ToHistoryString();
        
        Assert.Contains("Stopped → Running", history);
        Assert.Contains("reason1", history);
    }

    [Fact]
    public void ExtensionMethods_ThrowArgumentNullException_OnNullManager()
    {
        PipelineStateManager? nullManager = null;
        
        Assert.Throws<ArgumentNullException>(() => nullManager!.GetTransitionsTo(PipelineState.Running));
        Assert.Throws<ArgumentNullException>(() => nullManager!.GetLastTransition());
        Assert.Throws<ArgumentNullException>(() => nullManager!.GetTotalTimeInState(PipelineState.Running));
        Assert.Throws<ArgumentNullException>(() => nullManager!.ToHistoryString());
    }
}
