namespace Assets.Domain.Domain.Entities;

public class SensorShiftResult
{
    public Guid Id { get; set; }
    public int ShiftName { get; set; }
    public DateTime DateStart { get; set; } // Waktu mulai shift
    public DateTime DateEnd { get; set; } // Waktu akhir shift
    public Guid SensorId { get; set; } // ID Sensor 
    public decimal AverageOrErrorValue { get; set; } // Nilai rata-rata atau error
    public string Unit { get; set; }
    public int ErrorStatus { get; set; }
    public SensorShiftResult()
    {
        Unit = string.Empty;
    }

    public SensorShiftResult(
        int shiftName,
        DateTime dateStart,
        DateTime dateEnd,
        Guid sensorId,
        decimal averageOrErrorValue,
        string unit, 
        int errorStatus)
    {
        Id = Guid.NewGuid();
        ShiftName = shiftName;
        DateStart = dateStart;
        DateEnd = dateEnd;
        SensorId = sensorId;
        AverageOrErrorValue = averageOrErrorValue;
        Unit = unit;
        ErrorStatus = errorStatus;
    }

    public void UpdateValue(decimal averageOrErrorValue, string unit, int errorStatus)
    {
        AverageOrErrorValue = averageOrErrorValue;
        Unit = unit;
        ErrorStatus = errorStatus; 
    }
}