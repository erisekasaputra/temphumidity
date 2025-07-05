using Assets.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MqttHub.Applications.Interfaces;
using MqttHub.Applications.Services;
using MqttHub.Entities.Models;
using StreamGate.Worker.Application.Interfaces;
using StreamGate.Worker.Application.Services;
using StreamGate.Worker.Configuration;
using StreamGate.Worker.Infrastructure; 
using StreamGate.Worker.Infrastructure.Redis.Interfaces;
using StreamGate.Worker.Infrastructure.Redis.Services;
using StreamGate.Worker.SeedWorks;
using StreamGate.Worker.Workers;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RedisOption>(builder.Configuration.GetSection(RedisOption.Section)); 
builder.Services.Configure<CassandraOption>(builder.Configuration.GetSection(CassandraOption.Section)); 
builder.Services.Configure<MqttOption>(builder.Configuration.GetSection(MqttOption.Section)); 

builder.Services.AddSingleton<ShiftService>();
builder.Services.AddSingleton<IMqttHubService>(sp =>
{ 
    var mqttOptions = sp.GetRequiredService<IOptions<MqttOption>>().Value;

    var mqttHubService = new MqttHubService()
        .UseTcpServer(new Server(mqttOptions.BrokerAddress, mqttOptions.Port))
        .Build();

    return mqttHubService;
}); 

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
 

if (!await LicenseValidator.IsValid())
{
    builder.Services.AddSingleton<IRedisService, RedisService>();
    builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

    builder.Services.AddHostedService<SensorEventListener>();
    builder.Services.AddHostedService<SensorSynchronizer>();

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddHostedService<SensorEventPublisher>();
    }
}

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    dbContext.Database.Migrate(); 
} 

host.Run();
