namespace CleanArchitecture.Domain._Shared;

public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; set; }

    protected BaseEntity()
    {
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    protected BaseEntity(DateTime utcNow)
    {
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }
}
