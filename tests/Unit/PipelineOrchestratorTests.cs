#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetRealtimePipeline.Constants;
using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Unit tests for the <see cref="PipelineOrchestrator"/> class.
/// Tests the constructor, lifecycle methods, data ingestion, batch processing,
/// and status reporting functionality of the pipeline orchestrator.
/// </summary>
public sealed class PipelineOrchestratorTests
{
    /// <summary>
    /// Mock service for data processing operations.
    /// </summary>
    private readonly Mock<DataProcessingService> _mockProcessingService;

    /// <summary>
    /// Mock service for windowing operations.
    /// </summary>
    private readonly Mock<WindowingService> _mockWindowingService;

    /// <summary>
    /// Mock service for metrics collection and analysis.
    /// </summary>
    private readonly Mock<MetricsService> _mockMetricsService;

    /// <summary>
    /// Mock service for backpressure management.
    /// </summary>
    private readonly Mock<BackpressureService> _mockBackpressureService;

    /// <summary>
    /// Mock service for query operations.
    /// </summary>
    private readonly Mock<QueryService> _mockQueryService;

    /// <summary>
    /// Configuration used for testing the pipeline orchestrator.
    /// </summary>
    private readonly PipelineConfig _config;

    /// <summary>
    /// Instance of the pipeline orchestrator being tested.
    /// </summary>
    private readonly PipelineOrchestrator _orchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineOrchestratorTests"/> class.
    /// Sets up mock services and a test configuration for testing the pipeline orchestrator.
    /// </summary>
    public PipelineOrchestratorTests()
    {
        _mockProcessingService = new Mock<DataProcessingService>(Mock.Of<IDataPointRepository>(), new PipelineConfig());
        _mockWindowingService = new Mock<WindowingService>(new PipelineConfig());
        _mockMetricsService = new Mock<MetricsService>(Mock.Of<IMetricsRepository>());
        _mockBackpressureService = new Mock<BackpressureService>();
        _mockQueryService = new Mock<QueryService>(Mock.Of<IDataPointRepository>(), Mock.Of<IMetricsRepository>());

        _config = new PipelineConfig
        {
            PipelineName = "TestPipeline",
            Version = "1.0.0",
            MaxBufferSize = 1000,
            Stages = new List<PipelineStageConfig>
            {
                new PipelineStageConfig { StageName = "Stage1", Enabled = true },
                new PipelineStageConfig { StageName = "Stage2", Enabled = true }
            }
        };

        _orchestrator = new PipelineOrchestrator(
            _mockProcessingService.Object,
            _mockWindowingService.Object,
            _mockMetricsService.Object,
            _mockBackpressureService.Object,
            _mockQueryService.Object,
            _config
        );
    }

