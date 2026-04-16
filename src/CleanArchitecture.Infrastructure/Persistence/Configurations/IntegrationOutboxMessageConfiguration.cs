using CleanArchitecture.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;

public sealed class IntegrationOutboxMessageConfiguration : IEntityTypeConfiguration<IntegrationOutboxMessage>
{
    public void Configure(EntityTypeBuilder<IntegrationOutboxMessage> builder)
    {
        builder.ToTable("IntegrationOutboxMessages");

        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(500); // Lengthy to accommodate AssemblyQualifiedName

        builder.Property(x => x.Content)
            .IsRequired(); // Holds the JSON payload

        builder.Property(x => x.OccurredOnUtc)
            .IsRequired();

        // Optional fields
        builder.Property(x => x.ProcessedAtUtc)
            .IsRequired(false);

        builder.Property(x => x.Error)
            .IsRequired(false);

        // --- INDEXES ---

        // This is the most critical index. 
        // The background job constantly queries for unprocessed messages.
        builder.HasIndex(x => x.ProcessedAtUtc)
            .HasFilter("[ProcessedAtUtc] IS NULL")
            .HasDatabaseName("IX_IntegrationOutbox_Unprocessed");

        // Useful for debugging and ordering
        builder.HasIndex(x => x.OccurredOnUtc);
    }
}