# Angular Authentication Integration Guide

This document provides comprehensive guidance on integrating the Angular authentication system with your .NET Core backend API.

## Table of Contents
1. [Overview](#overview)
2. [Security Architecture](#security-architecture)
3. [Backend Requirements](#backend-requirements)
4. [Frontend Implementation](#frontend-implementation)
5. [Usage Examples](#usage-examples)
6. [Testing](#testing)
7. [Troubleshooting](#troubleshooting)

## Overview

The authentication system implements industry best practices for token-based authentication:

- **Access Tokens**: Stored in memory (never in localStorage) for security
- **Refresh Tokens**: Stored in httpOnly cookies set by backend
- **Automatic Token Refresh**: Handles 401 errors transparently
- **Route Guards**: Protects routes based on authentication and roles
- **Observable State**: Reactive authentication state management

## Security Architecture

### Token Flow

```
1. Login Request → Backend
2. Backend Returns:
   - Access Token (JWT) in response body
   - Refresh Token in httpOnly cookie (Set-Cookie header)
3. Angular stores Access Token in memory
4. All API requests include Access Token in Authorization header
5. On 401 error:
   - Interceptor pauses outgoing requests
   - Calls refresh endpoint (sends httpOnly cookie automatically)
   - Backend returns new Access Token
   - Retry queued requests with new token
```

### Why httpOnly Cookies for Refresh Tokens?

- **XSS Protection**: JavaScript cannot access httpOnly cookies
- **CSRF Mitigation**: Use SameSite cookie attribute
- **Automatic Handling**: Browser sends cookies automatically
- **Secure Storage**: More secure than localStorage/sessionStorage

## Backend Requirements

### 1. API Endpoints

Your .NET Core backend must implement these endpoints:

#### Login Endpoint
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "loginIdentifier": "user@example.com",
  "password": "password123"
}

Response:
{
  "isSuccess": true,
  "message": "Login successful",
  "response": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "userId": "123",
      "email": "user@example.com",
      "userName": "John Doe",
      "role": "User"
    }
  }
}

Headers:
Set-Cookie: refreshToken=...; HttpOnly; Secure; SameSite=Strict; Path=/; Max-Age=604800
```

#### Refresh Endpoint
```http
POST /api/v1/auth/refresh
Cookie: refreshToken=...

Response:
{
  "isSuccess": true,
  "response": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}

Headers:
Set-Cookie: refreshToken=...; HttpOnly; Secure; SameSite=Strict; Path=/; Max-Age=604800
```

#### Logout Endpoint
```http
POST /api/v1/auth/logout
Cookie: refreshToken=...

Response:
{
  "isSuccess": true,
  "message": "Logout successful"
}

Headers:
Set-Cookie: refreshToken=; HttpOnly; Secure; SameSite=Strict; Path=/; Max-Age=0
```

### 2. .NET Core Implementation Example

#### Startup.cs / Program.cs Configuration

```csharp
// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Angular dev server
                  .AllowCredentials() // CRITICAL: Required for cookies
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Use CORS
app.UseCors("AllowAngularApp");
```

#### AuthController.cs

```csharp
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.AuthenticateAsync(request.LoginIdentifier, request.Password);
        
        if (!result.IsSuccess)
        {
            return Unauthorized(new { isSuccess = false, message = "Invalid credentials" });
        }

        // Set refresh token as httpOnly cookie
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(new
        {
            isSuccess = true,
            message = "Login successful",
            response = new
            {
                token = result.AccessToken,
                user = result.User
            }
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        // Get refresh token from cookie
        var refreshToken = Request.Cookies["refreshToken"];
        
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { isSuccess = false, message = "No refresh token" });
        }

        var result = await _authService.RefreshTokenAsync(refreshToken);
        
        if (!result.IsSuccess)
        {
            return Unauthorized(new { isSuccess = false, message = "Invalid refresh token" });
        }

        // Set new refresh token cookie
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(new
        {
            isSuccess = true,
            response = new
            {
                token = result.AccessToken
            }
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Clear refresh token cookie
        Response.Cookies.Delete("refreshToken");
        
        return Ok(new { isSuccess = true, message = "Logout successful" });
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Use HTTPS in production
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(7) // 7 days
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
```

### 3. JWT Configuration

Ensure your JWT tokens include necessary claims:

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
        expires: DateTime.UtcNow.AddMinutes(15), // Short-lived access token
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### 4. Cookie Configuration Notes

**Development Environment:**
```csharp
Secure = false // Allow cookies over HTTP in development
SameSite = SameSiteMode.Lax // More permissive for development
```

**Production Environment:**
```csharp
Secure = true // Require HTTPS
SameSite = SameSiteMode.Strict // Maximum protection
Domain = ".yourdomain.com" // Set if using subdomains
```

## Frontend Implementation

### File Structure

```
src/app/
├── core/
│   ├── services/
│   │   ├── auth.service.ts
│   │   └── auth.service.spec.ts
│   ├── interceptors/
│   │   ├── auth.interceptor.ts
│   │   └── auth.interceptor.spec.ts
│   └── guards/
│       ├── auth.guard.ts
│       ├── auth.guard.spec.ts
│       ├── role.guard.ts
│       └── role.guard.spec.ts
└── features/
    └── auth/
        └── login/
            ├── login.component.ts
            ├── login.component.html
            └── login.component.scss
```

### Key Components

#### AuthService
- Manages authentication state
- Handles login, logout, refresh operations
- Provides observables for reactive state management
- Decodes JWT tokens to extract user information

#### AuthInterceptor
- Attaches Authorization header to all requests
- Handles 401 errors by refreshing token
- Queues requests during refresh to prevent duplicates
- Automatically retries failed requests after refresh

#### Guards
- **authGuard**: Protects routes from unauthenticated access
- **guestGuard**: Redirects authenticated users away from login
- **roleGuard**: Protects routes based on user roles
- **permissionGuard**: Protects routes based on user permissions

## Usage Examples

### 1. Protecting Routes

```typescript
// app.routes.ts
import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  // Public route (login)
  {
    path: '',
    loadComponent: () => import('./features/auth/login/login.component'),
    canActivate: [guestGuard] // Redirect if already logged in
  },
  
  // Protected routes
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component'),
    canActivate: [authGuard] // Require authentication
  },
  
  // Admin-only routes
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin.module'),
    canActivate: [roleGuard],
    data: { roles: ['Admin', 'SuperAdmin'] }
  },
  
  // Role-based routes
  {
    path: 'manager',
    loadComponent: () => import('./features/manager/manager.component'),
    canActivate: [roleGuard],
    data: { roles: ['Manager', 'Admin'] }
  }
];
```

### 2. Using AuthService in Components

```typescript
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit {
  user$ = this.authService.currentUser$;
  isLoggedIn$ = this.authService.isLoggedIn$;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    // Subscribe to user changes
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        console.log('Current user:', user);
      }
    });
  }

  logout() {
    this.authService.logout().subscribe(() => {
      this.router.navigate(['/']);
    });
  }
}
```

### 3. Template Usage

```html
<!-- profile.component.html -->
<div *ngIf="isLoggedIn$ | async">
  <div *ngIf="user$ | async as user">
    <h2>Welcome, {{ user.userName }}!</h2>
    <p>Email: {{ user.email }}</p>
    <p>Role: {{ user.role }}</p>
    <button (click)="logout()">Logout</button>
  </div>
