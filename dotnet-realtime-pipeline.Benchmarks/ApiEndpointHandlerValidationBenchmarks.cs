using BenchmarkDotNet.Attributes;
using DotNetRealtimePipeline.API;
using System;
using System.Collections.Generic;

namespace DotNetRealtimePipeline.Benchmarks;

[MemoryDiagnoser]
public class ApiEndpointHandlerValidationBenchmarks
{
    private ApiEndpointHandler.ApiResponse<object> _validApiResponse;
    private ApiEndpointHandler.ApiResponse<object> _invalidApiResponse;
    private BatchIngestResult _validBatchResult;
    private BatchIngestResult _invalidBatchResult;
    private PipelineStatusInfo _validStatusInfo;
    private PipelineStatusInfo _invalidStatusInfo;

    [GlobalSetup]
    public void Setup()
    {
        _validApiResponse = new ApiEndpointHandler.ApiResponse<object>
        {
            Success = true,
            Data = new object(),
            Message = "Valid message",
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        };

        _invalidApiResponse = new ApiEndpointHandler.ApiResponse<object>
        {
            Success = false,
            Data = new object(),
            Message = "", // Invalid: empty string
            StatusCode = 0, // Invalid: must be 100-599
            Timestamp = default // Invalid: default DateTime
        };

        _validBatchResult = new BatchIngestResult
        {
            SuccessfulCount = 10,
            FailedCount = 5,
            TotalCount = 15
        };

        _invalidBatchResult = new BatchIngestResult
        {
            SuccessfulCount = -1, // Invalid: negative
            FailedCount = 0,
            TotalCount = 0
        };

        _validStatusInfo = new PipelineStatusInfo
        {
            PipelineName = "TestPipeline",
            Version = "v1.0.0",
            IsRunning = true,
            TotalProcessed = 100,
            TotalFailed = 0,
            Pending = 0,
            HealthStatus = "Healthy"
        };

        _invalidStatusInfo = new PipelineStatusInfo
        {
            PipelineName = "", // Invalid: empty
            Version = "1.0.0", // Invalid: missing 'v'
            IsRunning = true,
            TotalProcessed = -1, // Invalid: negative
            TotalFailed = 0,
            Pending = 0,
            HealthStatus = "" // Invalid: empty
        };
    }

    [Benchmark]
    public void ValidateApiResponse_Valid() => _validApiResponse.Validate();

    [Benchmark]
    public void ValidateApiResponse_Invalid() => _invalidApiResponse.Validate();

    [Benchmark]
    public void ValidateBatchIngestResult_Valid() => _validBatchResult.Validate();

    [Benchmark]
    public void ValidateBatchIngestResult_Invalid() => _invalidBatchResult.Validate();

    [Benchmark]
    public void ValidatePipelineStatusInfo_Valid() => _validStatusInfo.Validate();

    [Benchmark]
    public void ValidatePipelineStatusInfo_Invalid() => _invalidStatusInfo.Validate();
}
