using CleanArchitecture.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);

        // SQL Server handles strings efficiently, but AssemblyQualifiedName 
        // can be quite large, so we set a reasonable max.
        builder.Property(x => x.Type)
            .HasMaxLength(500)
            .IsRequired();

        // Using nvarchar(max) for the JSON content
        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.OccurredOnUtc)
            .IsRequired();

        // Nullable columns
        builder.Property(x => x.ProcessedAtUtc)
            .IsRequired(false);

        builder.Property(x => x.Error)
            .IsRequired(false);

        // SQL Server Filtered Index
        // This index only stores rows where ProcessedAtUtc is NULL.
        // It makes the Background Job query extremely fast.
        builder.HasIndex(x => x.ProcessedAtUtc)
            .HasDatabaseName("IX_OutboxMessages_ProcessedAtUtc_Null")
            .HasFilter("[ProcessedAtUtc] IS NULL");
    }
}