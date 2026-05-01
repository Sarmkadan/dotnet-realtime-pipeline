# Development Guide - dotnet-realtime-pipeline

## Getting Started with Development

This guide provides step-by-step instructions for setting up a development environment and contributing to the dotnet-realtime-pipeline project.

## Prerequisites

- **.NET 10.0 SDK** - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com)
- **Git** - Version control
- **Docker** (optional) - For containerized development
- **Visual Studio 2022** or **VS Code** - Recommended IDEs
- **4GB RAM minimum** - For development

## Initial Setup

### 1. Clone the Repository

```bash
git clone https://github.com/Sarmkadan/dotnet-realtime-pipeline.git
cd dotnet-realtime-pipeline
```

### 2. Run Setup Script

```bash
# On Linux/macOS
chmod +x scripts/setup.sh
./scripts/setup.sh

# On Windows
powershell -ExecutionPolicy Bypass -File scripts/setup.ps1
```

### 3. Verify Installation

```bash
# Check .NET version
dotnet --version

# Build project
dotnet build

# Run tests
dotnet test

# Run application
dotnet run
```

## Project Structure

```
dotnet-realtime-pipeline/
├── src/                       # Source code
│   ├── API/                  # REST API handlers
│   ├── Caching/              # Caching layer
│   ├── CLI/                  # Command-line interface
│   ├── Configuration/        # Configuration management
│   ├── Constants/            # Constants
│   ├── Data/                 # Data access
│   ├── Domain/               # Domain models
│   ├── Events/               # Event system
│   ├── Formatters/           # Output formatters
│   ├── Initialization/       # Initialization logic
│   ├── Integration/          # External integrations
│   ├── Middleware/           # Middleware components
│   ├── Monitoring/           # Health and monitoring
│   ├── Plugins/              # Plugin system
│   ├── Services/             # Core services
│   ├── State/                # State management
│   ├── Utilities/            # Utility functions
│   └── Workers/              # Background workers
├── tests/                    # Test projects
│   ├── Unit/                 # Unit tests
│   └── Integration/          # Integration tests
├── examples/                 # Example applications
├── docs/                     # Documentation
├── scripts/                  # Utility scripts
├── monitoring/               # Monitoring configs
├── sql/                      # Database schemas
├── .github/                  # GitHub configuration
├── README.md                 # Project overview
├── Makefile                  # Build targets
└── dotnet-realtime-pipeline.csproj  # Project file
```

## Development Workflow

### Using VS Code

1. Open folder: `File > Open Folder`
2. Install "C# Dev Kit" extension
3. Press `Ctrl+Shift+B` to build
4. Press `F5` to debug
5. Open terminal: ``Ctrl+` ``

### Using Visual Studio 2022

1. Open solution file
2. Press `Ctrl+Shift+B` to build
3. Press `F5` to debug
4. Open Package Manager Console: `Tools > NuGet Package Manager > Package Manager Console`

## Common Development Tasks

### Build Project

```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Using Make
make build
make release
```

### Run Application

```bash
# Debug mode
dotnet run

# Release mode
dotnet run -c Release

# With arguments
dotnet run -- --help

# Using Make
make run
make run-release
```

### Run Tests

```bash
# All tests
dotnet test

# Specific category
dotnet test --filter "Category=Unit"

# With coverage
dotnet test /p:CollectCoverage=true

# Watch mode
dotnet watch test

# Using Make
make test
make test-unit
make coverage
```

### Code Formatting

```bash
# Format all code
dotnet format

# Check formatting
dotnet format --verify-no-changes

# Using Make
make format
make format-check
```

### Run Examples

```bash
# All examples
make examples

# Specific example
make example-01
dotnet run -c Release --project examples/ -- 01-simple-ingestion
```

### Run Specific Service Tests

```bash
# Test data repository
dotnet test --filter "ClassName=DataPointRepositoryTests"

# Test backpressure service
dotnet test --filter "ClassName=BackpressureServiceTests"

# Test windowing service
dotnet test --filter "ClassName=WindowingServiceTests"
```

## Code Guidelines

### Naming Conventions

```csharp
// Classes, Methods: PascalCase
public class DataPointRepository { }
public async Task ProcessDataAsync() { }

// Private fields: _camelCase
private readonly string _connectionString;
private int _buffer;

// Local variables: camelCase
var dataPoints = new List<DataPoint>();
int processedCount = 0;

// Constants: UPPER_SNAKE_CASE
private const int DEFAULT_BUFFER_SIZE = 10000;
public const string DEFAULT_WINDOW_TYPE = "TUMBLING";
```

### File Organization

```csharp
// 1. Using statements
using System;
using Microsoft.Extensions.DependencyInjection;

// 2. Namespace
namespace DotNetRealtimePipeline.Services;

// 3. Class/Interface/Record declaration
public class MyService
{
    // 4. Fields
    private readonly ILogger _logger;

    // 5. Constructor
    public MyService(ILogger logger) => _logger = logger;

    // 6. Public methods
    public async Task DoSomethingAsync() { }

