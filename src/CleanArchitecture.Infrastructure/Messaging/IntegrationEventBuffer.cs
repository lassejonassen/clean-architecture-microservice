using CleanArchitecture.SharedKernel.IntegrationEvents;

namespace CleanArchitecture.Infrastructure.Messaging;

public class IntegrationEventBuffer
{
    private readonly List<IIntegrationEvent> _events = [];
    public IReadOnlyList<IIntegrationEvent> Events => _events.AsReadOnly();
    public void Add(IIntegrationEvent @event) => _events.Add(@event);
    public void Clear() => _events.Clear();
}
