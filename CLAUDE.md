# CLAUDE.md

## Project overview

`dotnet-realtime-pipeline` - a .NET 10 console app/library for real-time data processing with backpressure, sliding windows, aggregation, dead-letter queue and retry policies. Root namespace `DotNetRealtimePipeline`.

## Build

```bash
dotnet restore
dotnet build                         # Debug
dotnet build -c Release              # what CI runs (dotnet 8.0.x and 10.0.x matrix)
dotnet run                           # runs the demo in Program.cs
make build | make build-release      # Makefile wrappers
```

Solution: `dotnet-realtime-pipeline.sln` (main project + test project). Benchmarks (`dotnet-realtime-pipeline.Benchmarks/`, BenchmarkDotNet) are excluded from the main csproj and not in the solution.

## Test

```bash
dotnet test                                                   # all tests (xUnit)
dotnet test --no-build -c Release --verbosity normal          # CI form
dotnet test --filter "FullyQualifiedName~CacheService"        # single class
```

- Test project: `tests/dotnet-realtime-pipeline.Tests/` (xUnit 2.9, FluentAssertions 8, Moq 4.20). Integration tests in `tests/dotnet-realtime-pipeline.Tests/Integration/`.
- Naming: `<TypeName>Tests.cs`, methods `Method_Scenario` or `TestMethod_Scenario`, Arrange/Act/Assert comments.
- WARNING: the ~175 `*.cs` files directly in `tests/` (not in the `.Tests` subfolder) are NOT compiled by any project (main csproj has `<Compile Remove="tests/**" />`). Put new tests in `tests/dotnet-realtime-pipeline.Tests/`.
- Current state (2026-09): build is green with ~560 warnings; test run has some failures (36/443 at last check). Do not assume a clean baseline - run tests before and after a change and compare.

## Lint / Format

```bash
dotnet format                        # apply .editorconfig
dotnet format --verify-no-changes    # check only (make format-check)
```

`.editorconfig` is the source of truth: 4-space indent, Allman braces (`csharp_new_line_before_open_brace = all`), `Nullable` and `ImplicitUsings` enabled, `LangVersion latest`. `GenerateDocumentationFile` is on, so missing XML docs on public members produce CS1591 warnings.

## Architecture

- `Program.cs` - entry point (top-level statements): builds a `ServiceCollection`, calls `AddPipelineServices(...)`, resolves `PipelineOrchestrator`, runs a demo.
- `src/Configuration/ServiceCollectionExtensions.cs` - the DI composition root (`AddPipelineServices`). All services are registered as singletons.
- `src/Services/` - core: `PipelineOrchestrator`, `DataProcessingService`, `WindowingService`, `MetricsService`, `BackpressureService`, `DynamicScalingService`, `QueryService`.
- `src/Domain/` - `Models/` (`DataPoint`, `StreamEvent`, `WindowEvent`, `PipelineConfig`, `ProcessingResult`, ...), `Enums/PipelineEnums.cs`, `Exceptions/PipelineException.cs`.
- `src/Data/Repositories/` - `IDataPointRepository`, `IMetricsRepository` + `InMemory*` implementations.
- `src/DeadLetter/` - `IDeadLetterQueue`, `IRetryPolicy`, `ExponentialBackoffRetryPolicy`, `RetryPolicyOptions`.
- Other subsystems, one folder each: `API`, `CLI`, `Caching`, `Events`, `Formatters`, `Integration`, `Metrics`, `Middleware`, `Monitoring`, `Plugins`, `State`, `Utilities`, `Visualization`, `Workers`.
- `src/stray/` - leftover fragments from a broken edit; ignore, do not extend.
- `docs/` - one markdown file per type plus `architecture.md`, `TESTING.md`, `faq.md`. `monitoring/` - Prometheus config; `docker-compose.yml` brings up pipeline + Prometheus + Grafana.

## Conventions

- Every source file starts with `#nullable enable` and the author header block (Vladyslav Zaiets | https://sarmkadan.com). File-scoped namespaces; `using` directives placed after the namespace.
- One namespace per folder: `DotNetRealtimePipeline.<Folder>`.
- Companion-file pattern per type `X`: `X.cs`, `XExtensions.cs` (static extension helpers), `XJsonExtensions.cs` (`ToJson(indented)`, `FromJson`, `TryFromJson`), `XValidation.cs` (`Validate()` returning `IReadOnlyList<string>`, `IsValid()`, `EnsureValid()` throwing `ArgumentException`). Follow it when adding a new type.
- Argument checks via `ArgumentNullException.ThrowIfNull(...)`; domain errors via `PipelineException(message, errorCode[, errorDetails])`.
- Naming: PascalCase public members, `_camelCase` private fields, `Async` suffix on async methods, interfaces prefixed `I`.
- XML doc comments on all public APIs (enforced by warnings).
- Logging via `ILogger<T>` with structured templates (`{Name}`), never string interpolation.
- Commits: conventional commits (`feat:`, `fix:`, `docs:`, `style(Scope):`, `ci:`). No `Co-Authored-By` lines.
- Do not commit `bin/`, `obj/`, `build/`, `.aider*`, `TestResults/` (gitignored). The stray zero-byte files in the repo root (e.g. `.Build();`, `}`) are junk from a tooling accident, not part of the project.
