# Authentication Design Documentation

## Overview

This document describes the robust authentication system implemented for the EncryptzERP Core API. The system uses JWT access tokens combined with opaque refresh tokens stored securely in the database with token rotation support.

## Architecture

### Components

1. **AuthService** (`BusinessLogic.Core.Services.Auth.AuthService`)
   - Core business logic for authentication operations
   - Password validation and hashing
   - Token generation and validation
   - User registration and login

2. **AuthController** (`encryptzERP.Controllers.Core.AuthController`)
   - RESTful API endpoints for authentication
   - HTTP-only secure cookie management
   - IP address tracking for security

3. **RefreshTokenRepository** (`Repository.Core.RefreshTokenRepository`)
   - Database operations for refresh token storage
   - Token rotation and revocation
   - Cleanup of expired tokens

4. **PasswordHasher** (`BusinessLogic.Admin.Services.PasswordHasher`)
   - Uses ASP.NET Core Identity's `PasswordHasher<T>`
   - Implements PBKDF2 with HMAC-SHA256
   - 128-bit salt, 256-bit subkey, 10,000 iterations

## Authentication Flow

### 1. Registration Flow

```
Client                          API                           Database
  |                              |                                |
  |---POST /api/v1/auth/register--->                              |
  |     (RegisterRequestDto)     |                                |
  |                              |---Validate Password----------->|
  |                              |---Check User Exists---------->|
  |                              |<--User Not Found--------------|
  |                              |---Hash Password--------------->|
  |                              |---Create User----------------->|
  |                              |<--User Created-----------------|
  |                              |---Generate Access Token------->|
  |                              |---Generate Refresh Token------>|
  |                              |---Hash Refresh Token---------->|
  |                              |---Store Token Hash------------>|
  |                              |<--Token Stored-----------------|
  |<--AuthResponseDto + Cookie---|                                |
```

**Steps:**
1. Client sends registration data (username, email, password)
2. API validates password against security requirements
3. API checks if user already exists
4. Password is hashed using PBKDF2
5. User is created in database
6. JWT access token is generated with user claims
7. Opaque refresh token is generated (cryptographically random)
8. Refresh token is hashed using SHA256
9. Token hash is stored in database with metadata (IP, expiry)
10. Access token returned in response body
11. Refresh token set in HTTP-only, Secure, SameSite=Strict cookie

### 2. Login Flow

```
Client                          API                           Database
  |                              |                                |
  |---POST /api/v1/auth/login--->|                                |
  |     (LoginRequestDto)        |                                |
  |                              |---Find User by Email/Handle--->|
  |                              |<--User Found-------------------|
  |                              |---Verify Password------------->|
  |                              |---Check IsActive-------------->|
  |                              |---Generate Tokens------------->|
  |                              |---Store Token Hash------------>|
  |<--AuthResponseDto + Cookie---|                                |
```

**Steps:**
1. Client sends email/username and password
2. API finds user by email or username
3. Password is verified using PasswordHasher
4. User active status is checked
5. New JWT access token is generated
6. New refresh token is generated and hashed
7. Token hash is stored with IP and timestamp
8. Tokens returned (access token in body, refresh in cookie)

### 3. Token Refresh Flow (with Rotation)

```
Client                          API                           Database
  |                              |                                |
  |---POST /api/v1/auth/refresh->|                                |
  |     (Cookie: refreshToken)   |                                |
  |                              |---Hash Incoming Token--------->|
  |                              |---Lookup Token Hash----------->|
  |                              |<--Token Found------------------|
  |                              |---Validate Token (Active?)---->|
  |                              |---Get User-------------------->|
  |                              |<--User Found-------------------|
  |                              |---Generate New Tokens--------->|
  |                              |---Store New Token Hash-------->|
  |                              |---Revoke Old Token------------>|
  |                              |---Set ReplacedBy Link--------->|
  |<--New Tokens + Cookie--------|                                |
```

**Token Rotation Steps:**
1. Client sends refresh token (from cookie or body)
2. API hashes the incoming token
3. Token hash is looked up in database
4. Token is validated (not expired, not revoked)
5. User is retrieved and validated
6. New access token and refresh token are generated
7. New refresh token hash is stored
8. Old token is marked as revoked
9. Old token's `replaced_by_token_id` points to new token
10. New tokens returned to client

