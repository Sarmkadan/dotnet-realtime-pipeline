// existing content ...

## DateTimeExtensions
The `DateTimeExtensions` class provides utility methods for converting between `DateTime` and Unix timestamp formats, as well as calculating time window boundaries and ages. It enables easy conversion to and from Unix timestamps in milliseconds.

### Usage Examples

#### Convert to and from Unix timestamps
```csharp
var dateTime = new DateTime(2024, 9, 16, 10, 30, 0, DateTimeKind.Utc);
long unixTimestampMs = dateTime.ToUnixMilliseconds();
Console.WriteLine($"Unix timestamp: {unixTimestampMs}");

DateTime convertedDateTime = DateTimeExtensions.FromUnixMilliseconds(unixTimestampMs);
Console.WriteLine($"Converted back to DateTime: {convertedDateTime}");
```

#### Get current Unix timestamp
```csharp
long currentUnixTimestampMs = DateTimeExtensions.GetCurrentUnixMilliseconds();
Console.WriteLine($"Current Unix timestamp: {currentUnixTimestampMs}");
```

#### Calculate time window boundaries
```csharp
long timestampMs = 1721481600000; // Example timestamp
long windowSizeMs = 60000; // 1 minute window

long windowStart = DateTimeExtensions.GetWindowStart(timestampMs, windowSizeMs);
long windowEnd = DateTimeExtensions.GetWindowEnd(timestampMs, windowSizeMs);

Console.WriteLine($"Window start: {windowStart}");
Console.WriteLine($"Window end: {windowEnd}");
```

#### Calculate age of a timestamp
```csharp
long oldTimestampMs = 1721474400000; // Example old timestamp
long ageMs = DateTimeExtensions.GetAgeMs(oldTimestampMs);
Console.WriteLine($"Age: {ageMs}ms");
```

#### Round to window boundary
```csharp
long timestampMs = 172148123456; // Example timestamp
long windowSizeMs = 300000; // 5 minute window

long roundedTimestampMs = DateTimeExtensions.RoundToWindowBoundary(timestampMs, windowSizeMs);
Console.WriteLine($"Rounded to window boundary: {roundedTimestampMs}");
```

## CompressionHelper
The `CompressionHelper` class provides methods for compressing and decompressing data using GZIP and Deflate algorithms, along with file-level compression/decompression and compression efficiency analysis. It supports string and file operations, and includes utilities to calculate compression ratios and compare algorithm performance.

### Usage Examples

#### Compress and decompress data
```csharp
string originalData = "This is a sample text to compress.";

// Compress using GZIP
byte[] compressedGzip = CompressionHelper.CompressGzip(originalData);
string decompressedGzip = CompressionHelper.DecompressGzip(compressedGzip);
Console.WriteLine($"GZIP Decompressed: {decompressedGzip}");

// Compress using Deflate
byte[] compressedDeflate = CompressionHelper.CompressDeflate(originalData);
string decompressedDeflate = CompressionHelper.DecompressDeflate(compressedDeflate);
Console.WriteLine($"Deflate Decompressed: {decompressedDeflate}");
```

#### Analyze compression efficiency
```csharp
CompressionStats stats = CompressionAnalyzer.AnalyzeCompression(originalData);
Console.WriteLine($"Compression Stats: {stats}");
Console.WriteLine($"Original Size: {stats.OriginalSizeBytes}B");
Console.WriteLine($"Compressed Size: {stats.CompressedSizeBytes}B");
Console.WriteLine($"Savings: {stats.SavingsBytes}B ({stats.CompressionRatioPercent:F1}%)");
```

#### Compare compression algorithms
```csharp
CompressionComparison comparison = CompressionAnalyzer.CompareAlgorithms(originalData);
Console.WriteLine($"Best Algorithm: {comparison.BestAlgorithm}");
Console.WriteLine($"GZIP Ratio: {comparison.GzipRatioPercent:F1}%");
Console.WriteLine($"Deflate Ratio: {comparison.DeflateRatioPercent:F1}%");
```

#### Compress and decompress files
```csharp
await CompressionHelper.CompressFileAsync("input.txt", "output.gz");
await CompressionHelper.DecompressFileAsync("output.gz", "decompressed.txt");
```