    // 7. Private methods
    private void HelperMethod() { }
}
```

### Documentation

```csharp
/// <summary>
/// Processes a single data point through the pipeline.
/// </summary>
/// <param name="dataPoint">The data point to process</param>
/// <returns>true if processing succeeded; otherwise false</returns>
/// <exception cref="ArgumentNullException">Thrown when dataPoint is null</exception>
public async Task<bool> ProcessDataPointAsync(DataPoint dataPoint)
{
    ArgumentNullException.ThrowIfNull(dataPoint);
    // Implementation
}
```

### Error Handling

```csharp
// Use specific exceptions
try
{
    // code
}
catch (ArgumentNullException ex)
{
    _logger.LogError(ex, "Invalid argument");
    throw;
}
catch (InvalidOperationException ex)
{
    _logger.LogError(ex, "Invalid operation");
    // Handle or propagate
}
```

### Async/Await

```csharp
// Always use async/await for I/O operations
public async Task<DataPoint> GetDataPointAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

// Don't use .Result or .Wait()
// Bad: var point = GetDataPointAsync(1).Result;
// Good: var point = await GetDataPointAsync(1);
```

### Testing Patterns

```csharp
[Fact]
public async Task MethodName_WhenCondition_ShouldBehavior()
{
    // Arrange - Setup
    var service = new MyService();
    var input = CreateTestInput();

    // Act - Execute
    var result = await service.ProcessAsync(input);

    // Assert - Verify
    Assert.NotNull(result);
    Assert.True(result.Success);
}
```

## Performance Considerations

### Memory

```csharp
// Use object pooling for frequently allocated objects
private readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;

byte[] buffer = _pool.Rent(size);
try
{
    // Use buffer
}
finally
{
    _pool.Return(buffer);
}
```

### Concurrency

```csharp
// Use thread-safe collections
private readonly ConcurrentDictionary<string, int> _cache = new();

// Or use locks for complex operations
private readonly object _lockObj = new();
lock (_lockObj)
{
    // Thread-safe operation
}
```

### Async Operations

```csharp
// Prefer ValueTask for hot paths
public ValueTask<bool> TryProcessAsync(DataPoint point)
{
    // Implementation that often completes synchronously
}
```

## Debugging

### Enable Detailed Logging

```bash
# Set environment variable
export DOTNET_LOG_LEVEL=Debug
dotnet run
```

### Attach Debugger in VS Code

1. Create `.vscode/launch.json`:
```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch",
            "type": "coreclr",
            "request": "launch",
            "program": "${workspaceFolder}/bin/Debug/net10.0/dotnet-realtime-pipeline.dll",
            "cwd": "${workspaceFolder}",
            "stopAtEntry": false,
            "console": "internalConsole"
        }
    ]
}
```

2. Press `F5` to start debugging

### Print Debugging

```csharp
// Use structured logging instead of Console.WriteLine
_logger.LogInformation("Processing data: {DataId}", dataPoint.Id);
_logger.LogError(ex, "Processing failed for {DataId}", dataPoint.Id);
```

## Git Workflow

### Feature Branch

```bash
# Create feature branch
git checkout -b feature/amazing-feature

# Make changes and commit
git add src/MyFile.cs
git commit -m "Add amazing feature"

# Push and create PR
git push origin feature/amazing-feature
```

### Commit Messages

```
# Good commit message
Fix backpressure calculation when buffer exceeds 80% capacity

This fixes issue #123 where buffer calculations were incorrect
when approaching capacity threshold. The fix ensures accurate
backpressure decisions.

Fixes #123

# Avoid: "Fix stuff", "Update file", "WIP"
```

### Pull Request Checklist

- [ ] Code builds without warnings
- [ ] All tests pass
- [ ] Code is formatted (`dotnet format`)
- [ ] Documentation is updated
- [ ] Commit messages are clear
- [ ] No hardcoded values or secrets
- [ ] Performance impact is minimal

## Continuous Integration

The project uses GitHub Actions for CI. Workflows are in `.github/workflows/`:

- **build.yml**: Builds and tests on multiple platforms
- Runs on: Pull requests, pushes to main

To test locally:

```bash
# Build as CI would
dotnet build -c Release
dotnet test --no-build
```

## Troubleshooting

### NuGet Restore Issues

```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore
dotnet restore
```

### Build Errors

```bash
# Clean everything
dotnet clean
rm -rf bin obj

# Rebuild
dotnet build
```

### Test Failures

```bash
# Run with detailed output
dotnet test --verbosity detailed

# Run specific test
dotnet test --filter "Name=MyTestName"
```

### Performance Issues During Development

```bash
# Use Release configuration for testing performance
dotnet run -c Release
dotnet test -c Release
```

## Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [async/await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [xUnit Testing](https://xunit.net/)

## Getting Help

- Check [FAQ](./faq.md) for common questions
- Review [API Reference](./api-reference.md) for available methods
- Check existing [GitHub Issues](https://github.com/Sarmkadan/dotnet-realtime-pipeline/issues)
- Review [examples](../examples/) for usage patterns

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
