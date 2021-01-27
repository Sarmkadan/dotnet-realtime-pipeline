## PathHelper
The `PathHelper` class provides cross-platform file path validation, normalization, and directory operations. It includes utilities for path combination, disk space checks, file size formatting, and directory monitoring via `FileSystemMonitor`.

Example usage:
```csharp
// Validate and normalize paths
bool isValid = PathHelper.IsValidPath("/invalid/path/");
string normalized = PathHelper.Normalize("C:\\\\Windows\\\\..\\\\Users");
string combined = PathHelper.CombinePaths("data", "logs", "2023-10");

// Check file relationships
bool isInDir = PathHelper.IsPathInDirectory("/home/user/data/file.txt", "/home/user/data");

// Sanitize and generate filenames
string safeName = PathHelper.SanitizeFilename("report<>.txt");
string uniquePath = PathHelper.GenerateUniqueFilename("report.txt", "/output");

// Disk space and file operations
long freeSpace = PathHelper.GetAvailableDiskSpace("/");
long totalSpace = PathHelper.GetTotalDiskSpace("/");
PathHelper.EnsureDirectory("/temp/backups");

// Temporary files and formatting
string tempFile = PathHelper.GetTemporaryFilePath(".log");
long dirSize = PathHelper.GetDirectorySize("/var/logs");
string formattedSize = PathHelper.FormatFileSize(dirSize);

// File system monitoring
using var monitor = new PathHelper.FileSystemMonitor("/watched-folder");
monitor.Changed += (sender, e) => Console.WriteLine($"File changed: {e.FullPath}");
monitor.Created += (sender, e) => Console.WriteLine($"File created: {e.FullPath}");
monitor.Start();
// ... monitor for changes ...
monitor.Stop();
```
This example demonstrates path validation, directory checks, filename sanitization, disk space queries, and file system monitoring using `PathHelper` and `FileSystemMonitor`.

## RetryHelper
The `RetryHelper` class implements retry logic with exponential backoff, jitter, and customizable retry conditions. It supports both synchronous and asynchronous operations, allowing developers to define retry policies with specific delays, maximum attempts, and exception types to retry on.

Example usage:
```csharp
// Configure retry policy with exponential backoff and jitter
var policy = new RetryPolicyBuilder()
    .WithMaxAttempts(5)
    .WithInitialDelay(500)
    .WithMaxDelay(30000)
    .WithJitter(true)
    .RetryOn<HttpRequestException>()
    .RetryOn<TimeoutException>()
    .Build();

// Execute async operation with retry policy
try
{
    var result = await policy.ExecuteAsync<string>(async () =>
    {
        // Simulate a flaky API call
        if (new Random().Next(0, 3) == 0)
            throw new HttpRequestException("Simulated network failure");
        
        return await FetchDataFromServiceAsync();
    });
    
    Console.WriteLine($"Success: {result}");
}
catch (Exception ex) when (ex is OperationCanceledException)
{
    Console.WriteLine($"Operation failed after retries: {ex.Message}");
}

// Track retry statistics
var stats = new RetryStatistics();
stats.RecordAttempt(true, 500);  // Record successful attempt
stats.RecordAttempt(false, 1000); // Record failed attempt
Console.WriteLine($"Success rate: {stats.SuccessRate:F2}%");
```
This example demonstrates configuring a retry policy with exponential backoff, jitter, and specific retryable exceptions, then using it to execute an asynchronous operation while tracking retry statistics.
