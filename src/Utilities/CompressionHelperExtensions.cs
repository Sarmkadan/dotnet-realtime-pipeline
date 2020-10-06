#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="CompressionHelper"/>.
/// </summary>
public static class CompressionHelperExtensions
{
    /// <summary>
    /// Compresses a file synchronously using GZIP.
    /// </summary>
    /// <param name="inputPath">The path to the input file.</param>
    /// <param name="outputPath">The path to the output file.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="inputPath"/> or <paramref name="outputPath"/> is null or empty.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    public static void CompressFile(string inputPath, string outputPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(inputPath, nameof(inputPath));
        ArgumentException.ThrowIfNullOrEmpty(outputPath, nameof(outputPath));

        using (var inputFile = new FileStream(inputPath, FileMode.Open))
        {
            using (var outputFile = new FileStream(outputPath, FileMode.Create))
            {
                using (var gzipStream = new GZipStream(outputFile, CompressionMode.Compress))
                {
                    inputFile.CopyTo(gzipStream);
                }
            }
        }
    }

    /// <summary>
    /// Calculates the compression ratio for a byte array.
    /// </summary>
    /// <param name="compressionHelper">The <see cref="CompressionHelper"/> instance.</param>
    /// <param name="data">The byte array to calculate the compression ratio for.</param>
    /// <returns>The compression ratio.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="compressionHelper"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public static double CalculateCompressionRatio(this CompressionHelper compressionHelper, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(compressionHelper);
        ArgumentNullException.ThrowIfNull(data);

        var compressed = CompressionHelper.CompressGzip(Encoding.UTF8.GetString(data));
        return (double)compressed.Length / data.Length;
    }

    /// <summary>
    /// Compresses a stream using GZIP.
    /// </summary>
    /// <param name="compressionHelper">The <see cref="CompressionHelper"/> instance.</param>
    /// <param name="inputStream">The input stream to compress.</param>
    /// <returns>The compressed stream.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="compressionHelper"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inputStream"/> is null.</exception>
    public static MemoryStream CompressStream(this CompressionHelper compressionHelper, Stream inputStream)
    {
        ArgumentNullException.ThrowIfNull(compressionHelper);
        ArgumentNullException.ThrowIfNull(inputStream);

        using (var outputStream = new MemoryStream())
        {
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                inputStream.CopyTo(gzipStream);
            }

            outputStream.Position = 0;
            return outputStream;
        }
    }
}