</div>
```

### 4. Conditional Content Based on Roles

```typescript
import { Component } from '@angular/core';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html'
})
export class NavigationComponent {
  constructor(public authService: AuthService) {}

  get isAdmin(): boolean {
    return this.authService.hasRole('Admin');
  }

  get canManageUsers(): boolean {
    return this.authService.hasPermission('user.manage');
  }
}
```

```html
<!-- navigation.component.html -->
<nav>
  <a routerLink="/dashboard">Dashboard</a>
  <a *ngIf="isAdmin" routerLink="/admin">Admin Panel</a>
  <a *ngIf="canManageUsers" routerLink="/users">Manage Users</a>
</nav>
```

## Testing

### Running Unit Tests

```bash
# Run all tests
ng test

# Run tests for specific component
ng test --include='**/auth.service.spec.ts'

# Run tests with code coverage
ng test --code-coverage
```

### Manual Testing Checklist

- [ ] Login with valid credentials
- [ ] Login with invalid credentials
- [ ] Access protected route while logged out (should redirect to login)
- [ ] Access protected route while logged in (should allow)
- [ ] Logout clears session
- [ ] Token refresh on 401 error
- [ ] Multiple concurrent requests during token refresh
- [ ] Page refresh maintains session (if refresh token valid)
- [ ] Access admin route without admin role (should deny)

## Troubleshooting

### Cookie Not Being Sent

**Problem**: Refresh token cookie is not sent with requests

**Solutions**:
1. Ensure `withCredentials: true` in Angular HttpClient calls
2. Check CORS configuration allows credentials
3. Verify cookie domain matches request domain
4. Check SameSite attribute in development vs production

```typescript
// Angular - Ensure withCredentials is set
this.http.post(url, data, { withCredentials: true })

