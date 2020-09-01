# CommandLineParser

`CommandLineParser` is a utility class for parsing and validating command-line arguments in .NET applications, particularly for defining and executing pipeline commands with options and flags.

## API

### `public CommandLineParser()`
Initializes a new instance of the `CommandLineParser` class with default settings.

### `public void RegisterCommand(string verb, Type commandType)`
Registers a command type with the parser under a specified verb.

- **Parameters**
  - `verb`: The command verb (e.g., `"ingest"`, `"query"`).
  - `commandType`: The concrete type deriving from `CommandBase` to associate with the verb.
- **Throws**
  - `ArgumentNullException`: If `verb` or `commandType` is `null`.
  - `InvalidOperationException`: If `verb` is already registered.

### `public ParsedCommand Parse(string[] args)`
Parses the provided command-line arguments into a structured command representation.

- **Parameters**
  - `args`: The command-line arguments to parse.
- **Returns**
  - A `ParsedCommand` instance representing the parsed command.
- **Throws**
  - `ArgumentNullException`: If `args` is `null`.
  - `FormatException`: If the arguments are malformed or required options are missing.

### `public string Verb { get; }`
Gets the verb of the parsed command (e.g., `"ingest"`, `"query"`).

### `public Dictionary<string, string> Options { get; }`
Gets the parsed command-line options as key-value pairs (e.g., `{"--input", "file.txt"}`).

### `public List<string> RequiredOptions { get; }`
Gets the list of required option names that must be present for the command to be valid.

### `public bool IsValid { get; }`
Gets a value indicating whether the parsed command is valid (i.e., all required options are present).

### `public string ErrorMessage { get; }`
Gets the error message describing why the command is invalid, if applicable.

### `public string GetOption(string key)`
Retrieves the value of a specified option.

- **Parameters**
  - `key`: The option key (e.g., `"--input"`).
- **Returns**
  - The option value, or `null` if the option is not present.
- **Throws**
  - `ArgumentNullException`: If `key` is `null`.

### `public bool HasFlag(string flag)`
Checks whether a specified flag is present in the parsed command.

- **Parameters**
  - `flag`: The flag name (e.g., `"--verbose"`).
- **Returns**
  - `true` if the flag is present; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `flag` is `null`.

### `public abstract Task<int> ExecuteAsync()`
Executes the command asynchronously and returns an exit code. Implemented by derived command classes.

### `public override Task<int> ExecuteAsync()`
Executes the parsed command asynchronously and returns an exit code. This method is called after successful parsing.

### `public IngestCommand`
A concrete command class for the `"ingest"` verb, deriving from `CommandBase`.

### `public QueryCommand`
A concrete command class for the `"query"` verb, deriving from `CommandBase`.

### `public ExportCommand`
A concrete command class for the `"export"` verb, deriving from `CommandBase`.

## Usage

### Example 1: Basic Command Parsing and Execution
```csharp
var parser = new CommandLineParser();
parser.RegisterCommand("ingest", typeof(IngestCommand));

var args = new[] { "ingest", "--input", "data.csv", "--format", "csv" };
var parsed = parser.Parse(args);

if (parsed.IsValid)
{
    var exitCode = await parser.ExecuteAsync();
    Console.WriteLine($"Command executed with exit code: {exitCode}");
}
else
{
    Console.WriteLine($"Error: {parsed.ErrorMessage}");
}
```

### Example 2: Handling Required Options
```csharp
var parser = new CommandLineParser();
parser.RegisterCommand("query", typeof(QueryCommand));
parser.RequiredOptions.Add("--query");

var args = new[] { "query", "--query", "SELECT * FROM table" };
var parsed = parser.Parse(args);

if (parsed.IsValid)
{
    var exitCode = await parser.ExecuteAsync();
    Console.WriteLine($"Query executed successfully.");
}
else
{
    Console.WriteLine($"Missing required option: {parsed.ErrorMessage}");
}
```

## Notes

- **Thread Safety**: `CommandLineParser` is not thread-safe. Concurrent parsing or registration operations may lead to undefined behavior. External synchronization is required if used across threads.
- **Error Handling**: `Parse` throws exceptions for malformed input; ensure proper exception handling in calling code.
- **Option Precedence**: Later occurrences of the same option override earlier ones (e.g., `--input a --input b` results in `b` being retained).
- **Verb Registration**: Commands must be registered before parsing; otherwise, `Parse` will fail with an unrecognized verb error.
- **Case Sensitivity**: Option keys (e.g., `"--Input"` vs. `"--input"`) are treated as case-sensitive unless normalized by the caller.
