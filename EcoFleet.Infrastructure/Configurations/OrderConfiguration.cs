using EcoFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoFleet.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        // 1. Primary Key (Strongly Typed ID)
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => new OrderId(value));

        // 2. Foreign Key: DriverId
        builder.Property(o => o.DriverId)
            .HasConversion(
                id => id.Value,
                value => new DriverId(value))
            .IsRequired();

        // 3. Enum as String
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // 4. Dates
        builder.Property(o => o.CreatedDate)
            .IsRequired();

        builder.Property(o => o.FinishedDate)
            .IsRequired(false);

        // 5. Value Object: PickUpLocation (Geolocation)
        builder.OwnsOne(o => o.PickUpLocation, locationBuilder =>
        {
            locationBuilder.Property(l => l.Latitude).HasColumnName("PickUpLatitude");
            locationBuilder.Property(l => l.Longitude).HasColumnName("PickUpLongitude");
        });

        // 6. Value Object: DropOffLocation (Geolocation)
        builder.OwnsOne(o => o.DropOffLocation, locationBuilder =>
        {
            locationBuilder.Property(l => l.Latitude).HasColumnName("DropOffLatitude");
            locationBuilder.Property(l => l.Longitude).HasColumnName("DropOffLongitude");
        });

        // 7. Price
        builder.Property(o => o.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        // 8. Index for efficient querying by driver
        builder.HasIndex(o => o.DriverId);
    }
}
