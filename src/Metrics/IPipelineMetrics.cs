#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Metrics;

/// <summary>
/// Provides real-time pipeline metrics including per-stage throughput.
/// </summary>
public interface IPipelineMetrics
{
    /// <summary>
    /// Returns the current pipeline-wide throughput in events per second,
    /// computed over a sliding time window.
    /// </summary>
    double GetThroughput();

    /// <summary>
    /// Returns the throughput in events per second for a specific pipeline stage.
    /// Returns 0 if the stage is not tracked.
    /// </summary>
    double GetThroughput(string stageName);

    /// <summary>
    /// Records that <paramref name="count"/> events passed through the pipeline.
    /// </summary>
    void RecordEvents(long count);

    /// <summary>
    /// Records that <paramref name="count"/> events passed through a specific stage.
    /// </summary>
    void RecordEvents(string stageName, long count);
}
