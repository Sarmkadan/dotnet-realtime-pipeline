using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DotNetRealtimePipeline.CLI
{
    /// <summary>
    /// Provides extension methods for <see cref="CommandLineParser"/> to simplify common parsing scenarios.
    /// </summary>
    public static class CommandLineParserExtensions
    {
        /// <summary>
        /// Registers a command with the parser using a factory method.
        /// </summary>
        /// <param name="parser">The parser instance.</param>
        /// <param name="verb">The command verb to register.</param>
        /// <param name="commandFactory">Factory method that creates the command instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="parser"/>, <paramref name="verb"/>, or <paramref name="commandFactory"/> is <see langword="null"/>.</exception>
        public static void RegisterCommand(this CommandLineParser parser, string verb, Func<ParsedCommand> commandFactory)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(verb);
            ArgumentNullException.ThrowIfNull(commandFactory);

            parser.RegisterCommand(verb, commandFactory);
        }

        /// <summary>
        /// Parses command line arguments into a structured command.
        /// </summary>
        /// <param name="parser">The parser instance.</param>
        /// <param name="args">The command line arguments to parse.</param>
        /// <returns>The parsed command instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="parser"/> or <paramref name="args"/> is <see langword="null"/>.</exception>
        public static ParsedCommand ParseCommand(this CommandLineParser parser, string[] args)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(args);

            return parser.Parse(args);
        }

        /// <summary>
        /// Attempts to parse command line arguments and execute the appropriate command.
        /// Returns the exit code from the executed command.
        /// </summary>
        /// <param name="parser">The parser instance.</param>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The exit code from the executed command (0 for success, non-zero for failure).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="parser"/> or <paramref name="args"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when command execution fails unexpectedly.</exception>
        public static int TryParseAndExecute(this CommandLineParser parser, string[] args)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(args);

            try
            {
                var parsed = parser.Parse(args);
                if (!parsed.IsValid)
                {
                    Console.Error.WriteLine(parsed.ErrorMessage);
                    return 1;
                }

                return parsed.ExecuteAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        /// <summary>
        /// Registers multiple commands with the parser using a dictionary of verb to factory mappings.
        /// </summary>
        /// <param name="parser">The parser instance.</param>
        /// <param name="commands">Dictionary mapping verbs to their factory methods.</param>
        /// <exception cref="ArgumentNullException"><paramref name="parser"/> or <paramref name="commands"/> is <see langword="null"/>.</exception>
        public static void RegisterCommands(this CommandLineParser parser, IReadOnlyDictionary<string, Func<ParsedCommand>> commands)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(commands);

            foreach (var (verb, factory) in commands)
            {
                parser.RegisterCommand(verb, factory);
            }
        }

        /// <summary>
        /// Registers multiple commands with the parser using a sequence of verb-factory pairs.
        /// </summary>
        /// <param name="parser">The parser instance.</param>
        /// <param name="commandRegistrations">Sequence of (verb, factory) tuples to register.</param>
        /// <exception cref="ArgumentNullException"><paramref name="parser"/> or <paramref name="commandRegistrations"/> is <see langword="null"/>.</exception>
        public static void RegisterCommands(this CommandLineParser parser, IEnumerable<(string verb, Func<ParsedCommand> factory)> commandRegistrations)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(commandRegistrations);

            foreach (var (verb, factory) in commandRegistrations)
            {
                parser.RegisterCommand(verb, factory);
            }
        }
    }
}