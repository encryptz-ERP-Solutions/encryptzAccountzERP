# Authentication System - Quick Start Guide

## 🚀 5-Minute Setup

### Step 1: Run Database Migration

```bash
cd CoreModule
psql -h localhost -U postgres -d encryptzERPCore \
  -f migrations/sql/2025_11_20_create_refresh_tokens_table.sql
```

### Step 2: Configure JWT Settings (Development)

The `appsettings.Development.json` is already configured with development settings. For production, update `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YOUR_64_CHAR_SECRET_KEY_HERE",
    "Issuer": "yourdomain.com",
    "Audience": "your-api",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Step 3: Build and Run

```bash
cd API/encryptzERP
dotnet build
dotnet run
```

### Step 4: Test the Endpoints

Navigate to `https://localhost:5286/swagger` and test the authentication endpoints.

## 📝 API Endpoints Overview

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/v1/auth/register` | Register new user | ❌ No |
| POST | `/api/v1/auth/login` | Login with credentials | ❌ No |
| POST | `/api/v1/auth/refresh` | Refresh access token | ❌ No |
| POST | `/api/v1/auth/logout` | Logout (revoke all tokens) | ✅ Yes |
| POST | `/api/v1/auth/revoke` | Revoke specific token | ✅ Yes |

## 💻 Example Usage

### 1. Register a New User

```bash
curl -X POST https://localhost:5286/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userHandle": "johndoe",
    "fullName": "John Doe",
    "email": "john@example.com",
    "password": "SecureP@ssw0rd123"
  }'
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2025-11-20T12:15:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userHandle": "johndoe"
}
```

### 2. Login

```bash
curl -X POST https://localhost:5286/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -c cookies.txt \
  -d '{
    "emailOrUserHandle": "john@example.com",
    "password": "SecureP@ssw0rd123"
  }'
```

### 3. Call Protected Endpoint

```bash
curl -X GET https://localhost:5286/api/users \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### 4. Refresh Token (when access token expires)

```bash
curl -X POST https://localhost:5286/api/v1/auth/refresh \
  -b cookies.txt \
  -c cookies.txt
```

### 5. Logout

```bash
curl -X POST https://localhost:5286/api/v1/auth/logout \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -b cookies.txt
```

## 🔐 Password Requirements

- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character

**Valid Examples:**
- `SecureP@ssw0rd123`
- `MyP@ssw0rd2024`
- `Admin#12345!`

## 🍪 Cookie vs Body-Based Refresh Tokens

### Using Cookies (Recommended)

**More Secure** - HTTP-only, Secure, SameSite=Strict

```javascript
// Frontend - Register/Login
const response = await fetch('/api/v1/auth/login', {
  method: 'POST',
  credentials: 'include', // Important!
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ emailOrUserHandle, password })
});

// Refresh Token
await fetch('/api/v1/auth/refresh', {
  method: 'POST',
  credentials: 'include' // Cookie sent automatically
});
```

### Using Body (Alternative)

```javascript
// Save refresh token from response
const { refreshToken, accessToken } = await response.json();
localStorage.setItem('refreshToken', refreshToken);

// Refresh Token
await fetch('/api/v1/auth/refresh', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ refreshToken })
});
```

## 🧪 Testing with Swagger

1. Navigate to `https://localhost:5286/swagger`
2. Register or login to get an access token
3. Click the "Authorize" button at the top
4. Enter: `Bearer YOUR_ACCESS_TOKEN`
5. Click "Authorize"
6. Now you can test protected endpoints

## 🛠️ Frontend Integration (React/Angular/Vue)

### React Example

```typescript
// services/auth.service.ts
export class AuthService {
  private accessToken: string | null = null;

  async register(data: RegisterDto) {
    const response = await fetch('/api/v1/auth/register', {
      method: 'POST',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    });
    const result = await response.json();
    this.accessToken = result.accessToken;
    return result;
  }

  async login(email: string, password: string) {
    const response = await fetch('/api/v1/auth/login', {
      method: 'POST',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ emailOrUserHandle: email, password })
    });
    const result = await response.json();
    this.accessToken = result.accessToken;
    return result;
  }

  async refreshToken() {
    try {
      const response = await fetch('/api/v1/auth/refresh', {
        method: 'POST',
        credentials: 'include'
      });
      if (response.ok) {
        const result = await response.json();
        this.accessToken = result.accessToken;
        return true;
      }
      return false;
    } catch {
      return false;
    }
  }

  async apiCall(url: string, options: RequestInit = {}) {
    const response = await fetch(url, {
      ...options,
      headers: {
        ...options.headers,
        'Authorization': `Bearer ${this.accessToken}`
      }
    });

    if (response.status === 401) {
      // Try to refresh
      const refreshed = await this.refreshToken();
      if (refreshed) {
        // Retry original request
        return fetch(url, {
          ...options,
          headers: {
            ...options.headers,
            'Authorization': `Bearer ${this.accessToken}`
          }
        });
      } else {
        // Redirect to login
        window.location.href = '/login';
      }
    }

    return response;
  }

  async logout() {
    await fetch('/api/v1/auth/logout', {
      method: 'POST',
      credentials: 'include',
      headers: { 'Authorization': `Bearer ${this.accessToken}` }
    });
    this.accessToken = null;
    window.location.href = '/login';
  }
}
```

### Angular HttpInterceptor

```typescript
// auth.interceptor.ts
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Add access token to request
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${this.authService.getAccessToken()}`
      },
      withCredentials: true // Important for cookies
    });

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Token expired, try refresh
          return this.authService.refreshToken().pipe(
            switchMap(() => {
              // Retry original request
              const retryReq = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${this.authService.getAccessToken()}`
                },
                withCredentials: true
              });
              return next.handle(retryReq);
            }),
            catchError(() => {
              // Refresh failed, redirect to login
              this.authService.logout();
              return throwError(error);
            })
          );
        }
        return throwError(error);
      })
    );
  }
}
```

## 🔧 Troubleshooting

### "Invalid refresh token" error

**Cause**: Cookie not being sent
**Solution**: Ensure `credentials: 'include'` in fetch or `withCredentials: true` in axios

### "Unauthorized" on protected endpoints

**Cause**: Access token expired or invalid
**Solution**: Call `/refresh` endpoint to get new access token

### CORS issues

**Update `Program.cs`:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowEncryptzCorsPolicy",
        builder => builder
            .WithOrigins("http://localhost:4200") // Your frontend URL
            .AllowCredentials() // Important for cookies
            .AllowAnyMethod()
            .AllowAnyHeader());
});
```

## 📚 Documentation

For detailed information, see:
- **[auth-design.md](./auth-design.md)** - Complete authentication design
- **[auth-implementation-summary.md](./auth-implementation-summary.md)** - Implementation details

## ⚡ Production Checklist

Before deploying:

- [ ] Generate secure JWT secret key (64+ characters)
- [ ] Store secrets in environment variables
- [ ] Enable HTTPS only
- [ ] Configure specific CORS origins
- [ ] Implement rate limiting
- [ ] Set up monitoring and alerts
- [ ] Test token rotation thoroughly
- [ ] Configure automated token cleanup
- [ ] Review security best practices

## 🆘 Need Help?

1. Check the [full documentation](./auth-design.md)
2. Review [implementation summary](./auth-implementation-summary.md)
3. Check application logs
4. Review unit tests for examples

---

**Quick Start Version**: 1.0  
**Last Updated**: 2025-11-20

