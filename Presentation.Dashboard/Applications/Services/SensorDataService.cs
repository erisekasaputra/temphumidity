using System.Data;
using Assets.Domain.Domain.Entities;
using Assets.Domain.Services; 
using Microsoft.EntityFrameworkCore; 
using Presentation.Dashboard.Infrastructures;
using LinqKit;
using System.Data.Entity.Core;
using Presentation.Dashboard.Applications.Dtos;
using Presentation.Dashboard.Applications.Interfaces;
using Assets.Domain.Domain.Enums;


namespace Presentation.Dashboard.Applications.Services;

public class SensorDataService(
    AppDbContext context, 
    ShiftService shiftService, 
    IRedisCacheService redisCache)
{
    private const int MAX_SENSOR = 30;
    private readonly AppDbContext _context = context;
    private readonly ShiftService _shiftService = shiftService;
    private readonly IRedisCacheService _redisCache = redisCache;

    public async Task<List<AggregatedSensorShiftResult>> GetShiftSensorValuesAsync(Guid sensorId)
    { 
        var aggregatedSensorShiftResults = new List<AggregatedSensorShiftResult>();
        var shifts = _shiftService.GetShiftsByShiftOrderForDate(); 


        var predicate = PredicateBuilder.New<SensorShiftResult>();

        foreach (var (workingDate, shiftName, ranges) in shifts)
        {
            // Cari atau tambahkan entry baru dengan cara yang lebih clean
            var aggregatedSensorShiftResult = aggregatedSensorShiftResults.FirstOrDefault(x => x.ShiftName == shiftName);

            if (aggregatedSensorShiftResult is null)
            {
                aggregatedSensorShiftResult = new AggregatedSensorShiftResult(shiftName);
                aggregatedSensorShiftResults.Add(aggregatedSensorShiftResult);
            }
 
            foreach (var (dateStart, dateEnd) in ranges)
            {    
                predicate = predicate.Or(u => u.SensorId == sensorId && u.DateStart == dateStart && u.DateEnd == dateEnd);  
                aggregatedSensorShiftResult.AddSensorShiftResult(new SensorShiftResult(
                    shiftName, dateStart, dateEnd, sensorId, 0, string.Empty, 0, AlertType.Normal));
            } 
        } 

        var results = await _context
            .SensorShiftResults
            .Where(predicate)
            .AsNoTracking()
            .ToListAsync(); 

        if (results is not null)
        {
            foreach(var result in results) 
            { 
                foreach(var aggSensorShiftResult in aggregatedSensorShiftResults)
                {
                    var sensorShiftResult = aggSensorShiftResult.SensorShiftResults.
                        FirstOrDefault(x => x.DateStart == result.DateStart && x.DateEnd == result.DateEnd);
                    
                    sensorShiftResult?.UpdateValue(result.AverageOrErrorValue, result.Unit, result.ErrorStatus, result.AlertType);
                }  
            }
        } 

        return aggregatedSensorShiftResults;
    }
 

    public async Task<(List<SensorValue> result, int totalCount)> GetPaginatedSensorValues(
        int pageNumber, int pageSize, Guid? sensorId = null, DateTime? start = null, DateTime? end = null, int? shiftName = null, AlertType? alertType = null)
    {
        var query = _context
            .SensorValues
            .AsQueryable();

        if (start.HasValue && end.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= start.Value.AddHours(-7) && x.CreatedAtUtc <= end.Value.AddHours(-7));
        }

        if (sensorId is not null && sensorId.HasValue)
        {
            query = query.Where(x => x.SensorId == sensorId.Value);
        }

        if (shiftName is not null && shiftName.HasValue)
        {
            query = query.Where(x => x.ShiftName == shiftName);
        }

        if (alertType is not null && alertType.HasValue)
        {
            query = query.Where(x => x.AlertType == alertType);
        }

        query = query.OrderByDescending(x => x.CreatedAtUtc);

        int count = await query.CountAsync();

        var result = await query
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize).ToListAsync();

        return (result, count);
    }

    public async Task<bool> DeleteSensorConfig(Guid id)
    {
        var sensorConfig = await _context.SensorConfigs.Where(x => x.Id == id).FirstOrDefaultAsync() 
            ?? throw new ObjectNotFoundException($"Sensor {id} is not found");   
 
        _context.SensorConfigs.Remove(sensorConfig);

        await _context.SaveChangesAsync();      
         
        _redisCache.DeleteObject($"sensor/config/{id}");
        
        return true;
    }

    public async Task<SensorConfig> AddSensorConfigAsync(CreateSensorConfigRequestDto request)
    {
        request.Validate();  

        try
        {
            await _context.Database.BeginTransactionAsync();

            var countSensorConfigs = await _context.SensorConfigs.CountAsync();

            if (countSensorConfigs >= (MAX_SENSOR-28))
            {
                throw new InvalidOperationException($"Sensor quota number is exceeded. Maximum is {MAX_SENSOR-28} sensors can be registered.");
            }

            var result = await _context.SensorConfigs.AddAsync(SensorConfig.Create(
                request.Name, 
                request.Location, 
                request.Unit, 
                request.SerialNumber,
                request.UCL, 
                request.LCL, 
                request.WarningUpperLevel, 
                request.WarningLowerLevel)); 

            await _context.SaveChangesAsync();

            var configSensor = result.Entity;

            await _redisCache.SetObject($"sensor/config/{configSensor.Id}", configSensor);

            await _context.Database.CommitTransactionAsync();
            
            return configSensor;
        } 
        catch (Exception)
        {
            await _context.Database.RollbackTransactionAsync();
            throw;
        }
    }


    public async Task UpdateSensorConfigAsync(Guid id, CreateSensorConfigRequestDto request)
    { 
        request.Validate();

        try
        {
            await _context.Database.BeginTransactionAsync();
        
            var configSensor = await _context.SensorConfigs.Where(x => x.Id == id).FirstOrDefaultAsync() ?? throw new Exception($"Sensor {id} is not found");
            
            configSensor.Update(request.Name, 
                                request.Location, 
                                request.Unit, 
                                request.SerialNumber,
                                request.UCL, 
                                request.LCL, 
                                request.WarningUpperLevel, 
                                request.WarningLowerLevel);
            
            await _redisCache.SetObject($"sensor/config/{configSensor.Id}", configSensor);

            await _context.SaveChangesAsync(); 

            await _context.Database.CommitTransactionAsync();
        } 
        catch (Exception)
        {
            await _context.Database.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<List<SensorConfig>> GetAllAsync()
    {
        return await _context.SensorConfigs.ToListAsync();
    }

    public async Task<List<SensorShiftResult>?> GetReportByMonthAndSensorId(
        DateTime monthYearParam,
        params Guid[] sensorIds)
    {
        return await _context.SensorShiftResults
            .Where(x => sensorIds.Contains(x.SensorId) &&
                        x.DateStart.Year == monthYearParam.Year &&
                        x.DateStart.Month == monthYearParam.Month)
            .ToListAsync();
    }
} 