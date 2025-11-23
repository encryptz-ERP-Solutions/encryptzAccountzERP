#!/bin/bash

# Script to run authentication tests
# Usage: ./run-auth-tests.sh [options]
#   Options:
#     all          - Run all tests (default)
#     unit         - Run unit tests only
#     integration  - Run integration tests only
#     coverage     - Run tests with coverage report
#     watch        - Run tests in watch mode
#     help         - Show this help message

set -e

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Project paths
PROJECT_ROOT="/Applications/Work/Encryptz/Github/encryptzAccountzERP/CoreModule"
TEST_PROJECT="$PROJECT_ROOT/API/Tests/AuthTests"

echo -e "${BLUE}================================${NC}"
echo -e "${BLUE}Authentication Tests Runner${NC}"
echo -e "${BLUE}================================${NC}\n"

# Check if test project exists
if [ ! -d "$TEST_PROJECT" ]; then
    echo -e "${RED}Error: Test project not found at $TEST_PROJECT${NC}"
    exit 1
fi

# Change to test directory
cd "$TEST_PROJECT"

# Function to show help
show_help() {
    echo "Usage: ./run-auth-tests.sh [options]"
    echo ""
    echo "Options:"
    echo "  all          - Run all tests (default)"
    echo "  unit         - Run unit tests only"
    echo "  integration  - Run integration tests only"
    echo "  coverage     - Run tests with coverage report"
    echo "  watch        - Run tests in watch mode"
    echo "  help         - Show this help message"
    echo ""
    echo "Examples:"
    echo "  ./run-auth-tests.sh"
    echo "  ./run-auth-tests.sh unit"
    echo "  ./run-auth-tests.sh coverage"
}

# Function to run all tests
run_all_tests() {
    echo -e "${GREEN}Running all authentication tests...${NC}\n"
    dotnet test --logger "console;verbosity=normal"
}

# Function to run unit tests
run_unit_tests() {
    echo -e "${GREEN}Running unit tests only...${NC}\n"
    dotnet test --filter "FullyQualifiedName~AuthServiceUnitTests" --logger "console;verbosity=normal"
}

# Function to run integration tests
run_integration_tests() {
    echo -e "${GREEN}Running integration tests only...${NC}\n"
    dotnet test --filter "FullyQualifiedName~AuthIntegrationTests" --logger "console;verbosity=normal"
}

# Function to run tests with coverage
run_with_coverage() {
    echo -e "${GREEN}Running tests with code coverage...${NC}\n"
    
    # Run tests with coverage
    dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./coverage/
    
    # Check if reportgenerator is installed
    if ! command -v reportgenerator &> /dev/null; then
        echo -e "\n${YELLOW}Installing reportgenerator...${NC}"
        dotnet tool install -g dotnet-reportgenerator-globaltool
    fi
    
    # Generate HTML report
    echo -e "\n${GREEN}Generating coverage report...${NC}"
    reportgenerator -reports:./coverage/coverage.opencover.xml -targetdir:./coverage/report -reporttypes:Html
    
    # Open report in browser (macOS)
    if [[ "$OSTYPE" == "darwin"* ]]; then
        echo -e "\n${GREEN}Opening coverage report in browser...${NC}"
        open ./coverage/report/index.html
    else
        echo -e "\n${GREEN}Coverage report generated at: ./coverage/report/index.html${NC}"
    fi
}

# Function to run tests in watch mode
run_watch_mode() {
    echo -e "${GREEN}Running tests in watch mode...${NC}"
    echo -e "${YELLOW}Press 'q' to quit watch mode${NC}\n"
    dotnet watch test
}

# Parse command line arguments
COMMAND=${1:-all}

case $COMMAND in
    all)
        run_all_tests
        ;;
    unit)
        run_unit_tests
        ;;
    integration)
        run_integration_tests
        ;;
    coverage)
        run_with_coverage
        ;;
    watch)
        run_watch_mode
        ;;
    help)
        show_help
        ;;
    *)
        echo -e "${RED}Unknown option: $COMMAND${NC}\n"
        show_help
        exit 1
        ;;
esac

# Show summary
echo -e "\n${BLUE}================================${NC}"
echo -e "${GREEN}Tests execution completed!${NC}"
echo -e "${BLUE}================================${NC}"

echo -e "\n${YELLOW}Tip: Run './run-auth-tests.sh help' to see all options${NC}"

