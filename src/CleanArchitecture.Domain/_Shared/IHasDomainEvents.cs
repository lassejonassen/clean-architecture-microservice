using CleanArchitecture.Domain._Shared.DomainEvents;

namespace CleanArchitecture.Domain._Shared;

public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> GetDomainEvents();
    void ClearDomainEvents();
}
