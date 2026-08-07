using Microsoft.EntityFrameworkCore;
using SmartWarehouse.Services;
using SmartWarehouse.Services.Interfaces;
using SmartWarehouse.Database;
using SmartWarehouse.Models;
using SmartWarehouse.Services.Mqtt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RobotBuffer>();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();


// MQTT Background Service reg
// Every backhround service must inherit BackhroundService!
builder.Services.AddHostedService<MqttBackgroundService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SmartWarehouseContext>(options => options.UseSqlServer(connectionString));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
