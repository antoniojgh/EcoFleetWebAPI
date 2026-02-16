using EcoFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoFleet.Infrastructure.Configurations;

public class ManagerDriverAssignmentConfiguration : IEntityTypeConfiguration<ManagerDriverAssignment>
{
    public void Configure(EntityTypeBuilder<ManagerDriverAssignment> builder)
    {
        builder.ToTable("ManagerDriverAssignments");

        // 1. Primary Key (Strongly Typed ID)
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(
                id => id.Value,
                value => new ManagerDriverAssignmentId(value));

        // 2. Foreign Key: ManagerId
        builder.Property(a => a.ManagerId)
            .HasConversion(
                id => id.Value,
                value => new ManagerId(value))
            .IsRequired();

        // 3. Foreign Key: DriverId
        builder.Property(a => a.DriverId)
            .HasConversion(
                id => id.Value,
                value => new DriverId(value))
            .IsRequired();

        // 4. AssignedDate
        builder.Property(a => a.AssignedDate)
            .IsRequired();

        // 5. IsActive
        builder.Property(a => a.IsActive)
            .IsRequired();

        // 6. Indexes for efficient querying
        builder.HasIndex(a => a.ManagerId);
        builder.HasIndex(a => a.DriverId);
    }
}
