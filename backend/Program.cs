using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using KapeBara.MenuService.Data;
using KapeBara.MenuService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MenuDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

// Validation (.NET 10 built-in)
builder.Services.AddValidation();

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSPA", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable CORS
app.UseCors("AllowSPA");

// Serve static files (for images)
app.UseStaticFiles();

// OpenAPI + Scalar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("KapeBara Menu Service API");
        options.WithTheme(ScalarTheme.BluePlanet);
        options.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
    });
}

app.MapCategoryEndpoints();
app.MapVariantEndpoints();
app.MapMenuItemEndpoints();
app.MapMenuItemVariantEndpoints();

// Root redirect to Scalar docs
app.MapGet("/", () => Results.Redirect("/scalar/v1"))
    .ExcludeFromDescription();

app.Run();
