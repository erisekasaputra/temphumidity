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

                Guid sensorId1 = Guid.Parse("824d9143-d703-45f4-882a-2da456cfa831");
                Guid sensorId2 = Guid.Parse("c8d36605-9e8f-4d56-bc7f-69c58a45157b"); 
                
                Random rand = new();

                var data1 = new 
                {
                    SensorId = sensorId1, 
                    SensorValue = rand.Next(40,70), 
                    SensorTimeUtc = utcNow
                };

                var data2 = new 
                {
                    SensorId = sensorId2, 
                    SensorValue = rand.Next(20,30),
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
                    
                    await _hub.PublishAsync(
                        new PublishOption(
                            $"sensor/{sensorId2}", 
                            MqttQualityOfServiceLevel.AtLeastOnce, 
                            JsonSerializer.Serialize(data2), 
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
