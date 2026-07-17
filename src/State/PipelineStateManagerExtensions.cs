#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DotNetRealtimePipeline.State;

/// <summary>
/// Extension methods that add useful query capabilities to <see cref="PipelineStateManager"/>.
/// </summary>
public static class PipelineStateManagerExtensions
{
    /// <summary>
    /// Returns all state transitions whose <c>ToState</c> matches the specified <paramref name="targetState"/>.
    /// </summary>
    /// <param name="manager">The <see cref="PipelineStateManager"/> instance.</param>
    /// <param name="targetState">The state to filter transitions by.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of matching <see cref="StateTransition"/> objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manager"/> is <c>null</c>.</exception>
    public static IReadOnlyList<StateTransition> GetTransitionsTo(this PipelineStateManager manager, PipelineState targetState)
    {
        ArgumentNullException.ThrowIfNull(manager);
        // The underlying GetStateHistory returns a List<StateTransition>; we project to an array for IReadOnlyList.
        return manager.GetStateHistory()
            .Where(t => t.ToState == targetState)
            .ToArray();
    }

    /// <summary>
    /// Retrieves the most recent state transition, or <c>null</c> if no transitions have been recorded.
    /// </summary>
    /// <param name="manager">The <see cref="PipelineStateManager"/> instance.</param>
    /// <returns>The latest <see cref="StateTransition"/>, or <c>null</c> when the history is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manager"/> is <c>null</c>.</exception>
    public static StateTransition? GetLastTransition(this PipelineStateManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);
        return manager.GetStateHistory() is var history && history.Count > 0 ? history[^1] : null;
    }

    /// <summary>
    /// Calculates the total amount of time the pipeline has spent in the specified <paramref name="state"/>
    /// based on the recorded transition history and the current state.
    /// </summary>
    /// <param name="manager">The <see cref="PipelineStateManager"/> instance.</param>
    /// <param name="state">The state for which to calculate total elapsed time.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the cumulative duration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manager"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is <c>null</c>.</exception>
    public static TimeSpan GetTotalTimeInState(this PipelineStateManager manager, PipelineState state)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(state);

        var history = manager.GetStateHistory();

        if (history.Count == 0)
        {
            // No transitions recorded; if the current state matches, count from start of the process.
            return manager.CurrentState == state ? manager.GetCurrentStateDuration() : TimeSpan.Zero;
        }

        // Ensure history is ordered by timestamp (it should already be, but guard just in case).
        var ordered = history.OrderBy(t => t.Timestamp).ToArray();

        TimeSpan total = TimeSpan.Zero;
        PipelineState previousState = ordered[0].FromState;
        DateTime previousTimestamp = ordered[0].Timestamp;

        // Iterate over each transition, adding time spent in the previous state.
        foreach (var transition in ordered)
        {
            var duration = transition.Timestamp - previousTimestamp;
            if (previousState == state)
                total += duration;

            previousState = transition.ToState;
            previousTimestamp = transition.Timestamp;
        }

        // Account for time from the last transition to now if the pipeline is still in the queried state.
        if (manager.CurrentState == state)
            total += DateTime.UtcNow - previousTimestamp;

        return total;
    }

    /// <summary>
    /// Returns a concise, culture‑invariant string that summarizes the transition history.
    /// Each line contains: <c>Timestamp | FromState → ToState | Reason</c>.
    /// </summary>
    /// <param name="manager">The <see cref="PipelineStateManager"/> instance.</param>
    /// <returns>A multi‑line string representation of the history.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manager"/> is <c>null</c>.</exception>
    public static string ToHistoryString(this PipelineStateManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);
        var history = manager.GetStateHistory()
            .OrderBy(t => t.Timestamp)
            .Select(t =>
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0:O} | {1} → {2} | {3}",
                    t.Timestamp,
                    t.FromState,
                    t.ToState,
                    string.IsNullOrEmpty(t.Reason) ? "<no reason>" : t.Reason));

        return string.Join(Environment.NewLine, history);
    }
}