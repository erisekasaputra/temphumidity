using System.Text.Json;
using MqttHub.Applications.Interfaces;
using MqttHub.Entities.Models;
using MQTTnet.Protocol;

namespace StreamGate.Worker.Workers;

public class SensorEventPublisher(ILogger<SensorEventPublisher> logger, IMqttHubService _mqttHubService) : BackgroundService
{
    private readonly ILogger<SensorEventPublisher> _logger = logger; 
    private readonly IMqttHubService _hub = _mqttHubService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                var utcNow = DateTime.UtcNow; 

                Guid sensorId1 = Guid.Parse("24ee31fd-971b-4cd3-a661-bb977c332d11"); 
                
                Random rand = new();

                var data1 = new 
                {
                    SensorId = sensorId1, 
                    SensorValue = rand.Next(80,120), 
                    SensorTimeUtc = utcNow
                };
 
 

                await Task.Run(async () => 
                { 
                    await _hub.PublishAsync(
                        new PublishOption(
                            $"sensor/{sensorId1}", 
                            MqttQualityOfServiceLevel.AtLeastOnce, 
                            JsonSerializer.Serialize(data1), 
                            PayloadContentType.JSON,
                            true));  
                     

                }, stoppingToken).ConfigureAwait(false);
                 
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Publisher Exception: {ex.Message}");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_hub is null)
        {
            return;
        }

        await _hub.DisconnectAsync();
        _hub.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
