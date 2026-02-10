using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoFleet.Infrastructure.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");

        // 1. Primary Key (Strongly Typed ID)
        builder.HasKey(d => d.Id);

        // Convert DriverId record <-> Guid for Database
        builder.Property(d => d.Id)
            .HasConversion(
                id => id.Value,
                value => new DriverId(value));

        // 2. Value Object: FullName (Multiple Columns)
        builder.OwnsOne(d => d.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            nameBuilder.Property(n => n.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        // 3. Value Object: DriverLicense (Single Column)
        builder.Property(d => d.License)
            .HasConversion(
                license => license.Value,
                value => DriverLicense.Create(value))
            .HasMaxLength(20)
            .IsRequired();

        // 4. Enum as String
        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // 5. Foreign Key (Nullable)
        builder.Property(d => d.CurrentVehicleId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? new VehicleId(value.Value) : null)
            .IsRequired(false);
    }
}
