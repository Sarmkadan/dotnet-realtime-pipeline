using Xunit;

namespace DotNetRealtimePipeline.CLI.Tests;

public class CommandLineParserExtensionsTests
{
    [Fact]
    public void RegisterCommand_HappyPath()
    {
        // Arrange
        var parser = new CommandLineParser();
        var verb = "test";
        var factory = () => new ParsedCommand();

        // Act
        parser.RegisterCommand(verb, factory);

        // Assert
        Assert.Single(parser._commandRegistry);
    }

    [Fact]
    public void RegisterCommand_NullParser_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new CommandLineParserExtensions(null).RegisterCommand("test", () => new ParsedCommand()));
    }

    [Fact]
    public void RegisterCommand_NullVerb_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new CommandLineParserExtensions(new CommandLineParser()).RegisterCommand(null, () => new ParsedCommand()));
    }

    [Fact]
    public void RegisterCommand_NullFactory_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new CommandLineParserExtensions(new CommandLineParser()).RegisterCommand("test", null));
    }

    [Fact]
    public void ParseCommand_HappyPath()
    {
        // Arrange
        var parser = new CommandLineParser();
        var args = new[] { "test" };
        var factory = () => new ParsedCommand();

        // Act
        parser.RegisterCommand("test", factory);
        var parsed = parser.ParseCommand(args);

        // Assert
        Assert.NotNull(parsed);
    }

    [Fact]
    public void ParseCommand_NullParser_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new CommandLineParserExtensions(null).ParseCommand(new[] { "test" }));
    }

    [Fact]
    public void ParseCommand_NullArgs_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new CommandLineParserExtensions(new CommandLineParser()).ParseCommand(null));
    }

    [Fact]
    public void TryParseAndExecute_HappyPath()
    {
        // Arrange
        var parser = new CommandLineParser();
        var args = new[] { "test" };
        var factory = () => new ParsedCommand();

        // Act
        parser.RegisterCommand("test", factory);
        var result = parser.TryParseAndExecute(args);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void TryParseAndExecute_NullParser_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new CommandLineParserExtensions(null).TryParseAndExecute(new[] { "test" }));
    }

    [Fact]
    public void TryParseAndExecute_NullArgs_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new CommandLineParserExtensions(new CommandLineParser()).TryParseAndExecute(null));
    }

    [Fact]
    public void RegisterCommands_HappyPath()
    {
        // Arrange
        var parser = new CommandLineParser();
        var commands = new Dictionary<string, Func<ParsedCommand>>
        {
            ["test1"] = () => new ParsedCommand(),
            ["test2"] = () => new ParsedCommand()
        };

        // Act
        parser.RegisterCommands(commands);

        // Assert
        Assert.Equal(2, parser._commandRegistry.Count);
    }

    [Fact]
    public void RegisterCommands_NullParser_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new CommandLineParserExtensions(null).RegisterCommands(new Dictionary<string, Func<ParsedCommand>>()));
    }

    [Fact]
    public void RegisterCommands_NullCommands_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new CommandLineParserExtensions(new CommandLineParser()).RegisterCommands(null));
    }
}
