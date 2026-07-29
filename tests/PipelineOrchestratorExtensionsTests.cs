namespace DotNetRealtimePipeline.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Services;
using Xunit;

public class PipelineOrchestratorExtensionsTests
{
    [Fact]
    public void GetStatusSummary_HappyPath_ReturnsStatusSummary()
    {
        // Arrange
        var orchestrator = new PipelineOrchestrator(); // Assuming a parameterless constructor

        // Act
        var statusSummary = PipelineOrchestratorExtensions.GetStatusSummary(orchestrator);

        // Assert
        Assert.NotNull(statusSummary);
    }

    [Fact]
    public void GetStatusSummary_NullOrchestrator_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PipelineOrchestratorExtensions.GetStatusSummary(null));
    }

    [Fact]
    public async Task GetHealthReportAsync_HappyPath_ReturnsHealthReport()
    {
        // Arrange
        var orchestrator = new PipelineOrchestrator(); // Assuming a parameterless constructor

        // Act
        var healthReport = await PipelineOrchestratorExtensions.GetHealthReportAsync(orchestrator);

        // Assert
        Assert.NotNull(healthReport);
    }

    [Fact]
    public async Task GetHealthReportAsync_NullOrchestrator_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => PipelineOrchestratorExtensions.GetHealthReportAsync(null));
    }

    [Fact]
    public async Task ProcessBatchAsync_HappyPath_ReturnsBatchProcessingResult()
    {
        // Arrange
        var orchestrator = new PipelineOrchestrator(); // Assuming a parameterless constructor
        var dataPoints = new List<DataPoint> { new DataPoint() };

        // Act
        var batchProcessingResult = await PipelineOrchestratorExtensions.ProcessBatchAsync(orchestrator, dataPoints);

        // Assert
        Assert.NotNull(batchProcessingResult);
    }

    [Fact]
    public async Task ProcessBatchAsync_NullOrchestrator_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => PipelineOrchestratorExtensions.ProcessBatchAsync(null, new List<DataPoint>()));
    }

    [Fact]
    public async Task ProcessBatchAsync_NullDataPoints_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => PipelineOrchestratorExtensions.ProcessBatchAsync(new PipelineOrchestrator(), null));
    }

    [Fact]
    public async Task ProcessBatchAsync_EmptyDataPoints_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => PipelineOrchestratorExtensions.ProcessBatchAsync(new PipelineOrchestrator(), new List<DataPoint>()));
    }
}
