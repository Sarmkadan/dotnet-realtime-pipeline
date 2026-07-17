#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="PerformanceHelper"/> results and measurements.
/// </summary>
public static class PerformanceHelperValidation
{
    /// <summary>
    /// Validates a <see cref="BenchmarkResult"/> instance.
    /// </summary>
    /// <param name="result">The benchmark result to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this BenchmarkResult? result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var problems = new List<string>();

        if (result.Iterations <= 0)
        {
            problems.Add($"Iterations must be positive, but was {result.Iterations}.");
        }

        if (result.Measurements is null)
        {
            problems.Add("Measurements collection cannot be null.");
        }
        else if (result.Measurements.Count == 0)
        {
            problems.Add("Measurements collection cannot be empty.");
        }
        else
        {
            foreach (var measurement in result.Measurements)
            {
                if (measurement < 0)
                {
                    problems.Add($"Measurement value cannot be negative, but found {measurement}ms.");
                }
            }
        }

        if (double.IsNaN(result.AverageMs) || double.IsInfinity(result.AverageMs))
        {
            problems.Add("AverageMs must be a valid number.");
        }
        else if (result.AverageMs < 0)
        {
            problems.Add($"AverageMs cannot be negative, but was {result.AverageMs:F2}ms.");
        }

        if (result.MinMs < 0)
        {
            problems.Add($"MinMs cannot be negative, but was {result.MinMs}ms.");
        }

        if (result.MaxMs < 0)
        {
            problems.Add($"MaxMs cannot be negative, but was {result.MaxMs}ms.");
        }

        if (result.MedianMs < 0 || double.IsNaN(result.MedianMs) || double.IsInfinity(result.MedianMs))
        {
            problems.Add("MedianMs must be a valid non-negative number.");
        }

        if (result.P95Ms < 0 || double.IsNaN(result.P95Ms) || double.IsInfinity(result.P95Ms))
        {
            problems.Add("P95Ms must be a valid non-negative number.");
        }

        if (result.P99Ms < 0 || double.IsNaN(result.P99Ms) || double.IsInfinity(result.P99Ms))
        {
            problems.Add("P99Ms must be a valid non-negative number.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="MemoryStats"/> instance.
    /// </summary>
    /// <param name="stats">The memory statistics to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stats"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this MemoryStats? stats)
    {
        ArgumentNullException.ThrowIfNull(stats);

        var problems = new List<string>();

        if (stats.WorkingSetMb < 0 || double.IsNaN(stats.WorkingSetMb) || double.IsInfinity(stats.WorkingSetMb))
        {
            problems.Add("WorkingSetMb must be a valid non-negative number.");
        }

        if (stats.PrivateMemoryMb < 0 || double.IsNaN(stats.PrivateMemoryMb) || double.IsInfinity(stats.PrivateMemoryMb))
        {
            problems.Add("PrivateMemoryMb must be a valid non-negative number.");
        }

        if (stats.PeakWorkingSetMb < 0 || double.IsNaN(stats.PeakWorkingSetMb) || double.IsInfinity(stats.PeakWorkingSetMb))
        {
            problems.Add("PeakWorkingSetMb must be a valid non-negative number.");
        }

        if (stats.GC0Collections < 0)
        {
            problems.Add($"GC0Collections cannot be negative, but was {stats.GC0Collections}.");
        }

        if (stats.GC1Collections < 0)
        {
            problems.Add($"GC1Collections cannot be negative, but was {stats.GC1Collections}.");
        }

        if (stats.GC2Collections < 0)
        {
            problems.Add($"GC2Collections cannot be negative, but was {stats.GC2Collections}.");
        }

        if (stats.TotalMemoryMb < 0 || double.IsNaN(stats.TotalMemoryMb) || double.IsInfinity(stats.TotalMemoryMb))
        {
            problems.Add("TotalMemoryMb must be a valid non-negative number.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="BenchmarkResult"/> is valid.
    /// </summary>
    /// <param name="result">The benchmark result to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this BenchmarkResult? result) => Validate(result).Count == 0;

    /// <summary>
    /// Determines whether the specified <see cref="MemoryStats"/> is valid.
    /// </summary>
    /// <param name="stats">The memory statistics to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this MemoryStats? stats) => Validate(stats).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="BenchmarkResult"/> is valid, throwing an exception if not.
    /// </summary>
    /// <param name="result">The benchmark result to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the result is invalid.</exception>
    public static void EnsureValid(this BenchmarkResult? result)
    {
        var problems = Validate(result);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"BenchmarkResult is invalid. Problems:\n{string.Join("\n", problems)}");
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="MemoryStats"/> is valid, throwing an exception if not.
    /// </summary>
    /// <param name="stats">The memory statistics to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the stats are invalid.</exception>
    public static void EnsureValid(this MemoryStats? stats)
    {
        var problems = Validate(stats);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"MemoryStats are invalid. Problems:\n{string.Join("\n", problems)}");
        }
    }

    /// <summary>
    /// Validates the execution result from <see cref="PerformanceHelper.MeasureExecution{T}"/>.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="executionResult">The execution result to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="executionResult"/> is null.</exception>
    public static IReadOnlyList<string> Validate<T>(
        this (T Result, long ElapsedMs) executionResult)
    {
        ArgumentNullException.ThrowIfNull(executionResult);

        var problems = new List<string>();

        if (executionResult.ElapsedMs < 0)
        {
            problems.Add($"ElapsedMs cannot be negative, but was {executionResult.ElapsedMs}ms.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified execution result is valid.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="executionResult">The execution result to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid<T>(
        this (T Result, long ElapsedMs) executionResult) => Validate(executionResult).Count == 0;

    /// <summary>
    /// Ensures that the specified execution result is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="executionResult">The execution result to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the result is invalid.</exception>
    public static void EnsureValid<T>(
        this (T Result, long ElapsedMs) executionResult)
    {
        var problems = Validate(executionResult);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Execution result is invalid. Problems:\n{string.Join("\n", problems)}");
        }
    }

    /// <summary>
    /// Validates the execution result from <see cref="PerformanceHelper.MeasureExecutionAsync{T}"/>.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="executionResult">The execution result to validate.</param>
    /// <param name="expectedResult">Optional expected result to compare against.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="executionResult"/> is null.</exception>
    public static IReadOnlyList<string> Validate<T>(
        this (T Result, long ElapsedMs) executionResult,
        T? expectedResult = default)
    {
        ArgumentNullException.ThrowIfNull(executionResult);

        var problems = new List<string>();

        if (executionResult.ElapsedMs < 0)
        {
            problems.Add($"ElapsedMs cannot be negative, but was {executionResult.ElapsedMs}ms.");
        }

        if (expectedResult is not null && !EqualityComparer<T>.Default.Equals(executionResult.Result, expectedResult))
        {
            problems.Add("Result does not match expected value.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified async execution result is valid.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="executionResult">The execution result to validate.</param>
    /// <param name="expectedResult">Optional expected result to compare against.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid<T>(
        this (T Result, long ElapsedMs) executionResult,
        T? expectedResult = default) => Validate(executionResult, expectedResult).Count == 0;

    /// <summary>
    /// Ensures that the specified async execution result is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="executionResult">The execution result to validate.</param>
    /// <param name="expectedResult">Optional expected result to compare against.</param>
    /// <exception cref="ArgumentException">Thrown when the result is invalid.</exception>
    public static void EnsureValid<T>(
        this (T Result, long ElapsedMs) executionResult,
        T? expectedResult = default)
    {
        var problems = Validate(executionResult, expectedResult);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Async execution result is invalid. Problems:\n{string.Join("\n", problems)}");
        }
    }
}