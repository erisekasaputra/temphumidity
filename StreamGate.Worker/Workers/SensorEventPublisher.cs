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

                Guid sensorId1 = Guid.Parse("658ca0f9-0991-4623-a8a4-a8c96e0e83cc");
                Guid sensorId2 = Guid.Parse("11bc09b9-cae2-4e0d-9296-a96863eedc8d"); 
                
                Random rand = new();

                var data1 = new 
                {
                    SensorId = sensorId1, 
                    SensorValue = rand.Next(80,120), 
                    SensorTimeUtc = utcNow
                };

                var data2 = new 
                {
                    SensorId = sensorId2, 
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
                    
                    await _hub.PublishAsync(
                        new PublishOption(
                            $"sensor/{sensorId2}", 
                            MqttQualityOfServiceLevel.AtLeastOnce, 
                            JsonSerializer.Serialize(data2), 
                            PayloadContentType.JSON,
                            true));  

                }, stoppingToken).ConfigureAwait(false);
                 
                await Task.Delay(5000, stoppingToken);
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
