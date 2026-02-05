
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoFleet.Infrastructure.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        // 1. Primary Key (Strongly Typed ID)
        builder.HasKey(v => v.Id);

        // Convert VehicleId record <-> Guid for Database
        builder.Property(v => v.Id)
            .HasConversion(
                id => id.Value,
                value => new VehicleId(value));

        // 2. Value Object: LicensePlate (Single Column)
        builder.Property(v => v.Plate)
            .HasConversion(
                plate => plate.Value,
                value => LicensePlate.Create(value))
            .HasMaxLength(15)
            .IsRequired();

        // 3. Value Object: Geolocation (Multiple Columns)
        // We use "OwnsOne" to flatten Latitude/Longitude into the Vehicles table
        builder.OwnsOne(v => v.CurrentLocation, locationBuilder =>
        {
            locationBuilder.Property(l => l.Latitude).HasColumnName("Latitude");
            locationBuilder.Property(l => l.Longitude).HasColumnName("Longitude");
        });

        // 4. Enum as String (More readable in DB than 0, 1, 2)
        builder.Property(v => v.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // 5. Foreign Key (Nullable)
        builder.Property(v => v.CurrentDriverId)
            .IsRequired(false);
    }
}