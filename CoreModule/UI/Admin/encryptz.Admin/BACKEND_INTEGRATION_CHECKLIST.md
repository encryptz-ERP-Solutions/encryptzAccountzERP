# Backend Integration Checklist for Angular Auth

This checklist ensures your .NET Core backend properly integrates with the Angular authentication system.

## ⚠️ Critical Requirements

### 1. CORS Configuration (REQUIRED)

Without this, cookies will not be sent/received:

```csharp
// In Program.cs or Startup.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Your Angular dev URL
              .AllowCredentials()  // ⚠️ CRITICAL: Must have this!
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Apply CORS
app.UseCors("AllowAngularApp");
```

**Production Configuration:**
```csharp
policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
      .AllowCredentials()
      .AllowAnyHeader()
      .AllowAnyMethod();
```

### 2. Cookie Configuration (REQUIRED)

Your auth endpoints must set httpOnly cookies:

```csharp
private void SetRefreshTokenCookie(string refreshToken)
{
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,      // ⚠️ CRITICAL: Prevents JavaScript access
        Secure = true,        // ⚠️ REQUIRED in production (HTTPS only)
        SameSite = SameSiteMode.Strict,  // CSRF protection
        Path = "/",
        Expires = DateTimeOffset.UtcNow.AddDays(7)
    };

    Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
}
```

**Development Environment:**
```csharp
Secure = false,  // Allow HTTP in development
SameSite = SameSiteMode.Lax  // More permissive for development
```

## 📋 Endpoint Requirements

### Endpoint 1: Login
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // 1. Validate credentials
    var user = await _authService.ValidateUser(request.LoginIdentifier, request.Password);
    if (user == null)
    {
        return Unauthorized(new { isSuccess = false, message = "Invalid credentials" });
    }

    // 2. Generate tokens
    var accessToken = GenerateAccessToken(user);
    var refreshToken = GenerateRefreshToken(user);

    // 3. Store refresh token in database (for validation later)
    await _authService.SaveRefreshToken(user.UserId, refreshToken);

    // 4. Set httpOnly cookie
    SetRefreshTokenCookie(refreshToken);

    // 5. Return response
    return Ok(new
    {
        isSuccess = true,
        message = "Login successful",
        response = new
        {
            token = accessToken,
            user = new
            {
                userId = user.UserId,
                email = user.Email,
                userName = user.UserName,
                role = user.Role
            }
        }
    });
}
```

### Endpoint 2: Refresh Token
```csharp
[HttpPost("refresh")]
public async Task<IActionResult> Refresh()
{
    // 1. Get refresh token from cookie
    var refreshToken = Request.Cookies["refreshToken"];
    
    if (string.IsNullOrEmpty(refreshToken))
    {
        return Unauthorized(new { isSuccess = false, message = "No refresh token" });
    }

    // 2. Validate refresh token
    var userId = await _authService.ValidateRefreshToken(refreshToken);
    if (userId == null)
    {
        return Unauthorized(new { isSuccess = false, message = "Invalid refresh token" });
    }

    // 3. Get user
    var user = await _userService.GetUserById(userId.Value);
    if (user == null)
    {
        return Unauthorized(new { isSuccess = false, message = "User not found" });
    }

    // 4. Generate new tokens
    var newAccessToken = GenerateAccessToken(user);
    var newRefreshToken = GenerateRefreshToken(user);

    // 5. Update refresh token in database
    await _authService.UpdateRefreshToken(userId.Value, newRefreshToken);

    // 6. Set new httpOnly cookie
    SetRefreshTokenCookie(newRefreshToken);

    // 7. Return new access token
    return Ok(new
    {
        isSuccess = true,
        response = new
        {
            token = newAccessToken
        }
    });
}
```

### Endpoint 3: Logout
```csharp
[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    // 1. Get refresh token from cookie
    var refreshToken = Request.Cookies["refreshToken"];
    
    // 2. Invalidate refresh token in database
    if (!string.IsNullOrEmpty(refreshToken))
    {
        await _authService.RevokeRefreshToken(refreshToken);
    }

    // 3. Clear cookie
    Response.Cookies.Delete("refreshToken");

    // 4. Return success
    return Ok(new { isSuccess = true, message = "Logout successful" });
}
```

## 🔐 JWT Token Generation

### Access Token (Short-lived: 15 minutes)

```csharp
private string GenerateAccessToken(User user)
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("businessId", user.BusinessId?.ToString() ?? ""),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(15), // Short-lived
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Refresh Token (Long-lived: 7 days)

