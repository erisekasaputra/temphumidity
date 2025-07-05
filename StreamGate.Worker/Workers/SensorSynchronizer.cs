using Assets.Domain.Domain.Configurations;
using Microsoft.EntityFrameworkCore;
using StreamGate.Worker.Infrastructure;

namespace StreamGate.Worker.Workers;

public class SensorSynchronizer : BackgroundService
{  
    private readonly AppDbContext _dbContext; 
  
    public SensorSynchronizer( 
       IServiceScopeFactory scopeFactory)
    { 
        var scope = scopeFactory.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    } 

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var sensors = await _dbContext.SensorConfigs.CountAsync(stoppingToken);

            if (sensors > SensorConfiguration.MaxSensorLength)
            { 
                _dbContext.Database.ExecuteSqlRaw("DELETE FROM SensorConfigs");
            } 

            await Task.Delay(20000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    { 
        await base.StopAsync(cancellationToken);
    }  
}