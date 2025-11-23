
using Microsoft.AspNetCore.Mvc.Routing;
using Repository.Core.Interface;
using Repository.Core;
using Microsoft.OpenApi.Models;
using Repository.Admin;
using Repository.Admin.Interface;
using Infrastructure;
using BusinessLogic.Admin.Services;
using BusinessLogic.Admin.Interface;
using BusinessLogic.Core.Services;
using BusinessLogic.Core.Interface;
using Business.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using Repository.Accounts;
using BusinessLogic.Accounts;
using System;
using System.Threading.Tasks;
using BusinessLogic.Core.Services.Auth;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configure Forwarded Headers for reverse proxy (Nginx)
// This is CRITICAL for Linux hosting behind a reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    // Trust only the reverse proxy (Nginx running on localhost)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    // If your reverse proxy is on a different server, add it here:
    // options.KnownProxies.Add(IPAddress.Parse("your-proxy-ip"));
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not found in configuration");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
        // Skip authentication for OPTIONS requests (CORS preflight)
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Method == "OPTIONS")
                {
                    context.Token = null;
                }
                return Task.CompletedTask;
            }
        };
    });
// Add services to the container.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageBusinesses", policy =>
        policy.RequireClaim("permission", "CanManageBusinesses"));
});

// Enable CORS with explicit origins for security
// Get CORS origins from configuration (appsettings.json or appsettings.Production.json)
// Falls back to development origins if not configured
var corsOriginsConfig = builder.Configuration.GetSection("CorsOrigins").Get<string[]>();
var allowedOrigins = corsOriginsConfig != null && corsOriginsConfig.Length > 0
    ? corsOriginsConfig
    : new[]
    {
        // Development origins (used when CorsOrigins is not configured)
        "http://localhost:4200",      // Angular dev server
        "http://127.0.0.1:4200",      // Angular dev server (alternative)
        "https://localhost:4200",     // Angular dev server (HTTPS)
        "http://localhost:5286",      // Swagger/API (for testing)
        "https://localhost:7037",      // API HTTPS endpoint
        "http://72.60.206.241:5007", // Production UI server
        "https://72.60.206.241",  // Production UI server HTTPS  
        "http://72.60.206.241"
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowEncryptzCorsPolicy",
        policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()  // This includes OPTIONS
            .AllowAnyHeader()   // This includes all headers like Authorization, Content-Type, etc.
            .AllowCredentials()  // Required for cookies/refresh tokens
            .WithExposedHeaders("Authorization", "Content-Type", "Accept", "Origin") // Expose headers to client
            .SetPreflightMaxAge(TimeSpan.FromSeconds(86400))); // Cache preflight for 24 hours
});


builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmailService>();

// Add AutoMapper and scan for mapping profiles
builder.Services.AddAutoMapper(typeof(BusinessLogic.Admin.Mappers.UserMappingProfile).Assembly);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https.aka.ms/aspnetcore/swashbuckle

// Register database helper (ADO.NET)
builder.Services.AddScoped<CoreSQLDbHelper>();

// Register repository layer
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IUserBusinessRoleRepository, UserBusinessRoleRepository>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IUserBusinessRoleService, UserBusinessRoleService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IAccountTypeRepository, AccountTypeRepository>();
builder.Services.AddScoped<IChartOfAccountRepository, ChartOfAccountRepository>();
builder.Services.AddScoped<IAccountTypeService, AccountTypeService>();
builder.Services.AddScoped<IChartOfAccountService, ChartOfAccountService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<ILedgerService, LedgerService>();

// Register new repository layer
builder.Services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
builder.Services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();
builder.Services.AddScoped<ISubscriptionPlanPermissionRepository, SubscriptionPlanPermissionRepository>();

// Register new business logic layer
builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
builder.Services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();
builder.Services.AddScoped<ISubscriptionPlanPermissionService, SubscriptionPlanPermissionService>();

// Register new infrastructure for PostgreSQL
builder.Services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();

// Register new audit and user-business services
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IUserBusinessRepository, UserBusinessRepository>();
builder.Services.AddScoped<IUserBusinessService, UserBusinessService>();

// Register dashboard service
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Register authentication services
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthService, BusinessLogic.Core.Services.Auth.AuthService>();

builder.Services.AddScoped<ExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ERP API", Version = "v1" });
    
    // FIX: Use full type name to avoid schema ID collisions when multiple DTOs have the same class name
    // This resolves conflicts like BusinessLogic.Core.DTOs.LoginRequestDto vs BusinessLogic.Core.DTOs.Auth.LoginRequestDto
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and the token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.

// CRITICAL: Use Forwarded Headers BEFORE other middleware
// This must be first to properly handle requests from reverse proxy
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CRITICAL: Explicit OPTIONS handling BEFORE routing to ensure CORS preflight works
// This middleware handles OPTIONS requests early and validates the origin
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        var origin = context.Request.Headers["Origin"].ToString();
        
        // Get allowed origins from configuration (same logic as CORS policy)
        var corsOriginsConfig = app.Configuration.GetSection("CorsOrigins").Get<string[]>();
        var allowedOriginsList = corsOriginsConfig != null && corsOriginsConfig.Length > 0
            ? corsOriginsConfig
            : new[]
            {
                "http://localhost:4200",
                "http://127.0.0.1:4200",
                "https://localhost:4200",
                "http://localhost:5286",
                "https://localhost:7037",
                "http://72.60.206.241:5007",
                "https://72.60.206.241",
                "http://72.60.206.241"
            };
        
        // Check if origin is in allowed list
        if (!string.IsNullOrEmpty(origin) && allowedOriginsList.Contains(origin))
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, PATCH, OPTIONS";
            context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With, Accept, Origin";
            context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
            context.Response.Headers["Access-Control-Max-Age"] = "86400";
            context.Response.StatusCode = 204;
            return;
        }
        else
        {
            // Origin not allowed - let CORS middleware handle it
            await next();
            return;
        }
    }
    await next();
});

// CRITICAL: Routing must be configured BEFORE CORS
app.UseRouting();

// CRITICAL: CORS must be configured BEFORE authentication and authorization
// This ensures preflight OPTIONS requests are handled correctly
// CORS middleware will automatically handle OPTIONS requests and add the required headers
app.UseCors("AllowEncryptzCorsPolicy");

// Ensure authentication doesn't interfere with OPTIONS requests
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();

// Make the Program class public for integration tests
public partial class Program { }