**Why Token Rotation?**
- Limits the window of opportunity for token theft
- Provides audit trail of token usage
- Enables detection of token reuse attacks
- Follows OAuth 2.0 best practices

### 4. Logout Flow

```
Client                          API                           Database
  |                              |                                |
  |---POST /api/v1/auth/logout-->|                                |
  |     (Authorization: Bearer)  |                                |
  |                              |---Get UserID from JWT--------->|
  |                              |---Revoke All User Tokens------>|
  |                              |<--Tokens Revoked---------------|
  |<--Success + Clear Cookie-----|                                |
```

**Steps:**
1. Client sends authenticated request with access token
2. API extracts user ID from JWT claims
3. All user's refresh tokens are marked as revoked
4. Refresh token cookie is cleared
5. Success response returned

## Security Features

### 1. Password Security
- **Minimum Requirements:**
  - At least 8 characters
  - At least one uppercase letter
  - At least one lowercase letter
  - At least one digit
  - At least one special character
- **Hashing:** PBKDF2 with HMAC-SHA256 (ASP.NET Core Identity)
- **Never stored in plaintext**

### 2. Refresh Token Security
- **Opaque Tokens:** Cryptographically random, not predictable
- **Hashed Storage:** SHA256 hash stored in database, not plaintext
- **Token Rotation:** New token issued on each refresh, old token revoked
- **HTTP-only Cookies:** Prevents JavaScript access (XSS protection)
- **Secure Flag:** Only transmitted over HTTPS
- **SameSite=Strict:** CSRF protection
- **IP Tracking:** Created and revoked IPs logged for audit
- **Expiration:** Tokens have definite expiry (default 7 days)

### 3. Access Token Security
- **Short-lived:** 15 minutes default expiration
- **Signed:** HMAC-SHA256 signature prevents tampering
- **Stateless:** No database lookup required for validation
- **Claims-based:** Contains user identity and permissions

### 4. Additional Security Measures
- **Token Reuse Detection:** Revoked tokens cannot be reused
- **Audit Trail:** All token operations logged with IP and timestamp
- **Automatic Cleanup:** Expired tokens can be automatically deleted
- **User Account Status:** Inactive users cannot authenticate

## Database Schema

### refresh_tokens Table

```sql
CREATE TABLE core.refresh_tokens (
    refresh_token_id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    token_hash VARCHAR(255) NOT NULL UNIQUE,
    expires_at_utc TIMESTAMP NOT NULL,
    created_at_utc TIMESTAMP NOT NULL,
    is_revoked BOOLEAN NOT NULL DEFAULT FALSE,
    revoked_at_utc TIMESTAMP NULL,
    replaced_by_token_id UUID NULL,
    created_by_ip VARCHAR(45) NULL,
    revoked_by_ip VARCHAR(45) NULL,
    
    FOREIGN KEY (user_id) REFERENCES core.users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (replaced_by_token_id) REFERENCES core.refresh_tokens(refresh_token_id)
);
```

**Columns:**
- `refresh_token_id`: Unique identifier
- `user_id`: Owner of the token
- `token_hash`: SHA256 hash of the refresh token (secure storage)
- `expires_at_utc`: Expiration timestamp
- `created_at_utc`: Creation timestamp
- `is_revoked`: Revocation flag
- `revoked_at_utc`: When token was revoked
- `replaced_by_token_id`: Points to replacement token (rotation chain)
- `created_by_ip`: IP that created the token
- `revoked_by_ip`: IP that revoked the token