```csharp
private string GenerateRefreshToken(User user)
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("type", "refresh")
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:RefreshKey"]));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddDays(7), // Long-lived
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

## 💾 Database Schema for Refresh Tokens

```sql
CREATE TABLE RefreshTokens (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Token NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NOT NULL,
    RevokedAt DATETIME2 NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
```

## 🧪 Testing Your Backend

### Test 1: Login
```bash
curl -X POST http://localhost:5286/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"loginIdentifier":"user@example.com","password":"password"}' \
  -c cookies.txt -v
```

**Expected Response:**
- Status: 200 OK
- Body: `{ "isSuccess": true, "response": { "token": "..." } }`
- Cookie: `Set-Cookie: refreshToken=...; HttpOnly; Secure; SameSite=Strict`

### Test 2: Refresh
```bash
curl -X POST http://localhost:5286/api/v1/auth/refresh \
  -b cookies.txt \
  -v
```

**Expected Response:**
- Status: 200 OK
- Body: `{ "isSuccess": true, "response": { "token": "..." } }`
- Cookie: `Set-Cookie: refreshToken=...; HttpOnly; Secure; SameSite=Strict`

### Test 3: Logout
```bash
curl -X POST http://localhost:5286/api/v1/auth/logout \
  -b cookies.txt \
  -v
```

**Expected Response:**
- Status: 200 OK
- Body: `{ "isSuccess": true, "message": "Logout successful" }`
- Cookie: `Set-Cookie: refreshToken=; Max-Age=0`

## ✅ Verification Checklist

### Configuration
- [ ] CORS enabled with `AllowCredentials()`
- [ ] CORS origins include your Angular app URL
- [ ] Cookie options set `HttpOnly = true`
- [ ] Cookie `Secure = true` in production
- [ ] Cookie `SameSite = Strict` or `Lax`

### Endpoints
- [ ] POST /api/v1/auth/login implemented
- [ ] POST /api/v1/auth/refresh implemented
- [ ] POST /api/v1/auth/logout implemented
- [ ] All endpoints return correct response format
- [ ] All endpoints set/clear cookies correctly

### JWT Configuration
- [ ] Access token includes required claims (sub, email, name, role)
- [ ] Access token expires in 15 minutes
- [ ] Refresh token stored in database
- [ ] Refresh token expires in 7 days
- [ ] Token validation logic implemented

### Database
- [ ] RefreshTokens table created
- [ ] Save refresh token on login
- [ ] Validate refresh token on refresh
- [ ] Revoke refresh token on logout
- [ ] Clean up expired tokens (scheduled job)

### Security
- [ ] Passwords hashed (never stored plain text)
- [ ] Refresh tokens hashed in database
- [ ] Rate limiting on auth endpoints
- [ ] Failed login attempts logged
- [ ] Account lockout after failed attempts
- [ ] HTTPS enforced in production

## 🐛 Common Backend Issues

### Issue 1: Cookie Not Received by Angular

**Symptoms:**
- Login works but refresh fails with "No refresh token"
- Cookie visible in Network tab but not sent on subsequent requests

**Fixes:**
1. Check CORS has `AllowCredentials()`
2. Verify cookie domain matches request domain
3. Check SameSite attribute (use Lax in development)
4. Ensure Angular sends `withCredentials: true`

### Issue 2: CORS Errors

**Symptoms:**
- "CORS policy: No 'Access-Control-Allow-Origin' header"
- "Credential is not supported if the CORS header 'Access-Control-Allow-Origin' is '*'"

**Fixes:**
1. Don't use wildcard (*) with credentials
2. Specify exact origins in CORS policy
3. Apply CORS before authentication middleware
4. Check CORS is enabled in production environment

### Issue 3: 401 on Refresh

**Symptoms:**
- Login works but refresh always returns 401

**Fixes:**
1. Check cookie is being received (log Request.Cookies)
2. Verify refresh token validation logic
3. Check refresh token hasn't expired
4. Ensure refresh token exists in database
5. Verify token signature with correct secret key

## 📝 Implementation Order

1. **Setup CORS** - Critical first step
2. **Create database table** - For refresh tokens
3. **Implement token generation** - JWT creation
4. **Implement login endpoint** - Test basic flow
5. **Implement refresh endpoint** - Test token refresh
6. **Implement logout endpoint** - Test cleanup
7. **Test with Postman** - Before Angular integration
8. **Test with Angular** - Full integration test

## 🎯 Success Criteria

Your backend is ready when:
- [ ] Can login and receive access token + cookie
- [ ] Can refresh token using only the cookie
- [ ] Can logout and cookie is cleared
- [ ] CORS allows Angular app with credentials
- [ ] Cookies have correct security flags
- [ ] All responses match expected format
- [ ] Token expiry times are appropriate

## 📞 Need Help?

Check these resources:
1. `ANGULAR_AUTH_INTEGRATION.md` - Full .NET Core examples
2. Microsoft JWT documentation
3. ASP.NET Core CORS documentation
4. Cookie security best practices

## 🚀 Deploy to Production

Before deploying:
- [ ] Change `Secure = true` in cookie options
- [ ] Use strong JWT secret keys
- [ ] Configure production CORS origins
- [ ] Enable HTTPS
- [ ] Set up SSL certificate
- [ ] Test with production URLs
- [ ] Monitor auth endpoint logs
- [ ] Set up token cleanup job
