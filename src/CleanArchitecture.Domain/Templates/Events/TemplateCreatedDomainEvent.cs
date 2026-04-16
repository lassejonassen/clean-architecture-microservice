using CleanArchitecture.Domain._Shared.DomainEvents;

namespace CleanArchitecture.Domain.Templates.Events;

public sealed record TemplateCreatedDomainEvent(Guid TemplateId) : DomainEvent;
