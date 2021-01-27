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
