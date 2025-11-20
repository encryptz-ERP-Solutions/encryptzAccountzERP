# 🚀 Authentication Testing - Quickstart

**Get testing in 3 minutes!**

## Option 1: Automated Tests (Recommended)

```bash
# Step 1: Navigate to test project
cd /Applications/Work/Encryptz/Github/encryptzAccountzERP/CoreModule/API/Tests/AuthTests

# Step 2: Setup (first time only)
dotnet restore
dotnet build

# Step 3: Run tests
dotnet test

# ✅ You should see: Test Run Successful - 20+ tests passed
```

## Option 2: Use Test Runner Script

```bash
# From project root
cd /Applications/Work/Encryptz/Github/encryptzAccountzERP/CoreModule

# Run all tests
./run-auth-tests.sh

# Run specific test types
./run-auth-tests.sh unit         # Unit tests only (fast)
./run-auth-tests.sh integration  # Integration tests only
./run-auth-tests.sh coverage     # With coverage report
```

## Option 3: Postman Collection

```bash
# Step 1: Import in Postman
# - File: postman/Auth.postman_collection.json
# - File: postman/Auth.postman_environment.json

# Step 2: Select Environment
# Choose "EncryptzERP - Local Development"

# Step 3: Run Collection
# Click "Runner" → Select collection → Run

# ✅ All 8 requests should pass with green checkmarks
```

## What Gets Tested?

✅ User Registration  
✅ Login Authentication  
✅ Protected Endpoint Access (`/users/me`)  
✅ Token Refresh with Rotation  
✅ Logout & Token Revocation  
✅ Password Validation  
✅ Security (invalid tokens, weak passwords, etc.)  

## Test Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/register` | Register new user |
| POST | `/api/v1/auth/login` | Login with credentials |
| POST | `/api/v1/auth/refresh` | Refresh access token |
| POST | `/api/v1/auth/logout` | Logout and revoke tokens |
| GET | `/api/v1/auth/users/me` | Get current user info (protected) |

## Expected Results

### Automated Tests
```
Test Run Successful.
Total tests: 20
     Passed: 20
     Failed: 0
  Skipped: 0
 Total time: ~5 seconds
```

### Postman Collection
```
8/8 requests passed
All test assertions ✓
Tokens managed automatically ✓
```

## Need Help?

📖 **Full Documentation**: `docs/testing-auth.md`  
📋 **Implementation Details**: `TESTING_SUMMARY.md`  
⚙️ **Setup Instructions**: `API/Tests/AuthTests/SETUP.md`  
🎯 **Complete Guide**: `AUTH_TESTING_COMPLETE.md`  

## Troubleshooting

**"Assets file not found"**  
→ Run `dotnet restore` first

**"Database connection error"**  
→ Unit tests don't need DB (run: `./run-auth-tests.sh unit`)  
→ Integration tests need API running

**"Port 7001 in use"**  
→ Stop other API instances or change port in `appsettings.json`

## What Was Built?

📦 **16 new/modified files**  
✅ **20+ automated tests** (unit + integration)  
✅ **8 Postman requests** with automated assertions  
✅ **4 documentation guides**  
✅ **1 convenience script** for easy testing  

## Quick Commands Cheat Sheet

```bash
# Run all tests
dotnet test

# Run with details
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "Name=RegisterLoginAndAccessProtectedEndpoint_ShouldSucceed"

# Run unit tests only (no DB)
dotnet test --filter "FullyQualifiedName~AuthServiceUnitTests"

# Run integration tests only
dotnet test --filter "FullyQualifiedName~AuthIntegrationTests"

# Get coverage report
./run-auth-tests.sh coverage
```

## 🎉 Ready to Test!

Choose your preferred method above and start testing!

All authentication flows are fully tested and documented.

---

**Status**: ✅ Ready  
**Framework**: xUnit + Postman  
**Coverage**: Complete auth system  
**Time to run**: ~5 seconds

