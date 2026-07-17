#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Initialization;

using DotNetRealtimePipeline.Configuration;
using DotNetRealtimePipeline.Events;
using DotNetRealtimePipeline.Integration;
using DotNetRealtimePipeline.Middleware;
using DotNetRealtimePipeline.Monitoring;
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.State;
using DotNetRealtimePipeline.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

/// <summary>
/// Orchestrates complete pipeline initialization with all components.
/// Ensures proper startup sequence and dependency resolution.
/// </summary>
public sealed class PipelineInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PipelineInitializer> _logger;
    private readonly PipelineStateManager _stateManager;
    private bool _isInitialized;

    public PipelineInitializer(IServiceProvider serviceProvider, ILogger<PipelineInitializer> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stateManager = serviceProvider.GetRequiredService<PipelineStateManager>();
    }

    /// <summary>
    /// Initializes all pipeline components.
    /// </summary>
    public async Task<InitializationResult> InitializeAsync()
    {
        var result = new InitializationResult { StartTime = DateTime.UtcNow };

        try
        {
            _logger.LogInformation("=== Pipeline Initialization Started ===");

            _stateManager.TransitionTo(State.PipelineState.Initializing, "Initialization started");

            // Initialize core services
            InitializeCoreServices();

            // Initialize event system
            InitializeEventSubscribers();

            // Initialize middleware
            InitializeMiddleware();

            // Initialize background workers
            await InitializeWorkersAsync();

            // Initialize external integrations
            InitializeIntegrations();

            // Initialize monitoring
            InitializeMonitoring();

            _isInitialized = true;
            result.Success = true;
            result.ComponentsInitialized = 8;

            _logger.LogInformation("=== Pipeline Initialization Completed Successfully ===");
            _logger.LogInformation("Components initialized: {Count}", result.ComponentsInitialized);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pipeline initialization failed");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _stateManager.TransitionTo(State.PipelineState.Failed, "Initialization failed");

            return result;
        }
        finally
        {
            result.EndTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Starts the pipeline after initialization.
    /// </summary>
    public async Task<bool> StartAsync()
    {
        if (!_isInitialized)
        {
            _logger.LogError("Pipeline must be initialized before starting");
            return false;
        }

        try
        {
            _logger.LogInformation("Starting pipeline...");

            var orchestrator = _serviceProvider.GetRequiredService<PipelineOrchestrator>();
            await orchestrator.StartAsync();

            _stateManager.TransitionTo(State.PipelineState.Running, "Pipeline started");

            // Start background workers
            var coordinator = _serviceProvider.GetRequiredService<WorkerCoordinator>();
            coordinator.StartAll();

            _logger.LogInformation("Pipeline started successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start pipeline");
            _stateManager.TransitionTo(State.PipelineState.Failed, "Failed to start");
            return false;
        }
    }

    /// <summary>
    /// Stops the pipeline gracefully.
    /// </summary>
    public async Task<bool> StopAsync()
    {
        try
        {
            _logger.LogInformation("Stopping pipeline...");

            // Stop background workers
            var coordinator = _serviceProvider.GetRequiredService<WorkerCoordinator>();
            await coordinator.StopAllAsync();

            // Stop pipeline orchestrator
            var orchestrator = _serviceProvider.GetRequiredService<PipelineOrchestrator>();
            await orchestrator.StopAsync();

            _stateManager.TransitionTo(State.PipelineState.Stopped, "Pipeline stopped");

            _logger.LogInformation("Pipeline stopped successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping pipeline");
            return false;
        }
    }

    /// <summary>
    /// Initializes core pipeline services.
    /// </summary>
    private void InitializeCoreServices()
    {
        _logger.LogInformation("Initializing core services...");

        try
        {
            var orchestrator = _serviceProvider.GetRequiredService<PipelineOrchestrator>();
            _logger.LogInformation("✓ Pipeline orchestrator initialized");

            var dataProcessing = _serviceProvider.GetRequiredService<DataProcessingService>();
            _logger.LogInformation("✓ Data processing service initialized");

            var metrics = _serviceProvider.GetRequiredService<MetricsService>();
            _logger.LogInformation("✓ Metrics service initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing core services");
            throw;
        }
    }

    /// <summary>
    /// Initializes event subscribers.
    /// </summary>
    private void InitializeEventSubscribers()
    {
        _logger.LogInformation("Initializing event subscribers...");

        try
        {
            var publisher = _serviceProvider.GetRequiredService<PipelineEventPublisher>();

            var dataIngestSubscriber = _serviceProvider.GetRequiredService<DataIngestSubscriber>();
            dataIngestSubscriber.Subscribe();
            _logger.LogInformation("✓ Data ingestion subscriber registered");

            var processingSubscriber = _serviceProvider.GetRequiredService<ProcessingCompletionSubscriber>();
            processingSubscriber.Subscribe();
            _logger.LogInformation("✓ Processing completion subscriber registered");

            var backpressureSubscriber = _serviceProvider.GetRequiredService<BackpressureAlertSubscriber>();
            backpressureSubscriber.Subscribe();
            _logger.LogInformation("✓ Backpressure alert subscriber registered");

            var metricsSubscriber = _serviceProvider.GetRequiredService<MetricsAggregationSubscriber>();
            metricsSubscriber.Subscribe();
            _logger.LogInformation("✓ Metrics aggregation subscriber registered");

            var errorSubscriber = _serviceProvider.GetRequiredService<ErrorAlertSubscriber>();
            errorSubscriber.Subscribe();
            _logger.LogInformation("✓ Error alert subscriber registered");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing event subscribers");
            throw;
        }
    }

    /// <summary>
    /// Initializes middleware components.
    /// </summary>
    private void InitializeMiddleware()
    {
        _logger.LogInformation("Initializing middleware components...");

        try
        {
            _serviceProvider.GetRequiredService<LoggingMiddleware>();
            _logger.LogInformation("✓ Logging middleware initialized");

            _serviceProvider.GetRequiredService<ErrorHandlingMiddleware>();
            _logger.LogInformation("✓ Error handling middleware initialized");

            _serviceProvider.GetRequiredService<RateLimitingMiddleware>();
            _logger.LogInformation("✓ Rate limiting middleware initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing middleware");
            throw;
        }
    }

    /// <summary>
    /// Initializes background workers.
    /// </summary>
    private async Task InitializeWorkersAsync()
    {
        _logger.LogInformation("Initializing background workers...");

        try
        {
            _serviceProvider.GetRequiredService<BackgroundProcessingWorker>();
            _logger.LogInformation("✓ Background processing worker initialized");

            _serviceProvider.GetRequiredService<MetricsAggregationWorker>();
            _logger.LogInformation("✓ Metrics aggregation worker initialized");

            _serviceProvider.GetRequiredService<HealthCheckWorker>();
            _logger.LogInformation("✓ Health check worker initialized");

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing workers");
            throw;
        }
    }

    /// <summary>
    /// Initializes external integrations.
    /// </summary>
    private void InitializeIntegrations()
    {
        _logger.LogInformation("Initializing external integrations...");

        try
        {
            _serviceProvider.GetRequiredService<PipelineHttpClientFactory>();
            _logger.LogInformation("✓ HTTP client factory initialized");

            _logger.LogInformation("✓ External integrations initialized");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Warning: External integrations initialization incomplete");
        }
    }

    /// <summary>
    /// Initializes monitoring components.
    /// </summary>
    private void InitializeMonitoring()
    {
        _logger.LogInformation("Initializing monitoring components...");

        try
        {
            _serviceProvider.GetRequiredService<HealthCheckService>();
            _logger.LogInformation("✓ Health check service initialized");

            _serviceProvider.GetRequiredService<ResourceMonitor>();
            _logger.LogInformation("✓ Resource monitor initialized");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Warning: Monitoring components initialization incomplete");
        }
    }

    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Gets the service provider.
    /// </summary>
    /// <returns>The service provider instance.</returns>
    internal IServiceProvider GetServiceProvider() => _serviceProvider;

    /// <summary>
    /// Gets the logger.
    /// </summary>
    /// <returns>The logger instance.</returns>
    internal ILogger<PipelineInitializer> GetLogger() => _logger;

    /// <summary>
    /// Gets the state manager.
    /// </summary>
    /// <returns>The state manager instance.</returns>
    internal PipelineStateManager GetStateManager() => _stateManager;
}

/// <summary>
/// Result of pipeline initialization.
/// </summary>
public sealed class InitializationResult
{
    public bool Success { get; set; }
    public int ComponentsInitialized { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public TimeSpan Duration => EndTime - StartTime;

    public override string ToString()
    {
        if (Success)
        {
            return $"Initialization successful: {ComponentsInitialized} components initialized in {Duration.TotalSeconds:F2}s";
        }

        return $"Initialization failed: {ErrorMessage}";
    }
}
