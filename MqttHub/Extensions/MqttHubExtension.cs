using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MqttHub.Entities.Configs;

namespace MqttHub.Extensions;

public static class MqttHubExtension
{
    public static IServiceCollection AddMqttHub(this IServiceCollection services, IConfiguration configuration, string sectionName = "MqttSetting")
    {
        services.Configure<MqttSetting>(configuration.GetSection(sectionName));    
        return services;
    }
}
