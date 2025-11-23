# Quick Summary: Required Changes for Linux Hosting

## ✅ Already Updated (No Action Needed)

1. **Program.cs** - ✅ Updated with:
   - Forwarded headers support for reverse proxy
   - Configurable CORS origins from appsettings

## ⚠️ Action Required: Update appsettings.json

### Change 1: Update Kestrel Endpoint

**Find this section:**
```json
"Kestrel": {
    "Endpoints": {
        "Http": {
            "Url": "http://localhost:5286"
        }
    }
}
```

**Change to:**
```json
"Kestrel": {
    "Endpoints": {
        "Http": {
            "Url": "http://0.0.0.0:5000"
        }
    }
}
```

### Change 2: Add CORS Origins Configuration

**Add this new section** (anywhere in the JSON, typically after JwtSettings):
```json
"CorsOrigins": [
    "https://yourdomain.com",
    "https://www.yourdomain.com"
]
```

**Example complete appsettings.json structure:**
```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Host=your_host;Port=5432;Database=your_database;Username=your_username;Password=your_password"
    },
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*",
    "EmailSettings": {
        "SmtpServer": "your.smtp.server",
        "Port": 465,
        "EnableSSL": "true",
        "SenderEmail": "noreply@yourdomain.com",
        "SenderPassword": "your_email_password"
    },
    "JwtSettings": {
        "SecretKey": "REPLACE_WITH_STRONG_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG_FOR_PRODUCTION",
        "Issuer": "yourdomain.com",
        "Audience": "youraudience.com",
        "AccessTokenExpirationMinutes": 15,
        "RefreshTokenExpirationDays": 7
    },
    "CorsOrigins": [
        "https://yourdomain.com",
        "https://www.yourdomain.com"
    ],
    "Kestrel": {
        "Endpoints": {
            "Http": {
                "Url": "http://0.0.0.0:5000"
            }
        }
    }
}
```

## Why These Changes?

1. **`0.0.0.0:5000` instead of `localhost:5286`**
   - `0.0.0.0` allows Nginx (reverse proxy) to connect
   - Port `5000` is standard for Linux deployments
   - Matches the Nginx configuration

2. **CORS Origins in Configuration**
   - Allows production domains to access the API
   - Prevents CORS errors
   - No code changes needed for different environments

## Alternative: Use appsettings.Production.json

Instead of modifying `appsettings.json`, you can:
1. Keep `appsettings.json` for development
2. Use `appsettings.Production.json` for production (already created)
3. Set `ASPNETCORE_ENVIRONMENT=Production` on Linux server

The `appsettings.Production.json` file already has the correct structure.

## Verification

After making changes, verify:
1. API starts on port 5000: `curl http://localhost:5000/api/health`
2. Nginx can proxy to API
3. CORS works with your frontend domain

