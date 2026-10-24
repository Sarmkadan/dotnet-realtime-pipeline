# Contributing to dotnet-realtime-pipeline

Thanks for helping out. Keep changes small, tested, and consistent with the code around them.

## Prerequisites

- **.NET 10 SDK** (the projects target `net10.0`). Check with `dotnet --version`.
- Git. Docker is only needed for `make docker-*` targets.

## Build

```bash
dotnet restore
dotnet build -c Release      # or: make build-release
```

The build currently reports several hundred warnings. Warnings are **not** errors (`TreatWarningsAsErrors` is not set), but do not add new ones.

## Test

Run from the repository root:

```bash
dotnet test                                                 # all tests (or: make test)
dotnet test --no-build -c Release --verbosity normal        # same form CI uses
dotnet test --filter "FullyQualifiedName~CacheService"      # one class
```

Tests live in `tests/dotnet-realtime-pipeline.Tests/` (xUnit, FluentAssertions, Moq). Some tests fail on `main` at the time of writing, so run the suite before and after your change and compare results rather than assuming a green baseline.

## Coding conventions

- **`.editorconfig` is the source of truth**: 4-space indent, Allman braces (opening brace on its own line), trailing newline. Run `dotnet format` (or `make format`) before pushing. CI does not run the format check yet, so verify it yourself with `dotnet format --verify-no-changes`.
- **Nullable reference types and implicit usings are enabled.** New source files start with `#nullable enable`.
- **File-scoped namespaces**, one per folder: `DotNetRealtimePipeline.<Folder>`. `using` directives go after the namespace.
- **`var`** is the norm for local variables. Spell the type out only when it is not apparent from the right-hand side.
- **Naming**: PascalCase for types and public members, `_camelCase` for private fields, `Async` suffix on async methods, `I` prefix on interfaces.
- **XML doc comments** on all public APIs (missing ones produce CS1591 warnings).
- **Logging** via `ILogger<T>` with structured templates (`{Name}`), never string interpolation.
- **Argument checks** with `ArgumentNullException.ThrowIfNull(...)`; domain errors as `PipelineException`.
- **Tests** go in `tests/dotnet-realtime-pipeline.Tests/`, named `<TypeName>Tests.cs`, with methods named `Method_Scenario` and Arrange/Act/Assert comments. Do not add files directly under `tests/`; they are not compiled.

## Branches and pull requests

1. Branch from an up-to-date `main`, using a prefix that matches the change:
   ```bash
   git switch main && git pull
   git switch -c fix/short-description
   ```
   Prefixes already in use: `feat/`, `fix/`, `docs/`, `test/`, `ci/`. External contributors should work from a fork.
2. Keep PRs **small and focused**: one logical change per PR. Do not mix refactors, formatting sweeps, and features.
3. Write **conventional commit** messages (`feat:`, `fix:`, `docs:`, `test:`, `refactor:`, `ci:`). Do not add `Co-Authored-By` lines.
4. Do not commit `bin/`, `obj/`, `TestResults/`, or other build output.
5. Push and open a PR against `main`. Fill in `.github/pull_request_template.md`: describe what changed and why, and list the exact commands you ran.
6. **CI must pass.** The `CI` workflow (`.github/workflows/ci.yml`) runs restore, `dotnet build -c Release`, and `dotnet test` on every push and PR to `main`. Fix failures before asking for review.

## Reporting issues

Use GitHub Issues. Include steps to reproduce, expected versus actual behaviour, and your output of `dotnet --version` plus your OS.

## License

MIT. By contributing, you agree that your changes are released under the same license.
