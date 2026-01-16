// ============================================================================
// FoodDeliverySystem.API/Program.cs
// ============================================================================
// IMPORTANT: This application uses ONLY STORED PROCEDURES - NO Entity Framework
// Database operations use ADO.NET with SqlConnection
// ApplicationDbContext is ONLY used for database seeding (optional)
// ============================================================================

using FoodDeliverySystem.Application.Interfaces;
using FoodDeliverySystem.Application.Services;
using FoodDeliverySystem.Common.Helpers;
using FoodDeliverySystem.Infrastructure.Data;
using FoodDeliverySystem.Infrastructure.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========== 1. ADD SERVICES TO CONTAINER ==========
builder.Services.AddControllers();

// ========== 2. CONFIGURE DATABASE CONNECTION STRING ==========
// Connection string is used by ADO.NET for stored procedure execution
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Fallback connection string
    connectionString = "Server=localhost\\SQLEXPRESS;Database=FoodDeliveryDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true;";
}

// ============================================================================
// OPTIONAL: Keep DbContext ONLY for seeding purposes
// If you don't need seeding, you can remove this entirely
// AuthService and AuthController no longer use DbContext
// ============================================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ========== 3. CONFIGURE JWT SETTINGS ==========
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

if (jwtSettings == null)
{
    // Default settings if not configured in appsettings.json
    jwtSettings = new JwtSettings
    {
        Secret = "YourSuperSecretKeyHereAtLeast32CharactersLong!123456",
        Issuer = "FoodDeliverySystem",
        Audience = "FoodDeliveryClients",
        ExpiryMinutes = 60
    };
}

// Register JwtSettings as a singleton so it can be injected into services
builder.Services.AddSingleton(jwtSettings);

// ========== 4. CONFIGURE AUTHENTICATION ==========
var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ========== 5. ADD AUTHORIZATION ==========
builder.Services.AddAuthorization();

// ========== 6. REGISTER APPLICATION SERVICES ==========
// ============================================================================
// AuthService now uses IConfiguration (not DbContext) to get connection string
// All database operations execute stored procedures via ADO.NET
// ============================================================================
builder.Services.AddScoped<IAuthService, AuthService>();

// ========== 7. CONFIGURE CORS ==========
builder.Services.AddCors();

// ========== 8. CONFIGURE SWAGGER ==========
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Food Delivery System API",
        Version = "v1",
        Description = "Authentication API for Food Delivery System (Using Stored Procedures)"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// ========== 9. CONFIGURE PIPELINE ==========

// Use CORS before other middleware
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// Configure Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Food Delivery API v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at app's root (localhost:port/)
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ========== 10. SIMPLE HEALTH CHECK ==========
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    message = "Food Delivery API is running (Using Stored Procedures)",
    databaseMode = "ADO.NET with Stored Procedures"
}));

// ========== 11. SEED DATABASE (OPTIONAL) ==========
// ============================================================================
// OPTIONAL: You can remove this entire section if you don't need seeding
// This is the ONLY place where DbContext is used
// Consider creating a stored procedure for seeding instead (SP_SeedSuperAdmin)
// ============================================================================
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Check database connection
    var canConnect = await context.Database.CanConnectAsync();
    Console.WriteLine($"[{DateTime.UtcNow}] Database connection: {(canConnect ? "SUCCESS" : "FAILED")}");

    if (canConnect)
    {
        // Seed SuperAdmin account
        await DatabaseSeeder.SeedSuperAdminAsync(context);
        Console.WriteLine($"[{DateTime.UtcNow}] Database seeding completed");
    }
    else
    {
        Console.WriteLine($"[{DateTime.UtcNow}] WARNING: Cannot connect to database - seeding skipped");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[{DateTime.UtcNow}] Database seeding error: {ex.Message}");
    Console.WriteLine($"[{DateTime.UtcNow}] Application will continue without seeding...");
}

// ========== 12. RUN THE APP ==========
Console.WriteLine("=======================================");
Console.WriteLine("Food Delivery System API");
Console.WriteLine("Architecture: Stored Procedures with ADO.NET");
Console.WriteLine("---------------------------------------");
Console.WriteLine("Swagger UI: https://localhost:7164/");
Console.WriteLine("Swagger UI: http://localhost:5254/");
Console.WriteLine("Health Check: /api/health");
Console.WriteLine("---------------------------------------");
Console.WriteLine("Endpoints:");
Console.WriteLine("  POST /api/auth/login");
Console.WriteLine("  POST /api/auth/register/customer");
Console.WriteLine("  POST /api/auth/create/admin [SuperAdmin]");
Console.WriteLine("  POST /api/auth/create/rider [Admin]");
Console.WriteLine("  DELETE /api/auth/delete [Admin]");
Console.WriteLine("  GET /api/auth/users [Admin]");
Console.WriteLine("  GET /api/auth/profile [Authenticated]");
Console.WriteLine("  POST /api/auth/logout [Authenticated]");
Console.WriteLine("=======================================");

app.Run();