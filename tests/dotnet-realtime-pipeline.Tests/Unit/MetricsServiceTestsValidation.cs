#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Moq;

namespace DotNetRealtimePipeline.Tests.Unit;

public static class MetricsServiceTestsValidation
{
    private const string ServiceFieldName = "_service";
    private const string RepoMockFieldName = "_repoMock";

    /// <summary>
    /// Validates the <see cref="MetricsServiceTests"/> instance for common issues.
    /// </summary>
    /// <param name="value">The test class instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this MetricsServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate that the service instance is properly initialized using reflection
        var serviceField = value.GetType().GetField(
            ServiceFieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (serviceField?.GetValue(value) is null)
        {
            errors.Add("MetricsServiceTests._service field is null");
        }

        // Validate that the repository mock is properly initialized using reflection
        var repoMockField = value.GetType().GetField(
            RepoMockFieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (repoMockField?.GetValue(value) is null)
        {
            errors.Add("MetricsServiceTests._repoMock field is null");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="MetricsServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test class instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this MetricsServiceTests? value)
    {
        return value?.Validate() is var errors && errors.Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="MetricsServiceTests"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with a detailed message if it is not.
    /// </summary>
    /// <param name="value">The test class instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this MetricsServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                message: $"MetricsServiceTests instance is not valid. Problems:\n{string.Join("\n", errors)}",
                paramName: nameof(value));
        }
    }
}
