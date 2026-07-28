using Xunit;

namespace DotNetRealtimePipeline.Tests
{
    public class CommandLineParserJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var parser = new CommandLineParser();
            parser.AddOption("option1", "value1");
            parser.AddOption("option2", "value2");

            // Act
            var json = parser.ToJson();

            // Assert
            Assert.NotNull(json);
            Assert.Contains("option1", json);
            Assert.Contains("value1", json);
            Assert.Contains("option2", json);
            Assert.Contains("value2", json);
        }

        [Fact]
        public void ToJson_NullParser_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new CommandLineParser().ToJson());
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsParserInstance()
        {
            // Arrange
            var json = "{\"option1\":\"value1\",\"option2\":\"value2\"}";

            // Act
            var parser = CommandLineParserJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(parser);
            Assert.Contains("option1", parser.Options);
            Assert.Contains("value1", parser.Options);
            Assert.Contains("option2", parser.Options);
            Assert.Contains("value2", parser.Options);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => CommandLineParserJsonExtensions.FromJson(null));
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"option1\":\"value1\",\"option2\":\"value2\"}";

            // Act
            var result = CommandLineParserJsonExtensions.TryFromJson(json, out var parser);

            // Assert
            Assert.True(result);
            Assert.NotNull(parser);
            Assert.Contains("option1", parser.Options);
            Assert.Contains("value1", parser.Options);
            Assert.Contains("option2", parser.Options);
            Assert.Contains("value2", parser.Options);
        }

        [Fact]
        public void TryFromJson_NullJson_ReturnsFalse()
        {
            // Act
            var result = CommandLineParserJsonExtensions.TryFromJson(null, out var parser);

            // Assert
            Assert.False(result);
            Assert.Null(parser);
        }
    }
}
