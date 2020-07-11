#!/bin/bash
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# Test execution script with detailed reporting
# =============================================================================

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "=== Running Tests ==="
echo ""

# Parse arguments
TEST_TYPE=${1:-all}
VERBOSITY=${2:-minimal}

case $TEST_TYPE in
    unit)
        echo "Running unit tests..."
        dotnet test tests/Unit/ --verbosity=$VERBOSITY
        ;;
    integration)
        echo "Running integration tests..."
        dotnet test tests/Integration/ --verbosity=$VERBOSITY
        ;;
    all)
        echo "Running all tests..."
        dotnet test tests/ --verbosity=$VERBOSITY
        ;;
    coverage)
        echo "Running tests with code coverage..."
        dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover /p:CoverletOutput=../coverage/
        echo ""
        echo "Coverage report available in: coverage/coverage.xml"
        ;;
    watch)
        echo "Running tests in watch mode..."
        dotnet watch test tests/
        ;;
    *)
        echo -e "${RED}Unknown test type: $TEST_TYPE${NC}"
        echo "Usage: ./test.sh [unit|integration|all|coverage|watch] [verbosity]"
        exit 1
        ;;
esac

echo ""
echo -e "${GREEN}✓ Tests completed${NC}"
