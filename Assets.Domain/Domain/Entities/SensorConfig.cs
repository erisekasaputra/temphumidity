namespace Assets.Domain.Domain.Entities;

public class SensorConfig
{
    public Guid Id { get; set; } 
    public string Name { get; set; }
    public string Location { get; set; }
    public string Unit { get; set; }
    public string SerialNumber { get; set; }
    public decimal UCL { get; set; }
    public decimal LCL { get; set; }
    public decimal WarningUpperLevel { get; set; }
    public decimal WarningLowerLevel { get; set; }
    public SensorConfig()
    {
        Name = string.Empty;
        Location = string.Empty;
        Unit = string.Empty;
        SerialNumber = string.Empty;
    }

    public SensorConfig( 
        string name,
        string location,
        string unit,
        string serialNumber,
        decimal ucl, 
        decimal lcl, 
        decimal warningUpperLevel, 
        decimal warningLowerLevel)
    {
        Id = Guid.NewGuid(); 
        Name = name;
        Location = location;
        Unit = unit;
        SerialNumber = serialNumber;
        UCL = ucl;
        LCL = lcl;
        WarningUpperLevel = warningUpperLevel;
        WarningLowerLevel = warningLowerLevel;
    }

    public void Update(
        string name,
        string location,
        string unit,
        string serialNumber,
        decimal ucl, 
        decimal lcl, 
        decimal warningUpperLevel, 
        decimal warningLowerLevel)
    {
        Name = name;
        Location = location;
        Unit = unit;
        SerialNumber = serialNumber;
        UCL = ucl;
        LCL = lcl;
        WarningUpperLevel = warningUpperLevel;
        WarningLowerLevel = warningLowerLevel; 
    }

    public static SensorConfig Create( 
        string name,
        string location,
        string unit,
        string serialNumber,
        decimal ucl, 
        decimal lcl, 
        decimal warningUpperLevel, 
        decimal warningLowerLevel)
    {
        return new SensorConfig(name, location, unit, serialNumber, ucl, lcl, warningUpperLevel, warningLowerLevel);
    }
}