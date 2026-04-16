using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.Abstractions.IntegrationEvents;
using CleanArchitecture.SharedKernel.IntegrationEvents;

namespace CleanArchitecture.Infrastructure.Messaging;

public class IntegrationEventStagingService(
    IntegrationEventBuffer buffer,
    ICorrelationContext correlationContext) : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class, IIntegrationEvent
    {
        if (integrationEvent is IntegrationEvent baseEvent)
        {
            integrationEvent.CorrelationId = correlationContext.CorrelationId;
        }

        buffer.Add(integrationEvent);
        return Task.CompletedTask;
    }
}