**Indexes:**
- Primary key on `refresh_token_id`
- Unique index on `token_hash` (fast lookup)
- Index on `user_id` (user's tokens)
- Index on `expires_at_utc` (cleanup operations)
- Composite index on `(user_id, is_revoked, expires_at_utc)` for active tokens

## API Endpoints

### POST /api/v1/auth/register
Register a new user account.

**Request:**
```json
{
  "userHandle": "johndoe",
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "SecureP@ssw0rd",
  "mobileCountryCode": "+1",
  "mobileNumber": "5551234567"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2025-11-20T12:15:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userHandle": "johndoe"
}
```

**Cookie Set:**
```
Set-Cookie: refreshToken=<token>; HttpOnly; Secure; SameSite=Strict; Path=/api/v1/auth; Expires=...
```

### POST /api/v1/auth/login
Authenticate user with email/username and password.

**Request:**
```json
{
  "emailOrUserHandle": "john@example.com",
  "password": "SecureP@ssw0rd"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2025-11-20T12:15:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userHandle": "johndoe"
}
```

### POST /api/v1/auth/refresh
Refresh access token using refresh token.

**Request (Optional - token can come from cookie):**
```json
{
  "refreshToken": "optional-if-using-cookies"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2025-11-20T12:30:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userHandle": "johndoe"
}
```

### POST /api/v1/auth/logout
Logout and revoke all user's refresh tokens.

**Headers:**
```
Authorization: Bearer <access-token>
```

**Response (200 OK):**
```json
{
  "message": "Logged out successfully."
}
```

### POST /api/v1/auth/revoke
Revoke a specific refresh token.

**Headers:**
```
Authorization: Bearer <access-token>
```

**Request:**
```json
{
  "refreshToken": "token-to-revoke"
}
```

**Response (200 OK):**
```json
{
  "message": "Token revoked successfully."
}
```

## Configuration

### appsettings.json

```json
{
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "yourdomain.com",
    "Audience": "youraudience.com",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

**Configuration Properties:**
- `SecretKey`: Secret key for JWT signing (MUST be at least 32 characters)
- `Issuer`: JWT issuer claim (your domain)
- `Audience`: JWT audience claim (your application)
- `AccessTokenExpirationMinutes`: How long access tokens are valid (default: 15 minutes)
- `RefreshTokenExpirationDays`: How long refresh tokens are valid (default: 7 days)

### Environment Variables (Recommended for Production)

Instead of storing secrets in `appsettings.json`, use environment variables:

```bash
export JwtSettings__SecretKey="your-production-secret-key"
export JwtSettings__Issuer="production.yourdomain.com"
export JwtSettings__Audience="production-api"
```

### Secret Key Generation

Generate a secure secret key using:

**PowerShell:**
```powershell
[Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }))
```

**Bash/Linux:**
```bash
openssl rand -base64 64
```

**C#:**
```csharp
using System.Security.Cryptography;
var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
```

## Dependency Injection

Services registered in `Program.cs`:

```csharp
// Authentication services
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<TokenService>();

