
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
using BusinessLogic.Core.Services.Auth;

var builder = WebApplication.CreateBuilder(args);

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
    });
// Add services to the container.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageBusinesses", policy =>
        policy.RequireClaim("permission", "CanManageBusinesses"));
});

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowEncryptzCorsPolicy",
        builder => builder
            .AllowAnyOrigin()  // You can replace with .WithOrigins("http://localhost:4200") for more control
            .AllowAnyMethod()
            .AllowAnyHeader());
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
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IUserBusinessRoleRepository, UserBusinessRoleRepository>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IUserBusinessRoleService, UserBusinessRoleService>();
builder.Services.AddScoped<IAccountTypeRepository, AccountTypeRepository>();
builder.Services.AddScoped<IChartOfAccountRepository, ChartOfAccountRepository>();
builder.Services.AddScoped<IAccountTypeService, AccountTypeService>();
builder.Services.AddScoped<IChartOfAccountService, ChartOfAccountService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

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
builder.Services.AddScoped<IUserBusinessRepository, UserBusinessRepository>();
builder.Services.AddScoped<IUserBusinessService, UserBusinessService>();

// Register authentication services
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthService, BusinessLogic.Core.Services.Auth.AuthService>();

builder.Services.AddScoped<ExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ERP API", Version = "v1" });
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add CORS before authorization
app.UseCors("AllowEncryptzCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
