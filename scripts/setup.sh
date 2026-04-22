#!/bin/bash
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# Setup script for dotnet-realtime-pipeline development environment
# =============================================================================

set -e

echo "=== dotnet-realtime-pipeline Setup ==="
echo ""

# Check .NET installation
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 10 SDK."
    echo "   Visit: https://dotnet.microsoft.com/download"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo "✓ .NET $DOTNET_VERSION detected"

# Restore dependencies
echo ""
echo "Restoring dependencies..."
dotnet restore

# Build solution
echo ""
echo "Building solution..."
dotnet build

# Install local tools
echo ""
echo "Installing local tools..."
dotnet tool restore 2>/dev/null || echo "⚠ Some tools could not be installed (optional)"

# Build tests
echo ""
echo "Building tests..."
dotnet build tests/

# Create directories if they don't exist
echo ""
echo "Creating directory structure..."
mkdir -p artifacts
mkdir -p coverage
mkdir -p reports

echo ""
echo "✓ Setup complete!"
echo ""
echo "Next steps:"
echo "  1. Run the application: dotnet run"
echo "  2. Run tests: dotnet test"
echo "  3. Run examples: make examples"
echo "  4. View docs: make docs"
