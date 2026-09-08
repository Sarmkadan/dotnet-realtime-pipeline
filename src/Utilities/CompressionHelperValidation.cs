#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="CompressionHelper"/> operations.
/// </summary>
public static class CompressionHelperValidation
{
    /// <summary>
    /// Verifies that a <see cref="CompressionHelper"/> instance is not <see langword="null"/>.
    /// </summary>
    /// <param name="value">The compression helper instance to validate.</param>
    /// <returns>An empty list when <paramref name="value"/> is not <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CompressionHelper? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CompressionHelper"/> instance is not <see langword="null"/>.
    /// </summary>
    /// <param name="value">The compression helper instance to check.</param>
    /// <returns><see langword="true"/> when <paramref name="value"/> is not <see langword="null"/>; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this CompressionHelper? value) => value is not null;

    /// <summary>
    /// Ensures that the specified <see cref="CompressionHelper"/> instance is not <see langword="null"/>.
    /// </summary>
    /// <param name="value">The compression helper instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this CompressionHelper? value) => ArgumentNullException.ThrowIfNull(value);

    /// <summary>
    /// Checks whether data supplied for Gzip compression is null or empty.
    /// </summary>
    /// <param name="data">The data to compress.</param>
    /// <returns>A validation message when <paramref name="data"/> is null or empty; otherwise, an empty list.</returns>
    public static IReadOnlyList<string> ValidateForCompressGzip(this string? data)
    {
        return string.IsNullOrEmpty(data)
            ? new List<string> { "Data cannot be null or empty for compression." }
            : Array.Empty<string>();
    }

    /// <summary>
    /// Checks whether data supplied for Gzip decompression is null or empty.
    /// </summary>
    /// <param name="compressedData">The compressed data to decompress.</param>
    /// <returns>A validation message when <paramref name="compressedData"/> is null or empty; otherwise, an empty list.</returns>
    public static IReadOnlyList<string> ValidateForDecompressGzip(this byte[]? compressedData)
    {
        if (compressedData is null)
        {
            return new List<string> { "Compressed data cannot be null." };
        }

        if (compressedData.Length == 0)
        {
            return new List<string> { "Compressed data cannot be empty." };
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks whether data supplied for Deflate compression is null or empty.
    /// </summary>
    /// <param name="data">The data to compress.</param>
    /// <returns>A validation message when <paramref name="data"/> is null or empty; otherwise, an empty list.</returns>
    public static IReadOnlyList<string> ValidateForCompressDeflate(this string? data)
    {
        return string.IsNullOrEmpty(data)
            ? new List<string> { "Data cannot be null or empty for compression." }
            : Array.Empty<string>();
    }

    /// <summary>
    /// Checks whether data supplied for Deflate decompression is null or empty.
    /// </summary>
    /// <param name="compressedData">The compressed data to decompress.</param>
    /// <returns>A validation message when <paramref name="compressedData"/> is null or empty; otherwise, an empty list.</returns>
    public static IReadOnlyList<string> ValidateForDecompressDeflate(this byte[]? compressedData)
    {
        if (compressedData is null)
        {
            return new List<string> { "Compressed data cannot be null." };
        }

        if (compressedData.Length == 0)
        {
            return new List<string> { "Compressed data cannot be empty." };
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks whether the input and output paths supplied for file compression are null or empty.
    /// </summary>
    /// <param name="inputPath">The path to the input file.</param>
    /// <param name="outputPath">The path to the output file.</param>
    /// <returns>Validation messages for each null or empty path; otherwise, an empty list.</returns>
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

        return errors;
    }

    /// <summary>
    /// Checks whether the input and output paths supplied for file decompression are null or empty.
    /// </summary>
    /// <param name="inputPath">The path to the input file.</param>
    /// <param name="outputPath">The path to the output file.</param>
    /// <returns>Validation messages for each null or empty path; otherwise, an empty list.</returns>
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

        return errors;
    }

    /// <summary>
    /// Checks whether the original data supplied for compression-ratio calculation is null or empty.
    /// </summary>
    /// <param name="originalData">The original data to calculate ratio for.</param>
    /// <returns>A validation message when <paramref name="originalData"/> is null or empty; otherwise, an empty list.</returns>
    public static IReadOnlyList<string> ValidateForCalculateCompressionRatio(this string? originalData)
    {
        return string.IsNullOrEmpty(originalData)
            ? new List<string> { "Original data cannot be null or empty for compression ratio calculation." }
            : Array.Empty<string>();
    }

    /// <summary>
    /// Checks whether data supplied for compression analysis is null or empty.
    /// </summary>
    /// <param name="data">The data to analyze.</param>
    /// <returns>A validation message when <paramref name="data"/> is null or empty; otherwise, an empty list.</returns>
    public static IReadOnlyList<string> ValidateForAnalyzeCompression(this string? data)
    {
        return string.IsNullOrEmpty(data)
            ? new List<string> { "Data cannot be null or empty for compression analysis." }
            : Array.Empty<string>();
    }

    /// <summary>
    /// Checks whether data supplied for algorithm comparison is null or empty.
    /// </summary>
    /// <param name="data">The data to compare algorithms for.</param>
    /// <returns>A validation message when <paramref name="data"/> is null or empty; otherwise, an empty list.</returns>
    public static IReadOnlyList<string> ValidateForCompareAlgorithms(this string? data)
    {
        return string.IsNullOrEmpty(data)
            ? new List<string> { "Data cannot be null or empty for algorithm comparison." }
            : Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified data is neither null nor empty according to both compression validators.
    /// </summary>
    /// <param name="data">The data to check.</param>
    /// <returns><see langword="true"/> when <paramref name="data"/> is neither null nor empty; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidForCompression(this string? data)
        => ValidateForCompressGzip(data).Count == 0 && ValidateForCompressDeflate(data).Count == 0;

    /// <summary>
    /// Ensures that the specified data is neither null nor empty according to both compression validators.
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
    /// Determines whether the specified compressed data is neither null nor empty according to both decompression validators.
    /// </summary>
    /// <param name="compressedData">The compressed data to check.</param>
    /// <returns><see langword="true"/> when <paramref name="compressedData"/> is neither null nor empty; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidForDecompression(this byte[]? compressedData)
        => ValidateForDecompressGzip(compressedData).Count == 0 && ValidateForDecompressDeflate(compressedData).Count == 0;

    /// <summary>
    /// Ensures that the specified compressed data is neither null nor empty according to both decompression validators.
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
