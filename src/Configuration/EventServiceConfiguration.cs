#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Configuration options for background workers.
/// </summary>
public sealed class WorkerOptions
{
    /// <summary>
    /// Default interval, in milliseconds, between metrics aggregation passes.
    /// </summary>
    public const int DefaultMetricsAggregationIntervalMs = 5000;

    /// <summary>
    /// Default interval, in milliseconds, between health check passes.
    /// </summary>
    public const int DefaultHealthCheckIntervalMs = 10000;

    public int MetricsAggregationIntervalMs { get; set; } = DefaultMetricsAggregationIntervalMs;
    public int HealthCheckIntervalMs { get; set; } = DefaultHealthCheckIntervalMs;
    public bool EnableProcessingWorker { get; set; } = true;
    public bool EnableMetricsWorker { get; set; } = true;
    public bool EnableHealthCheckWorker { get; set; } = true;

    /// <summary>
    /// Returns a one-line representation of the worker configuration.
    /// </summary>
    public override string ToString() => $"WorkerOptions {{ MetricsAggregationIntervalMs = {MetricsAggregationIntervalMs}, HealthCheckIntervalMs = {HealthCheckIntervalMs}, EnableProcessingWorker = {EnableProcessingWorker}, EnableMetricsWorker = {EnableMetricsWorker}, EnableHealthCheckWorker = {EnableHealthCheckWorker} }}";
}