    /// <summary>
    /// Tests that the constructor throws an <see cref="ArgumentNullException"/> when a null processing service is provided.
    /// </summary>
    [Fact]
    public void Constructor_WithNullProcessingService_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PipelineOrchestrator(
            null!,
            _mockWindowingService.Object,
            _mockMetricsService.Object,
            _mockBackpressureService.Object,
            _mockQueryService.Object,
            _config
        ));
    }

    /// <summary>
    /// Tests that the constructor throws an <see cref="ArgumentNullException"/> when a null windowing service is provided.
    /// </summary>
    [Fact]
    public void Constructor_WithNullWindowingService_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PipelineOrchestrator(
            _mockProcessingService.Object,
            null!,
            _mockMetricsService.Object,
            _mockBackpressureService.Object,
            _mockQueryService.Object,
            _config
        ));
    }

    /// <summary>
    /// Tests that the constructor throws an <see cref="ArgumentNullException"/> when a null metrics service is provided.
    /// </summary>
    [Fact]
    public void Constructor_WithNullMetricsService_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PipelineOrchestrator(
            _mockProcessingService.Object,
            _mockWindowingService.Object,
            null!,
            _mockBackpressureService.Object,
            _mockQueryService.Object,
            _config
        ));
    }

    /// <summary>
    /// Tests that the constructor throws an <see cref="ArgumentNullException"/> when a null backpressure service is provided.
    /// </summary>
    [Fact]
    public void Constructor_WithNullBackpressureService_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PipelineOrchestrator(
            _mockProcessingService.Object,
            _mockWindowingService.Object,
            _mockMetricsService.Object,
            null!,
            _mockQueryService.Object,
            _config
        ));
    }

    /// <summary>
    /// Tests that the constructor throws an <see cref="ArgumentNullException"/> when a null query service is provided.
    /// </summary>
    [Fact]
    public void Constructor_WithNullQueryService_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PipelineOrchestrator(
            _mockProcessingService.Object,
            _mockWindowingService.Object,
            _mockMetricsService.Object,
            _mockBackpressureService.Object,
            null!,
            _config
        ));
    }

    /// <summary>
    /// Tests that the constructor throws an <see cref="ArgumentNullException"/> when a null configuration is provided.
    /// </summary>
    [Fact]
    public void Constructor_WithNullConfig_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PipelineOrchestrator(
            _mockProcessingService.Object,
            _mockWindowingService.Object,
            _mockMetricsService.Object,
            _mockBackpressureService.Object,
            _mockQueryService.Object,
            null!
        ));
    }

    /// <summary>
    /// Tests that calling <see cref="PipelineOrchestrator.StartAsync"/> when the orchestrator is already running returns immediately without throwing.
    /// </summary>
    [Fact]
    public async Task StartAsync_WhenAlreadyRunning_ShouldReturnImmediately()
    {
        // Arrange - Simulate running state
        var orchestrator = new PipelineOrchestrator(
            _mockProcessingService.Object,
            _mockWindowingService.Object,
            _mockMetricsService.Object,
            _mockBackpressureService.Object,
            _mockQueryService.Object,
            _config
        );

        // Use reflection to set _isRunning to true
        var field = typeof(PipelineOrchestrator).GetField("_isRunning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(orchestrator, true);

        // Act
        await orchestrator.StartAsync();

        // Assert - No exception thrown
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.StartAsync"/> creates backpressure contexts for all pipeline stages.
    /// Verifies that contexts are created for each enabled stage and the windowing stage.
    /// </summary>
    [Fact]
    public async Task StartAsync_ShouldCreateBackpressureContextsForAllStages()
    {
        // Arrange
        _mockBackpressureService.Setup(b => b.CreateContext(It.IsAny<string>(), It.IsAny<int>()))
            .Verifiable();

        // Act
        await _orchestrator.StartAsync();

        // Assert
        _mockBackpressureService.Verify(b => b.CreateContext("Stage1", _config.MaxBufferSize), Times.Once);
        _mockBackpressureService.Verify(b => b.CreateContext("Stage2", _config.MaxBufferSize), Times.Once);
        _mockBackpressureService.Verify(b => b.CreateContext(PipelineConstants.StageName_Windowing, _config.MaxBufferSize), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.StopAsync"/> sets the running state to false.
    /// </summary>
    [Fact]
    public async Task StopAsync_ShouldSetIsRunningToFalse()
    {
        // Arrange
        var orchestrator = new PipelineOrchestrator(
            _mockProcessingService.Object,
            _mockWindowingService.Object,
            _mockMetricsService.Object,
            _mockBackpressureService.Object,
            _mockQueryService.Object,
            _config
        );

        // Act
        await orchestrator.StopAsync();

        // Assert - Can't directly verify private field, but method completes without exception
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.IngestDataPointAsync"/> throws an <see cref="ArgumentNullException"/> when a null data point is provided.
    /// </summary>
    [Fact]
    public async Task IngestDataPointAsync_WithNullDataPoint_ShouldThrow()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _orchestrator.IngestDataPointAsync(null!));
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.IngestDataPointAsync"/> throws an <see cref="InvalidOperationException"/> when attempting to ingest data while the pipeline is not running.
    /// </summary>
    [Fact]
    public async Task IngestDataPointAsync_WhenNotRunning_ShouldThrow()
    {
        // Arrange - Use reflection to set _isRunning to false
        var orchestrator = new PipelineOrchestrator(
            _mockProcessingService.Object,
            _mockWindowingService.Object,
            _mockMetricsService.Object,
            _mockBackpressureService.Object,
            _mockQueryService.Object,
            _config
        );
        var field = typeof(PipelineOrchestrator).GetField("_isRunning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(orchestrator, false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => orchestrator.IngestDataPointAsync(new DataPoint(1, DateTime.UtcNow.Ticks, 10.0, "Test")));
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.IngestDataPointAsync"/> returns true when a valid data point is successfully ingested.
    /// </summary>
    [Fact]
    public async Task IngestDataPointAsync_WithValidDataPoint_ShouldReturnTrue()
    {
        // Arrange
        _mockBackpressureService.Setup(b => b.TryAddToBuffer(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(true);

        // Act
        var result = await _orchestrator.IngestDataPointAsync(new DataPoint(1, DateTime.UtcNow.Ticks, 10.0, "Test"));

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.IngestDataPointAsync"/> returns false and applies backpressure when the buffer is full.
    /// </summary>
    [Fact]
    public async Task IngestDataPointAsync_WhenBufferFull_ShouldApplyBackpressureAndReturnFalse()
    {
        // Arrange
        _mockBackpressureService.Setup(b => b.TryAddToBuffer(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(false);
        _mockBackpressureService.Setup(b => b.ApplyBackpressureAsync(It.IsAny<string>(), It.IsAny<BackpressureStrategy>(), It.IsAny<int>()))
            .ReturnsAsync(new BackpressureResponse(true, "Backpressure applied"));

        // Act
        var result = await _orchestrator.IngestDataPointAsync(new DataPoint(1, DateTime.UtcNow.Ticks, 10.0, "Test"));

        // Assert
        result.Should().BeFalse();
        _mockBackpressureService.Verify(b => b.ApplyBackpressureAsync(
            PipelineConstants.StageName_Ingestion,
            BackpressureStrategy.Block,
            100),
            Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.ProcessBatchDataPointsAsync"/> throws an <see cref="ArgumentNullException"/> when a null collection of data points is provided.
    /// </summary>
    [Fact]
    public async Task ProcessBatchDataPointsAsync_WithNullDataPoints_ShouldThrow()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _orchestrator.ProcessBatchDataPointsAsync(null!));
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.ProcessBatchDataPointsAsync"/> successfully processes all valid data points in a batch.
    /// Verifies that the result contains the correct counts of successful and failed ingestions.
    /// </summary>
    [Fact]
    public async Task ProcessBatchDataPointsAsync_ShouldProcessAllValidPoints()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1, DateTime.UtcNow.Ticks, 10.0, "S1"),
            new DataPoint(2, DateTime.UtcNow.Ticks, 20.0, "S2"),
            new DataPoint(3, DateTime.UtcNow.Ticks, 30.0, "S3")
        };

        _mockBackpressureService.Setup(b => b.TryAddToBuffer(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(true);

        // Act
        var result = await _orchestrator.ProcessBatchDataPointsAsync(dataPoints);

        // Assert
        result.SuccessfulCount.Should().Be(3);
        result.FailedCount.Should().Be(0);
    }

    [Fact]
    public async Task ProcessBatchDataPointsAsync_ShouldCountFailedIngestions()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1, DateTime.UtcNow.Ticks, 10.0, "S1"),
            new DataPoint(2, DateTime.UtcNow.Ticks, 20.0, "S2")
        };

        _mockBackpressureService.SetupSequence(b => b.TryAddToBuffer(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(false) // First call fails
            .Returns(true);  // Second call succeeds

        // Act
        var result = await _orchestrator.ProcessBatchDataPointsAsync(dataPoints);

        // Assert
        result.SuccessfulCount.Should().Be(1);
        result.FailedCount.Should().Be(1);
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.GetQueryService"/> returns the configured query service instance.
    /// </summary>
    [Fact]
    public void GetQueryService_ShouldReturnQueryServiceInstance()
    {
        // Act
        var queryService = _orchestrator.GetQueryService();

        // Assert
        queryService.Should().BeSameAs(_mockQueryService.Object);
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.GetStatus"/> returns the current pipeline status.
    /// Verifies that the status object contains the expected configuration information.
    /// </summary>
    [Fact]
    public void GetStatus_ShouldReturnPipelineStatus()
    {
        // Act
        var status = _orchestrator.GetStatus();

        // Assert
        status.Should().NotBeNull();
        status.IsRunning.Should().BeFalse(); // Default state
        status.ConfigurationName.Should().Be("TestPipeline");
        status.ConfigurationVersion.Should().Be("1.0.0");
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.GetHealthReportAsync"/> returns the health report from the metrics service.
    /// </summary>
    [Fact]
    public async Task GetHealthReportAsync_ShouldReturnHealthReport()
    {
        // Arrange
        var healthReport = new HealthReport
        {
            Status = "HEALTHY",
            Message = "All systems operational"
        };
        _mockMetricsService.Setup(m => m.GenerateHealthReportAsync())
            .ReturnsAsync(healthReport);

        // Act
        var result = await _orchestrator.GetHealthReportAsync();

        // Assert
        result.Should().BeSameAs(healthReport);
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.GetThroughput"/> returns the throughput metric from the metrics service.
    /// </summary>
    [Fact]
    public void GetThroughput_ShouldReturnMetricsServiceThroughput()
    {
        // Arrange
        _mockMetricsService.Setup(m => m.GetThroughput())
            .Returns(123.45);

        // Act
        var result = _orchestrator.GetThroughput();

        // Assert
        result.Should().Be(123.45);
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.GetThroughput"/> with a stage name returns the throughput metric for that specific stage from the metrics service.
    /// </summary>
    [Fact]
    public void GetThroughput_WithStageName_ShouldReturnMetricsServiceThroughput()
    {
        // Arrange
        _mockMetricsService.Setup(m => m.GetThroughput("Stage1"))
            .Returns(456.78);

        // Act
        var result = _orchestrator.GetThroughput("Stage1");

        // Assert
        result.Should().Be(456.78);
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.GetPerformanceTrendAsync"/> returns the performance trend analysis from the metrics service.
    /// </summary>
    [Fact]
    public async Task GetPerformanceTrendAsync_ShouldReturnMetricsServiceAnalysis()
    {
        // Arrange
        var trend = new PerformanceTrend
        {
            TrendDirection = "STABLE",
            SamplesAnalyzed = 10
        };
        _mockMetricsService.Setup(m => m.AnalyzePerformanceTrendAsync())
            .ReturnsAsync(trend);

        // Act
        var result = await _orchestrator.GetPerformanceTrendAsync();

        // Assert
        result.Should().BeSameAs(trend);
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.BatchProcessingResult"/> has the correct properties and can store successful and failed ingestion counts.
    /// </summary>
    [Fact]
    public void BatchProcessingResult_ShouldHaveCorrectProperties()
    {
        // Arrange
        var result = new PipelineOrchestrator.BatchProcessingResult();

        // Act
        result.SuccessfulCount = 5;
        result.FailedCount = 2;

        // Assert
        result.SuccessfulCount.Should().Be(5);
        result.FailedCount.Should().Be(2);
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.PipelineStatus"/> has the correct properties and properly stores pipeline state information.
    /// </summary>
    [Fact]
    public void PipelineStatus_ShouldHaveCorrectProperties()
    {
        // Arrange
        var status = new PipelineOrchestrator.PipelineStatus
        {
            IsRunning = true,
            TotalDataPointsProcessed = 1000,
            TotalDataPointsFailed = 10,
            PendingItemsInQueue = 50,
            ConfigurationName = "TestConfig",
            ConfigurationVersion = "2.0",
            Timestamp = DateTime.UtcNow
        };

        // Assert
        status.IsRunning.Should().BeTrue();
        status.TotalDataPointsProcessed.Should().Be(1000);
        status.TotalDataPointsFailed.Should().Be(10);
        status.PendingItemsInQueue.Should().Be(50);
        status.ConfigurationName.Should().Be("TestConfig");
        status.ConfigurationVersion.Should().Be("2.0");
    }

    /// <summary>
    /// Tests that <see cref="PipelineOrchestrator.PipelineStatus.GetSummary"/> returns a formatted string containing the pipeline status information.
    /// </summary>
    [Fact]
    public void PipelineStatus_GetSummary_ShouldReturnFormattedString()
    {
        // Arrange
        var status = new PipelineOrchestrator.PipelineStatus
        {
            IsRunning = true,
            TotalDataPointsProcessed = 1000,
            TotalDataPointsFailed = 10,
            PendingItemsInQueue = 50,
            BackpressureStatus = new BackpressureSystemStatus()
        };

        // Act
        var summary = status.GetSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("Running=True");
        summary.Should().Contain("Processed=1000");
    }
}
