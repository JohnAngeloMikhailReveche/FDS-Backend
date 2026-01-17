using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NotificationService.Models;
using NotificationService.Repositories;
using NotificationService.Helpers;
using System.Text.Json.Serialization;
using NotificationService.Interfaces;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT Authentication
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "NotificationService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "NotificationService";

// var jwtKey = builder.Configuration["Jwt:Key"] 
//     ?? throw new InvalidOperationException("JWT Key is not configured. Please set 'Jwt:Key' in appsettings.json");
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured. Please set 'Jwt:Key' in appsettings.json or environment variable.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero 
    };
});

builder.Services.AddAuthorization();


// Register Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IInboxService, InboxService>();
builder.Services.AddScoped<ISMSService, SMSService>();
builder.Services.AddScoped<IUserService, UserService>();

// Register SMS Service
builder.Services.AddScoped<SemaphoreSMSService>();

// Register Gmail Service
builder.Services.AddScoped<GmailEmailService>();

// Register HttpClient
builder.Services.AddHttpClient();

builder.Services.AddDbContext<NotificationServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NotificationDatabase")));

builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:NotificationService"] ?? "http://localhost:5006");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

// Included from remote (Good practice to keep this)
app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

app.Run();