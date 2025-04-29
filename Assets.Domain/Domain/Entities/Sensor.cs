namespace Assets.Domain.Domain.Entities;

public class Sensor
{
    public Guid Id { get; set; }
    public Guid SensorId { get; set;}
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public List<SensorValue> Values { get; set; } = [];
    public int DataLength { get; set; }
    public Alert Alert { get; set; } 
    public DateTime LastIngestAtUtc { get; set; } 

    public Sensor()
    {
        Alert = Alert.Normal();
    }
    
    public Sensor(Guid sensorId, string name, string location, string serialNumber, int dataLength)
    {
        Id = Guid.NewGuid();
        SensorId = sensorId;
        Name = name;
        Location = location;
        SerialNumber = serialNumber;
        DataLength = dataLength; 
        Alert = Alert.Normal();
    }

    public void UpdateProperties(string name, string location, string serialNumber)
    {
        Name = name;
        Location = location;
        SerialNumber = serialNumber;
    }
    
    public (SensorValue sensorValue, bool isShouldSave) InsertSensorData(SensorConfig config, decimal value, int shiftName, DateTime shiftDate, DateTime sensorTimeUtc)
    {   
        SensorValue sensorValue;
        Alert alert;  

        if (value >= config.UCL)
        {
            alert = Alert.UCLExceeded($"Critical Alert: Sensor value {value} exceeds the Upper Control Limit (UCL)");
            sensorValue = new SensorValue(SensorId, Name, value, shiftName, shiftDate, alert, config, sensorTimeUtc); 
            return (sensorValue, AddSensorData(sensorValue, alert, config));
        }

        if (value <= config.LCL)
        { 
            alert = Alert.LCLExceeded($"Critical Alert: Sensor value {value} is below the Lower Control Limit (LCL)");
            sensorValue = new SensorValue(SensorId, Name, value, shiftName, shiftDate, alert, config, sensorTimeUtc); 
            return (sensorValue, AddSensorData(sensorValue, alert, config));
        }

        if (value >= config.WarningUpperLevel && value < config.UCL)
        { 
            alert = Alert.UCLApproachingWarning(
                $"Warning: Sensor value {value} is approaching the Upper Control Limit (UCL) of {config.UCL}. {config.WarningUpperLevel} Please monitor closely");
            sensorValue = new SensorValue(SensorId, Name, value, shiftName, shiftDate, alert, config, sensorTimeUtc); 
            return (sensorValue, AddSensorData(sensorValue, alert, config));
        }

        if (value <= config.WarningLowerLevel && value > config.LCL)
        { 
            alert = Alert.LCLApproachingWarning(
                $"Warning: Sensor value {value} is approaching the Lower Control Limit (LCL) of {config.LCL}. {config.WarningLowerLevel} Please monitor closely"
            );
            sensorValue = new SensorValue(SensorId, Name, value, shiftName, shiftDate, alert, config, sensorTimeUtc); 
            return (sensorValue, AddSensorData(sensorValue, alert, config));
        }
        
        alert = Alert.Normal();
        sensorValue = new SensorValue(SensorId, Name, value, shiftName, shiftDate, alert, config, sensorTimeUtc);  
        return (sensorValue, AddSensorData(sensorValue, alert, config));
    }

    private bool AddSensorData(SensorValue sensorValue, Alert alert, SensorConfig config)
    {   
        Alert = alert;
        if (Values.Count >= DataLength)
        {
            Values.RemoveAt(0);
        } 
 
        Values.Add(sensorValue);

        if(Math.Abs((LastIngestAtUtc - DateTime.UtcNow).TotalMinutes) >= 10)
        {
            LastIngestAtUtc = DateTime.UtcNow;
            return true;
        }
        else
        {
            if (sensorValue.Value > config.UCL || sensorValue.Value < config.LCL)
            {
                return true;
            }
            
            return false;
        } 
    }
}