using CleanArchitecture.Domain._Shared.DomainEvents;

namespace CleanArchitecture.Infrastructure.DomainEvents;

public interface IDomainEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}