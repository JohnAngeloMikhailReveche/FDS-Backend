using Microsoft.EntityFrameworkCore;
using NotificationService.Models;
using NotificationService.Repositories;
using NotificationService.Integration.Email;
using System.Text.Json.Serialization; 

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});


builder.Services.AddScoped<INotificationRepository, NotificationRepository>();


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<GmailService>();


builder.Services.AddDbContext<NotificationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NotificationDatabase")));


builder.Services.AddHttpClient("OrderService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:OrderService"] ?? "http://localhost:5001");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options => 
    { 
        options.SerializeAsV2 = true; 
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}


app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();