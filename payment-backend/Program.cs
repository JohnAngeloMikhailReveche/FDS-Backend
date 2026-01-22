using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PaymentService2.Data;
using PaymentService2.Services;

var builder = WebApplication.CreateBuilder(args);

// Load .env.local if exists
var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env.local");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
        
        var eqIndex = trimmed.IndexOf('=');
        if (eqIndex > 0)
        {
            var key = trimmed[..eqIndex].Trim();
            var value = trimmed[(eqIndex + 1)..].Trim().Trim('"');
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

// Get connection string
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=PaymentService2;Trusted_Connection=True;TrustServerCertificate=True;";

// Register SqlHelper
builder.Services.AddSingleton(new SqlHelper(connectionString));

// Register HttpClientFactory (required for PayMongo)
builder.Services.AddHttpClient();

// Register Services
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITopUpService, TopUpService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IRefundService, RefundService>();
builder.Services.AddScoped<IPayMongoService, PayMongoService>();
builder.Services.AddScoped<IPaymentProvider, PayMongoPaymentProvider>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payment Service 2 API (Stored Procedures)",
        Version = "v1",
        Description = "Payment Service using ADO.NET and Stored Procedures"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
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
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JwtSettings__Secret")
                ?? builder.Configuration["JwtSettings:Secret"]
                ?? "YourSuperSecretKeyHereAtLeast32CharactersLong!";
var jwtIssuer = Environment.GetEnvironmentVariable("JwtSettings__Issuer")
                ?? builder.Configuration["JwtSettings:Issuer"]
                ?? "FoodDeliverySystem";
var jwtAudience = Environment.GetEnvironmentVariable("JwtSettings__Audience")
                ?? builder.Configuration["JwtSettings:Audience"]
                ?? "FoodDeliveryClients";

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
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateLifetime = true
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()  // Allow tunnel URLs
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service 2 API v1");
});

app.UseCors();
app.UseStaticFiles(); // Serve uploaded photos from wwwroot
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Get tunnel URL from environment
var tunnelUrl = Environment.GetEnvironmentVariable("TUNNEL_URL");

// Startup message
Console.WriteLine("===========================================");
Console.WriteLine("Payment Service 2 (Stored Procedures)");
Console.WriteLine("-------------------------------------------");
Console.WriteLine("Swagger UI: http://localhost:5201/swagger");
Console.WriteLine("-------------------------------------------");
Console.WriteLine("Architecture: ADO.NET + Stored Procedures");
Console.WriteLine("Database: " + (connectionString.Contains("PaymentService2") ? "PaymentService2" : "Custom"));

// Dynamic webhook registration
if (!string.IsNullOrEmpty(tunnelUrl))
{
    Console.WriteLine("-------------------------------------------");
    Console.WriteLine($"Tunnel URL: {tunnelUrl}");
    Console.WriteLine("Registering PayMongo webhook...");
    
    try
    {
        using var scope = app.Services.CreateScope();
        var paymongoService = scope.ServiceProvider.GetRequiredService<IPayMongoService>();
        var webhookUrl = $"{tunnelUrl}/api/payments/webhook";
        var result = await paymongoService.RegisterOrUpdateWebhookAsync(webhookUrl);
        
        if (result.Success)
        {
            Console.WriteLine($"✓ Webhook registered: {webhookUrl}");
            Console.WriteLine($"  Webhook ID: {result.WebhookId}");
        }
        else
        {
            Console.WriteLine($"✗ Webhook registration failed: {result.Message}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Webhook registration error: {ex.Message}");
    }
}
else
{
    Console.WriteLine("-------------------------------------------");
    Console.WriteLine("No TUNNEL_URL set. Webhook not registered.");
    Console.WriteLine("Set TUNNEL_URL in .env.local or environment");
}

Console.WriteLine("===========================================");

// Run database migrations
using (var scope = app.Services.CreateScope())
{
    var sql = scope.ServiceProvider.GetRequiredService<SqlHelper>();
    var migrationService = new MigrationService(sql);
    
    try
    {
        await migrationService.RunMigrationsAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration warning: {ex.Message}");
    }
}

app.Run();

