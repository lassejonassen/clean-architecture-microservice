using CleanArchitecture.Infrastructure.Messaging;
using CleanArchitecture.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Text.Json;

namespace CleanArchitecture.Infrastructure.Persistence.Interceptors;

public sealed class ConvertIntegrationEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (eventData.Context is null) return base.SavingChangesAsync(eventData, result, ct);

        // Resolve the scoped buffer from the current context's scope
        var buffer = eventData.Context.GetService<IntegrationEventBuffer>();

        if (buffer is not null && buffer.Events.Any())
        {
            var messages = buffer.Events.Select(e => new IntegrationOutboxMessage
            {
                Id = e.Id,
                OccurredOnUtc = e.OccurredOnUtc,
                Type = e.GetType().AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(e, e.GetType())
            }).ToList();

            buffer.Clear();
            eventData.Context.Set<IntegrationOutboxMessage>().AddRange(messages);
        }

        return base.SavingChangesAsync(eventData, result, ct);
    }
}