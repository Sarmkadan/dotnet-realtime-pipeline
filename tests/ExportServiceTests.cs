#nullable enable

using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Data;
using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.Logging;

namespace DotNetRealtimePipeline.Data.Tests;

public class ExportServiceTests
{
    private readonly ILogger<ExportService> _logger;

    public ExportServiceTests()
    {
        _logger = new LoggerFactory().CreateLogger<ExportService>();
    }

    [Fact]
    public async Task ExportDataPointsAsync_HappyPath_WritesToFile()
    {
        // Arrange
        var exportService = new ExportService(_logger);
        var dataPoints = new List<DataPoint> { new DataPoint() };
        var outputPath = Path.GetTempFileName();

        // Act
        var result = await exportService.ExportDataPointsAsync(dataPoints, outputPath, OutputFormat.Json);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public async Task ExportResultsAsync_HappyPath_WritesToFile()
    {
        // Arrange
        var exportService = new ExportService(_logger);
        var results = new List<ProcessingResult> { new ProcessingResult() };
        var outputPath = Path.GetTempFileName();

        // Act
        var result = await exportService.ExportResultsAsync(results, outputPath);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public async Task ExportMetricsAsync_HappyPath_WritesToFile()
    {
        // Arrange
        var exportService = new ExportService(_logger);
        var metrics = new MetricAggregation();
        var outputPath = Path.GetTempFileName();

        // Act
        var result = await exportService.ExportMetricsAsync(metrics, outputPath);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public async Task ExportMultiFormatAsync_HappyPath_WritesToMultipleFiles()
    {
        // Arrange
        var exportService = new ExportService(_logger);
        var dataPoints = new List<DataPoint> { new DataPoint() };
        var outputDirectory = Path.GetTempPath();

        // Act
        var results = await exportService.ExportMultiFormatAsync(dataPoints, outputDirectory, OutputFormat.Json, OutputFormat.Csv);

        // Assert
        Assert.All(results, result => Assert.True(result.Success));
        Assert.All(results, result => Assert.True(File.Exists(result.OutputPath)));
    }

    [Fact]
    public async Task ExportDataPointsAsync_NullDataPoints_ThrowsArgumentNullException()
    {
        // Arrange
        var exportService = new ExportService(_logger);
        var outputPath = Path.GetTempFileName();

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => exportService.ExportDataPointsAsync(null, outputPath, OutputFormat.Json));
    }

    [Fact]
    public async Task ExportDataPointsAsync_EmptyDataPoints_WritesEmptyFile()
    {
        // Arrange
        var exportService = new ExportService(_logger);
        var dataPoints = new List<DataPoint>();
        var outputPath = Path.GetTempFileName();

        // Act
        var result = await exportService.ExportDataPointsAsync(dataPoints, outputPath, OutputFormat.Json);

        // Assert
        Assert.True(result.Success);
        Assert.True(File.Exists(outputPath));
        Assert.Empty(File.ReadAllText(outputPath));
    }
}
