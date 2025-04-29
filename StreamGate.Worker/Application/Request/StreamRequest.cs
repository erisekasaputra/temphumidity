namespace StreamGate.Worker.Application.Request;

public record StreamRequest(Guid SensorId, decimal SensorValue, DateTime? SensorTimeUtc); 