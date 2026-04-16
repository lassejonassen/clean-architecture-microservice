using CleanArchitecture.Application.Abstractions;

namespace CleanArchitecture.Infrastructure;

// Internal interface so only Infrastructure/Presentation can set the ID
public interface ICorrelationIdSetter
{
    void Set(Guid correlationId);
}

public class CorrelationContext : ICorrelationContext, ICorrelationIdSetter
{
    // Defaults to a new Guid if not set by middleware/consumer
    public Guid CorrelationId { get; private set; } = Guid.NewGuid();

    public void Set(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
}