// .NET Core - Ensure CORS allows credentials
policy.WithOrigins("http://localhost:4200")
      .AllowCredentials()
```

### CORS Errors

**Problem**: Cross-Origin Resource Sharing errors

**Solution**:
```csharp
// .NET Core - Allow specific origins with credentials
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowCredentials()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
```

### Token Not Being Decoded

**Problem**: User information not extracted from token

**Solution**: Verify JWT claims match the decoding logic in `auth.service.ts`:

```typescript
// Check your backend JWT claims match these property names
{
  sub: user.userId,
  email: user.email,
  name: user.userName,
  role: user.role
}
```

### 401 Loop (Infinite Refresh Attempts)

**Problem**: App continuously tries to refresh token

**Solution**: 
1. Check refresh endpoint returns proper response format
2. Verify refresh token in cookie is valid
3. Add logging to identify failure point
4. Ensure interceptor properly handles refresh failures

### Session Lost on Page Refresh

**Problem**: User is logged out when page refreshes

**Causes**:
1. Refresh token cookie not persisted (check cookie expiry)
2. Refresh endpoint failing (check backend logs)
3. Cookie not being sent (check SameSite, Secure flags)

**Solution**: Verify backend sets persistent refresh token cookie with appropriate expiry

## Best Practices

### Security

1. **Always use HTTPS in production** - Required for Secure cookies
2. **Set short expiry for access tokens** (15 minutes recommended)
3. **Set longer expiry for refresh tokens** (7 days recommended)
4. **Implement token rotation** - Issue new refresh token on each refresh
5. **Invalidate refresh tokens on logout** - Clear from both client and server
6. **Monitor for suspicious activity** - Log refresh attempts, multiple failures

### Performance

1. **Cache user information** - Store decoded JWT in service
2. **Batch API calls** - Reduce overhead of token attachment
3. **Implement request queuing** - Prevent duplicate refresh calls

### User Experience

1. **Show loading states** during authentication
2. **Preserve intended destination** - Use returnUrl parameter
3. **Clear error messages** - Guide users on auth failures
4. **Graceful session expiry** - Notify users before forced logout

## Alternative Approach (If httpOnly Cookies Not Possible)

If your backend cannot support httpOnly cookies, you can modify the implementation:

### Caveat

⚠️ **Security Warning**: Storing refresh tokens in localStorage is less secure than httpOnly cookies.

### Modified Implementation

```typescript
// In auth.service.ts
private setSession(token: string, refreshToken: string): void {
  this.accessToken = token; // Keep in memory
  // WARNING: Less secure, vulnerable to XSS attacks
  localStorage.setItem('refresh_token', refreshToken);
}

refresh(): Observable<RefreshResponse> {
  const refreshToken = localStorage.getItem('refresh_token');
  
  return this.http.post<RefreshResponse>(
    `${environment.apiUrl}api/v1/auth/refresh`,
    { refreshToken } // Send in body instead of cookie
  );
}
```

### Additional Security Measures

If using localStorage:
1. Implement Content Security Policy (CSP)
2. Sanitize all user input rigorously
3. Validate all external scripts
4. Use SubResource Integrity (SRI) for CDN resources
5. Regular security audits

## Support

For additional help:
- Check the test files for usage examples
- Review the inline code comments
- Consult Angular and .NET Core documentation

## Conclusion

This authentication system provides a robust, secure foundation for your Angular application. The combination of in-memory access tokens and httpOnly refresh token cookies follows industry best practices and protects against common security vulnerabilities.
