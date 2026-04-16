namespace CleanArchitecture.Domain.Templates.Events;

public sealed record TemplateCreatedDomainEvent(Guid TemplateId) : DomainEvent;
