using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using ClosedXML.Excel;
using Assets.Domain.Domain.Entities;
using Presentation.Dashboard.Infrastructures;
using Presentation.Dashboard.Applications.Interfaces;
using Microsoft.EntityFrameworkCore;
using Assets.Domain.Domain.Enums;

namespace Presentation.Dashboard.Applications.Services;

public static class ExcelReportGeneratorService
{
    public static string GetDateColumn(DateTime time)
    {
        return time.Day switch
        {
            1 => "D",
            2 => "E",
            3 => "F",
            4 => "G",
            5 => "H",
            6 => "I",
            7 => "J",
            8 => "K",
            9 => "L",
            10 => "M",
            11 => "N",
            12 => "O",
            13 => "P",
            14 => "Q",
            15 => "R",
            16 => "S",
            17 => "D",
            18 => "E",
            19 => "F",
            20 => "G",
            21 => "H",
            22 => "I",
            23 => "J",
            24 => "K",
            25 => "L",
            26 => "M",
            27 => "N",
            28 => "O",
            29 => "P",
            30 => "Q",
            31 => "R",
            _ => ""
        };
    }

    public static int GetDateRow(DateTime time)
    {
        var date = time.Day;

        var result = time.Hour switch
        { 
            6 when date <= 16 => 10,
            7 when date <= 16 => 12,
            8 when date <= 16 => 14,
            9 when date <= 16 => 16,
            10 when date <= 16 => 18,
            11 when date <= 16 => 20,
            12 when date <= 16 => 22,
            13 when date <= 16 => 24,
            14 when date <= 16 => 26,
            15 when date <= 16 => 28,
            16 when date <= 16 => 30,
            17 when date <= 16 => 32,
            18 when date <= 16 => 34,
            19 when date <= 16 => 36,
            20 when date <= 16 => 38,
            21 when date <= 16 => 40,
            22 when date <= 16 => 42,
            23 when date <= 16 => 44,
            0 when date <= 16 => 46,
            1 when date <= 16 => 48,
            2 when date <= 16 => 50,
            3 when date <= 16 => 52,
            4 when date <= 16 => 54,
            5 when date <= 16 => 56,



            6 when date > 16 => 63,
            7 when date > 16 => 65,
            8 when date > 16 => 67,
            9 when date > 16 => 69,
            10 when date > 16 => 71,
            11 when date > 16 => 73,
            12 when date > 16 => 75,
            13 when date > 16 => 77,
            14 when date > 16 => 79,
            15 when date > 16 => 81,
            16 when date > 16 => 83,
            17 when date > 16 => 85,
            18 when date > 16 => 87,
            19 when date > 16 => 89,
            20 when date > 16 => 91,
            21 when date > 16 => 93,
            22 when date > 16 => 95,
            23 when date > 16 => 97,
            0 when date > 16 => 99,
            1 when date > 16 => 101,
            2 when date > 16 => 103,
            3 when date > 16 => 105,
            4 when date > 16 => 107,
            5 when date > 16 => 109, 
            _ => 0
        };

        return result;
    }

    public static string GetCriteriaColumn()
    {
        return "C";
    }

    public static async Task<List<SensorConfig>> GetSensorsConfig(List<Guid> ids, AppDbContext context, IRedisCacheService redisCache)
    {
        var listConfigs = new List<SensorConfig>();

        foreach(var id in ids.Distinct().ToList())
        {
            var sensorConfig = await redisCache.GetObject<SensorConfig>($"sensor/config/{id}");
            
            if (sensorConfig is null)
            {
                sensorConfig = await context.SensorConfigs
                    .FirstOrDefaultAsync(x => x.Id == id) ?? throw new Exception(
                        $"Sensor {id} has no master configuration.");

                await redisCache.SetObject($"sensor/config/{id}", sensorConfig);
            }

            if(sensorConfig is not null)
            {
                listConfigs.Add(sensorConfig);
            }
        } 

        return listConfigs;
    }

    public static async Task<byte[]> Generate(
        DateTime month, 
        List<SensorShiftResult>? sensorShiftResults, 
        AppDbContext context, 
        IRedisCacheService redisCache)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateReport.xlsx");
        
        using var workbook = new XLWorkbook(path);

        var ws = workbook.Worksheet(1); 

        if (sensorShiftResults is not null)
        {  
            var ids = sensorShiftResults.Select(x => x.SensorId).Distinct().ToList();

            var listSensor = await GetSensorsConfig(ids, context, redisCache);  
          

            foreach(var sensorShiftResult in sensorShiftResults)
            { 
                var sensor = listSensor.FirstOrDefault(x => x.Id == sensorShiftResult.SensorId);
                
                var incrementor = sensor?
                    .Name.Contains("humi", StringComparison.CurrentCultureIgnoreCase) ?? false ? 1 : 0; 

                var column = GetDateColumn(sensorShiftResult.DateStart);
                var row = GetDateRow(sensorShiftResult.DateStart) + incrementor;

                var cell = ws.Cell($"{column}{row}");
                cell.Value = 
                    $"{sensorShiftResult.AverageOrErrorValue} {sensorShiftResult.Unit}";
                
                if (sensorShiftResult.AlertType is AlertType.UCLExceeded or AlertType.LCLExceeded)
                {
                    cell.Style.Fill.BackgroundColor = XLColor.Red;       // Cell background
                    cell.Style.Font.FontColor = XLColor.White;           // Text color
                    cell.Style.Font.Bold = true;  
                }

                if (sensorShiftResult.AlertType is AlertType.UCLApproachingWarning or AlertType.LCLApproachingWarning)
                {
                    cell.Style.Fill.BackgroundColor = XLColor.Yellow;       // Cell background
                    cell.Style.Font.FontColor = XLColor.Black;           // Text color
                    cell.Style.Font.Bold = true;  
                }
            } 
        }

        ws.Cell("B6").Value = month.ToString("MMMM yyyy");

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}