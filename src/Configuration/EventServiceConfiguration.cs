/// <summary>
/// Configuration options for background workers.
/// </summary>
public sealed class WorkerOptions
{
    public int MetricsAggregationIntervalMs { get; set; } = 5000;
    public int HealthCheckIntervalMs { get; set; } = 10000;
    public bool EnableProcessingWorker { get; set; } = true;
    public bool EnableMetricsWorker { get; set; } = true;
    public bool EnableHealthCheckWorker { get; set; } = true;

    public override string ToString() => $"WorkerOptions {{ MetricsAggregationIntervalMs = {MetricsAggregationIntervalMs}, HealthCheckIntervalMs = {HealthCheckIntervalMs}, EnableProcessingWorker = {EnableProcessingWorker}, EnableMetricsWorker = {EnableMetricsWorker}, EnableHealthCheckWorker = {EnableHealthCheckWorker} }}";
}