using Assets.Domain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Presentation.Dashboard.Infrastructures;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SensorValue> SensorValues { get; set; }
    public DbSet<SensorConfig> SensorConfigs { get; set; }
    public DbSet<SensorShiftResult> SensorShiftResults { get; set; } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {   
        modelBuilder.Entity<SensorConfig>(entity => 
        {
            entity.HasKey(s => s.Id); 

            entity.Property(s => s.Name)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            entity.Property(s => s.Location)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            entity.Property(s => s.SerialNumber)
                .IsRequired()
                .HasColumnType("nvarchar(max)");
            
            entity.Property(s => s.Unit)
                .IsRequired(false)
                .HasMaxLength(100);

            entity.Property(s => s.UCL)
                .HasColumnType("decimal(18,2)");

            entity.Property(s => s.LCL)
                .HasColumnType("decimal(18,2)");

            entity.Property(s => s.WarningUpperLevel)
                .HasColumnType("decimal(18,2)");

            entity.Property(s => s.WarningLowerLevel)
                .HasColumnType("decimal(18,2)");
        });


        modelBuilder.Entity<SensorShiftResult>(entity => 
        {
            entity.HasKey(s => s.Id); 

            entity.HasIndex(s => s.SensorId);

            entity.HasIndex(s => s.ShiftName);

            entity.HasIndex(s => s.DateStart);
            
            entity.HasIndex(s => s.DateEnd);

            entity.Property(s => s.DateStart)
                .HasColumnType("datetime2"); 

            entity.Property(s => s.DateEnd)
                .HasColumnType("datetime2"); 
            
            entity.Property(s => s.AverageOrErrorValue)
                .HasColumnType("decimal(18,2)"); 
        });


        // Konfigurasi untuk SensorValue
        modelBuilder.Entity<SensorValue>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.HasIndex(s => s.SensorId);

            entity.Property(s => s.SensorName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(s => s.Value) 
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(s => s.Unit)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(s => s.Location)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            entity.Property(s => s.SerialNumber)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            entity.Property(s => s.ShiftDate)
                .HasColumnType("datetime2");
                
            entity.Property(s => s.CreatedAtUtc)
                .HasColumnType("datetime2");

            // Simpan Alert sebagai JSON string
            entity.Property(s => s.Alert)
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Alert>(v) ?? new Alert()
                );

            // Simpan Config sebagai JSON string
            entity.Property(s => s.Config)
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<SensorConfig>(v)
                );
        });
    }
}