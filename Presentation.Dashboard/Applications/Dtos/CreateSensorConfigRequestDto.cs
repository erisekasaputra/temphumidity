namespace Presentation.Dashboard.Applications.Dtos;

public class CreateSensorConfigRequestDto( 
    string name,
    string location,
    string unit,
    string serialNumber,
    decimal ucl,
    decimal lcl,
    decimal warningUpperLevel,
    decimal warningLowerLevel)
{ 
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public string Unit { get; set; } = unit;
    public string SerialNumber { get; set; } = serialNumber;
    public decimal UCL { get; set; } = ucl;
    public decimal LCL { get; set; } = lcl;
    public decimal WarningUpperLevel { get; set; } = warningUpperLevel;
    public decimal WarningLowerLevel { get; set; } = warningLowerLevel;

    public void Validate()
    {
        if (string.IsNullOrEmpty(Name))
        {
            throw new ArgumentNullException($"{nameof(Name)} can not be empty");
        }

        if (string.IsNullOrEmpty(Location))
        {
            throw new ArgumentNullException($"{nameof(Location)} can not be empty"); 
        }

        if (string.IsNullOrEmpty(Unit))
        {
            throw new ArgumentNullException($"{nameof(Unit)} can not be empty");
        }  

        if (string.IsNullOrEmpty(SerialNumber))
        {
            throw new ArgumentNullException($"{nameof(SerialNumber)} can not be empty"); 
        }
    }
}