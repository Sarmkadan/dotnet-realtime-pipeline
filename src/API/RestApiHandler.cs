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
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

/// <summary>
/// Handler for data ingestion endpoint.
/// </summary>
public class DataIngestionHandler : ApiEndpointHandler
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
    public async Task<ApiResponse<bool>> IngestAsync(DataPoint dataPoint)
    {
        try
        {
            if (dataPoint == null)
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
    public async Task<ApiResponse<BatchIngestResult>> IngestBatchAsync(List<DataPoint> dataPoints)
    {
        try
        {
            if (dataPoints == null || dataPoints.Count == 0)
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

public class BatchIngestResult
{
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public int TotalCount { get; set; }
}

/// <summary>
/// Handler for pipeline status endpoint.
/// </summary>
public class StatusHandler : ApiEndpointHandler
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
    public async Task<ApiResponse<PipelineStatusInfo>> GetStatusAsync()
    {
        try
        {
            var status = _orchestrator.GetStatus();
            var health = await _orchestrator.GetHealthReportAsync();

            var response = new PipelineStatusInfo
            {
                PipelineName = status.PipelineName,
                Version = status.Version,
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

public class PipelineStatusInfo
{
    public string PipelineName { get; set; }
    public string Version { get; set; }
    public bool IsRunning { get; set; }
    public long TotalProcessed { get; set; }
    public long TotalFailed { get; set; }
    public int Pending { get; set; }
    public string HealthStatus { get; set; }
    public double Throughput { get; set; }
    public double SuccessRate { get; set; }
    public double AverageLatency { get; set; }
}

/// <summary>
/// Handler for query endpoint.
/// </summary>
public class QueryHandler : ApiEndpointHandler
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
public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiErrorResponse BadRequest(string message)
    {
        return new ApiErrorResponse { StatusCode = 400, Message = message, ErrorCode = "BAD_REQUEST" };
    }

    public static ApiErrorResponse NotFound(string message)
    {
        return new ApiErrorResponse { StatusCode = 404, Message = message, ErrorCode = "NOT_FOUND" };
    }

    public static ApiErrorResponse InternalError(string message)
    {
        return new ApiErrorResponse { StatusCode = 500, Message = message, ErrorCode = "INTERNAL_ERROR" };
    }

    public static ApiErrorResponse TooManyRequests(string message)
    {
        return new ApiErrorResponse { StatusCode = 429, Message = message, ErrorCode = "RATE_LIMIT" };
    }
}
