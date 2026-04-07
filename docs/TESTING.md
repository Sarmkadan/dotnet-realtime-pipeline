# Testing Guide - dotnet-realtime-pipeline

## Overview

This document provides comprehensive testing guidance for the dotnet-realtime-pipeline project. We include unit tests, integration tests, performance tests, and end-to-end testing strategies.

## Test Structure

```
tests/
├── Unit/                          # Unit tests
│   ├── DataPointRepositoryTests.cs
│   ├── BackpressureServiceTests.cs
│   └── WindowingServiceTests.cs
├── Integration/                   # Integration tests
│   └── PipelineIntegrationTests.cs
└── dotnet-realtime-pipeline.Tests.csproj  # Test project file
```

## Running Tests

### Run All Tests

```bash
# Using Make
make test

# Using dotnet CLI
dotnet test

# With verbose output
dotnet test --verbosity detailed
```

### Run Specific Test Categories

```bash
# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests only
dotnet test --filter "Category=Integration"

# Specific test class
dotnet test --filter "ClassName=DataPointRepositoryTests"

# Specific test method
dotnet test --filter "Name=AddAsync_WithValidDataPoint_ShouldSucceed"
```

### Run Tests in Watch Mode

```bash
# Automatically re-run tests on code changes
make watch
./scripts/test.sh watch
```

### Generate Code Coverage

```bash
# Generate coverage report
make coverage

./scripts/test.sh coverage
```

Coverage reports are generated in the `coverage/` directory with:
- OpenCover format: `coverage.xml`
- HTML report (if configured): `index.html`

## Test Categories

### Unit Tests

**Purpose**: Test individual components in isolation

**Location**: `tests/Unit/`

**Examples**:
- `DataPointRepositoryTests` - Tests data persistence layer
- `BackpressureServiceTests` - Tests flow control logic
- `WindowingServiceTests` - Tests windowing calculations

**Running Unit Tests**:
```bash
dotnet test tests/Unit/ --verbosity minimal
```

**Best Practices**:
1. Each test should be independent
2. Use meaningful test names: `[Method]_[Condition]_[Expected]`
3. Arrange-Act-Assert (AAA) pattern
4. Mock external dependencies
5. Test both success and failure paths

### Integration Tests

**Purpose**: Test multiple components working together

**Location**: `tests/Integration/`

**Examples**:
- `PipelineIntegrationTests` - Tests full pipeline workflows
- Multi-source data ingestion
- Concurrent processing scenarios

**Running Integration Tests**:
```bash
dotnet test tests/Integration/ --verbosity minimal
```

**Key Scenarios**:
1. Pipeline startup and shutdown
2. Data ingestion and processing
3. Query and retrieval operations
4. Health reporting
5. Backpressure handling
6. Multi-source concurrent processing

## Test Patterns

### Pattern 1: Basic Service Testing

```csharp
[Fact]
public async Task ProcessData_WithValidInput_ShouldSucceed()
{
    // Arrange
    var service = new DataProcessingService();
    var dataPoint = new DataPoint(1, DateTime.UtcNow.Ticks, 42.5m, "Sensor-1");

    // Act
    var result = await service.ProcessDataPointAsync(dataPoint);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.Success);
}
```

### Pattern 2: Repository Testing

```csharp
[Fact]
public async Task GetBySourceAsync_WithValidSource_ShouldReturnMatching()
{
    // Arrange
    var repository = new InMemoryDataPointRepository();
    var points = new[] {
        new DataPoint(1, DateTime.UtcNow.Ticks, 10, "Sensor-1"),
        new DataPoint(2, DateTime.UtcNow.Ticks, 20, "Sensor-2"),
        new DataPoint(3, DateTime.UtcNow.Ticks, 30, "Sensor-1")
    };
    await repository.AddRangeAsync(points);

    // Act
    var result = await repository.GetBySourceAsync("Sensor-1");

    // Assert
    Assert.Equal(2, result.Count());
}
```

### Pattern 3: Integration Testing

```csharp
[Fact]
public async Task EndToEnd_ShouldProcessDataSuccessfully()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddPipelineServices();
    var provider = services.BuildServiceProvider();
    var orchestrator = provider.GetRequiredService<PipelineOrchestrator>();

    await orchestrator.StartAsync();

    try
    {
        // Act
        var point = new DataPoint(1, DateTime.UtcNow.Ticks, 42.5m, "S1");
        await orchestrator.IngestDataPointAsync(point);
        await Task.Delay(500);

        // Assert
        var status = orchestrator.GetStatus();
        Assert.True(status.TotalDataPointsProcessed > 0);
    }
    finally
    {
        await orchestrator.StopAsync();
    }
}
```

## Performance Testing

### Load Testing

```csharp
[Fact]
public async Task ProcessLargeDataset_ShouldCompleteTiming()
{
    var orchestrator = SetupOrchestrator();
    var stopwatch = Stopwatch.StartNew();

    // Ingest 100,000 data points
    for (int i = 0; i < 100000; i++)
    {
        var point = new DataPoint(i, DateTime.UtcNow.Ticks, i * 0.1m, "S1");
        await orchestrator.IngestDataPointAsync(point);
    }

    stopwatch.Stop();

    // Assert throughput is acceptable
    var throughput = 100000 / stopwatch.Elapsed.TotalSeconds;
    Assert.True(throughput > 10000); // At least 10k items/sec
}
```

