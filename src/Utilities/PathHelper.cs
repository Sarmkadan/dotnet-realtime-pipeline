#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Helper class for file path operations, validation, and normalization.
/// Provides cross-platform path handling utilities.
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// Validates if a file path is valid and accessible.
    /// </summary>
    public static bool IsValidPath(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var fullPath = Path.GetFullPath(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Normalizes a file path for the current platform.
    /// </summary>
    public static string Normalize(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        return Path.GetFullPath(path);
    }

    /// <summary>
    /// Combines multiple path segments safely.
    /// </summary>
    public static string CombinePaths(params string[] segments)
    {
        if (segments is null || segments.Length == 0)
            return string.Empty;

        return Path.Combine(segments);
    }

    /// <summary>
    /// Gets the relative path from one directory to another.
    /// </summary>
    public static string GetRelativePath(string fromPath, string toPath)
    {
        try
        {
            return Path.GetRelativePath(fromPath, toPath);
        }
        catch
        {
            return toPath;
        }
    }

    /// <summary>
    /// Checks if a file path is within a directory.
    /// </summary>
    public static bool IsPathInDirectory(string filePath, string directory)
    {
        try
        {
            var fullFilePath = Path.GetFullPath(filePath);
            var fullDirectory = Path.GetFullPath(directory);

            return fullFilePath.StartsWith(fullDirectory + Path.DirectorySeparatorChar) || fullFilePath == fullDirectory;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Sanitizes a filename to remove invalid characters.
    /// </summary>
    public static string SanitizeFilename(string filename)
    {
        var invalidChars = new string(Path.GetInvalidFileNameChars());
        var pattern = $"[{Regex.Escape(invalidChars)}]";
        return Regex.Replace(filename, pattern, "_");
    }

    /// <summary>
    /// Generates a unique filename to avoid collisions.
    /// </summary>
    public static string GenerateUniqueFilename(string baseFilename, string directory)
    {
        var sanitized = SanitizeFilename(baseFilename);
        var path = Path.Combine(directory, sanitized);

        if (!File.Exists(path))
            return path;

        var extension = Path.GetExtension(sanitized);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitized);

        int counter = 1;
        while (true)
        {
            var newFilename = $"{nameWithoutExtension}_{counter}{extension}";
            var newPath = Path.Combine(directory, newFilename);

            if (!File.Exists(newPath))
                return newPath;

            counter++;
        }
    }

    /// <summary>
    /// Gets the disk space available for a path.
    /// </summary>
    public static long GetAvailableDiskSpace(string path)
    {
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(path));
            return drive.AvailableFreeSpace;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Gets the total disk space for a path.
    /// </summary>
    public static long GetTotalDiskSpace(string path)
    {
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(path));
            return drive.TotalSize;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Gets the directory size in bytes.
    /// </summary>
    public static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
            return 0;

        try
        {
            var di = new DirectoryInfo(path);
            return di.EnumerateFiles("*", System.IO.SearchOption.AllDirectories)
                .Sum(f => f.Length);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Ensures a directory exists, creating it if necessary.
    /// </summary>
    public static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// Gets a safe temporary file path.
    /// </summary>
    public static string GetTemporaryFilePath(string extension = ".tmp")
    {
        var path = Path.Combine(Path.GetTempPath(), $"pipeline_{Guid.NewGuid():N}{extension}");
        return path;
    }

    /// <summary>
    /// Gets the file size in a human-readable format.
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F2} KB";
        if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024.0 * 1024):F2} MB";

        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}

/// <summary>
/// File system monitoring and change detection.
/// </summary>
public sealed class FileSystemMonitor : IDisposable
{
    private readonly FileSystemWatcher _watcher;

    public FileSystemMonitor(string path)
    {
        _watcher = new FileSystemWatcher(path);
    }

    public event FileSystemEventHandler Changed
    {
        add { _watcher.Changed += value; }
        remove { _watcher.Changed -= value; }
    }

    public event FileSystemEventHandler Created
    {
        add { _watcher.Created += value; }
        remove { _watcher.Created -= value; }
    }

    public event FileSystemEventHandler Deleted
    {
        add { _watcher.Deleted += value; }
        remove { _watcher.Deleted -= value; }
    }

    public void Start()
    {
        _watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
