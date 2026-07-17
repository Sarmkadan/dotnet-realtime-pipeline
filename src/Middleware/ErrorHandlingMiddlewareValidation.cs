#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Middleware;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Provides validation extensions for <see cref="ErrorHandlingMiddleware"/> instances.
/// </summary>
public static class ErrorHandlingMiddlewareValidation
{
    /// <summary>
    /// Validates the specified <see cref="ErrorHandlingMiddleware"/> instance.
    /// </summary>
    /// <param name="value">The <see cref="ErrorHandlingMiddleware"/> instance to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ErrorHandlingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate error mappers dictionary
        var errorMappers = value.GetErrorMappers();
        if (errorMappers == null)
        {
            errors.Add("Error mappers dictionary cannot be null.");
            return errors.AsReadOnly();
        }

        if (errorMappers.Count == 0)
        {
            errors.Add("Error mappers dictionary must contain at least one mapper.");
        }

        // Validate default mappers are registered correctly
        var requiredMappers = new[]
        {
            typeof(DotNetRealtimePipeline.Domain.Exceptions.PipelineException),
            typeof(TimeoutException),
            typeof(InvalidOperationException),
            typeof(ArgumentException)
        };

        foreach (var requiredType in requiredMappers)
        {
            if (!errorMappers.ContainsKey(requiredType))
            {
                errors.Add($"Required error mapper for type '{requiredType.Name}' is missing.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Gets the error mappers dictionary from the middleware instance using reflection.
    /// </summary>
    /// <param name="middleware">The middleware instance.</param>
    /// <returns>The error mappers dictionary or null if not accessible.</returns>
    private static Dictionary<Type, Func<Exception, ErrorResponse>>? GetErrorMappers(this ErrorHandlingMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        try
        {
            var field = typeof(ErrorHandlingMiddleware).GetField(
                "_errorMappers",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(middleware) as Dictionary<Type, Func<Exception, ErrorResponse>>;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="ErrorHandlingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="ErrorHandlingMiddleware"/> instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ErrorHandlingMiddleware value)
    {
        return !value.Validate().Any();
    }

    /// <summary>
    /// Ensures that the specified <see cref="ErrorHandlingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="ErrorHandlingMiddleware"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this ErrorHandlingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count is not 0)
        {
            throw new ArgumentException(
                $"ErrorHandlingMiddleware is not valid. Errors:{Environment.NewLine}- {
                    string.Join(
                        $"\n- ",
                        errors
                    )
                }",
                nameof(value)
            );
        }
    }
}