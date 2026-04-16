using CleanArchitecture.Domain._Shared;

namespace CleanArchitecture.Application.Abstractions.DomainEvents;

public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}
