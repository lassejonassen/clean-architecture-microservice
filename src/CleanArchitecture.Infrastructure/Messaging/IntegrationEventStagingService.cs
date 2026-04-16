using CleanArchitecture.Application.Abstractions.IntegrationEvents;
using CleanArchitecture.SharedKernel.IntegrationEvents;

namespace CleanArchitecture.Infrastructure.Messaging;

public class IntegrationEventStagingService(IntegrationEventBuffer buffer) : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class, IIntegrationEvent
    {
        buffer.Add(integrationEvent);
        return Task.CompletedTask;
    }
}