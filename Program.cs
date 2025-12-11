using Microsoft.EntityFrameworkCore;
using NotificationService.Models;
using NotificationService.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<NotificationContext>(options =>
    options.UseInMemoryDatabase("NotificationList"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.SerializeAsV2 = true;
    });
    app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}   


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
