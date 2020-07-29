#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Helper class for data compression and decompression.
/// Supports GZIP and Deflate algorithms with streaming support for large datasets.
/// </summary>
public sealed class CompressionHelper
{
    /// <summary>
    /// Compresses a string using GZIP compression.
    /// </summary>
    public static byte[] CompressGzip(string data)
    {
        if (string.IsNullOrEmpty(data))
            return Array.Empty<byte>();

        var inputBytes = Encoding.UTF8.GetBytes(data);

        using (var outputStream = new MemoryStream())
        {
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                gzipStream.Write(inputBytes, 0, inputBytes.Length);
            }

            return outputStream.ToArray();
        }
    }

    /// <summary>
    /// Decompresses a GZIP-compressed byte array.
    /// </summary>
    public static string DecompressGzip(byte[] compressedData)
    {
        if (compressedData is null || compressedData.Length == 0)
            return string.Empty;

        using (var inputStream = new MemoryStream(compressedData))
        {
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                using (var outputStream = new MemoryStream())
                {
                    gzipStream.CopyTo(outputStream);
                    return Encoding.UTF8.GetString(outputStream.ToArray());
                }
            }
        }
    }

    /// <summary>
    /// Compresses a string using Deflate compression.
    /// </summary>
    public static byte[] CompressDeflate(string data)
    {
        if (string.IsNullOrEmpty(data))
            return Array.Empty<byte>();

        var inputBytes = Encoding.UTF8.GetBytes(data);

        using (var outputStream = new MemoryStream())
        {
            using (var deflateStream = new DeflateStream(outputStream, CompressionMode.Compress))
            {
                deflateStream.Write(inputBytes, 0, inputBytes.Length);
            }

            return outputStream.ToArray();
        }
    }

    /// <summary>
    /// Decompresses a Deflate-compressed byte array.
    /// </summary>
    public static string DecompressDeflate(byte[] compressedData)
    {
        if (compressedData is null || compressedData.Length == 0)
            return string.Empty;

        using (var inputStream = new MemoryStream(compressedData))
        {
            using (var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
            {
                using (var outputStream = new MemoryStream())
                {
                    deflateStream.CopyTo(outputStream);
                    return Encoding.UTF8.GetString(outputStream.ToArray());
                }
            }
        }
    }

    /// <summary>
    /// Compresses a file asynchronously using GZIP.
    /// </summary>
    public static async Task CompressFileAsync(string inputPath, string outputPath)
    {
        using (var inputFile = new FileStream(inputPath, FileMode.Open))
        {
            using (var outputFile = new FileStream(outputPath, FileMode.Create))
            {
                using (var gzipStream = new GZipStream(outputFile, CompressionMode.Compress))
                {
                    await inputFile.CopyToAsync(gzipStream);
                }
            }
        }
    }

    /// <summary>
    /// Decompresses a file asynchronously using GZIP.
    /// </summary>
    public static async Task DecompressFileAsync(string inputPath, string outputPath)
    {
        using (var inputFile = new FileStream(inputPath, FileMode.Open))
        {
            using (var gzipStream = new GZipStream(inputFile, CompressionMode.Decompress))
            {
                using (var outputFile = new FileStream(outputPath, FileMode.Create))
                {
                    await gzipStream.CopyToAsync(outputFile);
                }
            }
        }
    }

    /// <summary>
    /// Calculates compression ratio for data.
    /// </summary>
    public static double CalculateCompressionRatio(string originalData)
    {
        var compressed = CompressGzip(originalData);
        var original = Encoding.UTF8.GetBytes(originalData);

        return compressed.Length > 0 ? (double)compressed.Length / original.Length : 0;
    }
}

/// <summary>
/// Helper for compression statistics and analysis.
/// </summary>
public sealed class CompressionAnalyzer
{
    /// <summary>
    /// Analyzes compression efficiency for a dataset.
    /// </summary>
    public static CompressionStats AnalyzeCompression(string data)
    {
        var originalBytes = Encoding.UTF8.GetBytes(data);
        var compressedBytes = CompressionHelper.CompressGzip(data);

        var savings = originalBytes.Length - compressedBytes.Length;
        var ratio = originalBytes.Length > 0 ? (double)savings / originalBytes.Length * 100 : 0;

        return new CompressionStats
        {
            OriginalSizeBytes = originalBytes.Length,
            CompressedSizeBytes = compressedBytes.Length,
            SavingsBytes = savings,
            CompressionRatioPercent = ratio,
            CompressionAlgorithm = "GZIP"
        };
    }

    /// <summary>
    /// Compares compression efficiency between algorithms.
    /// </summary>
    public static CompressionComparison CompareAlgorithms(string data)
    {
        var original = Encoding.UTF8.GetBytes(data).Length;
        var gzip = CompressionHelper.CompressGzip(data).Length;
        var deflate = CompressionHelper.CompressDeflate(data).Length;

        return new CompressionComparison
        {
            OriginalSizeBytes = original,
            GzipSizeBytes = gzip,
            DeflateSizeBytes = deflate,
            BestAlgorithm = gzip < deflate ? "GZIP" : "Deflate",
            GzipRatioPercent = (double)gzip / original * 100,
            DeflateRatioPercent = (double)deflate / original * 100
        };
    }
}

/// <summary>
/// Statistics for a compression operation.
/// </summary>
public sealed class CompressionStats
{
    public int OriginalSizeBytes { get; set; }
    public int CompressedSizeBytes { get; set; }
    public int SavingsBytes { get; set; }
    public double CompressionRatioPercent { get; set; }
    public string CompressionAlgorithm { get; set; }

    public override string ToString()
    {
        return $"Original: {OriginalSizeBytes:N0}B, Compressed: {CompressedSizeBytes:N0}B, Savings: {SavingsBytes:N0}B ({CompressionRatioPercent:F1}%)";
    }
}

/// <summary>
/// Comparison of different compression algorithms.
/// </summary>
public sealed class CompressionComparison
{
    public int OriginalSizeBytes { get; set; }
    public int GzipSizeBytes { get; set; }
    public int DeflateSizeBytes { get; set; }
    public string BestAlgorithm { get; set; }
    public double GzipRatioPercent { get; set; }
    public double DeflateRatioPercent { get; set; }
}
