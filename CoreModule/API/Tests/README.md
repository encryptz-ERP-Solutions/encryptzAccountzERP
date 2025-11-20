# Test Projects

This directory contains all test projects for the EncryptzERP Core API.

## Test Projects

### AuthTests
Comprehensive unit and integration tests for the authentication system.

**Location**: `AuthTests/`

**Coverage**:
- User registration
- Login authentication
- Token refresh with rotation
- Logout and token revocation
- Protected endpoint access
- Password validation
- Negative test cases

**Run Tests**:
```bash
cd AuthTests
dotnet test
```

See [Testing Auth Documentation](../../docs/testing-auth.md) for detailed instructions.

### BusinessLogic.Tests
Unit tests for business logic services (placeholder).

**Location**: `BusinessLogic.Tests/`

**Note**: This project contains initial tests for AuthService. More comprehensive tests are in the AuthTests project.

## Test Structure

```
Tests/
├── AuthTests/                          # Main authentication test suite
│   ├── AuthTests.csproj               # Test project configuration
│   ├── AuthIntegrationTests.cs        # Integration tests
│   ├── AuthServiceUnitTests.cs        # Unit tests
│   ├── appsettings.Test.json          # Test configuration
│   └── README.md                      # AuthTests documentation
├── BusinessLogic.Tests/               # Business logic unit tests
│   └── AuthServiceTests.cs           # Initial auth service tests
└── README.md                          # This file
```

## Running All Tests

From the `Tests/` directory:
```bash
# Run all test projects
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Test Configuration

Each test project can have its own `appsettings.Test.json` for test-specific configuration:
- Database connection strings
- JWT settings
- Logging configuration
- Feature flags

## Best Practices

1. **Isolation**: Tests should not depend on each other
2. **Deterministic**: Same input should always produce same output
3. **Fast**: Unit tests should complete quickly
4. **Descriptive**: Test names should clearly indicate what is being tested
5. **Arrange-Act-Assert**: Follow AAA pattern for test structure

## Continuous Integration

Tests are designed to run in CI/CD pipelines:
- No external dependencies (except database)
- Deterministic test data generation
- Clear pass/fail criteria
- Detailed error reporting

## Adding New Tests

When adding new test projects:
1. Create project using `dotnet new xunit`
2. Add to solution: `dotnet sln add <project-path>`
3. Reference required projects
4. Add test configuration file
5. Update this README

## Resources

- [Testing Auth Documentation](../../docs/testing-auth.md)
- [Authentication Implementation](../../docs/auth-implementation-summary.md)
- [Postman Collection](../../postman/Auth.postman_collection.json)

## Quick Commands

```bash
# Navigate to tests directory
cd /Applications/Work/Encryptz/Github/encryptzAccountzERP/CoreModule/API/Tests

# Run all tests
dotnet test

# Run specific project
cd AuthTests && dotnet test

# Run specific test
dotnet test --filter "Name=RegisterLoginAndAccessProtectedEndpoint_ShouldSucceed"

# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

