# PathHelper

The `PathHelper` class provides a comprehensive suite of static utility methods for robust file system path manipulation, validation, and disk space analysis within the `dotnet-realtime-pipeline` project. It also includes the nested `FileSystemMonitor` class for observing directory changes. This utility ensures cross-platform compatibility for path operations, handles filename sanitization, manages temporary file creation, and offers precise calculations for directory sizes and available storage, serving as a foundational component for reliable file I/O operations in real-time data processing pipelines.

## API

### Static Methods

#### `IsValidPath`
Determines whether a given string represents a valid file system path according to the current operating system's rules.
*   **Parameters**: `string path` - The path string to validate.
*   **Returns**: `bool` - `true` if the path syntax is valid; otherwise, `false`.
*   **Throws**: None. Returns `false` for null or empty inputs.

#### `Normalize`
Standardizes a path string by resolving relative segments (e.g., `.` and `..`), correcting directory separators, and removing redundant whitespace.
*   **Parameters**: `string path` - The path to normalize.
*   **Returns**: `string` - The normalized absolute path.
*   **Throws**: `ArgumentException` if the path contains invalid characters or is malformed.

#### `CombinePaths`
Safely combines multiple path segments into a single path, handling separator insertion automatically.
*   **Parameters**: `params string[] paths` - An array of path segments to combine.
*   **Returns**: `string` - The combined path.
*   **Throws**: `ArgumentException` if any segment contains invalid path characters.

#### `GetRelativePath`
Calculates the relative path from a base directory to a target path.
*   **Parameters**: 
    *   `string basePath` - The root directory path.
    *   `string targetPath` - The target file or directory path.
*   **Returns**: `string` - The relative path string.
*   **Throws**: `ArgumentException` if the paths are on different volumes (Windows) or if either path is invalid.

#### `IsPathInDirectory`
Verifies whether a specific file or directory path resides physically within a specified root directory.
*   **Parameters**: 
    *   `string directoryPath` - The root directory to check against.
    *   `string filePath` - The path to verify.
*   **Returns**: `bool` - `true` if `filePath` is inside `directoryPath`; otherwise, `false`.
*   **Throws**: None. Returns `false` if paths are invalid or unrelated.

#### `SanitizeFilename`
Removes or replaces characters illegal in file names for the current operating system.
*   **Parameters**: `string filename` - The raw filename string.
*   **Returns**: `string` - A safe filename string with invalid characters removed or substituted.
*   **Throws**: None. Returns an empty string if the result yields no valid characters.

#### `GenerateUniqueFilename`
Generates a unique filename within a specified directory, optionally based on a preferred name, by appending a counter or GUID if a collision exists.
*   **Parameters**: 
    *   `string directory` - The target directory.
    *   `string preferredName` - The desired filename (optional).
*   **Returns**: `string` - The full path to the unique filename.
*   **Throws**: `IOException` if the directory does not exist or is inaccessible.

#### `GetAvailableDiskSpace`
Retrieves the amount of free space available on the drive containing the specified path.
*   **Parameters**: `string path` - Any path on the target drive.
*   **Returns**: `long` - The number of free bytes.
*   **Throws**: `IOException` if the drive cannot be accessed or the path is invalid.

#### `GetTotalDiskSpace`
Retrieves the total capacity of the drive containing the specified path.
*   **Parameters**: `string path` - Any path on the target drive.
*   **Returns**: `long` - The total number of bytes.
*   **Throws**: `IOException` if the drive cannot be accessed or the path is invalid.

#### `GetDirectorySize`
Recursively calculates the total size of all files within a directory.
*   **Parameters**: `string directoryPath` - The root directory to measure.
*   **Returns**: `long` - The total size in bytes.
*   **Throws**: `DirectoryNotFoundException` if the path does not exist; `UnauthorizedAccessException` if access is denied to subdirectories.

#### `EnsureDirectory`
Creates a directory if it does not already exist.
*   **Parameters**: `string path` - The directory path to ensure.
*   **Returns**: `void`.
*   **Throws**: `IOException` if the directory cannot be created due to permissions or invalid characters.

#### `GetTemporaryFilePath`
Generates a full path for a temporary file in the system's temp directory with a specific extension.
*   **Parameters**: `string extension` - The file extension (e.g., ".tmp").
*   **Returns**: `string` - The full path to the new temporary file location (file is not created yet).
*   **Throws**: `ArgumentException` if the extension is invalid.

#### `FormatFileSize`
Converts a byte count into a human-readable string representation (e.g., "1.5 GB").
*   **Parameters**: `long bytes` - The size in bytes.
*   **Returns**: `string` - The formatted size string.
*   **Throws**: None. Returns "0 B" for negative or zero values.

