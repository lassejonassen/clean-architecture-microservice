namespace CleanArchitecture.SharedKernel.IntegrationEvents;

public abstract record IntegrationEvent : IIntegrationEvent
    {
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid? CorrelationId { get; set; }
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
