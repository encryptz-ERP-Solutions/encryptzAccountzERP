# Authentication Tests

This test project contains comprehensive unit and integration tests for the authentication system.

## Test Categories

### Unit Tests (`AuthServiceUnitTests.cs`)
Tests for the `AuthService` class with mocked dependencies:
- User registration validation
- Login credential verification
- Password strength validation
- Token generation
- User claims retrieval
- Logout functionality

### Integration Tests (`AuthIntegrationTests.cs`)
End-to-end tests using the actual API:
- Complete registration → login → protected endpoint flow
- Token refresh with rotation (old token revoked)
- Invalid password handling
- Weak password rejection
- Duplicate email prevention
- Logout token revocation
- Unauthorized access prevention

## Running the Tests

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL database (for integration tests)

### Run All Tests
```bash
cd /Applications/Work/Encryptz/Github/encryptzAccountzERP/CoreModule/API/Tests/AuthTests
dotnet test
```

### Run with Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Specific Test Category
```bash
# Unit tests only
dotnet test --filter "FullyQualifiedName~AuthServiceUnitTests"

# Integration tests only
dotnet test --filter "FullyQualifiedName~AuthIntegrationTests"
```

### Run with Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Test Configuration

Integration tests use the application's configuration from `appsettings.json`. Ensure your database connection string is configured correctly.

## Test Data

Integration tests create unique test users for each test run using GUIDs to avoid conflicts. All test data is isolated and deterministic.

## Troubleshooting

### Database Connection Issues
Ensure PostgreSQL is running and the connection string in `appsettings.json` is correct.

### Port Conflicts
If you encounter port conflicts during integration tests, ensure no other instance of the API is running.

### Test Failures
Check the detailed test output for specific error messages. Common issues:
- Database not seeded with required data (roles, permissions)
- JWT configuration missing in appsettings
- Database connection string incorrect

