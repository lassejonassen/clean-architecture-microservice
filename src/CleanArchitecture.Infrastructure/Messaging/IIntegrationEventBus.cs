using CleanArchitecture.SharedKernel.IntegrationEvents;

namespace CleanArchitecture.Infrastructure.Messaging;

public interface IIntegrationEventBus
{
    Task SendAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class, IIntegrationEvent;
}
