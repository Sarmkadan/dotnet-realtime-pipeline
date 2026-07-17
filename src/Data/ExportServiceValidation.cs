#nullable enable

namespace DotNetRealtimePipeline.Data;

/// <summary>
/// Provides validation helpers for <see cref="ExportResult"/> and <see cref="BatchExportResult"/> instances.
/// </summary>
public static class ExportServiceValidation
{
    /// <summary>
    /// Validates an <see cref="ExportResult"/> instance.
    /// </summary>
    /// <param name="value">The export result instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ExportResult? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Success property - Success should be true unless there's an error
        if (!value.Success && string.IsNullOrEmpty(value.ErrorMessage))
        {
            problems.Add("ExportResult.Success must be true when ErrorMessage is null or empty.");
        }

        // Validate OutputPath
        if (string.IsNullOrWhiteSpace(value.OutputPath))
        {
            problems.Add("ExportResult.OutputPath cannot be null or whitespace.");
        }
        else if (!System.IO.Path.IsPathRooted(value.OutputPath))
        {
            problems.Add("ExportResult.OutputPath must be an absolute path.");
        }

        // Validate RecordCount
        if (value.RecordCount < 0)
        {
            problems.Add("ExportResult.RecordCount cannot be negative.");
        }

        // Validate FileSizeBytes
        if (value.FileSizeBytes < 0)
        {
            problems.Add("ExportResult.FileSizeBytes cannot be negative.");
        }

        // Validate ErrorMessage - must be empty when Success is true
        if (!string.IsNullOrEmpty(value.ErrorMessage) && value.Success)
        {
            problems.Add("ExportResult.ErrorMessage must be null or empty when Success is true.");
        }

        // Validate StartTime
        if (value.StartTime == default)
        {
            problems.Add("ExportResult.StartTime cannot be the default DateTime value.");
        }
        else if (value.StartTime.Kind != DateTimeKind.Utc)
        {
            problems.Add("ExportResult.StartTime must be in UTC.");
        }

        // Validate EndTime
        if (value.EndTime == default)
        {
            problems.Add("ExportResult.EndTime cannot be the default DateTime value.");
        }
        else if (value.EndTime.Kind != DateTimeKind.Utc)
        {
            problems.Add("ExportResult.EndTime must be in UTC.");
        }

        // Validate temporal consistency
        if (value.StartTime != default && value.EndTime != default && value.StartTime > value.EndTime)
        {
            problems.Add("ExportResult.StartTime cannot be after EndTime.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="BatchExportResult"/> instance.
    /// </summary>
    /// <param name="value">The batch export result instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this BatchExportResult? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Success property - Success should be true unless there's an error
        if (!value.Success && string.IsNullOrEmpty(value.ErrorMessage))
        {
            problems.Add("BatchExportResult.Success must be true when ErrorMessage is null or empty.");
        }

        // Validate ExportedRecords
        if (value.ExportedRecords < 0)
        {
            problems.Add("BatchExportResult.ExportedRecords cannot be negative.");
        }

        // Validate BatchFiles - collection itself cannot be null
        if (value.BatchFiles is null)
        {
            problems.Add("BatchExportResult.BatchFiles cannot be null.");
        }
        else if (value.BatchFiles.Count == 0 && value.ExportedRecords > 0)
        {
            problems.Add("BatchExportResult.BatchFiles cannot be empty when records were exported.");
        }

        // Validate ErrorMessage - must be empty when Success is true
        if (!string.IsNullOrEmpty(value.ErrorMessage) && value.Success)
        {
            problems.Add("BatchExportResult.ErrorMessage must be null or empty when Success is true.");
        }

        // Validate StartTime
        if (value.StartTime == default)
        {
            problems.Add("BatchExportResult.StartTime cannot be the default DateTime value.");
        }
        else if (value.StartTime.Kind != DateTimeKind.Utc)
        {
            problems.Add("BatchExportResult.StartTime must be in UTC.");
        }

        // Validate EndTime
        if (value.EndTime == default)
        {
            problems.Add("BatchExportResult.EndTime cannot be the default DateTime value.");
        }
        else if (value.EndTime.Kind != DateTimeKind.Utc)
        {
            problems.Add("BatchExportResult.EndTime must be in UTC.");
        }

        // Validate temporal consistency
        if (value.StartTime != default && value.EndTime != default && value.StartTime > value.EndTime)
        {
            problems.Add("BatchExportResult.StartTime cannot be after EndTime.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="BatchExportResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The batch export result instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this BatchExportResult? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that a <see cref="BatchExportResult"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The batch export result instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this BatchExportResult? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"BatchExportResult validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Determines whether an <see cref="ExportResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The export result instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ExportResult? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that an <see cref="ExportResult"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The export result instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this ExportResult? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ExportResult validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}