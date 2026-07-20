using DotNetRealtimePipeline.CLI;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Tests for CommandLineParser class covering various parsing scenarios.
/// </summary>
public class CommandLineParserTests
{
    private readonly CommandLineParser _parser;

    public CommandLineParserTests()
    {
        _parser = new CommandLineParser();
    }

    [Fact]
    public void Parse_EmptyArgsArray_ReturnsHelpCommand()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<HelpCommand>();
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Parse_UnknownCommand_ReturnsUnknownCommand()
    {
        // Arrange
        var args = new[] { "unknowncommand" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<UnknownCommand>();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Unknown command: unknowncommand");
    }

    [Fact]
    public void Parse_HelpCommand_ReturnsHelpCommand()
    {
        // Arrange
        var args = new[] { "help" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<HelpCommand>();
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Parse_IngestCommandWithRequiredOptions_ReturnsValidIngestCommand()
    {
        // Arrange
        var args = new[] { "ingest", "--file", "data.json", "--batch" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<IngestCommand>();
        result.IsValid.Should().BeTrue();
        result.Verb.Should().Be("ingest");
        result.Options.Should().ContainKey("file").WhoseValue.Should().Be("data.json");
        result.Options.Should().ContainKey("batch");
        result.Options["batch"].Should().BeEmpty();
    }

    [Fact]
    public void Parse_IngestCommandWithAllOptions_ReturnsValidIngestCommand()
    {
        // Arrange
        var args = new[] { "ingest", "--file", "data.json", "--batch", "--format", "csv" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<IngestCommand>();
        result.IsValid.Should().BeTrue();
        result.Options.Should().ContainKey("file").WhoseValue.Should().Be("data.json");
        result.Options.Should().ContainKey("batch");
        result.Options.Should().ContainKey("format").WhoseValue.Should().Be("csv");
    }

    [Fact]
    public void Parse_IngestCommandWithoutRequiredFileOption_ReturnsInvalidIngestCommand()
    {
        // Arrange
        var args = new[] { "ingest", "--batch" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<IngestCommand>();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Missing required option: file");
    }

    [Fact]
    public void Parse_QueryCommandWithRequiredOptions_ReturnsValidQueryCommand()
    {
        // Arrange
        var args = new[] { "query", "--start", "1000", "--end", "2000" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<QueryCommand>();
        result.IsValid.Should().BeTrue();
        result.Options.Should().ContainKey("start").WhoseValue.Should().Be("1000");
        result.Options.Should().ContainKey("end").WhoseValue.Should().Be("2000");
    }

    [Fact]
    public void Parse_QueryCommandWithoutRequiredOptions_ReturnsInvalidQueryCommand()
    {
        // Arrange
        var args = new[] { "query", "--start", "1000" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<QueryCommand>();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Missing required option: end");
    }

    [Fact]
    public void Parse_QueryCommandWithOptionalSource_ReturnsValidQueryCommand()
    {
        // Arrange
        var args = new[] { "query", "--start", "1000", "--end", "2000", "--source", "pipeline1" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<QueryCommand>();
        result.IsValid.Should().BeTrue();
        result.Options.Should().ContainKey("source").WhoseValue.Should().Be("pipeline1");
    }

    [Fact]
    public void Parse_StatusCommand_ReturnsValidStatusCommand()
    {
        // Arrange
        var args = new[] { "status" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<StatusCommand>();
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Parse_StatusCommandWithFormatOption_ReturnsValidStatusCommand()
    {
        // Arrange
        var args = new[] { "status", "--format", "json" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<StatusCommand>();
        result.IsValid.Should().BeTrue();
        result.Options.Should().ContainKey("format").WhoseValue.Should().Be("json");
    }

    [Fact]
    public void Parse_ExportCommandWithAllRequiredOptions_ReturnsValidExportCommand()
    {
        // Arrange
        var args = new[] { "export", "--start", "1000", "--end", "2000", "--output", "/tmp/data.json" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ExportCommand>();
        result.IsValid.Should().BeTrue();
        result.Options.Should().ContainKey("start").WhoseValue.Should().Be("1000");
        result.Options.Should().ContainKey("end").WhoseValue.Should().Be("2000");
        result.Options.Should().ContainKey("output").WhoseValue.Should().Be("/tmp/data.json");
    }

    [Fact]
    public void Parse_ExportCommandWithoutRequiredOptions_ReturnsInvalidExportCommand()
    {
        // Arrange
        var args = new[] { "export", "--start", "1000" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ExportCommand>();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Missing required option: end");
    }

    [Fact]
    public void Parse_ExportCommandWithOptionalFormat_ReturnsValidExportCommand()
    {
        // Arrange
        var args = new[] { "export", "--start", "1000", "--end", "2000", "--output", "/tmp/data.json", "--format", "csv" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ExportCommand>();
        result.IsValid.Should().BeTrue();
        result.Options.Should().ContainKey("format").WhoseValue.Should().Be("csv");
    }

    [Fact]
    public void Parse_VisualizeCommand_ReturnsValidVisualizeCommand()
    {
        // Arrange
        var args = new[] { "visualize" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<VisualizeCommand>();
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Parse_VisualizeCommandWithCompactFlag_ReturnsValidVisualizeCommand()
    {
        // Arrange
        var args = new[] { "visualize", "--compact" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<VisualizeCommand>();
        result.IsValid.Should().BeTrue();
        result.Options.Should().ContainKey("compact");
    }

    [Fact]
    public void Parse_CommandWithMixedCaseOptions_OptionsAreCaseInsensitive()
    {
        // Arrange
        var args = new[] { "ingest", "--FILE", "data.json", "--BaTch" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<IngestCommand>();
        result.IsValid.Should().BeTrue();
        result.Options.Should().ContainKey("file").WhoseValue.Should().Be("data.json");
        result.Options.Should().ContainKey("batch");
    }

    [Fact]
    public void Parse_CommandWithGetOption_ReturnsDefaultValueWhenOptionMissing()
    {
        // Arrange
        var args = new[] { "status" };
        var command = _parser.Parse(args) as StatusCommand;

        // Act
        var format = command?.GetOption("format", "default_format");

        // Assert
        format.Should().Be("default_format");
    }

    [Fact]
    public void Parse_CommandWithGetOption_ReturnsOptionValueWhenOptionPresent()
    {
        // Arrange
        var args = new[] { "status", "--format", "json" };
        var command = _parser.Parse(args) as StatusCommand;

        // Act
        var format = command?.GetOption("format", "default_format");

        // Assert
        format.Should().Be("json");
    }

    [Fact]
    public void Parse_CommandWithHasFlag_ReturnsTrueWhenFlagPresent()
    {
        // Arrange
        var args = new[] { "ingest", "--file", "data.json", "--batch" };
        var command = _parser.Parse(args) as IngestCommand;

        // Act
        var hasBatch = command?.HasFlag("batch");

        // Assert
        hasBatch.Should().BeTrue();
    }

    [Fact]
    public void Parse_CommandWithHasFlag_ReturnsFalseWhenFlagMissing()
    {
        // Arrange
        var args = new[] { "ingest", "--file", "data.json" };
        var command = _parser.Parse(args) as IngestCommand;

        // Act
        var hasBatch = command?.HasFlag("batch");

        // Assert
        hasBatch.Should().BeFalse();
    }

    [Fact]
    public void Parse_CommandWithUnknownFlag_UnknownFlagIsStoredInOptions()
    {
        // Arrange
        var args = new[] { "status", "--unknown-flag", "value" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue(); // StatusCommand has no required options
        result.Options.Should().ContainKey("unknown-flag").WhoseValue.Should().Be("value");
    }

    [Fact]
    public void Parse_CommandWithMultipleUnknownFlags_AllUnknownFlagsStoredInOptions()
    {
        // Arrange
        var args = new[] { "status", "--flag1", "value1", "--flag2", "value2" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Options.Should().ContainKey("flag1").WhoseValue.Should().Be("value1");
        result.Options.Should().ContainKey("flag2").WhoseValue.Should().Be("value2");
    }
}