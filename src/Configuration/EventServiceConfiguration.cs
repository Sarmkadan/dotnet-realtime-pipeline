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

    /// <summary>
    /// Gets or sets the interval, in milliseconds, between metrics aggregation passes.
    /// </summary>
    /// <value>The interval, in milliseconds, between metrics aggregation passes. Default is 5000.</value>
    public int MetricsAggregationIntervalMs { get; set; } = DefaultMetricsAggregationIntervalMs;

    /// <summary>
    /// Gets or sets the interval, in milliseconds, between health check passes.
    /// </summary>
    /// <value>The interval, in milliseconds, between health check passes. Default is 10000.</value>
    public int HealthCheckIntervalMs { get; set; } = DefaultHealthCheckIntervalMs;

    /// <summary>
    /// Gets or sets a value indicating whether the processing worker is enabled.
    /// </summary>
    /// <value><c>true</c> if the processing worker is enabled; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    public bool EnableProcessingWorker { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the metrics worker is enabled.
    /// </summary>
    /// <value><c>true</c> if the metrics worker is enabled; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    public bool EnableMetricsWorker { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the health check worker is enabled.
    /// </summary>
    /// <value><c>true</c> if the health check worker is enabled; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    public bool EnableHealthCheckWorker { get; set; } = true;

    /// <summary>
    /// Returns a one-line representation of the worker configuration.
    /// </summary>
    public override string ToString() => $"WorkerOptions {{ MetricsAggregationIntervalMs = {MetricsAggregationIntervalMs}, HealthCheckIntervalMs = {HealthCheckIntervalMs}, EnableProcessingWorker = {EnableProcessingWorker}, EnableMetricsWorker = {EnableMetricsWorker}, EnableHealthCheckWorker = {EnableHealthCheckWorker} }}";
}
