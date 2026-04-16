namespace CleanArchitecture.Domain._Shared;

public class DomainEntity : BaseEntity, IHasDomainEvents
{
    public DomainEntity() { }
    public DomainEntity(DateTime utcNow) : base(utcNow) { }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
}
