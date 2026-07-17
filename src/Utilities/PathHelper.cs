#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Helper class for file path operations, validation, and normalization.
/// Provides cross-platform path handling utilities.
/// </summary>
public sealed class PathHelper
{
    /// <summary>
    /// Gets the original path string.
    /// </summary>
    public string OriginalPath { get; }

    /// <summary>
    /// Gets the normalized absolute path.
    /// </summary>
    public string NormalizedPath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PathHelper"/> class.
    /// </summary>
    /// <param name="path">The file path to process.</param>
    /// <exception cref="ArgumentNullException">Thrown when path is null.</exception>
    /// <exception cref="ArgumentException">Thrown when path is empty or whitespace.</exception>
    public PathHelper(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        OriginalPath = path;
        NormalizedPath = System.IO.Path.GetFullPath(path);
    }

    /// <summary>
    /// Validates if the path is valid and accessible.
    /// </summary>
    /// <returns>True if the path is valid; otherwise, false.</returns>
    public bool IsValidPath()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(OriginalPath))
                return false;

            var fullPath = Path.GetFullPath(OriginalPath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Normalizes the path for the current platform.
    /// </summary>
    /// <returns>The normalized absolute path.</returns>
    public string Normalize()
    {
        return NormalizedPath;
    }

    /// <summary>
    /// Combines multiple path segments safely.
    /// </summary>
    /// <param name="segments">The path segments to combine.</param>
    /// <returns>The combined path.</returns>
    public static string CombinePaths(params string[] segments)
    {
        if (segments is null || segments.Length == 0)
            return string.Empty;

        return Path.Combine(segments);
    }

    /// <summary>
    /// Gets the relative path from this path to another path.
    /// </summary>
    /// <param name="toPath">The target path.</param>
    /// <returns>The relative path from this path to the target path.</returns>
    public string GetRelativePath(string toPath)
    {
        try
        {
            return Path.GetRelativePath(NormalizedPath, toPath);
        }
        catch
        {
            return toPath;
        }
    }

    /// <summary>
    /// Checks if a file path is within a directory.
    /// </summary>
    /// <param name="directory">The directory path to check against.</param>
    /// <returns>True if the path is within the directory; otherwise, false.</returns>
    public bool IsPathInDirectory(string directory)
    {
        try
        {
            var fullDirectory = Path.GetFullPath(directory);

            return NormalizedPath.StartsWith(fullDirectory + Path.DirectorySeparatorChar) || NormalizedPath == fullDirectory;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Sanitizes a filename to remove invalid characters.
    /// </summary>
    /// <param name="filename">The filename to sanitize.</param>
    /// <returns>The sanitized filename.</returns>
    public static string SanitizeFilename(string filename)
    {
        ArgumentNullException.ThrowIfNull(filename);

        var invalidChars = new string(Path.GetInvalidFileNameChars());
        var pattern = $"[{Regex.Escape(invalidChars)}]";
        return Regex.Replace(filename, pattern, "_");
    }

    /// <summary>
    /// Generates a unique filename to avoid collisions.
    /// </summary>
    /// <param name="baseFilename">The base filename.</param>
    /// <param name="directory">The directory to check for existing files.</param>
    /// <returns>A unique file path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when baseFilename or directory is null.</exception>
    public static string GenerateUniqueFilename(string baseFilename, string directory)
    {
        ArgumentNullException.ThrowIfNull(baseFilename);
        ArgumentNullException.ThrowIfNull(directory);

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
    /// <param name="path">The path to check disk space for.</param>
    /// <returns>The available disk space in bytes, or -1 if unavailable.</returns>
    public static long GetAvailableDiskSpace(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(path)!);
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
    /// <param name="path">The path to check disk space for.</param>
    /// <returns>The total disk space in bytes, or -1 if unavailable.</returns>
    public static long GetTotalDiskSpace(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(path)!);
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
    /// <param name="path">The directory path to measure.</param>
    /// <returns>The size of the directory in bytes, or 0 if the directory doesn't exist.</returns>
    public static long GetDirectorySize(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!Directory.Exists(path))
            return 0;

        try
        {
            var di = new DirectoryInfo(path);
            return di.EnumerateFiles("*", SearchOption.AllDirectories)
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
    /// <param name="path">The directory path to ensure.</param>
    /// <exception cref="ArgumentNullException">Thrown when path is null.</exception>
    public static void EnsureDirectory(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// Gets a safe temporary file path.
    /// </summary>
    /// <param name="extension">The file extension to use (default: .tmp).</param>
    /// <returns>A temporary file path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when extension is null.</exception>
    public static string GetTemporaryFilePath(string extension = ".tmp")
    {
        ArgumentNullException.ThrowIfNull(extension);

        var path = Path.Combine(Path.GetTempPath(), $"pipeline_{Guid.NewGuid():N}{extension}");
        return path;
    }

    /// <summary>
    /// Gets the file size in a human-readable format.
    /// </summary>
    /// <param name="bytes">The file size in bytes.</param>
    /// <returns>A human-readable file size string.</returns>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemMonitor"/> class.
    /// </summary>
    /// <param name="path">The directory path to monitor.</param>
    /// <exception cref="ArgumentNullException">Thrown when path is null.</exception>
    public FileSystemMonitor(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        _watcher = new FileSystemWatcher(path);
    }

    public event FileSystemEventHandler? Changed
    {
        add { _watcher.Changed += value; }
        remove { _watcher.Changed -= value; }
    }

    public event FileSystemEventHandler? Created
    {
        add { _watcher.Created += value; }
        remove { _watcher.Created -= value; }
    }

    public event FileSystemEventHandler? Deleted
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