namespace Assets.Domain.Domain.Entities;

public class AggregatedSensorShiftResult
{
    public int ShiftName { get; set; }
    public List<SensorShiftResult> SensorShiftResults { get; set; } = [];

    public AggregatedSensorShiftResult(int shiftName)
    {
        ShiftName = shiftName;
    }

    public void AddSensorShiftResult(SensorShiftResult sensorShiftResult)
    {
        SensorShiftResults.Add(sensorShiftResult);
    }
}