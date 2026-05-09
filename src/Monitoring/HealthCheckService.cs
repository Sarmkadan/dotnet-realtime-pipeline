#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Monitoring;

using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Service for monitoring pipeline health and generating health reports.
/// Tracks component status, resource usage, and performance trends.
/// </summary>
public class HealthCheckService
{
    private readonly PipelineOrchestrator _orchestrator;
    private readonly ILogger<HealthCheckService> _logger;
    private readonly List<HealthCheckComponent> _components = new();

    public HealthCheckService(PipelineOrchestrator orchestrator, ILogger<HealthCheckService> logger)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a health check component.
    /// </summary>
    public void RegisterComponent(string name, Func<Task<ComponentHealth>> healthCheck)
    {
        _components.Add(new HealthCheckComponent
        {
            Name = name,
            HealthCheck = healthCheck,
            LastChecked = DateTime.MinValue,
            Status = ComponentStatus.Unknown
        });

        _logger.LogInformation("Health check component registered: {Name}", name);
    }

    /// <summary>
    /// Performs a complete health check of all components.
    /// </summary>
    public async Task<SystemHealthReport> PerformCompleteHealthCheckAsync()
    {
        var report = new SystemHealthReport
        {
            CheckedAt = DateTime.UtcNow,
            Components = new Dictionary<string, ComponentHealth>()
        };

        foreach (var component in _components)
        {
            try
            {
                var health = await component.HealthCheck();
                report.Components[component.Name] = health;
                component.Status = health.IsHealthy ? ComponentStatus.Healthy : ComponentStatus.Degraded;
                component.LastChecked = DateTime.UtcNow;

                _logger.LogDebug("Health check for {Component}: {Status}", component.Name, component.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking health of component: {Component}", component.Name);
                component.Status = ComponentStatus.Unhealthy;
                report.Components[component.Name] = new ComponentHealth
                {
                    IsHealthy = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        // Get pipeline health
        try
        {
            var pipelineHealth = await _orchestrator.GetHealthReportAsync();
            report.PipelineStatus = pipelineHealth?.Status.ToString() ?? "Unknown";
            report.Throughput = pipelineHealth?.ThroughputItemsPerSecond ?? 0;
            report.SuccessRate = pipelineHealth?.SuccessRatePercent ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pipeline health");
        }

        // Determine overall health
        report.OverallStatus = DetermineOverallStatus(report.Components);

        return report;
    }

    /// <summary>
    /// Gets a quick health status without detailed checks.
    /// </summary>
    public async Task<QuickHealthStatus> GetQuickStatusAsync()
    {
        try
        {
            var status = _orchestrator.GetStatus();
            var health = await _orchestrator.GetHealthReportAsync();

            return new QuickHealthStatus
            {
                IsRunning = status.IsRunning,
                HealthStatus = health?.Status.ToString() ?? "Unknown",
                PendingItems = status.PendingItemsInQueue,
                ThroughputOk = health is not null && health.ThroughputItemsPerSecond > 0,
                ErrorRateAcceptable = (health?.SuccessRatePercent ?? 100) >= 80
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quick status");
            return new QuickHealthStatus { HealthStatus = "Error" };
        }
    }

    /// <summary>
    /// Gets the status of a specific component.
    /// </summary>
    public ComponentStatus GetComponentStatus(string componentName)
    {
        var component = _components.Find(c => c.Name == componentName);
        return component?.Status ?? ComponentStatus.Unknown;
    }

    /// <summary>
    /// Determines overall system health based on component statuses.
    /// </summary>
    private SystemHealth DetermineOverallStatus(Dictionary<string, ComponentHealth> components)
    {
        if (components.Count == 0)
            return SystemHealth.Unknown;

        var unhealthyCount = components.Values.Count(c => !c.IsHealthy);
        var totalCount = components.Count;

        if (unhealthyCount == 0)
            return SystemHealth.Healthy;

        if (unhealthyCount < totalCount / 2)
            return SystemHealth.Degraded;

        return SystemHealth.Unhealthy;
    }
}

/// <summary>
/// Represents the health status of a component.
/// </summary>
public class ComponentHealth
{
    public bool IsHealthy { get; set; }
    public string Message { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Health status of a system component.
/// </summary>
public enum ComponentStatus
{
    Unknown,
    Healthy,
    Degraded,
    Unhealthy
}

/// <summary>
/// Overall system health status.
/// </summary>
public enum SystemHealth
{
    Unknown,
    Healthy,
    Degraded,
    Unhealthy
}

/// <summary>
/// Complete health report for the system.
/// </summary>
public class SystemHealthReport
{
    public DateTime CheckedAt { get; set; }
    public SystemHealth OverallStatus { get; set; }
    public Dictionary<string, ComponentHealth> Components { get; set; }
    public string PipelineStatus { get; set; }
    public double Throughput { get; set; }
    public double SuccessRate { get; set; }
}

/// <summary>
/// Quick health status without detailed information.
/// </summary>
public class QuickHealthStatus
{
    public bool IsRunning { get; set; }
    public string HealthStatus { get; set; }
    public int PendingItems { get; set; }
    public bool ThroughputOk { get; set; }
    public bool ErrorRateAcceptable { get; set; }
}

internal class HealthCheckComponent
{
    public string Name { get; set; }
    public Func<Task<ComponentHealth>> HealthCheck { get; set; }
    public DateTime LastChecked { get; set; }
    public ComponentStatus Status { get; set; }
}

/// <summary>
/// Resource monitoring service.
/// </summary>
public class ResourceMonitor
{
    private readonly ILogger<ResourceMonitor> _logger;

    public ResourceMonitor(ILogger<ResourceMonitor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets current resource usage.
    /// </summary>
    public ResourceUsage GetResourceUsage()
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();

            return new ResourceUsage
            {
                MemoryMB = process.WorkingSet64 / (1024.0 * 1024.0),
                ThreadCount = process.Threads.Count,
                CpuUsagePercent = 0, // CPU usage calculation requires PerfCounters
                PrivateMemoryMB = process.WorkingSet64 / (1024.0 * 1024.0), // Use working set as approximation
                CheckedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource usage");
            return new ResourceUsage { CheckedAt = DateTime.UtcNow };
        }
    }

    /// <summary>
    /// Checks if resource usage is within acceptable limits.
    /// </summary>
    public bool IsResourceUsageAcceptable(double maxMemoryMB = 1024, int maxThreads = 100)
    {
        var usage = GetResourceUsage();
        return usage.MemoryMB < maxMemoryMB && usage.ThreadCount < maxThreads;
    }

    private static double CalculateCpuUsage(System.Diagnostics.Process process)
    {
        // CPU usage calculation requires PerformanceCounter which may not be available in all environments
        return 0;
    }
}

/// <summary>
/// Current resource usage metrics.
/// </summary>
public class ResourceUsage
{
    public double MemoryMB { get; set; }
    public int ThreadCount { get; set; }
    public double CpuUsagePercent { get; set; }
    public double PrivateMemoryMB { get; set; }
    public DateTime CheckedAt { get; set; }
}
