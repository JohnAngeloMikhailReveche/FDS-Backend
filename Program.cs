using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

using NotificationService.Integration.Email; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the Gmail Service
builder.Services.AddScoped<GmailService>();

builder.Services.AddDbContext<NotificationContext>(options =>
    options.UseInMemoryDatabase("NotificationList"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();