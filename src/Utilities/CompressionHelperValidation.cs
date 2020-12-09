#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="CompressionHelper"/> operations.
/// </summary>
public static class CompressionHelperValidation
{
    /// <summary>
    /// Validates a <see cref="CompressionHelper"/> instance.
    /// </summary>
    /// <param name="value">The compression helper instance to validate.</param>
    /// <returns>A list of validation messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CompressionHelper? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CompressionHelper"/> instance is valid.
    /// </summary>
    /// <param name="value">The compression helper instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this CompressionHelper? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="CompressionHelper"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The compression helper instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this CompressionHelper? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"CompressionHelper validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Validates parameters for <see cref="CompressionHelper.CompressGzip"/> method.
    /// </summary>
    /// <param name="data">The data to compress.</param>
    /// <returns>A list of validation messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForCompressGzip(this string? data)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(data))
        {
            errors.Add("Data cannot be null or empty for compression.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="CompressionHelper.DecompressGzip"/> method.
    /// </summary>
    /// <param name="compressedData">The compressed data to decompress.</param>
    /// <returns>A list of validation messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForDecompressGzip(this byte[]? compressedData)
    {
        var errors = new List<string>();

        if (compressedData is null)
        {
            errors.Add("Compressed data cannot be null.");
        }
        else if (compressedData.Length == 0)
        {
            errors.Add("Compressed data cannot be empty.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="CompressionHelper.CompressDeflate"/> method.
    /// </summary>
    /// <param name="data">The data to compress.</param>
    /// <returns>A list of validation messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForCompressDeflate(this string? data)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(data))
        {
            errors.Add("Data cannot be null or empty for compression.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="CompressionHelper.DecompressDeflate"/> method.
    /// </summary>
    /// <param name="compressedData">The compressed data to decompress.</param>
    /// <returns>A list of validation messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForDecompressDeflate(this byte[]? compressedData)
    {
        var errors = new List<string>();

        if (compressedData is null)
        {
            errors.Add("Compressed data cannot be null.");
        }
        else if (compressedData.Length == 0)
        {
            errors.Add("Compressed data cannot be empty.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="CompressionHelper.CompressFileAsync"/> method.
    /// </summary>
    /// <param name="inputPath">The path to the input file.</param>
    /// <param name="outputPath">The path to the output file.</param>
    /// <returns>A list of validation messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForCompressFileAsync(
        this string? inputPath,
        string? outputPath)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(inputPath))
        {
            errors.Add("Input file path cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            errors.Add("Output file path cannot be null or empty.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="CompressionHelper.DecompressFileAsync"/> method.
    /// </summary>
    /// <param name="inputPath">The path to the input file.</param>
    /// <param name="outputPath">The path to the output file.</param>
    /// <returns>A list of validation messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForDecompressFileAsync(
        this string? inputPath,
        string? outputPath)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(inputPath))
        {
            errors.Add("Input file path cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            errors.Add("Output file path cannot be null or empty.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="CompressionHelper.CalculateCompressionRatio"/> method.
    /// </summary>
    /// <param name="originalData">The original data to calculate ratio for.</param>
    /// <returns>A list of validation messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForCalculateCompressionRatio(this string? originalData)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(originalData))
        {
            errors.Add("Original data cannot be null or empty for compression ratio calculation.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="CompressionAnalyzer.AnalyzeCompression"/> method.
    /// </summary>
    /// <param name="data">The data to analyze.</param>
    /// <returns>A list of validation messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForAnalyzeCompression(this string? data)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(data))
        {
            errors.Add("Data cannot be null or empty for compression analysis.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="CompressionAnalyzer.CompareAlgorithms"/> method.
    /// </summary>
    /// <param name="data">The data to compare algorithms for.</param>
    /// <returns>A list of validation messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForCompareAlgorithms(this string? data)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(data))
        {
            errors.Add("Data cannot be null or empty for algorithm comparison.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified data is valid for compression operations.
    /// </summary>
    /// <param name="data">The data to check.</param>
    /// <returns>True if the data is valid for compression; otherwise, false.</returns>
    public static bool IsValidForCompression(this string? data)
    {
        return ValidateForCompressGzip(data).Count == 0 && ValidateForCompressDeflate(data).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified data is valid for compression operations, throwing an exception if not.
    /// </summary>
    /// <param name="data">The data to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the data is not valid for compression.</exception>
    public static void EnsureValidForCompression(this string? data)
    {
        var errors = new List<string>();
        errors.AddRange(ValidateForCompressGzip(data));
        errors.AddRange(ValidateForCompressDeflate(data));

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Data validation failed for compression:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Determines whether the specified compressed data is valid for decompression operations.
    /// </summary>
    /// <param name="compressedData">The compressed data to check.</param>
    /// <returns>True if the compressed data is valid; otherwise, false.</returns>
    public static bool IsValidForDecompression(this byte[]? compressedData)
    {
        return ValidateForDecompressGzip(compressedData).Count == 0 && ValidateForDecompressDeflate(compressedData).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified compressed data is valid for decompression operations, throwing an exception if not.
    /// </summary>
    /// <param name="compressedData">The compressed data to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the compressed data is not valid for decompression.</exception>
    public static void EnsureValidForDecompression(this byte[]? compressedData)
    {
        var errors = new List<string>();
        errors.AddRange(ValidateForDecompressGzip(compressedData));
        errors.AddRange(ValidateForDecompressDeflate(compressedData));

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Compressed data validation failed for decompression:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}