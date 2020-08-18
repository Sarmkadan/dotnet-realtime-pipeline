#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.CLI;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Parses command-line arguments into structured command objects.
/// Supports verb-based commands with options and flags.
/// </summary>
public sealed class CommandLineParser
{
    private readonly Dictionary<string, Func<ParsedCommand>> _commandRegistry = new(StringComparer.OrdinalIgnoreCase);

    public CommandLineParser()
    {
        RegisterDefaultCommands();
    }

    /// <summary>
    /// Registers a command type with the parser.
    /// </summary>
    public void RegisterCommand(string verb, Func<ParsedCommand> commandFactory)
    {
        _commandRegistry[verb] = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
    }

    /// <summary>
    /// Parses command-line arguments into a structured command.
    /// </summary>
    public ParsedCommand Parse(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            return new HelpCommand { Verb = "help", IsValid = true };
        }

        var verb = args[0].TrimStart('-');
        var options = ParseOptions(args.Skip(1).ToArray());

        if (!_commandRegistry.TryGetValue(verb, out var factory))
        {
            return new UnknownCommand
            {
                Verb = verb,
                IsValid = false,
                ErrorMessage = $"Unknown command: {verb}"
            };
        }

        var command = factory();
        command.Verb = verb;
        command.Options = options;
        command.IsValid = ValidateCommand(command);

        if (!command.IsValid && string.IsNullOrEmpty(command.ErrorMessage))
        {
            command.ErrorMessage = "Invalid command options";
        }

        return command;
    }

    /// <summary>
    /// Parses key-value options from argument array.
    /// </summary>
    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("-"))
                continue;

            var key = arg.TrimStart('-');
            string value = string.Empty;

            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
            {
                value = args[i + 1];
                i++;
            }

            options[key] = value;
        }

        return options;
    }

    /// <summary>
    /// Validates required parameters for a command.
    /// </summary>
    private bool ValidateCommand(ParsedCommand command)
    {
        if (command.RequiredOptions is null || command.RequiredOptions.Count == 0)
            return true;

        foreach (var required in command.RequiredOptions)
        {
            if (!command.Options.ContainsKey(required))
            {
                command.ErrorMessage = $"Missing required option: {required}";
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Registers built-in commands.
    /// </summary>
    private void RegisterDefaultCommands()
    {
        RegisterCommand("help", () => new HelpCommand());
        RegisterCommand("ingest", () => new IngestCommand());
        RegisterCommand("query", () => new QueryCommand());
        RegisterCommand("status", () => new StatusCommand());
        RegisterCommand("export", () => new ExportCommand());
        RegisterCommand("visualize", () => new VisualizeCommand());
    }
}

/// <summary>
/// Represents a parsed command with options and metadata.
/// </summary>
public abstract class ParsedCommand
{
    public string Verb { get; set; } = string.Empty;
    public Dictionary<string, string> Options { get; set; } = new();
    public List<string> RequiredOptions { get; set; } = new();
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets an option value with fallback to default.
    /// </summary>
    public string GetOption(string name, string defaultValue = "")
    {
        return Options.TryGetValue(name, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Checks if an option flag is set.
    /// </summary>
    public bool HasFlag(string name)
    {
        return Options.ContainsKey(name);
    }

    public abstract Task<int> ExecuteAsync();
}

/// <summary>
/// Represents a command that failed to resolve to a known verb.
/// </summary>
public sealed class UnknownCommand : ParsedCommand
{
    public override Task<int> ExecuteAsync()
    {
        Console.WriteLine(string.IsNullOrEmpty(ErrorMessage) ? $"Unknown command: {Verb}" : ErrorMessage);
        return Task.FromResult(1);
    }
}

public sealed class HelpCommand : ParsedCommand
{
    public override Task<int> ExecuteAsync()
    {
        Console.WriteLine("Real-Time Pipeline - Command-Line Interface");
        Console.WriteLine("\nAvailable Commands:");
        Console.WriteLine("  ingest     --file <path> [--batch] [--format json|csv]");
        Console.WriteLine("  query      --start <ms> --end <ms> [--source <name>] [--quality <score>]");
        Console.WriteLine("  status     [--format json|text]");
        Console.WriteLine("  export     --start <ms> --end <ms> --output <path> [--format json|csv|xml]");
        Console.WriteLine("  visualize  [--compact]");
        Console.WriteLine("  help       Show this message");
        return Task.FromResult(0);
    }
}

public sealed class IngestCommand : ParsedCommand
{
    public IngestCommand()
    {
        RequiredOptions.Add("file");
    }

    public override Task<int> ExecuteAsync()
    {
        var filePath = GetOption("file");
        var batch = HasFlag("batch");
        var format = GetOption("format", "json");
        Console.WriteLine($"Ingesting data from: {filePath} (batch={batch}, format={format})");
        return Task.FromResult(0);
    }
}

public sealed class QueryCommand : ParsedCommand
{
    public QueryCommand()
    {
        RequiredOptions.Add("start");
        RequiredOptions.Add("end");
    }

    public override Task<int> ExecuteAsync()
    {
        var start = GetOption("start");
        var end = GetOption("end");
        var source = GetOption("source", "");
        Console.WriteLine($"Querying data: {start} to {end}, source={source}");
        return Task.FromResult(0);
    }
}

public sealed class StatusCommand : ParsedCommand
{
    public override Task<int> ExecuteAsync()
    {
        var format = GetOption("format", "text");
        Console.WriteLine($"Pipeline Status (format={format})");
        return Task.FromResult(0);
    }
}

public sealed class ExportCommand : ParsedCommand
{
    public ExportCommand()
    {
        RequiredOptions.Add("start");
        RequiredOptions.Add("end");
        RequiredOptions.Add("output");
    }

    public override Task<int> ExecuteAsync()
    {
        var start = GetOption("start");
        var end = GetOption("end");
        var output = GetOption("output");
        var format = GetOption("format", "json");
        Console.WriteLine($"Exporting data [{start},{end}] to {output} ({format})");
        return Task.FromResult(0);
    }
}

/// <summary>
/// Prints an ASCII visualization of the pipeline topology with live metrics.
/// Use --compact for a single-line summary suitable for log output.
/// </summary>
public sealed class VisualizeCommand : ParsedCommand
{
    public override Task<int> ExecuteAsync()
    {
        var compact = HasFlag("compact");
        Console.WriteLine(compact
            ? "Pipeline visualization (compact mode) — run via CommandExecutor.VisualizeAsync for live data."
            : "Pipeline visualization — run via CommandExecutor.VisualizeAsync for live data.");
        return Task.FromResult(0);
    }
}
