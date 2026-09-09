#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.API;

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Base handler for REST API endpoints.
/// Provides common patterns for request handling, validation, and response formatting.
/// </summary>
public abstract class ApiEndpointHandler
{
    protected readonly ILogger _logger;

    protected ApiEndpointHandler(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Represents an API response.
    /// </summary>
    public sealed class ApiResponse<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the request was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code associated with the response.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time when the response was created.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

/// <summary>
/// Handler for data ingestion endpoint.
/// </summary>
public sealed class DataIngestionHandler : ApiEndpointHandler
{
    private readonly PipelineOrchestrator _orchestrator;

    public DataIngestionHandler(PipelineOrchestrator orchestrator, ILogger<DataIngestionHandler> logger)
        : base(logger)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <summary>
    /// Handles data point ingestion request.
    /// </summary>
    /// <param name="dataPoint">The data point to ingest.</param>
    /// <returns>A response indicating whether the data point was ingested successfully.</returns>
    public async Task<ApiResponse<bool>> IngestAsync(DataPoint dataPoint)
    {
        try
        {
            if (dataPoint is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Data point is null",
                    StatusCode = 400
                };
            }

            var result = await _orchestrator.IngestDataPointAsync(dataPoint);

            return new ApiResponse<bool>
            {
                Success = result,
                Data = result,
                Message = result ? "Data point ingested successfully" : "Failed to ingest data point due to backpressure",
                StatusCode = result ? 200 : 429
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ingesting data point");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message,
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Handles batch data ingestion.
    /// </summary>
    /// <param name="dataPoints">The data points to ingest.</param>
    /// <returns>A response containing the batch ingestion result.</returns>
    public async Task<ApiResponse<BatchIngestResult>> IngestBatchAsync(List<DataPoint> dataPoints)
    {
        try
        {
            if (dataPoints is null || dataPoints.Count == 0)
            {
                return new ApiResponse<BatchIngestResult>
                {
                    Success = false,
                    Message = "Data points list is empty",
                    StatusCode = 400
                };
            }

            var result = await _orchestrator.ProcessBatchDataPointsAsync(dataPoints);

            return new ApiResponse<BatchIngestResult>
            {
                Success = true,
                Data = new BatchIngestResult
                {
                    SuccessfulCount = result.SuccessfulCount,
                    FailedCount = result.FailedCount,
                    TotalCount = dataPoints.Count
                },
                Message = $"Batch processed: {result.SuccessfulCount} successful, {result.FailedCount} failed",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch");
            return new ApiResponse<BatchIngestResult>
            {
                Success = false,
                Message = ex.Message,
                StatusCode = 500
            };
        }
    }
}

public sealed class BatchIngestResult
{
    /// <summary>
    /// Gets or sets the number of data points ingested successfully.
    /// </summary>
    public int SuccessfulCount { get; set; }

    /// <summary>
    /// Gets or sets the number of data points that failed ingestion.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of data points in the batch.
    /// </summary>
    public int TotalCount { get; set; }
}

/// <summary>
/// Handler for pipeline status endpoint.
/// </summary>
public sealed class StatusHandler : ApiEndpointHandler
{
    private readonly PipelineOrchestrator _orchestrator;

    public StatusHandler(PipelineOrchestrator orchestrator, ILogger<StatusHandler> logger)
        : base(logger)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <summary>
    /// Handles status request.
    /// </summary>
    /// <returns>A response containing the current pipeline status.</returns>
    public async Task<ApiResponse<PipelineStatusInfo>> GetStatusAsync()
    {
        try
        {
            var status = _orchestrator.GetStatus();
            var health = await _orchestrator.GetHealthReportAsync();

            var response = new PipelineStatusInfo
            {
                PipelineName = status.ConfigurationName,
                Version = status.ConfigurationVersion,
                IsRunning = status.IsRunning,
                TotalProcessed = status.TotalDataPointsProcessed,
                TotalFailed = status.TotalDataPointsFailed,
                Pending = status.PendingItemsInQueue,
                HealthStatus = health?.Status.ToString(),
                Throughput = health?.ThroughputItemsPerSecond ?? 0,
                SuccessRate = health?.SuccessRatePercent ?? 0,
                AverageLatency = health?.AverageProcessingTimeMs ?? 0
            };

            return new ApiResponse<PipelineStatusInfo>
            {
                Success = true,
                Data = response,
                Message = "Pipeline status retrieved",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pipeline status");
            return new ApiResponse<PipelineStatusInfo>
            {
                Success = false,
                Message = ex.Message,
                StatusCode = 500
            };
        }
    }
}

public sealed class PipelineStatusInfo
{
    /// <summary>
    /// Gets or sets the pipeline configuration name.
    /// </summary>
    public string PipelineName { get; set; }

    /// <summary>
    /// Gets or sets the pipeline configuration version.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the pipeline is running.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Gets or sets the total number of processed data points.
    /// </summary>
    public long TotalProcessed { get; set; }

    /// <summary>
    /// Gets or sets the total number of data points that failed processing.
    /// </summary>
    public long TotalFailed { get; set; }

    /// <summary>
    /// Gets or sets the number of items pending in the pipeline queue.
    /// </summary>
    public int Pending { get; set; }

    /// <summary>
    /// Gets or sets the pipeline health status.
    /// </summary>
    public string HealthStatus { get; set; }

    /// <summary>
    /// Gets or sets the pipeline throughput in items per second.
    /// </summary>
    public double Throughput { get; set; }

    /// <summary>
    /// Gets or sets the processing success rate as a percentage.
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// Gets or sets the average processing latency in milliseconds.
    /// </summary>
    public double AverageLatency { get; set; }
}

/// <summary>
/// Handler for query endpoint.
/// </summary>
public sealed class QueryHandler : ApiEndpointHandler
{
    private readonly PipelineOrchestrator _orchestrator;

    public QueryHandler(PipelineOrchestrator orchestrator, ILogger<QueryHandler> logger)
        : base(logger)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <summary>
    /// Handles data query request.
    /// </summary>
    /// <param name="startMs">The inclusive start of the query range in milliseconds.</param>
    /// <param name="endMs">The inclusive end of the query range in milliseconds.</param>
    /// <param name="source">The source by which to filter data points, or an empty string to include all sources.</param>
    /// <param name="minQuality">The minimum quality value for returned data points.</param>
    /// <returns>A response containing the data points that match the query.</returns>
    public async Task<ApiResponse<List<DataPoint>>> QueryAsync(long startMs, long endMs, string source = "", int minQuality = 0)
    {
        try
        {
            var queryService = _orchestrator.GetQueryService();
            var results = await queryService.SearchDataPointsAsync(startMs, endMs, source, minQuality);

            return new ApiResponse<List<DataPoint>>
            {
                Success = true,
                Data = results,
                Message = $"Query returned {results.Count} results",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query");
            return new ApiResponse<List<DataPoint>>
            {
                Success = false,
                Message = ex.Message,
                StatusCode = 500
            };
        }
    }
}

/// <summary>
/// API error response.
/// </summary>
public sealed class ApiErrorResponse
{
    /// <summary>
    /// Gets or sets the HTTP status code associated with the error.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the machine-readable error code.
    /// </summary>
    public string ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the error response was created.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a response for a bad request error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A bad request error response.</returns>
    public static ApiErrorResponse BadRequest(string message)
    {
        return new ApiErrorResponse { StatusCode = 400, Message = message, ErrorCode = "BAD_REQUEST" };
    }

    /// <summary>
    /// Creates a response for a resource not found error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A resource not found error response.</returns>
    public static ApiErrorResponse NotFound(string message)
    {
        return new ApiErrorResponse { StatusCode = 404, Message = message, ErrorCode = "NOT_FOUND" };
    }

    /// <summary>
    /// Creates a response for an internal server error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>An internal server error response.</returns>
    public static ApiErrorResponse InternalError(string message)
    {
        return new ApiErrorResponse { StatusCode = 500, Message = message, ErrorCode = "INTERNAL_ERROR" };
    }

    /// <summary>
    /// Creates a response for a rate limit error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A rate limit error response.</returns>
    public static ApiErrorResponse TooManyRequests(string message)
    {
        return new ApiErrorResponse { StatusCode = 429, Message = message, ErrorCode = "RATE_LIMIT" };
    }
}
