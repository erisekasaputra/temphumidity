

using Assets.Domain.Domain.Enums;
using Newtonsoft.Json;

namespace Assets.Domain.Domain.Entities;

public class SensorValue
{
    public Guid Id { get; set; }
    public Guid SensorId { get; set; }
    public string SensorName { get; set; }
    public decimal Value { get; set; }
    public string Unit { get; set; }
    public string Location { get; set; }
    public int ShiftName { get; set; }
    public DateTime ShiftDate { get; set; }
    public string SerialNumber { get; set; }
    public AlertType AlertType { get; set; } 
    public Alert Alert { get; set; } 
    public SensorConfig? Config { get; set; }
    public DateTime CreatedAtUtc { get; set; } 

    public SensorValue()
    {
        SensorName = string.Empty;
        Unit = string.Empty;
        Alert = null!;
        Location = string.Empty;
        SerialNumber = string.Empty;
    }
    
    public SensorValue(
        Guid sensorId,
        string sensorName,
        decimal value,  
        int shiftName, 
        DateTime shiftDate,
        Alert alert, 
        SensorConfig config,
        DateTime createdAtUtc)
    {
        Id = Guid.NewGuid();
        SensorId = sensorId;
        SensorName = sensorName;
        Value = value;
        ShiftName = shiftName;
        ShiftDate = shiftDate;
        Unit = config.Unit;
        Location = config.Location;
        SerialNumber = config.SerialNumber;
        Alert = alert;
        AlertType = alert.Type;
        Config = config;
        CreatedAtUtc = createdAtUtc;
    }

    public string SerializeAlert()
    {
        return JsonConvert.SerializeObject(Alert);
    }

    public string SerializeConfig()
    {
        return JsonConvert.SerializeObject(Config);
    }

    public static SensorConfig? DeserializeConfig(string configJson)
    {
        return JsonConvert.DeserializeObject<SensorConfig>(configJson); 
    }

    public static Alert? DeserializeAlert(string alertJson)
    {
        return JsonConvert.DeserializeObject<Alert>(alertJson); 
    }
}