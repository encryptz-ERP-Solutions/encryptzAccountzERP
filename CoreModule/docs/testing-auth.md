# Authentication Testing Guide

This document provides comprehensive instructions for testing the authentication system using both automated tests and Postman.

## Table of Contents
- [Overview](#overview)
- [Running Automated Tests](#running-automated-tests)
- [Using Postman Collection](#using-postman-collection)
- [Test Scenarios](#test-scenarios)
- [Troubleshooting](#troubleshooting)

## Overview

The authentication system includes:
- **User Registration**: Create new user accounts with validation
- **Login**: Authenticate with email/username and password
- **JWT Access Tokens**: Short-lived tokens for API access
- **Refresh Tokens**: Long-lived tokens stored in HTTP-only cookies
- **Token Rotation**: Automatic refresh token rotation for security
- **Protected Endpoints**: Endpoints requiring authentication
- **Logout**: Revoke all user tokens

## Running Automated Tests

### Prerequisites
1. .NET 8.0 SDK installed
2. PostgreSQL database running
3. Database connection configured in `appsettings.json`

### Running All Tests

Navigate to the test project directory:
```bash
cd /Applications/Work/Encryptz/Github/encryptzAccountzERP/CoreModule/API/Tests/AuthTests
```

Run all tests:
```bash
dotnet test
```

### Running Specific Test Categories

**Unit Tests Only** (no database required):
```bash
dotnet test --filter "FullyQualifiedName~AuthServiceUnitTests"
```

**Integration Tests Only** (requires database):
```bash
dotnet test --filter "FullyQualifiedName~AuthIntegrationTests"
```

### Running with Detailed Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Running with Code Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./coverage/
```

To view coverage report:
```bash
# Install reportgenerator if not already installed
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator -reports:./coverage/coverage.opencover.xml -targetdir:./coverage/report -reporttypes:Html

# Open in browser (macOS)
open ./coverage/report/index.html
```

### Running Specific Tests

Run a single test by name:
```bash
dotnet test --filter "Name=RegisterLoginAndAccessProtectedEndpoint_ShouldSucceed"
```

## Using Postman Collection

### Setup

#### 1. Import Collection and Environment

1. Open Postman
2. Click **Import** button
3. Import the collection:
   - File: `CoreModule/postman/Auth.postman_collection.json`
4. Import the environment:
   - File: `CoreModule/postman/Auth.postman_environment.json`

#### 2. Select Environment

In Postman, select **"EncryptzERP - Local Development"** from the environment dropdown (top right).

#### 3. Configure Base URL

If your API runs on a different port, update the `baseUrl` variable:
- Default: `https://localhost:7001`
- To change: Edit the environment and modify `baseUrl`

### Running the Collection

#### Sequential Execution (Recommended)

Run requests in order for a complete flow:

1. **Register New User**
   - Creates a unique test user
   - Saves access token automatically
   - Sets refresh token cookie

2. **Login**
   - Logs in with the created user
   - Updates access token

3. **Get Current User (Protected)**
   - Uses saved access token
   - Verifies authentication works

4. **Refresh Token**
   - Gets new access token using refresh cookie
   - Verifies token rotation

5. **Logout**
   - Revokes all tokens
   - Clears access token from environment

#### Running Collection Automatically

Use Collection Runner for automated testing:

1. Click **Runner** button in Postman
2. Select **"EncryptzERP - Authentication API"** collection
3. Select **"EncryptzERP - Local Development"** environment
4. Click **Run** to execute all requests in sequence

The collection includes automatic tests that verify:
- Correct HTTP status codes
- Response body structure
- Token presence and validity
- Cookie handling
- Error messages for negative tests

### Individual Request Testing

You can also run requests individually:

**Register**:
- Updates `testUserHandle` and `testEmail` automatically with unique values
- Password: `Test@1234` (default)

**Login**:
- Uses credentials from previous registration
- Can be modified to test existing users

**Protected Endpoints**:
- Automatically uses saved `accessToken` from environment
- Returns 401 if token is missing/invalid

### Negative Test Cases

The collection includes negative tests:

- **Login with Invalid Password**: Returns 401
- **Register with Weak Password**: Returns 400
- **Access Protected Endpoint Without Token**: Returns 401

## Test Scenarios

### Scenario 1: Complete Registration Flow

**Automated Test**: `RegisterLoginAndAccessProtectedEndpoint_ShouldSucceed`

**Postman Flow**:
1. Run "Register New User"
2. Run "Get Current User (Protected)"
3. Verify user data matches registration

**Expected Behavior**:
- Registration returns 200 with access token
- Protected endpoint returns user information
- User can access their data immediately after registration

### Scenario 2: Token Refresh with Rotation

**Automated Test**: `LoginAndRefresh_OldRefreshTokenShouldBeRevoked`

**Postman Flow**:
1. Run "Register New User"
2. Note the refresh token cookie
3. Run "Refresh Token"
4. New refresh token is different from the old one
5. Attempting to reuse old token fails with 401

**Expected Behavior**:
- Refresh returns new access token and new refresh token
- Old refresh token is automatically revoked
- System prevents replay attacks

### Scenario 3: Failed Login Attempts

**Automated Test**: `Login_WithInvalidPassword_ShouldReturnUnauthorized`

**Postman Flow**:
1. Run "Register New User"
2. Run "Login with Invalid Password (Negative Test)"

**Expected Behavior**:
- Returns 401 Unauthorized
- Error message indicates invalid credentials
- No tokens are issued

### Scenario 4: Password Validation

**Automated Test**: `Register_WithWeakPassword_ShouldReturnBadRequest`

**Postman Flow**:
1. Run "Register with Weak Password (Negative Test)"

**Expected Behavior**:
- Returns 400 Bad Request
- Error message lists password requirements:
  - Minimum 8 characters
  - At least one uppercase letter
  - At least one lowercase letter
  - At least one digit
  - At least one special character

### Scenario 5: Logout and Token Revocation

**Automated Test**: `Logout_ShouldRevokeAllTokens`

**Postman Flow**:
1. Run "Register New User"
2. Run "Logout"
3. Try "Refresh Token" - should fail
4. Try "Get Current User (Protected)" - should fail

**Expected Behavior**:
- Logout returns 200
- All refresh tokens are revoked
- Access to protected endpoints denied after logout

### Scenario 6: Unauthorized Access Prevention

**Automated Test**: `AccessProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized`

**Postman Flow**:
1. Clear `accessToken` from environment
2. Run "Access Protected Endpoint Without Token (Negative Test)"

**Expected Behavior**:
- Returns 401 Unauthorized
- No user data is exposed

## API Endpoints Reference

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/v1/auth/register` | Register new user | No |
| POST | `/api/v1/auth/login` | Login with credentials | No |
| POST | `/api/v1/auth/refresh` | Refresh access token | No (uses cookie) |
| POST | `/api/v1/auth/logout` | Logout and revoke tokens | Yes |
| POST | `/api/v1/auth/revoke` | Revoke specific token | Yes |
| GET | `/api/v1/auth/users/me` | Get current user info | Yes |

### Request/Response Examples

#### Register
**Request**:
```json
{
  "userHandle": "johndoe",
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "SecurePass@123",
  "mobileCountryCode": "+1",
  "mobileNumber": "5551234567"
}
```

**Response** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-11-20T23:00:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userHandle": "johndoe"
}
```

**Cookies Set**:
```
refreshToken=<encrypted-token>; HttpOnly; Secure; SameSite=Strict; Expires=...
```

#### Login
**Request**:
```json
{
  "emailOrUserHandle": "john@example.com",
  "password": "SecurePass@123"
}
```

**Response** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-11-20T23:00:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userHandle": "johndoe"
}
```

#### Get Current User
**Request**:
```
GET /api/v1/auth/users/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response** (200 OK):
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userHandle": "johndoe",
  "email": "john@example.com"
}
```

#### Refresh Token
**Request**:
```
POST /api/v1/auth/refresh
Cookie: refreshToken=<current-refresh-token>
```

**Response** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-11-20T23:00:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userHandle": "johndoe"
}
```

**New Cookie Set**:
```
refreshToken=<new-encrypted-token>; HttpOnly; Secure; SameSite=Strict; Expires=...
```

#### Logout
**Request**:
```
POST /api/v1/auth/logout
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response** (200 OK):
```json
{
  "message": "Logged out successfully."
}
```

## Troubleshooting

### Common Issues

#### 1. Database Connection Errors

**Symptom**: Tests fail with database connection errors

**Solution**:
- Verify PostgreSQL is running: `pg_isready`
- Check connection string in `appsettings.json`
- Ensure database exists and migrations are applied

#### 2. JWT Configuration Missing

**Symptom**: 500 Internal Server Error on authentication endpoints

**Solution**:
- Verify `appsettings.json` contains `JwtSettings` section:
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-at-least-32-characters-long",
    "Issuer": "EncryptzERP",
    "Audience": "EncryptzERP-Users",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

#### 3. Port Already in Use

**Symptom**: Integration tests fail to start API

**Solution**:
- Ensure no other instance of the API is running
- Check if port 7001 (or configured port) is available:
  ```bash
  lsof -i :7001
  ```
- Kill conflicting process or change port in `launchSettings.json`

#### 4. Cookie Not Being Set in Postman

**Symptom**: Refresh token cookie not appearing in Postman

**Solution**:
- Enable cookie capture in Postman:
  - Settings → General → Enable "Automatically follow redirects"
  - Settings → General → Enable "Capture cookies"
- Check that `Secure` flag is appropriate (set to `false` for HTTP in development)
- Verify domain/path in cookie settings match your request

#### 5. Token Expiration

**Symptom**: 401 Unauthorized on previously working requests

**Solution**:
- Access tokens expire after 15 minutes (default)
- Run "Refresh Token" to get a new access token
- Or run "Login" again to get new tokens

#### 6. Test Data Conflicts

**Symptom**: "User already exists" errors in tests

**Solution**:
- Tests generate unique usernames/emails using timestamps
- If tests run too quickly, conflicts may occur
- Clear test data from database:
  ```sql
  DELETE FROM core.users WHERE email LIKE 'test_%@example.com';
  DELETE FROM core.refresh_tokens WHERE user_id NOT IN (SELECT user_id FROM core.users);
  ```

#### 7. Failed Login Count / Account Lockout

**Note**: The current implementation does not track failed login attempts or lock accounts. This is a planned feature.

To implement (future enhancement):
- Add `failed_login_count` and `locked_until` columns to `users` table
- Update `LoginAsync` to increment counter on failure
- Lock account after threshold (e.g., 5 attempts)
- Add test: `Login_MultipleFailedAttempts_ShouldLockAccount`

### Debugging Tests

#### Enable Detailed Logging

Add to `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning",
      "BusinessLogic.Core.Services.Auth": "Trace"
    }
  }
}
```

#### View Database State

During test execution:
```sql
-- View test users
SELECT user_id, user_handle, email, is_active, created_at_utc
FROM core.users
WHERE email LIKE 'test_%@example.com'
ORDER BY created_at_utc DESC;

