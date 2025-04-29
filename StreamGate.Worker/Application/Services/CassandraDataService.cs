using Cassandra;
using StreamGate.Worker.Application.Interfaces;
using StreamGate.Worker.Infrastructure.Cassandra.Interfaces;
using Cassandra.Mapping; 
using Newtonsoft.Json;
using Assets.Domain.Domain.Entities;

namespace StreamGate.Worker.Application.Services;

public class CassandraDataService : ICassandraDataService
{
    private readonly ISession _context;
    private readonly Mapper _mapper;

    public CassandraDataService(ICassandraService cassandraService)
    {
        _context = cassandraService.GetSession();
        _mapper = new Mapper(_context);
    }
    
    public async Task Insert(SensorValue sensorValue)
    {  
        var alertJson = sensorValue.SerializeAlert();
 
        string insertQuery = @"
            INSERT INTO sensor_data.sensor_values (
                sensor_id, 
                sensor_name, 
                value, 
                alert_type, 
                alert_message, 
                created_at_utc
            ) 
            VALUES (?, ?, ?, ?, ?, ?)";
         
        var statement = new SimpleStatement(insertQuery,
            sensorValue.SensorId,
            sensorValue.SensorName,
            sensorValue.Value,
            sensorValue.Alert.Type.ToString(),  
            alertJson,   
            sensorValue.CreatedAtUtc);
 
        await _context.ExecuteAsync(statement);
    }

    public async Task<List<SensorValue>> GetSensorDataAsync(Guid sensorId)
    {
        string selectQuery = "SELECT * FROM sensor_data.sensor_values WHERE sensor_id = ?";
        var result = await _context.ExecuteAsync(new SimpleStatement(selectQuery, sensorId));

        var sensorValues = new List<SensorValue>();

        foreach (var row in result)
        { 
            var alertJson = row.GetValue<string>("alert_message");  
            var alert = SensorValue.DeserializeAlert(alertJson);

            
            var configJson = row.GetValue<string>("config_message");  
            var config = SensorValue.DeserializeConfig(configJson);

            if (alert is null  || config is null)
            {
                continue;
            }
 
            var sensorValue = new SensorValue(
                row.GetValue<Guid>("sensor_id"),
                row.GetValue<string>("sensor_name"),
                row.GetValue<decimal>("value"), 
                row.GetValue<int>("shift_name"), 
                row.GetValue<DateTime>("shift_date"), 
                alert,
                config,
                row.GetValue<DateTime>("created_at_utc")
            );

            sensorValues.Add(sensorValue);
        }

        return sensorValues;
    }
}