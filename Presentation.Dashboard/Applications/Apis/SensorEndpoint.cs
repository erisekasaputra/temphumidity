using Presentation.Dashboard.Applications.Services;

namespace Presentation.Dashboard.Applications.Apis;

public static class SensorEndpoint
{
    public static void MapSensorEndpoint(this WebApplication app)
    {
        app.MapGet("/sensor/{sensorId}", async (Guid sensorId, SensorDataService sensorDataService) =>
        {
            return Results.Ok(await sensorDataService.GetShiftSensorValuesAsync(sensorId));
        });
    }
} 