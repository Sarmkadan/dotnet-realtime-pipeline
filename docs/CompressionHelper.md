# CompressionHelper
The `CompressionHelper` class provides a set of static methods and properties to assist with compressing and decompressing data using Gzip and Deflate algorithms. It allows for the calculation of compression ratios, comparison of algorithms, and analysis of compression statistics. This class is designed to be used in a variety of scenarios where data compression is necessary, such as in data storage, networking, or real-time data processing applications.

## API
* `public static byte[] CompressGzip(byte[] data)`: Compresses the input data using the Gzip algorithm. Returns the compressed data as a byte array. Throws an exception if the input data is null.
* `public static string DecompressGzip(byte[] compressedData)`: Decompresses the input data using the Gzip algorithm. Returns the decompressed data as a string. Throws an exception if the input data is null or if decompression fails.
* `public static byte[] CompressDeflate(byte[] data)`: Compresses the input data using the Deflate algorithm. Returns the compressed data as a byte array. Throws an exception if the input data is null.
* `public static string DecompressDeflate(byte[] compressedData)`: Decompresses the input data using the Deflate algorithm. Returns the decompressed data as a string. Throws an exception if the input data is null or if decompression fails.
* `public static async Task CompressFileAsync(string filePath, string compressedFilePath)`: Compresses a file using the Gzip algorithm. The compressed file is saved to the specified path. Throws an exception if the input file does not exist or if compression fails.
* `public static async Task DecompressFileAsync(string compressedFilePath, string decompressedFilePath)`: Decompresses a file using the Gzip algorithm. The decompressed file is saved to the specified path. Throws an exception if the input file does not exist or if decompression fails.
* `public static double CalculateCompressionRatio(int originalSize, int compressedSize)`: Calculates the compression ratio based on the original and compressed sizes. Returns the compression ratio as a double value.
* `public static CompressionStats AnalyzeCompression(byte[] originalData, byte[] compressedData)`: Analyzes the compression statistics based on the original and compressed data. Returns a `CompressionStats` object containing the compression statistics.
* `public static CompressionComparison CompareAlgorithms(byte[] originalData, byte[] gzipCompressedData, byte[] deflateCompressedData)`: Compares the compression algorithms based on the original and compressed data. Returns a `CompressionComparison` object containing the comparison results.
* `public int OriginalSizeBytes { get; }`: Gets the original size of the data in bytes.
* `public int CompressedSizeBytes { get; }`: Gets the compressed size of the data in bytes.
* `public int SavingsBytes { get; }`: Gets the savings in bytes due to compression.
* `public double CompressionRatioPercent { get; }`: Gets the compression ratio as a percentage.
* `public string CompressionAlgorithm { get; }`: Gets the compression algorithm used.
* `public override string ToString()`: Returns a string representation of the compression statistics.
* `public int GzipSizeBytes { get; }`: Gets the size of the data compressed using the Gzip algorithm in bytes.
* `public int DeflateSizeBytes { get; }`: Gets the size of the data compressed using the Deflate algorithm in bytes.
* `public string BestAlgorithm { get; }`: Gets the best compression algorithm based on the compression statistics.

## Usage
```csharp
// Example 1: Compressing and decompressing data
byte[] originalData = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
byte[] compressedData = CompressionHelper.CompressGzip(originalData);
string decompressedData = CompressionHelper.DecompressGzip(compressedData);
Console.WriteLine(decompressedData); // Output: Hello, World!

// Example 2: Analyzing compression statistics
byte[] originalData2 = System.Text.Encoding.UTF8.GetBytes("This is a sample text.");
byte[] gzipCompressedData = CompressionHelper.CompressGzip(originalData2);
byte[] deflateCompressedData = CompressionHelper.CompressDeflate(originalData2);
CompressionComparison comparison = CompressionHelper.CompareAlgorithms(originalData2, gzipCompressedData, deflateCompressedData);
Console.WriteLine(comparison.BestAlgorithm); // Output: Gzip or Deflate, depending on the compression ratios
```

## Notes
* The `CompressionHelper` class is designed to be thread-safe, allowing for concurrent compression and decompression operations.
* The `CompressFileAsync` and `DecompressFileAsync` methods are asynchronous, allowing for non-blocking file compression and decompression operations.
* The `CalculateCompressionRatio` method may return a negative value if the compressed size is larger than the original size, indicating that compression was not effective.
* The `AnalyzeCompression` method may throw an exception if the input data is null or if the compression statistics cannot be calculated.
* The `CompareAlgorithms` method may return a `CompressionComparison` object with a `BestAlgorithm` property set to null if the compression ratios are equal or if the comparison fails.
