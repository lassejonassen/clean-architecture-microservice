using CleanArchitecture.Domain._Shared.DomainEvents;
using CleanArchitecture.Infrastructure.DomainEvents;
using CleanArchitecture.Infrastructure.Options;
using CleanArchitecture.Infrastructure.Persistence.DbContexts;
using CleanArchitecture.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CleanArchitecture.Infrastructure.BackgroundServices;

public class ProcessDomainEventsJob(
    IServiceProvider serviceProvider,
    ILogger<ProcessDomainEventsJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Domain Event Processor is starting.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IDomainEventPublisher>();

                var count = await context.Set<OutboxMessage>().CountAsync(stoppingToken);
                logger.LogDebug("Total DomainEvents in DB: {Count}", count);

                var pendingCount = await context.Set<OutboxMessage>().CountAsync(m => m.ProcessedAtUtc == null, stoppingToken);
                logger.LogDebug("Pending DomainEvents in DB: {Pending}", pendingCount);

                // 1. Fetch top 20 unprocessed messages
                var messages = await context.Set<OutboxMessage>()
                    .Where(m => m.ProcessedAtUtc == null)
                    .OrderBy(m => m.OccurredOnUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                if (messages.Count == 0)
                {
                    await Task.Delay(5000, stoppingToken);
                    continue;
                }

                foreach (var message in messages)
                {
                    try
                    {
                        var type = Type.GetType(message.Type);
                        if (type is null)
                        {
                            logger.LogError("Could not resolve type: {Type}", message.Type);
                            message.Error = $"Type resolution failed for {message.Type}";
                            continue;
                        }

                        var domainEvent = JsonSerializer.Deserialize(message.Content, type, JsonOptions.Default) as IDomainEvent;

                        if (domainEvent is null)
                        {
                            logger.LogError("Deserialization failed for message {Id}", message.Id);
                            continue;
                        }

                        await publisher.PublishAsync(domainEvent, stoppingToken);

                        message.ProcessedAtUtc = DateTime.UtcNow;
                        message.Error = null;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing outbox message {Id}", message.Id);
                        message.Error = ex.ToString();
                    }
                }

                await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "The background job encountered a fatal error.");
            }

            await Task.Delay(5000, stoppingToken); // Poll every 5 seconds
        }
    }
}
