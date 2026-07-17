#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DotNetRealtimePipeline.CLI;

/// <summary>
/// Provides validation helpers for <see cref="CommandLineParser"/> instances.
/// </summary>
public static class CommandLineParserValidation
{
    private const string CommandRegistryFieldName = "_commandRegistry";

    /// <summary>
    /// Validates the state of a <see cref="CommandLineParser"/> and returns a collection of human-readable problems.
    /// </summary>
    /// <param name="value">The parser to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> containing validation error messages; empty if the parser is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this CommandLineParser value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            // Use reflection to inspect the private command registry.
            var field = typeof(CommandLineParser).GetField(
                CommandRegistryFieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (field is null)
            {
                problems.Add("Unable to locate internal command registry.");
                return problems;
            }

            if (field.GetValue(value) is not IDictionary<string, Func<ParsedCommand>> registry)
            {
                problems.Add("Internal command registry is null or of incorrect type.");
                return problems;
            }

            if (registry.Count == 0)
            {
                problems.Add("No commands are registered with the parser.");
            }
            else
            {
                // Ensure each factory delegate is non-null.
                foreach (var (verb, factory) in registry)
                {
                    if (factory is null)
                    {
                        problems.Add($"Command factory for verb '{verb}' is null.");
                    }
                }
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or TargetInvocationException)
        {
            problems.Add($"Reflection failed while accessing command registry: {ex.Message}");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified <see cref="CommandLineParser"/> instance is valid.
    /// </summary>
    /// <param name="value">The parser to evaluate.</param>
    /// <returns><c>true</c> if the parser has no validation problems; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this CommandLineParser value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="CommandLineParser"/> instance is valid.
    /// </summary>
    /// <param name="value">The parser to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the parser contains validation problems; the exception message lists all problems.</exception>
    public static void EnsureValid(this CommandLineParser value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            var message = $"CommandLineParser validation failed: {string.Join("; ", problems)}";
            throw new ArgumentException(message, nameof(value));
        }
    }
}
