using EcoFleet.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoFleet.Infrastructure.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.OccurredOn)
            .IsRequired();

        builder.Property(x => x.ProcessedOn)
            .IsRequired(false);

        builder.Property(x => x.Error)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.HasIndex(x => x.ProcessedOn)
            .HasFilter("[ProcessedOn] IS NULL")
            .HasDatabaseName("IX_OutboxMessages_Unprocessed");
    }
}
