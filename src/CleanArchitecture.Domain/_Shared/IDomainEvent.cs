namespace CleanArchitecture.Domain._Shared;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}