using CleanArchitecture.Application.Abstractions.DomainEvents;
using CleanArchitecture.Application.Abstractions.IntegrationEvents;
using CleanArchitecture.Contracts.Templates;
using CleanArchitecture.Domain.Templates.Events;
using CleanArchitecture.Domain.Templates.Repositories;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Templates.DomainEventHandlers;

public sealed class TemplateCreatedDomainEventHandler(
    ITemplateRepository templateRepository,
    IIntegrationEventPublisher publisher,
    ILogger<TemplateCreatedDomainEventHandler> logger)
    : IDomainEventHandler<TemplateCreatedDomainEvent>
{
    public async Task HandleAsync(TemplateCreatedDomainEvent domainEvent, CancellationToken ct = default)
    {
        logger.LogInformation("Handling TemplateCreatedDomainEvent for TemplateId: {TemplateId}", domainEvent.TemplateId);

        var template = await templateRepository.GetByIdAsync(domainEvent.TemplateId, ct);

        if (template == null)
        {
            logger.LogWarning("Template with ID {TemplateId} not found in repository.", domainEvent.TemplateId);
        }

        var integrationEvent = new TemplateCreatedIntegrationEvent
        {
            TemplateId = template!.Id,
            TemplateName = template.Name
        };

        await publisher.PublishAsync(integrationEvent, ct);
    }
}
