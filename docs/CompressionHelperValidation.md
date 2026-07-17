# CompressionHelperValidation

The `CompressionHelperValidation` class provides a centralized set of static utility methods for validating input parameters and state conditions prior to executing compression or decompression operations within the `dotnet-realtime-pipeline`. It enforces strict guard clauses to ensure data integrity, file accessibility, and algorithm compatibility, returning detailed validation error messages or throwing immediate exceptions to prevent invalid operations from proceeding.

## API

### Validate
Validates general input conditions common to compression operations.
*   **Returns**: `IReadOnlyList<string>` containing error messages if validation fails; an empty list if valid.
*   **Throws**: Does not throw; returns errors as a list.

### IsValid
Determines if the general input conditions are valid.
*   **Returns**: `bool` indicating `true` if no validation errors exist, otherwise `false`.
*   **Throws**: Does not throw.

### EnsureValid
Enforces general input validity by throwing an exception if checks fail.
*   **Returns**: `void`.
*   **Throws**: Throws an exception (typically `ArgumentException` or `InvalidOperationException`) containing the aggregated validation errors if the input is invalid.

### ValidateForCompressGzip
Validates specific prerequisites for GZIP compression operations.
*   **Returns**: `IReadOnlyList<string>` containing error messages specific to GZIP compression constraints.
*   **Throws**: Does not throw.

### ValidateForDecompressGzip
Validates specific prerequisites for GZIP decompression operations.
*   **Returns**: `IReadOnlyList<string>` containing error messages specific to GZIP decompression constraints.
*   **Throws**: Does not throw.

### ValidateForCompressDeflate
Validates specific prerequisites for Deflate compression operations.
*   **Returns**: `IReadOnlyList<string>` containing error messages specific to Deflate compression constraints.
*   **Throws**: Does not throw.

### ValidateForDecompressDeflate
Validates specific prerequisites for Deflate decompression operations.
*   **Returns**: `IReadOnlyList<string>` containing error messages specific to Deflate decompression constraints.
*   **Throws**: Does not throw.

### ValidateForCompressFileAsync
Validates conditions required for asynchronous file compression, such as source file existence and destination path writability.
*   **Returns**: `IReadOnlyList<string>` containing file-system-specific error messages.
*   **Throws**: Does not throw.

### ValidateForDecompressFileAsync
Validates conditions required for asynchronous file decompression, including source file integrity and target directory availability.
*   **Returns**: `IReadOnlyList<string>` containing file-system-specific error messages.
*   **Throws**: Does not throw.

### ValidateForCalculateCompressionRatio
Validates inputs necessary for calculating compression ratios, ensuring both original and compressed sizes are available and logical.
*   **Returns**: `IReadOnlyList<string>` containing mathematical or data availability errors.
*   **Throws**: Does not throw.

### ValidateForAnalyzeCompression
Validates parameters required for deep compression analysis routines.
*   **Returns**: `IReadOnlyList<string>` containing configuration or data errors.
*   **Throws**: Does not throw.

### ValidateForCompareAlgorithms
Validates inputs when comparing multiple compression algorithms, ensuring comparable data sets and valid algorithm lists.
*   **Returns**: `IReadOnlyList<string>` containing comparison constraint errors.
*   **Throws**: Does not throw.

### IsValidForCompression
A high-level check determining if the current context is valid for any compression operation.
*   **Returns**: `bool` indicating overall compression readiness.
*   **Throws**: Does not throw.

### EnsureValidForCompression
Enforces validity for compression operations, aggregating all relevant compression checks.
*   **Returns**: `void`.
*   **Throws**: Throws an exception if any compression-specific validation fails.

### IsValidForDecompression
A high-level check determining if the current context is valid for any decompression operation.
*   **Returns**: `bool` indicating overall decompression readiness.
*   **Throws**: Does not throw.

### EnsureValidForDecompression
Enforces validity for decompression operations, aggregating all relevant decompression checks.
*   **Returns**: `void`.
*   **Throws**: Throws an exception if any decompression-specific validation fails.

## Usage

### Example 1: Pre-flight Check for File Compression
This example demonstrates using the specific file validation method to gather errors before attempting an asynchronous operation, allowing for graceful error handling without exceptions.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CompressionService
{
    public async Task CompressDataAsync(string sourcePath, string destPath)
    {
        // Gather validation errors without throwing
        var errors = CompressionHelperValidation.ValidateForCompressFileAsync(sourcePath, destPath);

        if (errors.Any())
        {
            // Log or return specific errors to the caller
            Console.WriteLine("Compression failed validation:");
            foreach (var error in errors)
            {
                Console.WriteLine($"- {error}");
            }
            return;
        }

        // Proceed with actual compression logic only if valid
        await PerformCompressionAsync(sourcePath, destPath);
    }

    private Task PerformCompressionAsync(string src, string dest)
    {
        // Implementation omitted
        return Task.CompletedTask;
    }
}
```

### Example 2: Enforcing Validity for General Compression
This example utilizes the `EnsureValidForCompression` method to strictly guard a critical section, causing the operation to fail fast if the environment or data is not suitable for compression.

```csharp
using System;

public class DataPipeline
{
    public void ProcessAndCompress(byte[] data)
    {
        try
        {
            // Throw immediately if compression conditions are not met
            CompressionHelperValidation.EnsureValidForCompression(data);

            // Execute compression logic
            var compressed = CompressInternal(data);
            Console.WriteLine($"Compressed {data.Length} bytes to {compressed.Length} bytes.");
        }
        catch (Exception ex)
        {
            // Handle the aggregated validation failure
            Console.Error.WriteLine($"Critical validation failure: {ex.Message}");
            throw;
        }
    }

    private byte[] CompressInternal(byte[] input)
    {
        // Implementation omitted
        return new byte[0];
    }
}
```

## Notes

*   **Thread Safety**: As `CompressionHelperValidation` consists entirely of `static` methods that operate on passed-in parameters without maintaining internal mutable state, the class is inherently thread-safe. Multiple threads may invoke validation methods concurrently without risk of race conditions.
*   **Exception Aggregation**: The `EnsureValid` and `EnsureValidFor...` methods typically aggregate all detected validation failures into a single exception message rather than throwing on the first error. This allows callers to see the complete list of issues requiring resolution.
*   **Return Value Consistency**: All `Validate...` methods return an `IReadOnlyList<string>`. An empty list signifies success. Callers should check `Count` or use LINQ's `Any()` method to determine validity rather than checking for `null`, as these methods generally return an empty collection rather than `null` when valid.
*   **File System Latency**: Methods ending in `...FileAsync` perform file system checks (existence, permissions). While the validation methods themselves are synchronous, they perform I/O. In high-throughput scenarios, be aware that these checks may introduce latency before the actual asynchronous operation begins.
*   **Algorithm Specificity**: Distinct validation methods exist for GZIP and Deflate. Ensure the specific method matching the intended algorithm is used, as cross-algorithm validation (e.g., using `ValidateForCompressGzip` for a Deflate stream) may miss format-specific constraints.
