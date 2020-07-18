#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Formatters;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Interface for data output formatters supporting multiple formats.
/// </summary>
public interface IOutputFormatter
{
    string Format<T>(T data) where T : class;
    Task<string> FormatAsync<T>(T data) where T : class;
}

/// <summary>
/// JSON output formatter for serializing data to JSON format.
/// </summary>
public class JsonOutputFormatter : IOutputFormatter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string Format<T>(T data) where T : class
    {
        return JsonSerializer.Serialize(data, Options);
    }

    public async Task<string> FormatAsync<T>(T data) where T : class
    {
        return await Task.FromResult(Format(data));
    }
}

/// <summary>
/// CSV output formatter for serializing data to CSV format.
/// </summary>
public class CsvOutputFormatter : IOutputFormatter
{
    public string Format<T>(T data) where T : class
    {
        if (data is List<DataPoint> dataPoints)
        {
            return FormatDataPoints(dataPoints);
        }

        if (data is List<ProcessingResult> results)
        {
            return FormatProcessingResults(results);
        }

        return string.Empty;
    }

    public async Task<string> FormatAsync<T>(T data) where T : class
    {
        return await Task.FromResult(Format(data));
    }

    private static string FormatDataPoints(List<DataPoint> dataPoints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Timestamp,Value,Source,Quality,Tags");

        foreach (var point in dataPoints)
        {
            sb.AppendLine($"{point.Id},{point.Timestamp},{point.Value:F4},{EscapeCsv(point.Source)},{point.Quality},{EscapeCsv(point.Tags)}");
        }

        return sb.ToString();
    }

    private static string FormatProcessingResults(List<ProcessingResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ResultId,Success,ErrorMessage,ProcessingTimeMs,StageName");

        foreach (var result in results)
        {
            sb.AppendLine($"{result.ResultId},{result.Success},{EscapeCsv(result.ErrorMessage)},{result.ProcessingTimeMs},{EscapeCsv(result.StageName)}");
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

/// <summary>
/// Table output formatter for displaying data in ASCII table format.
/// </summary>
public class TableOutputFormatter : IOutputFormatter
{
    private const int ColumnPadding = 2;

    public string Format<T>(T data) where T : class
    {
        if (data is List<DataPoint> dataPoints)
        {
            return FormatDataPointsTable(dataPoints);
        }

        if (data is Dictionary<string, object> dict)
        {
            return FormatDictionaryTable(dict);
        }

        return JsonSerializer.Serialize(data);
    }

    public async Task<string> FormatAsync<T>(T data) where T : class
    {
        return await Task.FromResult(Format(data));
    }

    private static string FormatDataPointsTable(List<DataPoint> dataPoints)
    {
        if (dataPoints.Count == 0)
            return "No data points";

        var sb = new StringBuilder();
        var headers = new[] { "Id", "Timestamp", "Value", "Source", "Quality" };
        var columnWidths = headers.Select(h => h.Length).ToArray();

        // Calculate column widths
        foreach (var point in dataPoints)
        {
            columnWidths[0] = Math.Max(columnWidths[0], point.Id.ToString().Length);
            columnWidths[1] = Math.Max(columnWidths[1], point.Timestamp.ToString().Length);
            columnWidths[2] = Math.Max(columnWidths[2], point.Value.ToString("F4").Length);
            columnWidths[3] = Math.Max(columnWidths[3], point.Source.Length);
            columnWidths[4] = Math.Max(columnWidths[4], point.Quality.ToString().Length);
        }

        // Add padding
        for (int i = 0; i < columnWidths.Length; i++)
        {
            columnWidths[i] += ColumnPadding;
        }

        // Print headers
        PrintRow(sb, headers, columnWidths);
        PrintSeparator(sb, columnWidths);

        // Print data rows
        foreach (var point in dataPoints.Take(10))
        {
            var values = new[] { point.Id.ToString(), point.Timestamp.ToString(), point.Value.ToString("F4"), point.Source, point.Quality.ToString() };
            PrintRow(sb, values, columnWidths);
        }

        if (dataPoints.Count > 10)
        {
            sb.AppendLine($"... and {dataPoints.Count - 10} more rows");
        }

        return sb.ToString();
    }

    private static string FormatDictionaryTable(Dictionary<string, object> dict)
    {
        var sb = new StringBuilder();
        var keyWidth = dict.Keys.Max(k => k.Length) + ColumnPadding;
        var valueWidth = dict.Values.Max(v => v?.ToString().Length ?? 0) + ColumnPadding;

        PrintRow(sb, new[] { "Key", "Value" }, new[] { keyWidth, valueWidth });
        PrintSeparator(sb, new[] { keyWidth, valueWidth });

        foreach (var kvp in dict)
        {
            PrintRow(sb, new[] { kvp.Key, kvp.Value?.ToString() ?? "null" }, new[] { keyWidth, valueWidth });
        }

        return sb.ToString();
    }

    private static void PrintRow(StringBuilder sb, string[] values, int[] columnWidths)
    {
        for (int i = 0; i < values.Length; i++)
        {
            sb.Append(values[i].PadRight(columnWidths[i]));
        }
        sb.AppendLine();
    }

    private static void PrintSeparator(StringBuilder sb, int[] columnWidths)
    {
        foreach (var width in columnWidths)
        {
            sb.Append(new string('-', width));
        }
        sb.AppendLine();
    }
}

/// <summary>
/// HTML output formatter for generating HTML reports.
/// </summary>
public class HtmlOutputFormatter : IOutputFormatter
{
    public string Format<T>(T data) where T : class
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine("<title>Pipeline Data Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
        sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        sb.AppendLine("th { background-color: #4CAF50; color: white; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<h1>Pipeline Data Report</h1>");

        if (data is List<DataPoint> dataPoints)
        {
            sb.Append(FormatDataPointsHtml(dataPoints));
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    public async Task<string> FormatAsync<T>(T data) where T : class
    {
        return await Task.FromResult(Format(data));
    }

    private static string FormatDataPointsHtml(List<DataPoint> dataPoints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Id</th><th>Timestamp</th><th>Value</th><th>Source</th><th>Quality</th></tr>");

        foreach (var point in dataPoints.Take(50))
        {
            sb.AppendLine($"<tr><td>{point.Id}</td><td>{point.Timestamp}</td><td>{point.Value:F4}</td><td>{point.Source}</td><td>{point.Quality}</td></tr>");
        }

        sb.AppendLine("</table>");

        if (dataPoints.Count > 50)
        {
            sb.AppendLine($"<p>Showing 50 of {dataPoints.Count} records</p>");
        }

        return sb.ToString();
    }
}

/// <summary>
/// Factory for creating appropriate output formatters.
/// </summary>
public static class OutputFormatterFactory
{
    public static IOutputFormatter Create(OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Json => new JsonOutputFormatter(),
            OutputFormat.Csv => new CsvOutputFormatter(),
            OutputFormat.Table => new TableOutputFormatter(),
            OutputFormat.Html => new HtmlOutputFormatter(),
            _ => new JsonOutputFormatter()
        };
    }
}

/// <summary>
/// Supported output formats.
/// </summary>
public enum OutputFormat
{
    Json,
    Csv,
    Table,
    Html
}
