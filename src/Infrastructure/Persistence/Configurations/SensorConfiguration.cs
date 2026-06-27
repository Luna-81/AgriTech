using Domain.Sensors.Entities;
using Domain.Sensors.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SensorConfiguration : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.ToTable("Sensors");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Status).HasConversion<string>().IsRequired();

        builder.OwnsOne(s => s.TemperatureThreshold, temp =>
        {
            temp.Property(t => t.Celsius)
                .HasColumnName("TemperatureThreshold")
                .IsRequired()
                .HasPrecision(5, 2);
        });

        builder.OwnsOne(s => s.Location, loc =>
        {
            loc.Property(l => l.Latitude).HasColumnName("Latitude").HasPrecision(10, 7);
            loc.Property(l => l.Longitude).HasColumnName("Longitude").HasPrecision(10, 7);
        });

        builder.OwnsMany(s => s.Readings, reading =>
        {
            reading.ToTable("SensorReadings");
            reading.WithOwner().HasForeignKey("SensorId");
            reading.Property<int>("Id").ValueGeneratedOnAdd();
            reading.HasKey("Id");
            reading.Property(r => r.Timestamp).IsRequired();
            reading.Property(r => r.Temperature).HasPrecision(5, 2);
            reading.Property(r => r.Humidity).HasPrecision(5, 2);
        });

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();


    }
}