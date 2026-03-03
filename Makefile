.PHONY: help build test clean run docker-build docker-up docker-down lint format analyze package install dev-setup

# Default target
help:
	@echo "dotnet-realtime-pipeline - Build & Development Commands"
	@echo ""
	@echo "Build Commands:"
	@echo "  make build           Build the project"
	@echo "  make build-release   Build release configuration"
	@echo "  make build-debug     Build debug configuration"
	@echo ""
	@echo "Development Commands:"
	@echo "  make dev             Build and run in debug mode"
	@echo "  make run             Run the application"
	@echo "  make test            Run unit tests"
	@echo "  make test-watch      Run tests in watch mode"
	@echo "  make coverage        Generate code coverage report"
	@echo ""
	@echo "Code Quality Commands:"
	@echo "  make lint            Run code analyzers"
	@echo "  make format          Format code with dotnet format"
	@echo "  make format-check    Check code formatting"
	@echo "  make analyze         Run full static analysis"
	@echo ""
	@echo "Docker Commands:"
	@echo "  make docker-build    Build Docker image"
	@echo "  make docker-up       Start services with docker-compose"
	@echo "  make docker-down     Stop services"
	@echo "  make docker-logs     Show container logs"
	@echo "  make docker-clean    Remove containers and images"
	@echo ""
	@echo "Packaging Commands:"
	@echo "  make package         Create NuGet package"
	@echo "  make install         Install to local NuGet cache"
	@echo ""
	@echo "Setup Commands:"
	@echo "  make dev-setup       Setup development environment"
	@echo "  make clean           Clean build artifacts"
	@echo ""

# Build commands
build:
	@echo "Building project..."
	dotnet build

build-release:
	@echo "Building release configuration..."
	dotnet build -c Release

build-debug:
	@echo "Building debug configuration..."
	dotnet build -c Debug

# Development commands
dev: clean build
	@echo "Development build complete. Run 'make run' to start."

run:
	@echo "Running application..."
	dotnet run

dev-setup:
	@echo "Setting up development environment..."
	dotnet tool restore
	dotnet format --verify-no-changes
	@echo "Development setup complete."

# Testing commands
test:
	@echo "Running tests..."
	dotnet test --verbosity normal

test-watch:
	@echo "Running tests in watch mode..."
	dotnet watch test

coverage:
	@echo "Generating code coverage report..."
	dotnet test /p:CollectCoverage=true /p:CoverletOutput=coverage/ /p:CoverletOutputFormat=opencover
	@echo "Coverage report generated in coverage/ directory"

# Code quality commands
lint:
	@echo "Running code analyzers..."
	dotnet analyzers

format:
	@echo "Formatting code..."
	dotnet format

format-check:
	@echo "Checking code formatting..."
	dotnet format --verify-no-changes

analyze:
	@echo "Running static analysis..."
	dotnet analyzers --severity=warning

# Docker commands
docker-build:
	@echo "Building Docker image..."
	docker build -t dotnet-realtime-pipeline:latest .

docker-up:
	@echo "Starting Docker services..."
	docker-compose up -d
	@echo "Services started. Grafana: http://localhost:3000 (admin/admin)"
	@echo "                  Prometheus: http://localhost:9090"
	@echo "                  Pipeline: http://localhost:5000"

docker-down:
	@echo "Stopping Docker services..."
	docker-compose down

docker-logs:
	@echo "Showing container logs..."
	docker-compose logs -f pipeline

docker-clean:
	@echo "Cleaning up Docker artifacts..."
	docker-compose down -v
	docker rmi dotnet-realtime-pipeline:latest

# Packaging commands
package:
	@echo "Creating NuGet package..."
	dotnet pack -c Release --output ./artifacts

install: package
	@echo "Installing package to local NuGet cache..."
	dotnet nuget add source ./artifacts --name local-pkg
	@echo "Package installed locally."

# Cleanup
clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	rm -rf bin obj artifacts coverage
	rm -f *.nupkg
	rm -f coverage.*
	@echo "Clean complete."

# Utility commands
restore:
	@echo "Restoring dependencies..."
	dotnet restore

watch:
	@echo "Watching for changes and rebuilding..."
	dotnet watch build

info:
	@echo "Project Information:"
	@echo "  Name: dotnet-realtime-pipeline"
	@echo "  Framework: .NET 10.0"
	@echo "  Language: C# 13"
	@echo ""
	@dotnet --version

docs:
	@echo "Generating documentation..."
	@echo "See docs/ directory for available documentation:"
	@ls -la docs/

examples:
	@echo "Available examples:"
	@ls -la examples/

git-info:
	@echo "Git Information:"
	@git log --oneline -10
	@echo ""
	@git status

.SILENT: help info examples docs
