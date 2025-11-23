# AuthTests Setup Guide

## First-Time Setup

Before running the tests, you need to restore NuGet packages and build the project.

### Step 1: Restore Dependencies

Navigate to the test project directory and restore packages:

```bash
cd /Applications/Work/Encryptz/Github/encryptzAccountzERP/CoreModule/API/Tests/AuthTests
dotnet restore
```

This will download all required NuGet packages:
- xUnit test framework
- Moq for mocking
- Microsoft.AspNetCore.Mvc.Testing for integration tests
- Testcontainers.PostgreSql (optional, for containerized tests)
- Code coverage tools

### Step 2: Build the Project

```bash
dotnet build
```

This ensures all project references are resolved and the project compiles successfully.

### Step 3: Configure Test Database (Optional for Integration Tests)

If you want to run integration tests with a real database, update `appsettings.Test.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=encryptzERPCore_Test;Username=postgres;Password=yourpassword"
  }
}
```

**Note**: Integration tests can run against the main API which uses its own configuration. The test database configuration is optional.

### Step 4: Run Tests

Run all tests:
```bash
dotnet test
```

Run unit tests only (no database required):
```bash
dotnet test --filter "FullyQualifiedName~AuthServiceUnitTests"
```

Run integration tests only:
```bash
dotnet test --filter "FullyQualifiedName~AuthIntegrationTests"
```

## Using the Test Runner Script

Alternatively, use the convenience script from the root directory:

```bash
cd /Applications/Work/Encryptz/Github/encryptzAccountzERP/CoreModule
./run-auth-tests.sh all        # Run all tests
./run-auth-tests.sh unit       # Run unit tests only
./run-auth-tests.sh integration # Run integration tests only
./run-auth-tests.sh coverage   # Run with coverage report
./run-auth-tests.sh help       # Show help
```

## Troubleshooting

### Issue: "Assets file not found"
**Solution**: Run `dotnet restore` first

### Issue: "Project reference could not be resolved"
**Solution**: Ensure all referenced projects exist and are built:
```bash
cd ../../Business && dotnet build
cd ../Repository && dotnet build
cd ../Entities && dotnet build
cd ../Infrastructure && dotnet build
```

### Issue: Database connection errors
**Solution**: 
- For unit tests: No database needed, they use mocks
- For integration tests: Ensure API is configured correctly, tests use the running API

### Issue: Port conflicts
**Solution**: Ensure no other instance of the API is running on port 7001

## Project Structure

```
AuthTests/
├── AuthTests.csproj              # Project file with dependencies
├── AuthIntegrationTests.cs       # Integration tests (9 tests)
├── AuthServiceUnitTests.cs       # Unit tests (11 tests)
├── appsettings.Test.json         # Test configuration
├── README.md                     # Test documentation
└── SETUP.md                      # This file
```

## Dependencies

This project depends on:
- BusinessLogic (../../Business/BusinessLogic.csproj)
- Repository (../../Repository/Repository.csproj)
- Entities (../../Entities/Entities.csproj)
- Infrastructure (../../Infrastructure/Infrastructure.csproj)
- encryptzERP (../../encryptzERP/encryptzERP.csproj) - for integration tests

All dependencies are automatically restored with `dotnet restore`.

## Next Steps

After setup is complete:
1. Run tests: `dotnet test`
2. Import Postman collection from `../../../../postman/`
3. Read testing guide: `../../../../docs/testing-auth.md`

## Quick Reference

```bash
# From AuthTests directory:
dotnet restore                    # First time setup
dotnet build                      # Build project
dotnet test                       # Run all tests
dotnet test --logger "console;verbosity=detailed"  # Detailed output

# Or use the script:
cd ../../../..
./run-auth-tests.sh
```