### Memory Testing

```bash
# Build Release configuration
dotnet build -c Release

# Run with memory monitoring
dotnet run -c Release --project examples/

# Use .NET diagnostic tools
dotnet counters monitor --process-id <PID>
```

### Benchmarking

```bash
# Run all examples in Release mode for consistent benchmarking
dotnet build -c Release
./scripts/test.sh coverage

# Measure specific scenario
time dotnet run -c Release -- 02-multi-source-processing
```

## Continuous Integration

### GitHub Actions Workflow

The CI/CD pipeline automatically:
1. Builds on multiple .NET versions
2. Runs all tests
3. Generates coverage reports
4. Publishes artifacts

**Workflow File**: `.github/workflows/build.yml`

**Key Jobs**:
- **Build**: Compiles on Windows, Linux, macOS
- **Test**: Runs all test suites
- **Coverage**: Generates coverage reports

### Running CI Locally

```bash
# Simulate CI build
dotnet build -c Release
dotnet test tests/ --verbosity minimal

# Check code formatting
dotnet format --verify-no-changes

# Run static analysis
dotnet build /p:EnforceCodeStyleInBuild=true
```

## Debugging Tests

### Enable Verbose Logging

```bash
# Detailed test output
dotnet test --verbosity detailed

# Show debug messages
dotnet test --logger "console;verbosity=detailed"
```

### Run Single Test with Debugging

```bash
# Use your IDE's test explorer
# Or run specific test by name
dotnet test --filter "Name=YourTestName" --no-build
```

### Attach Debugger

In Visual Studio:
1. Open Test Explorer (Test > Test Explorer)
2. Right-click test
3. Select "Debug Selected Tests"

## Best Practices

### 1. Test Naming Convention

```
[MethodName]_[Condition]_[ExpectedResult]

Example:
AddAsync_WithValidDataPoint_ShouldSucceed
GetBySourceAsync_WithNonExistentSource_ShouldReturnEmpty
ProcessBatch_WhenExceeding_ShouldApplyBackpressure
```

### 2. Arrange-Act-Assert Pattern

```csharp
[Fact]
public void TestSomething()
{
    // Arrange - Setup test data and dependencies
    var service = new MyService();
    var input = CreateTestInput();

    // Act - Execute the code being tested
    var result = service.DoSomething(input);

    // Assert - Verify the result
    Assert.NotNull(result);
}
```

### 3. One Assertion Per Concept

```csharp
// Good - Multiple assertions on same concept
[Fact]
public void Calculate_ShouldReturnCorrectValues()
{
    var result = Calculate(10);
    
    Assert.Equal(20, result.Value);
    Assert.Equal(Status.Success, result.Status);
}

// Avoid - Testing different concerns in one test
[Fact]
public void DoMultipleThings_ShouldWork()
{
    // Testing 5 different things in one test
}
```

### 4. Use Fixtures for Setup

```csharp
public class MyServiceTests
{
    private readonly ServiceFixture _fixture = new();

    [Fact]
    public void Test1() => Assert.NotNull(_fixture.Service);

    [Fact]
    public void Test2() => Assert.NotNull(_fixture.Repository);
}
```

### 5. Test Edge Cases

```csharp
[Fact]
public void ProcessData_WithZeroValue_ShouldHandle()
{
    var point = new DataPoint(1, DateTime.UtcNow.Ticks, 0, "S1");
    var result = _service.Process(point);
    Assert.NotNull(result);
}

[Fact]
public void ProcessData_WithNegativeValue_ShouldReject()
{
    var point = new DataPoint(1, DateTime.UtcNow.Ticks, -100, "S1");
    var result = _service.Process(point);
    Assert.False(result.Success);
}

[Fact]
public void ProcessData_WithNullSource_ShouldThrow()
{
    var point = new DataPoint(1, DateTime.UtcNow.Ticks, 50, null!);
    Assert.Throws<ArgumentNullException>(() => _service.Process(point));
}
```

## Troubleshooting

### Tests Hanging

**Cause**: Deadlock or infinite loop in async code

**Solution**:
```bash
# Set timeout
dotnet test --logger "console;verbosity=detailed" /p:TestTimeout=5000

# Run in parallel with timeout
dotnet test -m:1 --logger "console"
```

### Flaky Tests

**Cause**: Race conditions, timing issues, external dependencies

**Solution**:
```csharp
// Add explicit waits
await Task.Delay(100);

// Use retry logic
await Task.Delay(TimeSpan.FromSeconds(1));
var result = await service.GetResultAsync();
```

### Memory Leaks

**Cause**: Disposed resources not cleaned up in tests

**Solution**:
```csharp
[Fact]
public async Task Test_ShouldCleanup()
{
    using var service = new MyService();
    
    // Test code

    // Disposal happens automatically
}
```

## Resources

- [xUnit.net Documentation](https://xunit.net/)
- [Microsoft Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [.NET Test Runners](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test)

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
