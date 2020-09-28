#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRealtimePipeline.Monitoring;

/// <summary>
/// Extension methods that add convenient health‑check operations to <see cref="HealthCheckService"/>.
/// </summary>
public static class HealthCheckServiceExtensions
{
    /// <summary>
    /// Retrieves the overall system health status by performing a complete health check.
    /// </summary>
    /// <param name="service">The <see cref="HealthCheckService"/> instance.</param>
    /// <returns>A <see cref="SystemHealth"/> value representing the overall health.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <c>null</c>.</exception>
    public static async Task<SystemHealth> GetOverallHealthAsync(this HealthCheckService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        var report = await service.PerformCompleteHealthCheckAsync().ConfigureAwait(false);
        return report.OverallStatus;
    }

    /// <summary>
    /// Retrieves the health information for a specific registered component.
    /// </summary>
    /// <param name="service">The <see cref="HealthCheckService"/> instance.</param>
    /// <param name="componentName">The name of the component to query.</param>
    /// <returns>
    /// The <see cref="ComponentHealth"/> for the component, or <c>null</c> if the component is not registered.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="componentName"/> is <c>null</c> or empty.</exception>
    public static async Task<ComponentHealth?> GetComponentHealthAsync(this HealthCheckService service, string componentName)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(componentName);
        var report = await service.PerformCompleteHealthCheckAsync().ConfigureAwait(false);
        return report.Components.TryGetValue(componentName, out var health) ? health : null;
    }

    /// <summary>
    /// Produces a concise, machine‑readable health summary string.
    /// </summary>
    /// <param name="service">The <see cref="HealthCheckService"/> instance.</param>
    /// <returns>A formatted string containing overall status, pipeline status, throughput and success rate.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <c>null</c>.</exception>
    public static async Task<string> GetHealthSummaryAsync(this HealthCheckService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        var report = await service.PerformCompleteHealthCheckAsync().ConfigureAwait(false);
        return string.Format(
            CultureInfo.InvariantCulture,
            "Overall: {0}, Pipeline: {1}, Throughput: {2:F2} items/s, SuccessRate: {3:P2}",
            report.OverallStatus,
            report.PipelineStatus,
            report.Throughput,
            report.SuccessRate / 100.0);
    }

    /// <summary>
    /// Registers multiple health‑check components in a single call.
    /// </summary>
    /// <param name="service">The <see cref="HealthCheckService"/> instance.</param>
    /// <param name="components">
    /// A collection of tuples where <c>Item1</c> is the component name and <c>Item2</c> is the async health‑check delegate.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="components"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when any component name is <c>null</c> or empty.</exception>
    public static void RegisterComponents(this HealthCheckService service, IEnumerable<(string Name, Func<Task<ComponentHealth>> HealthCheck)> components)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(components);

        foreach (var (name, healthCheck) in components)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentNullException.ThrowIfNull(healthCheck);
            service.RegisterComponent(name, healthCheck);
        }
    }
}
