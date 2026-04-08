# Contributing to dotnet-realtime-pipeline

Thank you for your interest in contributing to `dotnet-realtime-pipeline`!

## Getting Started

1. **Fork** the repository on GitHub.
2. **Clone** your fork locally:
   ```bash
   git clone https://github.com/<your-username>/dotnet-realtime-pipeline.git
   cd dotnet-realtime-pipeline
   ```
3. **Create a branch** for your feature or bug fix:
   ```bash
   git checkout -b feature/my-feature
   ```

## Development Requirements

- **.NET 10.0 SDK** — required to build and run the project. Download from https://dotnet.microsoft.com/download
- **Docker** (optional) — required only for container-based testing

## Building Locally

```bash
# Restore NuGet packages
dotnet restore

# Build in Release configuration
dotnet build --configuration Release

# Or use the provided script
./scripts/build.sh
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run with verbose output and generate a TRX report
dotnet test --verbosity normal --logger "trx;LogFileName=test-results.trx"

# Run only unit tests
dotnet test tests/dotnet-realtime-pipeline.Tests/dotnet-realtime-pipeline.Tests.csproj

# Or use the provided script
./scripts/test.sh
```

## Code Style

- Follow the `.editorconfig` rules already present in the repository — most editors apply these automatically.
- Use `PascalCase` for types, properties, and public members.
- Prefix private fields with `_` (e.g., `_myField`).
- Suffix `async` methods with `Async` (e.g., `ProcessAsync`).
- Ensure all public APIs have **XML documentation comments**.
- Do not leave unused `using` directives or dead code.

## Submitting a Pull Request

1. Make sure all tests pass locally before opening a PR.
2. Write clear, descriptive commit messages (use conventional commits where appropriate: `feat:`, `fix:`, `docs:`, `ci:`, etc.).
3. Push your branch to your fork and open a Pull Request against `main`.
4. Fill in the pull request template — include a summary of changes and any relevant context.
5. A CI check will run automatically; ensure it passes before requesting review.

## Reporting Issues

If you find a bug or have a feature request, please use **GitHub Issues**.  
When reporting a bug, include:
- A clear description of the problem
- Minimal steps to reproduce it
- Expected vs. actual behaviour
- Your .NET version (`dotnet --version`) and OS

## License

By contributing, you agree that your contributions will be licensed under the MIT License.