
using Assets.Domain.Domain.Entities;

namespace StreamGate.Worker.Application.Interfaces;

public interface ICassandraDataService
{
    Task Insert(SensorValue sensorValue);
    Task<List<SensorValue>> GetSensorDataAsync(Guid sensorId);
}