# Contributing to dotnet-realtime-pipeline

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing to the project.

## Code of Conduct

We are committed to providing a welcoming and inclusive environment for all contributors. Please:

- Be respectful and constructive in all interactions
- Value diverse perspectives and backgrounds
- Focus on what is best for the community
- Show empathy towards other community members
- Report unacceptable behavior to maintainers

## Ways to Contribute

### 1. Reporting Bugs

Found a bug? Please report it by opening an issue with:

- **Title**: Clear, descriptive summary
- **Description**: What you expected vs. what happened
- **Steps to Reproduce**: Clear steps to reproduce the issue
- **Environment**: OS, .NET version, configuration
- **Code Sample**: Minimal reproducible example if applicable
- **Logs/Error Messages**: Full stack trace

**Good Bug Report Example**:
```
Title: BackpressureService not triggering at configured threshold

Description:
Setting BackpressureThreshold to 0.75 doesn't trigger backpressure 
when buffer reaches 75% capacity.

Steps to Reproduce:
1. Configure BackpressureThreshold = 0.75
2. Set MaxBufferSize = 10000
3. Ingest 7500+ items
4. Observe: No backpressure triggered

Expected: Backpressure should trigger around 7500 items
Actual: No backpressure until buffer is completely full

Environment:
- OS: Ubuntu 22.04
- .NET: 10.0.0
- dotnet-realtime-pipeline: v1.2.0
```

### 2. Suggesting Enhancements

Have an idea to improve the project?

- **Title**: Brief feature description
- **Motivation**: Why this feature would be useful
- **Proposed Solution**: How you envision it working
- **Alternative Approaches**: Other solutions considered
- **Additional Context**: Links, examples, use cases

### 3. Submitting Pull Requests

#### Before You Start

1. **Fork** the repository
2. **Clone** your fork locally
3. **Create** a feature branch: `git checkout -b feature/your-feature-name`
4. **Read** this entire contributing guide

#### Development Setup

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/dotnet-realtime-pipeline.git
cd dotnet-realtime-pipeline

# Install dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test

# Format your code
dotnet format
```

#### Writing Code

**Code Style**:
- Follow [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/fundamentals/coding-style)
- Use meaningful variable and method names
- Keep methods focused and under 50 lines when possible
- Use async/await for I/O operations

**File Structure**:
```
src/
├── Services/
│   ├── MyNewService.cs      # Class implementing the service
│   └── IMyNewService.cs     # Interface (optional)
├── Domain/
│   ├── Models/
│   │   └── MyNewModel.cs    # Domain model
│   └── Enums/
│       └── MyNewEnum.cs     # Enumeration
└── Utilities/
    └── MyNewHelper.cs       # Helper utilities
```

**Comments and Documentation**:

```csharp
// =============================================================================
// Author: Your Name | https://yoursite.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Processes a collection of data points with validation and error handling.
/// </summary>
/// <param name="dataPoints">The data points to process</param>
/// <returns>Processing results with status information</returns>
/// <exception cref="ArgumentNullException">Thrown when dataPoints is null</exception>
public async Task<List<ProcessingResult>> ProcessBatchAsync(
    IEnumerable<DataPoint> dataPoints)
{
    // Brief comment explaining why, not what
    // ...
}
```

**Testing**:

```csharp
[Fact]
public async Task ProcessDataPointAsync_WithValidInput_ReturnsSuccessResult()
{
    // Arrange
    var service = new DataProcessingService();
    var dataPoint = new DataPoint(1, DateTime.UtcNow.Ticks, 42.5m, "Test");

    // Act
    var result = await service.ProcessDataPointAsync(dataPoint);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.QualityScore > 0);
}

[Fact]
public async Task ProcessDataPointAsync_WithInvalidInput_ReturnsFailureResult()
{
    // Arrange
    var service = new DataProcessingService();
    var dataPoint = new DataPoint(1, 0, decimal.MinValue, "");

    // Act
    var result = await service.ProcessDataPointAsync(dataPoint);

    // Assert
    Assert.False(result.IsSuccess);
}
```

#### Commit Messages

Write clear, descriptive commit messages:

```
feat: Add trend analysis to MetricsService

Add AnalyzePerformanceTrendAsync() method to detect performance trends.
Implements exponential smoothing for trend calculation.

- Detects upward, downward, stable, and oscillating trends
- Returns trend direction and slope value
- Handles edge cases with < 3 data points

