using CleanArchitecture.SharedKernel.IntegrationEvents;

namespace CleanArchitecture.Contracts.Templates;

public record TemplateCreatedIntegrationEvent : IntegrationEvent
{
    public required Guid TemplateId { get; init; }
    public required string TemplateName { get; init; }
}