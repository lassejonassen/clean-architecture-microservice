namespace CleanArchitecture.SharedKernel.IntegrationEvents;

public interface IIntegrationEvent
{
    Guid Id { get; }
    Guid CorrelationId { get; set; }
    DateTime OccurredOnUtc { get; }
}