// Existing services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<CoreSQLDbHelper>();
```

## Database Migration

Run the migration script to create the refresh_tokens table:

```bash
psql -h your_host -d your_database -U your_user -f migrations/sql/2025_11_20_create_refresh_tokens_table.sql
```

**Or using your migration tool:**
```bash
# Add to your migration process
dotnet ef migrations add AddRefreshTokens
dotnet ef database update
```

## Client-Side Implementation

### Registration/Login
```typescript
async function login(email: string, password: string) {
  const response = await fetch('https://api.yourdomain.com/api/v1/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include', // Important: includes cookies
    body: JSON.stringify({ emailOrUserHandle: email, password })
  });
  
  const data = await response.json();
  // Store access token in memory or state management
  setAccessToken(data.accessToken);
  // Refresh token is automatically stored in HTTP-only cookie
}
```

### API Requests with Access Token
```typescript
async function makeAuthenticatedRequest(url: string) {
  const response = await fetch(url, {
    headers: {
      'Authorization': `Bearer ${getAccessToken()}`
    }
  });
  
  if (response.status === 401) {
    // Access token expired, try to refresh
    await refreshToken();
    // Retry original request
    return makeAuthenticatedRequest(url);
  }
  
  return response.json();
}
```

### Token Refresh
```typescript
async function refreshToken() {
  const response = await fetch('https://api.yourdomain.com/api/v1/auth/refresh', {
    method: 'POST',
    credentials: 'include' // Sends refresh token cookie
  });
  
  if (response.ok) {
    const data = await response.json();
    setAccessToken(data.accessToken);
    return true;
  } else {
    // Refresh failed, redirect to login
    logout();
    return false;
  }
}
```

### Logout
```typescript
async function logout() {
  await fetch('https://api.yourdomain.com/api/v1/auth/logout', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${getAccessToken()}`
    },
    credentials: 'include'
  });
  
  clearAccessToken();
  // Redirect to login page
  window.location.href = '/login';
}
```

## Security Best Practices

### Production Checklist

- [ ] Use strong, randomly generated secret key (64+ characters)
- [ ] Store secrets in environment variables, not code
- [ ] Enable HTTPS only (set `Secure` cookie flag)
- [ ] Set appropriate CORS policies
- [ ] Implement rate limiting on auth endpoints
- [ ] Enable request logging for audit
- [ ] Implement account lockout after failed attempts
- [ ] Add email verification for new registrations
- [ ] Implement password reset functionality
- [ ] Monitor for suspicious token usage patterns
- [ ] Regularly run cleanup job for expired tokens
- [ ] Use connection pooling for database connections
- [ ] Implement IP whitelisting for admin accounts
- [ ] Add multi-factor authentication (MFA) for sensitive operations

### Common Vulnerabilities Addressed

1. **XSS (Cross-Site Scripting)**
   - HTTP-only cookies prevent JavaScript access to refresh tokens
   - Access tokens in memory only (not localStorage)

2. **CSRF (Cross-Site Request Forgery)**
   - SameSite=Strict cookie attribute
   - Short-lived access tokens

3. **Token Theft**
   - Token rotation limits damage from stolen tokens
   - IP tracking enables detection of token abuse
   - Tokens stored as hashes, not plaintext

4. **Brute Force Attacks**
   - Strong password requirements
   - PBKDF2 with 10,000 iterations slows attacks

5. **Man-in-the-Middle**
   - HTTPS required (Secure cookie flag)
   - Signed JWTs prevent tampering

## Maintenance

### Token Cleanup Job

Run periodically to remove expired tokens:

```sql
SELECT core.cleanup_expired_refresh_tokens();
```

**Schedule with cron:**
```bash
# Run daily at 2 AM
0 2 * * * psql -h localhost -d encryptzERPCore -U app_user -c "SELECT core.cleanup_expired_refresh_tokens();"
```

**Or implement in application:**
```csharp
// In a background service
public class TokenCleanupService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _refreshTokenRepository.DeleteExpiredTokensAsync();
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
```

### Monitoring

Monitor these metrics:
- Failed login attempts per user/IP
- Token refresh frequency
- Revoked token usage attempts (indicates token theft)
- Active sessions per user
- Average token lifetime before refresh

## Testing

### Unit Tests

See `BusinessLogic.Tests/AuthServiceTests.cs` for test stubs.

**Run tests:**
```bash
dotnet test API/Tests/BusinessLogic.Tests/
```

### Integration Testing

Use Postman or similar tools to test authentication flow:

1. Register a new user
2. Login with credentials
3. Use access token to call protected endpoint
4. Refresh access token
5. Logout
6. Verify revoked token cannot be used

## Troubleshooting

### Common Issues

**Issue: "Invalid refresh token" on refresh**
- Check cookie is being sent with request
- Verify cookie path matches API path
- Ensure HTTPS in production (Secure flag)

**Issue: "Unauthorized" on protected endpoints**
- Verify access token in Authorization header
- Check token hasn't expired
- Verify JWT secret key matches configuration

**Issue: Token rotation not working**
- Check database constraints on refresh_tokens table
- Verify foreign key for replaced_by_token_id
- Check for database transaction issues

## References

- [RFC 6749 - OAuth 2.0 Authorization Framework](https://tools.ietf.org/html/rfc6749)
- [RFC 7519 - JSON Web Token (JWT)](https://tools.ietf.org/html/rfc7519)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [ASP.NET Core Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)

## Support

For issues or questions:
- Check this documentation first
- Review unit tests for examples
- Check application logs for detailed error messages
- Contact the development team

---

**Version:** 1.0  
**Last Updated:** 2025-11-20  
**Author:** EncryptzERP Development Team

