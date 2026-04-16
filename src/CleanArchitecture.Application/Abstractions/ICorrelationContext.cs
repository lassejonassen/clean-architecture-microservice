namespace CleanArchitecture.Application.Abstractions;

public interface ICorrelationContext
{
    Guid CorrelationId { get; }
}