-- View refresh tokens
SELECT rt.refresh_token_id, rt.user_id, rt.is_revoked, rt.expires_at_utc, u.user_handle
FROM core.refresh_tokens rt
JOIN core.users u ON rt.user_id = u.user_id
WHERE u.email LIKE 'test_%@example.com'
ORDER BY rt.created_at_utc DESC;
```

#### Run Single Test with Debug

```bash
# Set breakpoint in test code, then run with debugger
dotnet test --filter "Name=RegisterLoginAndAccessProtectedEndpoint_ShouldSucceed" --logger "console;verbosity=detailed"
```

## Best Practices

### For Automated Tests
1. **Test Isolation**: Each test creates unique test data
2. **Cleanup**: Tests should be independent and not rely on previous state
3. **Deterministic**: Tests should produce same results on every run
4. **Fast**: Unit tests should complete in milliseconds
5. **Meaningful Names**: Test names describe what they test

### For Manual Testing with Postman
1. **Use Environments**: Different environments for dev/staging/prod
2. **Automated Scripts**: Let pre-request and test scripts handle token management
3. **Sequential Flow**: Run registration → login → protected endpoint in order
4. **Verify Responses**: Check that test scripts pass
5. **Cookie Management**: Ensure cookies are captured for refresh token flow

### Security Testing
1. **Test Invalid Inputs**: Weak passwords, SQL injection attempts
2. **Test Unauthorized Access**: Access protected endpoints without tokens
3. **Test Token Expiry**: Verify expired tokens are rejected
4. **Test Token Revocation**: Verify logout revokes all tokens
5. **Test Rotation**: Verify old refresh tokens cannot be reused

## Additional Resources

- [Authentication Implementation Summary](./auth-implementation-summary.md)
- [Authentication Design](./auth-design.md)
- [Authentication Quick Start](./auth-quickstart.md)
- [Migration Runbook](./migration-runbook.md)

## Commands Cheat Sheet

```bash
# Run all tests
cd CoreModule/API/Tests/AuthTests && dotnet test

# Run unit tests only
dotnet test --filter "FullyQualifiedName~AuthServiceUnitTests"

# Run integration tests only
dotnet test --filter "FullyQualifiedName~AuthIntegrationTests"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test
dotnet test --filter "Name=RegisterLoginAndAccessProtectedEndpoint_ShouldSucceed"

# Detailed output
dotnet test --logger "console;verbosity=detailed"

# Import Postman collection (manual in Postman UI)
# File: CoreModule/postman/Auth.postman_collection.json
# File: CoreModule/postman/Auth.postman_environment.json
```

## Next Steps

1. Run automated tests to verify auth system works
2. Import Postman collection to manually test API
3. Review test results and ensure all scenarios pass
4. Integrate tests into CI/CD pipeline
5. Add additional test scenarios as needed

For questions or issues, refer to the troubleshooting section or contact the development team.

