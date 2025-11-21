# Angular Authentication - Quick Start Guide

This is a quick reference for the Angular authentication system implementation.

## 🚀 Quick Setup Checklist

### 1. Files Created

- ✅ `src/app/core/services/auth.service.ts` - Main authentication service
- ✅ `src/app/core/services/auth.service.spec.ts` - Unit tests
- ✅ `src/app/core/interceptors/auth.interceptor.ts` - HTTP interceptor with 401 handling
- ✅ `src/app/core/interceptors/auth.interceptor.spec.ts` - Unit tests
- ✅ `src/app/core/guards/auth.guard.ts` - Route guards (auth, guest)
- ✅ `src/app/core/guards/auth.guard.spec.ts` - Unit tests
- ✅ `src/app/core/guards/role.guard.ts` - Role/permission guards
- ✅ `src/app/core/guards/role.guard.spec.ts` - Unit tests
- ✅ Updated `src/app/features/auth/login/login.component.ts`

### 2. Backend Requirements

Your .NET Core API must implement:

```
POST /api/v1/auth/login    - Login endpoint
POST /api/v1/auth/refresh  - Refresh token endpoint
POST /api/v1/auth/logout   - Logout endpoint
```

**Critical**: Backend must set httpOnly cookie with refresh token:

```csharp
Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    Path = "/",
    Expires = DateTimeOffset.UtcNow.AddDays(7)
});
```

**Critical**: CORS must allow credentials:

```csharp
policy.WithOrigins("http://localhost:4200")
      .AllowCredentials()
      .AllowAnyHeader()
      .AllowAnyMethod();
```

## 📝 Basic Usage

### Login

```typescript
constructor(private authService: AuthService) {}

login() {
    this.authService.login({ 
        loginIdentifier: 'user@example.com', 
        password: 'password' 
    }).subscribe({
        next: (response) => {
            if (response.isSuccess) {
                this.router.navigate(['/dashboard']);
            }
        },
        error: (error) => {
            console.error('Login failed:', error);
        }
    });
}
```

### Logout

```typescript
logout() {
    this.authService.logout().subscribe(() => {
        this.router.navigate(['/']);
    });
}
```

### Check Authentication

```typescript
// In component
isLoggedIn$ = this.authService.isLoggedIn$;
currentUser$ = this.authService.currentUser$;

// In template
<div *ngIf="isLoggedIn$ | async">
    <p>Welcome, {{ (currentUser$ | async)?.userName }}</p>
</div>
```

### Protect Routes

```typescript
// app.routes.ts
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
    // Public
    { path: '', component: LoginComponent },
    
    // Protected
    { 
        path: 'dashboard', 
        component: DashboardComponent,
        canActivate: [authGuard] 
    },
    
    // Admin only
    { 
        path: 'admin', 
        component: AdminComponent,
        canActivate: [roleGuard],
        data: { roles: ['Admin'] }
    }
];
```

## 🔒 Security Features

### ✅ What's Implemented

- **In-Memory Access Token** - Not stored in localStorage (XSS protection)
- **httpOnly Refresh Token** - Stored in secure cookie by backend
- **Automatic Token Refresh** - On 401 errors
- **Request Queuing** - Prevents multiple refresh calls
- **Route Guards** - Authentication and role-based protection
- **Observable State** - Reactive authentication status

### ⚠️ What Backend Must Do

1. Set httpOnly cookie for refresh token
2. Enable CORS with credentials
3. Return proper JWT in login response
4. Validate refresh token from cookie
5. Clear cookie on logout

## 🧪 Testing

Run tests:
```bash
ng test
```

Test specific file:
```bash
ng test --include='**/auth.service.spec.ts'
```

## 📚 Documentation

- **ANGULAR_AUTH_INTEGRATION.md** - Complete integration guide with backend examples
- **AUTH_ROUTES_EXAMPLE.md** - Route configuration examples
- **AUTH_QUICK_START.md** - This file

## 🐛 Common Issues

### Cookie Not Being Sent

**Problem**: Backend doesn't receive refresh token cookie

**Fix**:
1. Check `withCredentials: true` in AuthService
2. Verify CORS allows credentials in backend
3. Check cookie SameSite attribute

### CORS Errors

**Problem**: Cross-origin errors

**Fix**:
```csharp
// In .NET Core Startup/Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowCredentials() // MUST have this
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

### Token Not Decoded

**Problem**: User info not extracted from JWT

**Fix**: Verify JWT claims in backend match AuthService decoding:
```typescript
{
  sub: userId,        // or 'userId' or 'id'
  email: email,
  name: userName,     // or 'userName' or 'unique_name'
  role: role,         // or 'http://schemas.microsoft.com/...'
  exp: expiry
}
```

### 401 Loop

**Problem**: Infinite refresh attempts

**Fix**:
1. Check refresh endpoint returns correct format
2. Verify refresh token validity in backend logs
3. Ensure interceptor catches refresh failures

## 🎯 Next Steps

1. **Update Backend** - Implement the three auth endpoints
2. **Configure CORS** - Enable credentials in backend
3. **Test Login Flow** - Verify tokens are set correctly
4. **Test Token Refresh** - Wait for access token to expire
5. **Test Route Guards** - Try accessing protected routes
6. **Add Error Handling** - Implement user-friendly error messages

## 🔑 Key Files to Review

1. `auth.service.ts` - Main service logic
2. `auth.interceptor.ts` - 401 handling and refresh
3. `auth.guard.ts` - Route protection
4. `login.component.ts` - Login implementation

## 💡 Tips

- Access tokens are short-lived (15 min recommended)
- Refresh tokens are long-lived (7 days recommended)
- Always use HTTPS in production
- Monitor auth failures in backend logs
- Implement rate limiting on auth endpoints
- Consider implementing token rotation

## 📞 Need Help?

Refer to the comprehensive documentation:
- `ANGULAR_AUTH_INTEGRATION.md` - Full implementation guide
- Test files (*.spec.ts) - Usage examples
- Inline code comments - Detailed explanations
