#nullable enable

using System;
using System.Collections.Generic;

namespace DotNetRealtimePipeline.CLI;

/// <summary>
/// Provides validation helpers for <see cref="CommandExecutor"/> instances.
/// </summary>
public static class CommandExecutorValidation
{
    /// <summary>
    /// Returns a read-only list of validation problems for the supplied <see cref="CommandExecutor"/>.
    /// </summary>
    /// <param name="value">The <see cref="CommandExecutor"/> instance to validate.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> of human-readable problem descriptions.
    /// The list is empty when the instance is considered valid.
    /// </returns>
    /// <remarks>
    /// This validation trusts that the constructor's null checks are sufficient,
    /// as <see cref="CommandExecutor"/> validates its dependencies during construction.
    /// </remarks>
    public static IReadOnlyList<string> Validate(this CommandExecutor? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the supplied <see cref="CommandExecutor"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to test.</param>
    /// <returns><c>true</c> if no validation problems are reported; otherwise <c>false</c>.</returns>
    public static bool IsValid(this CommandExecutor? value) => value is not null;

    /// <summary>
    /// Ensures that the supplied <see cref="CommandExecutor"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <c>null</c>.
    /// </exception>
    public static void EnsureValid(this CommandExecutor? value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}