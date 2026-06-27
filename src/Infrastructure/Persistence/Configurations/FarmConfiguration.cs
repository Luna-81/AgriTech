using Domain.Farms.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class FarmConfiguration : IEntityTypeConfiguration<Farm>
{
    public void Configure(EntityTypeBuilder<Farm> builder)
    {
        builder.ToTable("Farms");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(f => f.Name).IsRequired().HasMaxLength(100);

        builder.Property(f => f.MaxSensorCapacity)
            .HasColumnName("MaxCapacity")
            .IsRequired()
            .HasDefaultValue(50);

        builder.Property(f => f.IsDeleted).HasDefaultValue(false);

        builder.OwnsOne(f => f.Location, loc =>
        {
            loc.Property(l => l.Latitude).HasColumnName("Latitude").HasPrecision(10, 7);
            loc.Property(l => l.Longitude).HasColumnName("Longitude").HasPrecision(10, 7);
        });    

        builder.HasMany(f => f.Sensors)
            .WithOne()
            .HasForeignKey("FarmId")  
            .OnDelete(DeleteBehavior.Restrict);
    }
}