using CleanArchitecture.Application.Abstractions.DomainEvents;
using CleanArchitecture.Domain._Shared.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.DomainEvents;

public class DomainEventPublisher(IServiceProvider serviceProvider) : IDomainEventPublisher
{
    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handlers = serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler is null) continue;

            // Using dynamic to resolve the specific HandleAsync(TEvent) call
            await ((dynamic)handler).HandleAsync((dynamic)domainEvent, ct);
        }
    }
}