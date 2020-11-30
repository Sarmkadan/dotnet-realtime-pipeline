#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetRealtimePipeline.CLI;

/// <summary>
/// Provides validation helpers for <see cref="CommandExecutor"/> instances.
/// </summary>
public static class CommandExecutorValidation
{
    /// <summary>
    /// Returns a read‑only list of validation problems for the supplied <see cref="CommandExecutor"/>.
    /// </summary>
    /// <param name="value">The <see cref="CommandExecutor"/> instance to validate.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> of human‑readable problem descriptions.
    /// The list is empty when the instance is considered valid.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Never thrown by this method; it always returns a list.
    /// </exception>
    public static IReadOnlyList<string> Validate(this CommandExecutor? value)
    {
        var problems = new List<string>();

        if (value is null)
        {
            problems.Add("CommandExecutor instance is null.");
            return problems;
        }

        // The three constructor‑injected dependencies must be non‑null.
        // They are private fields, so we inspect them via reflection.
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var orchestratorField = typeof(CommandExecutor).GetField("_orchestrator", flags);
        var loggerField       = typeof(CommandExecutor).GetField("_logger", flags);
        var visualizerField   = typeof(CommandExecutor).GetField("_visualizer", flags);

        if (orchestratorField?.GetValue(value) is null)
            problems.Add("Orchestrator dependency is null.");

        if (loggerField?.GetValue(value) is null)
            problems.Add("Logger dependency is null.");

        if (visualizerField?.GetValue(value) is null)
            problems.Add("Visualizer dependency is null.");

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the supplied <see cref="CommandExecutor"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to test.</param>
    /// <returns><c>true</c> if no validation problems are reported; otherwise <c>false</c>.</returns>
    public static bool IsValid(this CommandExecutor? value) => !value.Validate().Any();

    /// <summary>
    /// Ensures that the supplied <see cref="CommandExecutor"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more validation problems are detected. The exception message
    /// contains a semicolon‑separated list of the problems.
    /// </exception>
    public static void EnsureValid(this CommandExecutor? value)
    {
        var problems = value.Validate();
        if (problems.Any())
        {
            var message = $"CommandExecutor validation failed: {string.Join("; ", problems)}";
            throw new ArgumentException(message, nameof(value));
        }
    }
}
