using Assets.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MqttHub.Applications.Interfaces;
using MqttHub.Applications.Services;
using MqttHub.Entities.Models;
using MudBlazor.Services;
using Presentation.Dashboard.Applications.Apis;
using Presentation.Dashboard.Applications.Interfaces;
using Presentation.Dashboard.Applications.Services;
using Presentation.Dashboard.Components;
using Presentation.Dashboard.Configurations;
using Presentation.Dashboard.Infrastructures;
using Presentation.Dashboard.Infrastructures.Redis.Interfaces;
using Presentation.Dashboard.Infrastructures.Redis.Services;

var builder = WebApplication.CreateBuilder(args);
 
builder.Services.Configure<MqttOption>(builder.Configuration.GetSection(MqttOption.Section));
builder.Services.Configure<RedisOption>(builder.Configuration.GetSection(RedisOption.Section)); 

builder.Services.AddSingleton<ShiftService>();  

builder.Services.AddSingleton<IMqttHubService>(sp =>
{
    var mqttOptions = sp.GetRequiredService<IOptions<MqttOption>>().Value;
    var mqttHubService = new MqttHubService()
        .UseTcpServer(new Server(mqttOptions.BrokerAddress, mqttOptions.Port))
        .Build();

    return mqttHubService;
}); 
builder.Services.AddSingleton<MqttService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();  

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))); 

builder.Services.AddMudServices();

var app = builder.Build();

var preloadedMqtt = app.Services.GetRequiredService<MqttService>(); // dont remove the code

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true); 
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(); 
 
app.Run(); 