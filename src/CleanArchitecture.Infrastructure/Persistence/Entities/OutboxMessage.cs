namespace CleanArchitecture.Infrastructure.Persistence.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // AssemblyQualifiedName
    public string Content { get; set; } = string.Empty; // JSON
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public string? Error { get; set; }
}