Fixes #123
```

**Commit Message Format**:
- Start with type: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`
- Use imperative mood ("Add" not "Added")
- Keep first line under 72 characters
- Reference issues with `Fixes #123`
- Explain *why*, not what

#### Pull Request Process

1. **Update** your branch with latest main: `git rebase main`
2. **Ensure** tests pass: `dotnet test`
3. **Format** code: `dotnet format`
4. **Push** your changes: `git push origin feature/your-feature`
5. **Create** Pull Request on GitHub

**PR Description Template**:

```markdown
## Summary
Brief description of changes

## Motivation
Why these changes are needed

## Changes
- Change 1
- Change 2
- Change 3

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
How to test these changes

## Checklist
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] Code formatted with `dotnet format`
- [ ] No breaking changes (or documented)
- [ ] Performance impact analyzed (if applicable)

## Related Issues
Fixes #123
Relates to #456
```

#### PR Review Process

- Expect feedback and be open to suggestions
- Respond to comments within reasonable timeframe
- Request re-review after making changes
- Thank reviewers for their time and feedback

### 4. Improving Documentation

Documentation improvements are highly valued:

- Fix typos and grammar errors
- Clarify confusing explanations
- Add examples to documentation
- Update outdated information
- Translate documentation to other languages

Documentation files are in:
- `README.md`: Main project overview
- `docs/`: Detailed guides and references
- Code comments: Inline documentation

### 5. Writing Tests

Help improve test coverage:

```bash
# Generate coverage report
make coverage

# View coverage in coverage/index.html
```

Target coverage: **80%+** of code

Test file naming: `[Feature]Tests.cs`

## Development Workflow

### Local Development

```bash
# Start development server
dotnet run

# Run tests in watch mode
dotnet watch test

# Format and lint code
dotnet format
dotnet analyzers

# Build release version
dotnet build -c Release
```

### Using Docker for Development

```bash
# Build and start services
docker-compose up -d

# View logs
docker-compose logs -f pipeline

# Stop services
docker-compose down
```

### Performance Benchmarking

```bash
# Run benchmarks
dotnet run -c Release -- --benchmark

# View results in BenchmarkDotNet.Artifacts/
```

## Project Structure

```
dotnet-realtime-pipeline/
├── src/                    # Source code
│   ├── Configuration/      # DI setup
│   ├── Domain/            # Domain models
│   ├── Services/          # Business logic
│   ├── Data/              # Repositories
│   └── Utilities/         # Helpers
├── tests/                 # Unit tests
├── examples/              # Example programs
├── docs/                  # Documentation
├── .github/workflows/     # CI/CD
├── Dockerfile             # Container image
├── docker-compose.yml     # Local development
└── Makefile              # Build commands
```

## Coding Standards Checklist

Before submitting a PR, verify:

- [ ] Code follows C# conventions
- [ ] Variable names are descriptive
- [ ] Methods are focused (< 50 lines)
- [ ] Async/await used for I/O
- [ ] No blocking `.Result` or `.Wait()`
- [ ] Comments explain *why*, not *what*
- [ ] No commented-out code
- [ ] Tests added for new features
- [ ] Tests pass locally: `dotnet test`
- [ ] Code formatted: `dotnet format`
- [ ] No analyzer warnings: `dotnet analyzers`
- [ ] Documentation updated if needed
- [ ] CHANGELOG.md entry added

## Performance Guidelines

When contributing performance-sensitive code:

- Benchmark before and after
- Document performance trade-offs
- Consider memory allocation
- Use `.NET Diagnostics` tools
- Profile with `dotnet trace`

## Security Considerations

When reporting security vulnerabilities:

1. **Do NOT** open a public issue
2. **Email** vladyslav.zaiets@amdaris.com with:
   - Description of vulnerability
   - Impact assessment
   - Suggested fix (if available)
3. Give maintainers 90 days to address before disclosure

## Recognition

Contributors will be recognized:

- In CHANGELOG.md for new features/fixes
- In GitHub contributors section
- In project documentation as appropriate

## Questions?

- **Implementation questions**: Open a discussion
- **API questions**: Check api-reference.md
- **Architecture questions**: See architecture.md
- **General questions**: Open an issue

## Additional Resources

- [Code of Conduct](./CODE_OF_CONDUCT.md)
- [Architecture Guide](./docs/architecture.md)
- [API Reference](./docs/api-reference.md)
- [Deployment Guide](./docs/deployment.md)

## Thank You!

Your contributions make this project better. Thank you for taking the time to contribute!

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**
