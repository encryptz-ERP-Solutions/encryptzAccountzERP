# Required Configuration Changes for Linux Hosting

This document outlines the **specific changes needed** in `appsettings.json` and `Program.cs` for Linux server deployment.

## Summary of Changes

### ✅ Changes Made to Program.cs

1. **Added Forwarded Headers Support** (CRITICAL)
   - Added `Microsoft.AspNetCore.HttpOverrides` namespace
   - Configured `ForwardedHeadersOptions` to trust reverse proxy headers
   - Added `app.UseForwardedHeaders()` as the first middleware
   - **Why**: When behind Nginx reverse proxy, the app needs to trust forwarded headers for proper IP detection, HTTPS detection, and host information

2. **Made CORS Origins Configurable**
   - CORS origins now read from `appsettings.json` or `appsettings.Production.json`
   - Falls back to development origins if not configured
   - **Why**: Allows different CORS origins for development vs production without code changes

### ✅ Changes Needed in appsettings.json

#### 1. Kestrel Endpoint Configuration

**Current:**
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

**Reasons:**
- `0.0.0.0` instead of `localhost` allows binding to all network interfaces (required for reverse proxy)
- Port `5000` is standard for Linux deployments and matches Nginx configuration
- Port `5286` was a development port

#### 2. Add CORS Origins Configuration

**Add this section to appsettings.json or appsettings.Production.json:**
```json
"CorsOrigins": [
    "https://yourdomain.com",
    "https://www.yourdomain.com",
    "https://api.yourdomain.com"
]
```

**Why:** 
- Allows production domains to access the API
- Prevents CORS errors in production
- Configurable without code changes

#### 3. Remove JSON Comments (Optional but Recommended)

**Current appsettings.json has comments:**
```json
"DefaultConnection": "Host=72.60.206.241;Port=5432;..."
    // PostgreSQL connection string format:
    // "Host=server_address;Port=5432;..."
```

**Note:** While .NET Core's JSON parser usually handles comments, standard JSON doesn't support them. For production, consider:
- Removing comments, OR
- Using `appsettings.Production.json` (which doesn't have comments)

## Complete appsettings.json for Production

Use `appsettings.Production.json` as a template. Key differences:

1. **Kestrel binding:** `http://0.0.0.0:5000`
2. **CORS Origins:** Configured in JSON array
3. **No comments:** Clean JSON
4. **Production values:** Strong JWT secret, production database, etc.

## Deployment Checklist

- [ ] Update `appsettings.json` Kestrel endpoint to `http://0.0.0.0:5000`
- [ ] Add `CorsOrigins` array to `appsettings.json` or `appsettings.Production.json`
- [ ] Update CORS origins with your production domain(s)
- [ ] Verify `Program.cs` has forwarded headers configuration (already updated)
- [ ] Test API behind Nginx reverse proxy
- [ ] Verify CORS works with production frontend domain

## Why These Changes Are Important

### Forwarded Headers (Program.cs)
Without forwarded headers support:
- ❌ IP addresses will show as `127.0.0.1` (proxy IP) instead of client IP
- ❌ HTTPS detection may fail
- ❌ Host header may be incorrect
- ❌ HTTPS redirection may not work properly

### Kestrel Binding (appsettings.json)
- `localhost` only binds to loopback interface (127.0.0.1)
- `0.0.0.0` binds to all interfaces, allowing Nginx to connect
- Port `5000` matches standard Linux deployment practices

### CORS Origins (appsettings.json)
- Hardcoded localhost origins won't work in production
- Production domains must be explicitly allowed
- Configurable origins allow environment-specific settings

## Testing After Changes

1. **Test API directly:**
   ```bash
   curl http://localhost:5000/api/health
   ```

2. **Test through Nginx:**
   ```bash
   curl http://api.yourdomain.com/api/health
   ```

3. **Verify CORS:**
   - Open browser console on your frontend
   - Check for CORS errors
   - Verify API calls work

4. **Check logs:**
   ```bash
   sudo journalctl -u encryptz-api -f
   ```

## Additional Notes

### HTTPS Redirection
The `app.UseHttpsRedirection()` middleware is kept in the code. When behind Nginx with SSL termination:
- Nginx handles HTTPS
- Nginx forwards HTTP to the API (on port 5000)
- The API sees HTTP, but the original request was HTTPS
- Forwarded headers middleware makes the API aware of the original HTTPS request

### Environment Variables
You can also override settings using environment variables:
```bash
export ASPNETCORE_URLS="http://0.0.0.0:5000"
export ASPNETCORE_ENVIRONMENT="Production"
```

### Multiple Environments
Consider using:
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides (already created)
