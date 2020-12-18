#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetRealtimePipeline.State;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

public sealed class PipelineStateManagerTests
{
    private readonly Mock<ILogger<PipelineStateManager>> _mockLogger;
    private readonly PipelineStateManager _manager;

    public PipelineStateManagerTests()
    {
        _mockLogger = new Mock<ILogger<PipelineStateManager>>();
        _manager = new PipelineStateManager(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PipelineStateManager(null!));
    }

    [Fact]
    public void CurrentState_ShouldDefaultToStopped()
    {
        // Act
        var state = _manager.CurrentState;

        // Assert
        state.Should().Be(PipelineStateManager.PipelineState.Stopped);
    }

    [Fact]
    public void IsOperational_WhenStopped_ShouldBeFalse()
    {
        // Act
        var isOperational = _manager.IsOperational;

        // Assert
        isOperational.Should().BeFalse();
    }

    [Fact]
    public async Task TransitionTo_FromStoppedToRunning_ShouldSucceed()
    {
        // Act
        var result = _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting pipeline");

        // Assert
        result.Should().BeTrue();
        _manager.CurrentState.Should().Be(PipelineStateManager.PipelineState.Running);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Running")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public void TransitionTo_FromRunningToPaused_ShouldSucceed()
    {
        // Arrange - First transition to Running
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");

        // Act
        var result = _manager.TransitionTo(PipelineStateManager.PipelineState.Paused, "Pausing pipeline");

        // Assert
        result.Should().BeTrue();
        _manager.CurrentState.Should().Be(PipelineStateManager.PipelineState.Paused);
    }

    [Fact]
    public void TransitionTo_FromPausedToRunning_ShouldSucceed()
    {
        // Arrange - Transition to Running then Paused
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");
        _manager.TransitionTo(PipelineStateManager.PipelineState.Paused, "Pausing");

        // Act
        var result = _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Resuming pipeline");

        // Assert
        result.Should().BeTrue();
        _manager.CurrentState.Should().Be(PipelineStateManager.PipelineState.Running);
    }

    [Fact]
    public void TransitionTo_FromRunningToStopped_ShouldSucceed()
    {
        // Arrange - Transition to Running
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");

        // Act
        var result = _manager.TransitionTo(PipelineStateManager.PipelineState.Stopped, "Stopping pipeline");

        // Assert
        result.Should().BeTrue();
        _manager.CurrentState.Should().Be(PipelineStateManager.PipelineState.Stopped);
    }

    [Fact]
    public void TransitionTo_FromPausedToStopped_ShouldSucceed()
    {
        // Arrange - Transition to Running then Paused
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");
        _manager.TransitionTo(PipelineStateManager.PipelineState.Paused, "Pausing");

        // Act
        var result = _manager.TransitionTo(PipelineStateManager.PipelineState.Stopped, "Stopping pipeline");

        // Assert
        result.Should().BeTrue();
        _manager.CurrentState.Should().Be(PipelineStateManager.PipelineState.Stopped);
    }

    [Fact]
    public void TransitionTo_FromFailedToStopped_ShouldSucceed()
    {
        // Arrange - Transition to Failed
        _manager.TransitionTo(PipelineStateManager.PipelineState.Failed, "Pipeline failed");

        // Act
        var result = _manager.TransitionTo(PipelineStateManager.PipelineState.Stopped, "Recovering from failure");

        // Assert
        result.Should().BeTrue();
        _manager.CurrentState.Should().Be(PipelineStateManager.PipelineState.Stopped);
    }

    [Fact]
    public void TransitionTo_FromStoppedToStopped_ShouldFail()
    {
        // Act
        var result = _manager.TransitionTo(PipelineStateManager.PipelineState.Stopped, "Already stopped");

        // Assert
        result.Should().BeFalse();
        _manager.CurrentState.Should().Be(PipelineStateManager.PipelineState.Stopped);
    }

    [Fact]
    public void TransitionTo_FromRunningToRunning_ShouldFail()
    {
        // Arrange - Transition to Running
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");

        // Act
        var result = _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Already running");

        // Assert
        result.Should().BeFalse();
        _manager.CurrentState.Should().Be(PipelineStateManager.PipelineState.Running);
    }

    [Fact]
    public void TransitionTo_FromStoppedToFailed_ShouldSucceed()
    {
        // Act
        var result = _manager.TransitionTo(PipelineStateManager.PipelineState.Failed, "Pipeline failed");

        // Assert
        result.Should().BeTrue();
        _manager.CurrentState.Should().Be(PipelineStateManager.PipelineState.Failed);
    }

    [Fact]
    public void TransitionTo_FromAnyToFailed_ShouldSucceed()
    {
        // Arrange - Transition to Running
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");

        // Act
        var result = _manager.TransitionTo(PipelineStateManager.PipelineState.Failed, "Critical failure");

        // Assert
        result.Should().BeTrue();
        _manager.CurrentState.Should().Be(PipelineStateManager.PipelineState.Failed);
    }

    [Fact]
    public void TransitionTo_FromStoppedToInitializing_ShouldFail()
    {
        // Act
        var result = _manager.TransitionTo(PipelineStateManager.PipelineState.Initializing, "Initializing");

        // Assert
        result.Should().BeFalse();
        _manager.CurrentState.Should().Be(PipelineStateManager.PipelineState.Stopped);
    }

    [Fact]
    public void GetStateHistory_ShouldReturnEmptyListInitially()
    {
        // Act
        var history = _manager.GetStateHistory();

        // Assert
        history.Should().BeEmpty();
    }

    [Fact]
    public void GetStateHistory_ShouldReturnAllTransitions()
    {
        // Arrange
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");
        _manager.TransitionTo(PipelineStateManager.PipelineState.Paused, "Pausing");
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Resuming");

        // Act
        var history = _manager.GetStateHistory();

        // Assert
        history.Should().HaveCount(3);
        history[0].FromState.Should().Be(PipelineStateManager.PipelineState.Stopped);
        history[0].ToState.Should().Be(PipelineStateManager.PipelineState.Running);
        history[1].FromState.Should().Be(PipelineStateManager.PipelineState.Running);
        history[1].ToState.Should().Be(PipelineStateManager.PipelineState.Paused);
        history[2].FromState.Should().Be(PipelineStateManager.PipelineState.Paused);
        history[2].ToState.Should().Be(PipelineStateManager.PipelineState.Running);
    }

    [Fact]
    public void GetStateHistory_ShouldReturnCopyNotReference()
    {
        // Arrange
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");

        // Act
        var history1 = _manager.GetStateHistory();
        history1.Add(new PipelineStateManager.StateTransition()); // Try to modify
        var history2 = _manager.GetStateHistory();

        // Assert
        history2.Should().HaveCount(1); // Should not include the added transition
    }

    [Fact]
    public void GetCurrentStateDuration_WithNoTransitions_ShouldReturnZero()
    {
        // Act
        var duration = _manager.GetCurrentStateDuration();

        // Assert
        duration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void GetCurrentStateDuration_ShouldReturnTimeSinceLastTransition()
    {
        // Arrange
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");
        var startTime = DateTime.UtcNow;

        // Wait a small amount
        Thread.Sleep(10);

        // Act
        var duration = _manager.GetCurrentStateDuration();

        // Assert
        duration.Should().BeGreaterThan(TimeSpan.Zero);
        duration.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RegisterStateChangeListener_ShouldReceiveCallbacks()
    {
        // Arrange
        bool callbackInvoked = false;
        PipelineStateManager.PipelineState oldState = default;
        PipelineStateManager.PipelineState newState = default;

        void listener(PipelineStateManager.PipelineState from, PipelineStateManager.PipelineState to)
        {
            callbackInvoked = true;
            oldState = from;
            newState = to;
        }

        _manager.RegisterStateChangeListener(listener);

        // Act
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");

        // Assert
        callbackInvoked.Should().BeTrue();
        oldState.Should().Be(PipelineStateManager.PipelineState.Stopped);
        newState.Should().Be(PipelineStateManager.PipelineState.Running);
    }

    [Fact]
    public void RegisterStateChangeListener_ShouldHandleExceptionsGracefully()
    {
        // Arrange
        void failingListener(PipelineStateManager.PipelineState from, PipelineStateManager.PipelineState to)
        {
            throw new Exception("Listener failed");
        }

        _manager.RegisterStateChangeListener(failingListener);

        // Act - Should not throw even if listener throws
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");

        // Assert - Transition still succeeds
        _manager.CurrentState.Should().Be(PipelineStateManager.PipelineState.Running);
    }

    [Fact]
    public void RegisterStateChangeListener_MultipleListeners_ShouldAllBeCalled()
    {
        // Arrange
        int callCount = 0;

        void listener1(PipelineStateManager.PipelineState from, PipelineStateManager.PipelineState to) => callCount++;
        void listener2(PipelineStateManager.PipelineState from, PipelineStateManager.PipelineState to) => callCount++;
        void listener3(PipelineStateManager.PipelineState from, PipelineStateManager.PipelineState to) => callCount++;

        _manager.RegisterStateChangeListener(listener1);
        _manager.RegisterStateChangeListener(listener2);
        _manager.RegisterStateChangeListener(listener3);

        // Act
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");

        // Assert
        callCount.Should().Be(3);
    }

    [Fact]
    public void StateTransition_ShouldHaveCorrectProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var transition = new PipelineStateManager.StateTransition
        {
            FromState = PipelineStateManager.PipelineState.Stopped,
            ToState = PipelineStateManager.PipelineState.Running,
            Timestamp = now,
            Reason = "Starting pipeline"
        };

        // Assert
        transition.FromState.Should().Be(PipelineStateManager.PipelineState.Stopped);
        transition.ToState.Should().Be(PipelineStateManager.PipelineState.Running);
        transition.Timestamp.Should().Be(now);
        transition.Reason.Should().Be("Starting pipeline");
    }

    [Fact]
    public void IsOperational_WhenRunning_ShouldBeTrue()
    {
        // Arrange
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");

        // Act
        var isOperational = _manager.IsOperational;

        // Assert
        isOperational.Should().BeTrue();
    }

    [Fact]
    public void IsOperational_WhenPaused_ShouldBeFalse()
    {
        // Arrange
        _manager.TransitionTo(PipelineStateManager.PipelineState.Running, "Starting");
        _manager.TransitionTo(PipelineStateManager.PipelineState.Paused, "Pausing");

        // Act
        var isOperational = _manager.IsOperational;

        // Assert
        isOperational.Should().BeFalse();
    }

    [Fact]
    public void IsOperational_WhenFailed_ShouldBeFalse()
    {
        // Arrange
        _manager.TransitionTo(PipelineStateManager.PipelineState.Failed, "Failed");

        // Act
        var isOperational = _manager.IsOperational;

        // Assert
        isOperational.Should().BeFalse();
    }

    [Fact]
    public void IsOperational_WhenInitializing_ShouldBeFalse()
    {
        // Arrange
        _manager.TransitionTo(PipelineStateManager.PipelineState.Initializing, "Initializing");

        // Act
        var isOperational = _manager.IsOperational;

        // Assert
        isOperational.Should().BeFalse();
    }
}
