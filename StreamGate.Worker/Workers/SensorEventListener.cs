using System.Text.Json;
using Assets.Domain.Domain.Entities;
using Assets.Domain.Domain.Enums;
using Assets.Domain.Extensions;
using Assets.Domain.Services;
using Microsoft.EntityFrameworkCore;
using MqttHub.Applications.Interfaces;
using MqttHub.Entities.Models;
using MQTTnet.Protocol;
using StreamGate.Worker.Application.Interfaces;
using StreamGate.Worker.Application.Request;
using StreamGate.Worker.Infrastructure;

namespace StreamGate.Worker.Workers;

public class SensorEventListener : BackgroundService
{ 
    private readonly ShiftService _shiftService;
    private readonly AppDbContext _dbContext;
    private readonly IMqttHubService _hub;
    private readonly string _consumerGroup = "SensorGroup";
    private readonly string _topic = "sensor/#";
    private readonly IRedisCacheService _redisCache;
    // private readonly ICassandraDataService _cassandraData;
    public SensorEventListener( 
        ShiftService shiftService,
        IMqttHubService _mqttHubService,
        IRedisCacheService redisCache,
        IServiceScopeFactory scopeFactory
        )
    { 
        _shiftService = shiftService;
        var scope = scopeFactory.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        
        _hub = _mqttHubService;
        _hub.OnMessageReceived += Hub_OnMessageReceived;
        _hub.OnLog += Hub_OnLog; 
    
        _redisCache = redisCache;
        // _cassandraData = cassandraData;
    } 

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    { 
        int retryCount = 0;
        const int maxRetry = 5;

        while (!stoppingToken.IsCancellationRequested && _hub is not null)
        {
            try
            { 
                await _hub.ConnectAsync();

                await _hub.SubscribeAsync(
                    new SubscribeOption(_consumerGroup, _topic, false, MqttQualityOfServiceLevel.AtLeastOnce, false));

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

    private Task Hub_OnLog(object data)
    { 
        Console.WriteLine($"Log [{DateTime.UtcNow}][{typeof(SensorEventListener)}]: {data}");
        return Task.CompletedTask;
    }

    
    private async Task Hub_OnMessageReceived(string topic, string payload)
    { 
        try
        {  
            var request = JsonSerializer.Deserialize<StreamRequest>(payload) ?? throw new InvalidDataException();

            if (!topic.Contains(request.SensorId.ToString()))
            {
                return;
            } 

            var sensorConfig = await _redisCache.GetObject<SensorConfig>($"sensor/config/{request.SensorId}");
            
            if (sensorConfig is null)
            {
                sensorConfig = await _dbContext.SensorConfigs
                    .FirstOrDefaultAsync(x => x.Id == request.SensorId) 
                    ?? throw new Exception($"Sensor {request.SensorId} has no master configuration.");

                await _redisCache.SetObject($"sensor/config/{request.SensorId}", sensorConfig);
            }
            
 
            var sensor = await _redisCache.GetObject<Sensor>(topic); 
            sensor ??= new Sensor(sensorConfig.Id, sensorConfig.Name, sensorConfig.Location, sensorConfig.SerialNumber, 100);


            var (shiftName, start, end, shiftDate) = _shiftService.GetCurrentShiftDateStartEnd();  

            shiftDate ??= DateTime.UtcNow.FromUtcToJakarta();

            if (sensor is not null)
            {
                sensor.UpdateProperties(sensorConfig.Name, sensorConfig.Location, sensorConfig.SerialNumber);

                (var sensorValue, var isShouldSave) = sensor.InsertSensorData(
                    sensorConfig, 
                    request.SensorValue,  
                    shiftName, 
                    shiftDate.Value,
                    request.SensorTimeUtc ?? DateTime.UtcNow); 
            
                await _redisCache.SetObject(topic, sensor); 

                var serializedData = JsonSerializer.Serialize(sensor); 

                await _hub.PublishAsync(
                    new PublishOption(
                        $"monitor/sensor/{sensorConfig.Id}", 
                        MqttQualityOfServiceLevel.AtLeastOnce, 
                        serializedData,
                        PayloadContentType.JSON,
                        true));  

                await _hub.PublishAsync(
                    new PublishOption(
                        $"monitor/sensor/alert/{sensorConfig.Id}", 
                        MqttQualityOfServiceLevel.AtLeastOnce, 
                        sensorValue.Alert.Type.ToString(),
                        PayloadContentType.TextPlain,
                        true));  
                
                if (isShouldSave)
                { 
                    await _dbContext.SensorValues.AddAsync(sensorValue);
                    await _dbContext.SaveChangesAsync();
                }    


                if (shiftName <= 0 || start is null || end is null)
                {
                    Console.WriteLine("No shift was found");
                    return;
                }
 
 
                var sensorShiftResult = await _redisCache
                    .GetObject<SensorShiftResult>($"sensor/each/shif/{sensorConfig.Id}/{start.Value:yyyyMMddHHmmss}/{end.Value:yyyyMMddHHmmss}"); 

                sensorShiftResult ??= await _dbContext.SensorShiftResults
                            .Where(x => x.DateStart == start && x.DateEnd == end && x.SensorId == sensorConfig.Id)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();  

                bool isValueError = sensorValue.Alert.Type == AlertType.UCLExceeded || sensorValue.Alert.Type == AlertType.LCLExceeded;    
            
                if (sensorShiftResult is not null)
                {
                    if  (
                            isValueError 
                            && 
                            (
                                (sensorValue.Value > sensorShiftResult.AverageOrErrorValue && sensorValue.Value > sensorConfig.UCL) 
                                ||
                                (sensorValue.Value < sensorShiftResult.AverageOrErrorValue && sensorValue.Value < sensorConfig.LCL)
                            )
                        )
                    {
                        var trackedSensorShiftResult = await _dbContext.SensorShiftResults
                            .Where(x => x.DateStart == start && x.DateEnd == end && x.SensorId == sensorConfig.Id).FirstOrDefaultAsync(); 

                        if (trackedSensorShiftResult is null)
                        {
                            return;
                        }

                        trackedSensorShiftResult.UpdateValue(sensorValue.Value, sensorConfig.Unit, 1, sensorValue.Alert.Type);
                        _dbContext.SensorShiftResults.Update(trackedSensorShiftResult);
                        await _dbContext.SaveChangesAsync(); 
                        
                        await SaveSensorEachShiftToRedis(sensorConfig.Id, start.Value, end.Value, trackedSensorShiftResult); 
                    }
                } 
                else
                { 
                    var newSensorShiftResult = new SensorShiftResult(shiftName,
                                                                     start.Value,
                                                                     end.Value,
                                                                     sensorConfig.Id,
                                                                     sensorValue.Value,
                                                                     sensorConfig.Unit,
                                                                     sensorValue.Alert.Type is AlertType.UCLExceeded or AlertType.LCLExceeded ? 1 : 0,
                                                                     sensorValue.Alert.Type);
                    
                    await _dbContext.SensorShiftResults.AddAsync(newSensorShiftResult);
                    await _dbContext.SaveChangesAsync();
                    
                    await SaveSensorEachShiftToRedis(sensorConfig.Id, start.Value, end.Value, newSensorShiftResult);
                }
            }  
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Consumer Exception: {ex.Message}");
        } 
    }

    private async Task SaveSensorEachShiftToRedis(Guid sensorId, DateTime start, DateTime end, SensorShiftResult sensorShiftResult)
    {
        await _redisCache
            .SetObject($"sensor/each/shif/{sensorId}/{start:yyyyMMddHHmmss}/{end:yyyyMMddHHmmss}", sensorShiftResult, TimeSpan.FromDays(2)); 
    }
}