### Nested Class: `FileSystemMonitor`

A disposable component for monitoring file system events within a specific directory.

#### `Start`
Initiates the monitoring process, enabling event raising for file creations, deletions, and changes.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: `InvalidOperationException` if the monitor is already running or not configured with a path.

#### `Stop`
Halts the monitoring process and disables event raising.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: None.

#### `Dispose`
Releases unmanaged resources and stops the monitor if it is currently running.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: None.

## Usage

### Example 1: Safe File Writing with Validation and Space Check
This example demonstrates validating a target path, ensuring the directory exists, checking for sufficient disk space, and generating a unique filename to prevent overwrites.

```csharp
using System;
using System.IO;

public class DataIngestionService
{
    public void SaveDataPacket(string rootDirectory, byte[] data, string suggestedName)
    {
        // Normalize and validate the root directory
        string safeRoot = PathHelper.Normalize(rootDirectory);
        if (!PathHelper.IsValidPath(safeRoot))
        {
            throw new ArgumentException("Invalid root directory provided.");
        }

        // Ensure the directory structure exists
        PathHelper.EnsureDirectory(safeRoot);

        // Check available disk space (require at least 10MB)
        long availableSpace = PathHelper.GetAvailableDiskSpace(safeRoot);
        if (availableSpace < 10 * 1024 * 1024)
        {
            throw new IOException($"Insufficient disk space. Available: {PathHelper.FormatFileSize(availableSpace)}");
        }

        // Sanitize the filename and ensure uniqueness
        string cleanName = PathHelper.SanitizeFilename(suggestedName);
        string finalPath = PathHelper.GenerateUniqueFilename(safeRoot, cleanName);

        // Write the data (implementation omitted)
        File.WriteAllBytes(finalPath, data);
        
        Console.WriteLine($"Data saved to: {PathHelper.GetRelativePath(safeRoot, finalPath)}");
    }
}
```

### Example 2: Directory Analysis and Monitoring
This example illustrates calculating the current size of a log directory and attaching a monitor to track new incoming logs in real-time.

```csharp
using System;
using System.Threading;

public class LogAnalyzer
{
    public void AnalyzeAndWatch(string logDirectory)
    {
        if (!Directory.Exists(logDirectory))
        {
            Console.WriteLine("Directory not found.");
            return;
        }

        // Get current total size
        long currentSize = PathHelper.GetDirectorySize(logDirectory);
        Console.WriteLine($"Current log volume: {PathHelper.FormatFileSize(currentSize)}");

        // Initialize and start the file system monitor
        var monitor = new PathHelper.FileSystemMonitor(logDirectory);
        
        // Note: Event subscription logic would be handled internally or via callbacks 
        // depending on the specific implementation of FileSystemMonitor events.
        
        try
        {
            monitor.Start();
            Console.WriteLine("Monitoring started. Press any key to stop.");
            Console.ReadKey();
        }
        finally
        {
            monitor.Stop();
            monitor.Dispose();
            Console.WriteLine("Monitoring stopped and resources released.");
        }
    }
}
```

## Notes

*   **Thread Safety**: All static methods in `PathHelper` are thread-safe and stateless, relying solely on input parameters and standard .NET I/O APIs. The `FileSystemMonitor` class is **not** thread-safe; instances should not be shared across threads without external synchronization, and `Start`/`Stop` should not be called concurrently.
*   **Path Separators**: The `Normalize` and `CombinePaths` methods automatically handle conversion between forward slashes (`/`) and backslashes (`\`) to match the host operating system, ensuring consistency in cross-platform deployments.
*   **Race Conditions**: While `GenerateUniqueFilename` attempts to guarantee uniqueness at the time of generation, a race condition may still occur if multiple processes attempt to create the same generated name simultaneously. It is recommended to wrap the actual file creation in a retry loop or use `FileMode.CreateNew`.
*   **Symbolic Links**: `GetDirectorySize` follows symbolic links by default. If the directory structure contains circular symbolic links, this method may result in a `StackOverflowException` or infinite loop depending on the underlying OS behavior; callers should ensure the target directory structure is safe.
*   **Permissions**: Methods accessing the disk (`GetAvailableDiskSpace`, `GetDirectorySize`, `EnsureDirectory`) will throw `UnauthorizedAccessException` if the executing user lacks necessary permissions. These should be caught explicitly in high-availability pipeline scenarios.
*   **Disposable Pattern**: The `FileSystemMonitor` implements `IDisposable`. It is critical to call `Dispose` (or use a `using` statement) to release underlying OS file handles, failure to do so may lock the directory and prevent other processes from modifying files within it.
