using Xunit;

namespace DotNetRealtimePipeline.CLI.Tests;

public class CommandLineParserValidationTests
{
    [Fact]
    public void Validate_ReturnsEmptyList_WhenParserIsValid()
    {
        // Arrange
        var parser = new CommandLineParser();

        // Act
        var problems = CommandLineParserValidation.Validate(parser);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_ReturnsListWithProblem_WhenParserIsInvalid()
    {
        // Arrange
        var parser = new CommandLineParser();
        parser._commandRegistry = new Dictionary<string, Func<ParsedCommand>>();

        // Act
        var problems = CommandLineParserValidation.Validate(parser);

        // Assert
        Assert.Single(problems);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenParserIsNull()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => CommandLineParserValidation.Validate(null));
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenParserIsValid()
    {
        // Arrange
        var parser = new CommandLineParser();

        // Act
        var isValid = CommandLineParserValidation.IsValid(parser);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenParserIsInvalid()
    {
        // Arrange
        var parser = new CommandLineParser();
        parser._commandRegistry = new Dictionary<string, Func<ParsedCommand>>();

        // Act
        var isValid = CommandLineParserValidation.IsValid(parser);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_WhenParserIsInvalid()
    {
        // Arrange
        var parser = new CommandLineParser();
        parser._commandRegistry = new Dictionary<string, Func<ParsedCommand>>();

        // Act and Assert
        Assert.Throws<ArgumentException>(() => CommandLineParserValidation.EnsureValid(parser));
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_WhenParserIsValid()
    {
        // Arrange
        var parser = new CommandLineParser();

        // Act and Assert
        Assert.Empty(() => CommandLineParserValidation.EnsureValid(parser));
    }
}
