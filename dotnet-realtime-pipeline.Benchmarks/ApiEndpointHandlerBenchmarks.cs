#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using DotNetRealtimePipeline.API;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;

namespace DotNetRealtimePipeline.Benchmarks
{
    /// <summary>
    /// Benchmarks for the public API endpoint handlers defined in <c>RestApiHandler.cs</c>.
    /// Covers the most frequently used handler methods and the static error‑response factories.
    /// </summary>
    [MemoryDiagnoser]
    public class ApiEndpointHandlerBenchmarks
    {
        private Mock<PipelineOrchestrator> _orchestratorMock = null!;
        private DataIngestionHandler _dataIngestionHandler = null!;
        private DataPoint _sampleDataPoint = null!;
        private ILogger<DataIngestionHandler> _logger = null!;

        /// <summary>
        /// Sets up a mock <c>PipelineOrchestrator</c> and a minimal <c>DataPoint</c> instance.
        /// The mock returns successful results for the methods used by the handlers.
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            // Use a no‑op logger to avoid any I/O during benchmarks.
            _logger = NullLogger<DataIngestionHandler>.Instance;

            // Create a mock orchestrator that always reports success for ingestion.
            _orchestratorMock = new Mock<PipelineOrchestrator>();
            _orchestratorMock
                .Setup(o => o.IngestDataPointAsync(It.IsAny<DataPoint>()))
                .ReturnsAsync(true);

            // Instantiate the handler under test.
            _dataIngestionHandler = new DataIngestionHandler(_orchestratorMock.Object, _logger);

            // Prepare a simple DataPoint instance. Its internal fields are not used by the handler
            // beyond the null‑check, so a default instance is sufficient.
            _sampleDataPoint = new DataPoint();
        }

        /// <summary>
        /// Benchmarks the single data‑point ingestion endpoint.
        /// </summary>
        [Benchmark]
        public async Task IngestAsync()
        {
            await _dataIngestionHandler.IngestAsync(_sampleDataPoint);
        }

        /// <summary>
        /// Benchmarks the static factory method for a 400 Bad Request error response.
        /// </summary>
        [Benchmark]
        public ApiErrorResponse BadRequest()
        {
            return ApiErrorResponse.BadRequest("sample bad request");
        }

        /// <summary>
        /// Benchmarks the static factory method for a 404 Not Found error response.
        /// </summary>
        [Benchmark]
        public ApiErrorResponse NotFound()
        {
            return ApiErrorResponse.NotFound("sample not found");
        }

        /// <summary>
        /// Benchmarks the static factory method for a 500 Internal Server Error response.
        /// </summary>
        [Benchmark]
        public ApiErrorResponse InternalError()
        {
            return ApiErrorResponse.InternalError("sample internal error");
        }

        /// <summary>
        /// Benchmarks the static factory method for a 429 Too Many Requests error response.
        /// </summary>
        [Benchmark]
        public ApiErrorResponse TooManyRequests()
        {
            return ApiErrorResponse.TooManyRequests("sample rate limit");
        }
    }
}
