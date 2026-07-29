// File-scoped namespace to match existing test files
namespace DotNetRealtimePipeline.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using DotNetRealtimePipeline.Data;
using Xunit;

public sealed class ExportServiceValidationTests
{
    private static ExportResult CreateValidExportResult()
    {
        return new ExportResult
        {
            Success = true,
            ErrorMessage = null,
            OutputPath = Path.GetFullPath("export.json"),
            RecordCount = 10,
            FileSizeBytes = 1024,
            StartTime = DateTime.UtcNow.AddMinutes(-5),
            EndTime = DateTime.UtcNow
        };
    }

    private static BatchExportResult CreateValidBatchExportResult()
    {
        return new BatchExportResult
        {
            Success = true,
            ErrorMessage = null,
            ExportedRecords = 20,
            BatchFiles = new List<string> { Path.GetFullPath("batch1.json") },
            StartTime = DateTime.UtcNow.AddMinutes(-10),
            EndTime = DateTime.UtcNow
        };
    }

    [Fact]
    public void Validate_ExportResult_HappyPath_ReturnsEmpty()
    {
        var result = CreateValidExportResult();

        var problems = result.Validate();

        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_BatchExportResult_HappyPath_ReturnsEmpty()
    {
        var batch = CreateValidBatchExportResult();

        var problems = batch.Validate();

        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_NullExportResult_ThrowsArgumentNullException()
    {
        ExportResult? nullResult = null;
        Assert.Throws<ArgumentNullException>(() => nullResult!.Validate());
    }

    [Fact]
    public void Validate_NullBatchExportResult_ThrowsArgumentNullException()
    {
        BatchExportResult? nullBatch = null;
        Assert.Throws<ArgumentNullException>(() => nullBatch!.Validate());
    }

    [Fact]
    public void EnsureValid_InvalidExportResult_ThrowsArgumentException()
    {
        var invalid = new ExportResult
        {
            Success = false,
            ErrorMessage = null, // violates rule: error message required when Success is false
            OutputPath = "relative/path.json", // also not absolute
            RecordCount = -1, // negative
            FileSizeBytes = -10,
            StartTime = default,
            EndTime = default
        };

        var ex = Assert.Throws<ArgumentException>(() => invalid.EnsureValid());
        Assert.Contains("ExportResult.Success must be true when ErrorMessage is null or empty.", ex.Message);
        Assert.Contains("ExportResult.OutputPath must be an absolute path.", ex.Message);
        Assert.Contains("ExportResult.RecordCount cannot be negative.", ex.Message);
        Assert.Contains("ExportResult.FileSizeBytes cannot be negative.", ex.Message);
        Assert.Contains("ExportResult.StartTime cannot be the default DateTime value.", ex.Message);
        Assert.Contains("ExportResult.EndTime cannot be the default DateTime value.", ex.Message);
    }

    [Fact]
    public void IsValid_InvalidExportResult_ReturnsFalse()
    {
        var invalid = new ExportResult
        {
            Success = false,
            ErrorMessage = null,
            OutputPath = Path.GetFullPath("invalid.json"),
            RecordCount = 0,
            FileSizeBytes = 0,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow
        };

        Assert.False(invalid.IsValid());
    }

    [Fact]
    public void EnsureValid_NullBatchExportResult_ThrowsArgumentNullException()
    {
        BatchExportResult? nullBatch = null;
        Assert.Throws<ArgumentNullException>(() => nullBatch!.EnsureValid());
    }

    [Fact]
    public void Validate_BatchExportResult_NegativeExportedRecords_ReturnsProblem()
    {
        var batch = CreateValidBatchExportResult();
        batch.ExportedRecords = -5;

        var problems = batch.Validate();

        Assert.Contains("BatchExportResult.ExportedRecords cannot be negative.", problems);
    }

    [Fact]
    public void Validate_BatchExportResult_EmptyFilesWhenRecordsExported_ReturnsProblem()
    {
        var batch = CreateValidBatchExportResult();
        batch.BatchFiles = new List<string>(); // empty collection while ExportedRecords > 0

        var problems = batch.Validate();

        Assert.Contains("BatchExportResult.BatchFiles cannot be empty when records were exported.", problems);
    }
}
