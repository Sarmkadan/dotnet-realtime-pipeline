#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Models;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Extension methods for <see cref="DataProcessingServiceTests"/> to reduce boilerplate and improve test clarity.
/// Provides factory methods for creating test data and service dependencies.
/// </summary>
public static class DataProcessingServiceTestsExtensions
{
    /// <summary>
    /// Creates a new valid <see cref="DataPoint"/> instance with default values.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="id">The data point ID. Defaults to 1.</param>
    /// <param name="value">The data point value. Defaults to 10.0.</param>
    /// <param name="source">The data point source. Defaults to "Sensor1".</param>
    /// <param name="quality">The data quality score (0-100). Defaults to 80.</param>
    /// <returns>A new valid <see cref="DataPoint"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static DataPoint CreateValidDataPoint(
        this DataProcessingServiceTests tests,
        long id = 1,
        double value = 10.0,
        string source = "Sensor1",
        int quality = 80)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new DataPoint(id, DateTime.UtcNow.Ticks, value, source) { Quality = quality };
    }

    /// <summary>
    /// Creates a new invalid <see cref="DataPoint"/> instance with low quality.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="id">The data point ID. Defaults to 1.</param>
    /// <param name="value">The data point value. Defaults to 10.0.</param>
    /// <param name="source">The data point source. Defaults to "Sensor1".</param>
    /// <returns>A new invalid <see cref="DataPoint"/> instance with low quality.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static DataPoint CreateLowQualityDataPoint(
        this DataProcessingServiceTests tests,
        long id = 1,
        double value = 10.0,
        string source = "Sensor1")
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new DataPoint(id, DateTime.UtcNow.Ticks, value, source) { Quality = 30 };
    }

    /// <summary>
    /// Creates a new invalid <see cref="DataPoint"/> instance with zero/empty properties.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>A new invalid <see cref="DataPoint"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static DataPoint CreateInvalidDataPoint(this DataProcessingServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new DataPoint(0, 0, 0, string.Empty);
    }

    /// <summary>
    /// Creates a new <see cref="PipelineConfig"/> instance with default test values.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="validateOnIngestion">Whether to validate on ingestion. Defaults to true.</param>
    /// <param name="minDataQualityThreshold">Minimum quality threshold. Defaults to 50.</param>
    /// <param name="maxRetries">Maximum retry attempts. Defaults to 2.</param>
    /// <param name="retryDelayMs">Retry delay in milliseconds. Defaults to 10.</param>
    /// <returns>A new <see cref="PipelineConfig"/> instance configured for testing.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static PipelineConfig CreateTestPipelineConfig(
        this DataProcessingServiceTests tests,
        bool validateOnIngestion = true,
        int minDataQualityThreshold = 50,
        int maxRetries = 2,
        int retryDelayMs = 10)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new PipelineConfig
        {
            ValidateOnIngestion = validateOnIngestion,
            MinDataQualityThreshold = minDataQualityThreshold,
            MaxRetries = maxRetries,
            RetryDelayMs = retryDelayMs
        };
    }

    /// <summary>
    /// Creates a new <see cref="ProcessingResult"/> instance representing a successful operation.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="resultId">The result ID. Defaults to 1.</param>
    /// <param name="stageName">The stage name. Defaults to "Output".</param>
    /// <returns>A new successful <see cref="ProcessingResult"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static ProcessingResult CreateSuccessfulResult(
        this DataProcessingServiceTests tests,
        int resultId = 1,
        string stageName = "Output")
    {
        ArgumentNullException.ThrowIfNull(tests);

        var result = new ProcessingResult(resultId, true, stageName);
        result.MarkSuccess();
        return result;
    }

    /// <summary>
    /// Creates a new <see cref="ProcessingResult"/> instance representing a failed operation.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="resultId">The result ID. Defaults to 1.</param>
    /// <param name="errorMessage">The error message. Defaults to "Test failure".</param>
    /// <param name="stageName">The stage name. Defaults to "Ingestion".</param>
    /// <returns>A new failed <see cref="ProcessingResult"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static ProcessingResult CreateFailedResult(
        this DataProcessingServiceTests tests,
        int resultId = 1,
        string errorMessage = "Test failure",
        string stageName = "Ingestion")
    {
        ArgumentNullException.ThrowIfNull(tests);

        var result = new ProcessingResult(resultId, false, stageName);
        result.MarkFailure(errorMessage);
        return result;
    }
}