// tests/DataProcessingServiceExtensionsTests.cs
namespace DotNetRealtimePipeline.Tests;

public class DataProcessingServiceExtensionsTests
{
    [Fact]
    public async Task ProcessBatchWithQualityFilterAsync_HappyPath_ReturnsResults()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { Quality = 90 },
            new DataPoint { Quality = 80 },
            new DataPoint { Quality = 70 }
        };
        var service = new DataProcessingService(new MetricsRepository(), new PipelineMetrics());

        // Act
        var results = await DataProcessingServiceExtensions.ProcessBatchWithQualityFilterAsync(service, dataPoints, 85);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task ProcessBatchWithQualityFilterAsync_NullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => DataProcessingServiceExtensions.ProcessBatchWithQualityFilterAsync(null, new List<DataPoint>(), 85));
    }

    [Fact]
    public async Task ProcessBatchWithQualityFilterAsync_NullDataPoints_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => DataProcessingServiceExtensions.ProcessBatchWithQualityFilterAsync(new DataProcessingService(new MetricsRepository(), new PipelineMetrics()), null, 85));
    }

    [Fact]
    public async Task GetProcessedDataWithAnalysisAsync_HappyPath_ReturnsDataPointsAndAnalysis()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { Quality = 90 },
            new DataPoint { Quality = 80 },
            new DataPoint { Quality = 70 }
        };
        var service = new DataProcessingService(new MetricsRepository(), new PipelineMetrics());

        // Act
        var (dataPointsResult, analysis) = await DataProcessingServiceExtensions.GetProcessedDataWithAnalysisAsync(service, 1643723400, 1643723405, true);

        // Assert
        Assert.NotNull(dataPointsResult);
        Assert.NotNull(analysis);
    }

    [Fact]
    public async Task GetProcessedDataWithAnalysisAsync_NullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => DataProcessingServiceExtensions.GetProcessedDataWithAnalysisAsync(null, 1643723400, 1643723405, true));
    }

    [Fact]
    public async Task GetProcessedDataWithAnalysisAsync_EmptyDataPoints_ReturnsEmptyDataPointsAndNullAnalysis()
    {
        // Arrange
        var service = new DataProcessingService(new MetricsRepository(), new PipelineMetrics());

        // Act
        var (dataPointsResult, analysis) = await DataProcessingServiceExtensions.GetProcessedDataWithAnalysisAsync(service, 1643723400, 1643723405, true);

        // Assert
        Assert.NotNull(dataPointsResult);
        Assert.Null(analysis);
    }

    [Fact]
    public void GenerateQualityReportString_HappyPath_ReturnsReportString()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { Quality = 90 },
            new DataPoint { Quality = 80 },
            new DataPoint { Quality = 70 }
        };
        var service = new DataProcessingService(new MetricsRepository(), new PipelineMetrics());

        // Act
        var reportString = DataProcessingServiceExtensions.GenerateQualityReportString(service, dataPoints);

        // Assert
        Assert.NotNull(reportString);
    }

    [Fact]
    public void GenerateQualityReportString_NullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DataProcessingServiceExtensions.GenerateQualityReportString(null, new List<DataPoint>()));
    }

    [Fact]
    public void GenerateQualityReportString_NullDataPoints_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DataProcessingServiceExtensions.GenerateQualityReportString(new DataProcessingService(new MetricsRepository(), new PipelineMetrics()), null));
    }

    [Fact]
    public async Task GetProcessingStatisticsAsync_HappyPath_ReturnsStatistics()
    {
        // Arrange
        var service = new DataProcessingService(new MetricsRepository(), new PipelineMetrics());

        // Act
        var statistics = await DataProcessingServiceExtensions.GetProcessingStatisticsAsync(service);

        // Assert
        Assert.NotNull(statistics);
    }

    [Fact]
    public async Task GetProcessingStatisticsAsync_NullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => DataProcessingServiceExtensions.GetProcessingStatisticsAsync(null));
    }
}
