using System.Collections.Concurrent;
using System.Text.Json;
using MqttHub.Applications.Interfaces;
using MQTTnet.Protocol;

namespace Presentation.Dashboard.Applications.Services;

public class MqttService
{

    private readonly ILogger<MqttService> _logger;
    private readonly IMqttHubService _hub;
    private readonly string _consumerGroup = "MonitorSensorGroup";
    private readonly string _topic = "monitor/sensor/#";
    private readonly ConcurrentDictionary<string, ConcurrentBag<Action<string, string, string>>> _sensorHandlers = new();


    
    public MqttService(
        ILogger<MqttService> logger,
        IMqttHubService _mqttHubService)
    {
        _logger = logger;

        _hub = _mqttHubService;
        _hub.OnMessageReceived += Hub_OnMessageReceived;
        _hub.OnLog += Hub_OnLog;

        Task.Run(async () =>
        { 
            int retryCount = 0;
            const int maxRetry = 5;

            CancellationToken stoppingToken = new();

            while (!stoppingToken.IsCancellationRequested && _hub is not null)
            {
                try
                {   
                    await _hub.ConnectAsync(); 
                    await _hub.SubscribeAsync(new(_consumerGroup, _topic, false, MqttQualityOfServiceLevel.AtLeastOnce, false));

                    retryCount = 0; // Reset retry count jika berhasil terhubung 
                    
                    // Tunggu hingga token dibatalkan, artinya tidak ada polling terus-menerus
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"Failed to connect (attempt {retryCount}/{maxRetry}): {ex.Message}");

                    if (retryCount >= maxRetry)
                    {
                        Console.WriteLine("Max retry reached. Stopping service.");
                        throw;
                    }

                    // Jika gagal, tunggu 5 detik sebelum mencoba lagi
                    await Task.Delay(5000, stoppingToken);
                }
            }
        });
    } 

    public void Subscribe(Guid sensorId, Action<string, string, string> callback)
    { 
        var handlers = _sensorHandlers.GetOrAdd(sensorId.ToString(), _ => []);
        handlers.Add(callback);
    }

    public void Unsubscribe(Guid sensorId, Action<string, string, string> callback)
    {
        if (_sensorHandlers.TryGetValue(sensorId.ToString(), out var handlers))
        {
            var newHandlers = handlers.Where(h => h != callback).ToList(); // Buat list baru untuk menghindari race condition
            _sensorHandlers[sensorId.ToString()] = [.. newHandlers];
        }
    }


    private Task Hub_OnLog(object data)
    {
        Console.WriteLine($"Log [{DateTime.UtcNow}]: {data}");
        return Task.CompletedTask;
    }

    private async Task Hub_OnMessageReceived(string topic, string payload)
    {
        try
        { 
            var topicParts = topic.Split('/');
            if (topicParts.Length < 2) return;
            var sensorId = topicParts[2]; 
            
            if (_sensorHandlers.TryGetValue(sensorId, out var handlers))
            {
                foreach (var handler in handlers)
                {  
                    handler.Invoke(topic, sensorId, payload);
                }
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Consumer Exception: {ex.Message}");
        }
    }
}