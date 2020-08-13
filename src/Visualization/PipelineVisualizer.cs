#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Visualization;

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Generates human-readable ASCII diagrams of the pipeline topology.
/// Combines static configuration with live runtime state (buffer levels,
/// throughput, backpressure) so operators can understand pipeline health
/// at a glance.
/// </summary>
public sealed class PipelineVisualizer
{
    private readonly BackpressureService _backpressureService;
    private readonly MetricsService _metricsService;

    public PipelineVisualizer(BackpressureService backpressureService, MetricsService metricsService)
    {
        _backpressureService = backpressureService ?? throw new ArgumentNullException(nameof(backpressureService));
        _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
    }

    /// <summary>
    /// Builds a list of visualization nodes from the pipeline configuration,
    /// annotated with live runtime data.
    /// </summary>
    public List<PipelineVisualizationNode> BuildNodes(PipelineConfig config)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));

        var nodes = new List<PipelineVisualizationNode>();
        var stages = config.Stages;

        for (int i = 0; i < stages.Count; i++)
        {
            var stage = stages[i];
            var ctx = _backpressureService.GetContext(stage.StageName);

            var node = new PipelineVisualizationNode
            {
                StageName = stage.StageName,
                StageType = stage.StageType,
                BufferFillPercent = ctx?.GetBufferFillPercentage() ?? 0,
                IsBackpressured = ctx?.IsBackpressured ?? false,
                DroppedItems = ctx?.DroppedItemCount ?? 0,
                ThroughputEps = _metricsService.GetThroughput(stage.StageName)
            };

            node.HealthLabel = node.ComputeHealthLabel();

            if (i + 1 < stages.Count)
                node.DownstreamStages.Add(stages[i + 1].StageName);

            nodes.Add(node);
        }

        return nodes;
    }

    /// <summary>
    /// Renders an ASCII block diagram of the pipeline and returns it as a string.
    /// Each stage is shown as a box with live buffer, throughput, and health data.
    /// Downstream arrows connect stages top-to-bottom.
    /// </summary>
    public string Render(PipelineConfig config)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));

        var nodes = BuildNodes(config);
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine($"  Pipeline: {config.PipelineName}  (v{config.Version})");
        sb.AppendLine(new string('─', 72));

        foreach (var node in nodes)
        {
            AppendNodeBox(sb, node);

            if (node.DownstreamStages.Count > 0)
            {
                sb.AppendLine("       │");
                sb.AppendLine("       ▼");
            }
        }

        sb.AppendLine(new string('─', 72));

        var sysStatus = _backpressureService.GetSystemStatus();
        sb.AppendLine($"  System health : {sysStatus.GetHealthStatus()}");
        sb.AppendLine($"  Backpressured : {sysStatus.BackpressuredStages}/{sysStatus.TotalStages} stage(s)");
        sb.AppendLine($"  Dropped total : {sysStatus.TotalDroppedItems:N0} item(s)");
        sb.AppendLine($"  Pipeline EPS  : {_metricsService.GetThroughput():F1}");
        sb.AppendLine($"  Snapshot at   : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Renders a compact single-line summary of the pipeline stages.
    /// Useful for embedding in log lines or status responses.
    /// </summary>
    public string RenderCompact(PipelineConfig config)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));

        var nodes = BuildNodes(config);
        var parts = new List<string>(nodes.Count);

        foreach (var node in nodes)
            parts.Add(node.ToInlineString());

        return string.Join(" → ", parts);
    }

    // -------------------------------------------------------------------------

    private static void AppendNodeBox(StringBuilder sb, PipelineVisualizationNode node)
    {
        string healthIcon = node.HealthLabel switch
        {
            "CRITICAL" => "!",
            "WARNING"  => "~",
            _          => "+"
        };

        string bufBar = BuildBar(node.BufferFillPercent, 20);
        string bpFlag = node.IsBackpressured ? " [BACKPRESSURE]" : "";

        sb.AppendLine("  +------------------------------------------------------+");
        sb.AppendLine($"  | {healthIcon} {node.StageName,-18} ({node.StageType}){bpFlag,-15}|");
        sb.AppendLine($"  |   Buffer : [{bufBar}] {node.BufferFillPercent,5:F1}%             |");
        sb.AppendLine($"  |   EPS    : {node.ThroughputEps,8:F2}   Dropped: {node.DroppedItems,8:N0}        |");
        sb.AppendLine("  +------------------------------------------------------+");
    }

    private static string BuildBar(double percent, int width)
    {
        int filled = (int)Math.Round(percent / 100.0 * width);
        filled = Math.Clamp(filled, 0, width);
        return new string('#', filled) + new string('.', width - filled);
    }
}
